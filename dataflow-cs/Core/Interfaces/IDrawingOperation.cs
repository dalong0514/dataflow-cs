using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;

namespace dataflow_cs.Core.Interfaces
{
    /// <summary>
    /// 绘图操作接口，定义标准的图形操作方法
    /// </summary>
    public interface IDrawingOperation
    {
        /// <summary>
        /// 执行绘图操作
        /// </summary>
        /// <param name="transaction">当前事务</param>
        /// <param name="blockTable">块表</param>
        /// <param name="database">当前数据库</param>
        /// <returns>操作结果，通常包含创建或修改的对象ID</returns>
        IEnumerable<ObjectId> Execute(Transaction transaction, BlockTable blockTable, Database database);

        /// <summary>
        /// 撤销操作
        /// </summary>
        /// <param name="transaction">当前事务</param>
        /// <param name="database">当前数据库</param>
        /// <returns>操作是否成功</returns>
        bool Undo(Transaction transaction, Database database);

        /// <summary>
        /// 验证操作参数
        /// </summary>
        /// <returns>参数是否有效</returns>
        bool ValidateParameters();
    }
} 