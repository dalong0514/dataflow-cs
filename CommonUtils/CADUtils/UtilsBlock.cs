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

        public static  List<ObjectId> UtilsGetBlockBySelectByBlockName(string blockName)
        {
            List<ObjectId> blockIds = new List<ObjectId>();
            // 任务1: 在AutoCAD中获得块实体对象的选择集
            SelectionSet selSet = UtilsSelectionSet.UtilsGetBlockSelectionSet();

            // 任务2: 根据块实体对象的选择集获取所有块实体对象的所有属性值
            if (selSet != null)
            {
                foreach (ObjectId objectId in selSet.GetObjectIds())
                {
                    // get the block reference
                    BlockReference blockRef = objectId.GetObject(OpenMode.ForRead) as BlockReference;
                    if (blockRef != null && blockRef.Name == blockName)
                    {
                        blockIds.Add(objectId);
                    }
                }
            }
            return blockIds;
        }

        // AutoCAD中获得所有块名为{Instrument}的ObjectId
        public static List<ObjectId> UtilsGetAllBlockIdsByBlockName(string blockName)
        {
            List<ObjectId> blockIds = new List<ObjectId>();

            // 任务1: 在AutoCAD中获得块实体对象的选择集
            SelectionSet selSet = UtilsSelectionSet.UtilsGetAllBlockSelectionSet();

            // 任务2: 根据块实体对象的选择集获取所有块实体对象的所有属性值
            if (selSet != null)
            {
                foreach (ObjectId objectId in selSet.GetObjectIds())
                {
                    // get the block reference
                    BlockReference blockRef = objectId.GetObject(OpenMode.ForRead) as BlockReference;
                    if (blockRef != null && blockRef.Name == blockName)
                    {
                        blockIds.Add(objectId);
                    }
                }
                
            }
            return blockIds;
        }

    }
}
