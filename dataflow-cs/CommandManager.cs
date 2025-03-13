using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using dataflow_cs.Business.PipeFlow.Commands;
using dataflow_cs.Core.Interfaces;
using dataflow_cs.Core.Services;
using dataflow_cs.Utils.Helpers;
using System;
using System.Collections.Generic;

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
                RegisterCommandHandler(new SyncPipeDataCommand());
                RegisterCommandHandler(new BatchSyncPipeDataCommand());
                RegisterCommandHandler(new TestCommand());
                RegisterCommandHandler(new FunctionTestCommand());
                
                // 将来可在此处添加更多命令
                
                LoggingService.Instance.LogInfo($"已成功注册 {_commandHandlers.Count} 个命令。");
            }
            catch (Exception ex)
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
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, $"执行命令 {commandName} 时发生错误");
            }
        }
        
        #region 命令定义
        
        /// <summary>
        /// 管道数据同步命令
        /// </summary>
        [CommandMethod("GsLcSynFromToLocationData")]
        public void GsLcSynFromToLocationData()
        {
            ExecuteCommand("GsLcSynFromToLocationData");
        }
        
        /// <summary>
        /// 批量同步管道数据命令
        /// </summary>
        [CommandMethod("DPS")]
        public void DPS()
        {
            ExecuteCommand("DPS");
        }
        
        /// <summary>
        /// 测试命令
        /// </summary>
        [CommandMethod("CsTest")]
        public void CsTest()
        {
            ExecuteCommand("CsTest");
        }
        
        /// <summary>
        /// 功能测试命令
        /// </summary>
        [CommandMethod("CsFuncitonTest")]
        public void CsFuncitonTest()
        {
            ExecuteCommand("CsFuncitonTest");
        }
        
        // 在这里添加更多命令定义...
        
        #endregion
    }
} 