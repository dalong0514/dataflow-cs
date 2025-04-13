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
                
                // 引入工艺数据流组件块定义
                ObjectId blockId = UtilsBlock.UtilsImportBlockFromExternalDwg(ConstFileName.GsLcBlocksPath, "GsLcElement");
                if (blockId == ObjectId.Null)
                {
                    editor.WriteMessage("\n导入块定义失败，请检查块文件路径和块名称。");
                    return false;
                }

                // 使用封装的拖拽插入方法（自动计算初始点）
                bool result = InsertBlockJig.DragAndInsertBlock(
                    editor,
                    database,
                    "工艺组件",
                    blockId,
                    0, // 初始旋转角度为0
                    "0", // 图层设置为"0"
                    "请选择插入点或输入[旋转(R)]:",
                    "命令已取消。",
                    "工艺组件已插入，继续拖动放置新的工艺组件，输入\"R\"可旋转，ESC退出",
                    "工艺组件已旋转，当前角度: {1}度"
                );

                return result;
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

                // 使用封装的拖拽插入方法（自动计算初始点）
                bool result = InsertBlockJig.DragAndInsertBlock(
                    editor,
                    database,
                    "GsLcValveBall",
                    blockId,
                    0, // 初始旋转角度为0
                    "0", // 图层设置为"0"
                    "请选择插入点或输入[旋转(R)]:",
                    "命令已取消。",
                    "球阀已插入，继续拖动放置新的球阀，输入\"R\"可旋转，ESC退出",
                    "球阀已旋转，当前角度: {1}度"
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
                editor.WriteMessage("\n正在插入仪表P块...");
                
                // 引入仪表P块定义
                ObjectId blockId = UtilsBlock.UtilsImportBlockFromExternalDwg(ConstFileName.GsLcBlocksPath, "GsLcInstrumentP");
                if (blockId == ObjectId.Null)
                {
                    editor.WriteMessage("\n导入块定义失败，请检查块文件路径和块名称。");
                    return false;
                }

                // 使用封装的拖拽插入方法（自动计算初始点）
                bool result = InsertBlockJig.DragAndInsertBlock(
                    editor,
                    database,
                    "仪表P",
                    blockId,
                    0, // 初始旋转角度为0
                    "0", // 图层设置为"0"
                    "请选择插入点或输入[旋转(R)]:",
                    "命令已取消。",
                    "仪表P已插入，继续拖动放置新的仪表P，输入\"R\"可旋转，ESC退出",
                    "仪表P已旋转，当前角度: {1}度"
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
                editor.WriteMessage("\n正在插入仪表L块...");
                
                // 引入仪表L块定义
                ObjectId blockId = UtilsBlock.UtilsImportBlockFromExternalDwg(ConstFileName.GsLcBlocksPath, "GsLcInstrumentL");
                if (blockId == ObjectId.Null)
                {
                    editor.WriteMessage("\n导入块定义失败，请检查块文件路径和块名称。");
                    return false;
                }

                // 使用封装的拖拽插入方法（自动计算初始点）
                bool result = InsertBlockJig.DragAndInsertBlock(
                    editor,
                    database,
                    "仪表L",
                    blockId,
                    0, // 初始旋转角度为0
                    "0", // 图层设置为"0"
                    "请选择插入点或输入[旋转(R)]:",
                    "命令已取消。",
                    "仪表L已插入，继续拖动放置新的仪表L，输入\"R\"可旋转，ESC退出",
                    "仪表L已旋转，当前角度: {1}度"
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
