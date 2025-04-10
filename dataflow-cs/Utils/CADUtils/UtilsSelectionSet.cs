using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.Geometry;

namespace dataflow_cs.Utils.CADUtils
{
    /// <summary>
    /// 提供AutoCAD选择集操作的辅助方法
    /// </summary>
    public static class UtilsSelectionSet
    {
        // 选择集操作结果信息类
        public class SelectionResult
        {
            public SelectionSet SelectionSet { get; set; }
            public bool IsSuccessful { get; set; }
            public string ErrorMessage { get; set; }
            public PromptStatus Status { get; set; }

            public SelectionResult(SelectionSet selSet, PromptStatus status, string errorMessage = null)
            {
                SelectionSet = selSet;
                Status = status;
                IsSuccessful = status == PromptStatus.OK && selSet != null;
                ErrorMessage = errorMessage ?? GetErrorMessageFromStatus(status);
            }

            private string GetErrorMessageFromStatus(PromptStatus status)
            {
                switch (status)
                {
                    case PromptStatus.OK:
                        return null;
                    case PromptStatus.Cancel:
                        return "用户取消了操作";
                    case PromptStatus.Error:
                        return "选择过程中发生错误";
                    case PromptStatus.Keyword:
                        return "用户输入了关键字";
                    case PromptStatus.Modeless:
                        return "无模式操作";
                    case PromptStatus.None:
                        return "未执行任何操作";
                    case PromptStatus.Other:
                        return "发生了未知情况";
                    default:
                        return $"未知状态: {status}";
                }
            }

            // 隐式转换器，允许将SelectionResult直接用作SelectionSet
            public static implicit operator SelectionSet(SelectionResult result)
            {
                return result?.SelectionSet;
            }
        }

        // get all entity selection set by filter
        public static SelectionResult UtilsGetAllBlockSelectionSet() => UtilsGetAllSelectionSetByFilter("INSERT");
        public static SelectionResult UtilsGetAllBlockSelectionSetByLayerName(string layerName) => UtilsGetAllSelectionSetByFilter("INSERT", layerName);
        public static SelectionResult UtilsGetAllMTextSelectionSet() => UtilsGetAllSelectionSetByFilter("MText");
        public static SelectionResult UtilsGetAllMTextSelectionSetByLayerName(string layerName) => UtilsGetAllSelectionSetByFilter("MText", layerName);
        public static SelectionResult UtilsGetAllTextSelectionSet() => UtilsGetAllSelectionSetByFilter("Text");
        public static SelectionResult UtilsGetAllTextSelectionSetByLayerName(string layerName) => UtilsGetAllSelectionSetByFilter("Text", layerName);
        public static SelectionResult UtilsGetAllPolylineSelectionSet() => UtilsGetAllSelectionSetByFilter("LWPOLYLINE");
        public static SelectionResult UtilsGetAllPolylineSelectionSetByLayerName(string layerName) => UtilsGetAllSelectionSetByFilter("LWPOLYLINE", layerName);

        // get all entity selection set by filter
        public static SelectionResult UtilsGetAllBlockSelectionSetByCrossingWindow(Extents3d extents) => UtilsGetSelectionSetByFilterByCrossingWindow("INSERT", extents);
        public static SelectionResult UtilsGetAllBlockSelectionSetByLayerNameByCrossingWindow(Extents3d extents, string layerName) => UtilsGetSelectionSetByFilterByCrossingWindow("INSERT", extents, layerName);
        public static SelectionResult UtilsGetAllMTextSelectionSetByCrossingWindow(Extents3d extents) => UtilsGetSelectionSetByFilterByCrossingWindow("MText", extents);
        public static SelectionResult UtilsGetAllMTextSelectionSetByLayerNameByCrossingWindow(Extents3d extents, string layerName) => UtilsGetSelectionSetByFilterByCrossingWindow("MText", extents, layerName);
        public static SelectionResult UtilsGetAllTextSelectionSetByCrossingWindow(Extents3d extents) => UtilsGetSelectionSetByFilterByCrossingWindow("Text", extents);
        public static SelectionResult UtilsGetAllTextSelectionSetByLayerNameByCrossingWindow(Extents3d extents, string layerName) => UtilsGetSelectionSetByFilterByCrossingWindow("Text", extents, layerName);
        public static SelectionResult UtilsGetAllPolylineSelectionSetByCrossingWindow(Extents3d extents) => UtilsGetSelectionSetByFilterByCrossingWindow("LWPOLYLINE", extents);
        public static SelectionResult UtilsGetAllPolylineSelectionSetByLayerNameByCrossingWindow(Extents3d extents, string layerName) => UtilsGetSelectionSetByFilterByCrossingWindow("LWPOLYLINE", extents, layerName);

