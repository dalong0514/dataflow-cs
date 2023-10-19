using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtils.CADUtils;

namespace CommonUtils.CADUtils
{

    public static class UtilsSelectionSet
    {
        // AutoCAD中获得所有块实体对象的选择集
        public static SelectionSet UtilsGetAllBlockSelectionSet() => UtilsGetAllSelectionSetByEntityType("INSERT");
        public static SelectionSet UtilsGetAllMTextSelectionSet() => UtilsGetAllSelectionSetByEntityType("MText");
        public static SelectionSet UtilsGetAllTextSelectionSet() => UtilsGetAllSelectionSetByEntityType("Text");

        // AutoCAD中获得所有块实体对象的选择集
        public static SelectionSet UtilsGetBlockSelectionSet() => UtilsGetSelectionSetByEntityType("INSERT");
        public static SelectionSet UtilsGetMTextSelectionSet() => UtilsGetSelectionSetByEntityType("MText");
        public static SelectionSet UtilsGetTextSelectionSet() => UtilsGetSelectionSetByEntityType("Text");

        public static SelectionSet UtilsGetSelectionSetByEntityType(string entityType)
        {
            // Create a new selection filter for block references
            TypedValue[] filterList = new TypedValue[]
            {
                new TypedValue((int)DxfCode.Start, entityType)
            };
            SelectionFilter filter = new SelectionFilter(filterList);

            // Prompt the user to select block references
            PromptSelectionOptions opts = new PromptSelectionOptions();
            // 当用户在选择对象时，这个字符串将作为提示信息显示
            opts.MessageForAdding = "Select block references: ";
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

            //UtilsCADActive.Editor.WriteMessage("\nNumber of objects selected: " + selSet.Count.ToString());

            return selSet;
        }

        // AutoCAD中通过实体类型获得所有实体对象的选择集，无需用户选择
        public static SelectionSet UtilsGetAllSelectionSetByEntityType(string entityType)
        {
            // Create a new selection filter for block references
            TypedValue[] filterList = new TypedValue[]
            {
                new TypedValue((int)DxfCode.Start, entityType)
            };
            SelectionFilter filter = new SelectionFilter(filterList);

            PromptSelectionResult selRes = UtilsCADActive.Editor.SelectAll(filter);
            if (selRes.Status != PromptStatus.OK) return null;

            SelectionSet selSet = selRes.Value;
            if (selSet == null) return null;

            return selSet;
        }

    }
}
