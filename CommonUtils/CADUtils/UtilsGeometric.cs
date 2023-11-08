using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtils.CADUtils;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using System.Reflection;

namespace CommonUtils.CADUtils
{

    public static class UtilsGeometric
    {
        /// <summary>
        /// get the closest distance from the basePoint to the polyline
        /// </summary>
        /// <param name="point"></param>
        /// <param name="polylineObjectId"></param>
        /// <returns></returns>
        public static double UtilsGetPointToPolylineShortestDistance(Point3d point, ObjectId polylineObjectId)
        {
            // 获get the closest distance from the basePoint to the polyline
            Polyline polyline = polylineObjectId.GetObject(OpenMode.ForRead) as Polyline;
            Point3d closestPoint = polyline.GetClosestPointTo(point, false);
            return point.DistanceTo(closestPoint);
        }

        public static bool UtilsIsPointOnPolyline(Point3d point, ObjectId polylineObjectId, double tolerance)
        {
            return UtilsGetPointToPolylineShortestDistance(point, polylineObjectId) < tolerance;
        }

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

        public static bool UtilsIsPointWithRectangleBlock(Point3d point, ObjectId blockObjectId)
        {
            BlockReference blockRef = blockObjectId.GetObject(OpenMode.ForRead) as BlockReference;
            Extents3d geYuanFrameExtents = blockRef.GeometricExtents;
            // Check if the position of pipeNum is within the extents of the rectangle
            return geYuanFrameExtents.MinPoint.X <= point.X && point.X <= geYuanFrameExtents.MaxPoint.X &&
                geYuanFrameExtents.MinPoint.Y <= point.Y && point.Y <= geYuanFrameExtents.MaxPoint.Y;
        }

        // 完成任务：1）已知直线的两端点A点和B点。1）以A点为原点计算该直线在图纸空间的角度
        public static double UtilsGetAngleByTwoPoint(Point3d startPoint, Point3d endPoint)
        {
            double angle = Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X);
            return angle * (180.0 / Math.PI);
        }

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

        public static Point2d UtilsPoint3dToPoint2d(this Point3d point)
        {
            return new Point2d(point.X, point.Y);
        }

        // 完成任务：1）已知一个块的objectId和一个多段线（lwpolyline）的objectId。2）获取这个块和多段线的交点
        public static List<Point3d> UtilsGetIntersectionPointsByBlockAndPolyLine(ObjectId blockObjectId, ObjectId polylineObjectId)
        {
            Point3dCollection intersectionPoints = new Point3dCollection();
            BlockReference blockRef = blockObjectId.GetObject(OpenMode.ForRead) as BlockReference;
            // the key logic: get the boundary of the block
            Polyline p = UtilsGetBoundary(blockRef.GeometricExtents);
            Polyline polyline = polylineObjectId.GetObject(OpenMode.ForRead) as Polyline;
            // Get the intersection points between the polyline and the boundary of the block
            polyline.IntersectWith(p, Intersect.OnBothOperands, intersectionPoints, IntPtr.Zero, IntPtr.Zero);
            return intersectionPoints.OfType<Point3d>().ToList(); ;
        }

        // 完成任务：已知直线的两个点断，判断其是水平直线还是垂直直线
        public static bool UtilsIsLineHorizontal(Point3d startPoint, Point3d endPoint)
        {
            return startPoint.Y == endPoint.Y;
        }

        public static double UtilsGetVectorAngleByTwoLineEnd(Point3d line1Start, Point3d line1End, Point3d line2Start, Point3d line2End)
        {
            Vector3d direction1 = line1End - line1Start; // 线段1的方向向量
            Vector3d direction2 = line2End - line2Start; // 线段2的方向向量
            if (direction1.Y > 0 && direction1.Y > 0)
            {
                return 0;
            }
            return 0;


            ////double crossProduct = direction1.X * direction2.Y - direction1.Y * direction2.X;
            ////return crossProduct;


            //// Calculate the angle between the vector and the x-axis
            //double angleInRadians = direction1.GetAngleTo(direction2, Vector3d.ZAxis);

            //if (direction1.Y < 0)
            //{
            //    angleInRadians = -angleInRadians;
            //}

            //// Convert the angle to degrees
            //double angleInDegrees = angleInRadians * (180.0 / Math.PI);
            //return angleInDegrees;
        }

    }
}
