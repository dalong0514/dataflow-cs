using System;
using System.Windows;
using dataflow_cs.ViewModel;

namespace dataflow_cs.Views
{
    /// <summary>
    /// GsLcSysDataFromClient.xaml 的交互逻辑
    /// </summary>
    public partial class GsLcSysDataFromClient : Window
    {
        private readonly GsLcSysDataFromClientViewModel _viewModel;
        
        public GsLcSysDataFromClient()
        {
            InitializeComponent();
            
            // 创建并设置ViewModel
            _viewModel = new GsLcSysDataFromClientViewModel();
            
            // 设置数据上下文
            DataContext = _viewModel;
            
            // 订阅ViewModel事件
            _viewModel.ExportCompleted += (s, e) => 
            {
                // 可以在这里添加额外的UI逻辑
            };
            
            _viewModel.ExportCancelled += (s, e) => 
            {
                // 可以在这里添加额外的UI逻辑
            };
        }
        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            _viewModel.Close();
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
            _viewModel.CancelOperation();
        }
    }
} 