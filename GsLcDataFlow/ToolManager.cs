using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using CommonUtils.CADUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace GsLcDataFlow
{
    internal class ToolManager
    {

        public static void GetAllPolyline()
        {
            using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {
                // 获取模型空间多段线的选择集
                SelectionSet selSet = UtilsSelectionSet.UtilsGetAllPolylineSelectionSet();
                if (selSet == null) return;
                // 删除选择集中的所有对象
                foreach (ObjectId objectId in selSet.GetObjectIds())
                {
                    Entity entity = tr.GetObject(objectId, OpenMode.ForWrite) as Entity;
                    entity.Erase();
                }
                // 打印字符串“删除成功”
                UtilsCADActive.Editor.WriteMessage("\n删除成功");

                tr.Commit();
            }
        }

        public static void GsLcBindXDatatoPipe(Point3d basePoint, List<ObjectId> ObjectIds)
        {
            ObjectIds.Where(x => UtilsGeometric.UtilsGetPointToPolylineShortestDistance(basePoint, x) < 0.2)
                .ToList()
                .ForEach(x => UtilsCADActive.UtilsAddOneXData(x, "pipeNum", "PL1102"));
        }


        public static void GsLcUpdateInstrumentLocationOnPipe()
        {
            using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {
                Editor ed = UtilsCADActive.Editor;

                List<ObjectId> polylineObjectIds = UtilsPolyline.UtilsGetAllObjectIds();

                List<BlockReference> blockReferences = UtilsBlock.UtilsGetObjectIdsBySelectByBlockName("PipeArrowLeft")
                    .Select(x => x.GetObject(OpenMode.ForRead) as BlockReference)
                    .ToList();
                // 根据blockId获得块的基点
                List<Point3d> basePoints = blockReferences.Where(blockRef => blockRef != null)
                    .Select(blockRef => blockRef.Position)
                    .ToList();

                basePoints.ForEach(x => GsLcBindXDatatoPipe(x, polylineObjectIds));

                ed.WriteMessage("\n完成任务...");

                tr.Commit();
            }
        }


        public static void CsTest()
        {
            using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {
                Editor ed = UtilsCADActive.Editor;



                //// 通过拾取获得一个多段线的ObjectId
                //ObjectId polylineId = UtilsCADActive.Editor.GetEntity("\n请选择一个多段线").ObjectId;
                //// 根据多段线的ObjectId获得多段线的对象
                //Polyline polyline = tr.GetObject(polylineId, OpenMode.ForWrite) as Polyline;

                //UtilsCADActive.UtilsAddXData(polylineId, "pipeNum", "PL1101");
                //ed.WriteMessage("\n" + UtilsCADActive.UtilsGetXData(polylineId, "pipeNum"));

                // 通过拾取获得一个块的ObjectId
                ObjectId blockId = UtilsCADActive.Editor.GetEntity("\n请选择一个块").ObjectId;
                string propertyValue = UtilsBlock.UtilsGetPropertyValueByPropertyName(blockId, "pipeNum");
                ed.WriteMessage("\n" + propertyValue);

                tr.Commit();
            }



        }
    }
}
