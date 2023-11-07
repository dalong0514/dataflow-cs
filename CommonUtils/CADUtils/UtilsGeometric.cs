using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtils.CADUtils;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;

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

        // 完成任务：1）已知一个块的objectId和一个多段线（lwpolyline）的objectId。2）获取这个块和多段线的交点
        public static Point3dCollection UtilsGetIntersectionPointsByBlockAndPolyLine(ObjectId blockObjectId, ObjectId polylineObjectId)
        {
            Point3dCollection intersectionPoints = new Point3dCollection();
            BlockReference blockRef = blockObjectId.GetObject(OpenMode.ForRead) as BlockReference;
            Polyline polyline = polylineObjectId.GetObject(OpenMode.ForRead) as Polyline;
            // Get the intersection points between the polyline and the block
            polyline.IntersectWith(blockRef, Intersect.OnBothOperands, intersectionPoints, IntPtr.Zero, IntPtr.Zero);
            return intersectionPoints;
        }

    }
}
