using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace dataflow_cs.Utils.CADUtils
{
    /// <summary>
    /// 几何工具类，提供处理AutoCAD几何对象的方法
    /// </summary>
    public static class GeometryUtils
    {
        /// <summary>
        /// 获取点到多段线的最短距离
        /// </summary>
        /// <param name="point">检测点</param>
        /// <param name="polylineObjectId">多段线ObjectId</param>
        /// <returns>最短距离</returns>
        public static double GetPointToPolylineDistance(Point3d point, ObjectId polylineObjectId)
        {
            using (Transaction tr = polylineObjectId.Database.TransactionManager.StartTransaction())
            {
                Polyline polyline = tr.GetObject(polylineObjectId, OpenMode.ForRead) as Polyline;
                if (polyline != null)
                {
                    Point3d closestPoint = polyline.GetClosestPointTo(point, false);
                    return point.DistanceTo(closestPoint);
                }
                return double.MaxValue;
            }
        }

        /// <summary>
        /// 判断点是否在多段线上(指定容差内)
        /// </summary>
        /// <param name="point">检测点</param>
        /// <param name="polylineObjectId">多段线ObjectId</param>
        /// <param name="tolerance">容差</param>
        /// <returns>是否在多段线上</returns>
        public static bool IsPointOnPolyline(Point3d point, ObjectId polylineObjectId, double tolerance = 1.0)
        {
            double distance = GetPointToPolylineDistance(point, polylineObjectId);
            return distance <= tolerance;
        }

        /// <summary>
        /// 判断点是否在多段线的端点上(指定容差内)
        /// </summary>
        /// <param name="point">检测点</param>
        /// <param name="polylineObjectId">多段线ObjectId</param>
        /// <param name="tolerance">容差</param>
        /// <returns>是否在多段线端点上</returns>
        public static bool IsPointOnPolylineEnds(Point3d point, ObjectId polylineObjectId, double tolerance = 1.0)
        {
            using (Transaction tr = polylineObjectId.Database.TransactionManager.StartTransaction())
            {
                Polyline polyline = tr.GetObject(polylineObjectId, OpenMode.ForRead) as Polyline;
                if (polyline != null)
                {
                    Point3d startPoint = polyline.StartPoint;
                    Point3d endPoint = polyline.EndPoint;
                    
                    double distanceToStart = point.DistanceTo(startPoint);
                    double distanceToEnd = point.DistanceTo(endPoint);
                    
                    return distanceToStart <= tolerance || distanceToEnd <= tolerance;
                }
                return false;
            }
        }

        /// <summary>
        /// 判断点是否在块的矩形范围内
        /// </summary>
        /// <param name="point">检测点</param>
        /// <param name="blockObjectId">块参照ObjectId</param>
        /// <returns>是否在块内</returns>
        public static bool IsPointWithinBlock(Point3d point, ObjectId blockObjectId)
        {
            using (Transaction tr = blockObjectId.Database.TransactionManager.StartTransaction())
            {
                BlockReference blockRef = tr.GetObject(blockObjectId, OpenMode.ForRead) as BlockReference;
                if (blockRef != null)
                {
                    Extents3d extents = blockRef.GeometricExtents;
                    return point.X >= extents.MinPoint.X && point.X <= extents.MaxPoint.X &&
                           point.Y >= extents.MinPoint.Y && point.Y <= extents.MaxPoint.Y;
                }
                return false;
            }
        }

        /// <summary>
        /// 获取两点确定的角度(弧度)
        /// </summary>
        /// <param name="startPoint">起点</param>
        /// <param name="endPoint">终点</param>
        /// <returns>角度(弧度)</returns>
        public static double GetAngleBetweenTwoPoints(Point3d startPoint, Point3d endPoint)
        {
            return Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X);
        }

        /// <summary>
        /// 获取以基点为中心的两点形成的角度
        /// </summary>
        /// <param name="basePoint">基点</param>
        /// <param name="startPoint">起点</param>
        /// <param name="endPoint">终点</param>
        /// <returns>角度(弧度)</returns>
        public static double GetAngleBetweenThreePoints(Point3d basePoint, Point3d startPoint, Point3d endPoint)
        {
            double angle1 = GetAngleBetweenTwoPoints(basePoint, startPoint);
            double angle2 = GetAngleBetweenTwoPoints(basePoint, endPoint);
            double angle = angle2 - angle1;
            
            if (angle < 0)
            {
                angle += 2 * Math.PI;
            }
            
            return angle;
        }

        /// <summary>
        /// 根据给定范围创建矩形边界
        /// </summary>
        /// <param name="extents">范围</param>
        /// <param name="expansion">扩展距离</param>
        /// <param name="layerName">图层名</param>
        /// <returns>创建的多段线</returns>
        public static Polyline CreateBoundaryFromExtents(Extents3d extents, double expansion = 0, string layerName = "")
        {
            Point2d minPoint = new Point2d(extents.MinPoint.X - expansion, extents.MinPoint.Y - expansion);
            Point2d maxPoint = new Point2d(extents.MaxPoint.X + expansion, extents.MaxPoint.Y + expansion);
            
            Polyline polyline = new Polyline();
            polyline.AddVertexAt(0, minPoint, 0, 0, 0);
            polyline.AddVertexAt(1, new Point2d(maxPoint.X, minPoint.Y), 0, 0, 0);
            polyline.AddVertexAt(2, maxPoint, 0, 0, 0);
            polyline.AddVertexAt(3, new Point2d(minPoint.X, maxPoint.Y), 0, 0, 0);
            polyline.Closed = true;
            
            if (!string.IsNullOrEmpty(layerName))
            {
                polyline.Layer = layerName;
            }
            
            return polyline;
        }

        /// <summary>
        /// 以点为中心创建正方形多段线
        /// </summary>
        /// <param name="centerPoint">中心点</param>
        /// <param name="halfSize">中心到边的距离</param>
        /// <returns>创建的多段线</returns>
        public static Polyline CreateSquareAroundPoint(Point3d centerPoint, double halfSize)
        {
            Point2d minPoint = new Point2d(centerPoint.X - halfSize, centerPoint.Y - halfSize);
            Point2d maxPoint = new Point2d(centerPoint.X + halfSize, centerPoint.Y + halfSize);
            
            Polyline polyline = new Polyline();
            polyline.AddVertexAt(0, minPoint, 0, 0, 0);
            polyline.AddVertexAt(1, new Point2d(maxPoint.X, minPoint.Y), 0, 0, 0);
            polyline.AddVertexAt(2, maxPoint, 0, 0, 0);
            polyline.AddVertexAt(3, new Point2d(minPoint.X, maxPoint.Y), 0, 0, 0);
            polyline.Closed = true;
            
            return polyline;
        }

        /// <summary>
        /// 将Point3d转换为Point2d
        /// </summary>
        /// <param name="point">3D点</param>
        /// <returns>2D点</returns>
        public static Point2d ToPoint2d(this Point3d point)
        {
            return new Point2d(point.X, point.Y);
        }

        /// <summary>
        /// 获取块与多段线的交点
        /// </summary>
        /// <param name="blockObjectId">块参照ObjectId</param>
        /// <param name="polylineObjectId">多段线ObjectId</param>
        /// <returns>交点列表</returns>
        public static List<Point3d> GetIntersectionPointsBetweenBlockAndPolyline(ObjectId blockObjectId, ObjectId polylineObjectId)
        {
            List<Point3d> intersectionPoints = new List<Point3d>();
            
            using (Transaction tr = blockObjectId.Database.TransactionManager.StartTransaction())
            {
                BlockReference blockRef = tr.GetObject(blockObjectId, OpenMode.ForRead) as BlockReference;
                Polyline polyline = tr.GetObject(polylineObjectId, OpenMode.ForRead) as Polyline;
                
                if (blockRef != null && polyline != null)
                {
                    Extents3d blockExtents = blockRef.GeometricExtents;
                    Polyline boundaryPoly = CreateBoundaryFromExtents(blockExtents);
                    
                    Point3dCollection intersections = new Point3dCollection();
                    polyline.IntersectWith(boundaryPoly, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);
                    
                    foreach (Point3d point in intersections)
                    {
                        intersectionPoints.Add(point);
                    }
                }
                
                return intersectionPoints;
            }
        }

        /// <summary>
        /// 判断线是否为水平线(在指定容差内)
        /// </summary>
        /// <param name="startPoint">起点</param>
        /// <param name="endPoint">终点</param>
        /// <param name="tolerance">容差</param>
        /// <returns>是否为水平线</returns>
        public static bool IsLineHorizontal(Point3d startPoint, Point3d endPoint, double tolerance = 1.0)
        {
            double deltaY = Math.Abs(endPoint.Y - startPoint.Y);
            return deltaY <= tolerance;
        }

        /// <summary>
        /// 获取两条线端点相交的角度
        /// </summary>
        /// <param name="line1Start">线1起点</param>
        /// <param name="line1End">线1终点</param>
        /// <param name="line2Start">线2起点</param>
        /// <param name="line2End">线2终点</param>
        /// <returns>交角(弧度)</returns>
        public static double GetIntersectionAngleBetweenLines(Point3d line1Start, Point3d line1End, Point3d line2Start, Point3d line2End)
        {
            double angle1 = GetAngleBetweenTwoPoints(line1Start, line1End);
            double angle2 = GetAngleBetweenTwoPoints(line2Start, line2End);
            double angle = Math.Abs(angle1 - angle2);
            
            // 确保角度在0到PI之间
            if (angle > Math.PI)
            {
                angle = 2 * Math.PI - angle;
            }
            
            return angle;
        }
    }
} 