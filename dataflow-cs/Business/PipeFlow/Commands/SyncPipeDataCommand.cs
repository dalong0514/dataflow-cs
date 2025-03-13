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
    /// 管道数据同步命令
    /// </summary>
    public class SyncPipeDataCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "GsLcSynFromToLocationData";

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
                // 获取所有管道线
                List<ObjectId> pipeLineIds = GetAllPipeLines(editor);
                if (pipeLineIds.Count == 0)
                {
                    ErrorHandler.ShowWarning("未找到管道线，操作取消。");
                    return false;
                }

                // 获取所有管道编号块
                List<ObjectId> pipeNumberBlockIds = GetAllPipeNumberBlocks();
                if (pipeNumberBlockIds.Count == 0)
                {
                    ErrorHandler.ShowWarning("未找到管道编号块，操作取消。");
                    return false;
                }

                // 处理管道数据
                int successCount = 0;
                foreach (ObjectId pipeNumberId in pipeNumberBlockIds)
                {
                    // 获取管道数据
                    Dictionary<string, string> pipeData = GetPipeData(pipeNumberId);
                    if (pipeData.Count == 0)
                    {
                        continue;
                    }

                    // 获取管道编号所在的管道线
                    ObjectId pipeLineId = GetPipeLineBelongingToElement(pipeNumberId, pipeLineIds);
                    if (pipeLineId == ObjectId.Null)
                    {
                        LoggingService.Instance.LogWarning($"管道编号 {pipeData["pipeNum"]} 不在任何管道线上");
                        continue;
                    }

                    // 同步管道数据到管道线上的其他元素
                    if (SyncPipeDataToElements(pipeLineId, pipeData))
                    {
                        successCount++;
                    }
                }

                editor.WriteMessage($"\n成功同步了 {successCount} 个管道的数据。");
                return successCount > 0;
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "执行管道数据同步过程中发生错误");
                return false;
            }
        }

        /// <summary>
        /// 获取所有管道线
        /// </summary>
        protected virtual List<ObjectId> GetAllPipeLines(Editor editor)
        {
            // 提示用户选择管道线
            PromptSelectionOptions opts = new PromptSelectionOptions();
            opts.MessageForAdding = "\n请选择管道线(多段线):";
            opts.AllowDuplicates = false;

            // 创建过滤器，只选择多段线
            TypedValue[] tvs = new TypedValue[1];
            tvs[0] = new TypedValue((int)DxfCode.Start, "LWPOLYLINE");
            SelectionFilter filter = new SelectionFilter(tvs);

            PromptSelectionResult result = editor.GetSelection(opts, filter);
            if (result.Status != PromptStatus.OK)
            {
                return new List<ObjectId>();
            }

            return new List<ObjectId>(result.Value.GetObjectIds());
        }

        /// <summary>
        /// 获取所有管道编号块
        /// </summary>
        protected virtual List<ObjectId> GetAllPipeNumberBlocks()
        {
            // 获取名称包含"管道编号"或"PIPENUM"的块
            List<ObjectId> pipeNumBlocks = BlockUtils.GetAllBlocksByName("管道编号", false);
            List<ObjectId> pipeNumBlocks2 = BlockUtils.GetAllBlocksByName("PIPENUM", false);
            
            // 合并列表，去重
            HashSet<ObjectId> uniqueBlocks = new HashSet<ObjectId>();
            foreach (var id in pipeNumBlocks.Concat(pipeNumBlocks2))
            {
                uniqueBlocks.Add(id);
            }
            
            return uniqueBlocks.ToList();
        }

        /// <summary>
        /// 获取管道数据
        /// </summary>
        protected virtual Dictionary<string, string> GetPipeData(ObjectId pipeNumObjectId)
        {
            Dictionary<string, string> properties = BlockUtils.GetAllProperties(pipeNumObjectId);
            Dictionary<string, string> pipeData = new Dictionary<string, string>();
            
            if (properties.ContainsKey("PIPENUM"))
            {
                pipeData["pipeNum"] = properties["PIPENUM"];
            }
            
            if (properties.ContainsKey("ELEVATION"))
            {
                pipeData["pipeElevation"] = properties["ELEVATION"];
            }
            
            return pipeData;
        }

        /// <summary>
        /// 判断管道元素是否在管道线上
        /// </summary>
        protected virtual bool IsPipeElementOnPipeLine(Point3d basePoint, ObjectId pipeLineObjectId)
        {
            return GeometryUtils.IsPointOnPolyline(basePoint, pipeLineObjectId, 5);
        }

        /// <summary>
        /// 判断管道元素是否在管道线端点上
        /// </summary>
        protected virtual bool IsPipeElementOnPipeLineEnds(Point3d basePoint, ObjectId pipeLineObjectId)
        {
            return GeometryUtils.IsPointOnPolylineEnds(basePoint, pipeLineObjectId, 5);
        }

        /// <summary>
        /// 获取管道元素所属的管道线
        /// </summary>
        protected virtual ObjectId GetPipeLineBelongingToElement(ObjectId pipeElementObjectId, List<ObjectId> pipeLineObjectIds)
        {
            Point3d elementPosition = BlockUtils.GetBlockBasePoint(pipeElementObjectId);
            List<ObjectId> matchingLines = pipeLineObjectIds
                .Where(x => IsPipeElementOnPipeLine(elementPosition, x))
                .ToList();
                
            if (matchingLines.Count > 0)
            {
                return matchingLines[0];
            }
            
            return ObjectId.Null;
        }

        /// <summary>
        /// 同步管道数据到管道线上的其他元素
        /// </summary>
        protected virtual bool SyncPipeDataToElements(ObjectId pipeLineId, Dictionary<string, string> pipeData)
        {
            if (_processedPolylineIds.Contains(pipeLineId))
            {
                // 已经处理过此管道线
                return false;
            }
            
            _processedPolylineIds.Add(pipeLineId);
            
            try
            {
                // 获取管道线上的所有元素
                List<ObjectId> allPipeElementBlockIds = BlockUtils.GetAllBlocks();
                List<ObjectId> elementsOnPipeLine = new List<ObjectId>();
                
                foreach (ObjectId blockId in allPipeElementBlockIds)
                {
                    Point3d blockPos = BlockUtils.GetBlockBasePoint(blockId);
                    if (IsPipeElementOnPipeLine(blockPos, pipeLineId))
                    {
                        // 元素在此管道线上
                        elementsOnPipeLine.Add(blockId);
                    }
                }
                
                if (elementsOnPipeLine.Count == 0)
                {
                    return false;
                }
                
                // 更新管道线上的元素属性
                int updatedCount = 0;
                foreach (ObjectId elementId in elementsOnPipeLine)
                {
                    // 判断是否为管道配件
                    string blockName = BlockUtils.GetBlockName(elementId);
                    if (blockName.Contains("阀门") || blockName.Contains("接头") || blockName.Contains("弯头"))
                    {
                        // 是配件，同步配件的属性
                        if (BlockUtils.SetPropertyValues(elementId, pipeData))
                        {
                            updatedCount++;
                        }
                    }
                }
                
                return updatedCount > 0;
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, $"同步管道数据到元素失败，管道编号：{pipeData["pipeNum"]}");
                return false;
            }
        }
    }
} 