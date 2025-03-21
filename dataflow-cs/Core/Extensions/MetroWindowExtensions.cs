using MahApps.Metro.Controls;
using System;
using System.Windows;

namespace dataflow_cs.Core.Extensions
{
    /// <summary>
    /// MetroWindow的扩展方法类
    /// </summary>
    public static class MetroWindowExtensions
    {
        /// <summary>
        /// 显示模态对话框
        /// </summary>
        /// <param name="window">MetroWindow实例</param>
        /// <returns>对话框结果</returns>
        public static bool? ShowMetroDialog(this MetroWindow window)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));
                
            return window.ShowDialog();
        }
    }
} 