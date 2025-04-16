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
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace dataflow_cs.Utils.CADUtils
{
    /// <summary>
    /// AutoCAD活动会话工具类，提供获取当前活动文档、编辑器和数据库的方法
    /// </summary>
    public static class UtilsCADActive
    {
        /// <summary>
        /// 获取当前活动的编辑器对象
        /// </summary>
        public static Editor Editor
        {
            get { return Document.Editor; }
        }
        
        /// <summary>
        /// 获取当前活动的文档对象
        /// </summary>
        public static Document Document
        {
            get { return Application.DocumentManager.MdiActiveDocument; }
        }
        
        /// <summary>
        /// 获取当前活动的数据库对象
        /// </summary>
        public static Database Database
        {
            get { return Document.Database; }
        }
        
        /// <summary>
        /// 向当前活动的命令行发送消息
        /// </summary>
        /// <param name="message">要发送的消息内容</param>
        public static void WriteMessage(string message)
        {
            Editor.WriteMessage(message);
        }
        
        /// <summary>
        /// 向当前活动的命令行发送格式化消息
        /// </summary>
        /// <param name="message">包含格式说明符的消息模板</param>
        /// <param name="parameter">要替换到格式字符串中的变量</param>
        public static void WriteMessage(string message, params object[] parameter)
        {
            Editor.WriteMessage(message, parameter);
        }

        /// <summary>
        /// 执行AutoCAD命令
        /// </summary>
        /// <param name="commandName">命令名称</param>
        public static void RunCommand(string commandName)
        {
            if (string.IsNullOrEmpty(commandName))
                return;

            try
            {
                // 获取当前文档
                Document doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null)
                {
                    Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n无法执行命令 {commandName}: 没有活动文档");
                    return;
                }

                // 确保命令能被准确执行
                string commandString = commandName.Trim();
                
                // 对命令进行格式化，确保正确执行
                if (!commandString.StartsWith("_")) // 非国际化命令
                {
                    if (!commandString.EndsWith(" "))
                    {
                        commandString += " "; // 添加空格作为命令结束符
                    }
                }
                
                // 执行命令
                doc.SendStringToExecute(commandString, true, false, true);
                
                // 记录命令执行
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n成功执行命令: {commandName}");
            }
            catch (System.Exception ex)
            {
                // 记录错误但不中断操作
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n执行命令 {commandName} 时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 在注册应用程序表（RegAppTable）中添加一个新的记录
        /// 这是必要的，因为在给实体添加扩展数据之前，需要确保扩展数据的应用程序名已经在注册应用程序表中
        /// </summary>
        /// <param name="regAppName">要注册的应用程序名称</param>
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

        /// <summary>
        /// 为对象添加多个扩展数据
        /// </summary>
        /// <param name="objectId">要添加扩展数据的对象ID</param>
        /// <param name="xdataDictList">包含应用程序名和数据内容的字典</param>
        public static void UtilsAddXData(ObjectId objectId, Dictionary<string, string> xdataDictList)
        {
            foreach (KeyValuePair<string, string> xdataDict in xdataDictList)
            {
                UtilsAddOneXData(objectId, xdataDict);
            }
        }

        /// <summary>
        /// 为对象添加单个扩展数据
        /// </summary>
        /// <param name="objectId">要添加扩展数据的对象ID</param>
        /// <param name="xdataDict">包含应用程序名和数据内容的键值对</param>
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

        /// <summary>
        /// 为对象添加单个扩展数据
        /// </summary>
        /// <param name="objectId">要添加扩展数据的对象ID</param>
        /// <param name="regAppName">注册应用程序名称</param>
        /// <param name="xdataContent">扩展数据内容</param>
        public static void UtilsAddOneXData(ObjectId objectId, string regAppName, string xdataContent)
        {
            AddRegAppTableRecord(regAppName);

            ResultBuffer rb = new ResultBuffer(new TypedValue(1001, regAppName), new TypedValue(1000, xdataContent));
            Entity ent = objectId.GetObject(OpenMode.ForWrite) as Entity;
            ent.XData = rb;
            rb.Dispose();
        }

        /// <summary>
        /// 获取对象的扩展数据
        /// </summary>
        /// <param name="objectId">要获取扩展数据的对象ID</param>
        /// <param name="regAppName">注册应用程序名称</param>
        /// <returns>扩展数据内容，如果不存在则返回空字符串</returns>
        public static string UtilsGetXData(ObjectId objectId, string regAppName)
        {
            string result = string.Empty;
            Entity ent = objectId.GetObject(OpenMode.ForRead) as Entity;
            ResultBuffer rb = ent.GetXDataForApplication(regAppName);
            if (rb == null)
            {
                //WriteMessage("\nNo XData found for the application {0}.", regAppName);
                return string.Empty;
            }

            foreach (TypedValue tv in rb)
            {
                if (tv.TypeCode == 1000)
                {
                    result = tv.Value.ToString();
                }
            }

            rb.Dispose();
            return result;
        }

        /// <summary>
        /// 获取实体的扩展数据
        /// </summary>
        /// <param name="ent">要获取扩展数据的实体</param>
        /// <param name="regAppName">注册应用程序名称</param>
        /// <returns>扩展数据内容，如果不存在则返回空字符串</returns>
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

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="objectId">要删除的实体的对象ID</param>
        public static void UtilsDeleteEntity(ObjectId objectId)
        {
            DBObject obj = objectId.GetObject(OpenMode.ForWrite);
            obj.Erase();
        }

        /// <summary>
        /// 从用户获取一个点坐标
        /// </summary>
        /// <returns>用户选择的点坐标，如果用户取消则返回原点(0,0,0)</returns>
        public static Point3d UtilsGetPointFromUser()
        {
            // Prompt the user to select a point
            PromptPointResult result = Editor.GetPoint("\nSelect a point: ");

            if (result.Status == PromptStatus.OK)
            {
                // If the user selected a point, return it
                return result.Value;
            }
            else
            {
                // Otherwise, return a default point
                return new Point3d();
            }
        }
    }
}
