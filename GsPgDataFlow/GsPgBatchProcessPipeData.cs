using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using CommonUtils.CADUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GsPgDataFlow
{
    public class GsPgBatchProcessPipeData
    {
        public static bool IsPipeNumOnPipeLine(Point3d basePoint, ObjectId objectId)
        {
            return UtilsGeometric.UtilsGetPointToPolylineShortestDistance(basePoint, objectId) < 5;

        }

        public static void GsPgBindXDatatoPipe(ObjectId pipeNumObjectId, ObjectId pipeLineObjectId)
        {
            List<string> propertyNameList = new List<string> { "pipeNum", "elevation" };
            Dictionary<string, string> probertyValueDictList = UtilsBlock.UtilsGetPropertyDictListByPropertyNameList(pipeNumObjectId, propertyNameList);
            Dictionary<string, string> xdataDictList = new Dictionary<string, string>
            {
                { "pipeNum", probertyValueDictList["PIPENUM"] },
                { "pipeElevation", probertyValueDictList["ELEVATION"] }
            };
            UtilsCADActive.UtilsAddXData(pipeLineObjectId, xdataDictList);
        }

        public static void GsPgSynOnePipeData(ObjectId pipeNumObjectId, List<ObjectId> pipeLineObjectIds, List<ObjectId> ElbowObjectIds)
        {
            //pipeLineObjectIds.Where(x => IsPipeNumOnPipeLine(UtilsBlock.UtilsGetBlockBasePoint(pipeNumObjectId), x))
            //    .ToList()
            //    .ForEach(x => GsPgBindXDatatoPipe(pipeNumObjectId, x));

            pipeLineObjectIds.Where(x => IsPipeNumOnPipeLine(UtilsBlock.UtilsGetBlockBasePoint(pipeNumObjectId), x))
                .ToList()
                .ForEach(x => UtilsPolyline.UtilsChangeColor(x, 1));

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

                ed.WriteMessage("\n同步数据成功...");

                tr.Commit();
            }
        }

        public static void CsTest()
        {
            using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {
                Editor ed = UtilsCADActive.Editor;
                Database db = UtilsCADActive.Database;


                //// 通过拾取获得一个多段线的ObjectId
                //ObjectId polylineId = UtilsCADActive.Editor.GetEntity("\n请选择一个多段线").ObjectId;
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
                ed.WriteMessage("\n" + UtilsBlock.UtilsGetBlockName(blockId));


                tr.Commit();
            }

        }

    }
}
