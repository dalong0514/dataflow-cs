using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.Geometry;

namespace dataflow_cs.Utils.CADUtils
{
    public static class UtilsSelectionSet
    {
        // get all entity selection set by filter
        public static SelectionSet UtilsGetAllBlockSelectionSet() => UtilsGetAllSelectionSetByFilter("INSERT");
        public static SelectionSet UtilsGetAllBlockSelectionSetByLayerName(string layerName) => UtilsGetAllSelectionSetByFilter("INSERT", layerName);
        public static SelectionSet UtilsGetAllMTextSelectionSet() => UtilsGetAllSelectionSetByFilter("MText");
        public static SelectionSet UtilsGetAllMTextSelectionSetByLayerName(string layerName) => UtilsGetAllSelectionSetByFilter("MText", layerName);
        public static SelectionSet UtilsGetAllTextSelectionSet() => UtilsGetAllSelectionSetByFilter("Text");
        public static SelectionSet UtilsGetAllTextSelectionSetByLayerName(string layerName) => UtilsGetAllSelectionSetByFilter("Text", layerName);
        public static SelectionSet UtilsGetAllPolylineSelectionSet() => UtilsGetAllSelectionSetByFilter("LWPOLYLINE");
        public static SelectionSet UtilsGetAllPolylineSelectionSetByLayerName(string layerName) => UtilsGetAllSelectionSetByFilter("LWPOLYLINE", layerName);

        // get all entity selection set by filter
        public static SelectionSet UtilsGetAllBlockSelectionSetByCrossingWindow(Extents3d extents) => UtilsGetSelectionSetByFilterByCrossingWindow("INSERT", extents);
        public static SelectionSet UtilsGetAllBlockSelectionSetByLayerNameByCrossingWindow(Extents3d extents, string layerName) => UtilsGetSelectionSetByFilterByCrossingWindow("INSERT", extents, layerName);
        public static SelectionSet UtilsGetAllMTextSelectionSetByCrossingWindow(Extents3d extents) => UtilsGetSelectionSetByFilterByCrossingWindow("MText", extents);
        public static SelectionSet UtilsGetAllMTextSelectionSetByLayerNameByCrossingWindow(Extents3d extents, string layerName) => UtilsGetSelectionSetByFilterByCrossingWindow("MText", extents, layerName);
        public static SelectionSet UtilsGetAllTextSelectionSetByCrossingWindow(Extents3d extents) => UtilsGetSelectionSetByFilterByCrossingWindow("Text", extents);
        public static SelectionSet UtilsGetAllTextSelectionSetByLayerNameByCrossingWindow(Extents3d extents, string layerName) => UtilsGetSelectionSetByFilterByCrossingWindow("Text", extents, layerName);
        public static SelectionSet UtilsGetAllPolylineSelectionSetByCrossingWindow(Extents3d extents) => UtilsGetSelectionSetByFilterByCrossingWindow("LWPOLYLINE", extents);
        public static SelectionSet UtilsGetAllPolylineSelectionSetByLayerNameByCrossingWindow(Extents3d extents, string layerName) => UtilsGetSelectionSetByFilterByCrossingWindow("LWPOLYLINE", extents, layerName);

        // get entity selection set by filter
        public static SelectionSet UtilsGetBlockSelectionSet() => UtilsGetSelectionSetByFilter("INSERT");
        public static SelectionSet UtilsGetBlockSelectionSetByLayerName(string layerName) => UtilsGetSelectionSetByFilter("INSERT", layerName);
        public static SelectionSet UtilsGetMTextSelectionSet() => UtilsGetSelectionSetByFilter("MText");
        public static SelectionSet UtilsGetMTextSelectionSetByLayerName(string layerName) => UtilsGetSelectionSetByFilter("MText", layerName);
        public static SelectionSet UtilsGetTextSelectionSet() => UtilsGetSelectionSetByFilter("Text");
        public static SelectionSet UtilsGetTextSelectionSetByLayerName(string layerName) => UtilsGetSelectionSetByFilter("Text", layerName);
        public static SelectionSet UtilsGetPolylineSelectionSet() => UtilsGetSelectionSetByFilter("LWPOLYLINE");
        public static SelectionSet UtilsGetPolylineSelectionSetByLayerName(string layerName) => UtilsGetSelectionSetByFilter("LWPOLYLINE", layerName);

