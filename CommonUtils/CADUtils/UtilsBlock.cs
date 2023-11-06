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

    public static class UtilsBlock
    {
        public static Point3d UtilsGetBlockBasePoint(ObjectId objectId)
        {
            BlockReference blockRef = objectId.GetObject(OpenMode.ForRead) as BlockReference;
            return blockRef.Position;
        }

        public static void UtilsChangeBlockLayerName(ObjectId objectId, string layerName)
        {
            BlockReference blockRef = objectId.GetObject(OpenMode.ForWrite) as BlockReference;
            blockRef.Layer = layerName;
        }

        public static string UtilsGetBlockName(ObjectId objectId)
        {
            BlockReference blockRef = objectId.GetObject(OpenMode.ForRead) as BlockReference;
            if (blockRef != null)
            {
                if (blockRef.IsDynamicBlock)
                {
                    BlockTableRecord btr = blockRef.DynamicBlockTableRecord.GetObject(OpenMode.ForRead) as BlockTableRecord;
                    return btr.Name;
                }
                else
                {
                    return blockRef.Name;
                }
            }
            return string.Empty;

        }

        public static string UtilsGetBlockName(BlockReference blockRef)
        {
            if (blockRef != null)
            {
                if (blockRef.IsDynamicBlock)
                {
                    BlockTableRecord btr = blockRef.DynamicBlockTableRecord.GetObject(OpenMode.ForRead) as BlockTableRecord;
                    return btr.Name;
                }
                else
                {
                    return blockRef.Name;
                }
            }
            return string.Empty;

        }

        /// <summary>
        /// // get all proerty dict list of the block entity
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public static Dictionary<string, string> UtilsGetAllPropertyDictList(ObjectId objectId)
        {
            BlockReference blockRef = objectId.GetObject(OpenMode.ForRead) as BlockReference;
            Dictionary<string, string> blockAttribute = new Dictionary<string, string>();
            // Filter out block entity objects that has no attributes
            if (blockRef.AttributeCollection.Count == 0) return blockAttribute;
            // get the property value of the block entity
            foreach (ObjectId attId in blockRef.AttributeCollection)
            {
                AttributeReference attRef = attId.GetObject(OpenMode.ForRead) as AttributeReference;
                if (attRef != null)
                {
                    blockAttribute.Add(attRef.Tag, attRef.TextString);
                }
            }
            return blockAttribute;
        }

        public static Dictionary<string, string> UtilsGetPropertyDictListByPropertyNameList(ObjectId objectId, List<string> propertyNameList)
        {
            BlockReference blockRef = objectId.GetObject(OpenMode.ForRead) as BlockReference;
            Dictionary<string, string> blockAttribute = new Dictionary<string, string>();
            // filter out block entity objects that has no attributes
            if (blockRef.AttributeCollection.Count == 0) return blockAttribute;
            // get the property value of the block entity
            foreach (ObjectId attId in blockRef.AttributeCollection)
            {
                AttributeReference attRef = attId.GetObject(OpenMode.ForRead) as AttributeReference;
                if (attRef != null && propertyNameList.Any(s => s.Equals(attRef.Tag, StringComparison.OrdinalIgnoreCase)))
                {
                    blockAttribute.Add(attRef.Tag, attRef.TextString);
                }
            }
            return blockAttribute;
        }

        /// <summary>
        /// 根据块的ObjectId和属性名获取属性值
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static string UtilsGetPropertyValueByPropertyName(ObjectId objectId, string propertyName)
        {
            BlockReference blockRef = objectId.GetObject(OpenMode.ForRead) as BlockReference;

            if (blockRef.AttributeCollection.Count == 0) return string.Empty;
            // 获取块实体对象的属性值

            foreach (ObjectId attId in blockRef.AttributeCollection)
            {
                AttributeReference attRef = attId.GetObject(OpenMode.ForRead) as AttributeReference;
                if (attRef != null && string.Equals(attRef.Tag, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    return attRef.TextString;
                }
            }
            return string.Empty;
        }

        public static List<ObjectId> UtilsGetObjectIdsBySelectByBlockName(string blockName)
        {
            // 任务1: 在AutoCAD中获得块实体对象的选择集
            SelectionSet selSet = UtilsSelectionSet.UtilsGetBlockSelectionSet();
            List<ObjectId> blockIds = new List<ObjectId>();

            // 任务2: 根据块实体对象的选择集获取所有块实体对象的所有属性值
            if (selSet != null)
            {
                blockIds = selSet.GetObjectIds()
                    .Select(objectId => objectId.GetObject(OpenMode.ForRead) as BlockReference)
                    .Where(blockRef => blockRef != null && UtilsGetBlockName(blockRef) == blockName)
                    .Select(blockRef => blockRef.ObjectId)
                    .ToList();
            }
            return blockIds;
        }

        // AutoCAD中获得所有块名为{Instrument}的ObjectId
        public static List<ObjectId> UtilsGetAllObjectIdsByBlockName(string blockName)
        {
            // 任务1: 在AutoCAD中获得块实体对象的选择集
            SelectionSet selSet = UtilsSelectionSet.UtilsGetAllBlockSelectionSet();
            List<ObjectId> blockIds = new List<ObjectId>();

            // 任务2: 根据块实体对象的选择集获取所有块实体对象的所有属性值
            if (selSet != null)
            {
                blockIds = selSet.GetObjectIds()
                    .Select(objectId => objectId.GetObject(OpenMode.ForRead) as BlockReference)
                    .Where(blockRef => blockRef != null && UtilsGetBlockName(blockRef) == blockName)
                    .Select(blockRef => blockRef.ObjectId)
                    .ToList();
            }
            return blockIds;
        }

    }
}
