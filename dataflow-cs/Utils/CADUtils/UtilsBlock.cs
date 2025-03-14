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
            if (blockRef == null)
            {
                return string.Empty;
            }

            ObjectId blockId = blockRef.IsDynamicBlock ? blockRef.DynamicBlockTableRecord : blockRef.BlockTableRecord;

            if (!blockId.IsValid || blockId.IsErased)
            {
                return string.Empty;
            }

            BlockTableRecord btr = blockId.GetObject(OpenMode.ForRead) as BlockTableRecord;
            return btr?.Name ?? string.Empty;
        }

        public static string UtilsGetBlockLayer(ObjectId objectId)
        {
            BlockReference blockRef = objectId.GetObject(OpenMode.ForRead) as BlockReference;
            if (blockRef == null)
            {
                return string.Empty;
            }

            ObjectId blockId = blockRef.IsDynamicBlock ? blockRef.DynamicBlockTableRecord : blockRef.BlockTableRecord;

            if (!blockId.IsValid || blockId.IsErased)
            {
                return string.Empty;
            }

            return blockRef?.Layer ?? string.Empty;
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

            if (blockRef == null || blockRef.AttributeCollection.Count == 0) return string.Empty;
            // get property value of the block entity
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

        public static void UtilsSetPropertyValueByPropertyName(ObjectId objectId, string propertyName, string propertyValue)
        {
            BlockReference blockRef = objectId.GetObject(OpenMode.ForRead) as BlockReference;

            if (blockRef == null || blockRef.AttributeCollection.Count == 0) return;
            // set property value of the block entity
            foreach (ObjectId attId in blockRef.AttributeCollection)
            {
                AttributeReference attRef = attId.GetObject(OpenMode.ForRead) as AttributeReference;
                if (attRef != null && string.Equals(attRef.Tag, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    // Upgrade the attribute reference to allow modification in the model of ForRead
                    attRef.UpgradeOpen();
                    attRef.TextString = propertyValue;
                    // Downgrade the attribute reference to prevent further modifications
                    attRef.DowngradeOpen();
                }
            }
        }

        public static void UtilsSetPropertyValueByDictData(ObjectId objectId, Dictionary<string, string> propertyDict)
        {
            using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {
                BlockReference blockRef = objectId.GetObject(OpenMode.ForRead) as BlockReference;

                if (blockRef == null || blockRef.AttributeCollection.Count == 0) return;

                // 获取块引用所在的图层
                LayerTableRecord blockLayer = (LayerTableRecord)blockRef.LayerId.GetObject(OpenMode.ForRead);
                
                // 检查并解锁块引用所在的图层
                if (blockLayer.IsLocked)
                {
                    blockLayer.UpgradeOpen();
                    blockLayer.IsLocked = false;
                    blockLayer.DowngradeOpen();
                }

                // 设置属性值
                foreach (ObjectId attId in blockRef.AttributeCollection)
                {
                    AttributeReference attRef = attId.GetObject(OpenMode.ForRead) as AttributeReference;

                    // 获取属性引用所在的图层
                    LayerTableRecord attLayer = (LayerTableRecord)attRef.LayerId.GetObject(OpenMode.ForRead);
                    
                    // 检查并解锁属性引用所在的图层
                    if (attLayer.IsLocked)
                    {
                        attLayer.UpgradeOpen();
                        attLayer.IsLocked = false;
                        attLayer.DowngradeOpen();
                    }

                    foreach (var item in propertyDict)
                    {
                        if (attRef != null && string.Equals(attRef.Tag, item.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                // 升级属性引用以允许修改
                                attRef.UpgradeOpen();
                                attRef.TextString = item.Value;
                                // 降级属性引用以防止进一步修改
                                attRef.DowngradeOpen();
                            }
                            catch (Exception ex)
                            {
                                // 记录错误信息
                                UtilsCADActive.Editor.WriteMessage($"\nError setting property {item.Key}: {ex.Message}");
                            }
                        }
                    }
                }

                tr.Commit();
            }
        }

        public static void UtilsSetDynamicPropertyValueByDictData(ObjectId objectId, Dictionary<string, string> propertyDict)
        {
            BlockReference blockRef = objectId.GetObject(OpenMode.ForRead) as BlockReference;

            if (blockRef == null || blockRef.DynamicBlockTableRecord == ObjectId.Null) return;

            // Get dynamic block properties
            DynamicBlockReferencePropertyCollection props = blockRef.DynamicBlockReferencePropertyCollection;

            // set property value of the dynamic block entity
            foreach (var item in propertyDict)
            {
                // 2024-09-13
                // 使用 LINQ 的 FirstOrDefault 方法来查找匹配的属性，如果找不到则返回 null
                // 在尝试修改属性之前，先检查属性是否存在
                DynamicBlockReferenceProperty prop = props.Cast<DynamicBlockReferenceProperty>()
                    .FirstOrDefault(p => string.Equals(p.PropertyName, item.Key, StringComparison.OrdinalIgnoreCase));

                if (prop != null)
                {
                    try
                    {
                        // Upgrade the block reference to allow modification
                        blockRef.UpgradeOpen();
                        // Set the property value
                        prop.Value = Convert.ChangeType(item.Value, prop.Value.GetType());
                    }
                    catch (Exception ex)
                    {
                        // Log the error or handle it as appropriate
                        System.Diagnostics.Debug.WriteLine($"Error setting property {item.Key}: {ex.Message}");
                    }
                    finally
                    {
                        // Ensure the block reference is downgraded
                        if (blockRef.IsWriteEnabled)
                        {
                            blockRef.DowngradeOpen();
                        }
                    }
                }
                else
                {
                    // Log that the property was not found
                    System.Diagnostics.Debug.WriteLine($"Property {item.Key} not found in the dynamic block.");
                }
            }
        }

        public static List<ObjectId> UtilsGetObjectIdsBySelectByBlockName(string blockName, bool isIdentical = true)
        {
            // 任务1: 在AutoCAD中获得块实体对象的选择集
            SelectionSet selSet = UtilsSelectionSet.UtilsGetBlockSelectionSet();
            List<ObjectId> blockIds = new List<ObjectId>();

            // 任务2: 根据块实体对象的选择集获取所有块实体对象的所有属性值
            if (selSet != null)
            {
                if (isIdentical)
                {
                    blockIds = selSet.GetObjectIds()
                        .Where(ObjectId => ObjectId != null && UtilsGetBlockName(ObjectId) == blockName)
                        .ToList();
                }
                else
                {
                    blockIds = selSet.GetObjectIds()
                        .Where(ObjectId => ObjectId != null && UtilsGetBlockName(ObjectId).Contains(blockName))
                        .ToList();
                }
            }
            return blockIds;
        }

        public static List<ObjectId> UtilsGetAllBlockObjectIds()
        {
            SelectionSet selSet = UtilsSelectionSet.UtilsGetAllBlockSelectionSet();
            return selSet.GetObjectIds().ToList();
        }

        // AutoCAD中获得所有块名为{Instrument}的ObjectId
        public static List<ObjectId> UtilsGetAllObjectIdsByBlockName(string blockName, bool isIdentical = true)
        {
            // 任务1: 在AutoCAD中获得块实体对象的选择集
            SelectionSet selSet = UtilsSelectionSet.UtilsGetAllBlockSelectionSet();
            List<ObjectId> blockIds = new List<ObjectId>();

            // 任务2: 根据块实体对象的选择集获取所有块实体对象的所有属性值
            if (selSet != null)
            {
                if (isIdentical)
                {
                    blockIds = selSet.GetObjectIds()
                        .Where(objectId => objectId != null && UtilsGetBlockName(objectId) == blockName)
                        .ToList();
                }
                else
                {
                    blockIds = selSet.GetObjectIds()
                        .Where(objectId => objectId != null && UtilsGetBlockName(objectId).Contains(blockName))
                        .ToList();
                }
            }
            return blockIds;
        }

        public static List<ObjectId> UtilsGetAllObjectIdsByBlockName(List<ObjectId> blockIds, string blockName, bool isIdentical = true)
        {
            // 任务2: 根据块实体对象的选择集获取所有块实体对象的所有属性值
            if (blockIds != null)
            {
                if (isIdentical)
                {
                    blockIds = blockIds.Where(objectId => UtilsGetBlockName(objectId) == blockName)
                        .ToList();
                }
                else
                {
                    blockIds = blockIds.Where(objectId => UtilsGetBlockName(objectId).Contains(blockName))
                        .ToList();
                }
            }
            return blockIds;
        }

        public static Dictionary<string, List<ObjectId>> UtilsGetAllObjectIdsGroupsByBlockNameList(List<ObjectId> blockIds, List<string> blockNameList, bool isIdentical = true)
        {
            Dictionary<string, List<ObjectId>> objectIdsGroups = new Dictionary<string, List<ObjectId>>();

            // 初始化字典，为每个块名创建一个空列表
            foreach (string blockName in blockNameList)
            {
                objectIdsGroups[blockName] = new List<ObjectId>();
            }

            // 检查blockIds是否为空
            if (blockIds == null || blockIds.Count == 0)
            {
                return objectIdsGroups;
            }

            // 遍历所有的ObjectId
            foreach (ObjectId objectId in blockIds)
            {
                if (objectId == null) continue;

                string currentBlockName = UtilsGetBlockName(objectId);

                // 根据isIdentical标志，进行精确匹配或包含匹配
                foreach (var pair in objectIdsGroups)
                {
                    bool match = isIdentical ? currentBlockName == pair.Key : currentBlockName.Contains(pair.Key);

                    if (match)
                    {
                        pair.Value.Add(objectId);
                    }
                }
            }

            // 删除没有匹配到ObjectId的块名
            // 2023-11-29 无需删除，因为在初始化字典时，已经为每个块名创建了一个空列表。删了反而会出错，比如图纸里无格原图签的情况
            //objectIdsGroups = objectIdsGroups.Where(pair => pair.Value.Count > 0).ToDictionary(pair => pair.Key, pair => pair.Value);

            return objectIdsGroups;
        }

        public static List<ObjectId> UtilsGetAllObjectIdsByBlockNameByCrossingWindow(Extents3d extents, string blockName, bool isIdentical = true)
        {
            // 任务1: 在AutoCAD中获得块实体对象的选择集
            SelectionSet selSet = UtilsSelectionSet.UtilsGetAllBlockSelectionSetByCrossingWindow(extents);
            List<ObjectId> blockIds = new List<ObjectId>();

            // 任务2: 根据块实体对象的选择集获取所有块实体对象的所有属性值
            if (selSet != null)
            {
                if (isIdentical)
                {
                    blockIds = selSet.GetObjectIds()
                        .Where(objectId => objectId != null && UtilsGetBlockName(objectId) == blockName)
                        .ToList();
                }
                else
                {
                    blockIds = selSet.GetObjectIds()
                        .Where(objectId => objectId != null && UtilsGetBlockName(objectId).Contains(blockName))
                        .ToList();
                }
            }
            return blockIds;
        }

        public static double UtilsGetBlockRotaton(ObjectId objectId)
        {
            BlockReference blockRef = objectId.GetObject(OpenMode.ForRead) as BlockReference;

            if (blockRef == null ) return 0.0;
            return blockRef.Rotation;
        }

        public static double UtilsGetBlockRotatonInDegrees(ObjectId objectId)
        {
            BlockReference blockRef = objectId.GetObject(OpenMode.ForRead) as BlockReference;

            if (blockRef == null) return 0.0;
            return blockRef.Rotation * (180.0 / Math.PI);
        }

        public static void UtilsSetBlockRotaton(ObjectId objectId, double rotation)
        {
            BlockReference blockRef = objectId.GetObject(OpenMode.ForWrite) as BlockReference;

            if (blockRef == null) return;
            blockRef.Rotation = rotation;
        }

        public static void UtilsSetBlockRotatonInDegrees(ObjectId objectId, double rotationDegrees)
        {
            BlockReference blockRef = objectId.GetObject(OpenMode.ForWrite) as BlockReference;

            if (blockRef == null) return;
            blockRef.Rotation = rotationDegrees * (Math.PI / 180.0);
        }

        public static void UtilsSetBlockXYScale(ObjectId objectId, double xScale, double yScale)
        {
            BlockReference blockRef = objectId.GetObject(OpenMode.ForWrite) as BlockReference;

            if (blockRef == null) return;
            blockRef.ScaleFactors = new Scale3d(xScale, yScale, 1.0);
        }
    }
}
