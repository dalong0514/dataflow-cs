using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using CommonUtils.CADUtils;
using Newtonsoft.Json;

namespace StudyProgram
{
    public static class StudyProgramMain
    {

        public static void ChangeEntityColor()
        {

            using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {
                // Get the block table for the current database
                var blockTable = (BlockTable)tr.GetObject(UtilsCADActive.Database.BlockTableId, OpenMode.ForRead);
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
            using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {
                // Get the block table for the current database
                var blockTable = (BlockTable)tr.GetObject(UtilsCADActive.Database.BlockTableId, OpenMode.ForRead);
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

        public static void CsTestGetMTextContent()
        {
            using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {
                Document doc = UtilsCADActive.Document;
                Database db = UtilsCADActive.Database;
                Editor ed = UtilsCADActive.Editor;

                SelectionSet selSet = UtilsSelectionSet.UtilsGetMTextSelectionSet();

                // 任务: 根据块实体对象的选择集获取所有多行文字实体对象的文字内容
                if (selSet != null)
                {
                    foreach (ObjectId objectId in selSet.GetObjectIds())
                    {
                        MText mtext = tr.GetObject(objectId, OpenMode.ForRead) as MText;
                        if (mtext != null)
                        {
                            //Console.WriteLine(mtext.Contents);
                            ed.WriteMessage(mtext.Contents);
                        }
                    }
                }

                tr.Commit();
            }
        }

        // 任务1: 在AutoCAD中获取所有块实体对象的所有属性值
        // 任务2: 将任务1获取的属性值在本地生成一份标准的json文件，文件名为{D:\\testdata.json}
        public static void CsTestGetAllBlockAttributes()
        {
            using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {
                Document doc = UtilsCADActive.Document;
                Database db = UtilsCADActive.Database;
                Editor ed = UtilsCADActive.Editor;

                // 任务1: 在AutoCAD中获得块实体对象的选择集
                SelectionSet selSet =  UtilsSelectionSet.UtilsGetBlockSelectionSet();


                // 任务2: 根据块实体对象的选择集获取所有块实体对象的所有属性值
                if (selSet != null)
                {
                    // 获取块选择集中的所有块实体对象的属性值
                    List<Dictionary<string, string>> blockAttributes = new List<Dictionary<string, string>>();
                    foreach (ObjectId objectId in selSet.GetObjectIds())
                    {
                        BlockReference blockRef = tr.GetObject(objectId, OpenMode.ForRead) as BlockReference;
                        if (blockRef != null)
                        {
                            // 过滤掉没有属性的块实体对象
                            if (blockRef.AttributeCollection.Count == 0) continue;
                            // 获取块实体对象的属性值
                            Dictionary<string, string> blockAttribute = new Dictionary<string, string>();

                            foreach (ObjectId attId in blockRef.AttributeCollection)
                            {
                                AttributeReference attRef = tr.GetObject(attId, OpenMode.ForRead) as AttributeReference;
                                if (attRef != null)
                                {
                                    blockAttribute.Add(attRef.Tag, attRef.TextString);
                                }
                            }
                            blockAttributes.Add(blockAttribute);
                        }
                    }

                    // 任务3: 所有属性值写到json文件中
                    string json = JsonConvert.SerializeObject(blockAttributes);
                    string path = "D:\\testdata.json";
                    File.WriteAllText(path, json);

                }

                tr.Commit();
            }
        }   

    }
}
