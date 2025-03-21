using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using dataflow_cs.Core.Extensions;
using dataflow_cs.Core.Services;
using dataflow_cs.Views;
using System;
using System.Windows;

namespace dataflow_cs.Business.GsLc.Commands
{
    /// <summary>
    /// 显示数据导出面板命令
    /// </summary>
    public class GsLcSysDataFromClientCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcSysDataFromClient";

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
                editor.WriteMessage("\n正在显示数据导出面板...");

                // 创建GsLcSysDataWindow实例并设置事件处理
                var window = new GsLcSysDataFromClient();

                // 显示窗口 - 使用我们的扩展方法确保正确显示
                window.ShowDialog(); 


                return true;
            }
            catch (Exception ex)
            {
                editor.WriteMessage($"\n显示数据导出面板时发生错误: {ex.Message}");
                return false;
            }
        }
    }
} 