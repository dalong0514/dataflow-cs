// using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using CommonUtils.CADUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dataflow_cs
{
    public class Commands
    {
        [CommandMethod("Hello")] 
        public void Hello()
        {
            Editor editor = CADActive.Editor;
            editor.WriteMessage("Hello, dalong");
        }

        [CommandMethod("ENTCOLOR")]
        public void ChangeEntityColor()
        {

            using (var tr = CADActive.Database.TransactionManager.StartTransaction())
            {

                // Get the block table for the current database
                var blockTable = (BlockTable)tr.GetObject(CADActive.Database.BlockTableId, OpenMode.ForRead);
                // Get the model space block table record
                var modelSpace = (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                RXClass circleClass = RXObject.GetClass(typeof(Circle));
                // Loop through the entities in model space
                foreach (ObjectId objectId in modelSpace)
                {
                    // Look for circles
                    if (objectId.ObjectClass.IsDerivedFrom(circleClass))
                    {
                        var circle = (Circle)tr.GetObject(objectId, OpenMode.ForRead);
                        if (circle.Radius < 1.0)
                        {
                            circle.UpgradeOpen();
                            circle.ColorIndex = 1;
                        }
                    }
                }
                tr.Commit();
            }
        }

        public delegate void TransactionFunc(Action<Transaction> tr);

        public void UsingTransaction(Action<Transaction> action)
        {
            using (var tr = CADActive.Database.TransactionManager.StartTransaction())
            {
                // Invoke the method
                action(tr);
                tr.Commit();
            }
        }

        public void ChangeSmallCircleToRed(Transaction tr)
        {
            // Get the block table for the current database
            var blockTable = (BlockTable)tr.GetObject(CADActive.Database.BlockTableId, OpenMode.ForRead);
            // Get the model space block table record
            var modelSpace = (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);
            RXClass circleClass = RXObject.GetClass(typeof(Circle));
            // Loop through the entities in model space
            foreach (ObjectId objectId in modelSpace)
            {
                // Look for circles
                if (objectId.ObjectClass.IsDerivedFrom(circleClass))
                {
                    var circle = (Circle)tr.GetObject(objectId, OpenMode.ForRead);
                    if (circle.Radius < 1.0)
                    {
                        circle.UpgradeOpen();
                        circle.ColorIndex = 1;
                    }
                }
            }
        }

        [CommandMethod("ENTCOLOR3")]
        public void ChangeEntityColorWithDelegate()
        {
            UsingTransaction(ChangeSmallCircleToRed);
        }



    }
}
