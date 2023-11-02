using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using CommonUtils.CADUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GsLcDataFlow
{
    internal class ToolManager
    {
        public static void DLGetAllPolyline()
        {
            using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {
                // 获取模型空间多段线的选择集
                SelectionSet selSet = UtilsSelectionSet.UtilsGetAllPolylineSelectionSet();
                if (selSet == null) return;
                // 删除选择集中的所有对象
                foreach (ObjectId objectId in selSet.GetObjectIds())
                {
                    Entity entity = tr.GetObject(objectId, OpenMode.ForWrite) as Entity;
                    entity.Erase();
                }

                tr.Commit();
            }
        }
    }
}
