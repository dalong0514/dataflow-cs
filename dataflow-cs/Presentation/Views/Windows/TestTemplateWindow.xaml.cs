using System;
using System.Windows;
using MahApps.Metro.Controls;
using dataflow_cs.Presentation.ViewModel;

namespace dataflow_cs.Presentation.Views.Windows
{
    /// <summary>
    /// TestTemplateWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TestTemplateWindow : MetroWindow
    {
        private readonly TestTemplateWindowViewModel _viewModel;
        
        public TestTemplateWindow()
        {
            InitializeComponent();
            
            // 创建并设置ViewModel
            _viewModel = new TestTemplateWindowViewModel();
            
            // 设置数据上下文
            DataContext = _viewModel;
            
            // 订阅ViewModel事件
            _viewModel.ExportCompleted += (s, e) => 
            {
                // 导出完成后的UI逻辑
            };
            
            _viewModel.ExportCancelled += (s, e) => 
            {
                // 导出取消后的UI逻辑
            };
            
            // 关闭窗口时清理资源
            this.Closed += (s, e) => _viewModel.Close();
        }
        
        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectData();
        }
        
        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectAllData();
        }
        
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ExportData();
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
} 