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
        /// 一个点到多段线的最短距离 <summary>
        /// 一个点到多段线的最短距离
        /// </summary>
        /// <param name="point"></param>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public static double UtilsGetPointToPolylineShortestDistance(Point3d point, Polyline polyline)
        {
            // 获得 basePoint 与该多段线的最近点的距离
            Point3d closestPoint = polyline.GetClosestPointTo(point, false);
            return point.DistanceTo(closestPoint);
        }

    }
}
