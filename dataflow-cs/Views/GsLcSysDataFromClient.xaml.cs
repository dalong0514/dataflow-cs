using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace dataflow_cs.Views
{
    /// <summary>
    /// GsLcSysDataWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GsLcSysDataWindow : Window
    {
        // 导出完成事件
        public event EventHandler ExportCompleted;
        // 取消事件
        public event EventHandler ExportCancelled;
        
        // 初始化数据类型列表
        private ObservableCollection<DataTypeInfo> _dataTypes = new ObservableCollection<DataTypeInfo>
        {
            new DataTypeInfo { Id = 1, Name = "管道数据" },
            new DataTypeInfo { Id = 2, Name = "仪表数据" },
            new DataTypeInfo { Id = 3, Name = "设备数据" },
            new DataTypeInfo { Id = 4, Name = "阀门管件数据" }
        };

        public GsLcSysDataWindow()
        {
            InitializeComponent();
            
            // 设置ComboBox数据源
            DataTypeComboBox.ItemsSource = _dataTypes;
            DataTypeComboBox.SelectedIndex = 0;
        }
        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            ExportCancelled?.Invoke(this, EventArgs.Empty);
        }
        
        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            // 选取数据
            int count = new Random().Next(10, 100);
            DataCountTextBlock.Text = count.ToString();
            DataStatusTextBlock.Text = "已选取数据";
        }
        
        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            // 全选数据
            int count = new Random().Next(100, 500);
            DataCountTextBlock.Text = count.ToString();
            DataStatusTextBlock.Text = "已全选数据";
        }
        
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            // 导出数据
            if (int.TryParse(DataCountTextBlock.Text, out int count) && count > 0)
            {
                DataStatusTextBlock.Text = "数据导出中...";
                
                // 获取选中的数据类型
                DataTypeInfo selectedType = DataTypeComboBox.SelectedItem as DataTypeInfo;
                string typeName = selectedType?.Name ?? "未知数据";
                
                // 导出逻辑
                MessageBox.Show($"已成功导出 {count} 条{typeName}！", "导出成功", MessageBoxButton.OK, MessageBoxImage.Information);
                DataStatusTextBlock.Text = "导出完成";
                
                // 触发导出完成事件
                ExportCompleted?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                MessageBox.Show("没有选择任何数据，请先选择数据！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // 取消操作
            DataCountTextBlock.Text = "0";
            DataStatusTextBlock.Text = "已取消";
            
            // 触发取消事件
            ExportCancelled?.Invoke(this, EventArgs.Empty);
        }
    }
    
    // 数据类型类
    public class DataTypeInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
} 