using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using CommonUtils.CADUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace GsPgDataFlow
{
    public class ToolManager
    {
        public static List<ObjectId> processedPolylineIds = new List<ObjectId>();
        public static List<ObjectId> processedPipeElbowObjectIds = new List<ObjectId>();

        public static bool IsPipeElementOnPipeLine(Point3d basePoint, ObjectId pipeLineObjectId)
        {
            return UtilsGeometric.UtilsIsPointOnPolyline(basePoint, pipeLineObjectId, 5);

        }

        public static bool IsPipeElementOnPipeLineEnds(Point3d basePoint, ObjectId pipeLineObjectId)
        {
            return UtilsGeometric.UtilsIsPointOnPolylineEnds(basePoint, pipeLineObjectId, 5);

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
                    processedPipeElbowObjectIds.Add(x);
                    ObjectId result = GsPgGetPipeLineByOnPLEnd(x, pipeLineObjectIds);
                    if (result != ObjectId.Null)
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

        public static string GsPgGetPipeDiameter(string pipeNum)
        {
            string pipeDiameter = pipeNum.Split('-').ElementAtOrDefault(1);

            if (pipeDiameter == null)
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
            double result = UtilsCommnon.UtilsStringToDouble(pipeDiameter) * calMultiple;
            return result.ToString();
        }

        public static void GsPgChangeValvePropertyValue(ObjectId pipeLineObjectId, List<ObjectId> allValveObjectIds, Dictionary<string, string> pipeData)
        {
            allValveObjectIds.Where(x => IsPipeElementOnPipeLine(UtilsBlock.UtilsGetBlockBasePoint(x), pipeLineObjectId))
                .ToList()
                .ForEach(x =>
                {

                    string pipeDiater = GsPgGetPipeDiameter(pipeData["pipeNum"]);
                    Dictionary<string, string> propertyDict = new Dictionary<string, string>()
                    {
                        { "sideview-DN", pipeDiater },
                        { "topview-DN", pipeDiater }
                    };
                    UtilsBlock.UtilsSetDynamicPropertyValueByDictData(x, propertyDict);
                });
        }

        public static void GsPgSynPipeElementForOnePipeAssist(Dictionary<string, string> pipeData, List<ObjectId> pipeLineObjectIds, List<ObjectId> allPipeLineObjectIds, List<ObjectId> allElbowObjectIds, List<ObjectId> allPipeArrowAssistObjectIds, List<ObjectId> allValveObjectIds)
        {
            if (pipeLineObjectIds != null)
            {
                pipeLineObjectIds.ForEach(x =>
                {
                    UtilsCADActive.UtilsAddXData(x, pipeData);
                    // for test
                    UtilsPolyline.UtilsChangeColor(x, 1);
                    GsPgChangePipeArrowAssistPropertyValue(x, allPipeArrowAssistObjectIds, pipeData);
                    GsPgChangeValvePropertyValue(x, allValveObjectIds, pipeData);
                    // the key logic: remove the current polyline
                    allPipeLineObjectIds = allPipeLineObjectIds.Where(xx => xx != x).ToList();
                    List<ObjectId> otherPipeLineObjectIds = GsPgGetOtherPipeLineByInterset(x, allPipeLineObjectIds, allElbowObjectIds, pipeData);

                    if (otherPipeLineObjectIds != null)
                    {
                        GsPgSynPipeElementForOnePipeAssist(pipeData, otherPipeLineObjectIds, allPipeLineObjectIds, allElbowObjectIds, allPipeArrowAssistObjectIds, allValveObjectIds);
                    }
                    processedPolylineIds.Add(x);
                });

            }
        }

        public static void GsPgSetHorizontalElbow(ObjectId elbowObjectId, ObjectId pipeLineObjectId1, ObjectId pipeLineObjectId2)
        {
            Point3d horizontalPoint = new Point3d();
            Point3d verticalPoint = new Point3d();
            Point3d basePoint = UtilsBlock.UtilsGetBlockBasePoint(elbowObjectId);
            Point3d firstCrossPoint = UtilsGeometric.UtilsGetIntersectionPointsByBlockAndPolyLine(elbowObjectId, pipeLineObjectId1)[0];
            Point3d secondCrossPoint = UtilsGeometric.UtilsGetIntersectionPointsByBlockAndPolyLine(elbowObjectId, pipeLineObjectId2)[0];
            if (UtilsGeometric.UtilsIsLineHorizontal(basePoint, firstCrossPoint))
            {
                horizontalPoint = firstCrossPoint;
                verticalPoint = secondCrossPoint;
            }
            else
            {
                horizontalPoint = secondCrossPoint;
                verticalPoint = firstCrossPoint;
            }
            if (horizontalPoint.X - basePoint.X > 0 && verticalPoint.Y - basePoint.Y > 0)
            {
                UtilsBlock.UtilsSetBlockRotatonInDegrees(elbowObjectId, 0);
                return;
            }
            if (horizontalPoint.X - basePoint.X < 0 && verticalPoint.Y - basePoint.Y < 0)
            {
                UtilsBlock.UtilsSetBlockRotatonInDegrees(elbowObjectId, 180);
                return;
            }
            if (horizontalPoint.X - basePoint.X < 0 && verticalPoint.Y - basePoint.Y > 0)
            {
                UtilsBlock.UtilsSetBlockRotatonInDegrees(elbowObjectId, 90);
                return;
            }
            if (horizontalPoint.X - basePoint.X > 0 && verticalPoint.Y - basePoint.Y < 0)
            {
                UtilsBlock.UtilsSetBlockRotatonInDegrees(elbowObjectId, 270);
                return;
            }
        }

        public static void GsPgSynPipeElbowStatus(Dictionary<string, string> pipeData)
        {
            string pipeDiater = GsPgGetPipeDiameter(pipeData["pipeNum"]);
            processedPipeElbowObjectIds = processedPipeElbowObjectIds.Distinct().ToList();
            processedPolylineIds = processedPolylineIds.Distinct().ToList();

            processedPipeElbowObjectIds.ForEach(x =>
                {
                    List<ObjectId> pipeLineObjectIds = processedPolylineIds.Where(xx => IsPipeElementOnPipeLineEnds(UtilsBlock.UtilsGetBlockBasePoint(x), xx)).ToList();
                    int pipeLineObjectNum = pipeLineObjectIds.Count();
                    if (pipeLineObjectNum == 2)
                    {
                        //UtilsCADActive.Editor.WriteMessage("\n" + x);
                        double firstPipeElevation = UtilsCommnon.UtilsStringToDouble(UtilsCADActive.UtilsGetXData(pipeLineObjectIds[0], "pipeElevation"));
                        double secondPipeElevation = UtilsCommnon.UtilsStringToDouble(UtilsCADActive.UtilsGetXData(pipeLineObjectIds[1], "pipeElevation"));
                        Point3d basePoint = UtilsBlock.UtilsGetBlockBasePoint(x);
                        if (firstPipeElevation == secondPipeElevation)
                        {
                            UtilsBlock.UtilsSetDynamicPropertyValueByDictData(x, new Dictionary<string, string>() { { "status", "elbow90" } });
                            UtilsBlock.UtilsSetDynamicPropertyValueByDictData(x, new Dictionary<string, string>() { { "radius90", GsPgGetPipeElbowDiameter(pipeDiater, 1.5) } });
                            GsPgSetHorizontalElbow(x, pipeLineObjectIds[0], pipeLineObjectIds[1]);
                        }
                        else
                        {
                            UtilsBlock.UtilsSetDynamicPropertyValueByDictData(x, new Dictionary<string, string>() { { "status", "elbowdown" } });
                            if (firstPipeElevation > secondPipeElevation)
                            {
                                Point3d crossPoint = UtilsGeometric.UtilsGetIntersectionPointsByBlockAndPolyLine(x, pipeLineObjectIds[0])[0];
                                UtilsBlock.UtilsSetBlockRotatonInDegrees(x, UtilsGeometric.UtilsGetAngleByTwoPoint(basePoint, crossPoint));
                            }
                            else
                            {
                                Point3d crossPoint = UtilsGeometric.UtilsGetIntersectionPointsByBlockAndPolyLine(x, pipeLineObjectIds[1])[0];
                                UtilsBlock.UtilsSetBlockRotatonInDegrees(x, UtilsGeometric.UtilsGetAngleByTwoPoint(basePoint, crossPoint));
                            }
                        }
                    }
                });
        }

        public static void GsPgBatchSynPipeData()
        {
            using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {

                Editor ed = UtilsCADActive.Editor;

                ed.WriteMessage("\n请选择绿色箭头辅助管道块");
                List<ObjectId> pipeNumObjectIds = UtilsBlock.UtilsGetObjectIdsBySelectByBlockName("GsPgPipeElementArrowAssist").ToList();
                List<ObjectId> allPolylineObjectIds = UtilsPolyline.UtilsGetAllObjectIdsByLayerName("0DataFlow-GsPgPipeLine*");
                List<ObjectId> allPipeElbowObjectIds = UtilsBlock.UtilsGetAllObjectIdsByBlockName("GsPgPipeElementElbow").ToList();
                List<ObjectId> allPipeArrowAssistObjectIds = UtilsBlock.UtilsGetAllObjectIdsByBlockName("GsPgPipeElementArrowAssist").ToList();
                List<ObjectId> allValveObjectIds = UtilsBlock.UtilsGetAllObjectIdsByBlockName("GsPgValve", false).ToList();

                pipeNumObjectIds.ForEach(x =>
                {
                    List<ObjectId> pipeLineObjectIds = GsPgGetPipeLinesByOnPL(x, allPolylineObjectIds);
                    Dictionary<string, string> pipeData = GsPgGetPipeData(x);
                    GsPgSynPipeElementForOnePipeAssist(pipeData, pipeLineObjectIds, allPolylineObjectIds, allPipeElbowObjectIds, allPipeArrowAssistObjectIds, allValveObjectIds);
                    GsPgSynPipeElbowStatus(pipeData);

                    processedPipeElbowObjectIds.Clear();
                    processedPolylineIds.Clear();
                    UtilsCADActive.Editor.WriteMessage("\n" + pipeData["pipeNum"] + "数据已同步...");
                });

                UtilsCADActive.Editor.WriteMessage("\n同步数据完成...");

                tr.Commit();
            }
        }

        public static void CsTest()
        {
            using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {
                Editor ed = UtilsCADActive.Editor;
                Database db = UtilsCADActive.Database;


                // 通过拾取获得一个块的ObjectId
                //ObjectId blockId = UtilsCADActive.Editor.GetEntity("\n请选择一个块").ObjectId;
                //ed.WriteMessage("\n" + UtilsBlock.UtilsGetBlockRotatonInDegrees(blockId));
                //UtilsBlock.UtilsSetBlockRotatonInDegrees(blockId, 180.0);


                // 通过拾取获得一个多段线的ObjectId
                //ObjectId polylineId = UtilsCADActive.Editor.GetEntity("\n请选择一个多段线").ObjectId;
                //ObjectId polylineId2 = UtilsCADActive.Editor.GetEntity("\n请选择一个多段线").ObjectId;
                //ed.WriteMessage("\n" + UtilsPolyline.GetTwoPolyLineIntersectionAngleInDegrees(polylineId, polylineId2));

                // 完成任务：通过拾取获得一个Point3d
                //Point3d point1 = UtilsCADActive.GetPointFromUser();
                //Point3d point2 = UtilsCADActive.GetPointFromUser();
                //Polyline polyline = polylineId.GetObject(OpenMode.ForRead) as Polyline;
                //Polyline polyline2 = polylineId2.GetObject(OpenMode.ForRead) as Polyline;
                //List<Point3d> pts = UtilsGeometric.UtilsIntersectWith(polyline, polyline2, Intersect.OnBothOperands);

                //List<Point3d> intersectionPoints = UtilsGeometric.UtilsGetIntersectionPointsByBlockAndPolyLineNew(blockId, polylineId);
                ed.WriteMessage("\n" + "test");

                tr.Commit();
            }

        }
    }
}
