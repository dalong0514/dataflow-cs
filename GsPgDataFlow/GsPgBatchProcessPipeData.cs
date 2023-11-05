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

        public static void GsLcBindXDatatoPipe(ObjectId pipeNumObjectId, List<ObjectId> pipeLineObjectIds, List<ObjectId> ElbowObjectIds)
        {
            //pipeLineObjectIds.Where(x => IsPipeNumOnPipeLine(UtilsBlock.UtilsBlockGetBlockBasePoint(pipeNumObjectId), x))
            //    .ToList()
            //    .ForEach(x => UtilsCADActive.UtilsAddXData(x, "pipeNum", UtilsBlock.UtilsBlockGetPropertyValueByPropertyName(objectId, "pipeNum")));

            pipeLineObjectIds.Where(x => IsPipeNumOnPipeLine(UtilsBlock.GetBlockBasePoint(pipeNumObjectId), x))
                .ToList()
                .ForEach(x => UtilsPolyline.UtilsPolylineChangeColor(x, 1));

        }
        public static void GsPgBatchSynPipeData()
        {
            using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {
                Editor ed = UtilsCADActive.Editor;

                List<ObjectId> polylineObjectIds = UtilsPolyline.UtilsPolylineGetAllObjectIds();
                List<ObjectId> pipeNumObjectIds = UtilsBlock.GetObjectIdsBySelectByBlockName("GsPgPipeElementArrowAssist").ToList();
                List<ObjectId> pipeElbowObjectIds = UtilsBlock.GetAllObjectIdsByBlockName("GsPgPipeElementElbow").ToList();

                pipeNumObjectIds.ForEach(x => GsLcBindXDatatoPipe(x, polylineObjectIds, pipeElbowObjectIds));

                ed.WriteMessage("\n完成任务...");

                tr.Commit();
            }
        }

    }
}
