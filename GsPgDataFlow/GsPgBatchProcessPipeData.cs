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

        public static void GsLcBindXDatatoPipe(Point3d basePoint, List<ObjectId> ObjectIds)
        {
            //ObjectIds.Where(x => IsPipeNumOnPipeLine(basePoint, x))
            //    .ToList()
            //    .ForEach(x => UtilsCADActive.UtilsAddXData(x, "pipeNum", "PL1102"));

            ObjectIds.Where(x => IsPipeNumOnPipeLine(basePoint, x))
                .ToList()
                .ForEach(x => UtilsPolyline.UtilsPolylineChangeColor(x, 1));

        }
        public static void GsPgSynPipeData()
        {
            using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {
                Editor ed = UtilsCADActive.Editor;

                List<ObjectId> polylineObjectIds = UtilsPolyline.UtilsPolylineGetAllObjectIds();

                List<BlockReference> blockReferences = UtilsBlock.UtilsBlockGetObjectIdsBySelectByBlockName("GsPgPipeElementArrowAssist")
                    .Select(x => x.GetObject(OpenMode.ForRead) as BlockReference)
                    .ToList();
                // Retrieve the base point of the block according to its ObectId
                List<Point3d> basePoints = blockReferences.Where(blockRef => blockRef != null)
                    .Select(blockRef => blockRef.Position)
                    .ToList();

                basePoints.ForEach(x => GsLcBindXDatatoPipe(x, polylineObjectIds));

                ed.WriteMessage("\n完成任务...");

                tr.Commit();
            }
        }

    }
}
