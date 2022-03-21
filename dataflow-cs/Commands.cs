// using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
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
            Editor editor = Application.DocumentManager.MdiActiveDocument.Editor;
            editor.WriteMessage("Hello, dalong");
        }

        [CommandMethod("ENTCOLOR1")]
        public void ChangeEntityColor1()
        {
            // Get the various active objects
            Document document = Application.DocumentManager.MdiActiveDocument;
            Database database = document.Database;
            // Create a new transaction
            Transaction tr = database.TransactionManager.StartTransaction();
            using (tr)
            {
                // Get the block table for the current database
                var blockTable = (BlockTable)tr.GetObject(database.BlockTableId, OpenMode.ForRead);
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

        /// runtime environment.
        /// </summary>
        public static class Active
        {
            /// <summary>
            /// Returns the active Editor object.
            /// </summary>
            public static Editor Editor
            {
                get { return Document.Editor; }
            }
            /// <summary>
            /// Returns the active Document object.
            /// </summary>
            public static Document Document
            {
                get { return Application.DocumentManager.MdiActiveDocument; }
            }
            /// <summary>
            /// Returns the active Database object.
            /// </summary>
            public static Database Database
            {
                get { return Document.Database; }
            }
            /// <summary>
            /// Sends a string to the command line in the active Editor
            /// </summary>
            /// <param name="message">The message to send.</param>
            public static void WriteMessage(string message)
            {
                Editor.WriteMessage(message);
            }
            /// <summary>
            /// Sends a string to the command line in the active Editor using String.Format.
            /// </summary>
            /// <param name="message">The message containing format specifications.</param>
            /// <param name="parameter">The variables to substitute into the format string.</param>
            public static void WriteMessage(string message, params object[] parameter)
            {
                Editor.WriteMessage(message, parameter);
            }
        }

        [CommandMethod("ENTCOLOR2")]
        public void ChangeEntityColor2()
        {

            using (var tr = Active.Database.TransactionManager.StartTransaction())
            {

                // Get the block table for the current database
                var blockTable = (BlockTable)tr.GetObject(Active.Database.BlockTableId, OpenMode.ForRead);
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

    }
}
