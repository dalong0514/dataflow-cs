using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using dataflow_cs.Core.Services;
using dataflow_cs.Presentation.Views.Palettes;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace dataflow_cs.Business.Commands.GsLc
{
    /// <summary>
    /// 添加自定义菜单命令
    /// </summary>
    public class GsAddMenuCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsAddMenu";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            try
            {
                // 显示开始执行信息
                editor.WriteMessage("\n开始执行添加自定义菜单命令...");

                // 显示自定义菜单
                GsMenuPalette.ShowCustomMenu();

                editor.WriteMessage("\n自定义菜单已成功添加！");
                return true;
            }
            catch (Exception ex)
            {
                editor.WriteMessage($"\n执行添加自定义菜单命令时发生错误: {ex.Message}");
                return false;
            }
        }
    }
}
