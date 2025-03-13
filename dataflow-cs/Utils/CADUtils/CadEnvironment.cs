using System;
using System.Collections.Generic;
using System.Linq;
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
    /// 提供CAD环境相关的工具类，包括获取当前活动文档、数据库、编辑器等功能
    /// </summary>
    public static class CadEnvironment
    {
        /// <summary>
        /// 获取当前活动编辑器对象
        /// </summary>
        public static Editor Editor
        {
            get { return Document.Editor; }
        }

        /// <summary>
        /// 获取当前活动文档对象
        /// </summary>
        public static Document Document
        {
            get { return Application.DocumentManager.MdiActiveDocument; }
        }

        /// <summary>
        /// 获取当前活动数据库对象
        /// </summary>
        public static Database Database
        {
            get { return Document.Database; }
        }

        /// <summary>
        /// 向命令行发送消息
        /// </summary>
        /// <param name="message">消息内容</param>
        public static void WriteMessage(string message)
        {
            Editor.WriteMessage(message);
        }

        /// <summary>
        /// 向命令行发送格式化消息
        /// </summary>
        /// <param name="message">格式化字符串</param>
        /// <param name="parameters">格式化参数</param>
        public static void WriteMessage(string message, params object[] parameters)
        {
            Editor.WriteMessage(string.Format(message, parameters));
        }

        /// <summary>
        /// 添加应用程序注册记录
        /// </summary>
        /// <param name="regAppName">应用程序名称</param>
        public static void AddRegAppTableRecord(string regAppName)
        {
            Document doc = Document;
            Database db = Database;

            // 开始事务
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 获取注册应用程序表
                RegAppTable regTable = trans.GetObject(db.RegAppTableId, OpenMode.ForRead) as RegAppTable;

                // 如果注册表中不存在该应用程序名称
                if (!regTable.Has(regAppName))
                {
                    // 升级注册表访问权限为写入
                    regTable.UpgradeOpen();

                    // 创建新的注册应用程序表记录
                    RegAppTableRecord regRecord = new RegAppTableRecord();
                    regRecord.Name = regAppName;

                    // 添加记录到注册表并所有事务
                    regTable.Add(regRecord);
                    trans.AddNewlyCreatedDBObject(regRecord, true);
                }

                // 提交事务
                trans.Commit();
            }
        }

        /// <summary>
        /// 为对象添加扩展数据
        /// </summary>
        /// <param name="objectId">对象的ObjectId</param>
        /// <param name="xdataDictList">扩展数据字典</param>
        public static void AddXData(ObjectId objectId, Dictionary<string, string> xdataDictList)
        {
            foreach (KeyValuePair<string, string> xdataDict in xdataDictList)
            {
                AddOneXData(objectId, xdataDict);
            }
        }

        /// <summary>
        /// 为对象添加单个扩展数据
        /// </summary>
        /// <param name="objectId">对象的ObjectId</param>
        /// <param name="xdataDict">扩展数据键值对</param>
        public static void AddOneXData(ObjectId objectId, KeyValuePair<string, string> xdataDict)
        {
            string regAppName = xdataDict.Key;
            string xdataContent = xdataDict.Value;

            // 添加注册应用程序记录
            AddRegAppTableRecord(regAppName);

            // 添加扩展数据
            AddOneXData(objectId, regAppName, xdataContent);
        }

        /// <summary>
        /// 为对象添加单个扩展数据
        /// </summary>
        /// <param name="objectId">对象的ObjectId</param>
        /// <param name="regAppName">注册应用程序名称</param>
        /// <param name="xdataContent">扩展数据内容</param>
        public static void AddOneXData(ObjectId objectId, string regAppName, string xdataContent)
        {
            using (Transaction trans = Database.TransactionManager.StartTransaction())
            {
                Entity ent = trans.GetObject(objectId, OpenMode.ForWrite) as Entity;
                ResultBuffer rb = new ResultBuffer(new TypedValue(1001, regAppName), new TypedValue(1000, xdataContent));
                ent.XData = rb;
                trans.Commit();
            }
        }

        /// <summary>
        /// 获取对象的扩展数据
        /// </summary>
        /// <param name="objectId">对象的ObjectId</param>
        /// <param name="regAppName">注册应用程序名称</param>
        /// <returns>扩展数据内容</returns>
        public static string GetXData(ObjectId objectId, string regAppName)
        {
            string xdataContent = "";

            using (Transaction trans = Database.TransactionManager.StartTransaction())
            {
                // 获取对象
                Entity ent = trans.GetObject(objectId, OpenMode.ForRead) as Entity;

                // 如果对象存在
                if (ent != null)
                {
                    // 获取扩展数据
                    xdataContent = GetXData(ent, regAppName);
                }

                trans.Commit();
            }

            return xdataContent;
        }

        /// <summary>
        /// 获取实体的扩展数据
        /// </summary>
        /// <param name="ent">实体对象</param>
        /// <param name="regAppName">注册应用程序名称</param>
        /// <returns>扩展数据内容</returns>
        public static string GetXData(Entity ent, string regAppName)
        {
            string xdataContent = "";

            // 获取扩展数据
            ResultBuffer rb = ent.GetXDataForApplication(regAppName);

            // 如果存在扩展数据
            if (rb != null)
            {
                // 遍历结果缓冲区
                foreach (TypedValue value in rb)
                {
                    // 如果找到字符串类型的扩展数据（1000）
                    if (value.TypeCode == 1000)
                    {
                        xdataContent = value.Value.ToString();
                        break;
                    }
                }

                // 释放结果缓冲区
                rb.Dispose();
            }

            return xdataContent;
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="objectId">实体的ObjectId</param>
        public static void DeleteEntity(ObjectId objectId)
        {
            using (Transaction trans = Database.TransactionManager.StartTransaction())
            {
                Entity ent = trans.GetObject(objectId, OpenMode.ForWrite) as Entity;
                ent.Erase();
                trans.Commit();
            }
        }

        /// <summary>
        /// 从用户获取点坐标
        /// </summary>
        /// <returns>点坐标</returns>
        public static Point3d GetPointFromUser()
        {
            PromptPointResult ppr = Editor.GetPoint("\n选择点: ");
            if (ppr.Status == PromptStatus.OK)
            {
                return ppr.Value;
            }
            return new Point3d(0, 0, 0);
        }
    }
} 