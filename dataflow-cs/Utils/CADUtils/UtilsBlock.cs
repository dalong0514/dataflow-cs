using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using Newtonsoft.Json.Linq;

namespace dataflow_cs.Utils.CADUtils
{
    public static class UtilsBlock
    {
        /// <summary>
        /// 获取块的基准点坐标
        /// </summary>
        /// <param name="objectId">块的ObjectId</param>
        /// <returns>块的基准点坐标</returns>
        public static Point3d UtilsGetBlockBasePoint(ObjectId objectId)
        {
            BlockReference blockRef = objectId.GetObject(OpenMode.ForRead) as BlockReference;
            return blockRef.Position;
        }

        /// <summary>
        /// 修改块的图层名称
        /// </summary>
        /// <param name="objectId">块的ObjectId</param>
        /// <param name="layerName">新的图层名称</param>
        public static void UtilsChangeBlockLayerName(ObjectId objectId, string layerName)
        {
            BlockReference blockRef = objectId.GetObject(OpenMode.ForWrite) as BlockReference;
            blockRef.Layer = layerName;
        }

        /// <summary>
        /// 获取块的名称
        /// </summary>
        /// <param name="objectId">块的ObjectId</param>
        /// <returns>块的名称，如果块无效则返回空字符串</returns>
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

