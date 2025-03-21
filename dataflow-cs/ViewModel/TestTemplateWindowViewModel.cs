using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

namespace dataflow_cs.ViewModel
{
    /// <summary>
    /// 测试模板窗口的视图模型
    /// </summary>
    internal class TestTemplateWindowViewModel : INotifyPropertyChanged
    {
        // 实现INotifyPropertyChanged接口
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // 导出完成事件
        public event EventHandler ExportCompleted;
        // 取消事件
        public event EventHandler ExportCancelled;
        
        // 初始化数据类型列表
        private ObservableCollection<DataType> _dataTypes = new ObservableCollection<DataType>
        {
            new DataType { Id = 1, Name = "管道数据" },
            new DataType { Id = 2, Name = "仪表数据" },
            new DataType { Id = 3, Name = "设备数据" },
            new DataType { Id = 4, Name = "阀门管件数据" }
        };

        public ObservableCollection<DataType> DataTypes 
        { 
            get => _dataTypes; 
        }

        private DataType _selectedDataType;
        public DataType SelectedDataType
        {
            get => _selectedDataType;
            set
            {
                _selectedDataType = value;
                OnPropertyChanged("SelectedDataType");
            }
        }

        private string _dataCount = "0";
        public string DataCount
        {
            get => _dataCount;
            set
            {
                _dataCount = value;
                OnPropertyChanged("DataCount");
            }
        }

        private string _dataStatus = "";
        public string DataStatus
        {
            get => _dataStatus;
            set
            {
                _dataStatus = value;
                OnPropertyChanged("DataStatus");
            }
        }

        public TestTemplateWindowViewModel()
        {
            // 默认选择第一项
            SelectedDataType = _dataTypes.FirstOrDefault();
        }

        public void Close()
        {
            ExportCancelled?.Invoke(this, EventArgs.Empty);
        }
        
        public void SelectData()
        {
            // 选取数据
            int count = new Random().Next(10, 100);
            DataCount = count.ToString();
            DataStatus = "已选取数据";
        }
        
        public void SelectAllData()
        {
            // 全选数据
            int count = new Random().Next(100, 500);
            DataCount = count.ToString();
            DataStatus = "已全选数据";
        }
        
        public void ExportData()
        {
            // 导出数据
            if (int.TryParse(DataCount, out int count) && count > 0)
            {
                DataStatus = "数据导出中...";
                
                // 获取选中的数据类型
                string typeName = SelectedDataType?.Name ?? "未知数据";
                
                // 导出逻辑
                MessageBox.Show($"已成功导出 {count} 条{typeName}！", "导出成功", MessageBoxButton.OK, MessageBoxImage.Information);
                DataStatus = "导出完成";
                
                // 触发导出完成事件
                ExportCompleted?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                MessageBox.Show("没有选择任何数据，请先选择数据！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        public void CancelOperation()
        {
            // 取消操作
            DataCount = "0";
            DataStatus = "已取消";
            
            // 触发取消事件
            ExportCancelled?.Invoke(this, EventArgs.Empty);
        }
    }
    
    /// <summary>
    /// 数据类型类
    /// </summary>
    public class DataType
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