        // get entity selection set by filter
        public static SelectionResult UtilsGetBlockSelectionSet() => UtilsGetSelectionSetByFilter("INSERT");
        public static SelectionResult UtilsGetBlockSelectionSetByLayerName(string layerName) => UtilsGetSelectionSetByFilter("INSERT", layerName);
        public static SelectionResult UtilsGetMTextSelectionSet() => UtilsGetSelectionSetByFilter("MText");
        public static SelectionResult UtilsGetMTextSelectionSetByLayerName(string layerName) => UtilsGetSelectionSetByFilter("MText", layerName);
        public static SelectionResult UtilsGetTextSelectionSet() => UtilsGetSelectionSetByFilter("Text");
        public static SelectionResult UtilsGetTextSelectionSetByLayerName(string layerName) => UtilsGetSelectionSetByFilter("Text", layerName);
        public static SelectionResult UtilsGetPolylineSelectionSet() => UtilsGetSelectionSetByFilter("LWPOLYLINE");
        public static SelectionResult UtilsGetPolylineSelectionSetByLayerName(string layerName) => UtilsGetSelectionSetByFilter("LWPOLYLINE", layerName);
        
        // 2025-04-10 测试获取上一次选择集
        /// <summary>
        /// 从选择集获取ObjectId列表
        /// </summary>
        /// <param name="selResult">选择结果</param>
        /// <returns>ObjectId列表</returns>
        public static List<ObjectId> UtilsGetObjectIdsFromSelectionSet(SelectionResult selResult)
        {
            if (selResult.Status != PromptStatus.OK)
            {
                return new List<ObjectId>();
            }
            return selResult.SelectionSet.GetObjectIds().ToList();
        }

        /// <summary>
        /// 创建选择过滤器
        /// </summary>
        public static SelectionFilter UtilsCreateFilter(string entityType, string layerName = null)
        {
            // 验证参数
            if (string.IsNullOrEmpty(entityType))
            {
                throw new ArgumentNullException(nameof(entityType), "实体类型不能为空");
            }

            // Create a new list for filter values
            List<TypedValue> filterValues = new List<TypedValue>
            {
                // Add entity type filter
                new TypedValue((int)DxfCode.Start, entityType)
            };

            // Add layer name filter if layerName is not null
            if (!string.IsNullOrEmpty(layerName))
            {
                filterValues.Add(new TypedValue((int)DxfCode.LayerName, layerName));
            }

            // Convert the list to an array
            TypedValue[] filterList = filterValues.ToArray();
            return new SelectionFilter(filterList);
        }

