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
                RegisterCommandHandler(new GsLcInsertAllElementBlockCommand());

                // 仪表类
                RegisterCommandHandler(new GsLcInsertInstrumentLBlockCommand());
                RegisterCommandHandler(new GsLcInsertInstrumentPBlockCommand());
                RegisterCommandHandler(new GsLcInsertInstrumentSISBlockCommand());
                
                // 阀门类
                RegisterCommandHandler(new GsLcInsertGlobalBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveBallTeeBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveGlobeBlockCommand());
                RegisterCommandHandler(new GsLcInsertValvePressureReduceBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveCheckBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveCheckSwingBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveCockBlockCommand());
                RegisterCommandHandler(new GsLcInsertValvePlungerBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveNeedleBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveGateBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveFlapperBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveDiaphragmBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveButterflyBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveSafetyBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveBlastBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveTrapBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveBreathBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveBreathFlameArrestBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveFlameArrestBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveMetalHoseBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveFilterYBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveFilterTBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveFilterConeBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveFilterBasketBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveFlangeCoverBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveRestrictOrificeSingleBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveGlassBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveBlindBoard8OffBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveBlindBoard8OnBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveBlindBoardOffBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveBlindBoardOnBlockCommand());
                RegisterCommandHandler(new GsLcInsertValvePipeClassChangeBlockCommand());
                RegisterCommandHandler(new GsLcInsertValveReducerBlockCommand());

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
        [CommandMethod("DLGsLcInsertAllElementBlock")]
        public void DLGsLcInsertElementBlock()
        {
            ExecuteCommand("DLGsLcInsertAllElementBlock");
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

        // 新增命令方法
        // 仪表类
        /// <summary>
        /// 动态插入SIS仪表块
        /// </summary>
        [CommandMethod("DLGsLcInsertInstrumentSISBlock")]
        public void DLGsLcInsertInstrumentSISBlock()
        {
            ExecuteCommand("DLGsLcInsertInstrumentSISBlock");
        }

        // 阀门类
        /// <summary>
        /// 动态插入三通球阀块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveBallTeeBlock")]
        public void DLGsLcInsertValveBallTeeBlock()
        {
            ExecuteCommand("DLGsLcInsertValveBallTeeBlock");
        }

        /// <summary>
        /// 动态插入截止阀块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveGlobeBlock")]
        public void DLGsLcInsertValveGlobeBlock()
        {
            ExecuteCommand("DLGsLcInsertValveGlobeBlock");
        }

        /// <summary>
        /// 动态插入减压阀块
        /// </summary>
        [CommandMethod("DLGsLcInsertValvePressureReduceBlock")]
        public void DLGsLcInsertValvePressureReduceBlock()
        {
            ExecuteCommand("DLGsLcInsertValvePressureReduceBlock");
        }

        /// <summary>
        /// 动态插入止回阀块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveCheckBlock")]
        public void DLGsLcInsertValveCheckBlock()
        {
            ExecuteCommand("DLGsLcInsertValveCheckBlock");
        }

        /// <summary>
        /// 动态插入旋启式止回阀块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveCheckSwingBlock")]
        public void DLGsLcInsertValveCheckSwingBlock()
        {
            ExecuteCommand("DLGsLcInsertValveCheckSwingBlock");
        }

        /// <summary>
        /// 动态插入旋塞阀块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveCockBlock")]
        public void DLGsLcInsertValveCockBlock()
        {
            ExecuteCommand("DLGsLcInsertValveCockBlock");
        }

        /// <summary>
        /// 动态插入柱塞阀块
        /// </summary>
        [CommandMethod("DLGsLcInsertValvePlungerBlock")]
        public void DLGsLcInsertValvePlungerBlock()
        {
            ExecuteCommand("DLGsLcInsertValvePlungerBlock");
        }

        /// <summary>
        /// 动态插入针型阀块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveNeedleBlock")]
        public void DLGsLcInsertValveNeedleBlock()
        {
            ExecuteCommand("DLGsLcInsertValveNeedleBlock");
        }

        /// <summary>
        /// 动态插入闸阀块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveGateBlock")]
        public void DLGsLcInsertValveGateBlock()
        {
            ExecuteCommand("DLGsLcInsertValveGateBlock");
        }

        /// <summary>
        /// 动态插入插板阀块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveFlapperBlock")]
        public void DLGsLcInsertValveFlapperBlock()
        {
            ExecuteCommand("DLGsLcInsertValveFlapperBlock");
        }

        /// <summary>
        /// 动态插入隔膜阀块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveDiaphragmBlock")]
        public void DLGsLcInsertValveDiaphragmBlock()
        {
            ExecuteCommand("DLGsLcInsertValveDiaphragmBlock");
        }

        /// <summary>
        /// 动态插入蝶阀块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveButterflyBlock")]
        public void DLGsLcInsertValveButterflyBlock()
        {
            ExecuteCommand("DLGsLcInsertValveButterflyBlock");
        }

        /// <summary>
        /// 动态插入安全阀块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveSafetyBlock")]
        public void DLGsLcInsertValveSafetyBlock()
        {
            ExecuteCommand("DLGsLcInsertValveSafetyBlock");
        }

        /// <summary>
        /// 动态插入爆破片块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveBlastBlock")]
        public void DLGsLcInsertValveBlastBlock()
        {
            ExecuteCommand("DLGsLcInsertValveBlastBlock");
        }

        /// <summary>
        /// 动态插入疏水阀块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveTrapBlock")]
        public void DLGsLcInsertValveTrapBlock()
        {
            ExecuteCommand("DLGsLcInsertValveTrapBlock");
        }

        /// <summary>
        /// 动态插入呼吸阀块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveBreathBlock")]
        public void DLGsLcInsertValveBreathBlock()
        {
            ExecuteCommand("DLGsLcInsertValveBreathBlock");
        }

        /// <summary>
        /// 动态插入带阻火器呼吸阀块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveBreathFlameArrestBlock")]
        public void DLGsLcInsertValveBreathFlameArrestBlock()
        {
            ExecuteCommand("DLGsLcInsertValveBreathFlameArrestBlock");
        }

        /// <summary>
        /// 动态插入阻火器块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveFlameArrestBlock")]
        public void DLGsLcInsertValveFlameArrestBlock()
        {
            ExecuteCommand("DLGsLcInsertValveFlameArrestBlock");
        }

        /// <summary>
        /// 动态插入金属软件块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveMetalHoseBlock")]
        public void DLGsLcInsertValveMetalHoseBlock()
        {
            ExecuteCommand("DLGsLcInsertValveMetalHoseBlock");
        }

        /// <summary>
        /// 动态插入Y型过滤器块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveFilterYBlock")]
        public void DLGsLcInsertValveFilterYBlock()
        {
            ExecuteCommand("DLGsLcInsertValveFilterYBlock");
        }

        /// <summary>
        /// 动态插入T型过滤器块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveFilterTBlock")]
        public void DLGsLcInsertValveFilterTBlock()
        {
            ExecuteCommand("DLGsLcInsertValveFilterTBlock");
        }

        /// <summary>
        /// 动态插入锥型过滤器块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveFilterConeBlock")]
        public void DLGsLcInsertValveFilterConeBlock()
        {
            ExecuteCommand("DLGsLcInsertValveFilterConeBlock");
        }

        /// <summary>
        /// 动态插入罐式（篮式）型过滤器块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveFilterBasketBlock")]
        public void DLGsLcInsertValveFilterBasketBlock()
        {
            ExecuteCommand("DLGsLcInsertValveFilterBasketBlock");
        }

        /// <summary>
        /// 动态插入法兰盖块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveFlangeCoverBlock")]
        public void DLGsLcInsertValveFlangeCoverBlock()
        {
            ExecuteCommand("DLGsLcInsertValveFlangeCoverBlock");
        }

        /// <summary>
        /// 动态插入单板限流孔板块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveRestrictOrificeSingleBlock")]
        public void DLGsLcInsertValveRestrictOrificeSingleBlock()
        {
            ExecuteCommand("DLGsLcInsertValveRestrictOrificeSingleBlock");
        }

        /// <summary>
        /// 动态插入视镜块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveGlassBlock")]
        public void DLGsLcInsertValveGlassBlock()
        {
            ExecuteCommand("DLGsLcInsertValveGlassBlock");
        }

        /// <summary>
        /// 动态插入8字盲板（正常关闭）块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveBlindBoard8OffBlock")]
        public void DLGsLcInsertValveBlindBoard8OffBlock()
        {
            ExecuteCommand("DLGsLcInsertValveBlindBoard8OffBlock");
        }

        /// <summary>
        /// 动态插入8字盲板（正常开启）块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveBlindBoard8OnBlock")]
        public void DLGsLcInsertValveBlindBoard8OnBlock()
        {
            ExecuteCommand("DLGsLcInsertValveBlindBoard8OnBlock");
        }

        /// <summary>
        /// 动态插入圆形盲板（正常关闭）块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveBlindBoardOffBlock")]
        public void DLGsLcInsertValveBlindBoardOffBlock()
        {
            ExecuteCommand("DLGsLcInsertValveBlindBoardOffBlock");
        }

        /// <summary>
        /// 动态插入圆形盲板（正常开启）块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveBlindBoardOnBlock")]
        public void DLGsLcInsertValveBlindBoardOnBlock()
        {
            ExecuteCommand("DLGsLcInsertValveBlindBoardOnBlock");
        }

        /// <summary>
        /// 动态插入变管道等级块
        /// </summary>
        [CommandMethod("DLGsLcInsertValvePipeClassChangeBlock")]
        public void DLGsLcInsertValvePipeClassChangeBlock()
        {
            ExecuteCommand("DLGsLcInsertValvePipeClassChangeBlock");
        }

        /// <summary>
        /// 动态插入同心异径管块
        /// </summary>
        [CommandMethod("DLGsLcInsertValveReducerBlock")]
        public void DLGsLcInsertValveReducerBlock()
        {
            ExecuteCommand("DLGsLcInsertValveReducerBlock");
        }

        // 在这里添加更多命令定义...
        
        #endregion
    }
} 