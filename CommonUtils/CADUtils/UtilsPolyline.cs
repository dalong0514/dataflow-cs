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

    public static class UtilsPolyline
    {
        public static void UtilsChangeColor(ObjectId objectId, int colorIndex)
        {
            Polyline polyline = objectId.GetObject(OpenMode.ForWrite) as Polyline;
            polyline.ColorIndex = colorIndex;
        }

        /// 获得所有的多段线对象 <summary>
        /// 获得所有的多段线对象
        /// </summary>
        /// <returns></returns>
        public static List<Polyline> UtilsGetAllObjects()
        {

            SelectionSet selSet = UtilsSelectionSet.UtilsGetAllPolylineSelectionSet();
            List<Polyline> polylineObjects = new List<Polyline>();

            if (selSet != null)
            {
                // 通过选择集获取所有块实体对象的ObjectId
                polylineObjects = selSet.GetObjectIds()
                    .Select(objectId => objectId.GetObject(OpenMode.ForWrite) as Polyline)
                    .Where(polyline => polyline != null)
                    .ToList();
            }
            return polylineObjects;
        }

        public static List<Polyline> UtilsGetObjectsBySelect()
        {

            SelectionSet selSet = UtilsSelectionSet.UtilsGetPolylineSelectionSet();
            List<Polyline> polylineObjects = new List<Polyline>();

            if (selSet != null)
            {
                // 通过选择集获取所有块实体对象的ObjectId
                polylineObjects = selSet.GetObjectIds()
                    .Select(objectId => objectId.GetObject(OpenMode.ForWrite) as Polyline)
                    .Where(polyline => polyline != null)
                    .ToList();
            }
            return polylineObjects;
        }

        public static List<ObjectId> UtilsGetObjectIdsBySelect()
        {
            SelectionSet selSet = UtilsSelectionSet.UtilsGetPolylineSelectionSet();
            List<ObjectId> polylineObjectIds = new List<ObjectId>();

            if (selSet != null)
            {
                // 通过选择集获取所有块实体对象的ObjectId
                polylineObjectIds = selSet.GetObjectIds().ToList();
            }
            return polylineObjectIds;
        }

        public static List<ObjectId> UtilsGetObjectIdsBySelectByLayerName(string layerName)
        {
            SelectionSet selSet = UtilsSelectionSet.UtilsGetPolylineSelectionSetByLayerName(layerName);
            List<ObjectId> polylineObjectIds = new List<ObjectId>();

            if (selSet != null)
            {
                // 通过选择集获取所有块实体对象的ObjectId
                polylineObjectIds = selSet.GetObjectIds().ToList();
            }
            return polylineObjectIds;
        }

        public static List<ObjectId> UtilsGetAllObjectIdsByLayerName(string layerName)
        {
            SelectionSet selSet = UtilsSelectionSet.UtilsGetAllPolylineSelectionSetByLayerName(layerName);
            List<ObjectId> polylineObjectIds = new List<ObjectId>();

            if (selSet != null)
            {
                // 通过选择集获取所有块实体对象的ObjectId
                polylineObjectIds = selSet.GetObjectIds().ToList();
            }
            return polylineObjectIds;
        }

        public static List<ObjectId> UtilsGetAllObjectIdsByLayerNameByCrossingWindow(Extents3d extents, string layerName)
        {
            SelectionSet selSet = UtilsSelectionSet.UtilsGetAllPolylineSelectionSetByLayerNameByCrossingWindow(extents, layerName);
            List<ObjectId> polylineObjectIds = new List<ObjectId>();

            if (selSet != null)
            {
                // 通过选择集获取所有块实体对象的ObjectId
                polylineObjectIds = selSet.GetObjectIds().ToList();
            }
            return polylineObjectIds;
        }

        public static List<ObjectId> UtilsGetAllObjectIds()
        {
            SelectionSet selSet = UtilsSelectionSet.UtilsGetAllPolylineSelectionSet();
            List<ObjectId> polylineObjectIds = new List<ObjectId>();

            if (selSet != null)
            {
                // 通过选择集获取所有块实体对象的ObjectId
                polylineObjectIds = selSet.GetObjectIds().ToList();
            }
            return polylineObjectIds;
        }

        public static double GetTwoPolyLineIntersectionAngle(ObjectId polylineId1, ObjectId polylineId2)
        {
            double angle = 0.0;

            Polyline polyline1 = polylineId1.GetObject(OpenMode.ForRead) as Polyline;
            Polyline polyline2 = polylineId2.GetObject(OpenMode.ForRead) as Polyline;

            if (polyline1 != null && polyline2 != null)
            {
                // Find the intersection point(s)
                Point3dCollection intersections = new Point3dCollection();
                polyline1.IntersectWith(polyline2, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);

                if (intersections.Count > 0)
                {
                    // Get the lines near the intersection point
                    int index1 = UtilsGetSegmentIndexAtIntersection(polyline1, intersections[0]);
                    int index2 = UtilsGetSegmentIndexAtIntersection(polyline2, intersections[0]);

                    LineSegment2d line1 = polyline1.GetLineSegment2dAt(index1);
                    LineSegment2d line2 = polyline2.GetLineSegment2dAt(index2);

                    // Calculate the angle between the lines
                    Vector2d vector1 = line1.EndPoint - line1.StartPoint;
                    Vector2d vector2 = line2.EndPoint - line2.StartPoint;

                    angle = vector1.GetAngleTo(vector2);
                }
            }

            return angle;
        }

        public static double GetTwoPolyLineIntersectionAngleInDegrees(ObjectId polylineId1, ObjectId polylineId2)
        {
            return GetTwoPolyLineIntersectionAngle(polylineId1, polylineId2) * (180.0 / Math.PI);
        }

        public static int UtilsGetSegmentIndexAtIntersection(Polyline polyline, Point3d intersection)
        {
            int index = -1;

            for (int i = 0; i < polyline.NumberOfVertices; i++)
            {
                LineSegment2d segment = polyline.GetLineSegment2dAt(i);

                if (segment.IsOn(intersection.Convert2d(new Plane())))
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

    }
}
