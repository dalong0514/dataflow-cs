using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using dataflow_cs.Core.Services;
using dataflow_cs.Utils.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace dataflow_cs.Utils.CADUtils
{
    /// <summary>
    /// 块操作工具类，提供对AutoCAD块的各种操作方法
    /// </summary>
    public static class BlockUtils
    {
        /// <summary>
        /// 获取块参照的基点(插入点)
        /// </summary>
        /// <param name="objectId">块参照的ObjectId</param>
        /// <returns>插入点坐标</returns>
        public static Point3d UtilsGetBlockBasePoint(ObjectId objectId)
        {
            return ActiveDocumentService.ExecuteInTransaction((tr, db) =>
            {
                BlockReference blockRef = tr.GetObject(objectId, OpenMode.ForRead) as BlockReference;
                if (blockRef != null)
                {
                    return blockRef.Position;
                }
                return Point3d.Origin;
            });
        }


        /// <summary>
        /// 获取块参照的图层
        /// </summary>
        /// <param name="objectId">块参照的ObjectId</param>
        /// <returns>图层名称</returns>
        public static string UtilsGetBlockLayer(ObjectId objectId)
        {
            return ActiveDocumentService.ExecuteInTransaction((tr, db) =>
            {
                try
                {
                    BlockReference blockRef = tr.GetObject(objectId, OpenMode.ForRead) as BlockReference;
                    return blockRef?.Layer ?? string.Empty;
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleException(ex, "获取块图层失败");
                    return string.Empty;
                }
            });
        }

        /// <summary>
        /// 修改块参照的图层
        /// </summary>
        /// <param name="objectId">块参照的ObjectId</param>
        /// <param name="layerName">新图层名</param>
        /// <returns>操作是否成功</returns>
        public static bool UtilsSetBlockLayerName(ObjectId objectId, string layerName)
        {
            return ActiveDocumentService.ExecuteInTransaction((tr, db) =>
            {
                try
                {
                    BlockReference blockRef = tr.GetObject(objectId, OpenMode.ForWrite) as BlockReference;
                    if (blockRef != null)
                    {
                        blockRef.Layer = layerName;
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleException(ex, "修改块图层失败");
                    return false;
                }
            });
        }

        /// <summary>
        /// 获取块参照的名称
        /// </summary>
        /// <param name="objectId">块参照的ObjectId</param>
        /// <returns>块名称</returns>
        public static string UtilsGetBlockName(ObjectId objectId)
        {
            return ActiveDocumentService.ExecuteInTransaction((tr, db) =>
            {
                try
                {
                    BlockReference blockRef = tr.GetObject(objectId, OpenMode.ForRead) as BlockReference;
                    if (blockRef != null)
                    {
                        BlockTableRecord btr = tr.GetObject(blockRef.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                        if (btr != null)
                        {
                            return btr.Name;
                        }
                    }
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleException(ex, "获取块名称失败");
                    return string.Empty;
                }
            });
        }

        /// <summary>
        /// 获取块参照的所有属性
        /// </summary>
        /// <param name="objectId">块参照的ObjectId</param>
        /// <returns>属性字典</returns>
        public static Dictionary<string, string> UtilsGetAllPropertyDictList(ObjectId objectId)
        {
            return ActiveDocumentService.ExecuteInTransaction((tr, db) =>
            {
                Dictionary<string, string> propertyDict = new Dictionary<string, string>();
                
                try
                {
                    BlockReference blockRef = tr.GetObject(objectId, OpenMode.ForRead) as BlockReference;
                    if (blockRef == null)
                    {
                        return propertyDict;
                    }

                    foreach (ObjectId attId in blockRef.AttributeCollection)
                    {
                        AttributeReference attRef = tr.GetObject(attId, OpenMode.ForRead) as AttributeReference;
                        if (attRef != null)
                        {
                            propertyDict[attRef.Tag] = attRef.TextString;
                        }
                    }
                    
                    return propertyDict;
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleException(ex, "获取块属性失败");
                    return propertyDict;
                }
            });
        }

        /// <summary>
        /// 根据属性名列表获取块参照的属性
        /// </summary>
        /// <param name="objectId">块参照的ObjectId</param>
        /// <param name="propertyNames">属性名称列表</param>
        /// <returns>属性字典</returns>
        public static Dictionary<string, string> UtilsGetPropertyDictListByPropertyNameList(ObjectId objectId, List<string> propertyNameList)
        {
            Dictionary<string, string> allProperties = UtilsGetAllPropertyDictList(objectId);
            Dictionary<string, string> filteredProperties = new Dictionary<string, string>();
            
            foreach (string name in propertyNameList)
            {
                if (allProperties.ContainsKey(name))
                {
                    filteredProperties[name] = allProperties[name];
                }
            }
            
            return filteredProperties;
        }

        /// <summary>
        /// 根据属性名获取块参照的属性值
        /// </summary>
        /// <param name="objectId">块参照的ObjectId</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>属性值</returns>
        public static string UtilsGetPropertyValueByPropertyName(ObjectId objectId, string propertyName)
        {
            Dictionary<string, string> properties = UtilsGetPropertyDictListByPropertyNameList(objectId, new List<string> { propertyName });
            
            if (properties.ContainsKey(propertyName))
            {
                return properties[propertyName];
            }
            
            return string.Empty;
        }

        /// <summary>
        /// 根据属性名设置块参照的属性值
        /// </summary>
        /// <param name="objectId">块参照的ObjectId</param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="propertyValue">属性值</param>
        /// <returns>操作是否成功</returns>
        public static bool UtilsSetPropertyValueByPropertyName(ObjectId objectId, string propertyName, string propertyValue)
        {
            return ActiveDocumentService.ExecuteInTransaction((tr, db) =>
            {
                try
                {
                    BlockReference blockRef = tr.GetObject(objectId, OpenMode.ForRead) as BlockReference;
                    if (blockRef == null)
                    {
                        return false;
                    }

                    bool foundAttribute = false;
                    
                    foreach (ObjectId attId in blockRef.AttributeCollection)
                    {
                        AttributeReference attRef = tr.GetObject(attId, OpenMode.ForWrite) as AttributeReference;
                        if (attRef != null && attRef.Tag == propertyName)
                        {
                            attRef.TextString = propertyValue;
                            foundAttribute = true;
                            break;
                        }
                    }
                    
                    return foundAttribute;
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleException(ex, $"设置块属性 {propertyName} 失败");
                    return false;
                }
            });
        }

        /// <summary>
        /// 根据属性字典设置块参照的属性值
        /// </summary>
        /// <param name="objectId">块参照的ObjectId</param>
        /// <param name="propertyDict">属性字典</param>
        /// <returns>操作是否成功</returns>
        public static bool UtilsSetPropertyValueByDictData(ObjectId objectId, Dictionary<string, string> propertyDict)
        {
            if (propertyDict == null || propertyDict.Count == 0)
            {
                return false;
            }
            
            return ActiveDocumentService.ExecuteInTransaction((tr, db) =>
            {
                try
                {
                    BlockReference blockRef = tr.GetObject(objectId, OpenMode.ForRead) as BlockReference;
                    if (blockRef == null)
                    {
                        return false;
                    }

                    foreach (ObjectId attId in blockRef.AttributeCollection)
                    {
                        AttributeReference attRef = tr.GetObject(attId, OpenMode.ForWrite) as AttributeReference;
                        if (attRef != null && propertyDict.ContainsKey(attRef.Tag))
                        {
                            attRef.TextString = propertyDict[attRef.Tag];
                        }
                    }
                    
                    return true;
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleException(ex, "设置块属性失败");
                    return false;
                }
            });
        }

        /// <summary>
        /// 设置动态块的属性值
        /// </summary>
        /// <param name="objectId">块参照的ObjectId</param>
        /// <param name="propertyDict">属性字典</param>
        /// <returns>操作是否成功</returns>
        public static bool UtilsSetDynamicPropertyValueByDictData(ObjectId objectId, Dictionary<string, string> propertyDict)
        {
            if (propertyDict == null || propertyDict.Count == 0)
            {
                return false;
            }
            
            return ActiveDocumentService.ExecuteInTransaction((tr, db) =>
            {
                try
                {
                    BlockReference blockRef = tr.GetObject(objectId, OpenMode.ForRead) as BlockReference;
                    if (blockRef == null)
                    {
                        return false;
                    }

                    foreach (ObjectId attId in blockRef.AttributeCollection)
                    {
                        AttributeReference attRef = tr.GetObject(attId, OpenMode.ForWrite) as AttributeReference;
                        if (attRef != null && propertyDict.ContainsKey(attRef.Tag))
                        {
                            attRef.TextString = propertyDict[attRef.Tag];
                        }
                    }
                    
                    return true;
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleException(ex, "设置块属性失败");
                    return false;
                }
            });
        }

        /// <summary>
        /// 通过块名称选择块
        /// </summary>
        /// <param name="blockName">块名称</param>
        /// <param name="exactMatch">是否精确匹配</param>
        /// <returns>选择的块ObjectId列表</returns>
        public static List<ObjectId> UtilsGetObjectIdsBySelectByBlockName(string blockName, bool isIdentical = true)
        {
            Editor editor = ActiveDocumentService.GetEditor();
            if (editor == null)
            {
                return new List<ObjectId>();
            }
            
            try
            {
                TypedValue[] values = new TypedValue[2];
                values[0] = new TypedValue((int)DxfCode.Start, "INSERT");
                values[1] = new TypedValue((int)DxfCode.BlockName, blockName);
                
                SelectionFilter filter = new SelectionFilter(values);
                PromptSelectionOptions options = new PromptSelectionOptions
                {
                    MessageForAdding = $"\n选择名称为 {blockName} 的块:"
                };
                
                PromptSelectionResult result = editor.GetSelection(options, filter);
                if (result.Status == PromptStatus.OK)
                {
                    SelectionSet selSet = result.Value;
                    ObjectId[] ids = selSet.GetObjectIds();
                    
                    if (!isIdentical)
                    {
                        return new List<ObjectId>(ids);
                    }
                    
                    // 精确匹配时，再进行一次过滤
                    return ActiveDocumentService.ExecuteInTransaction((tr, db) =>
                    {
                        List<ObjectId> exactMatchIds = new List<ObjectId>();
                        
                        foreach (ObjectId id in ids)
                        {
                            string name = UtilsGetBlockName(id);
                            if (name == blockName)
                            {
                                exactMatchIds.Add(id);
                            }
                        }
                        
                        return exactMatchIds;
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, $"按名称选择块 {blockName} 失败");
            }
            
            return new List<ObjectId>();
        }

        /// <summary>
        /// 获取所有块参照
        /// </summary>
        /// <returns>所有块参照的ObjectId列表</returns>
        public static List<ObjectId> UtilsGetAllBlockObjectIds()
        {
            return ActiveDocumentService.ExecuteInTransaction((tr, db) =>
            {
                List<ObjectId> blockIds = new List<ObjectId>();
                
                try
                {
                    BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    if (bt == null)
                    {
                        return blockIds;
                    }
                    
                    BlockTableRecord ms = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                    if (ms == null)
                    {
                        return blockIds;
                    }
                    
                    foreach (ObjectId id in ms)
                    {
                        if (id.ObjectClass.DxfName == "INSERT")
                        {
                            blockIds.Add(id);
                        }
                    }
                    
                    return blockIds;
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleException(ex, "获取所有块参照失败");
                    return blockIds;
                }
            });
        }

        /// <summary>
        /// 获取所有指定名称的块参照
        /// </summary>
        /// <param name="blockName">块名称</param>
        /// <param name="exactMatch">是否精确匹配</param>
        /// <returns>指定名称的块参照ObjectId列表</returns>
        public static List<ObjectId> GetAllObjectIdsByBlockName(string blockName, bool isIdentical = true)
        {
            return ActiveDocumentService.ExecuteInTransaction((tr, db) =>
            {
                List<ObjectId> blockIds = new List<ObjectId>();
                
                try
                {
                    BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    if (bt == null)
                    {
                        return blockIds;
                    }
                    
                    BlockTableRecord ms = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                    if (ms == null)
                    {
                        return blockIds;
                    }
                    
                    foreach (ObjectId id in ms)
                    {
                        if (id.ObjectClass.DxfName == "INSERT")
                        {
                            string name = UtilsGetBlockName(id);
                            
                            if (isIdentical && name == blockName)
                            {
                                blockIds.Add(id);
                            }
                            else if (!isIdentical && name.Contains(blockName))
                            {
                                blockIds.Add(id);
                            }
                        }
                    }
                    
                    return blockIds;
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleException(ex, $"获取名称为 {blockName} 的块参照失败");
                    return blockIds;
                }
            });
        }

        /// <summary>
        /// 获取所有指定名称的块参照
        /// </summary>
        /// <param name="blockIds">要筛选的块参照ObjectId列表</param>
        /// <param name="blockName">块名称</param>
        /// <param name="exactMatch">是否精确匹配</param>
        /// <returns>指定名称的块参照ObjectId列表</returns>
        public static List<ObjectId> GetAllObjectIdsByBlockName(List<ObjectId> blockIds, string blockName, bool isIdentical = true)
        {
            return ActiveDocumentService.ExecuteInTransaction((tr, db) =>
            {
                List<ObjectId> filteredIds = new List<ObjectId>();
                
                try
                {
                    foreach (ObjectId id in blockIds)
                    {
                        string name = UtilsGetBlockName(id);
                        if (isIdentical && name == blockName || !isIdentical && name.Contains(blockName))
                        {
                            filteredIds.Add(id);
                        }
                    }
                    
                    return filteredIds;
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleException(ex, $"获取名称为 {blockName} 的块参照失败");
                    return filteredIds;
                }
            });
        }

        /// <summary>
        /// 根据块名称列表获取分组的块参照
        /// </summary>
        /// <param name="blockIds">要筛选的块参照ObjectId列表</param>
        /// <param name="blockNameList">块名称列表</param>
        /// <param name="exactMatch">是否精确匹配</param>
        /// <returns>按块名称分组的块参照ObjectId字典</returns>
        public static Dictionary<string, List<ObjectId>> UtilsGetAllObjectIdsGroupsByBlockNameList(List<ObjectId> blockIds, List<string> blockNameList, bool isIdentical = true)
        {
            return ActiveDocumentService.ExecuteInTransaction((tr, db) =>
            {
                Dictionary<string, List<ObjectId>> groups = new Dictionary<string, List<ObjectId>>();
                
                try
                {
                    foreach (string blockName in blockNameList)
                    {
                        groups[blockName] = UtilsGetAllObjectIdsByBlockName(blockIds, blockName, isIdentical);
                    }
                    
                    return groups;
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleException(ex, "获取块参照失败");
                    return groups;
                }
            });
        }

        /// <summary>
        /// 获取块参照的旋转角度（弧度）
        /// </summary>
        /// <param name="objectId">块参照的ObjectId</param>
        /// <returns>旋转角度（弧度）</returns>
        public static double UtilsGetBlockRotaton(ObjectId objectId)
        {
            return ActiveDocumentService.ExecuteInTransaction((tr, db) =>
            {
                try
                {
                    BlockReference blockRef = tr.GetObject(objectId, OpenMode.ForRead) as BlockReference;
                    return blockRef?.Rotation ?? 0.0;
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleException(ex, "获取块旋转角度失败");
                    return 0.0;
                }
            });
        }

        /// <summary>
        /// 获取块参照的旋转角度（度）
        /// </summary>
        /// <param name="objectId">块参照的ObjectId</param>
        /// <returns>旋转角度（度）</returns>
        public static double UtilsGetBlockRotatonInDegrees(ObjectId objectId)
        {
            double radians = UtilsGetBlockRotaton(objectId);
            return radians * 180.0 / Math.PI;
        }

        /// <summary>
        /// 设置块参照的旋转角度（弧度）
        /// </summary>
        /// <param name="objectId">块参照的ObjectId</param>
        /// <param name="rotation">旋转角度（弧度）</param>
        /// <returns>操作是否成功</returns>
        public static bool UtilsSetBlockRotaton(ObjectId objectId, double rotation)
        {
            return ActiveDocumentService.ExecuteInTransaction((tr, db) =>
            {
                try
                {
                    BlockReference blockRef = tr.GetObject(objectId, OpenMode.ForWrite) as BlockReference;
                    if (blockRef != null)
                    {
                        blockRef.Rotation = rotation;
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleException(ex, "设置块旋转角度失败");
                    return false;
                }
            });
        }

        /// <summary>
        /// 设置块参照的旋转角度（度）
        /// </summary>
        /// <param name="objectId">块参照的ObjectId</param>
        /// <param name="degrees">旋转角度（度）</param>
        /// <returns>操作是否成功</returns>
        public static bool UtilsSetBlockRotatonInDegrees(ObjectId objectId, double degrees)
        {
            double radians = degrees * Math.PI / 180.0;
            return UtilsSetBlockRotaton(objectId, radians);
        }

        /// <summary>
        /// 设置块参照的XY比例
        /// </summary>
        /// <param name="objectId">块参照的ObjectId</param>
        /// <param name="xScale">X方向比例</param>
        /// <param name="yScale">Y方向比例</param>
        /// <returns>操作是否成功</returns>
        public static bool UtilsSetBlockXYScale(ObjectId objectId, double xScale, double yScale)
        {
            return ActiveDocumentService.ExecuteInTransaction((tr, db) =>
            {
                try
                {
                    BlockReference blockRef = tr.GetObject(objectId, OpenMode.ForWrite) as BlockReference;
                    if (blockRef != null)
                    {
                        blockRef.ScaleFactors = new Scale3d(xScale, yScale, 1.0);
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleException(ex, "设置块缩放失败");
                    return false;
                }
            });
        }
    }
} 