using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using dataflow_cs.Utils.Helpers;
using System;

namespace dataflow_cs.Core.Services
{
    /// <summary>
    /// 活动文档服务，提供对当前AutoCAD文档和相关资源的访问
    /// </summary>
    public static class ActiveDocumentService
    {
        /// <summary>
        /// 获取当前活动文档
        /// </summary>
        /// <returns>活动文档，若无则返回null</returns>
        public static Document GetActiveDocument()
        {
            try
            {
                return Application.DocumentManager.MdiActiveDocument;
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "获取活动文档失败");
                return null;
            }
        }

        /// <summary>
        /// 获取当前数据库
        /// </summary>
        /// <returns>当前数据库，若无则返回null</returns>
        public static Database GetDatabase()
        {
            Document doc = GetActiveDocument();
            return doc?.Database;
        }

        /// <summary>
        /// 获取当前编辑器
        /// </summary>
        /// <returns>当前编辑器，若无则返回null</returns>
        public static Editor GetEditor()
        {
            Document doc = GetActiveDocument();
            return doc?.Editor;
        }

        /// <summary>
        /// 在活动文档中执行事务
        /// </summary>
        /// <param name="action">事务内执行的操作</param>
        /// <returns>操作是否成功</returns>
        public static bool ExecuteInTransaction(Action<Transaction, Database> action)
        {
            Document doc = GetActiveDocument();
            if (doc == null)
            {
                ErrorHandler.HandleError("无法获取活动文档");
                return false;
            }

            Database db = doc.Database;
            
            try
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    try
                    {
                        action(tr, db);
                        tr.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        tr.Abort();
                        ErrorHandler.HandleException(ex, "事务内操作执行失败");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "事务启动失败");
                return false;
            }
        }

        /// <summary>
        /// 在活动文档中执行事务并返回结果
        /// </summary>
        /// <typeparam name="T">返回结果类型</typeparam>
        /// <param name="func">事务内执行的操作</param>
        /// <returns>操作结果</returns>
        public static T ExecuteInTransaction<T>(Func<Transaction, Database, T> func)
        {
            Document doc = GetActiveDocument();
            if (doc == null)
            {
                ErrorHandler.HandleError("无法获取活动文档");
                return default(T);
            }

            Database db = doc.Database;
            
            try
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    try
                    {
                        T result = func(tr, db);
                        tr.Commit();
                        return result;
                    }
                    catch (Exception ex)
                    {
                        tr.Abort();
                        ErrorHandler.HandleException(ex, "事务内操作执行失败");
                        return default(T);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "事务启动失败");
                return default(T);
            }
        }

        /// <summary>
        /// 锁定当前文档
        /// </summary>
        /// <param name="action">锁定期间执行的操作</param>
        /// <returns>操作是否成功</returns>
        public static bool LockDocument(Action action)
        {
            Document doc = GetActiveDocument();
            if (doc == null)
            {
                ErrorHandler.HandleError("无法获取活动文档");
                return false;
            }

            try
            {
                using (doc.LockDocument())
                {
                    action();
                    return true;
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "文档锁定期间操作失败");
                return false;
            }
        }

        /// <summary>
        /// 锁定当前文档并返回结果
        /// </summary>
        /// <typeparam name="T">返回结果类型</typeparam>
        /// <param name="func">锁定期间执行的操作</param>
        /// <returns>操作结果</returns>
        public static T LockDocument<T>(Func<T> func)
        {
            Document doc = GetActiveDocument();
            if (doc == null)
            {
                ErrorHandler.HandleError("无法获取活动文档");
                return default(T);
            }

            try
            {
                using (doc.LockDocument())
                {
                    return func();
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "文档锁定期间操作失败");
                return default(T);
            }
        }
    }
} 