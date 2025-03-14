using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;

namespace dataflow_cs.Utils.CADUtils
{
    public static class UtilsPolyline
    {
        public static void UtilsChangeColor(ObjectId objectId, int colorIndex)
        {
            Polyline polyline = objectId.GetObject(OpenMode.ForWrite) as Polyline;
            polyline.ColorIndex = colorIndex;
        }

        /// <summary>
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

        public static List<ObjectId> UtilsGetAllPolylineObjectIds()
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

        public static double UtilsGetIntersectionAngleByTwoPolyLine(ObjectId polyline1Id, ObjectId polyline2Id)
        {
            Polyline polyline1 = polyline1Id.GetObject(OpenMode.ForRead) as Polyline;
            Polyline polyline2 = polyline2Id.GetObject(OpenMode.ForRead) as Polyline;

            Point3dCollection intersectionPoints = new Point3dCollection();

            try
            {
                polyline1.IntersectWith(polyline2, Intersect.OnBothOperands, intersectionPoints, IntPtr.Zero, IntPtr.Zero);
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                if (ex.ErrorStatus == Autodesk.AutoCAD.Runtime.ErrorStatus.NotApplicable)
                {
                    // Handle the eNullExtents exception
                    // You could log the error, fix or recreate the problematic entity, or continue processing other entities
                }
                else
                {
                    //throw;
                    return double.NaN;
                }
            }
            if (intersectionPoints.Count > 0)
            {
                // Get the intersection point
                Point3d intersectionPoint = intersectionPoints[0];
                try
                {
                    double param1 = polyline1.GetParameterAtPoint(intersectionPoint);
                    double param2 = polyline2.GetParameterAtPoint(intersectionPoint);

                    // Check if the parameters are within the valid range
                    if (param1 >= polyline1.StartParam && param1 <= polyline1.EndParam &&
                        param2 >= polyline2.StartParam && param2 <= polyline2.EndParam)
                    {
                        // Get the tangent direction at the intersection point for each polyline
                        Vector3d tangent1 = polyline1.GetFirstDerivative(param1);
                        Vector3d tangent2 = polyline2.GetFirstDerivative(param2);

                        // Calculate the angle between the two tangent directions
                        double angle = tangent1.GetAngleTo(tangent2);

                        return angle * (180.0 / Math.PI);
                    }
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    if (ex.ErrorStatus == Autodesk.AutoCAD.Runtime.ErrorStatus.NotApplicable)
                    {
                        // Handle the eNullExtents exception
                        // You could log the error, fix or recreate the problematic entity, or continue processing other entities
                    }
                    else
                    {
                        //throw;
                        return double.NaN;
                    }
                }
            }
            return double.NaN;
        }

        public static double UtilsGetIntersectionAngleByTwoPolyLine(Point3d intersectionPoint, ObjectId polyline1Id, ObjectId polyline2Id)
        {
            Polyline polyline1 = polyline1Id.GetObject(OpenMode.ForRead) as Polyline;
            Polyline polyline2 = polyline2Id.GetObject(OpenMode.ForRead) as Polyline;

            try
            {
                double param1 = polyline1.GetParameterAtPoint(intersectionPoint);
                double param2 = polyline2.GetParameterAtPoint(intersectionPoint);

                // Check if the parameters are within the valid range
                if (param1 >= polyline1.StartParam && param1 <= polyline1.EndParam &&
                    param2 >= polyline2.StartParam && param2 <= polyline2.EndParam)
                {
                    // Get the tangent direction at the intersection point for each polyline
                    Vector3d tangent1 = polyline1.GetFirstDerivative(param1);
                    Vector3d tangent2 = polyline2.GetFirstDerivative(param2);

                    // Calculate the angle between the two tangent directions
                    double angle = tangent1.GetAngleTo(tangent2);

                    return angle * (180.0 / Math.PI);
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                if (ex.ErrorStatus == Autodesk.AutoCAD.Runtime.ErrorStatus.NotApplicable)
                {
                    // Handle the eNullExtents exception
                    // You could log the error, fix or recreate the problematic entity, or continue processing other entities
                }
                else
                {
                    //throw;
                    return double.NaN;
                }
            }
            return double.NaN;
        }

        public static List<Point3d> UtilsGetIntersectionsByTwoPolyLine(ObjectId polyline1Id, Polyline polyline2)
        {
            Polyline polyline1 = polyline1Id.GetObject(OpenMode.ForRead) as Polyline;

            Point3dCollection intersectionPoints = new Point3dCollection();

            try
            {
                polyline1.IntersectWith(polyline2, Intersect.OnBothOperands, intersectionPoints, IntPtr.Zero, IntPtr.Zero);
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                if (ex.ErrorStatus == Autodesk.AutoCAD.Runtime.ErrorStatus.NotApplicable)
                {
                    // Handle the eNullExtents exception
                    // You could log the error, fix or recreate the problematic entity, or continue processing other entities
                    return null;
                }
                else
                {
                    //throw;
                    return null;
                }
            }
            if (intersectionPoints.Count > 0)
            {
                return intersectionPoints.OfType<Point3d>().ToList();
            }
            return null;
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