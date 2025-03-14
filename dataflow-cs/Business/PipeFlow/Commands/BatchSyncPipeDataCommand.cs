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
using System.Text.RegularExpressions;


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
        public static List<ObjectId> _processedPolylineIds = new List<ObjectId>();

        /// <summary>
        /// 已处理的管道弯头对象ID列表
        /// </summary>
        public static List<ObjectId> _processedPipeElbowObjectIds = new List<ObjectId>();

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            try
            {
                // 调用批量同步管道数据的方法
                GsPgBatchSynPipeData();
                return true;
            }
            catch (Exception ex)
            {
                editor.WriteMessage($"\n执行批量同步管道数据命令时发生错误: {ex.Message}");
                return false;
            }
        }

        public static bool IsPipeElementOnPipeLine(Point3d basePoint, ObjectId pipeLineObjectId)
        {
            return UtilsGeometry.UtilsIsPointOnPolyline(basePoint, pipeLineObjectId, 5);

        }

        public static bool IsPipeElementOnPipeLineEnds(Point3d basePoint, ObjectId pipeLineObjectId)
        {
            return UtilsGeometry.UtilsIsPointOnPolylineEnds(basePoint, pipeLineObjectId, 5);

        }

        public static Dictionary<string, string> GsPgGetPipeData(ObjectId pipeNumObjectId)
        {
            Dictionary<string, string> propertyValueDictList = UtilsBlock.UtilsGetAllPropertyDictList(pipeNumObjectId);
            // Not setting the Elevation attribute value can conveniently avoid synchronously modifying the auxiliary arrow block elevation on other pipelines
            return new Dictionary<string, string>
            {
                { "pipeNum", propertyValueDictList["PIPENUM"] },
                { "pipeElevation", propertyValueDictList["ELEVATION"] }
            };
        }

        public static ObjectId GsPgGetPipeLineByOnPL(ObjectId pipeElementObjectId, List<ObjectId> pipeLineObjectIds)
        {
            pipeLineObjectIds = pipeLineObjectIds.Where(x => IsPipeElementOnPipeLine(UtilsBlock.UtilsGetBlockBasePoint(pipeElementObjectId), x)).ToList();
            if (pipeLineObjectIds.Count != 0)
            {
                return pipeLineObjectIds[0];
            }
            return ObjectId.Null;
        }

        public static ObjectId GsPgGetPipeLineByOnPLEnd(ObjectId pipeElementObjectId, List<ObjectId> pipeLineObjectIds)
        {
            pipeLineObjectIds = pipeLineObjectIds.Where(x => IsPipeElementOnPipeLineEnds(UtilsBlock.UtilsGetBlockBasePoint(pipeElementObjectId), x)).ToList();
            if (pipeLineObjectIds.Count != 0)
            {
                return pipeLineObjectIds[0];
            }
            return ObjectId.Null;
        }

        public static List<ObjectId> GsPgGetPipeLinesByOnPL(ObjectId pipeElementObjectId, List<ObjectId> pipeLineObjectIds)
        {
            pipeLineObjectIds = pipeLineObjectIds.Where(x => IsPipeElementOnPipeLine(UtilsBlock.UtilsGetBlockBasePoint(pipeElementObjectId), x)).ToList();
            return pipeLineObjectIds;
        }

        public static List<ObjectId> GsPgGetPipeLinesByOnPLEnd(ObjectId pipeElementObjectId, List<ObjectId> pipeLineObjectIds)
        {
            pipeLineObjectIds = pipeLineObjectIds.Where(x => IsPipeElementOnPipeLineEnds(UtilsBlock.UtilsGetBlockBasePoint(pipeElementObjectId), x)).ToList();
            return pipeLineObjectIds;
        }

        public static List<ObjectId> GsPgGetOtherPipeLineByInterset(ObjectId pipeLineObjectId, List<ObjectId> pipeLineObjectIds, List<ObjectId> allElbowObjectIds, Dictionary<string, string> pipeData)
        {
            List<ObjectId> otherPipeLineObjectIds = new List<ObjectId>();
            allElbowObjectIds.Where(x => IsPipeElementOnPipeLineEnds(UtilsBlock.UtilsGetBlockBasePoint(x), pipeLineObjectId))
                .ToList()
                .ForEach(x =>
                {
                    UtilsBlock.UtilsSetPropertyValueByDictData(x, pipeData);
                    // The elbow that require synchronization have been incorporated into the global variables
                    _processedPipeElbowObjectIds.Add(x);
                    ObjectId result = GsPgGetPipeLineByOnPLEnd(x, pipeLineObjectIds);
                    // 2024-01-24 如果弯头块在终止图层 0DataFlow-GsPgPipeLineDPSBreak 上，则数据无法通过该弯头传递给其他管道
                    if (result != ObjectId.Null && UtilsBlock.UtilsGetBlockLayer(x) != "0DataFlow-GsPgPipeLineDPSBreak")
                    {
                        otherPipeLineObjectIds.Add(result);
                    }
                });
            return otherPipeLineObjectIds;
        }

        public static void GsPgChangePipeArrowAssistPropertyValue(ObjectId pipeLineObjectId, List<ObjectId> allPipeArrowAssistObjectIds, Dictionary<string, string> pipeData)
        {
            allPipeArrowAssistObjectIds.Where(x => IsPipeElementOnPipeLine(UtilsBlock.UtilsGetBlockBasePoint(x), pipeLineObjectId))
                .ToList()
                .ForEach(x =>
                {
                    UtilsBlock.UtilsSetPropertyValueByDictData(x, pipeData);
                    // key logic: the other pipelines elevation should be based on the current auxiliary arrow block elevation
                    UtilsCADActive.UtilsAddOneXData(pipeLineObjectId, "pipeElevation", UtilsBlock.UtilsGetPropertyValueByPropertyName(x, "elevation"));
                });
        }

        public static string GsPgGetPipeDiameter(string pipeNum, PipeInfoHelper pipeInfo)
        {
            string pipeDiameter = pipeInfo.GetPipeDiameter(pipeNum);
            //string pipeDiameter = pipeNum.Split('-').ElementAtOrDefault(1);

            if (pipeDiameter == null || !Regex.IsMatch(pipeDiameter, @"^\d+$"))
            {
                return string.Empty;
            }
            else
            {
                return pipeDiameter;
            }
        }

        public static string GsPgGetPipeElbowDiameter(string pipeDiameter, double calMultiple)
        {
            double result = UtilsCommon.UtilsStringToDouble(pipeDiameter) * calMultiple;
            return result.ToString();
        }

        public static void GsPgChangeValvePropertyValue(ObjectId pipeLineObjectId, List<ObjectId> allValveObjectIds, Dictionary<string, string> pipeData, PipeInfoHelper pipeInfo)
        {
            allValveObjectIds.Where(x => IsPipeElementOnPipeLine(UtilsBlock.UtilsGetBlockBasePoint(x), pipeLineObjectId))
                .ToList()
                .ForEach(x =>
                {

                    string pipeDiater = GsPgGetPipeDiameter(pipeData["pipeNum"], pipeInfo);
                    if (pipeDiater != string.Empty)
                    {
                        Dictionary<string, string> propertyDict = new Dictionary<string, string>()
                        {
                            { "sideview-DN", pipeDiater },
                            { "topview-DN", pipeDiater }
                        };
                        UtilsBlock.UtilsSetDynamicPropertyValueByDictData(x, propertyDict);
                    }

                });
        }

        public static void GsPgSynPipeElementForOnePipeAssist(Dictionary<string, string> pipeData, List<ObjectId> pipeLineObjectIds, List<ObjectId> allPipeLineObjectIds, List<ObjectId> allElbowObjectIds, List<ObjectId> allPipeArrowAssistObjectIds, List<ObjectId> allValveObjectIds, PipeInfoHelper pipeInfo)
        {
            if (pipeLineObjectIds != null)
            {
                pipeLineObjectIds.ForEach(x =>
                {
                    UtilsCADActive.UtilsAddXData(x, pipeData);
                    // for test
                    //UtilsPolyline.UtilsChangeColor(x, 1);
                    GsPgChangePipeArrowAssistPropertyValue(x, allPipeArrowAssistObjectIds, pipeData);
                    GsPgChangeValvePropertyValue(x, allValveObjectIds, pipeData, pipeInfo);
                    // the key logic: remove the current polyline
                    allPipeLineObjectIds = allPipeLineObjectIds.Where(xx => xx != x).ToList();
                    List<ObjectId> otherPipeLineObjectIds = GsPgGetOtherPipeLineByInterset(x, allPipeLineObjectIds, allElbowObjectIds, pipeData);

                    if (otherPipeLineObjectIds != null)
                    {
                        GsPgSynPipeElementForOnePipeAssist(pipeData, otherPipeLineObjectIds, allPipeLineObjectIds, allElbowObjectIds, allPipeArrowAssistObjectIds, allValveObjectIds, pipeInfo);
                    }
                    _processedPolylineIds.Add(x);
                });

            }
        }

        private static int GetObliqueRotationBasedOnPointPosition(double xDiff, double yDiff)
        {
            if (xDiff > 0 && yDiff > 0) return 45;
            if (xDiff < 0 && yDiff < 0) return 225;
            if (xDiff < 0 && yDiff > 0) return 90;
            return 275;
        }

        private static int GetRotationBasedOnPointPosition(double xDiff, double yDiff)
        {
            if (xDiff > 0 && yDiff > 0) return 0;
            if (xDiff < 0 && yDiff < 0) return 180;
            if (xDiff < 0 && yDiff > 0) return 90;
            return 270;
        }

        private static (Point3d, Point3d) GetCrossPoints(ObjectId elbowObjectId, ObjectId pipeLineObjectId1, ObjectId pipeLineObjectId2)
        {
            List<Point3d> firstPoint3ds = UtilsGeometry.UtilsGetIntersectionPointsByBlockAndPolyLine(elbowObjectId, pipeLineObjectId1);
            var firstCrossPoint = firstPoint3ds != null ? firstPoint3ds.FirstOrDefault() : Point3d.Origin;
            List<Point3d> secondPoint3ds = UtilsGeometry.UtilsGetIntersectionPointsByBlockAndPolyLine(elbowObjectId, pipeLineObjectId2);
            var secondCrossPoint = secondPoint3ds != null ? secondPoint3ds.FirstOrDefault() : Point3d.Origin;

            return (firstCrossPoint, secondCrossPoint);
        }

        private static (Point3d, Point3d) GetHorizontalAndVerticalPoints(Point3d basePoint, Point3d firstCrossPoint, Point3d secondCrossPoint)
        {
            return UtilsGeometry.UtilsIsLineHorizontal(basePoint, firstCrossPoint, 5.0)
                ? (firstCrossPoint, secondCrossPoint)
                : (secondCrossPoint, firstCrossPoint);
        }

        private static void SetRotationBasedOnElbowType(ObjectId elbowObjectId, string elbowType, double xDiff, double yDiff)
        {
            if (elbowType == "elbow90")
            {
                int rotation = GetRotationBasedOnPointPosition(xDiff, yDiff);
                UtilsBlock.UtilsSetBlockRotatonInDegrees(elbowObjectId, rotation);
            }
            else if (elbowType == "elbow45")
            {
                int rotation = GetObliqueRotationBasedOnPointPosition(xDiff, yDiff);
                UtilsBlock.UtilsSetBlockRotatonInDegrees(elbowObjectId, rotation);
            }
        }

        public static void GsPgSynElbowRotation(ObjectId elbowObjectId, ObjectId pipeLineObjectId1, ObjectId pipeLineObjectId2, string elbowType)
        {
            var basePoint = UtilsBlock.UtilsGetBlockBasePoint(elbowObjectId);
            var (firstCrossPoint, secondCrossPoint) = GetCrossPoints(elbowObjectId, pipeLineObjectId1, pipeLineObjectId2);

            if (firstCrossPoint != Point3d.Origin && secondCrossPoint != Point3d.Origin)
            {
                var (horizontalPoint, verticalPoint) = GetHorizontalAndVerticalPoints(basePoint, firstCrossPoint, secondCrossPoint);
                var xDiff = horizontalPoint.X - basePoint.X;
                var yDiff = verticalPoint.Y - basePoint.Y;

                SetRotationBasedOnElbowType(elbowObjectId, elbowType, xDiff, yDiff);
            }
        }

        private static (double, double, List<Point3d>, List<Point3d>) GetPipeElevationAndIntersectionPoints(ObjectId elbowObjectId, List<ObjectId> pipeLineObjectIds)
        {
            double firstPipeElevation = UtilsCommon.UtilsStringToDouble(UtilsCADActive.UtilsGetXData(pipeLineObjectIds[0], "pipeElevation"));
            double secondPipeElevation = UtilsCommon.UtilsStringToDouble(UtilsCADActive.UtilsGetXData(pipeLineObjectIds[1], "pipeElevation"));
            List<Point3d> firstIntersectionPoints = UtilsGeometry.UtilsGetIntersectionPointsByBlockAndPolyLine(elbowObjectId, pipeLineObjectIds[0]);
            List<Point3d> secondIntersectionPoints = UtilsGeometry.UtilsGetIntersectionPointsByBlockAndPolyLine(elbowObjectId, pipeLineObjectIds[1]);
            return (firstPipeElevation, secondPipeElevation, firstIntersectionPoints, secondIntersectionPoints);
        }

        private static void HandleElbowWithDifferentAngles(ObjectId elbowObjectId, string pipeDiater, List<ObjectId> pipeLineObjectIds, double intersectionAngle)
        {
            if (UtilsCommon.UtilsIsTwoNumEqual(intersectionAngle, 90, 2) || UtilsCommon.UtilsIsTwoNumEqual(intersectionAngle, 270, 2))
            {
                UtilsBlock.UtilsSetDynamicPropertyValueByDictData(elbowObjectId, new Dictionary<string, string>() { { "status", "elbow90" } });
                if (pipeDiater != string.Empty)
                {
                    UtilsBlock.UtilsSetDynamicPropertyValueByDictData(elbowObjectId, new Dictionary<string, string>() { { "radius90", GsPgGetPipeElbowDiameter(pipeDiater, 1.5) } }); 
                }
                GsPgSynElbowRotation(elbowObjectId, pipeLineObjectIds[0], pipeLineObjectIds[1], "elbow90");
            }
            else if (UtilsCommon.UtilsIsTwoNumEqual(intersectionAngle, 135, 2) || UtilsCommon.UtilsIsTwoNumEqual(intersectionAngle, 225, 2))
            {
                UtilsBlock.UtilsSetDynamicPropertyValueByDictData(elbowObjectId, new Dictionary<string, string>() { { "status", "elbow45" } });
                if (pipeDiater != string.Empty)
                {
                    UtilsBlock.UtilsSetDynamicPropertyValueByDictData(elbowObjectId, new Dictionary<string, string>() { { "radius90", GsPgGetPipeElbowDiameter(pipeDiater, 0.633) } });
                }
                GsPgSynElbowRotation(elbowObjectId, pipeLineObjectIds[0], pipeLineObjectIds[1], "elbow45");
            }
        }

        private static void HandleElbowWithDifferentElevation(ObjectId elbowObjectId, double firstPipeElevation, double secondPipeElevation, List<ObjectId> pipeLineObjectIds)
        {
            UtilsBlock.UtilsSetDynamicPropertyValueByDictData(elbowObjectId, new Dictionary<string, string>() { { "status", "elbowdown" } });
            if (firstPipeElevation > secondPipeElevation)
            {
                SetBlockRotationByIntersectionPoint(elbowObjectId, pipeLineObjectIds[0]);
            }
            else
            {
                SetBlockRotationByIntersectionPoint(elbowObjectId, pipeLineObjectIds[1]);
            }
        }

        private static void HandleTeeWithDifferentElevation(ObjectId elbowObjectId, double firstPipeElevation, double secondPipeElevation, List<ObjectId> pipeLineObjectIds, int firstIntersectionPointsNum)
        {
            if (firstPipeElevation > secondPipeElevation && firstIntersectionPointsNum == 2)
            {
                UtilsBlock.UtilsSetDynamicPropertyValueByDictData(elbowObjectId, new Dictionary<string, string>() { { "status", "teeup" } });
                SetBlockRotationByIntersectionPoint(elbowObjectId, pipeLineObjectIds[0]);
            }
            else if (firstPipeElevation < secondPipeElevation && firstIntersectionPointsNum == 2)
            {
                UtilsBlock.UtilsSetDynamicPropertyValueByDictData(elbowObjectId, new Dictionary<string, string>() { { "status", "teedown" } });
                SetBlockRotationByIntersectionPoint(elbowObjectId, pipeLineObjectIds[1]);
            }
            else if (firstPipeElevation < secondPipeElevation && firstIntersectionPointsNum == 1)
            {
                UtilsBlock.UtilsSetDynamicPropertyValueByDictData(elbowObjectId, new Dictionary<string, string>() { { "status", "teeup" } });
                SetBlockRotationByIntersectionPoint(elbowObjectId, pipeLineObjectIds[1]);
            }
            else if (firstPipeElevation > secondPipeElevation && firstIntersectionPointsNum == 1)
            {
                UtilsBlock.UtilsSetDynamicPropertyValueByDictData(elbowObjectId, new Dictionary<string, string>() { { "status", "teedown" } });
                SetBlockRotationByIntersectionPoint(elbowObjectId, pipeLineObjectIds[0]);
            }
        }

        private static void SetBlockRotationByIntersectionPoint(ObjectId elbowObjectId, ObjectId pipeLineObjectId)
        {
            List<Point3d> crossPoints = UtilsGeometry.UtilsGetIntersectionPointsByBlockAndPolyLine(elbowObjectId, pipeLineObjectId);
            if (crossPoints.Count > 0)
            {
                Point3d basePoint = UtilsBlock.UtilsGetBlockBasePoint(elbowObjectId);
                Point3d crossPoint = crossPoints[0];
                UtilsBlock.UtilsSetBlockRotatonInDegrees(elbowObjectId, UtilsGeometry.UtilsGetAngleByTwoPoint(basePoint, crossPoint));
            }
        }

        public static void GsPgSynPipeElbowStatus(Dictionary<string, string> pipeData, PipeInfoHelper pipeInfo)
        {
            string pipeDiater = GsPgGetPipeDiameter(pipeData["pipeNum"], pipeInfo);
            _processedPipeElbowObjectIds = _processedPipeElbowObjectIds.Distinct().ToList();
            _processedPolylineIds = _processedPolylineIds.Distinct().ToList();

            _processedPipeElbowObjectIds.ForEach(x =>
            {
                // 2024-01-24 设计人员可能会镜像弯头块，之前仅仅在HandleElbowWithDifferentAngles中重置XY比例，现在在这里整体重置
                UtilsBlock.UtilsSetBlockXYScale(x, 1, 1);
                List<ObjectId> pipeLineObjectIds = _processedPolylineIds.Where(xx => IsPipeElementOnPipeLineEnds(UtilsBlock.UtilsGetBlockBasePoint(x), xx)).ToList();
                List<ObjectId> teePipeLineObjectIds = _processedPolylineIds.Where(xx => IsPipeElementOnPipeLine(UtilsBlock.UtilsGetBlockBasePoint(x), xx)).ToList();
                if (pipeLineObjectIds.Count() == 2)
                {
                    var (firstPipeElevation, secondPipeElevation, firstIntersectionPoints, secondIntersectionPoints) = GetPipeElevationAndIntersectionPoints(x, pipeLineObjectIds);
                    if (firstIntersectionPoints != null && secondIntersectionPoints != null)
                    {
                        Point3d basePoint = UtilsBlock.UtilsGetBlockBasePoint(x);
                        Point3d firstIntersectionPoint = firstIntersectionPoints[0];
                        Point3d secondIntersectionPoint = secondIntersectionPoints[0];

                        double intersectionAngle = UtilsGeometry.UtilsGetAngleByThreePoint(basePoint, firstIntersectionPoint, secondIntersectionPoint);
                        if (firstPipeElevation == secondPipeElevation)
                        {
                            HandleElbowWithDifferentAngles(x, pipeDiater, pipeLineObjectIds, intersectionAngle);
                        }
                        else
                        {
                            HandleElbowWithDifferentElevation(x, firstPipeElevation, secondPipeElevation, pipeLineObjectIds);
                        }
                    }
                }
                else if (teePipeLineObjectIds.Count() == 2)
                {
                    var (firstPipeElevation, secondPipeElevation, firstIntersectionPoints, secondIntersectionPoints) = GetPipeElevationAndIntersectionPoints(x, teePipeLineObjectIds);
                    if (firstIntersectionPoints != null && secondIntersectionPoints != null)
                    {
                        Point3d basePoint = UtilsBlock.UtilsGetBlockBasePoint(x);
                        Point3d firstIntersectionPoint = firstIntersectionPoints[0];
                        Point3d secondIntersectionPoint = secondIntersectionPoints[0];

                        if (firstIntersectionPoints.Count + secondIntersectionPoints.Count == 3)
                        {
                            HandleTeeWithDifferentElevation(x, firstPipeElevation, secondPipeElevation, teePipeLineObjectIds, firstIntersectionPoints.Count);
                        }

                    }
                }
            });
        }

        public static string GsPgGetProjectNum(List<ObjectId> allGeYuanDrawObjectIds)
        {
            string projectNum = string.Empty;
            allGeYuanDrawObjectIds.ForEach(x =>
            {
                string result = UtilsCommon.UtilsGetNewTitleBlockInfoJObject(x).UtilsGetStrValue("projectnum");
                if (result != string.Empty)
                {
                    projectNum = result;
                }
            });
            return projectNum;
        }

        public static void GsPgBatchSynPipeData()
        {
            using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {
                UtilsCADActive.Editor.WriteMessage("\n请选择绿色箭头辅助管道块");
                List<ObjectId> pipeNumObjectIds = UtilsBlock.UtilsGetObjectIdsBySelectByBlockName("GsPgPipeElementArrowAssist").ToList();

                List<ObjectId> allBlockIds = UtilsBlock.UtilsGetAllBlockObjectIds();
                List<ObjectId> allPolylineObjectIds = UtilsPolyline.UtilsGetAllObjectIdsByLayerName("0DataFlow-GsPgPipeLine*");

                List<string> blockNameList = new List<string> { "GeYuanFrame", "GsPgPipeElementElbow", "GsPgPipeElementArrowAssist", "GsPgValve" };
                Dictionary<string, List<ObjectId>> allObjectIdsGroups = UtilsBlock.UtilsGetAllObjectIdsGroupsByBlockNameList(allBlockIds, blockNameList, false);
                List<ObjectId> allGeYuanDrawObjectIds = allObjectIdsGroups["GeYuanFrame"];
                List<ObjectId> allPipeElbowObjectIds = allObjectIdsGroups["GsPgPipeElementElbow"];
                List<ObjectId> allPipeArrowAssistObjectIds = allObjectIdsGroups["GsPgPipeElementArrowAssist"];
                List<ObjectId> allValveObjectIds = allObjectIdsGroups["GsPgValve"];

                string projectNum = GsPgGetProjectNum(allGeYuanDrawObjectIds);
                PipeInfoHelper pipeInfo = UtilsCommon.UtilsGetPipeInfo(projectNum);

                pipeNumObjectIds.ForEach(x =>
                {
                    List<ObjectId> pipeLineObjectIds = GsPgGetPipeLinesByOnPL(x, allPolylineObjectIds);
                    Dictionary<string, string> pipeData = GsPgGetPipeData(x);
                    GsPgSynPipeElementForOnePipeAssist(pipeData, pipeLineObjectIds, allPolylineObjectIds, allPipeElbowObjectIds, allPipeArrowAssistObjectIds, allValveObjectIds, pipeInfo);
                    UtilsCADActive.Editor.WriteMessage("\n" + pipeData["pipeNum"] + "数据已同步...");
                    GsPgSynPipeElbowStatus(pipeData, pipeInfo);
                });
                _processedPipeElbowObjectIds.Clear();
                _processedPolylineIds.Clear();

                UtilsCADActive.Editor.WriteMessage("\n同步数据完成...");

                tr.Commit();
            }
        }

    }
} 