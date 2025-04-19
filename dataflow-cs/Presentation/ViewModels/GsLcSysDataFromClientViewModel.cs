using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using dataflow_cs.Utils.CADUtils;
using dataflow_cs.Utils.ConstUtils;

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
            // 1. 读取本地JSON文件
            string jsonFilePath = ConstFileName.GsLcPipeDataImportPath;
            if (!File.Exists(jsonFilePath))
            {
                UtilsCADActive.Editor.WriteMessage($"\n找不到文件: {jsonFilePath}");
                return;
            }

            List<JObject> jsonArray = UtilsCommon.UtilsReadLocalJsonData<JObject>(jsonFilePath);
            
            // 3. 筛选出管道数据并转换为Dictionary<string, string>
            List<Dictionary<string, string>> pipeDataList = new List<Dictionary<string, string>>();
            foreach (JObject item in jsonArray)
            {
                if (item["data_class"]?.ToString() == "pipeline")
                {
                    Dictionary<string, string> pipeData = new Dictionary<string, string>();
                    foreach (var property in item.Properties())
                    {
                        pipeData[property.Name] = property.Value.ToString();
                    }
                    pipeDataList.Add(pipeData);
                }
            }
            pipeNumObjectIds.ForEach(pipeNumObjectId => {
                try
                {
                    // 4. 获取当前管道的ObjectId的句柄值
                    string currentHandle = pipeNumObjectId.Handle.ToString();
                    
                    // 5. 查找匹配的管道数据
                    Dictionary<string, string> matchedPipeData = pipeDataList.FirstOrDefault(
                        data => data.ContainsKey("entityhandle") && data["entityhandle"] == currentHandle);
                    
                    if (matchedPipeData != null)
                    {
                        // 6. 调用函数修改块属性值
                        UtilsBlock.UtilsSetPropertyValueByDictData(pipeNumObjectId, matchedPipeData);
                        // UtilsCADActive.Editor.WriteMessage($"\n已同步管道数据: {pipeNum}");
                    }
                }
                catch (Exception ex)
                {
                    UtilsCADActive.Editor.WriteMessage($"\n同步管道数据时发生错误: {ex.Message}");
                }
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
