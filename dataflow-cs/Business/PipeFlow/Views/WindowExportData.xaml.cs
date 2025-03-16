using dataflow_cs.Business.PipeFlow.ViewModels;
using System.Windows;

namespace dataflow_cs.Business.PipeFlow.Views
{
    /// <summary>
    /// WindowExportData.xaml 的交互逻辑
    /// </summary>
    public partial class WindowExportData : Window
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public WindowExportData()
        {
            InitializeComponent();
            this.DataContext = new ExportDataViewModel(this);
        }
    }
} 