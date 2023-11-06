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

    }
}