        public static SelectionSet UtilsGetSelectionSetByFilter(string entityType, string layerName = null)
        {
            // Create a new list for filter values
            List<TypedValue> filterValues = new List<TypedValue>
            {
                // Add entity type filter
                new TypedValue((int)DxfCode.Start, entityType)
            };
            // Add layer name filter if layerName is not null
            if (layerName != null)
            {
                filterValues.Add(new TypedValue((int)DxfCode.LayerName, layerName));
            }
            // Convert the list to an array
            TypedValue[] filterList = filterValues.ToArray();
            SelectionFilter filter = new SelectionFilter(filterList);

            // Prompt the user to select block references
            PromptSelectionOptions opts = new PromptSelectionOptions();
            // 当用户在选择对象时，这个字符串将作为提示信息显示
            opts.MessageForAdding = "Select: ";
            // 如果设置为true，则允许在选择集中包含重复的对象。如果设置为false，则选择集中不会包含重复的对象
            opts.AllowDuplicates = false;
            // 如果设置为true，则用户只能选择一个对象。如果设置为false，则用户可以选择多个对象
            opts.SingleOnly = false;
            // 如果设置为true，则用户每次只能在一个空间（例如模型空间或布局空间）中选择对象。如果设置为false，则用户可以在多个空间中选择对象
            opts.SinglePickInSpace = true;
            // 如果设置为true，则用户不能选择非当前空间的对象。如果设置为false，则用户可以选择非当前空间的对象
            opts.RejectObjectsFromNonCurrentSpace = true;
            // 如果设置为true，则用户不能选择被锁定图层上的对象。如果设置为false，则用户可以选择被锁定图层上的对象
            opts.RejectObjectsOnLockedLayers = false;
            // 如果设置为true，则用户不能选择布局空间视口中的对象。如果设置为false，则用户可以选择布局空间视口中的对象
            opts.RejectPaperspaceViewport = true;

            PromptSelectionResult selRes = UtilsCADActive.Editor.GetSelection(opts, filter);
            if (selRes.Status != PromptStatus.OK) return null;

            SelectionSet selSet = selRes.Value;
            if (selSet == null) return null;

            return selSet;
        }

        // AutoCAD中通过实体类型获得所有实体对象的选择集，无需用户选择
        public static SelectionSet UtilsGetAllSelectionSetByFilter(string entityType, string layerName = null)
        {
            // Create a new list for filter values
            List<TypedValue> filterValues = new List<TypedValue>
            {
                // Add entity type filter
                new TypedValue((int)DxfCode.Start, entityType)
            };
            // Add layer name filter if layerName is not null
            if (layerName != null)
            {
                filterValues.Add(new TypedValue((int)DxfCode.LayerName, layerName));
            }
            // Convert the list to an array
            TypedValue[] filterList = filterValues.ToArray();
            SelectionFilter filter = new SelectionFilter(filterList);

            PromptSelectionResult selRes = UtilsCADActive.Editor.SelectAll(filter);
            if (selRes.Status != PromptStatus.OK) return null;

            SelectionSet selSet = selRes.Value;
            if (selSet == null) return null;

            return selSet;
        }

        public static SelectionSet UtilsGetSelectionSetByFilterByCrossingWindow(string entityType, Extents3d extents, string layerName = null)
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
                SelectionFilter filter = new SelectionFilter(filterList);

                // 使用交叉窗口选择
                Point3d minPoint = new Point3d(extents.MinPoint.X, extents.MinPoint.Y, extents.MinPoint.Z);
                Point3d maxPoint = new Point3d(extents.MaxPoint.X, extents.MaxPoint.Y, extents.MaxPoint.Z);

                // 尝试选择对象
                PromptSelectionResult selRes = UtilsCADActive.Editor.SelectCrossingWindow(minPoint, maxPoint, filter);
                
                // 检查选择结果
                if (selRes.Status != PromptStatus.OK)
                {
                    System.Diagnostics.Debug.WriteLine($"选择操作未成功完成，状态: {selRes.Status}");
                    return null;
                }

                if (selRes.Value == null || selRes.Value.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("选择集为空");
                    return null;
                }

                return selRes.Value;
            }
            catch (Exception ex)
            {
                // 记录详细错误信息
                System.Diagnostics.Debug.WriteLine($"UtilsGetSelectionSetByFilterByCrossingWindow 发生错误: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                
                // 重新抛出异常以便上层处理
                throw new Exception($"选择对象时发生错误: {ex.Message}", ex);
            }
        }
    }
} 