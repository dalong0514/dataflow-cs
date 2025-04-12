using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;

namespace dataflow_cs.Utils.CADUtils
{
    /// <summary>
    /// AutoCAD几何操作工具类
    /// </summary>
    public static class UtilsGeometry
    {

        /// <summary>
        /// 判断点是否在直线的端点上
        /// </summary>
        public static bool UtilsIsPointOnLineEnd(Point3d point, List<Point3d> linePoints, double tolerance = 5)
        {
            return UtilsIsPointNearPoint(point, linePoints[0], tolerance) ||
                   UtilsIsPointNearPoint(point, linePoints[1], tolerance);
        }

        /// <summary>
        /// 判断两点是否接近
        /// </summary>
        /// <param name="pt1">第一个点</param>
        /// <param name="pt2">第二个点</param>
        /// <param name="tolerance">容差</param>
        /// <returns>两点是否接近</returns>
        public static bool UtilsIsPointNearPoint(Point3d pt1, Point3d pt2, double tolerance)
        {
            return pt1.DistanceTo(pt2) <= tolerance;
        }

        /// <summary>
        /// 获取点到多段线的最短距离
        /// </summary>
        /// <param name="point">参考点坐标</param>
        /// <param name="polylineObjectId">多段线的ObjectId</param>
        /// <returns>最短距离值</returns>
        public static double UtilsGetPointToPolylineShortestDistance(Point3d point, ObjectId polylineObjectId)
        {
            // get the closest distance from the basePoint to the polyline
            Polyline polyline = polylineObjectId.GetObject(OpenMode.ForRead) as Polyline;
            Point3d closestPoint = polyline.GetClosestPointTo(point, false);
            return point.DistanceTo(closestPoint);
        }

        /// <summary>
        /// 判断点是否在多段线上（在指定容差范围内）
        /// </summary>
        /// <param name="point">要检查的点</param>
        /// <param name="polylineObjectId">多段线的ObjectId</param>
        /// <param name="tolerance">容差值</param>
        /// <returns>如果点在多段线上（距离小于容差值）则返回true</returns>
        public static bool UtilsIsPointOnPolyline(Point3d point, ObjectId polylineObjectId, double tolerance)
        {
            return UtilsGetPointToPolylineShortestDistance(point, polylineObjectId) < tolerance;
        }

        /// <summary>
        /// 判断点是否在多段线的端点上（在指定容差范围内）
        /// </summary>
        /// <param name="point">要检查的点</param>
        /// <param name="polylineObjectId">多段线的ObjectId</param>
        /// <param name="tolerance">容差值</param>
        /// <returns>如果点在多段线的起点或终点上则返回true</returns>
        public static bool UtilsIsPointOnPolylineEnds(Point3d point, ObjectId polylineObjectId, double tolerance)
        {
            Polyline polyline = polylineObjectId.GetObject(OpenMode.ForRead) as Polyline;
            // Get the start and end points of the polyline
            Point3d startPoint = polyline.StartPoint;
            Point3d endPoint = polyline.EndPoint;

            // Check if the closest point is the same as the start or end point
            // Use a small tolerance for comparison because of potential floating point errors
            bool isOnStart = startPoint.DistanceTo(point) < tolerance;
            bool isOnEnd = endPoint.DistanceTo(point) < tolerance;

            return isOnStart || isOnEnd;
        }
        
        /// <summary>
        /// 判断点是否在线上
        /// </summary>
        /// <param name="pt1">要检查的点</param>
        /// <param name="lineStartPt">线起点</param>
        /// <param name="lineEndPt">线终点</param>
        /// <param name="tolerance">容差</param>
        /// <returns>是否在线上</returns>
        public static bool UtilsIsPointInLineByDistance(Point3d pt1, Point3d lineStartPt, Point3d lineEndPt, double tolerance)
        {
            // 计算点到线的角度
            double angle1 = UtilsGetAngleByTwoPoint(lineStartPt, pt1) - UtilsGetAngleByTwoPoint(lineStartPt, lineEndPt);
            
            // 计算点到线的最短距离
            double minDistance = Math.Abs(lineStartPt.DistanceTo(pt1) * Math.Sin(angle1));
            
            // 判断点是否在线上
            bool isNearEndPoints = pt1.DistanceTo(lineStartPt) <= tolerance || pt1.DistanceTo(lineEndPt) <= tolerance;
            bool isWithinBounds = (pt1.X > Math.Min(lineStartPt.X, lineEndPt.X) - tolerance && 
                           pt1.X < Math.Max(lineStartPt.X, lineEndPt.X) + tolerance && 
                           pt1.Y > Math.Min(lineStartPt.Y, lineEndPt.Y) - tolerance && 
                           pt1.Y < Math.Max(lineStartPt.Y, lineEndPt.Y) + tolerance);
            
            return minDistance <= tolerance && (isNearEndPoints || isWithinBounds);
        }

