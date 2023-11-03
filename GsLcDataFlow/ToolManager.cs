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

        public static void GsLcBindXDatatoPipe(Point3d basePoint, List<Polyline> polylines)
        {
            polylines.Where(x => UtilsGeometric.UtilsGetPointToPolylineShortestDistance(basePoint, x) < 0.2)
                .ToList()
                .ForEach(x => x.ColorIndex = 1);
        }




        public static void GsLcUpdateInstrumentLocationOnPipe()
        {
            using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {
                Editor ed = UtilsCADActive.Editor;

                //List<Polyline> polylines = UtilsPolyline.UtilsGetAllPolylineObjects();

                //List<BlockReference> blockReferences = UtilsBlock.UtilsGetBlockReferencesBySelectByBlockName("PipeArrowLeft");
                //// 根据blockId获得块的基点
                //List<Point3d> basePoints = blockReferences.Where(blockRef => blockRef != null)
                //    .Select(blockRef => blockRef.Position)
                //    .ToList();

                //basePoints.ForEach(x => GsLcBindXDatatoPipe(x, polylines));



                // 通过拾取获得一个多段线的ObjectId
                ObjectId polylineId = UtilsCADActive.Editor.GetEntity("\n请选择一个多段线").ObjectId;
                // 根据多段线的ObjectId获得多段线的对象
                Polyline polyline = tr.GetObject(polylineId, OpenMode.ForWrite) as Polyline;

                Entity ent = (Entity)tr.GetObject(polylineId, OpenMode.ForWrite);

                UtilsCADActive.UtilsAddXData(ent, "pipeNum", "PL1101");
                ed.WriteMessage("\n" + UtilsCADActive.UtilsGetXData(ent, "pipeNum"));


                //// 获得 basePoint 与该多段线的最近点的距离

                //double distance = UtilsGeometric.UtilsGetPointToPolylineShortestDistance(basePoint, polyline);

                //ed.WriteMessage("\n" + distance.ToString());



                //// 通过拾取获得一个块的ObjectId
                //ObjectId blockId = UtilsCADActive.Editor.GetEntity("\n请选择一个块").ObjectId;
                //// 根据块的ObjectId获得块的对象
                //BlockReference blockRef = blockId.GetObject(OpenMode.ForRead) as BlockReference;
                //Point3d basePoint = blockRef.Position;




                tr.Commit();
            }
        }
    }
}
