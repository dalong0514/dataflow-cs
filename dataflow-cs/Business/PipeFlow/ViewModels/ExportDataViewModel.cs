using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using dataflow_cs.Utils.CADUtils;
using dataflow_cs.Utils.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace dataflow_cs.Business.PipeFlow.ViewModels
{
    /// <summary>
    /// 导出数据视图模型
    /// </summary>
    public class ExportDataViewModel : INotifyPropertyChanged
    {
        #region 属性

        private string _dataType;
        private ObservableCollection<string> _dataTypeList;
        private double _exportDataCount;
        private string _exportDataStatus;
        private Window _window;

        /// <summary>
        /// 数据类型
        /// </summary>
        public string DataType
        {
            get { return _dataType; }
            set { _dataType = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 数据类型列表
        /// </summary>
        public ObservableCollection<string> DataTypeList
        {
            get { return _dataTypeList; }
            set { _dataTypeList = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 导出数据数量
        /// </summary>
        public double ExportDataCount
        {
            get { return _exportDataCount; }
            set { _exportDataCount = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 导出数据状态
        /// </summary>
        public string ExportDataStatus
        {
            get { return _exportDataStatus; }
            set { _exportDataStatus = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 导出窗口
        /// </summary>
        public Window Window
        {
            get { return _window; }
            set { _window = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 目标JSON数组
        /// </summary>
        public JArray TargetJArray { get; set; } = new JArray();

        /// <summary>
        /// 目标路径
        /// </summary>
        public string TargetPath { get; set; } = string.Empty;

        /// <summary>
        /// 导出消息
        /// </summary>
        public string ExportMessage { get; set; } = string.Empty;

        /// <summary>
        /// 管道块名称列表
        /// </summary>
        public List<string> PipeBlockNames { get; set; } = new List<string>()
        {
            "GsPgPipeElementArrowAssist",
        };

        /// <summary>
        /// 仪表块名称列表
        /// </summary>
        public List<string> InstrumentBlockNames { get; set; } = new List<string>()
        {
            "GsPgInstrument",
        };

        /// <summary>
        /// 设备块名称列表
        /// </summary>
        public List<string> EquipBlockNames { get; set; } = new List<string>()
        {
            "GsPgEquip",
        };

        /// <summary>
        /// 阀门块名称列表
        /// </summary>
        public List<string> ValveBlockNames { get; set; } = new List<string>()
        {
            "GsPgValve",
        };

        #endregion

        #region 命令

        /// <summary>
        /// 批量选择命令
        /// </summary>
        public ICommand BatchSelectionCommand { get; private set; }

        /// <summary>
        /// 全选命令
        /// </summary>
        public ICommand AllSelectionCommand { get; private set; }

        /// <summary>
        /// 导出命令
        /// </summary>
        public ICommand ExportCommand { get; private set; }

        /// <summary>
        /// 取消命令
        /// </summary>
        public ICommand CancelCommand { get; private set; }

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="window">窗口</param>
        public ExportDataViewModel(Window window)
        {
            // 初始化属性
            DataTypeList = new ObservableCollection<string> { "管道数据", "仪表数据", "设备数据", "阀门管件数据" };
            DataType = DataTypeList[0];
            Window = window;

            // 初始化命令
            BatchSelectionCommand = new RelayCommand(BatchSelection);
            AllSelectionCommand = new RelayCommand(AllSelection);
            ExportCommand = new RelayCommand(Export);
            CancelCommand = new RelayCommand(Cancel);
        }

        /// <summary>
        /// 批量选择
        /// </summary>
        private void BatchSelection()
        {
            try
            {
                // 使用事务处理
                using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
                {
                    // 获取图框信息
                    var drawInfo = GetDrawInfo();
                    if (drawInfo == null)
                    {
                        MessageBox.Show("请放入图框！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // 创建项目根节点
                    var projectRoot = CreateProjectRoot(drawInfo);

                    // 根据数据类型执行不同的选择逻辑
                    switch (DataType)
                    {
                        case "管道数据":
                            ProcessPipeData(projectRoot);
                            break;
                        case "仪表数据":
                            ProcessInstrumentData(projectRoot);
                            break;
                        case "设备数据":
                            ProcessEquipData(projectRoot);
                            break;
                        case "阀门管件数据":
                            ProcessValveData(projectRoot);
                            break;
                    }

                    // 更新UI
                    ExportDataCount = TargetJArray.Count - 1;
                    ExportDataStatus = "";
                    Window?.Focus();

                    tr.Commit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"批量选择时发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 全选
        /// </summary>
        private void AllSelection()
        {
            try
            {
                // 使用事务处理
                using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
                {
                    // 获取图框信息
                    var drawInfo = GetDrawInfo();
                    if (drawInfo == null)
                    {
                        MessageBox.Show("请放入图框！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // 创建项目根节点
                    var projectRoot = CreateProjectRoot(drawInfo);

                    // 根据数据类型执行不同的全选逻辑
                    switch (DataType)
                    {
                        case "管道数据":
                            ProcessAllPipeData(projectRoot);
                            break;
                        case "仪表数据":
                            ProcessAllInstrumentData(projectRoot);
                            break;
                        case "设备数据":
                            ProcessAllEquipData(projectRoot);
                            break;
                        case "阀门管件数据":
                            ProcessAllValveData(projectRoot);
                            break;
                    }

                    // 更新UI
                    ExportDataCount = TargetJArray.Count - 1;
                    ExportDataStatus = "";

                    tr.Commit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"全选时发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 导出
        /// </summary>
        private void Export()
        {
            try
            {
                if (string.IsNullOrEmpty(TargetPath))
                {
                    MessageBox.Show("请先点击选取或全选按钮!", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 确保目录存在
                var directory = Path.GetDirectoryName(TargetPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 写入文件
                File.WriteAllText(TargetPath, TargetJArray.ToString());
                ExportDataStatus = "已完成";
                
                // 输出消息
                UtilsCADActive.Editor.WriteMessage($"\n{ExportMessage}");
                
                MessageBox.Show(ExportMessage, "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出数据时发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 取消
        /// </summary>
        private void Cancel()
        {
            Window?.Close();
        }

        #region 辅助方法

        /// <summary>
        /// 获取图框信息
        /// </summary>
        /// <returns>图框信息</returns>
        private JObject GetDrawInfo()
        {
            return UtilsBlock.GetDrawInfo();
        }

        /// <summary>
        /// 创建项目根节点
        /// </summary>
        /// <param name="drawInfo">图框信息</param>
        /// <returns>项目根节点</returns>
        private JObject CreateProjectRoot(JObject drawInfo)
        {
            try
            {
                // 创建项目根节点
                JObject projectRoot = new JObject();
                
                // 添加项目信息
                projectRoot["projectName"] = drawInfo["projectName"]?.ToString() ?? "未知项目";
                projectRoot["projectNum"] = drawInfo["projectNum"]?.ToString() ?? "未知编号";
                projectRoot["drawingNum"] = drawInfo["drawingNum"]?.ToString() ?? "未知图号";
                projectRoot["drawingName"] = drawInfo["drawingName"]?.ToString() ?? "未知图名";
                projectRoot["drawingDate"] = drawInfo["drawingDate"]?.ToString() ?? DateTime.Now.ToString("yyyy-MM-dd");
                
                // 添加到目标数组
                TargetJArray = new JArray();
                TargetJArray.Add(projectRoot);
                
                return projectRoot;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"创建项目根节点时发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// 批量选择块
        /// </summary>
        private List<BlockReference> BatchSelectBlocks(List<string> blockNames, string message)
        {
            // 创建选择过滤器
            var filter = new SelectionFilter(new[] {
                new TypedValue((int)DxfCode.Start, "INSERT"),
                new TypedValue((int)DxfCode.BlockName, string.Join(",", blockNames))
            });

            // 创建选择选项
            var options = new PromptSelectionOptions();
            options.MessageForAdding = message;

            // 获取选择结果
            var result = UtilsCADActive.Editor.GetSelection(options, filter);
            if (result.Status != PromptStatus.OK)
            {
                return new List<BlockReference>();
            }

            // 转换为块引用列表
            return result.Value.GetObjectIds()
                .Select(id => id.GetObject(OpenMode.ForRead) as BlockReference)
                .Where(br => br != null)
                .ToList();
        }

        /// <summary>
        /// 处理管道数据
        /// </summary>
        /// <param name="projectRoot">项目根节点</param>
        private void ProcessPipeData(JObject projectRoot)
        {
            try
            {
                // 获取用户选择的管道元素
                var pipeElements = GetSelectedPipeElements();
                if (pipeElements == null || pipeElements.Count == 0)
                {
                    ExportDataStatus = "未选择任何管道元素";
                    return;
                }
                
                // 处理每个管道元素
                foreach (var element in pipeElements)
                {
                    // 获取块属性
                    var attrs = UtilsBlock.GetBlockAttrs(element);
                    
                    // 创建管道数据对象
                    JObject pipeData = new JObject();
                    pipeData["elementType"] = "Pipe";
                    pipeData["pipeNumber"] = attrs.ContainsKey("PIPENUMBER") ? attrs["PIPENUMBER"] : "";
                    pipeData["pipeSize"] = attrs.ContainsKey("PIPESIZE") ? attrs["PIPESIZE"] : "";
                    pipeData["pipeSpec"] = attrs.ContainsKey("PIPESPEC") ? attrs["PIPESPEC"] : "";
                    
                    // 添加到目标数组
                    TargetJArray.Add(pipeData);
                }
                
                ExportDataStatus = $"已选择 {pipeElements.Count} 个管道元素";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"处理管道数据时发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 获取选择的管道元素
        /// </summary>
        /// <returns>管道元素列表</returns>
        private List<BlockReference> GetSelectedPipeElements()
        {
            try
            {
                // 提示用户选择管道元素
                var editor = UtilsCADActive.Editor;
                editor.WriteMessage("\n请选择管道元素: ");
                
                // 创建选择过滤器
                TypedValue[] filterList = new TypedValue[]
                {
                    new TypedValue((int)DxfCode.Start, "INSERT"),
                };
                
                // 获取用户选择
                var selectionResult = editor.GetSelection(new SelectionFilter(filterList));
                if (selectionResult.Status != PromptStatus.OK)
                {
                    return new List<BlockReference>();
                }
                
                // 获取选择的对象ID
                var objectIds = selectionResult.Value.GetObjectIds();
                
                // 过滤出管道块
                List<BlockReference> pipeElements = new List<BlockReference>();
                using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
                {
                    foreach (var objectId in objectIds)
                    {
                        BlockReference blockRef = tr.GetObject(objectId, OpenMode.ForRead) as BlockReference;
                        if (blockRef != null)
                        {
                            string blockName = UtilsBlock.UtilsGetBlockName(objectId);
                            if (PipeBlockNames.Contains(blockName))
                            {
                                pipeElements.Add(blockRef);
                            }
                        }
                    }
                    tr.Commit();
                }
                
                return pipeElements;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取管道元素时发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<BlockReference>();
            }
        }

        /// <summary>
        /// 获取所有管道元素
        /// </summary>
        /// <returns>管道元素列表</returns>
        private List<BlockReference> GetAllPipeElements()
        {
            try
            {
                // 获取所有块
                List<BlockReference> pipeElements = new List<BlockReference>();
                using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
                {
                    // 获取所有块ID
                    var blockIds = UtilsBlock.UtilsGetAllBlockObjectIds();
                    
                    // 过滤出管道块
                    foreach (var objectId in blockIds)
                    {
                        BlockReference blockRef = tr.GetObject(objectId, OpenMode.ForRead) as BlockReference;
                        if (blockRef != null)
                        {
                            string blockName = UtilsBlock.UtilsGetBlockName(objectId);
                            if (PipeBlockNames.Contains(blockName))
                            {
                                pipeElements.Add(blockRef);
                            }
                        }
                    }
                    tr.Commit();
                }
                
                return pipeElements;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取所有管道元素时发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<BlockReference>();
            }
        }

        /// <summary>
        /// 处理仪表数据
        /// </summary>
        /// <param name="projectRoot">项目根节点</param>
        private void ProcessInstrumentData(JObject projectRoot)
        {
            // 实现仪表数据处理逻辑
            ExportDataStatus = "仪表数据处理功能尚未实现";
        }

        /// <summary>
        /// 处理设备数据
        /// </summary>
        /// <param name="projectRoot">项目根节点</param>
        private void ProcessEquipData(JObject projectRoot)
        {
            // 实现设备数据处理逻辑
            ExportDataStatus = "设备数据处理功能尚未实现";
        }

        /// <summary>
        /// 处理阀门数据
        /// </summary>
        /// <param name="projectRoot">项目根节点</param>
        private void ProcessValveData(JObject projectRoot)
        {
            // 实现阀门数据处理逻辑
            ExportDataStatus = "阀门数据处理功能尚未实现";
        }

        /// <summary>
        /// 处理所有管道数据
        /// </summary>
        private void ProcessAllPipeData(JObject projectRoot)
        {
            var pipeJArray = new JArray();
            pipeJArray.Add(projectRoot);

            // 获取所有管道块
            var pipeObjectIds = UtilsBlock.UtilsGetAllObjectIdsByBlockNameList(PipeBlockNames);
            if (pipeObjectIds != null)
            {
                // 添加管道数据
                foreach (var objectId in pipeObjectIds)
                {
                    var root = new JObject();
                    root["data_class"] = "pipe";
                    root["entityhandle"] = objectId.Handle.ToString();

                    // 获取块属性
                    var attrs = UtilsBlock.UtilsGetAllPropertyDictList(objectId);
                    foreach (var attr in attrs)
                    {
                        root[attr.Key.ToLower()] = attr.Value;
                    }
                    
                    root["blockname"] = UtilsBlock.UtilsGetBlockName(objectId);
                    pipeJArray.Add(root);
                }
            }

            // 设置目标路径和数组
            TargetPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DataFlow", "PipeData.json");
            TargetJArray = pipeJArray;
            ExportMessage = "管道数据导出成功";
        }

        /// <summary>
        /// 处理所有仪表数据
        /// </summary>
        private void ProcessAllInstrumentData(JObject projectRoot)
        {
            var instrumentJArray = new JArray();
            instrumentJArray.Add(projectRoot);

            // 获取所有仪表块
            var instrumentObjectIds = UtilsBlock.UtilsGetAllObjectIdsByBlockNameList(InstrumentBlockNames);
            if (instrumentObjectIds != null)
            {
                // 添加仪表数据
                foreach (var objectId in instrumentObjectIds)
                {
                    var root = new JObject();
                    root["data_class"] = "instrument";
                    root["entityhandle"] = objectId.Handle.ToString();

                    // 获取块属性
                    var attrs = UtilsBlock.UtilsGetAllPropertyDictList(objectId);
                    foreach (var attr in attrs)
                    {
                        root[attr.Key.ToLower()] = attr.Value;
                    }
                    
                    root["blockname"] = UtilsBlock.UtilsGetBlockName(objectId);
                    instrumentJArray.Add(root);
                }
            }

            // 设置目标路径和数组
            TargetPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DataFlow", "InstrumentData.json");
            TargetJArray = instrumentJArray;
            ExportMessage = "仪表数据导出成功";
        }

        /// <summary>
        /// 处理所有设备数据
        /// </summary>
        private void ProcessAllEquipData(JObject projectRoot)
        {
            var equipJArray = new JArray();
            equipJArray.Add(projectRoot);

            // 获取所有设备块
            var equipObjectIds = UtilsBlock.UtilsGetAllObjectIdsByBlockNameList(EquipBlockNames);
            if (equipObjectIds != null)
            {
                // 添加设备数据
                foreach (var objectId in equipObjectIds)
                {
                    var root = new JObject();
                    root["data_class"] = "equip";
                    root["entityhandle"] = objectId.Handle.ToString();

                    // 获取块属性
                    var attrs = UtilsBlock.UtilsGetAllPropertyDictList(objectId);
                    foreach (var attr in attrs)
                    {
                        root[attr.Key.ToLower()] = attr.Value;
                    }
                    
                    root["blockname"] = UtilsBlock.UtilsGetBlockName(objectId);
                    equipJArray.Add(root);
                }
            }

            // 设置目标路径和数组
            TargetPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DataFlow", "EquipData.json");
            TargetJArray = equipJArray;
            ExportMessage = "设备数据导出成功";
        }

        /// <summary>
        /// 处理所有阀门数据
        /// </summary>
        private void ProcessAllValveData(JObject projectRoot)
        {
            var valveJArray = new JArray();
            valveJArray.Add(projectRoot);

            // 获取所有阀门块
            var valveObjectIds = UtilsBlock.UtilsGetAllObjectIdsByBlockNameList(ValveBlockNames);
            if (valveObjectIds != null)
            {
                // 添加阀门数据
                foreach (var objectId in valveObjectIds)
                {
                    var root = new JObject();
                    root["data_class"] = "valve";
                    root["entityhandle"] = objectId.Handle.ToString();

                    // 获取块属性
                    var attrs = UtilsBlock.UtilsGetAllPropertyDictList(objectId);
                    foreach (var attr in attrs)
                    {
                        root[attr.Key.ToLower()] = attr.Value;
                    }
                    
                    root["blockname"] = UtilsBlock.UtilsGetBlockName(objectId);
                    valveJArray.Add(root);
                }
            }

            // 设置目标路径和数组
            TargetPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DataFlow", "ValveData.json");
            TargetJArray = valveJArray;
            ExportMessage = "阀门管件数据导出成功";
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    /// <summary>
    /// 命令中继类
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="execute">执行方法</param>
        /// <param name="canExecute">能否执行方法</param>
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 能否执行改变事件
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { System.Windows.Input.CommandManager.RequerySuggested += value; }
            remove { System.Windows.Input.CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// 能否执行
        /// </summary>
        /// <param name="parameter">参数</param>
        /// <returns>能否执行</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="parameter">参数</param>
        public void Execute(object parameter)
        {
            _execute();
        }
    }
} 