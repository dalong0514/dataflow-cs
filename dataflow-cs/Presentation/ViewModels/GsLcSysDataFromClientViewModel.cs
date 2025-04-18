using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using dataflow_cs.Utils.CADUtils;

namespace dataflow_cs.Presentation.ViewModel
{
    internal class GsLcSysDataFromClientViewModel : INotifyPropertyChanged
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
        private ObservableCollection<DataTypeInfo> _dataTypes = new ObservableCollection<DataTypeInfo>
        {
            new DataTypeInfo { Id = 1, Name = "管道数据" },
            new DataTypeInfo { Id = 2, Name = "仪表数据" },
            new DataTypeInfo { Id = 3, Name = "设备数据" },
            new DataTypeInfo { Id = 4, Name = "阀门管件数据" }
        };

        public ObservableCollection<DataTypeInfo> DataTypes 
        { 
            get => _dataTypes; 
        }

        private DataTypeInfo _selectedDataType;
        public DataTypeInfo SelectedDataType
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

        public GsLcSysDataFromClientViewModel()
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
            using (Transaction tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {

                try
                {
                    // 获取选中的数据类型
                    string typeName = SelectedDataType?.Name ?? "未知数据";
                    if (typeName == "管道数据")
                    {
                        List<ObjectId> pipeNumObjectIds = UtilsBlock.UtilsGetAllObjectIdsByBlockName(new List<string> { "PipeArrowLeft", "PipeArrowRight" }).ToList();
                        if (pipeNumObjectIds != null && pipeNumObjectIds.Count > 0)
                        {
                            SyncGsLcPipeData(pipeNumObjectIds);
                            // 同步管道数据
                            DataStatus = "同步完成";
                            // 触发导出完成事件
                            ExportCompleted?.Invoke(this, EventArgs.Empty);
                        }
                        else
                        {
                            DataStatus = "没有找到要同步的块...";
                        }
                        
                    }
                    else if (typeName == "仪表数据")
                    {
                        // 同步仪表数据
                    }
                    else if (typeName == "设备数据")
                    {
                        // 同步设备数据
                    }
                    else if (typeName == "阀门管件数据")
                    {
                        // 同步阀门管件数据
                    }
                    
                    tr.Commit();
                }
                catch (System.Exception ex)
                {
                    UtilsCADActive.Editor.WriteMessage($"\n同步发生错误: {ex.Message}");
                    tr.Abort();
                }
            }

            // if (int.TryParse(DataCount, out int count) && count > 0)
            // {
            //     DataStatus = "同步中...";
                
            //     // 导出逻辑
            //     MessageBox.Show($"已成功导出 {count} 条{typeName}！", "导出成功", MessageBoxButton.OK, MessageBoxImage.Information);
            // }
            // else
            // {
            //     MessageBox.Show("没有选择任何数据，请先选择数据！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            // }
        }
        
        public void CancelOperation()
        {
            // 取消操作
            DataCount = "0";
            DataStatus = "已取消";
            
            // 触发取消事件
            ExportCancelled?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 同步管道数据
        /// </summary>
        private void SyncGsLcPipeData(List<ObjectId> pipeNumObjectIds)
        {
            pipeNumObjectIds.ForEach(pipeNumObjectId => {
                // 获取管道编号
                string pipeNum = UtilsBlock.UtilsGetPropertyValueByPropertyName(pipeNumObjectId, "PipeNum");
                UtilsCADActive.UtilsDeleteEntity(pipeNumObjectId);
            });
        }

    }
    
    // 数据类型类
    public class DataTypeInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
