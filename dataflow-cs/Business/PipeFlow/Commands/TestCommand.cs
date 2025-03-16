using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using dataflow_cs.Business.PipeFlow.Views;
using dataflow_cs.Core.Services;
using dataflow_cs.Utils.CADUtils;
using dataflow_cs.Utils.Helpers;
using System;
using System.Windows;

namespace dataflow_cs.Business.PipeFlow.Commands
{
    /// <summary>
    /// 测试命令
    /// </summary>
    public class TestCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "CsTest";

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
                // 显示测试信息
                editor.WriteMessage("\n开始执行测试命令...");
                
                // 测试WindowExportData窗口
                TestWindowExportData();
                
                return true;
            }
            catch (Exception ex)
            {
                editor.WriteMessage($"\n执行测试命令时发生错误: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 测试WindowExportData窗口
        /// </summary>
        private void TestWindowExportData()
        {
            // 在UI线程中执行
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                try
                {
                    // 创建窗口
                    var window = new WindowExportData();
                    window.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"显示窗口时发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }));
        }
    }
} 