        /// <summary>
        /// 获取块所在的图层名称
        /// </summary>
        /// <param name="objectId">块的ObjectId</param>
        /// <returns>块所在的图层名称，如果块无效则返回空字符串</returns>
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
        /// 获取块实体的所有属性键值对
        /// </summary>
        /// <param name="objectId">块的ObjectId</param>
        /// <returns>包含块所有属性的键值对字典</returns>
        public static Dictionary<string, string> UtilsGetAllPropertyDictList(ObjectId objectId, bool isKeyCaseSensitive = true)
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
                    if (isKeyCaseSensitive)
                    {
                        blockAttribute.Add(attRef.Tag, attRef.TextString);
                    }
                    else
                    {
                        blockAttribute.Add(attRef.Tag.ToLower(), attRef.TextString);
                    }
                }
            }
            return blockAttribute;
        }

        /// <summary>
        /// 根据属性名列表获取块的指定属性键值对
        /// </summary>
        /// <param name="objectId">块的ObjectId</param>
        /// <param name="propertyNameList">要获取的属性名列表</param>
        /// <returns>包含指定属性的键值对字典</returns>
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
        /// <param name="objectId">块的ObjectId</param>
        /// <param name="propertyName">要获取的属性名</param>
        /// <returns>属性值，如果未找到或块无效则返回空字符串</returns>
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

        /// <summary>
        /// 根据属性名设置块的属性值
        /// </summary>
        /// <param name="objectId">块的ObjectId</param>
        /// <param name="propertyName">要设置的属性名</param>
        /// <param name="propertyValue">要设置的属性值</param>
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

        /// <summary>
        /// 根据字典数据设置块的多个属性值，自动处理图层锁定问题
        /// </summary>
        /// <param name="objectId">块的ObjectId</param>
        /// <param name="propertyDict">包含属性名和属性值的字典</param>
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

        /// <summary>
        /// 根据字典数据设置动态块的属性值
        /// </summary>
        /// <param name="objectId">动态块的ObjectId</param>
        /// <param name="propertyDict">包含属性名和属性值的字典</param>
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

        /// <summary>
        /// 通过用户选择获取指定名称的块的ObjectId列表
        /// </summary>
        /// <param name="blockName">块名称</param>
        /// <param name="isIdentical">是否完全匹配，true为完全匹配，false为包含匹配</param>
        /// <returns>匹配的块ObjectId列表</returns>
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

        /// <summary>
        /// 获取当前图纸中所有块的ObjectId列表
        /// </summary>
        /// <returns>所有块的ObjectId列表</returns>
        public static List<ObjectId> UtilsGetAllBlockObjectIds()
        {
            SelectionSet selSet = UtilsSelectionSet.UtilsGetAllBlockSelectionSet();
            return selSet.GetObjectIds().ToList();
        }

        /// <summary>
        /// 获取当前图纸中指定名称的所有块的ObjectId列表
        /// </summary>
        /// <param name="blockName">块名称</param>
        /// <param name="isIdentical">是否完全匹配，true为完全匹配，false为包含匹配</param>
        /// <returns>匹配的块ObjectId列表</returns>
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
        /// <summary>
        /// 获取当前图纸中指定名称的所有块的ObjectId列表
        /// </summary>
        /// <param name="blockNameList">块名称列表</param>
        /// <param name="isIdentical">是否完全匹配，true为完全匹配，false为包含匹配</param>
        /// <returns>匹配的块ObjectId列表</returns>
        public static List<ObjectId> UtilsGetAllObjectIdsByBlockName(List<string> blockNameList, bool isIdentical = true)
        {
            // 任务1: 在AutoCAD中获得块实体对象的选择集
            SelectionSet selSet = UtilsSelectionSet.UtilsGetAllBlockSelectionSet();
            List<ObjectId> blockIds = new List<ObjectId>();

            // 任务2: 根据块实体对象的选择集获取所有块实体对象的所有属性值
            if (selSet != null)
            {
                foreach (string blockName in blockNameList)
                {
                    blockIds.AddRange(UtilsGetAllObjectIdsByBlockName(blockName, isIdentical));
                }
            }
            return blockIds.Distinct().ToList(); // 返回去重后的结果
        }

        /// <summary>
        /// 从指定的块ObjectId列表中筛选出指定名称的块
        /// </summary>
        /// <param name="blockIds">要筛选的块ObjectId列表</param>
        /// <param name="blockName">要筛选的块名称</param>
        /// <param name="isIdentical">是否完全匹配，true为完全匹配，false为包含匹配</param>
        /// <returns>匹配的块ObjectId列表</returns>
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

        /// <summary>
        /// 根据块名称列表获取匹配的所有块的ObjectId列表
        /// </summary>
        /// <param name="blockNameList">块名称列表</param>
        /// <param name="isIdentical">是否完全匹配，true为完全匹配，false为包含匹配</param>
        /// <returns>匹配的块ObjectId列表（已去重）</returns>
        public static List<ObjectId> UtilsGetAllObjectIdsByBlockNameList(List<string> blockNameList, bool isIdentical = true)
        {
            // 任务1: 在AutoCAD中获得块实体对象的选择集
            List<ObjectId> allBlockIds = UtilsGetAllBlockObjectIds();
            List<ObjectId> resultBlockIds = new List<ObjectId>();

            // 遍历块名列表，获取每个块名对应的ObjectId
            foreach (string blockName in blockNameList)
            {
                var blockIds = UtilsGetAllObjectIdsByBlockName(allBlockIds, blockName, isIdentical);
                resultBlockIds.AddRange(blockIds);
            }

            return resultBlockIds.Distinct().ToList(); // 去重并返回
        }


        /// <summary>
        /// 根据块名称列表将指定的块ObjectId分组
        /// </summary>
        /// <param name="blockIds">要分组的块ObjectId列表</param>
        /// <param name="blockNameList">块名称列表</param>
        /// <param name="isIdentical">是否完全匹配，true为完全匹配，false为包含匹配</param>
        /// <returns>按块名称分组的ObjectId字典</returns>
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

        /// <summary>
        /// 在指定范围内通过交叉窗口选择获取指定名称的块的ObjectId列表
        /// </summary>
        /// <param name="extents">选择范围</param>
        /// <param name="blockName">块名称</param>
        /// <param name="isIdentical">是否完全匹配，true为完全匹配，false为包含匹配</param>
        /// <returns>匹配的块ObjectId列表</returns>
        public static List<ObjectId> UtilsGetAllObjectIdsByBlockNameByCrossingWindow(Extents3d extents, string blockName, bool isIdentical = true)
        {
            try
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
            catch (Exception ex)
            {
                // 记录详细错误信息
                System.Diagnostics.Debug.WriteLine($"UtilsGetAllObjectIdsByBlockNameByCrossingWindow 发生错误: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                
                // 重新抛出异常以便上层处理
                throw new Exception($"选择块时发生错误: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取块的旋转角度（弧度值）
        /// </summary>
        /// <param name="objectId">块的ObjectId</param>
        /// <returns>块的旋转角度（弧度值），如果块无效则返回0</returns>
        public static double UtilsGetBlockRotaton(ObjectId objectId)
        {
            BlockReference blockRef = objectId.GetObject(OpenMode.ForRead) as BlockReference;

            if (blockRef == null ) return 0.0;
            return blockRef.Rotation;
        }

        /// <summary>
        /// 获取块的旋转角度（度数值）
        /// </summary>
        /// <param name="objectId">块的ObjectId</param>
        /// <returns>块的旋转角度（度数值），如果块无效则返回0</returns>
        public static double UtilsGetBlockRotatonInDegrees(ObjectId objectId)
        {
            BlockReference blockRef = objectId.GetObject(OpenMode.ForRead) as BlockReference;

            if (blockRef == null) return 0.0;
            return blockRef.Rotation * (180.0 / Math.PI);
        }

        /// <summary>
        /// 设置块的旋转角度（弧度值）
        /// </summary>
        /// <param name="objectId">块的ObjectId</param>
        /// <param name="rotation">要设置的旋转角度（弧度值）</param>
        public static void UtilsSetBlockRotaton(ObjectId objectId, double rotation)
        {
            BlockReference blockRef = objectId.GetObject(OpenMode.ForWrite) as BlockReference;

            if (blockRef == null) return;
            blockRef.Rotation = rotation;
        }

        /// <summary>
        /// 设置块的旋转角度（度数值）
        /// </summary>
        /// <param name="objectId">块的ObjectId</param>
        /// <param name="rotationDegrees">要设置的旋转角度（度数值）</param>
        public static void UtilsSetBlockRotatonInDegrees(ObjectId objectId, double rotationDegrees)
        {
            BlockReference blockRef = objectId.GetObject(OpenMode.ForWrite) as BlockReference;

            if (blockRef == null) return;
            blockRef.Rotation = rotationDegrees * (Math.PI / 180.0);
        }

        /// <summary>
        /// 设置块的X和Y方向的缩放比例
        /// </summary>
        /// <param name="objectId">块的ObjectId</param>
        /// <param name="xScale">X方向的缩放比例</param>
        /// <param name="yScale">Y方向的缩放比例</param>
        public static void UtilsSetBlockXYScale(ObjectId objectId, double xScale, double yScale)
        {
            BlockReference blockRef = objectId.GetObject(OpenMode.ForWrite) as BlockReference;

            if (blockRef == null) return;
            blockRef.ScaleFactors = new Scale3d(xScale, yScale, 1.0);
        }


        /// <summary>
        /// 获取块属性
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <returns>属性字典</returns>
        public static Dictionary<string, string> GetBlockAttrs(BlockReference blockReference)
        {
            return UtilsBlock.UtilsGetAllPropertyDictList(blockReference.ObjectId);
        }

        /// <summary>
        /// 获取块名称
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <returns>块名称</returns>
        public static string GetBlockName(BlockReference blockReference)
        {
            return UtilsBlock.UtilsGetBlockName(blockReference.ObjectId);
        }

        /// <summary>
        /// 获取块属性值
        /// </summary>
        /// <param name="blockReference">块引用</param>
        /// <param name="attributeName">属性名称</param>
        /// <returns>属性值</returns>
        public static string GetBlockAttrValue(BlockReference blockReference, string attributeName)
        {
            var attrs = GetBlockAttrs(blockReference);
            if (attrs.ContainsKey(attributeName))
            {
                return attrs[attributeName];
            }
            return string.Empty;
        }
        
        /// <summary>
        /// 获取图框信息
        /// </summary>
        /// <returns>图框信息</returns>
        public static JObject GetDrawInfo()
        {
            try
            {
                // 获取所有块
                var blockIds = UtilsBlock.UtilsGetAllBlockObjectIds();
                if (blockIds == null || blockIds.Count == 0)
                {
                    return null;
                }
                
                // 查找图框块
                foreach (var blockId in blockIds)
                {
                    string blockName = UtilsBlock.UtilsGetBlockName(blockId);
                    if (blockName.Contains("TitleBlock") || blockName.Contains("图框"))
                    {
                        // 获取图框属性
                        var attrs = UtilsBlock.UtilsGetAllPropertyDictList(blockId);
                        if (attrs != null && attrs.Count > 0)
                        {
                            // 创建图框信息对象
                            JObject drawInfo = new JObject();
                            drawInfo["blockId"] = blockId.ToString();
                            drawInfo["blockName"] = blockName;
                            
                            // 添加图框属性
                            foreach (var attr in attrs)
                            {
                                drawInfo[attr.Key] = attr.Value;
                            }
                            
                            return drawInfo;
                        }
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"获取图框信息时发生错误: {ex.Message}", "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// 在当前空间中插入块参照
        /// </summary>
        /// <param name="blockName">要插入的块名称</param>
        /// <param name="insertionPoint">插入点坐标</param>
        /// <param name="xScale">X方向的缩放比例，默认为1.0</param>
        /// <param name="yScale">Y方向的缩放比例，默认为1.0</param>
        /// <param name="zScale">Z方向的缩放比例，默认为1.0</param>
        /// <param name="rotationAngle">旋转角度（弧度），默认为0.0</param>
        /// <param name="layerName">图层名称，如果不指定则使用当前图层</param>
        /// <param name="transaction">外部传入的事务对象，由调用方负责管理</param>
        /// <returns>新插入的块参照的ObjectId，如果插入失败则返回ObjectId.Null</returns>
        public static ObjectId UtilsInsertBlock(string blockName, Point3d insertionPoint, double xScale = 1.0, double yScale = 1.0, double zScale = 1.0, double rotationAngle = 0.0, string layerName = null, Transaction transaction = null)
        {
            Database db = UtilsCADActive.Database;
            ObjectId blockRefId = ObjectId.Null;
            
            try
            {
                // 检查是否提供了事务对象
                bool externalTransaction = transaction != null;
                Transaction tr = externalTransaction ? transaction : db.TransactionManager.TopTransaction;
                
                if (tr == null)
                {
                    UtilsCADActive.WriteMessage("\n错误：未提供有效的事务对象，且当前没有活动的事务。");
                    return ObjectId.Null;
                }
                
                // 打开块表以供读取
                BlockTable blockTable = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                
                // 检查指定的块名是否存在
                if (!blockTable.Has(blockName))
                {
                    UtilsCADActive.WriteMessage($"\n错误：找不到名为 '{blockName}' 的块定义。");
                    return ObjectId.Null;
                }
                
                // 获取块定义的ObjectId
                ObjectId blockDefId = blockTable[blockName];
                
                // 打开当前空间以供写入
                BlockTableRecord currentSpace = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                
                // 创建新的块参照
                using (BlockReference blockRef = new BlockReference(insertionPoint, blockDefId))
                {
                    // 设置块参照的属性
                    blockRef.ScaleFactors = new Scale3d(xScale, yScale, zScale);
                    blockRef.Rotation = rotationAngle;
                    
                    // 如果指定了图层名，则设置块参照的图层
                    if (!string.IsNullOrEmpty(layerName))
                    {
                        blockRef.Layer = layerName;
                    }
                    
                    // 将块参照添加到当前空间
                    blockRefId = currentSpace.AppendEntity(blockRef);
                    tr.AddNewlyCreatedDBObject(blockRef, true);
                    
                    // 处理块中的属性定义
                    BlockTableRecord blockDef = tr.GetObject(blockDefId, OpenMode.ForRead) as BlockTableRecord;
                    
                    // 检查块定义中是否有属性定义
                    if (blockDef.HasAttributeDefinitions)
                    {
                        // 遍历块定义中的所有对象
                        foreach (ObjectId id in blockDef)
                        {
                            DBObject obj = tr.GetObject(id, OpenMode.ForRead);
                            
                            // 如果对象是属性定义
                            if (obj is AttributeDefinition attDef)
                            {
                                // 创建一个新的属性引用
                                using (AttributeReference attRef = new AttributeReference())
                                {
                                    // 从属性定义中复制属性
                                    attRef.SetAttributeFromBlock(attDef, blockRef.BlockTransform);
                                    attRef.Position = attDef.Position.TransformBy(blockRef.BlockTransform);
                                    attRef.TextString = attDef.TextString; // 使用默认文本
                                    
                                    // 将属性引用添加到块参照中
                                    blockRef.AttributeCollection.AppendAttribute(attRef);
                                    tr.AddNewlyCreatedDBObject(attRef, true);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UtilsCADActive.WriteMessage($"\n插入块时发生错误: {ex.Message}");
                blockRefId = ObjectId.Null;
            }
            
            return blockRefId;
        }

        /// <summary>
        /// 将块参照中的所有直线实体(LINE)转换为多段线实体(LWPOLYLINE)，并设置线宽
        /// </summary>
        /// <param name="blockRefId">块参照的ObjectId</param>
        /// <param name="tr">外部事务对象</param>
        /// <param name="polylineWidth">多段线的线宽，默认为0.0</param>
        /// <returns>转换成功的直线数量</returns>
        public static void UtilsConvertBlockLinesToPolylines(ObjectId blockRefId, Transaction tr, double polylineWidth = 0.0)
        {
            
            try
            {
                // 获取块参照对象
                BlockReference blockRef = tr.GetObject(blockRefId, OpenMode.ForRead) as BlockReference;
                if (blockRef == null)
                {
                    UtilsCADActive.WriteMessage("\n无效的块参照ObjectId。");
                }
                
                // 获取块定义
                BlockTableRecord blockDef = tr.GetObject(blockRef.BlockTableRecord, OpenMode.ForWrite) as BlockTableRecord;
                if (blockDef == null)
                {
                    UtilsCADActive.WriteMessage("\n无法获取块定义。");
                }
                
                // 创建集合以存储要处理的直线对象
                List<ObjectId> lineIds = new List<ObjectId>();
                List<Line> lineEntities = new List<Line>();
                
                // 遍历块定义中的所有实体
                foreach (ObjectId entId in blockDef)
                {
                    Entity entity = tr.GetObject(entId, OpenMode.ForRead) as Entity;
                    if (entity is Line)
                    {
                        lineIds.Add(entId);
                        lineEntities.Add(entity as Line);
                    }
                }
                
                // 如果没有找到直线实体，则返回
                if (lineIds.Count == 0)
                {
                    UtilsCADActive.WriteMessage("\n块中未找到直线实体。");
                }
                
                // 处理每条直线
                for (int i = 0; i < lineIds.Count; i++)
                {
                    Line line = lineEntities[i];
                    
                    // 创建新的多段线
                    Polyline polyline = new Polyline();
                    
                    // 设置多段线的基本属性（从原始直线复制）
                    polyline.Layer = line.Layer;
                    polyline.Color = line.Color;
                    polyline.Linetype = line.Linetype;
                    polyline.LinetypeScale = line.LinetypeScale;
                    
                    // 添加多段线的顶点（转换直线的两个端点）
                    // 为每个端点设置起点和终点宽度
                    polyline.AddVertexAt(0, new Point2d(line.StartPoint.X, line.StartPoint.Y), 0, polylineWidth, polylineWidth);
                    polyline.AddVertexAt(1, new Point2d(line.EndPoint.X, line.EndPoint.Y), 0, polylineWidth, polylineWidth);
                    
                    // 确保设置全局线宽 (这只对均匀宽度的多段线有效)
                    polyline.ConstantWidth = polylineWidth;
                    
                    // 添加新的多段线到块定义
                    blockDef.AppendEntity(polyline);
                    tr.AddNewlyCreatedDBObject(polyline, true);
                    
                    // 删除原始直线
                    Entity lineEntity = tr.GetObject(lineIds[i], OpenMode.ForWrite) as Entity;
                    lineEntity.Erase();
                    
                }
                
                // 更新显示
                UtilsCADActive.Editor.UpdateScreen();
                
            }
            catch (Exception ex)
            {
                UtilsCADActive.WriteMessage($"\n转换直线为多段线时发生错误: {ex.Message}");
            }
            
        }

        /// <summary>
        /// 打散块引用为其组成部分
        /// </summary>
        /// <param name="objectId">块的ObjectId</param>
        /// <param name="transaction">外部传入的事务对象，如果为null则创建新事务</param>
        /// <returns>打散后的实体集合的ObjectId列表</returns>
        public static List<ObjectId> UtilsExplodeBlock(ObjectId objectId, Transaction transaction = null)
        {
            List<ObjectId> explodedEntityIds = new List<ObjectId>();
            Database db = UtilsCADActive.Database;
            
            try
            {
                // 检查是否提供了事务对象
                bool externalTransaction = transaction != null;
                Transaction tr = externalTransaction ? transaction : db.TransactionManager.StartTransaction();
                
                try
                {
                    // 获取块引用对象
                    BlockReference blockRef = tr.GetObject(objectId, OpenMode.ForWrite) as BlockReference;
                    if (blockRef == null)
                    {
                        UtilsCADActive.WriteMessage("\n无效的块引用ObjectId。");
                        return explodedEntityIds;
                    }
                    
                    // 获取块所在的空间（模型空间或图纸空间）
                    BlockTableRecord space = tr.GetObject(blockRef.BlockId, OpenMode.ForWrite) as BlockTableRecord;
                    if (space == null)
                    {
                        space = tr.GetObject(blockRef.OwnerId, OpenMode.ForWrite) as BlockTableRecord;
                        if (space == null)
                        {
                            UtilsCADActive.WriteMessage("\n无法获取块所在的空间。");
                            return explodedEntityIds;
                        }
                    }
                    
                    // 创建一个DBObjectCollection来存储打散后的实体
                    DBObjectCollection explodedObjects = new DBObjectCollection();
                    
                    // 打散块引用
                    blockRef.Explode(explodedObjects);
                    
                    // 添加打散后的实体到空间中
                    foreach (DBObject obj in explodedObjects)
                    {
                        if (obj is Entity entity)
                        {
                            // 设置实体的属性
                            // 由于打散的实体继承了块引用的属性，我们可能不需要手动设置这些属性
                            // 但如果需要，可以在这里设置：
                            // entity.Layer = blockRef.Layer;
                            // entity.Color = blockRef.Color;
                            // 等等
                            
                            // 将实体添加到空间中
                            ObjectId entityId = space.AppendEntity(entity);
                            tr.AddNewlyCreatedDBObject(entity, true);
                            
                            // 将新实体的ID添加到返回列表
                            explodedEntityIds.Add(entityId);
                        }
                    }
                    
                    // 删除原始块引用
                    blockRef.Erase();
                    
                    // 如果使用了内部事务，则提交它
                    if (!externalTransaction)
                    {
                        tr.Commit();
                    }
                }
                catch (Exception ex)
                {
                    UtilsCADActive.WriteMessage($"\n打散块时发生错误: {ex.Message}");
                    // 如果使用了内部事务，则终止它
                    if (!externalTransaction)
                    {
                        tr.Abort();
                    }
                }
                finally
                {
                    // 如果使用了内部事务，需要处理它
                    if (!externalTransaction)
                    {
                        tr.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                UtilsCADActive.WriteMessage($"\n处理打散块时发生错误: {ex.Message}");
            }
            
            return explodedEntityIds;
        }

        /// <summary>
        /// 设置块定义中WIPEOUT实体的绘制顺序为最底层
        /// </summary>
        /// <param name="blockTableRecordId">块表记录的ObjectId</param>
        /// <param name="transaction">外部传入的事务对象，如果为null则使用活动事务</param>
        /// <returns>处理的WIPEOUT实体数量</returns>
        public static int UtilsSetBlockTableRecordWipeoutBackward(ObjectId blockTableRecordId, Transaction transaction = null)
        {
            // 处理的WIPEOUT实体数量
            int wipeoutCount = 0;
            
            try
            {
                // 检查是否提供了事务对象
                bool externalTransaction = transaction != null;
                Transaction tr = externalTransaction ? transaction : UtilsCADActive.Database.TransactionManager.TopTransaction;
                
                if (tr == null)
                {
                    UtilsCADActive.WriteMessage("\n错误：未提供有效的事务对象，且当前没有活动的事务。");
                    return 0;
                }
                
                // 获取块表记录
                BlockTableRecord blockDef = tr.GetObject(blockTableRecordId, OpenMode.ForRead) as BlockTableRecord;
                if (blockDef == null)
                {
                    UtilsCADActive.WriteMessage("\n错误：无效的块表记录ObjectId。");
                    return 0;
                }
                
                // 获取绘制顺序表
                DrawOrderTable drawOrderTable = tr.GetObject(blockDef.DrawOrderTableId, OpenMode.ForWrite) as DrawOrderTable;
                if (drawOrderTable == null)
                {
                    UtilsCADActive.WriteMessage("\n错误：无法获取绘制顺序表。");
                    return 0;
                }
                
                // 获取所有实体的绘制顺序
                // 使用参数1表示忽略排序实体掩码
                ObjectIdCollection allEntityIds = drawOrderTable.GetFullDrawOrder(1);
                if (allEntityIds.Count == 0)
                {
                    return 0;
                }
                
                // 查找所有WIPEOUT实体
                List<ObjectId> wipeoutIds = new List<ObjectId>();
                foreach (ObjectId entityId in allEntityIds)
                {
                    if (entityId.ObjectClass.DxfName == "WIPEOUT")
                    {
                        wipeoutIds.Add(entityId);
                        wipeoutCount++;
                    }
                }
                
                // 如果没有WIPEOUT实体，则直接返回
                if (wipeoutIds.Count == 0)
                {
                    return 0;
                }
                
                // 从绘制顺序中移除WIPEOUT实体
                ObjectIdCollection nonWipeoutIds = new ObjectIdCollection();
                foreach (ObjectId entityId in allEntityIds)
                {
                    if (!wipeoutIds.Contains(entityId))
                    {
                        nonWipeoutIds.Add(entityId);
                    }
                }
                
                // 设置新的绘制顺序，WIPEOUT实体放在最底层（先绘制）
                ObjectIdCollection newOrder = new ObjectIdCollection();
                foreach (ObjectId wipeoutId in wipeoutIds)
                {
                    newOrder.Add(wipeoutId);
                }
                foreach (ObjectId entityId in nonWipeoutIds)
                {
                    newOrder.Add(entityId);
                }
                
                // 应用新的绘制顺序
                drawOrderTable.SetRelativeDrawOrder(newOrder);
                
                return wipeoutCount;
            }
            catch (Exception ex)
            {
                UtilsCADActive.WriteMessage($"\n设置块内WIPEOUT后置时发生错误: {ex.Message}");
                return 0;
            }
        }
        
        /// <summary>
        /// 设置块参照中WIPEOUT实体的绘制顺序为最底层
        /// </summary>
        /// <param name="blockReferenceId">块参照的ObjectId</param>
        /// <param name="transaction">外部传入的事务对象，如果为null则使用活动事务</param>
        /// <returns>处理的WIPEOUT实体数量</returns>
        public static int UtilsSetBlockWipeoutBackward(ObjectId blockReferenceId, Transaction transaction = null)
        {
            try
            {
                // 检查是否提供了事务对象
                bool externalTransaction = transaction != null;
                Transaction tr = externalTransaction ? transaction : UtilsCADActive.Database.TransactionManager.TopTransaction;
                
                if (tr == null)
                {
                    UtilsCADActive.WriteMessage("\n错误：未提供有效的事务对象，且当前没有活动的事务。");
                    return 0;
                }
                
                // 获取块参照对象
                BlockReference blockRef = tr.GetObject(blockReferenceId, OpenMode.ForRead) as BlockReference;
                if (blockRef == null)
                {
                    UtilsCADActive.WriteMessage("\n错误：无效的块参照ObjectId。");
                    return 0;
                }
                
                // 获取块表记录ID
                ObjectId blockDefId = blockRef.IsDynamicBlock ? blockRef.DynamicBlockTableRecord : blockRef.BlockTableRecord;
                
                // 处理块表记录 - 使用不同名称的方法避免二义性
                int count = UtilsSetBlockTableRecordWipeoutBackward(blockDefId, tr);
                
                // 如果处理了WIPEOUT实体，则更新块参照显示
                if (count > 0)
                {
                    // 升级块参照以允许修改
                    blockRef.UpgradeOpen();
                    // 通知CAD需要重新绘制块参照
                    blockRef.RecordGraphicsModified(true);
                    // 降级块参照以防止进一步修改
                    blockRef.DowngradeOpen();
                }
                
                return count;
            }
            catch (Exception ex)
            {
                UtilsCADActive.WriteMessage($"\n处理块参照WIPEOUT后置时发生错误: {ex.Message}");
                return 0;
            }
        }
        
        /// <summary>
        /// 设置当前图纸中所有块内的WIPEOUT实体后置
        /// </summary>
        /// <returns>处理的块数量</returns>
        public static int UtilsSetAllBlocksWipeoutBackward()
        {
            int processedBlocksCount = 0;
            
            try
            {
                using (Transaction tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
                {
                    // 获取当前图纸中所有块
                    List<ObjectId> blockIds = UtilsGetAllBlockObjectIds();
                    
                    // 获取所有不重复的块表记录
                    Dictionary<ObjectId, List<ObjectId>> blockTableRecords = new Dictionary<ObjectId, List<ObjectId>>();
                    
                    foreach (ObjectId blockId in blockIds)
                    {
                        BlockReference blockRef = tr.GetObject(blockId, OpenMode.ForRead) as BlockReference;
                        if (blockRef == null) continue;
                        
                        ObjectId blockDefId = blockRef.IsDynamicBlock ? blockRef.DynamicBlockTableRecord : blockRef.BlockTableRecord;
                        
                        if (!blockTableRecords.ContainsKey(blockDefId))
                        {
                            blockTableRecords[blockDefId] = new List<ObjectId>();
                        }
                        
                        blockTableRecords[blockDefId].Add(blockId);
                    }
                    
                    // 处理每个块表记录 - 使用不同名称的方法避免二义性
                    foreach (var pair in blockTableRecords)
                    {
                        int count = UtilsSetBlockTableRecordWipeoutBackward(pair.Key, tr);
                        
                        // 如果块表记录中有WIPEOUT实体，则更新所有引用它的块参照
                        if (count > 0)
                        {
                            foreach (ObjectId refId in pair.Value)
                            {
                                BlockReference blockRef = tr.GetObject(refId, OpenMode.ForWrite) as BlockReference;
                                if (blockRef != null)
                                {
                                    // 通知CAD需要重新绘制块参照
                                    blockRef.RecordGraphicsModified(true);
                                    processedBlocksCount++;
                                }
                            }
                        }
                    }
                    
                    tr.Commit();
                }
                
                // 更新显示
                UtilsCADActive.Editor.UpdateScreen();
                
                return processedBlocksCount;
            }
            catch (Exception ex)
            {
                UtilsCADActive.WriteMessage($"\n处理所有块WIPEOUT后置时发生错误: {ex.Message}");
                return processedBlocksCount;
            }
        }
    }
}
