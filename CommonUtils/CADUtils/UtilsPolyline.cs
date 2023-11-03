using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtils.CADUtils;
using Autodesk.AutoCAD.ApplicationServices;

namespace CommonUtils.CADUtils
{

    public static class UtilsPolyline
    {

        /// 获得所有的多段线对象 <summary>
        /// 获得所有的多段线对象
        /// </summary>
        /// <returns></returns>
        public static List<Polyline> UtilsGetAllPolylineObjects()
        {

            // 任务1: 在AutoCAD中获得块实体对象的选择集
            SelectionSet selSet = UtilsSelectionSet.UtilsGetPolylineSelectionSet();
            List<Polyline> polylineObjects = new List<Polyline>();

            // 任务2: 根据块实体对象的选择集获取所有块实体对象的所有属性值
            if (selSet != null)
            {
                // 通过选择集获取所有块实体对象的ObjectId
                polylineObjects = selSet.GetObjectIds()
                    .Select(objectId => objectId.GetObject(OpenMode.ForRead) as Polyline)
                    .Where(polyline => polyline != null)
                    .ToList();
            }
            return polylineObjects;
        }

        public static List<ObjectId> UtilsGetAllPolylineObjectIds()
        {

            // 任务1: 在AutoCAD中获得块实体对象的选择集
            SelectionSet selSet = UtilsSelectionSet.UtilsGetPolylineSelectionSet();
            List<ObjectId> polylineObjectIds = new List<ObjectId>();

            // 任务2: 根据块实体对象的选择集获取所有块实体对象的所有属性值
            if (selSet != null)
            {
                // 通过选择集获取所有块实体对象的ObjectId
                polylineObjectIds = selSet.GetObjectIds()
                    .Select(objectId => objectId.GetObject(OpenMode.ForRead) as Polyline)
                    .Where(polyline => polyline != null)
                    .Select(polyline => polyline.ObjectId)
                    .ToList();
            }
            return polylineObjectIds;
        }

    }
}
