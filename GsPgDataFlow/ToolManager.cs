using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using CommonUtils.CADUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GsPgDataFlow
{
    public class ToolManager
    {
        public static void CsTest()
        {
            using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {
                Editor ed = UtilsCADActive.Editor;



                //// 通过拾取获得一个多段线的ObjectId
                //ObjectId polylineId = UtilsCADActive.Editor.GetEntity("\n请选择一个多段线").ObjectId;
                //// 根据多段线的ObjectId获得多段线的对象
                //Polyline polyline = tr.GetObject(polylineId, OpenMode.ForWrite) as Polyline;

                //UtilsCADActive.UtilsAddXData(polylineId, "pipeNum", "PL1101");
                //ed.WriteMessage("\n" + UtilsCADActive.UtilsGetXData(polylineId, "pipeNum"));

                // 通过拾取获得一个块的ObjectId
                ObjectId blockId = UtilsCADActive.Editor.GetEntity("\n请选择一个块").ObjectId;
                string propertyValue = UtilsBlock.UtilsBlockGetPropertyValueByPropertyName(blockId, "pipeNum");
                ed.WriteMessage("\n" + propertyValue);

                tr.Commit();
            }



        }
    }
}
