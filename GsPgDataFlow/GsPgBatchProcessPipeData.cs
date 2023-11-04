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

        public static void GsLcBindXDatatoPipe(ObjectId objectId, List<ObjectId> ObjectIds)
        {
            ObjectIds.Where(x => IsPipeNumOnPipeLine(UtilsBlock.UtilsBlockGetBlockBasePoint(objectId), x))
                .ToList()
                .ForEach(x => UtilsCADActive.UtilsAddXData(x, "pipeNum", UtilsBlock.UtilsBlockGetPropertyValueByPropertyName(objectId, "pipeNum")));

            //ObjectIds.Where(x => IsPipeNumOnPipeLine(UtilsBlock.UtilsBlockGetBlockBasePoint(objectId), x))
            //    .ToList()
            //    .ForEach(x => UtilsPolyline.UtilsPolylineChangeColor(x, 1));

        }
        public static void GsPgSynPipeData()
        {
            using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {
                Editor ed = UtilsCADActive.Editor;

                List<ObjectId> polylineObjectIds = UtilsPolyline.UtilsPolylineGetAllObjectIds();
                List<ObjectId> blockObjectIds = UtilsBlock.UtilsBlockGetObjectIdsBySelectByBlockName("GsPgPipeElementArrowAssist").ToList();

                blockObjectIds.ForEach(x => GsLcBindXDatatoPipe(x, polylineObjectIds));

                ed.WriteMessage("\n完成任务...");

                tr.Commit();
            }
        }

    }
}
