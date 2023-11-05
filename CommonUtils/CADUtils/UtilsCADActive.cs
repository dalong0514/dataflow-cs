using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

namespace CommonUtils.CADUtils
{
    /// runtime environment.
    /// </summary>
    public static class UtilsCADActive
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

        /// <summary>
        /// 在注册应用程序表（RegAppTable）中添加一个新的记录
        /// 是必要的，因为在给实体添加扩展数据之前，你需要确保扩展数据的应用程序名已经在注册应用程序表中。
        /// </summary>
        /// <param name="regAppName"></param>
        public static void AddRegAppTableRecord(string regAppName)
        {

            Database db = Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                RegAppTable rat = (RegAppTable)tr.GetObject(db.RegAppTableId, OpenMode.ForRead, false);
                if (!rat.Has(regAppName))
                {
                    rat.UpgradeOpen();
                    RegAppTableRecord ratr = new RegAppTableRecord();
                    ratr.Name = regAppName;
                    rat.Add(ratr);
                    tr.AddNewlyCreatedDBObject(ratr, true);
                }
                tr.Commit();
            }
        }

        public static void UtilsAddXData(ObjectId objectId, Dictionary<string, string> xdataDictList)
        {
            foreach (KeyValuePair<string, string> xdataDict in xdataDictList)
            {
                UtilsAddOneXData(objectId, xdataDict);
            }
        }

        public static void UtilsAddOneXData(ObjectId objectId, KeyValuePair<string, string> xdataDict)
        {
            string regAppName = xdataDict.Key;
            string xdataContent = xdataDict.Value;
            AddRegAppTableRecord(regAppName);

            ResultBuffer rb = new ResultBuffer(new TypedValue(1001, regAppName), new TypedValue(1000, xdataContent));
            Entity ent = objectId.GetObject(OpenMode.ForWrite) as Entity;
            ent.XData = rb;
            rb.Dispose();
        }

        public static void UtilsAddOneXData(ObjectId objectId, string regAppName, string xdataContent)
        {
            AddRegAppTableRecord(regAppName);

            ResultBuffer rb = new ResultBuffer(new TypedValue(1001, regAppName), new TypedValue(1000, xdataContent));
            Entity ent = objectId.GetObject(OpenMode.ForWrite) as Entity;
            ent.XData = rb;
            rb.Dispose();
        }

        public static string UtilsGetXData(ObjectId objectId, string regAppName)
        {
            string result = string.Empty;
            Entity ent = objectId.GetObject(OpenMode.ForRead) as Entity;
            ResultBuffer rb = ent.GetXDataForApplication(regAppName);
            if (rb == null)
            {
                WriteMessage("\nNo XData found for the application {0}.", regAppName);
                return string.Empty;
            }

            foreach (TypedValue tv in rb)
            {
                if (tv.TypeCode == 1001)
                {
                    result = tv.Value.ToString();
                }
            }

            rb.Dispose();
            return result;
        }

        public static string UtilsGetXData(Entity ent, string regAppName)
        {
            string result = string.Empty;
            ResultBuffer rb = ent.GetXDataForApplication(regAppName);
            if (rb == null)
            {
                WriteMessage("\nNo XData found for the application {0}.", regAppName);
                return string.Empty;
            }

            foreach (TypedValue tv in rb)
            {
                if (tv.TypeCode == 1001)
                {
                    result = tv.Value.ToString();
                }
            }

            rb.Dispose();
            return result;
        }

    }
}
