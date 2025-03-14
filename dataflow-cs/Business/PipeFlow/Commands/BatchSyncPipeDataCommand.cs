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
                 //PipeInfoHelper pipeInfo = CommnonUtils.UtilsGetPipeInfo(projectNum);
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
                        SyncPipeElementForOnePipeAssist(
                            pipeData, 
                            pipeLineIds, 
                            allPolylineObjectIds, 
                            allPipeElbowObjectIds, 
                            allPipeArrowAssistObjectIds, 
                            allValveObjectIds, 
                            pipeInfo
                        );

                        // 同步管道弯头状态
                        SyncPipeElbowStatus(pipeData, pipeInfo);

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
                return BlockUtils.GetAllObjectIdsByBlockName("GsPgPipeElementArrowAssist", false);
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
                List<ObjectId> allBlockIds = BlockUtils.GetAllBlockObjectIds();
                
                // 获取所有指定图层名的多段线
                List<ObjectId> allPolylineObjectIds = PolylineUtils.GetAllObjectIdsByLayerName("0DataFlow-GsPgPipeLine*");

                // 按块名称分组
                List<string> blockNameList = new List<string> { "GeYuanFrame", "GsPgPipeElementElbow", "GsPgPipeElementArrowAssist", "GsPgValve" };
                foreach (string blockName in blockNameList)
                {
                    result[blockName] = BlockUtils.GetAllObjectIdsByBlockName(blockName, false);
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
                Dictionary<string, string> propertyValueDict = BlockUtils.UtilsGetAllPropertyDictList(pipeArrowAssistId);
                return new Dictionary<string, string>
                {
                    { "pipeNum", propertyValueDict.ContainsKey("PIPENUM") ? propertyValueDict["PIPENUM"] : string.Empty },
                    { "pipeElevation", propertyValueDict.ContainsKey("ELEVATION") ? propertyValueDict["ELEVATION"] : string.Empty }
                };
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
                Point3d basePoint = BlockUtils.UtilsGetBlockBasePoint(elementObjectId);
                return allPolylineObjectIds.Where(x => IsPipeElementOnPipeLine(basePoint, x)).ToList();
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "获取元素相关的管道线失败");
                return new List<ObjectId>();
            }
        }

        /// <summary>
        /// 判断管道元素是否在管道线上
        /// </summary>
        protected virtual bool IsPipeElementOnPipeLine(Point3d basePoint, ObjectId pipeLineObjectId)
        {
            return GeometricUtils.IsPointOnPolyline(basePoint, pipeLineObjectId, 5);
        }

        /// <summary>
        /// 判断管道元素是否在管道线端点上
        /// </summary>
        protected virtual bool IsPipeElementOnPipeLineEnds(Point3d basePoint, ObjectId pipeLineObjectId)
        {
            return GeometricUtils.IsPointOnPolylineEnds(basePoint, pipeLineObjectId, 5);
        }

        /// <summary>
        /// 同步管道数据到相关元素
        /// </summary>
        protected virtual void SyncPipeElementForOnePipeAssist(
            Dictionary<string, string> pipeData, 
            List<ObjectId> pipeLineObjectIds, 
            List<ObjectId> allPipeLineObjectIds, 
            List<ObjectId> allElbowObjectIds, 
            List<ObjectId> allPipeArrowAssistObjectIds, 
            List<ObjectId> allValveObjectIds, 
            object pipeInfo)
        {
            if (pipeLineObjectIds == null || pipeLineObjectIds.Count == 0)
            {
                return;
            }

            foreach (ObjectId pipeLineId in pipeLineObjectIds)
            {
                // 添加扩展数据
                EntityUtils.AddXData(pipeLineId, pipeData);
                
                // 更新管道箭头辅助块属性
                ChangePipeArrowAssistPropertyValue(pipeLineId, allPipeArrowAssistObjectIds, pipeData);
                
                // 更新阀门属性
                ChangeValvePropertyValue(pipeLineId, allValveObjectIds, pipeData, pipeInfo);
                
                // 移除当前管道线，避免重复处理
                List<ObjectId> remainingPipeLines = allPipeLineObjectIds.Where(x => !x.Equals(pipeLineId)).ToList();
                
                // 获取与当前管道线相交的其他管道线
                List<ObjectId> otherPipeLineIds = GetOtherPipeLineByIntersect(pipeLineId, remainingPipeLines, allElbowObjectIds, pipeData);
                
                // 递归处理相交的管道线
                if (otherPipeLineIds != null && otherPipeLineIds.Count > 0)
                {
                    SyncPipeElementForOnePipeAssist(pipeData, otherPipeLineIds, remainingPipeLines, allElbowObjectIds, allPipeArrowAssistObjectIds, allValveObjectIds, pipeInfo);
                }
                
                // 记录已处理的管道线
                _processedPolylineIds.Add(pipeLineId);
            }
        }

        /// <summary>
        /// 获取与当前管道线相交的其他管道线
        /// </summary>
        protected virtual List<ObjectId> GetOtherPipeLineByIntersect(
            ObjectId pipeLineObjectId, 
            List<ObjectId> pipeLineObjectIds, 
            List<ObjectId> allElbowObjectIds, 
            Dictionary<string, string> pipeData)
        {
            List<ObjectId> otherPipeLineObjectIds = new List<ObjectId>();
            
            // 找到所有在当前管道线端点上的弯头
            var elbowsOnPipeLineEnds = allElbowObjectIds
                .Where(x => IsPipeElementOnPipeLineEnds(BlockUtils.GetBlockBasePoint(x), pipeLineObjectId))
                .ToList();
                
            foreach (ObjectId elbowId in elbowsOnPipeLineEnds)
            {
                // 更新弯头属性
                BlockUtils.SetPropertyValueByDictData(elbowId, pipeData);
                
                // 记录已处理的弯头
                _processedPipeElbowObjectIds.Add(elbowId);
                
                // 获取与弯头相连的其他管道线
                Point3d elbowBasePoint = BlockUtils.GetBlockBasePoint(elbowId);
                ObjectId otherPipeLineId = pipeLineObjectIds
                    .FirstOrDefault(x => IsPipeElementOnPipeLineEnds(elbowBasePoint, x));
                    
                // 如果弯头不在终止图层上，则添加相连的管道线
                if (otherPipeLineId != ObjectId.Null && BlockUtils.GetBlockLayer(elbowId) != "0DataFlow-GsPgPipeLineDPSBreak")
                {
                    otherPipeLineObjectIds.Add(otherPipeLineId);
                }
            }
            
            return otherPipeLineObjectIds;
        }

        /// <summary>
        /// 更新管道箭头辅助块属性
        /// </summary>
        protected virtual void ChangePipeArrowAssistPropertyValue(
            ObjectId pipeLineObjectId, 
            List<ObjectId> allPipeArrowAssistObjectIds, 
            Dictionary<string, string> pipeData)
        {
            // 找到所有在当前管道线上的箭头辅助块
            var arrowAssistsOnPipeLine = allPipeArrowAssistObjectIds
                .Where(x => IsPipeElementOnPipeLine(BlockUtils.GetBlockBasePoint(x), pipeLineObjectId))
                .ToList();
                
            foreach (ObjectId arrowAssistId in arrowAssistsOnPipeLine)
            {
                // 更新箭头辅助块属性
                BlockUtils.SetPropertyValueByDictData(arrowAssistId, pipeData);
            }
        }

        /// <summary>
        /// 更新阀门属性
        /// </summary>
        protected virtual void ChangeValvePropertyValue(
            ObjectId pipeLineObjectId, 
            List<ObjectId> allValveObjectIds, 
            Dictionary<string, string> pipeData, 
            object pipeInfo)
        {
            // 找到所有在当前管道线上的阀门
            var valvesOnPipeLine = allValveObjectIds
                .Where(x => IsPipeElementOnPipeLine(BlockUtils.GetBlockBasePoint(x), pipeLineObjectId))
                .ToList();
                
            foreach (ObjectId valveId in valvesOnPipeLine)
            {
                // 更新阀门属性
                BlockUtils.SetPropertyValueByDictData(valveId, pipeData);
                
                // 获取管道直径
                string pipeDiameter = GetPipeDiameter(pipeData["pipeNum"], pipeInfo);
                
                // 更新阀门动态属性
                if (!string.IsNullOrEmpty(pipeDiameter))
                {
                    Dictionary<string, string> dynamicProps = new Dictionary<string, string>
                    {
                        { "sideview-DN", pipeDiameter },
                        { "topview-DN", pipeDiameter }
                    };
                    BlockUtils.SetDynamicPropertyValueByDictData(valveId, dynamicProps);
                }
            }
        }

        /// <summary>
        /// 获取管道直径
        /// </summary>
        protected virtual string GetPipeDiameter(string pipeNum, object pipeInfo)
        {
            try
            {
                // 暂时使用原有方法
                return GsPgDataFlow.ToolManager.GsPgGetPipeDiameter(pipeNum, pipeInfo);
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "获取管道直径失败");
                return string.Empty;
            }
        }

        /// <summary>
        /// 同步管道弯头状态
        /// </summary>
        protected virtual void SyncPipeElbowStatus(Dictionary<string, string> pipeData, object pipeInfo)
        {
            try
            {
                // 获取管道直径
                string pipeDiameter = GetPipeDiameter(pipeData["pipeNum"], pipeInfo);
                
                // 去重处理
                _processedPipeElbowObjectIds = _processedPipeElbowObjectIds.Distinct().ToList();
                _processedPolylineIds = _processedPolylineIds.Distinct().ToList();
                
                foreach (ObjectId elbowId in _processedPipeElbowObjectIds)
                {
                    // 重置弯头块的XY比例，避免镜像问题
                    BlockUtils.SetBlockXYScale(elbowId, 1, 1);
                    
                    // 获取与弯头相连的管道线
                    Point3d elbowBasePoint = BlockUtils.GetBlockBasePoint(elbowId);
                    List<ObjectId> connectedPipeLines = _processedPolylineIds
                        .Where(x => IsPipeElementOnPipeLineEnds(elbowBasePoint, x))
                        .ToList();
                        
                    List<ObjectId> teePipeLines = _processedPolylineIds
                        .Where(x => IsPipeElementOnPipeLine(elbowBasePoint, x))
                        .ToList();
                        
                    // 处理弯头
                    if (connectedPipeLines.Count == 2)
                    {
                        ProcessElbowWithTwoPipeLines(elbowId, connectedPipeLines, pipeDiameter);
                    }
                    // 处理三通
                    else if (teePipeLines.Count == 2)
                    {
                        ProcessTeeWithTwoPipeLines(elbowId, teePipeLines);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "同步管道弯头状态失败");
            }
        }

        /// <summary>
        /// 处理连接两条管道线的弯头
        /// </summary>
        protected virtual void ProcessElbowWithTwoPipeLines(ObjectId elbowId, List<ObjectId> pipeLineIds, string pipeDiameter)
        {
            try
            {
                // 获取管道高程和交点
                var (firstPipeElevation, secondPipeElevation, firstIntersectionPoints, secondIntersectionPoints) = 
                    GetPipeElevationAndIntersectionPoints(elbowId, pipeLineIds);
                    
                if (firstIntersectionPoints != null && firstIntersectionPoints.Count > 0 && 
                    secondIntersectionPoints != null && secondIntersectionPoints.Count > 0)
                {
                    Point3d basePoint = BlockUtils.GetBlockBasePoint(elbowId);
                    Point3d firstIntersectionPoint = firstIntersectionPoints[0];
                    Point3d secondIntersectionPoint = secondIntersectionPoints[0];
                    
                    // 计算交点角度
                    double intersectionAngle = GeometricUtils.GetAngleByThreePoints(
                        basePoint, firstIntersectionPoint, secondIntersectionPoint);
                        
                    // 根据高程差异处理弯头
                    if (firstPipeElevation == secondPipeElevation)
                    {
                        // 同一高程，根据角度处理
                        HandleElbowWithDifferentAngles(elbowId, pipeDiameter, pipeLineIds, intersectionAngle);
                    }
                    else
                    {
                        // 不同高程，处理立管弯头
                        HandleElbowWithDifferentElevation(elbowId, firstPipeElevation, secondPipeElevation, pipeLineIds);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, $"处理弯头 {elbowId} 失败");
            }
        }

        /// <summary>
        /// 处理连接两条管道线的三通
        /// </summary>
        protected virtual void ProcessTeeWithTwoPipeLines(ObjectId teeId, List<ObjectId> pipeLineIds)
        {
            try
            {
                // 获取管道高程和交点
                var (firstPipeElevation, secondPipeElevation, firstIntersectionPoints, secondIntersectionPoints) = 
                    GetPipeElevationAndIntersectionPoints(teeId, pipeLineIds);
                    
                if (firstIntersectionPoints != null && secondIntersectionPoints != null)
                {
                    // 检查是否为三通（交点总数为3）
                    if (firstIntersectionPoints.Count + secondIntersectionPoints.Count == 3)
                    {
                        // 处理三通
                        HandleTeeWithDifferentElevation(
                            teeId, firstPipeElevation, secondPipeElevation, pipeLineIds, firstIntersectionPoints.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, $"处理三通 {teeId} 失败");
            }
        }

        /// <summary>
        /// 获取管道高程和交点
        /// </summary>
        protected virtual (double, double, List<Point3d>, List<Point3d>) GetPipeElevationAndIntersectionPoints(
            ObjectId elbowObjectId, List<ObjectId> pipeLineObjectIds)
        {
            try
            {
                // 获取管道高程
                double firstPipeElevation = CommonUtils.UtilsStringToDouble(
                    EntityUtils.GetXData(pipeLineObjectIds[0], "pipeElevation"));
                    
                double secondPipeElevation = CommonUtils.UtilsStringToDouble(
                    EntityUtils.GetXData(pipeLineObjectIds[1], "pipeElevation"));
                    
                // 获取交点
                List<Point3d> firstIntersectionPoints = GeometricUtils.GetIntersectionPointsByBlockAndPolyline(
                    elbowObjectId, pipeLineObjectIds[0]);
                    
                List<Point3d> secondIntersectionPoints = GeometricUtils.GetIntersectionPointsByBlockAndPolyline(
                    elbowObjectId, pipeLineObjectIds[1]);
                    
                return (firstPipeElevation, secondPipeElevation, firstIntersectionPoints, secondIntersectionPoints);
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "获取管道高程和交点失败");
                return (0, 0, null, null);
            }
        }

        /// <summary>
        /// 处理不同角度的弯头
        /// </summary>
        protected virtual void HandleElbowWithDifferentAngles(
            ObjectId elbowObjectId, string pipeDiameter, List<ObjectId> pipeLineObjectIds, double intersectionAngle)
        {
            try
            {
                // 暂时使用原有方法
                GsPgDataFlow.ToolManager.HandleElbowWithDifferentAngles(
                    elbowObjectId, pipeDiameter, pipeLineObjectIds, intersectionAngle);
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "处理不同角度的弯头失败");
            }
        }

        /// <summary>
        /// 处理不同高程的弯头
        /// </summary>
        protected virtual void HandleElbowWithDifferentElevation(
            ObjectId elbowObjectId, double firstPipeElevation, double secondPipeElevation, List<ObjectId> pipeLineObjectIds)
        {
            try
            {
                // 暂时使用原有方法
                GsPgDataFlow.ToolManager.HandleElbowWithDifferentElevation(
                    elbowObjectId, firstPipeElevation, secondPipeElevation, pipeLineObjectIds);
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "处理不同高程的弯头失败");
            }
        }

        /// <summary>
        /// 处理不同高程的三通
        /// </summary>
        protected virtual void HandleTeeWithDifferentElevation(
            ObjectId elbowObjectId, double firstPipeElevation, double secondPipeElevation, 
            List<ObjectId> pipeLineObjectIds, int firstIntersectionPointsNum)
        {
            try
            {
                // 暂时使用原有方法
                GsPgDataFlow.ToolManager.HandleTeeWithDifferentElevation(
                    elbowObjectId, firstPipeElevation, secondPipeElevation, pipeLineObjectIds, firstIntersectionPointsNum);
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "处理不同高程的三通失败");
            }
        }
    }
} 