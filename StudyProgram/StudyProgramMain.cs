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
        public static void Hello()
        {
            Editor editor = UtilsCADActive.Editor;
            editor.WriteMessage("Hello, dalong");
        }

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

                // 任务1: 在AutoCAD中选择一个多行文本(mtext)实体对象
                PromptEntityOptions entOptions = new PromptEntityOptions("\n选择一个多行文本对象:");
                entOptions.SetRejectMessage("\n该实体不是多行文本对象。");
                entOptions.AddAllowedClass(typeof(MText), true);
                PromptEntityResult entResult = ed.GetEntity(entOptions);

                if (entResult.Status != PromptStatus.OK) return;

                MText mtext = tr.GetObject(entResult.ObjectId, OpenMode.ForRead) as MText;

                // 任务2: 获取多行文本的内容并打印到控制台
                if (mtext != null)
                {
                    //Console.WriteLine(mtext.Contents);
                    ed.WriteMessage(mtext.Contents);
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

                // 批量选择块实体对象




                // 任务1: 在AutoCAD中选择一个块实体对象
                PromptEntityOptions entOptions = new PromptEntityOptions("\n选择一个块对象:");
                // 框选块对象


                entOptions.SetRejectMessage("\n该实体不是块对象。");
                entOptions.AddAllowedClass(typeof(BlockReference), true);
                PromptEntityResult entResult = ed.GetEntity(entOptions);

                if (entResult.Status != PromptStatus.OK) return;

                BlockReference blockRef = tr.GetObject(entResult.ObjectId, OpenMode.ForRead) as BlockReference;

                // 任务2: 获取块实体对象的所有属性值并打印到控制台
                if (blockRef != null)
                {
                    // 获取块定义
                    BlockTableRecord btr = tr.GetObject(blockRef.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                    // 获取块定义中的所有属性定义
                    IEnumerable<ObjectId> attDefIds = btr.Cast<ObjectId>().Where(id => id.ObjectClass.Name == "AcDbAttributeDefinition");
                    // 获取块实体对象中的所有属性
                    IEnumerable<ObjectId> attIds = blockRef.AttributeCollection.Cast<ObjectId>();
                    // 获取块实体对象中的所有属性值
                    IEnumerable<AttributeReference> attRefs = attIds.Select(id => tr.GetObject(id, OpenMode.ForRead) as AttributeReference);

                    // 打印属性值
                    foreach (AttributeReference attRef in attRefs)
                    {
                        //Console.WriteLine(attRef.TextString);
                        ed.WriteMessage(attRef.TextString);
                    }

                    // 任务3: 将属性值保存到JSON文件
                    var attributes = attRefs.Select(attRef => new { attRef.Tag, attRef.TextString });
                    var json = JsonConvert.SerializeObject(attributes, Newtonsoft.Json.Formatting.Indented);
                    var filePath = "D:\\testdata.json";
                    File.WriteAllText(filePath, json);
                }

                tr.Commit();
            }
        }   



    }
}
