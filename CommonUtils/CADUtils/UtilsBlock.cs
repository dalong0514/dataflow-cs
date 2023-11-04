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
        public static Point3d UtilsBlockGetBlockBasePoint(ObjectId objectId)
        {
            BlockReference blockRef = objectId.GetObject(OpenMode.ForRead) as BlockReference;
            return blockRef.Position;
        }

        public static string UtilsBlockGetBlockName(ObjectId objectId)
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

        public static string UtilsBlockGetBlockName(BlockReference blockRef)
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
        /// 获得块的属性值字典对象
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public static List<Dictionary<string, string>> UtilsBlockGetPropertyDictList(ObjectId objectId)
        {
            // 获取块选择集中的所有块实体对象的属性值
            List<Dictionary<string, string>> blockAttributes = new List<Dictionary<string, string>>();
            BlockReference blockRef = objectId.GetObject(OpenMode.ForRead) as BlockReference;
            // 过滤掉没有属性的块实体对象
            if (blockRef.AttributeCollection.Count == 0) return blockAttributes;
            // 获取块实体对象的属性值
            Dictionary<string, string> blockAttribute = new Dictionary<string, string>();

            foreach (ObjectId attId in blockRef.AttributeCollection)
            {
                AttributeReference attRef = attId.GetObject(OpenMode.ForRead) as AttributeReference;
                if (attRef != null)
                {
                    blockAttribute.Add(attRef.Tag, attRef.TextString);
                }
            }
            blockAttributes.Add(blockAttribute);
            return blockAttributes;
        }

        /// <summary>
        /// 根据块的ObjectId和属性名获取属性值
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static string UtilsBlockGetPropertyValueByPropertyName(ObjectId objectId, string propertyName)
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

        public static List<ObjectId> UtilsBlockGetObjectIdsBySelectByBlockName(string blockName)
        {
            // 任务1: 在AutoCAD中获得块实体对象的选择集
            SelectionSet selSet = UtilsSelectionSet.UtilsGetBlockSelectionSet();
            List<ObjectId> blockIds = new List<ObjectId>();

            // 任务2: 根据块实体对象的选择集获取所有块实体对象的所有属性值
            if (selSet != null)
            {
                blockIds = selSet.GetObjectIds()
                    .Select(objectId => objectId.GetObject(OpenMode.ForRead) as BlockReference)
                    .Where(blockRef => blockRef != null && UtilsBlockGetBlockName(blockRef) == blockName)
                    .Select(blockRef => blockRef.ObjectId)
                    .ToList();
            }
            return blockIds;
        }

        // AutoCAD中获得所有块名为{Instrument}的ObjectId
        public static List<ObjectId> UtilsBlockGetAllObjectIdsByBlockName(string blockName)
        {
            // 任务1: 在AutoCAD中获得块实体对象的选择集
            SelectionSet selSet = UtilsSelectionSet.UtilsGetAllBlockSelectionSet();
            List<ObjectId> blockIds = new List<ObjectId>();

            // 任务2: 根据块实体对象的选择集获取所有块实体对象的所有属性值
            if (selSet != null)
            {
                blockIds = selSet.GetObjectIds()
                    .Select(objectId => objectId.GetObject(OpenMode.ForRead) as BlockReference)
                    .Where(blockRef => blockRef != null && UtilsBlockGetBlockName(blockRef) == blockName)
                    .Select(blockRef => blockRef.ObjectId)
                    .ToList();
            }
            return blockIds;
        }

    }
}
