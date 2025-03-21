using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace dataflow_cs.Views
{
    /// <summary>
    /// GsLcSysDataFromClient.xaml 的交互逻辑
    /// </summary>
    public partial class GsLcSysDataFromClient : UserControl, INotifyPropertyChanged
    {
        private int _dataCount;
        private string _dataStatus;

        public event PropertyChangedEventHandler PropertyChanged;

        public int DataCount
        {
            get { return _dataCount; }
            set
            {
                if (_dataCount != value)
                {
                    _dataCount = value;
                    OnPropertyChanged("DataCount");
                    TextBlockDataCount.Text = value.ToString();
                }
            }
        }

        public string DataStatus
        {
            get { return _dataStatus; }
            set
            {
                if (_dataStatus != value)
                {
                    _dataStatus = value;
                    OnPropertyChanged("DataStatus");
                    TextBlockDataStatus.Text = value;
                }
            }
        }

        // 导出完成事件
        public event EventHandler ExportCompleted;
        // 取消事件
        public event EventHandler ExportCancelled;

        public GsLcSysDataFromClient()
        {
            InitializeComponent();
            DataContext = this;
            DataCount = 0;
            DataStatus = "未开始";
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            ExportCancelled?.Invoke(this, EventArgs.Empty);
            Window.GetWindow(this)?.Close();
        }

        private void ButtonSelect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 这里实现选取功能
                // 模拟选取数据
                Random random = new Random();
                DataCount = random.Next(1, 100);
                DataStatus = "已选取";
                MessageBox.Show($"已选取{DataCount}条数据", "选取成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"选取失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonSelectAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 这里实现全选功能
                // 模拟全选数据
                Random random = new Random();
                DataCount = random.Next(100, 500);
                DataStatus = "已全选";
                MessageBox.Show($"已全选{DataCount}条数据", "全选成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"全选失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataCount <= 0)
                {
                    MessageBox.Show("请先选择要导出的数据", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string dataType = (ComboBoxDataType.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "未知数据";

                // 模拟导出操作
                DataStatus = "导出中...";

                // 实际项目中可以使用Task.Run执行耗时操作
                // 这里简化为直接模拟完成
                DataStatus = "导出完成";
                MessageBox.Show($"成功导出{DataCount}条{dataType}", "导出成功", MessageBoxButton.OK, MessageBoxImage.Information);

                // 触发导出完成事件
                ExportCompleted?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                DataStatus = "导出失败";
                MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            ExportCancelled?.Invoke(this, EventArgs.Empty);
            Window.GetWindow(this)?.Close();
        }
    }
}
