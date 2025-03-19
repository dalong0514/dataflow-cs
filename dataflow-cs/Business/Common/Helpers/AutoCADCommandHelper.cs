using System;
using Autodesk.AutoCAD.ApplicationServices;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace dataflow_cs.Business.Common.Helpers
{
    /// <summary>
    /// AutoCAD命令执行辅助类
    /// </summary>
    public static class AutoCADCommandHelper
    {
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
            catch (Exception ex)
            {
                // 记录错误但不中断操作
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n执行命令 {commandName} 时出错: {ex.Message}");
            }
        }
    }
} 