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

    public static class UtilsBlock
    {
        public static  List<ObjectId> UtilsGetBlockObjectIdsBySelectByBlockName(string blockName)
        {
            // 任务1: 在AutoCAD中获得块实体对象的选择集
            SelectionSet selSet = UtilsSelectionSet.UtilsGetBlockSelectionSet();
            List<ObjectId> blockIds = new List<ObjectId>();

            // 任务2: 根据块实体对象的选择集获取所有块实体对象的所有属性值
            if (selSet != null)
            {
                blockIds = selSet.GetObjectIds()
                    .Select(objectId => objectId.GetObject(OpenMode.ForRead) as BlockReference)
                    .Where(blockRef => blockRef != null && blockRef.Name == blockName)
                    .Select(blockRef => blockRef.ObjectId)
                    .ToList();
            }
            return blockIds;
        }

        // AutoCAD中获得所有块名为{Instrument}的ObjectId
        public static List<ObjectId> UtilsGetAllBlockObjectIdsByBlockName(string blockName)
        {
            // 任务1: 在AutoCAD中获得块实体对象的选择集
            SelectionSet selSet = UtilsSelectionSet.UtilsGetAllBlockSelectionSet();
            List<ObjectId> blockIds = new List<ObjectId>();

            // 任务2: 根据块实体对象的选择集获取所有块实体对象的所有属性值
            if (selSet != null)
            {
                blockIds = selSet.GetObjectIds()
                    .Select(objectId => objectId.GetObject(OpenMode.ForRead) as BlockReference)
                    .Where(blockRef => blockRef != null && blockRef.Name == blockName)
                    .Select(blockRef => blockRef.ObjectId)
                    .ToList();
            }
            return blockIds;
        }

    }
}
