using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using CommonUtils.CADUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GsPgDataFlow
{
    public class GsPgBatchProcessPipeData
    {
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
                    UtilsBlock.UtilsSetPropertyValueByDict(x, pipeData);
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
                .ForEach(x => UtilsBlock.UtilsSetPropertyValueByDict(x, pipeData));
        }

        public static void GsPgSynPipeElementForOnePipeAssist(Dictionary<string, string> pipeData, List<ObjectId> pipeLineObjectIds, List<ObjectId> allPipeLineObjectIds, List<ObjectId> allElbowObjectIds, List<ObjectId> allPipeArrowAssistObjectIds)
        {
            if (pipeLineObjectIds != null)
            {
                pipeLineObjectIds.ForEach(x =>
                {
                    UtilsCADActive.UtilsAddXData(x, pipeData);
                    // for test
                    UtilsPolyline.UtilsChangeColor(x, 1);
                    GsPgChangePipeArrowAssistPropertyValue(x, allPipeArrowAssistObjectIds, pipeData);
                    // the key logic: remove the current polyline
                    allPipeLineObjectIds = allPipeLineObjectIds.Where(xx => xx != x).ToList();
                    List<ObjectId> otherPipeLineObjectIds = GsPgGetOtherPipeLineByInterset(x, allPipeLineObjectIds, allElbowObjectIds, pipeData);

                    if (otherPipeLineObjectIds != null)
                    {
                        GsPgSynPipeElementForOnePipeAssist(pipeData, otherPipeLineObjectIds, allPipeLineObjectIds, allElbowObjectIds, allPipeArrowAssistObjectIds);
                    }

                });
                
            }
        }
        public static void GsPgBatchSynPipeData()
        {
            using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {
                Editor ed = UtilsCADActive.Editor;

                List<ObjectId> allPolylineObjectIds = UtilsPolyline.UtilsGetAllObjectIdsByLayerName("0DataFlow-GsPgPipeLine*");
                List<ObjectId> allPipeElbowObjectIds = UtilsBlock.UtilsGetAllObjectIdsByBlockName("GsPgPipeElementElbow").ToList();
                List<ObjectId> allPipeArrowAssistObjectIds = UtilsBlock.UtilsGetAllObjectIdsByBlockName("GsPgPipeElementArrowAssist").ToList();
                List<ObjectId> pipeNumObjectIds = UtilsBlock.UtilsGetObjectIdsBySelectByBlockName("GsPgPipeElementArrowAssist").ToList();

                
                pipeNumObjectIds.ForEach(x =>
                {
                    List<ObjectId> pipeLineObjectIds = GsPgGetPipeLinesByOnPL(x, allPolylineObjectIds);
                    Dictionary<string, string> pipeData = GsPgGetPipeData(x);
                    GsPgSynPipeElementForOnePipeAssist(pipeData, pipeLineObjectIds, allPolylineObjectIds, allPipeElbowObjectIds, allPipeArrowAssistObjectIds);
                    UtilsCADActive.Editor.WriteMessage("\n" + pipeData["pipeNum"] + "数据已同步...");
                });

                ed.WriteMessage("\n同步数据完成...");

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
                ObjectId blockId = UtilsCADActive.Editor.GetEntity("\n请选择一个块").ObjectId;

                Dictionary<string, string> propertyDict = new Dictionary<string, string>()
                {
                    { "pipeNum", "PL1101-50-2J5" },
                    { "elevation", "9.5" },
                    { "key3", "value3" }
                };
                UtilsBlock.UtilsSetPropertyValueByDict(blockId, propertyDict);
                

                //// 通过拾取获得一个多段线的ObjectId
                //ObjectId polylineId = UtilsCADActive.Editor.GetEntity("\n请选择一个多段线").ObjectId;
                //ed.WriteMessage("\n" + IsPipeElementOnPipeLineEnds(UtilsBlock.UtilsGetBlockBasePoint(blockId), polylineId));


                tr.Commit();
            }

        }

    }
}
