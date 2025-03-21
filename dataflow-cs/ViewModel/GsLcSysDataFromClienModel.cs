using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace dataflow_cs.ViewModel
{
    public class GsLcSysDataFromClienModel : ViewModelBase
    {
        private ObservableCollection<DataTypeInfo> _dataTypeList;
        private DataTypeInfo _selectedDataType;
        private string _dataCount;
        private string _dataStatus;

        // 添加事件定义
        public event EventHandler ExportCompleted;
        public event EventHandler ExportCancelled;

        public GsLcSysDataFromClienModel()
        {
            // 初始化数据类型列表
            DataTypeList = new ObservableCollection<DataTypeInfo>()
            {
                new DataTypeInfo { Id = 1, Name = "管道数据" },
                new DataTypeInfo { Id = 2, Name = "仪表数据" },
                new DataTypeInfo { Id = 3, Name = "设备数据" },
                new DataTypeInfo { Id = 4, Name = "阀门管件数据" }
            };

            SelectedDataType = DataTypeList.FirstOrDefault();
            DataCount = "0";
            DataStatus = "就绪";

            // 初始化命令
            SelectCommand = new RelayCommand(ExecuteSelect);
            SelectAllCommand = new RelayCommand(ExecuteSelectAll);
            ExportCommand = new RelayCommand(ExecuteExport);
            CancelCommand = new RelayCommand(ExecuteCancel);
            CloseCommand = new RelayCommand(ExecuteClose);
        }

        #region 属性

        public ObservableCollection<DataTypeInfo> DataTypeList
        {
            get { return _dataTypeList; }
            set 
            { 
                _dataTypeList = value; 
                RaisePropertyChanged(() => DataTypeList);
            }
        }

        public DataTypeInfo SelectedDataType
        {
            get { return _selectedDataType; }
            set 
            { 
                _selectedDataType = value; 
                RaisePropertyChanged(() => SelectedDataType);
            }
        }

        public string DataCount
        {
            get { return _dataCount; }
            set 
            { 
                _dataCount = value; 
                RaisePropertyChanged(() => DataCount);
            }
        }

        public string DataStatus
        {
            get { return _dataStatus; }
            set 
            { 
                _dataStatus = value; 
                RaisePropertyChanged(() => DataStatus);
            }
        }

        #endregion

        #region 命令

        public RelayCommand SelectCommand { get; private set; }
        public RelayCommand SelectAllCommand { get; private set; }
        public RelayCommand ExportCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }
        public RelayCommand CloseCommand { get; private set; }

        #endregion

        #region 命令执行方法

        private void ExecuteSelect()
        {
            // 选取数据的实现
            DataStatus = "已选取数据...";
            DataCount = new Random().Next(10, 100).ToString(); // 演示用，实际应从CAD中获取
        }

        private void ExecuteSelectAll()
        {
            // 全选数据的实现
            DataStatus = "已全选数据...";
            DataCount = new Random().Next(100, 500).ToString(); // 演示用，实际应从CAD中获取
        }

        private void ExecuteExport()
        {
            // 导出数据的实现
            if (string.IsNullOrEmpty(DataCount) || DataCount == "0")
            {
                MessageBox.Show("没有选择任何数据，请先选择数据！");
                return;
            }

            DataStatus = "数据导出中...";
            // 实际导出逻辑
            MessageBox.Show($"已成功导出 {DataCount} 条{SelectedDataType?.Name}！");
            DataStatus = "导出完成";
            
            // 触发导出完成事件
            ExportCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void ExecuteCancel()
        {
            // 取消选择
            DataCount = "0";
            DataStatus = "已取消";
            
            // 触发取消事件
            ExportCancelled?.Invoke(this, EventArgs.Empty);
        }

        private void ExecuteClose()
        {
            // 关闭窗口逻辑
            // 触发取消事件
            ExportCancelled?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }

    public class DataTypeInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
