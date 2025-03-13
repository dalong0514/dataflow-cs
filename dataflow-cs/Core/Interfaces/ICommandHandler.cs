using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;

namespace dataflow_cs.Core.Interfaces
{
    /// <summary>
    /// 命令处理器接口，定义AutoCAD命令的处理方法
    /// </summary>
    public interface ICommandHandler
    {
        /// <summary>
        /// 命令的名称
        /// </summary>
        string CommandName { get; }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="editor">当前编辑器</param>
        /// <param name="database">当前数据库</param>
        /// <returns>命令执行结果</returns>
        bool Execute(Editor editor, Database database);

        /// <summary>
        /// 验证命令执行前提条件
        /// </summary>
        /// <param name="editor">当前编辑器</param>
        /// <param name="database">当前数据库</param>
        /// <returns>是否满足执行条件</returns>
        bool Validate(Editor editor, Database database);
    }
} 