        /// <summary>
        /// 判断点是否在块的矩形边界内
        /// </summary>
        /// <param name="point">要检查的点</param>
        /// <param name="blockObjectId">块的ObjectId</param>
        /// <returns>如果点在块的矩形边界内则返回true</returns>
        public static bool UtilsIsPointWithRectangleBlock(Point3d point, ObjectId blockObjectId)
        {
            BlockReference blockRef = blockObjectId.GetObject(OpenMode.ForRead) as BlockReference;
            Extents3d geYuanFrameExtents = blockRef.GeometricExtents;
            // Check if the position of pipeNum is within the extents of the rectangle
            return geYuanFrameExtents.MinPoint.X <= point.X && point.X <= geYuanFrameExtents.MaxPoint.X &&
                geYuanFrameExtents.MinPoint.Y <= point.Y && point.Y <= geYuanFrameExtents.MaxPoint.Y;
        }

        /// <summary>
        /// 计算以起点为原点，从起点到终点的直线在图纸空间的角度（度数值）
        /// </summary>
        /// <param name="startPoint">起点</param>
        /// <param name="endPoint">终点</param>
        /// <returns>角度值（度数）</returns>
        public static double UtilsGetAngleByTwoPoint(Point3d startPoint, Point3d endPoint)
        {
            double angle = Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X);
            return angle * (180.0 / Math.PI);
        }

        /// <summary>
        /// 计算三点形成的角度，以basePoint为顶点，计算向startPoint和endPoint的两条线段之间的夹角
        /// </summary>
        /// <param name="basePoint">角的顶点</param>
        /// <param name="startPoint">第一条线段的另一个端点</param>
        /// <param name="endPoint">第二条线段的另一个端点</param>
        /// <returns>两条线段之间的夹角（度数）</returns>
        public static double UtilsGetAngleByThreePoint(Point3d basePoint, Point3d startPoint, Point3d endPoint)
        {
            // 完成任务：1）已知三个点A点、B点和C点。1）计算AB和AC的夹角
            double angle1 = UtilsGetAngleByTwoPoint(basePoint, startPoint);
            double angle2 = UtilsGetAngleByTwoPoint(basePoint, endPoint);
            return Math.Abs(angle2 - angle1);
        }

        /// <summary>
        /// 基于3D范围创建一个矩形多段线边界
        /// </summary>
        /// <param name="extents">3D范围</param>
        /// <param name="exDis">边界扩展距离</param>
        /// <param name="layerName">多段线的图层名称，如果为空则使用当前图层</param>
        /// <returns>矩形多段线对象</returns>
        public static Polyline UtilsGetBoundary(this Extents3d extents, double exDis = 0, string layerName = "")
        {
            Polyline polyline = new Polyline();
            polyline.AddVertexAt(0, extents.MinPoint.UtilsPoint3dToPoint2d() + new Vector2d(-exDis, -exDis), 0, 0, 0);
            polyline.AddVertexAt(1, new Point2d(extents.MaxPoint.X, extents.MinPoint.Y) + new Vector2d(exDis, -exDis), 0, 0, 0);
            polyline.AddVertexAt(2, extents.MaxPoint.UtilsPoint3dToPoint2d() + new Vector2d(exDis, exDis), 0, 0, 0);
            polyline.AddVertexAt(3, new Point2d(extents.MinPoint.X, extents.MaxPoint.Y) + new Vector2d(-exDis, exDis), 0, 0, 0);
            polyline.Closed = true;
            if (!string.IsNullOrEmpty(layerName))
                polyline.Layer = layerName;

            return polyline;
        }

        /// <summary>
        /// 以指定点为中心创建一个正方形多段线
        /// </summary>
        /// <param name="point">中心点</param>
        /// <param name="distance">中心点到边的距离（半边长）</param>
        /// <returns>正方形多段线对象</returns>
        public static Polyline UtilsCreateRectangleByCenterPoint(Point3d point, double distance)
        {
            Polyline polyline = new Polyline();
            polyline.AddVertexAt(0, point.UtilsPoint3dToPoint2d() + new Vector2d(-distance, -distance), 0, 0, 0);
            polyline.AddVertexAt(1, new Point2d(point.X + distance, point.Y - distance), 0, 0, 0);
            polyline.AddVertexAt(2, point.UtilsPoint3dToPoint2d() + new Vector2d(distance, distance), 0, 0, 0);
            polyline.AddVertexAt(3, new Point2d(point.X - distance, point.Y + distance), 0, 0, 0);
            polyline.Closed = true;
            return polyline;
        }

        /// <summary>
        /// 将3D点转换为2D点（忽略Z坐标）
        /// </summary>
        /// <param name="point">3D点</param>
        /// <returns>转换后的2D点</returns>
        public static Point2d UtilsPoint3dToPoint2d(this Point3d point)
        {
            return new Point2d(point.X, point.Y);
        }

