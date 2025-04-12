using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using dataflow_cs.Core.Interfaces;
using dataflow_cs.Core.Services;
using dataflow_cs.Utils.Helpers;
using System.Collections.Generic;
using dataflow_cs.Business.Commands.GsPg;
using dataflow_cs.Business.Commands.Common;
using dataflow_cs.Business.Commands.GsLc;

[assembly: CommandClass(typeof(dataflow_cs.CommandManager))]

namespace dataflow_cs
{
    /// <summary>
    /// 命令管理器，用于注册和管理所有AutoCAD命令
    /// </summary>
    public class CommandManager
    {
        private static Dictionary<string, ICommandHandler> _commandHandlers = new Dictionary<string, ICommandHandler>();
        
        /// <summary>
        /// 静态构造函数，注册所有命令处理器
        /// </summary>
        static CommandManager()
        {
            try
            {
                // 注册命令处理器
                RegisterCommandHandler(new LocateByHandleCommand());
                RegisterCommandHandler(new AddCustomMenuCommand());
                RegisterCommandHandler(new TestTemplateWindowCommand());
                RegisterCommandHandler(new TestCommand());
                // 工艺二维配管
                RegisterCommandHandler(new GsPgBatchSyncPipeDataCommand());
                // 工艺流程图
                RegisterCommandHandler(new GsAddMenuCommand());
                RegisterCommandHandler(new GsLcSysDataFromClientCommand());
                RegisterCommandHandler(new GsLcInsertElementBlockCommand());
                RegisterCommandHandler(new GsLcInsertInstrumentPBlockCommand());
                RegisterCommandHandler(new GsLcInsertInstrumentLBlockCommand());
                RegisterCommandHandler(new GsLcInsertGlobalBlockCommand());

                // 将来可在此处添加更多命令
                
                LoggingService.Instance.LogInfo($"已成功注册 {_commandHandlers.Count} 个命令。");
            }
            catch (System.Exception ex)
            {
                LoggingService.Instance.LogException(ex, "注册命令处理器时发生错误");
            }
        }
        
        /// <summary>
        /// 注册命令处理器
        /// </summary>
        private static void RegisterCommandHandler(ICommandHandler handler)
        {
            if (handler != null && !string.IsNullOrEmpty(handler.CommandName))
            {
                _commandHandlers[handler.CommandName] = handler;
                LoggingService.Instance.LogInfo($"已注册命令: {handler.CommandName}");
            }
        }
        
        /// <summary>
        /// 获取命令处理器
        /// </summary>
        private static ICommandHandler GetCommandHandler(string commandName)
        {
            if (_commandHandlers.ContainsKey(commandName))
            {
                return _commandHandlers[commandName];
            }
            return null;
        }
        
        /// <summary>
        /// 执行命令
        /// </summary>
        private static void ExecuteCommand(string commandName)
        {
            try
            {
                ICommandHandler handler = GetCommandHandler(commandName);
                if (handler == null)
                {
                    ErrorHandler.HandleError($"未找到命令 {commandName} 的处理器");
                    return;
                }
                
                Document doc = ActiveDocumentService.GetActiveDocument();
                if (doc == null)
                {
                    ErrorHandler.HandleError("无法获取活动文档");
                    return;
                }
                
                using (doc.LockDocument())
                {
                    handler.Execute(doc.Editor, doc.Database);
                }
            }
            catch (System.Exception ex)
            {
                ErrorHandler.HandleException(ex, $"执行命令 {commandName} 时发生错误");
            }
        }
        
        #region 命令定义
        
        /// <summary>
        /// 测试命令
        /// </summary>
        [CommandMethod("CsTest")]
        public void CsTest()
        {
            ExecuteCommand("CsTest");
        }
        
        /// <summary>
        /// 定位实体对象位置
        /// </summary>
        [CommandMethod("DLLocateByHandle")]
        public void DLLocateByHandle()
        {
            ExecuteCommand("DLLocateByHandle");
        }
        
        /// <summary>
        /// 添加自定义菜单
        /// </summary>
        [CommandMethod("DLAddCustomMenu")]
        public void DLAddCustomMenu()
        {
            ExecuteCommand("DLAddCustomMenu");
        }
        
        //--------------------------------------------------------------------------------------//
        // 工艺二维配管
        //--------------------------------------------------------------------------------------//

        /// <summary>
        /// 批量同步管道数据命令
        /// </summary>
        [CommandMethod("DPS")]
        public void DPS()
        {
            ExecuteCommand("DPS");
        }
        


        //--------------------------------------------------------------------------------------//
        // 工艺流程图
        //--------------------------------------------------------------------------------------//

        /// <summary>
        /// 同步流程数据
        /// </summary>
        [CommandMethod("DLGsLcSysDataFromClient")]
        public void DLGsLcSysDataFromClient()
        {
            ExecuteCommand("DLGsLcSysDataFromClient");
        }

        [CommandMethod("DLTestTemplateWindow")]
        public void DLTestTemplateWindow()
        {
            ExecuteCommand("DLTestTemplateWindow");
        }

        /// <summary>
        /// 添加工艺数据流面板
        /// </summary>
        [CommandMethod("DLGsAddMenu")]
        public void DLGsAddMenu()
        {
            ExecuteCommand("DLGsAddMenu");
        }

        /// <summary>
        /// 动态插入工艺数据流组件块
        /// </summary>
        [CommandMethod("DLGsLcInsertElementBlock")]
        public void DLGsLcInsertElementBlock()
        {
            ExecuteCommand("DLGsLcInsertElementBlock");
        }

        /// <summary>
        /// 动态插入仪表P块
        /// </summary>
        [CommandMethod("DLGsLcInsertInstrumentPBlock")]
        public void DLGsLcInsertInstrumentPBlock()
        {
            ExecuteCommand("DLGsLcInsertInstrumentPBlock");
        }

        /// <summary>
        /// 动态插入仪表L块
        /// </summary>
        [CommandMethod("DLGsLcInsertInstrumentLBlock")]
        public void DLGsLcInsertInstrumentLBlock()
        {
            ExecuteCommand("DLGsLcInsertInstrumentLBlock");
        }

        /// <summary>
        /// 动态插入全局数据流块
        /// </summary>
        [CommandMethod("DLGsLcInsertGlobalBlock")]
        public void DLGsLcInsertGlobalBlock()
        {
            ExecuteCommand("DLGsLcInsertGlobalBlock");
        }

        // 在这里添加更多命令定义...
        
        #endregion
    }
} 