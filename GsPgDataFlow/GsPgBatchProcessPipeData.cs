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

        public static void GsPgSynOnePipeData(ObjectId pipeNumObjectId, List<ObjectId> pipeLineObjectIds, List<ObjectId> ElbowObjectIds)
        {
            ObjectId pipeLineObjectId = GsPgGetPipeLineByOnPL(pipeNumObjectId, pipeLineObjectIds);
            if (pipeLineObjectId != ObjectId.Null)
            {
                UtilsCADActive.UtilsAddXData(pipeLineObjectId, GsPgGetPipeData(pipeNumObjectId));
                // for test
                UtilsPolyline.UtilsChangeColor(pipeLineObjectId, 1);
                // the key logic: remove the current polyline
                pipeLineObjectIds = pipeLineObjectIds.Where(x => x != pipeLineObjectId).ToList();

                ElbowObjectIds.Where(x => IsPipeElementOnPipeLineEnds(UtilsBlock.UtilsGetBlockBasePoint(x), pipeLineObjectId))
                    .ToList()
                    .ForEach(x => UtilsBlock.UtilsChangeBlockLayerName(x, "0"));




                ElbowObjectIds.Where(x => IsPipeElementOnPipeLineEnds(UtilsBlock.UtilsGetBlockBasePoint(x), pipeLineObjectId))
                    .ToList()
                    .ForEach(x => UtilsCADActive.Editor.WriteMessage("\n" + UtilsBlock.UtilsGetBlockBasePoint(x)));

                //ElbowObjectIds.ForEach(x => UtilsCADActive.Editor.WriteMessage("\n" + UtilsBlock.UtilsGetBlockBasePoint(x)));
                UtilsCADActive.Editor.WriteMessage("\n" + UtilsBlock.UtilsGetPropertyValueByPropertyName(pipeNumObjectId, "pipeNum") + "数据已同步...");
            }
        }
        public static void GsPgBatchSynPipeData()
        {
            using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {
                Editor ed = UtilsCADActive.Editor;

                List<ObjectId> polylineObjectIds = UtilsPolyline.UtilsGetAllObjectIdsByLayerName("0DataFlow-GsPgPipeLine*");
                List<ObjectId> pipeNumObjectIds = UtilsBlock.UtilsGetObjectIdsBySelectByBlockName("GsPgPipeElementArrowAssist").ToList();
                List<ObjectId> pipeElbowObjectIds = UtilsBlock.UtilsGetAllObjectIdsByBlockName("GsPgPipeElementElbow").ToList();

                pipeNumObjectIds.ForEach(x => GsPgSynOnePipeData(x, polylineObjectIds, pipeElbowObjectIds));

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
