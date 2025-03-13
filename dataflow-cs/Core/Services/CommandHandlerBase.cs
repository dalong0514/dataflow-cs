using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using dataflow_cs.Core.Interfaces;
using dataflow_cs.Utils.Helpers;
using System;

namespace dataflow_cs.Core.Services
{
    /// <summary>
    /// 命令处理器基类，提供通用的命令执行流程
    /// </summary>
    public abstract class CommandHandlerBase : ICommandHandler
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public abstract string CommandName { get; }
        
        /// <summary>
        /// 执行命令
        /// </summary>
        public bool Execute(Editor editor, Database database)
        {
            try
            {
                // 记录命令开始
                LoggingService.Instance.LogInfo($"开始执行命令: {CommandName}");
                
                // 验证执行条件
                if (!Validate(editor, database))
                {
                    ErrorHandler.HandleError($"命令 {CommandName} 的执行条件不满足，无法执行。");
                    return false;
                }
                
                // 执行前准备工作
                if (!BeforeExecute(editor, database))
                {
                    return false;
                }
                
                // 执行主要逻辑
                bool result = ExecuteCore(editor, database);
                
                // 执行后处理工作
                AfterExecute(editor, database, result);
                
                // 记录命令完成
                LoggingService.Instance.LogInfo($"命令 {CommandName} 执行完成，结果: {(result ? "成功" : "失败")}");
                
                return result;
            }
            catch (System.Exception ex)
            {
                // 处理异常
                ErrorHandler.HandleException(ex, $"执行命令 {CommandName} 过程中发生异常");
                return false;
            }
        }
        
        /// <summary>
        /// 验证命令执行条件
        /// </summary>
        public virtual bool Validate(Editor editor, Database database)
        {
            // 默认实现：检查是否有活动文档和编辑器
            if (editor == null || database == null)
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 执行命令前的准备工作
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>是否可以继续执行</returns>
        protected virtual bool BeforeExecute(Editor editor, Database database)
        {
            // 默认实现：显示命令开始信息
            editor.WriteMessage($"\n开始执行 {CommandName} 命令...");
            return true;
        }
        
        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected abstract bool ExecuteCore(Editor editor, Database database);
        
        /// <summary>
        /// 命令执行后的处理工作
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <param name="success">命令是否成功执行</param>
        protected virtual void AfterExecute(Editor editor, Database database, bool success)
        {
            // 默认实现：显示命令完成信息
            string message = success
                ? $"\n命令 {CommandName} 执行成功。"
                : $"\n命令 {CommandName} 执行失败。";
            
            editor.WriteMessage(message);
        }
        
        /// <summary>
        /// 请求用户输入点
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="message">提示消息</param>
        /// <returns>用户输入的点，若取消则返回null</returns>
        protected Point3d? GetPointFromUser(Editor editor, string message)
        {
            PromptPointOptions options = new PromptPointOptions(message);
            options.AllowNone = false;
            
            PromptPointResult result = editor.GetPoint(options);
            if (result.Status == PromptStatus.OK)
            {
                return result.Value;
            }
            
            return null;
        }
        
        /// <summary>
        /// 请求用户选择对象
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="message">提示消息</param>
        /// <param name="singleSelect">是否只选择单个对象</param>
        /// <returns>选择结果</returns>
        protected PromptSelectionResult GetSelectionFromUser(Editor editor, string message, bool singleSelect = false)
        {
            if (singleSelect)
            {
                PromptEntityOptions options = new PromptEntityOptions($"\n{message}");
                PromptEntityResult result = editor.GetEntity(options);
                
                if (result.Status == PromptStatus.OK)
                {
                    SelectionSet selSet = new SelectionSet(new ObjectId[] { result.ObjectId });
                    return new PromptSelectionResult(selSet, result.Status);
                }
                
                return new PromptSelectionResult(null, result.Status);
            }
            else
            {
                PromptSelectionOptions options = new PromptSelectionOptions();
                options.MessageForAdding = $"\n{message}";
                
                return editor.GetSelection(options);
            }
        }
        
        /// <summary>
        /// 请求用户输入字符串
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="message">提示消息</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>用户输入的字符串，若取消则返回null</returns>
        protected string GetStringFromUser(Editor editor, string message, string defaultValue = "")
        {
            PromptStringOptions options = new PromptStringOptions($"\n{message}");
            options.AllowSpaces = true;
            options.DefaultValue = defaultValue;
            
            PromptResult result = editor.GetString(options);
            if (result.Status == PromptStatus.OK)
            {
                return result.StringResult;
            }
            
            return null;
        }
    }
} 