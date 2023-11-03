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
        public static void GsPgSynPipeData()
        {
            using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {
                Editor ed = UtilsCADActive.Editor;

                List<ObjectId> polylineObjectIds = UtilsPolyline.UtilsGetAllPolylineObjectIds();

                List<BlockReference> blockReferences = UtilsBlock.UtilsBlockGetObjectIdsBySelectByBlockName("GsPgPipeElementArrowAssist")
                    .Select(x => x.GetObject(OpenMode.ForRead) as BlockReference)
                    .ToList();
                // 根据blockId获得块的基点
                List<Point3d> basePoints = blockReferences.Where(blockRef => blockRef != null)
                    .Select(blockRef => blockRef.Position)
                    .ToList();

                //basePoints.ForEach(x => GsLcBindXDatatoPipe(x, polylineObjectIds));

                ed.WriteMessage("\n完成任务...");

                tr.Commit();
            }
        }

    }
}
