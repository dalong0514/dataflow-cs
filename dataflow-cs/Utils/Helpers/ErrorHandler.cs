using AcadApp = Autodesk.AutoCAD.ApplicationServices;
using dataflow_cs.Core.Services;
using System;
using System.Windows.Forms;

namespace dataflow_cs.Utils.Helpers
{
    /// <summary>
    /// 错误处理辅助类，提供全局错误处理方法
    /// </summary>
    public static class ErrorHandler
    {
        /// <summary>
        /// 是否显示详细错误信息
        /// </summary>
        public static bool ShowDetailedError { get; set; } = false;
        
        /// <summary>
        /// 处理异常并记录日志
        /// </summary>
        /// <param name="ex">捕获的异常</param>
        /// <param name="message">错误消息</param>
        /// <param name="showMessageBox">是否显示消息框</param>
        public static void HandleException(Exception ex, string message = "", bool showMessageBox = true)
        {
            // 记录到日志
            LoggingService.Instance.LogException(ex, message);
            
            // 输出到AutoCAD命令行
            if (!string.IsNullOrEmpty(message))
            {
                AcadApp.Document doc = AcadApp.Application.DocumentManager.MdiActiveDocument;
                if (doc != null)
                {
                    doc.Editor.WriteMessage("\n错误: " + message);
                    if (ShowDetailedError)
                    {
                        doc.Editor.WriteMessage("\n详细信息: " + ex.Message);
                    }
                }
            }
            
            // 显示错误对话框
            if (showMessageBox)
            {
                string boxMessage = string.IsNullOrEmpty(message) ? "操作过程中发生错误。" : message;
                
                if (ShowDetailedError)
                {
                    boxMessage += "\n\n详细信息: " + ex.Message;
                }
                
                MessageBox.Show(
                    boxMessage,
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        
        /// <summary>
        /// 处理错误并显示消息
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <param name="showMessageBox">是否显示消息框</param>
        public static void HandleError(string message, bool showMessageBox = true)
        {
            // 记录到日志
            LoggingService.Instance.LogError(message);
            
            // 输出到AutoCAD命令行
            AcadApp.Document doc = AcadApp.Application.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                doc.Editor.WriteMessage("\n错误: " + message);
            }
            
            // 显示错误对话框
            if (showMessageBox)
            {
                MessageBox.Show(
                    message,
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }
        
        /// <summary>
        /// 显示警告消息
        /// </summary>
        /// <param name="message">警告消息</param>
        /// <param name="showMessageBox">是否显示消息框</param>
        public static void ShowWarning(string message, bool showMessageBox = true)
        {
            // 记录到日志
            LoggingService.Instance.LogWarning(message);
            
            // 输出到AutoCAD命令行
            AcadApp.Document doc = AcadApp.Application.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                doc.Editor.WriteMessage("\n警告: " + message);
            }
            
            // 显示警告对话框
            if (showMessageBox)
            {
                MessageBox.Show(
                    message,
                    "警告",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }
        
        /// <summary>
        /// 显示信息消息
        /// </summary>
        /// <param name="message">信息消息</param>
        /// <param name="showMessageBox">是否显示消息框</param>
        public static void ShowInfo(string message, bool showMessageBox = false)
        {
            // 记录到日志
            LoggingService.Instance.LogInfo(message);
            
            // 输出到AutoCAD命令行
            AcadApp.Document doc = AcadApp.Application.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                doc.Editor.WriteMessage("\n" + message);
            }
            
            // 显示信息对话框
            if (showMessageBox)
            {
                MessageBox.Show(
                    message,
                    "信息",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }
    }
} 