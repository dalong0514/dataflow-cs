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
    /// 提供选择集(SelectionSet)操作的工具类，包括不同类型实体的选择集创建和过滤功能
    /// </summary>
    public static class SelectionSetUtils
    {
        #region 获取所有实体的选择集方法

        /// <summary>
        /// 获取所有块参照的选择集
        /// </summary>
        /// <returns>块参照选择集</returns>
        public static SelectionSet GetAllBlockSelectionSet() => GetAllSelectionSetByFilter("INSERT");

        /// <summary>
        /// 根据图层名获取所有块参照的选择集
        /// </summary>
        /// <param name="layerName">图层名</param>
        /// <returns>块参照选择集</returns>
        public static SelectionSet GetAllBlockSelectionSetByLayerName(string layerName) => GetAllSelectionSetByFilter("INSERT", layerName);

        /// <summary>
        /// 获取所有多行文本的选择集
        /// </summary>
        /// <returns>多行文本选择集</returns>
        public static SelectionSet GetAllMTextSelectionSet() => GetAllSelectionSetByFilter("MText");

        /// <summary>
        /// 根据图层名获取所有多行文本的选择集
        /// </summary>
        /// <param name="layerName">图层名</param>
        /// <returns>多行文本选择集</returns>
        public static SelectionSet GetAllMTextSelectionSetByLayerName(string layerName) => GetAllSelectionSetByFilter("MText", layerName);

        /// <summary>
        /// 获取所有单行文本的选择集
        /// </summary>
        /// <returns>单行文本选择集</returns>
        public static SelectionSet GetAllTextSelectionSet() => GetAllSelectionSetByFilter("Text");

        /// <summary>
        /// 根据图层名获取所有单行文本的选择集
        /// </summary>
        /// <param name="layerName">图层名</param>
        /// <returns>单行文本选择集</returns>
        public static SelectionSet GetAllTextSelectionSetByLayerName(string layerName) => GetAllSelectionSetByFilter("Text", layerName);

        /// <summary>
        /// 获取所有多段线的选择集
        /// </summary>
        /// <returns>多段线选择集</returns>
        public static SelectionSet GetAllPolylineSelectionSet() => GetAllSelectionSetByFilter("LWPOLYLINE");

        /// <summary>
        /// 根据图层名获取所有多段线的选择集
        /// </summary>
        /// <param name="layerName">图层名</param>
        /// <returns>多段线选择集</returns>
        public static SelectionSet GetAllPolylineSelectionSetByLayerName(string layerName) => GetAllSelectionSetByFilter("LWPOLYLINE", layerName);

        #endregion

        #region 通过窗口选择获取所有实体的选择集方法

        /// <summary>
        /// 通过窗口选择获取所有块参照的选择集
        /// </summary>
        /// <param name="extents">范围</param>
        /// <returns>块参照选择集</returns>
        public static SelectionSet GetAllBlockSelectionSetByCrossingWindow(Extents3d extents) => GetSelectionSetByFilterByCrossingWindow("INSERT", extents);

        /// <summary>
        /// 通过窗口选择和图层名获取所有块参照的选择集
        /// </summary>
        /// <param name="extents">范围</param>
        /// <param name="layerName">图层名</param>
        /// <returns>块参照选择集</returns>
        public static SelectionSet GetAllBlockSelectionSetByLayerNameByCrossingWindow(Extents3d extents, string layerName) => GetSelectionSetByFilterByCrossingWindow("INSERT", extents, layerName);

        /// <summary>
        /// 通过窗口选择获取所有多行文本的选择集
        /// </summary>
        /// <param name="extents">范围</param>
        /// <returns>多行文本选择集</returns>
        public static SelectionSet GetAllMTextSelectionSetByCrossingWindow(Extents3d extents) => GetSelectionSetByFilterByCrossingWindow("MText", extents);

        /// <summary>
        /// 通过窗口选择和图层名获取所有多行文本的选择集
        /// </summary>
        /// <param name="extents">范围</param>
        /// <param name="layerName">图层名</param>
        /// <returns>多行文本选择集</returns>
        public static SelectionSet GetAllMTextSelectionSetByLayerNameByCrossingWindow(Extents3d extents, string layerName) => GetSelectionSetByFilterByCrossingWindow("MText", extents, layerName);

        /// <summary>
        /// 通过窗口选择获取所有单行文本的选择集
        /// </summary>
        /// <param name="extents">范围</param>
        /// <returns>单行文本选择集</returns>
        public static SelectionSet GetAllTextSelectionSetByCrossingWindow(Extents3d extents) => GetSelectionSetByFilterByCrossingWindow("Text", extents);

        /// <summary>
        /// 通过窗口选择和图层名获取所有单行文本的选择集
        /// </summary>
        /// <param name="extents">范围</param>
        /// <param name="layerName">图层名</param>
        /// <returns>单行文本选择集</returns>
        public static SelectionSet GetAllTextSelectionSetByLayerNameByCrossingWindow(Extents3d extents, string layerName) => GetSelectionSetByFilterByCrossingWindow("Text", extents, layerName);

        /// <summary>
        /// 通过窗口选择获取所有多段线的选择集
        /// </summary>
        /// <param name="extents">范围</param>
        /// <returns>多段线选择集</returns>
        public static SelectionSet GetAllPolylineSelectionSetByCrossingWindow(Extents3d extents) => GetSelectionSetByFilterByCrossingWindow("LWPOLYLINE", extents);

        /// <summary>
        /// 通过窗口选择和图层名获取所有多段线的选择集
        /// </summary>
        /// <param name="extents">范围</param>
        /// <param name="layerName">图层名</param>
        /// <returns>多段线选择集</returns>
        public static SelectionSet GetAllPolylineSelectionSetByLayerNameByCrossingWindow(Extents3d extents, string layerName) => GetSelectionSetByFilterByCrossingWindow("LWPOLYLINE", extents, layerName);

        #endregion

        #region 用户选择获取实体的选择集方法

        /// <summary>
        /// 用户选择获取块参照的选择集
        /// </summary>
        /// <returns>块参照选择集</returns>
        public static SelectionSet GetBlockSelectionSet() => GetSelectionSetByFilter("INSERT");

        /// <summary>
        /// 用户选择根据图层名获取块参照的选择集
        /// </summary>
        /// <param name="layerName">图层名</param>
        /// <returns>块参照选择集</returns>
        public static SelectionSet GetBlockSelectionSetByLayerName(string layerName) => GetSelectionSetByFilter("INSERT", layerName);

        /// <summary>
        /// 用户选择获取多行文本的选择集
        /// </summary>
        /// <returns>多行文本选择集</returns>
        public static SelectionSet GetMTextSelectionSet() => GetSelectionSetByFilter("MText");

        /// <summary>
        /// 用户选择根据图层名获取多行文本的选择集
        /// </summary>
        /// <param name="layerName">图层名</param>
        /// <returns>多行文本选择集</returns>
        public static SelectionSet GetMTextSelectionSetByLayerName(string layerName) => GetSelectionSetByFilter("MText", layerName);

        /// <summary>
        /// 用户选择获取单行文本的选择集
        /// </summary>
        /// <returns>单行文本选择集</returns>
        public static SelectionSet GetTextSelectionSet() => GetSelectionSetByFilter("Text");

        /// <summary>
        /// 用户选择根据图层名获取单行文本的选择集
        /// </summary>
        /// <param name="layerName">图层名</param>
        /// <returns>单行文本选择集</returns>
        public static SelectionSet GetTextSelectionSetByLayerName(string layerName) => GetSelectionSetByFilter("Text", layerName);

        /// <summary>
        /// 用户选择获取多段线的选择集
        /// </summary>
        /// <returns>多段线选择集</returns>
        public static SelectionSet GetPolylineSelectionSet() => GetSelectionSetByFilter("LWPOLYLINE");

        /// <summary>
        /// 用户选择根据图层名获取多段线的选择集
        /// </summary>
        /// <param name="layerName">图层名</param>
        /// <returns>多段线选择集</returns>
        public static SelectionSet GetPolylineSelectionSetByLayerName(string layerName) => GetSelectionSetByFilter("LWPOLYLINE", layerName);

        #endregion

        #region 核心实现方法

        /// <summary>
        /// 用户选择根据实体类型和图层名过滤获取选择集
        /// </summary>
        /// <param name="entityType">实体类型</param>
        /// <param name="layerName">图层名（可选）</param>
        /// <returns>选择集</returns>
        public static SelectionSet GetSelectionSetByFilter(string entityType, string layerName = null)
        {
            // 创建过滤器值列表
            List<TypedValue> filterValues = new List<TypedValue>
            {
                // 添加实体类型过滤器
                new TypedValue((int)DxfCode.Start, entityType)
            };

            // 如果提供了图层名，添加图层过滤器
            if (!string.IsNullOrEmpty(layerName))
            {
                filterValues.Add(new TypedValue((int)DxfCode.LayerName, layerName));
            }

            // 创建选择过滤器
            SelectionFilter filter = new SelectionFilter(filterValues.ToArray());

            // 获取当前编辑器
            Editor editor = CadEnvironment.Editor;

            // 提示用户选择对象
            PromptSelectionResult selectionResult = editor.GetSelection(filter);

            // 如果成功获取选择集
            if (selectionResult.Status == PromptStatus.OK)
            {
                return selectionResult.Value;
            }

            return null;
        }

        /// <summary>
        /// 获取所有符合实体类型和图层名过滤的选择集
        /// </summary>
        /// <param name="entityType">实体类型</param>
        /// <param name="layerName">图层名（可选）</param>
        /// <returns>选择集</returns>
        public static SelectionSet GetAllSelectionSetByFilter(string entityType, string layerName = null)
        {
            // 创建过滤器值列表
            List<TypedValue> filterValues = new List<TypedValue>
            {
                // 添加实体类型过滤器
                new TypedValue((int)DxfCode.Start, entityType)
            };

            // 如果提供了图层名，添加图层过滤器
            if (!string.IsNullOrEmpty(layerName))
            {
                filterValues.Add(new TypedValue((int)DxfCode.LayerName, layerName));
            }

            // 创建选择过滤器
            SelectionFilter filter = new SelectionFilter(filterValues.ToArray());

            // 获取当前编辑器
            Editor editor = CadEnvironment.Editor;

            // 获取所有符合条件的对象
            PromptSelectionResult selectionResult = editor.SelectAll(filter);

            // 如果成功获取选择集
            if (selectionResult.Status == PromptStatus.OK)
            {
                return selectionResult.Value;
            }

            return null;
        }

        /// <summary>
        /// 通过窗口选择根据实体类型和图层名过滤获取选择集
        /// </summary>
        /// <param name="entityType">实体类型</param>
        /// <param name="extents">范围</param>
        /// <param name="layerName">图层名（可选）</param>
        /// <returns>选择集</returns>
        public static SelectionSet GetSelectionSetByFilterByCrossingWindow(string entityType, Extents3d extents, string layerName = null)
        {
            // 创建过滤器值列表
            List<TypedValue> filterValues = new List<TypedValue>
            {
                // 添加实体类型过滤器
                new TypedValue((int)DxfCode.Start, entityType)
            };

            // 如果提供了图层名，添加图层过滤器
            if (!string.IsNullOrEmpty(layerName))
            {
                filterValues.Add(new TypedValue((int)DxfCode.LayerName, layerName));
            }

            // 创建选择过滤器
            SelectionFilter filter = new SelectionFilter(filterValues.ToArray());

            // 获取当前编辑器
            Editor editor = CadEnvironment.Editor;

            // 通过窗口选择对象
            Point3d minPoint = new Point3d(extents.MinPoint.X, extents.MinPoint.Y, 0);
            Point3d maxPoint = new Point3d(extents.MaxPoint.X, extents.MaxPoint.Y, 0);

            PromptSelectionResult selectionResult = editor.SelectCrossingWindow(minPoint, maxPoint, filter);

            // 如果成功获取选择集
            if (selectionResult.Status == PromptStatus.OK)
            {
                return selectionResult.Value;
            }

            return null;
        }

        #endregion
    }
} 