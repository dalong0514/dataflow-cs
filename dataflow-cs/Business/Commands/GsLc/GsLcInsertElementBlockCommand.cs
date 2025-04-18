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
    internal class GsLcInsertAllElementBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertAllElementBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            // 使用拖拽插入方法并获取插入的块ID
            using (Transaction tr = database.TransactionManager.StartTransaction())
            {

                try
                {
                    // 引入工艺数据流组件块定义
                    ObjectId blockId = UtilsGeometry.UtilsImportBlockFromExternalDwg(ConstFileName.GsLcBlocksPath, ConstBlockName.GsLcAllBlocks);
                    if (blockId == ObjectId.Null)
                    {
                        editor.WriteMessage("\n导入块定义失败，请检查块文件路径和块名称。");
                        return false;
                    }
                    // 拾取一个点
                    PromptPointResult pointResult = editor.GetPoint("\n请选择插入点:");
                    if (pointResult.Status != PromptStatus.OK)
                    {
                        editor.WriteMessage("\n拾取点失败，请重新选择。");
                        return false;
                    }
                    UtilsBlock.UtilsInsertBlock(ConstBlockName.GsLcAllBlocks, pointResult.Value);
                    // 获取最后插入的块引用ID
                    SelectionSet selSet = UtilsSelectionSet.UtilsGetLastCreatedObject();
                    ObjectId insertedBlockId = selSet.GetObjectIds().FirstOrDefault();
                    UtilsBlock.UtilsExplodeBlock(insertedBlockId, tr);
                    // 方法2: 使用带连字符的命令版本以避免显示对话框
                    UtilsCADActive.Document.SendStringToExecute("WIPEOUT F OFF ", true, false, false);
                    
                    tr.Commit();
                }
                catch (System.Exception ex)
                {
                    editor.WriteMessage($"\n插入工艺数据流组件块时发生错误: {ex.Message}");
                    tr.Abort();
                }
            }
            return true;
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
            try
            {
                string blockName = ConstBlockName.GsLcValveBall;
                // 引入球阀块定义
                ObjectId blockId = UtilsGeometry.UtilsImportBlockFromExternalDwg(ConstFileName.GsLcBlocksPath, blockName);
                if (blockId == ObjectId.Null)
                {
                    editor.WriteMessage("\n导入块定义失败，请检查块文件路径和块名称。");
                    return false;
                }

                // 使用封装的拖拽插入方法（自动计算初始点）
                bool result = InsertBlockJig.DragAndInsertBlock(
                    editor,
                    database,
                    blockName,
                    blockId
                );

                return result;
            }
            catch (System.Exception ex)
            {
                editor.WriteMessage($"\n插入块时发生错误: {ex.Message}");
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
                string blockName = ConstBlockName.GsLcInstrumentP;
                
                // 引入仪表P块定义
                ObjectId blockId = UtilsGeometry.UtilsImportBlockFromExternalDwg(ConstFileName.GsLcBlocksPath, blockName);
                if (blockId == ObjectId.Null)
                {
                    editor.WriteMessage("\n导入块定义失败，请检查块文件路径和块名称。");
                    return false;
                }

                // 使用封装的拖拽插入方法（自动计算初始点）
                bool result = InsertBlockJig.DragAndInsertBlock(
                    editor,
                    database,
                    blockName,
                    blockId
                );

                return result;
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
                string blockName = ConstBlockName.GsLcInstrumentL;
                
                // 引入仪表L块定义
                ObjectId blockId = UtilsGeometry.UtilsImportBlockFromExternalDwg(ConstFileName.GsLcBlocksPath, blockName);
                if (blockId == ObjectId.Null)
                {
                    editor.WriteMessage("\n导入块定义失败，请检查块文件路径和块名称。");
                    return false;
                }

                // 使用封装的拖拽插入方法（自动计算初始点）
                bool result = InsertBlockJig.DragAndInsertBlock(
                    editor,
                    database,
                    blockName,
                    blockId
                );

                return result;
            }
            catch (System.Exception ex)
            {
                editor.WriteMessage($"\n插入仪表L块时发生错误: {ex.Message}");
                return false;
            }
        }
    }
}
