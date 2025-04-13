using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using dataflow_cs.Core.Services;
using dataflow_cs.Presentation.Views.Windows;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using dataflow_cs.Utils.CADUtils;
using dataflow_cs.Utils.JigUtils;
using dataflow_cs.Utils.ConstUtils;

namespace dataflow_cs.Business.Commands.GsLc
{
    internal class GsLcInsertElementBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertElementBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            try
            {
                editor.WriteMessage("\n正在插入工艺数据流组件块...");

                return true;
            }
            catch (System.Exception ex)
            {
                editor.WriteMessage($"\n插入工艺数据流组件块时发生错误: {ex.Message}");
                return false;
            }
        }
    }

    internal class GsLcInsertGlobalBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertGlobalBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            // 为防止事务嵌套，先声明但不立即启动事务
            Transaction tr = null;
            
            try
            {
                editor.WriteMessage("\n正在插入球阀块...");
                
                // 引入球阀块定义
                ObjectId blockId = UtilsBlock.UtilsImportBlockFromExternalDwg(ConstFileName.GsLcBlocksPath, "GsLcValveBall");
                if (blockId == ObjectId.Null)
                {
                    editor.WriteMessage("\n导入块定义失败，请检查块文件路径和块名称。");
                    return false;
                }

                // 创建初始插入点（原点）
                Point3d initialPoint = Point3d.Origin;
                // 从UCS坐标系转换到WCS坐标系
                Autodesk.AutoCAD.ApplicationServices.Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Matrix3d ucsToWcs = doc.Editor.CurrentUserCoordinateSystem;
                initialPoint = initialPoint.TransformBy(ucsToWcs);

                // 初始旋转角度为0
                double rotation = 0;

                // 显示命令行提示
                editor.WriteMessage("\n拖动放置球阀，输入\"R\"可旋转90度，按ESC退出");

                // 进入交互循环
                while (true)
                {
                    try
                    {
                        // 开始新事务
                        tr = database.TransactionManager.StartTransaction();
                        
                        // 创建新的块参照用于拖拽
                        BlockReference sourceBr = new BlockReference(initialPoint, blockId);
                        sourceBr.Layer = "0"; // 设置默认图层
                        sourceBr.Rotation = rotation;

                        // 创建拖拽对象并显示
                        InsertBlockJig jig = new InsertBlockJig(
                            initialPoint, 
                            sourceBr, 
                            rotation, 
                            "请选择插入点或输入[旋转(R)]:");

                        // 执行拖拽
                        PromptResult jigResult = editor.Drag(jig);

                        // 用户确定了位置
                        if (jigResult.Status == PromptStatus.OK)
                        {
                            // 用户确定了位置，使用UtilsInsertBlock函数添加块
                            // 获取块定义名
                            string blockName = "GsLcValveBall"; // 默认使用已知的块名
                            if (blockId != ObjectId.Null)
                            {
                                BlockTableRecord blockDef = tr.GetObject(blockId, OpenMode.ForRead) as BlockTableRecord;
                                if (blockDef != null)
                                {
                                    blockName = blockDef.Name;
                                }
                            }
                            
                            ObjectId newBlockId = UtilsBlock.UtilsInsertBlock(
                                blockName,
                                jig.InsertionPoint,
                                1.0, 1.0, 1.0, // 使用默认缩放比例
                                jig.Rotation, // 使用jig中的旋转角度
                                "0", // 设置图层为0
                                tr // 传入当前事务
                            );
                            
                            // 提交事务，使块立即显示
                            tr.Commit();
                            tr = null;
                            
                            // 更新初始点为当前位置，方便连续插入
                            initialPoint = jig.InsertionPoint;
                            rotation = jig.Rotation; // 保持当前旋转角度
                            
                            // 显示命令行提示，提醒用户继续操作
                            editor.WriteMessage("\n球阀已插入，继续拖动放置新的球阀，输入\"R\"可旋转，ESC退出");
                        }
                        // 用户输入了关键字
                        else if (jigResult.Status == PromptStatus.Keyword)
                        {
                            // 获取更新后的旋转角度
                            rotation = jig.Rotation;
                            
                            // 放弃当前事务
                            tr.Dispose();
                            tr = null;
                            
                            // 显示命令行提示，告知用户已旋转
                            editor.WriteMessage($"\n球阀已旋转，当前角度: {Math.Round(rotation * 180 / Math.PI)}度");
                        }
                        // 用户取消或按ESC
                        else
                        {
                            if (tr != null)
                            {
                                tr.Dispose();
                                tr = null;
                            }
                            editor.WriteMessage("\n命令已取消。");
                            break;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        editor.WriteMessage($"\n执行操作时发生错误: {ex.Message}");
                        if (tr != null)
                        {
                            tr.Dispose();
                            tr = null;
                        }
                    }
                }

                return true;
            }
            catch (System.Exception ex)
            {
                editor.WriteMessage($"\n插入块时发生错误: {ex.Message}");
                if (tr != null)
                {
                    tr.Dispose();
                }
                return false;
            }
        }
    }

    internal class GsLcInsertInstrumentPBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertInstrumentPBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            try
            {
                editor.WriteMessage("\n正在插入仪表P块...");

                return true;
            }
            catch (System.Exception ex)
            {
                editor.WriteMessage($"\n插入仪表P块时发生错误: {ex.Message}");
                return false;
            }
        }
    }

    internal class GsLcInsertInstrumentLBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertInstrumentLBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            try
            {
                editor.WriteMessage("\n正在插入仪表L块...");

                return true;
            }
            catch (System.Exception ex)
            {
                editor.WriteMessage($"\n插入仪表L块时发生错误: {ex.Message}");
                return false;
            }
        }
    }
}