        /// <summary>
        /// 创建选择选项
        /// </summary>
        public static PromptSelectionOptions UtilsCreateSelectionOptions(string promptMessage = "Select: ")
        {
            PromptSelectionOptions opts = new PromptSelectionOptions
            {
                // 当用户在选择对象时，这个字符串将作为提示信息显示
                MessageForAdding = promptMessage,
                // 如果设置为true，则允许在选择集中包含重复的对象。如果设置为false，则选择集中不会包含重复的对象
                AllowDuplicates = false,
                // 如果设置为true，则用户只能选择一个对象。如果设置为false，则用户可以选择多个对象
                SingleOnly = false,
                // 如果设置为true，则用户每次只能在一个空间（例如模型空间或布局空间）中选择对象
                SinglePickInSpace = true,
                // 如果设置为true，则用户不能选择非当前空间的对象
                RejectObjectsFromNonCurrentSpace = true,
                // 如果设置为true，则用户不能选择被锁定图层上的对象
                RejectObjectsOnLockedLayers = false,
                // 如果设置为true，则用户不能选择布局空间视口中的对象
                RejectPaperspaceViewport = false
            };
            return opts;
        }

        /// <summary>
        /// 通过过滤器让用户选择对象
        /// </summary>
        /// <param name="entityType">实体类型</param>
        /// <param name="layerName">图层名称，可选</param>
        /// <returns>选择结果</returns>
        public static SelectionResult UtilsGetSelectionSetByFilter(string entityType, string layerName = null)
        {
            try
            {
                // 检查当前文档和编辑器是否可用
                if (UtilsCADActive.Document == null || UtilsCADActive.Editor == null)
                {
                    return new SelectionResult(null, PromptStatus.Error, "当前没有打开的AutoCAD文档或编辑器不可用");
                }

                // 创建过滤器
                SelectionFilter filter = UtilsCreateFilter(entityType, layerName);

                // 创建选择选项
                PromptSelectionOptions opts = UtilsCreateSelectionOptions();

                // 执行选择操作
                PromptSelectionResult selRes = UtilsCADActive.Editor.GetSelection(opts, filter);

                // 返回结果
                return new SelectionResult(selRes.Value, selRes.Status);
            }
            catch (Exception ex)
            {
                // 记录详细错误信息
                System.Diagnostics.Debug.WriteLine($"UtilsGetSelectionSetByFilter 发生错误: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                
                return new SelectionResult(null, PromptStatus.Error, ex.Message);
            }
        }

        /// <summary>
        /// 通过实体类型自动选择所有对象，无需用户交互
        /// </summary>
        /// <param name="entityType">实体类型</param>
        /// <param name="layerName">图层名称，可选</param>
        /// <returns>选择结果</returns>
        public static SelectionResult UtilsGetAllSelectionSetByFilter(string entityType, string layerName = null)
        {
            try
            {
                // 检查当前文档和编辑器是否可用
                if (UtilsCADActive.Document == null || UtilsCADActive.Editor == null)
                {
                    return new SelectionResult(null, PromptStatus.Error, "当前没有打开的AutoCAD文档或编辑器不可用");
                }
                
                // 创建过滤器
                SelectionFilter filter = UtilsCreateFilter(entityType, layerName);

                // 执行选择操作
                PromptSelectionResult selRes = UtilsCADActive.Editor.SelectAll(filter);

                // 返回结果
                return new SelectionResult(selRes.Value, selRes.Status);
            }
            catch (Exception ex)
            {
                // 记录详细错误信息
                System.Diagnostics.Debug.WriteLine($"UtilsGetAllSelectionSetByFilter 发生错误: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                
                return new SelectionResult(null, PromptStatus.Error, ex.Message);
            }
        }

        /// <summary>
        /// 使用交叉窗口选择指定类型的实体对象
        /// </summary>
        /// <param name="entityType">实体类型</param>
        /// <param name="extents">选择范围</param>
        /// <param name="layerName">图层名称，可选</param>
        /// <returns>选择结果</returns>
        public static SelectionResult UtilsGetSelectionSetByFilterByCrossingWindow(string entityType, Extents3d extents, string layerName = null)
        {
            try
            {
                // 检查输入参数
                if (string.IsNullOrEmpty(entityType))
                {
                    throw new ArgumentNullException(nameof(entityType), "实体类型不能为空");
                }

                if (extents == null)
                {
                    throw new ArgumentNullException(nameof(extents), "选择范围不能为空");
                }

                // 检查当前文档和编辑器是否可用
                if (UtilsCADActive.Document == null || UtilsCADActive.Editor == null)
                {
                    throw new InvalidOperationException("当前没有打开的AutoCAD文档或编辑器不可用");
                }

                // 创建过滤器
                SelectionFilter filter = UtilsCreateFilter(entityType, layerName);

                // 使用交叉窗口选择
                Point3d minPoint = new Point3d(extents.MinPoint.X, extents.MinPoint.Y, extents.MinPoint.Z);
                Point3d maxPoint = new Point3d(extents.MaxPoint.X, extents.MaxPoint.Y, extents.MaxPoint.Z);

                // 尝试选择对象
                PromptSelectionResult selRes = UtilsCADActive.Editor.SelectCrossingWindow(minPoint, maxPoint, filter);
                
                // 检查选择结果并返回
                if (selRes.Status != PromptStatus.OK)
                {
                    return new SelectionResult(null, selRes.Status);
                }

                if (selRes.Value == null || selRes.Value.Count == 0)
                {
                    return new SelectionResult(null, PromptStatus.OK, "未找到符合条件的对象");
                }

                return new SelectionResult(selRes.Value, selRes.Status);
            }
            catch (Exception ex)
            {
                // 记录详细错误信息
                System.Diagnostics.Debug.WriteLine($"UtilsGetSelectionSetByFilterByCrossingWindow 发生错误: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                
                return new SelectionResult(null, PromptStatus.Error, ex.Message);
            }
        }

        /// <summary>
        /// 获取PickFirst选择集（在命令开始前用户已经选择的对象）
        /// </summary>
        /// <returns>选择结果</returns>
        public static SelectionResult UtilsGetPickFirstSelectionSet()
        {
            try
            {
                // 检查当前文档和编辑器是否可用
                if (UtilsCADActive.Document == null || UtilsCADActive.Editor == null)
                {
                    return new SelectionResult(null, PromptStatus.Error, "当前没有打开的AutoCAD文档或编辑器不可用");
                }

                // 获取PickFirst选择集
                PromptSelectionResult selRes = UtilsCADActive.Editor.SelectImplied();

                // 返回结果
                return new SelectionResult(selRes.Value, selRes.Status);
            }
            catch (Exception ex)
            {
                // 记录详细错误信息
                System.Diagnostics.Debug.WriteLine($"UtilsGetPickFirstSelectionSet 发生错误: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                
                return new SelectionResult(null, PromptStatus.Error, ex.Message);
            }
        }

        /// <summary>
        /// 清除当前的PickFirst选择集
        /// </summary>
        public static void UtilsClearPickFirstSelectionSet()
        {
            try
            {
                // 检查当前文档和编辑器是否可用
                if (UtilsCADActive.Document == null || UtilsCADActive.Editor == null)
                {
                    return;
                }

                // 创建一个空的ObjectId数组来清除PickFirst选择集
                ObjectId[] emptyArray = new ObjectId[0];
                UtilsCADActive.Editor.SetImpliedSelection(emptyArray);
            }
            catch (Exception ex)
            {
                // 记录详细错误信息
                System.Diagnostics.Debug.WriteLine($"UtilsClearPickFirstSelectionSet 发生错误: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// 获取上一次选择的对象集合
        /// </summary>
        /// <returns>选择结果</returns>
        public static SelectionResult UtilsGetPreviousSelectionSet()
        {
            try
            {
                // 检查当前文档和编辑器是否可用
                if (UtilsCADActive.Document == null || UtilsCADActive.Editor == null)
                {
                    return new SelectionResult(null, PromptStatus.Error, "当前没有打开的AutoCAD文档或编辑器不可用");
                }

                // 获取上一次选择的对象集合
                PromptSelectionResult selRes = UtilsCADActive.Editor.SelectPrevious();

                // 返回结果
                return new SelectionResult(selRes.Value, selRes.Status);
            }
            catch (Exception ex)
            {
                // 记录详细错误信息
                System.Diagnostics.Debug.WriteLine($"UtilsGetPreviousSelectionSet 发生错误: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                
                return new SelectionResult(null, PromptStatus.Error, ex.Message);
            }
        }

        /// <summary>
        /// 获取最后创建的对象
        /// </summary>
        /// <returns>选择结果</returns>
        public static SelectionResult UtilsGetLastCreatedObject()
        {
            try
            {
                // 检查当前文档和编辑器是否可用
                if (UtilsCADActive.Document == null || UtilsCADActive.Editor == null)
                {
                    return new SelectionResult(null, PromptStatus.Error, "当前没有打开的AutoCAD文档或编辑器不可用");
                }

                // 获取最后创建的对象
                PromptSelectionResult selRes = UtilsCADActive.Editor.SelectLast();

                // 返回结果
                return new SelectionResult(selRes.Value, selRes.Status);
            }
            catch (Exception ex)
            {
                // 记录详细错误信息
                System.Diagnostics.Debug.WriteLine($"UtilsGetLastCreatedObject 发生错误: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                
                return new SelectionResult(null, PromptStatus.Error, ex.Message);
            }
        }

        /// <summary>
        /// 使用窗口选择对象
        /// </summary>
        /// <param name="firstCorner">窗口的第一个角点</param>
        /// <param name="secondCorner">窗口的第二个角点</param>
        /// <param name="filter">选择过滤器，可选</param>
        /// <returns>选择结果</returns>
        public static SelectionResult UtilsGetSelectionSetByWindow(Point3d firstCorner, Point3d secondCorner, SelectionFilter filter = null)
        {
            try
            {
                // 检查当前文档和编辑器是否可用
                if (UtilsCADActive.Document == null || UtilsCADActive.Editor == null)
                {
                    return new SelectionResult(null, PromptStatus.Error, "当前没有打开的AutoCAD文档或编辑器不可用");
                }

                // 使用窗口选择
                PromptSelectionResult selRes = UtilsCADActive.Editor.SelectWindow(firstCorner, secondCorner, filter);

                // 返回结果
                return new SelectionResult(selRes.Value, selRes.Status);
            }
            catch (Exception ex)
            {
                // 记录详细错误信息
                System.Diagnostics.Debug.WriteLine($"UtilsGetSelectionSetByWindow 发生错误: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                
                return new SelectionResult(null, PromptStatus.Error, ex.Message);
            }
        }

        /// <summary>
        /// 使用交叉窗口选择对象
        /// </summary>
        /// <param name="firstCorner">窗口的第一个角点</param>
        /// <param name="secondCorner">窗口的第二个角点</param>
        /// <param name="filter">选择过滤器，可选</param>
        /// <returns>选择结果</returns>
        public static SelectionResult UtilsGetSelectionSetByCrossingWindow(Point3d firstCorner, Point3d secondCorner, SelectionFilter filter = null)
        {
            try
            {
                // 检查当前文档和编辑器是否可用
                if (UtilsCADActive.Document == null || UtilsCADActive.Editor == null)
                {
                    return new SelectionResult(null, PromptStatus.Error, "当前没有打开的AutoCAD文档或编辑器不可用");
                }

                // 使用交叉窗口选择
                PromptSelectionResult selRes = UtilsCADActive.Editor.SelectCrossingWindow(firstCorner, secondCorner, filter);

                // 返回结果
                return new SelectionResult(selRes.Value, selRes.Status);
            }
            catch (Exception ex)
            {
                // 记录详细错误信息
                System.Diagnostics.Debug.WriteLine($"UtilsGetSelectionSetByCrossingWindow 发生错误: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                
                return new SelectionResult(null, PromptStatus.Error, ex.Message);
            }
        }

        /// <summary>
        /// 合并多个选择集
        /// </summary>
        /// <param name="selectionResults">选择结果数组</param>
        /// <returns>合并后的ObjectId集合</returns>
        public static ObjectIdCollection UtilsMergeSelectionSets(params SelectionResult[] selectionResults)
        {
            ObjectIdCollection mergedIds = new ObjectIdCollection();

            try
            {
                if (selectionResults == null || selectionResults.Length == 0)
                {
                    return mergedIds;
                }

                foreach (SelectionResult result in selectionResults)
                {
                    if (result != null && result.IsSuccessful && result.SelectionSet != null)
                    {
                        ObjectId[] objectIds = result.SelectionSet.GetObjectIds();
                        foreach (ObjectId id in objectIds)
                        {
                            if (!mergedIds.Contains(id))
                            {
                                mergedIds.Add(id);
                            }
                        }
                    }
                }

                return mergedIds;
            }
            catch (Exception ex)
            {
                // 记录详细错误信息
                System.Diagnostics.Debug.WriteLine($"UtilsMergeSelectionSets 发生错误: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                
                return mergedIds;
            }
        }

        /// <summary>
        /// 使用选择集处理对象的通用方法，确保使用事务和资源正确释放
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="selectionSet">选择集</param>
        /// <param name="processAction">处理选择集的操作</param>
        /// <returns>处理结果</returns>
        public static T ProcessWithTransaction<T>(SelectionSet selectionSet, Func<Transaction, ObjectIdCollection, T> processAction)
        {
            if (selectionSet == null || selectionSet.Count == 0)
            {
                throw new ArgumentException("选择集为空或无效", nameof(selectionSet));
            }

            if (UtilsCADActive.Document == null || UtilsCADActive.Database == null)
            {
                throw new InvalidOperationException("当前没有打开的AutoCAD文档或数据库不可用");
            }

            // 获取选择集的ObjectId集合
            ObjectIdCollection objectIds = new ObjectIdCollection(selectionSet.GetObjectIds());
            
            // 使用事务处理
            using (Transaction trans = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {
                try
                {
                    // 执行指定的处理操作
                    T result = processAction(trans, objectIds);
                    
                    // 提交事务
                    trans.Commit();
                    
                    return result;
                }
                catch (Exception ex)
                {
                    // 事务出错时回滚
                    trans.Abort();
                    
                    // 记录详细错误信息
                    System.Diagnostics.Debug.WriteLine($"处理选择集时发生错误: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                    
                    // 重新抛出异常以便上层处理
                    throw new Exception($"处理选择集时发生错误: {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// 使用ObjectIdCollection处理对象的通用方法，确保使用事务和资源正确释放
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="objectIds">ObjectId集合</param>
        /// <param name="processAction">处理ObjectId集合的操作</param>
        /// <returns>处理结果</returns>
        public static T ProcessWithTransaction<T>(ObjectIdCollection objectIds, Func<Transaction, ObjectIdCollection, T> processAction)
        {
            if (objectIds == null || objectIds.Count == 0)
            {
                throw new ArgumentException("ObjectId集合为空或无效", nameof(objectIds));
            }

            if (UtilsCADActive.Document == null || UtilsCADActive.Database == null)
            {
                throw new InvalidOperationException("当前没有打开的AutoCAD文档或数据库不可用");
            }
            
            // 使用事务处理
            using (Transaction trans = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {
                try
                {
                    // 执行指定的处理操作
                    T result = processAction(trans, objectIds);
                    
                    // 提交事务
                    trans.Commit();
                    
                    return result;
                }
                catch (Exception ex)
                {
                    // 事务出错时回滚
                    trans.Abort();
                    
                    // 记录详细错误信息
                    System.Diagnostics.Debug.WriteLine($"处理ObjectId集合时发生错误: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                    
                    // 重新抛出异常以便上层处理
                    throw new Exception($"处理ObjectId集合时发生错误: {ex.Message}", ex);
                }
            }
        }
    }
} 