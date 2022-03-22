using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using CommonUtils.CADUtils;

namespace StudyProgram
{
    public static class StudyProgramMain
    {
        public static void Hello()
        {
            Editor editor = CADActive.Editor;
            editor.WriteMessage("Hello, dalong");
        }

        public static void ChangeEntityColor()
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

        public static void CreateOneLine()
        {
            using (var tr = CADActive.Database.TransactionManager.StartTransaction())
            {
                // Get the block table for the current database
                var blockTable = (BlockTable)tr.GetObject(CADActive.Database.BlockTableId, OpenMode.ForRead);
                // Get the model space block table record
                var modelSpace = (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                Point3d firstInspt = new Point3d(0, 0, 0);
                Point3d secondInspt = new Point3d(10, 10, 0);
                Line lineObj = new Line(firstInspt, secondInspt);
                modelSpace.AppendEntity(lineObj);
                tr.AddNewlyCreatedDBObject(lineObj, true);
                tr.Commit();
            }
        }
    }
}