        /// <summary>
        /// 获取块与多段线的交点
        /// </summary>
        /// <param name="blockObjectId">块的ObjectId</param>
        /// <param name="polylineObjectId">多段线的ObjectId</param>
        /// <returns>交点列表</returns>
        public static List<Point3d> UtilsGetIntersectionPointsByBlockAndPolyLine(ObjectId blockObjectId, ObjectId polylineObjectId)
        {
            BlockReference blockRef = blockObjectId.GetObject(OpenMode.ForRead) as BlockReference;
            // the key logic: get the boundary of the block
            Polyline p = UtilsGetBoundary(blockRef.GeometricExtents);
            if (polylineObjectId != null && p != null)
            {
                return UtilsPolyline.UtilsGetIntersectionsByTwoPolyLine(polylineObjectId, p);
            }
            return null;
        }

        /// <summary>
        /// 判断线段是否为水平线段
        /// </summary>
        /// <param name="startPoint">起点</param>
        /// <param name="endPoint">终点</param>
        /// <param name="tolerance">容差值，默认为1.0</param>
        /// <returns>如果线段为水平线段则返回true</returns>
        public static bool UtilsIsLineHorizontal(Point3d startPoint, Point3d endPoint, double tolerance = 1.0)
        {
            // 2023-11-29 fix bug: 1）当直线的起点和终点的Y坐标相差小于5时，认为该直线是水平直线
            return Math.Abs(startPoint.Y - endPoint.Y) < tolerance;

        }

        /// <summary>
        /// 计算两条线段的交角
        /// </summary>
        /// <param name="line1Start">第一条线段的起点</param>
        /// <param name="line1End">第一条线段的终点</param>
        /// <param name="line2Start">第二条线段的起点</param>
        /// <param name="line2End">第二条线段的终点</param>
        /// <returns>两条线段的交角（度数）</returns>
        public static double UtilsGetIntersectionAngleByTwoLineEnd(Point3d line1Start, Point3d line1End, Point3d line2Start, Point3d line2End)
        {
            Vector3d direction1 = line1End - line1Start; // 线段1的方向向量
            Vector3d direction2 = line2End - line2Start; // 线段2的方向向量
            if (direction1.Y > 0 && direction1.Y > 0)
            {
                return 0;
            }
            return 0;
        }

        /// <summary>
        /// 将指定实体对象前置到绘制顺序的最前面
        /// </summary>
        /// <param name="objectId">要前置的实体对象的ObjectId</param>
        /// <returns>操作是否成功</returns>
        public static bool UtilsBringToFront(ObjectId objectId)
        {
            try
            {
                // 获取当前数据库
                Database db = objectId.Database;
                
                // 开启事务
                using (Transaction transaction = db.TransactionManager.StartTransaction())
                {
                    // 获取当前空间的BlockTableRecord
                    BlockTableRecord blockTableRecord = transaction.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                    
                    // 获取绘制顺序表
                    DrawOrderTable drawOrderTable = transaction.GetObject(blockTableRecord.DrawOrderTableId, OpenMode.ForWrite) as DrawOrderTable;
                    
                    // 将指定对象移动到绘制顺序的顶部
                    ObjectIdCollection idsToMove = new ObjectIdCollection();
                    idsToMove.Add(objectId);
                    drawOrderTable.MoveToTop(idsToMove);
                    
                    // 提交事务
                    transaction.Commit();
                    
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                // 记录错误，但不中断程序执行
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n前置对象时出错: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 批量将指定实体对象前置到绘制顺序的最前面
        /// </summary>
        /// <param name="objectIds">要前置的实体对象的ObjectId列表</param>
        /// <returns>操作是否成功</returns>
        /// 循环外创建事务明显更好，原因：
        // AutoCAD的事务操作开销较大，尤其是对大量对象进行操作时
        // DrawOrderTable.MoveToTop方法本身就设计为可以批量处理ObjectIdCollection
        // 性能差异显著，尤其是当处理几十、几百个对象时
        // 批量操作符合AutoCAD API的设计理念
        public static bool UtilsBringToFrontBatch(List<ObjectId> objectIds)
        {
            if (objectIds == null || objectIds.Count == 0)
                return false;
                
            try
            {
                Database db = objectIds.First().Database;
                using (Transaction transaction = db.TransactionManager.StartTransaction())
                {
                    BlockTableRecord blockTableRecord = transaction.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                    DrawOrderTable drawOrderTable = transaction.GetObject(blockTableRecord.DrawOrderTableId, OpenMode.ForWrite) as DrawOrderTable;
                    
                    ObjectIdCollection idsToMove = new ObjectIdCollection();
                    foreach (ObjectId id in objectIds)
                    {
                        idsToMove.Add(id);
                    }
                    
                    drawOrderTable.MoveToTop(idsToMove);
                    transaction.Commit();
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n批量前置对象时出错: " + ex.Message);
                return false;
            }
        }

    }
} 