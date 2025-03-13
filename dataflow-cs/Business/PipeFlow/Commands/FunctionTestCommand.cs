using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using dataflow_cs.Core.Services;
using dataflow_cs.Utils.Helpers;
using System;
using System.Collections.Generic;

namespace dataflow_cs.Business.PipeFlow.Commands
{
    /// <summary>
    /// 功能测试命令类
    /// </summary>
    public class FunctionTestCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "CsFuncitonTest";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            try
            {
                editor.WriteMessage("\n开始执行功能测试命令...");
                
                // 调用原有的批量同步管道数据方法
                GsPgDataFlow.ToolManager.GsPgBatchSynPipeData();
                
                editor.WriteMessage("\n功能测试命令执行完成。");
                return true;
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "执行功能测试命令时发生错误");
                return false;
            }
        }
    }
} 