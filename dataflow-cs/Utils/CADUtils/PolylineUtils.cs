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
    /// 提供多段线(Polyline)操作的工具类，包括多段线选择、属性修改、交点计算等功能
    /// </summary>
    public static class PolylineUtils
    {
        /// <summary>
        /// 更改多段线的颜色
        /// </summary>
        /// <param name="objectId">多段线的ObjectId</param>
        /// <param name="colorIndex">颜色索引</param>
        public static void ChangeColor(ObjectId objectId, int colorIndex)
        {
            Polyline polyline = objectId.GetObject(OpenMode.ForWrite) as Polyline;
            polyline.ColorIndex = colorIndex;
        }

        /// <summary>
        /// 获取所有的多段线对象
        /// </summary>
        /// <returns>多段线对象列表</returns>
        public static List<Polyline> GetAllObjects()
        {
            SelectionSet selSet = SelectionSetUtils.GetAllPolylineSelectionSet();
            List<Polyline> polylineObjects = new List<Polyline>();

            if (selSet != null)
            {
                // 通过选择集获取所有多段线对象
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
        /// <returns>多段线对象列表</returns>
        public static List<Polyline> GetObjectsBySelect()
        {
            SelectionSet selSet = SelectionSetUtils.GetPolylineSelectionSet();
            List<Polyline> polylineObjects = new List<Polyline>();

            if (selSet != null)
            {
                // 通过选择集获取所有多段线对象
                polylineObjects = selSet.GetObjectIds()
                    .Select(objectId => objectId.GetObject(OpenMode.ForWrite) as Polyline)
                    .Where(polyline => polyline != null)
                    .ToList();
            }
            return polylineObjects;
        }

        /// <summary>
        /// 通过用户选择获取多段线的ObjectId列表
        /// </summary>
        /// <returns>多段线ObjectId列表</returns>
        public static List<ObjectId> GetObjectIdsBySelect()
        {
            SelectionSet selSet = SelectionSetUtils.GetPolylineSelectionSet();
            List<ObjectId> polylineObjectIds = new List<ObjectId>();

            if (selSet != null)
            {
                // 通过选择集获取所有多段线对象的ObjectId
                polylineObjectIds = selSet.GetObjectIds().ToList();
            }
            return polylineObjectIds;
        }

        /// <summary>
        /// 通过用户选择和图层名获取多段线的ObjectId列表
        /// </summary>
        /// <param name="layerName">图层名</param>
        /// <returns>多段线ObjectId列表</returns>
        public static List<ObjectId> GetObjectIdsBySelectByLayerName(string layerName)
        {
            SelectionSet selSet = SelectionSetUtils.GetPolylineSelectionSetByLayerName(layerName);
            List<ObjectId> polylineObjectIds = new List<ObjectId>();

            if (selSet != null)
            {
                // 通过选择集获取所有多段线对象的ObjectId
                polylineObjectIds = selSet.GetObjectIds().ToList();
            }
            return polylineObjectIds;
        }

        /// <summary>
        /// 通过图层名获取所有多段线的ObjectId列表
        /// </summary>
        /// <param name="layerName">图层名</param>
        /// <returns>多段线ObjectId列表</returns>
        public static List<ObjectId> GetAllObjectIdsByLayerName(string layerName)
        {
            SelectionSet selSet = SelectionSetUtils.GetAllPolylineSelectionSetByLayerName(layerName);
            List<ObjectId> polylineObjectIds = new List<ObjectId>();

            if (selSet != null)
            {
                // 通过选择集获取所有多段线对象的ObjectId
                polylineObjectIds = selSet.GetObjectIds().ToList();
            }
            return polylineObjectIds;
        }

        /// <summary>
        /// 通过图层名和范围窗口获取多段线的ObjectId列表
        /// </summary>
        /// <param name="extents">范围</param>
        /// <param name="layerName">图层名</param>
        /// <returns>多段线ObjectId列表</returns>
        public static List<ObjectId> GetAllObjectIdsByLayerNameByCrossingWindow(Extents3d extents, string layerName)
        {
            SelectionSet selSet = SelectionSetUtils.GetAllPolylineSelectionSetByLayerNameByCrossingWindow(extents, layerName);
            List<ObjectId> polylineObjectIds = new List<ObjectId>();

            if (selSet != null)
            {
                // 通过选择集获取所有多段线对象的ObjectId
                polylineObjectIds = selSet.GetObjectIds().ToList();
            }
            return polylineObjectIds;
        }

        /// <summary>
        /// 获取所有多段线的ObjectId列表
        /// </summary>
        /// <returns>多段线ObjectId列表</returns>
        public static List<ObjectId> GetAllPolylineObjectIds()
        {
            SelectionSet selSet = SelectionSetUtils.GetAllPolylineSelectionSet();
            List<ObjectId> polylineObjectIds = new List<ObjectId>();

            if (selSet != null)
            {
                // 通过选择集获取所有多段线对象的ObjectId
                polylineObjectIds = selSet.GetObjectIds().ToList();
            }
            return polylineObjectIds;
        }

        /// <summary>
        /// 计算两条多段线的交点处的夹角
        /// </summary>
        /// <param name="polyline1Id">第一条多段线的ObjectId</param>
        /// <param name="polyline2Id">第二条多段线的ObjectId</param>
        /// <returns>交点处的夹角（弧度）</returns>
        public static double GetIntersectionAngleByTwoPolyLine(ObjectId polyline1Id, ObjectId polyline2Id)
        {
            Polyline polyline1 = polyline1Id.GetObject(OpenMode.ForRead) as Polyline;
            Polyline polyline2 = polyline2Id.GetObject(OpenMode.ForRead) as Polyline;

            if (polyline1 == null || polyline2 == null)
                return 0;

            Point3dCollection intersections = new Point3dCollection();
            polyline1.IntersectWith(polyline2, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);

            if (intersections.Count == 0)
                return 0;

            // 获取第一个交点
            Point3d intersection = intersections[0];

            // 获取两条多段线在交点处的切线方向
            double param1 = polyline1.GetParameterAtPoint(intersection);
            double param2 = polyline2.GetParameterAtPoint(intersection);

            Vector3d direction1 = polyline1.GetFirstDerivative(param1);
            Vector3d direction2 = polyline2.GetFirstDerivative(param2);

            direction1 = direction1.GetNormal();
            direction2 = direction2.GetNormal();

            // 计算夹角
            double angle = direction1.GetAngleTo(direction2);

            return angle;
        }

        /// <summary>
        /// 计算两条多段线在指定交点处的夹角
        /// </summary>
        /// <param name="intersectionPoint">指定的交点</param>
        /// <param name="polyline1Id">第一条多段线的ObjectId</param>
        /// <param name="polyline2Id">第二条多段线的ObjectId</param>
        /// <returns>交点处的夹角（弧度）</returns>
        public static double GetIntersectionAngleByTwoPolyLine(Point3d intersectionPoint, ObjectId polyline1Id, ObjectId polyline2Id)
        {
            Polyline polyline1 = polyline1Id.GetObject(OpenMode.ForRead) as Polyline;
            Polyline polyline2 = polyline2Id.GetObject(OpenMode.ForRead) as Polyline;

            if (polyline1 == null || polyline2 == null)
                return 0;

            // 获取两条多段线在交点处的切线方向
            double param1 = polyline1.GetParameterAtPoint(intersectionPoint);
            double param2 = polyline2.GetParameterAtPoint(intersectionPoint);

            Vector3d direction1 = polyline1.GetFirstDerivative(param1);
            Vector3d direction2 = polyline2.GetFirstDerivative(param2);

            direction1 = direction1.GetNormal();
            direction2 = direction2.GetNormal();

            // 计算夹角
            double angle = direction1.GetAngleTo(direction2);

            return angle;
        }

        /// <summary>
        /// 获取两条多段线的所有交点
        /// </summary>
        /// <param name="polyline1Id">第一条多段线的ObjectId</param>
        /// <param name="polyline2">第二条多段线对象</param>
        /// <returns>交点坐标列表</returns>
        public static List<Point3d> GetIntersectionsByTwoPolyLine(ObjectId polyline1Id, Polyline polyline2)
        {
            List<Point3d> intersectionPoints = new List<Point3d>();
            Polyline polyline1 = polyline1Id.GetObject(OpenMode.ForRead) as Polyline;

            if (polyline1 == null || polyline2 == null)
                return intersectionPoints;

            Point3dCollection intersections = new Point3dCollection();
            polyline1.IntersectWith(polyline2, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);

            // 将交点集合转换为列表
            for (int i = 0; i < intersections.Count; i++)
            {
                intersectionPoints.Add(intersections[i]);
            }

            return intersectionPoints;
        }

        /// <summary>
        /// 获取多段线上交点所在的线段索引
        /// </summary>
        /// <param name="polyline">多段线对象</param>
        /// <param name="intersection">交点坐标</param>
        /// <returns>线段索引</returns>
        public static int GetSegmentIndexAtIntersection(Polyline polyline, Point3d intersection)
        {
            if (polyline == null)
                return -1;

            double param = polyline.GetParameterAtPoint(intersection);
            int segmentIndex = (int)Math.Floor(param);

            return segmentIndex;
        }
    }
} 