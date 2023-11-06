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

        public static List<ObjectId> GsPgGetOtherPipeLineByInterset(ObjectId pipeLineObjectId, List<ObjectId> pipeLineObjectIds, List<ObjectId> ElbowObjectIds)
        {
            List<ObjectId> otherPipeLineObjectIds = new List<ObjectId>();
            ElbowObjectIds.Where(x => IsPipeElementOnPipeLineEnds(UtilsBlock.UtilsGetBlockBasePoint(x), pipeLineObjectId))
                .ToList()
                .ForEach(x =>
                {
                    ObjectId result = GsPgGetPipeLineByOnPLEnd(x, pipeLineObjectIds);
                    if (result != ObjectId.Null)
                    {
                        otherPipeLineObjectIds.Add(result);
                    }
                });
            return otherPipeLineObjectIds;
        }

        public static void GsPgSynPipeElementForOnePipeAssist(Dictionary<string, string> pipeData, List<ObjectId> pipeLineObjectIds, List<ObjectId> allPipeLineObjectIds, List<ObjectId> ElbowObjectIds)
        {
            if (pipeLineObjectIds != null)
            {
                pipeLineObjectIds.ForEach(x =>
                {
                    UtilsCADActive.UtilsAddXData(x, pipeData);
                    // for test
                    UtilsPolyline.UtilsChangeColor(x, 1);
                    // the key logic: remove the current polyline
                    allPipeLineObjectIds = allPipeLineObjectIds.Where(xx => xx != x).ToList();


                    ElbowObjectIds.Where(xx => IsPipeElementOnPipeLineEnds(UtilsBlock.UtilsGetBlockBasePoint(xx), x))
                        .ToList()
                        .ForEach(xx => UtilsBlock.UtilsChangeBlockLayerName(xx, "0"));

                    // for test
                    ElbowObjectIds.Where(xx => IsPipeElementOnPipeLineEnds(UtilsBlock.UtilsGetBlockBasePoint(xx), x))
                        .ToList()
                        .ForEach(xx => UtilsCADActive.Editor.WriteMessage("\n" + UtilsBlock.UtilsGetBlockBasePoint(xx)));

                    //ElbowObjectIds.ForEach(x => UtilsCADActive.Editor.WriteMessage("\n" + UtilsBlock.UtilsGetBlockBasePoint(x)));
                    UtilsCADActive.Editor.WriteMessage("\n" + pipeData["pipeNum"] + "数据已同步...");

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
                List<ObjectId> pipeNumObjectIds = UtilsBlock.UtilsGetObjectIdsBySelectByBlockName("GsPgPipeElementArrowAssist").ToList();

                
                pipeNumObjectIds.ForEach(x =>
                {
                    List<ObjectId> pipeLineObjectIds = GsPgGetPipeLinesByOnPL(x, allPolylineObjectIds);
                    Dictionary<string, string> pipeData = GsPgGetPipeData(x);
                    GsPgSynPipeElementForOnePipeAssist(pipeData, pipeLineObjectIds, allPolylineObjectIds, allPipeElbowObjectIds);
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


                //// 根据多段线的ObjectId获得多段线的对象
                //Polyline polyline = tr.GetObject(polylineId, OpenMode.ForWrite) as Polyline;

                //UtilsCADActive.UtilsAddXData(polylineId, "pipeNum", "PL1101");
                //ed.WriteMessage("\n" + UtilsCADActive.UtilsGetXData(polylineId, "pipeNum"));

                //// 通过拾取获得一个块的ObjectId
                //ObjectId blockId = UtilsCADActive.Editor.GetEntity("\n请选择一个块").ObjectId;
                //string propertyValue = UtilsBlock.UtilsBlockGetPropertyValueByPropertyName(blockId, "pipeNum");
                //ed.WriteMessage("\n" + propertyValue);

                // 通过拾取获得一个块的ObjectId
                ObjectId blockId = UtilsCADActive.Editor.GetEntity("\n请选择一个块").ObjectId;
                // 通过拾取获得一个多段线的ObjectId
                ObjectId polylineId = UtilsCADActive.Editor.GetEntity("\n请选择一个多段线").ObjectId;
                ed.WriteMessage("\n" + IsPipeElementOnPipeLineEnds(UtilsBlock.UtilsGetBlockBasePoint(blockId), polylineId));


                tr.Commit();
            }

        }

    }
}
