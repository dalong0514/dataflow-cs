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
        /// 从外部CAD文件引用特定的块定义到当前图形中
        /// </summary>
        /// <param name="sourceDwgPath">源CAD文件的完整路径</param>
        /// <param name="blockName">要引用的块定义名称</param>
        /// <returns>如果成功，返回新块的ObjectId；如果失败，返回ObjectId.Null</returns>
        public static ObjectId UtilsImportBlockFromExternalDwg(string sourceDwgPath, string blockName)
        {
            // 获取当前数据库
            Database destDb = UtilsCADActive.Database;
            // 要返回的块定义的ObjectId
            ObjectId blockId = ObjectId.Null;
           
            // 创建一个新的数据库对象来读取源DWG文件
            using (Database sourceDb = new Database(false, true))
            {
                try
                {
                    // 读取源DWG文件到sourceDb
                    sourceDb.ReadDwgFile(sourceDwgPath, FileOpenMode.OpenForReadAndAllShare, false, "");
                   
                    // 开始当前数据库的事务
                    using (Transaction destTr = destDb.TransactionManager.StartTransaction())
                    {
                        // 打开目标数据库的块表以供写入
                        BlockTable destBlockTable = destTr.GetObject(destDb.BlockTableId, OpenMode.ForWrite) as BlockTable;
                       
                        // 构建新块的名称
                        string newBlockName = blockName;
                       
                        // 如果目标数据库中已存在同名块，则使用现有块
                        if (destBlockTable.Has(newBlockName))
                        {
                            UtilsCADActive.WriteMessage($"\n块定义 '{newBlockName}' 已存在于当前图形中，将使用现有块。");
                            blockId = destBlockTable[newBlockName];
                            destTr.Commit();
                            return blockId;
                        }
                       
                        // 开始源数据库的事务
                        using (Transaction sourceTr = sourceDb.TransactionManager.StartTransaction())
                        {
                            // 打开源数据库的块表以供读取
                            BlockTable sourceBlockTable = sourceTr.GetObject(sourceDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                           
                            // 检查源数据库中是否存在指定的块
                            if (!sourceBlockTable.Has(blockName))
                            {
                                UtilsCADActive.WriteMessage($"\n在源文件中找不到块定义 '{blockName}'。");
                                sourceTr.Commit();
                                destTr.Commit();
                                return ObjectId.Null;
                            }
                           
                            // 获取源块表记录
                            ObjectId sourceBlockId = sourceBlockTable[blockName];
                            // 使用更直接的方法复制块
                            try
                            {
                                // 创建一个对象ID集合，只包含要复制的块的ID
                                ObjectIdCollection blockIds = new ObjectIdCollection();
                                blockIds.Add(sourceBlockId);
                                
                                // 创建映射对象
                                IdMapping idMap = new IdMapping();
                                
                                // 复制块定义及其所有相关对象（包括动态块相关数据）
                                // 使用CopyObjects方法可以更完整地复制整个块的定义，包括动态块属性
                                // 注意：直接复制块表记录比单独复制其内部实体更可靠
                                sourceDb.WblockCloneObjects(blockIds, destBlockTable.ObjectId, idMap, DuplicateRecordCloning.MangleName, false);
                                
                                // 在映射中查找新创建的块ID
                                foreach (IdPair pair in idMap)
                                {
                                    if (pair.Key == sourceBlockId && pair.Value != ObjectId.Null)
                                    {
                                        blockId = pair.Value;
                                        // 获取新创建的块表记录
                                        BlockTableRecord newBlockRec = destTr.GetObject(blockId, OpenMode.ForWrite) as BlockTableRecord;
                                        // 确保块名称正确
                                        if (newBlockRec.Name != newBlockName)
                                        {
                                            newBlockRec.Name = newBlockName;
                                        }
                                        break;
                                    }
                                }
                                
                                // 检查块是否是动态块
                                if (blockId != ObjectId.Null)
                                {
                                    BlockTableRecord sourceBlockRec = sourceTr.GetObject(sourceBlockId, OpenMode.ForRead) as BlockTableRecord;
                                    BlockTableRecord destBlockRec = destTr.GetObject(blockId, OpenMode.ForRead) as BlockTableRecord;
                                    
                                    if (sourceBlockRec.IsDynamicBlock)
                                    {
                                        // 验证目标块是否也是动态块
                                        if (destBlockRec.IsDynamicBlock)
                                        {
                                            UtilsCADActive.WriteMessage($"\n成功复制动态块 '{blockName}' 的完整定义，包括所有动态属性。");
                                        }
                                        else
                                        {
                                            UtilsCADActive.WriteMessage($"\n警告：源块是动态块，但目标块可能没有完全复制动态属性。");
                                            
                                            // 尝试通过完整克隆的方式再次复制
                                            try
                                            {
                                                // 删除之前创建的非动态块
                                                destBlockRec.UpgradeOpen();
                                                destBlockRec.Erase();
                                                destTr.TransactionManager.QueueForGraphicsFlush();
                                                
                                                // 创建一个新的块表记录
                                                using (BlockTableRecord newBlockRec = new BlockTableRecord())
                                                {
                                                    newBlockRec.Name = newBlockName;
                                                    blockId = destBlockTable.Add(newBlockRec);
                                                    destTr.AddNewlyCreatedDBObject(newBlockRec, true);
                                                    
                                                    // 收集所有动态块相关的对象
                                                    ObjectIdCollection allRelatedIds = new ObjectIdCollection();
                                                    
                                                    // 添加动态块的主块记录
                                                    allRelatedIds.Add(sourceBlockId);
                                                    
                                                    // 直接复制整个动态块定义
                                                    sourceDb.WblockCloneObjects(allRelatedIds, destBlockTable.ObjectId, idMap, DuplicateRecordCloning.Replace, false);
                                                    
                                                    UtilsCADActive.WriteMessage($"\n尝试了替代方法复制动态块定义。");
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                UtilsCADActive.WriteMessage($"\n尝试替代复制方法时出错: {ex.Message}");
                                            }
                                        }
                                    }
                                }
                                
                                UtilsCADActive.WriteMessage($"\n成功导入块定义 '{blockName}'，现在可以使用名称 '{newBlockName}' 在当前图形中引用该块。");
                            }
                            catch (Exception ex)
                            {
                                UtilsCADActive.WriteMessage($"\n使用主要方法复制块时出错: {ex.Message}");
                                
                                // 使用备用方法 - 创建新块并复制内容
                                try
                                {
                                    UtilsCADActive.WriteMessage($"\n尝试使用备用方法...");
                                    BlockTableRecord sourceBlockRec = sourceTr.GetObject(sourceBlockId, OpenMode.ForRead) as BlockTableRecord;
                                    
                                    // 创建一个新的块表记录对象
                                    using (BlockTableRecord destBlockRec = new BlockTableRecord())
                                    {
                                        // 设置新块的基本属性
                                        destBlockRec.Name = newBlockName;
                                        destBlockRec.Origin = sourceBlockRec.Origin;
                                        destBlockRec.Comments = $"从 {sourceDwgPath} 导入";
                                        
                                        // 将新块添加到目标块表中
                                        blockId = destBlockTable.Add(destBlockRec);
                                        destTr.AddNewlyCreatedDBObject(destBlockRec, true);
                                        
                                        // 收集源块中的所有实体和相关对象
                                        ObjectIdCollection entitySet = new ObjectIdCollection();
                                        foreach (ObjectId entId in sourceBlockRec)
                                        {
                                            entitySet.Add(entId);
                                        }
                                        
                                        // 如果源块是动态块，需要确保复制所有相关对象
                                        if (sourceBlockRec.IsDynamicBlock)
                                        {
                                            // BlockTableRecord没有DynamicBlockTableRecord属性
                                            // 尝试通过AnonymousBlockTableRecord获取动态块的其他组件
                                            try
                                            {
                                                // 在当前图形中查找所有与此动态块相关的匿名块
                                                SymbolTableEnumerator btrEnum = sourceBlockTable.GetEnumerator();
                                                while (btrEnum.MoveNext())
                                                {
                                                    ObjectId btrId = btrEnum.Current;
                                                    BlockTableRecord btr = sourceTr.GetObject(btrId, OpenMode.ForRead) as BlockTableRecord;
                                                    
                                                    // 检查是否是匿名块，匿名块通常以*开头，并且与动态块相关
                                                    if (btr.IsAnonymous && btr.IsDependent)
                                                    {
                                                        entitySet.Add(btrId);
                                                        
                                                        // 添加此匿名块中的所有对象
                                                        foreach (ObjectId entId in btr)
                                                        {
                                                            entitySet.Add(entId);
                                                        }
                                                    }
                                                }
                                                
                                                UtilsCADActive.WriteMessage($"\n已添加与动态块相关的匿名块及其组件。");
                                            }
                                            catch (Exception dynEx)
                                            {
                                                UtilsCADActive.WriteMessage($"\n查找动态块的关联对象时出错: {dynEx.Message}");
                                            }
                                        }
                                        
                                        // 创建ID映射
                                        IdMapping idMap = new IdMapping();
                                        
                                        // 复制所有对象
                                        sourceDb.WblockCloneObjects(entitySet, destBlockRec.ObjectId, idMap, DuplicateRecordCloning.Replace, false);
                                        
                                        if (sourceBlockRec.IsDynamicBlock)
                                        {
                                            UtilsCADActive.WriteMessage($"\n已尝试复制动态块的所有组件。");
                                        }
                                    }
                                }
                                catch (Exception fallbackEx)
                                {
                                    UtilsCADActive.WriteMessage($"\n备用方法也失败: {fallbackEx.Message}");
                                }
                            }
                            
                            sourceTr.Commit();
                        }
                        
                        destTr.Commit();
                    }
                }
                catch (System.Exception ex)
                {
                    UtilsCADActive.WriteMessage($"\n导入块定义时发生错误: {ex.Message}");
                    return ObjectId.Null;
                }
            }
            
            return blockId;
        }

        /// <summary>
        /// 将块参照中的所有直线实体(LINE)转换为多段线实体(LWPOLYLINE)，并设置线宽
        /// </summary>
        /// <param name="blockRefId">块参照的ObjectId</param>
        /// <param name="polylineWidth">多段线的线宽，默认为0.0</param>
        /// <returns>转换成功的直线数量</returns>
        public static int UtilsConvertBlockLinesToPolylines(ObjectId blockRefId, double polylineWidth = 0.0)
        {
            int convertedCount = 0;
            Database db = UtilsCADActive.Database;
            
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    // 获取块参照对象
                    BlockReference blockRef = tr.GetObject(blockRefId, OpenMode.ForRead) as BlockReference;
                    if (blockRef == null)
                    {
                        UtilsCADActive.WriteMessage("\n无效的块参照ObjectId。");
                        tr.Commit();
                        return 0;
                    }
                    
                    // 获取块定义
                    BlockTableRecord blockDef = tr.GetObject(blockRef.BlockTableRecord, OpenMode.ForWrite) as BlockTableRecord;
                    if (blockDef == null)
                    {
                        UtilsCADActive.WriteMessage("\n无法获取块定义。");
                        tr.Commit();
                        return 0;
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
                        tr.Commit();
                        return 0;
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
                        
                        convertedCount++;
                    }
                    
                    // 更新显示
                    UtilsCADActive.Editor.UpdateScreen();
                    
                    UtilsCADActive.WriteMessage($"\n已成功将 {convertedCount} 条直线转换为多段线。");
                }
                catch (Exception ex)
                {
                    UtilsCADActive.WriteMessage($"\n转换直线为多段线时发生错误: {ex.Message}");
                }
                
                tr.Commit();
            }
            
            return convertedCount;
        }
    }
}
