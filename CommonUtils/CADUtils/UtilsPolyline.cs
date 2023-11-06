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

    }
}
