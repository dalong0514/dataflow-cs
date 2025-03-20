using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using dataflow_cs.Core.Services;
using dataflow_cs.Business.GsLc.Views;
using System;
using System.Windows;

namespace dataflow_cs.Business.GsLc.Commands
{
    /// <summary>
    /// 显示UserControl1面板命令
    /// </summary>
    public class ShowUserControl1Command : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLShowUserControl1";

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
                editor.WriteMessage("\n正在显示UserControl1面板...");

                // 创建一个新窗口来承载UserControl1
                Window window = new Window
                {
                    Title = "天正数据设计",
                    Width = 520,
                    Height = 350,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize
                };

                // 创建UserControl1实例并设置事件处理
                UserControl1 userControl = new UserControl1();
                userControl.ExportCompleted += (sender, e) =>
                {
                    editor.WriteMessage("\n导出操作已完成");
                };
                userControl.ExportCancelled += (sender, e) =>
                {
                    editor.WriteMessage("\n导出操作已取消");
                };

                // 设置窗口内容
                window.Content = userControl;
                
                // 显示窗口
                window.ShowDialog();

                return true;
            }
            catch (Exception ex)
            {
                editor.WriteMessage($"\n显示UserControl1面板时发生错误: {ex.Message}");
                return false;
            }
        }
    }
} 