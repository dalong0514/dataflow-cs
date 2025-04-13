﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using dataflow_cs.Core.Services;
using dataflow_cs.Utils.CADUtils;
using dataflow_cs.Presentation.Views.Windows;

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
            catch (Exception ex)
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
            using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {
                try
                {
                    editor.WriteMessage("\n正在插入全局数据流块...");
                    UtilsBlock.UtilsImportBlockFromExternalDwg(@"D:\dataflowcad\dataflowcad\allBlocks\GsLcBlocks.dwg", "GsLcValveBall");
                    // 提示用户选择插入点
                    PromptPointResult result = UtilsCADActive.Editor.GetPoint("\n请指定块的插入点: ");
                    
                    if (result.Status != PromptStatus.OK)
                    {
                        UtilsCADActive.WriteMessage("\n操作已取消。");
                        return false;
                    }

                    // 调用带有插入点参数的方法
                    UtilsBlock.UtilsInsertBlock("GsLcValveBall", result.Value, 1, 1, 1, 0, "0");

                    tr.Commit();

                    return true;
                }
                catch (Exception ex)
                {
                    editor.WriteMessage($"\n插入全局数据流块时发生错误: {ex.Message}");
                    return false;
                }
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
            catch (Exception ex)
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
            catch (Exception ex)
            {
                editor.WriteMessage($"\n插入仪表L块时发生错误: {ex.Message}");
                return false;
            }
        }
    }
}
