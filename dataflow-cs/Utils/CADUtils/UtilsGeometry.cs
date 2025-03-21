using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace dataflow_cs.Utils.CADUtils
{
    /// <summary>
    /// AutoCAD几何操作工具类
    /// </summary>
    public static class UtilsGeometry
    {
        /// <summary>
        /// 获取点到多段线的最短距离
        /// </summary>
        /// <param name="point">参考点坐标</param>
        /// <param name="polylineObjectId">多段线的ObjectId</param>
        /// <returns>最短距离值</returns>
        public static double UtilsGetPointToPolylineShortestDistance(Point3d point, ObjectId polylineObjectId)
        {
            // 获get the closest distance from the basePoint to the polyline
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
    }
} 