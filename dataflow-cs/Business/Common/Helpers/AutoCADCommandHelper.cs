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
                    return;

                // 执行命令，确保添加空格作为命令结束符
                doc.SendStringToExecute($"{commandName} ", true, false, true);
            }
            catch (Exception ex)
            {
                // 记录错误但不中断操作
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n执行命令 {commandName} 时出错: {ex.Message}");
            }
        }
    }
} 