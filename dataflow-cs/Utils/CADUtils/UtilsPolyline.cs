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
    /// <summary>
    /// AutoCAD多段线操作工具类
    /// </summary>
    public static class UtilsPolyline
    {
        /// <summary>
        /// 修改多段线的颜色
        /// </summary>
        /// <param name="objectId">多段线的ObjectId</param>
        /// <param name="colorIndex">颜色索引值</param>
        public static void UtilsChangeColor(ObjectId objectId, int colorIndex)
        {
            Polyline polyline = objectId.GetObject(OpenMode.ForWrite) as Polyline;
            polyline.ColorIndex = colorIndex;
        }

        /// <summary>
        /// 获取当前图纸中所有的多段线对象
        /// </summary>
        /// <returns>多段线对象列表</returns>
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

        /// <summary>
        /// 通过用户选择获取多段线对象
        /// </summary>
        /// <returns>用户选择的多段线对象列表</returns>
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

        /// <summary>
        /// 通过用户选择获取多段线的ObjectId
        /// </summary>
        /// <returns>用户选择的多段线ObjectId列表</returns>
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

        /// <summary>
        /// 通过用户选择获取指定图层上的多段线ObjectId
        /// </summary>
        /// <param name="layerName">图层名称</param>
        /// <returns>用户选择的指定图层上的多段线ObjectId列表</returns>
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

        /// <summary>
        /// 获取当前图纸中指定图层上的所有多段线ObjectId
        /// </summary>
        /// <param name="layerName">图层名称</param>
        /// <returns>指定图层上的所有多段线ObjectId列表</returns>
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

        /// <summary>
        /// 在指定范围内通过交叉窗口选择获取指定图层上的多段线ObjectId
        /// </summary>
        /// <param name="extents">选择范围</param>
        /// <param name="layerName">图层名称</param>
        /// <returns>范围内指定图层上的多段线ObjectId列表</returns>
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

        /// <summary>
        /// 获取当前图纸中所有多段线的ObjectId
        /// </summary>
        /// <returns>所有多段线的ObjectId列表</returns>
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

        /// <summary>
        /// 计算两条多段线的交角
        /// </summary>
        /// <param name="polyline1Id">第一条多段线的ObjectId</param>
        /// <param name="polyline2Id">第二条多段线的ObjectId</param>
        /// <returns>交角（度数），如果不相交则返回NaN</returns>
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

        /// <summary>
        /// 根据指定交点计算两条多段线的交角
        /// </summary>
        /// <param name="intersectionPoint">已知的交点</param>
        /// <param name="polyline1Id">第一条多段线的ObjectId</param>
        /// <param name="polyline2Id">第二条多段线的ObjectId</param>
        /// <returns>交角（度数），如果不相交则返回NaN</returns>
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

        /// <summary>
        /// 获取两条多段线的交点
        /// </summary>
        /// <param name="polyline1Id">第一条多段线的ObjectId</param>
        /// <param name="polyline2">第二条多段线对象</param>
        /// <returns>交点列表，如果不相交则返回null</returns>
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
                // 转换成List<Point3d>
                List<Point3d> result = new List<Point3d>();
                foreach (Point3d pt in intersectionPoints)
                {
                    result.Add(pt);
                }
                return result;
            }
            return null;
        }

        /// <summary>
        /// 获取交点所在的多段线段索引
        /// </summary>
        /// <param name="polyline">多段线对象</param>
        /// <param name="intersection">交点</param>
        /// <returns>段索引，如果找不到则返回-1</returns>
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