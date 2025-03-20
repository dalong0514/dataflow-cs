using System;
using System.Windows.Input;
using dataflow_cs.Business.GsLc.Views;

namespace dataflow_cs.Business.GsLc.Commands
{
    /// <summary>
    /// 导出CAD数据命令
    /// </summary>
    public class ExportCadDataCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return true; // 始终可执行
        }

        public void Execute(object parameter)
        {
            // 弹出导出CAD数据窗口
            var window = ExportCadDataWindow.ShowExportWindow();
            
            // 可以根据需要订阅窗口的导出完成和取消事件
            window.ExportCompleted += (s, e) =>
            {
                // 导出完成后的处理逻辑
                Console.WriteLine("CAD数据导出完成");
            };
            
            window.ExportCancelled += (s, e) =>
            {
                // 导出取消后的处理逻辑
                Console.WriteLine("CAD数据导出已取消");
            };
        }
    }
} 