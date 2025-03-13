using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using dataflow_cs.Core.Models;
using dataflow_cs.Core.Services;
using dataflow_cs.Utils.CADUtils;
using dataflow_cs.Utils.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace dataflow_cs.Business.PipeFlow.Commands
{
    /// <summary>
    /// 批量同步管道数据命令
    /// </summary>
    public class BatchSyncPipeDataCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DPS";

        /// <summary>
        /// 已处理的多段线ID列表
        /// </summary>
        protected List<ObjectId> _processedPolylineIds = new List<ObjectId>();
        
        /// <summary>
        /// 已处理的管道弯头对象ID列表
        /// </summary>
        protected List<ObjectId> _processedPipeElbowObjectIds = new List<ObjectId>();

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            try
            {
                // 清空已处理列表
                _processedPolylineIds.Clear();
                _processedPipeElbowObjectIds.Clear();

                // 提示用户选择绿色箭头辅助管道块
                editor.WriteMessage("\n请选择绿色箭头辅助管道块");
                List<ObjectId> pipeArrowAssistBlockIds = GetPipeArrowAssistBlocks();
                if (pipeArrowAssistBlockIds.Count == 0)
                {
                    ErrorHandler.ShowWarning("未找到绿色箭头辅助管道块，操作取消。");
                    return false;
                }

                // 获取所有相关的块和多段线
                Dictionary<string, List<ObjectId>> allObjectsGroups = GetAllNeededObjects();
                if (allObjectsGroups == null)
                {
                    return false;
                }
                
                List<ObjectId> allGeYuanDrawObjectIds = allObjectsGroups["GeYuanFrame"];
                List<ObjectId> allPipeElbowObjectIds = allObjectsGroups["GsPgPipeElementElbow"];
                List<ObjectId> allPipeArrowAssistObjectIds = allObjectsGroups["GsPgPipeElementArrowAssist"];
                List<ObjectId> allValveObjectIds = allObjectsGroups["GsPgValve"];
                List<ObjectId> allPolylineObjectIds = allObjectsGroups["PipeLines"];

                // 获取项目编号和管道信息
                string projectNum = GetProjectNumber(allGeYuanDrawObjectIds);
                if (string.IsNullOrEmpty(projectNum))
                {
                    ErrorHandler.ShowWarning("无法获取项目编号，操作取消。");
                    return false;
                }

                // 这里应该调用管道信息助手获取管道信息
                // PipeInfoHelper pipeInfo = UtilsCommnon.UtilsGetPipeInfo(projectNum);
                // 由于我们重构过程中还没有完全实现这个功能，先使用原始的调用
                object pipeInfo = GsPgDataFlow.ToolManager.CsTest(); // 这只是一个临时的解决方案，实际应该使用重构后的方法

                // 处理每一个管道箭头块
                int successCount = 0;
                foreach (ObjectId pipeArrowAssistId in pipeArrowAssistBlockIds)
                {
                    try
                    {
                        // 获取与该箭头相关的管道线
                        List<ObjectId> pipeLineIds = GetPipeLinesByElement(pipeArrowAssistId, allPolylineObjectIds);
                        if (pipeLineIds.Count == 0)
                        {
                            continue;
                        }

                        // 获取管道数据
                        Dictionary<string, string> pipeData = GetPipeData(pipeArrowAssistId);
                        if (pipeData.Count == 0 || !pipeData.ContainsKey("pipeNum"))
                        {
                            continue;
                        }

                        // 同步管道数据到相关元素
                        // 这里我们先使用原来的方法调用，后续可以替换为重构后的方法
                        GsPgDataFlow.ToolManager.GsPgSynPipeElementForOnePipeAssist(
                            pipeData, 
                            pipeLineIds, 
                            allPolylineObjectIds, 
                            allPipeElbowObjectIds, 
                            allPipeArrowAssistObjectIds, 
                            allValveObjectIds, 
                            pipeInfo
                        );

                        // 同步管道弯头状态
                        GsPgDataFlow.ToolManager.GsPgSynPipeElbowStatus(pipeData, pipeInfo);

                        editor.WriteMessage($"\n{pipeData["pipeNum"]}数据已同步...");
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Instance.LogException(ex, $"处理管道块 {pipeArrowAssistId} 时出错");
                    }
                }

                // 清空处理列表
                _processedPipeElbowObjectIds.Clear();
                _processedPolylineIds.Clear();

                editor.WriteMessage($"\n成功同步了 {successCount} 个管道的数据。");
                return successCount > 0;
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "执行批量管道数据同步过程中发生错误");
                return false;
            }
        }

        /// <summary>
        /// 获取绿色箭头辅助管道块
        /// </summary>
        protected virtual List<ObjectId> GetPipeArrowAssistBlocks()
        {
            try
            {
                return BlockUtils.GetAllBlocksByName("GsPgPipeElementArrowAssist", false);
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "获取绿色箭头辅助管道块失败");
                return new List<ObjectId>();
            }
        }

        /// <summary>
        /// 获取所有需要的对象
        /// </summary>
        protected virtual Dictionary<string, List<ObjectId>> GetAllNeededObjects()
        {
            try
            {
                Dictionary<string, List<ObjectId>> result = new Dictionary<string, List<ObjectId>>();

                // 获取所有块
                List<ObjectId> allBlockIds = BlockUtils.GetAllBlocks();
                
                // 获取所有指定图层名的多段线
                // 这里暂时使用原有的方法调用，后续应该改为重构后的方法
                List<ObjectId> allPolylineObjectIds = new List<ObjectId>();
                ActiveDocumentService.ExecuteInTransaction((tr, db) =>
                {
                    allPolylineObjectIds = GsPgDataFlow.ToolManager.CsTest(); // 临时方案
                });

                // 按块名称分组
                List<string> blockNameList = new List<string> { "GeYuanFrame", "GsPgPipeElementElbow", "GsPgPipeElementArrowAssist", "GsPgValve" };
                foreach (string blockName in blockNameList)
                {
                    result[blockName] = BlockUtils.GetAllBlocksByName(blockName, false);
                }

                result["PipeLines"] = allPolylineObjectIds;
                
                return result;
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "获取所需对象失败");
                return null;
            }
        }

        /// <summary>
        /// 获取项目编号
        /// </summary>
        protected virtual string GetProjectNumber(List<ObjectId> geYuanFrameObjectIds)
        {
            try
            {
                // 获取项目编号的逻辑暂时使用原有的方法
                return GsPgDataFlow.ToolManager.GsPgGetProjectNum(geYuanFrameObjectIds);
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "获取项目编号失败");
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取管道数据
        /// </summary>
        protected virtual Dictionary<string, string> GetPipeData(ObjectId pipeArrowAssistId)
        {
            try
            {
                return GsPgDataFlow.ToolManager.GsPgGetPipeData(pipeArrowAssistId);
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "获取管道数据失败");
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// 根据元素获取相关的管道线
        /// </summary>
        protected virtual List<ObjectId> GetPipeLinesByElement(ObjectId elementObjectId, List<ObjectId> allPolylineObjectIds)
        {
            try
            {
                return GsPgDataFlow.ToolManager.GsPgGetPipeLinesByOnPL(elementObjectId, allPolylineObjectIds);
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "获取元素相关的管道线失败");
                return new List<ObjectId>();
            }
        }
    }
} 