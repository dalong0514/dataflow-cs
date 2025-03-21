using System;
using System.Windows.Controls;

namespace dataflow_cs.Views
{
    /// <summary>
    /// GsLcSysDataFromClient.xaml 的交互逻辑
    /// </summary>
    public partial class GsLcSysDataFromClient : UserControl
    {
        // 导出完成事件
        public event EventHandler ExportCompleted;
        // 取消事件
        public event EventHandler ExportCancelled;

        public GsLcSysDataFromClient()
        {
            // 在框架编译时，此方法会被自动实现
        }
    }
}
