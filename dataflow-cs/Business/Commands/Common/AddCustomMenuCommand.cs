using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using dataflow_cs.Core.Services;
using Autodesk.AutoCAD.Windows;
using System.Windows.Forms;
using System.Drawing;
using Autodesk.AutoCAD.ApplicationServices;
using System.IO;
using System.Drawing.Drawing2D;
using dataflow_cs.Domain.ValueObjects;
using dataflow_cs.Business.Services;
using dataflow_cs.Infrastructure.Configuration;
using dataflow_cs.Infrastructure.AutoCAD.Services;
using dataflow_cs.Presentation.Views.Controls;
using dataflow_cs.Presentation.Views.Palettes;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Panel = System.Windows.Forms.Panel;

namespace dataflow_cs.Business.Commands.Common
{
    /// <summary>
    /// 添加自定义菜单命令
    /// </summary>
    public class AddCustomMenuCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLAddCustomMenu";

        private readonly MenuConfigService _menuConfigService;

        public AddCustomMenuCommand()
        {
            // 使用依赖注入原则，初始化服务
            var repository = new FileMenuConfigRepository();
            _menuConfigService = new MenuConfigService(repository);
        }

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
                
                // 加载菜单配置
                editor.WriteMessage("\n正在加载菜单配置...");
                MenuConfig config = _menuConfigService.LoadMenuConfig();
                
                // 输出配置信息，帮助诊断
                editor.WriteMessage($"\n配置加载成功:");
                editor.WriteMessage($"\n - 面板标题: {config.PaletteTitle}");
                editor.WriteMessage($"\n - 菜单组数量: {config.MenuGroups?.Count ?? 0}");
                if (config.MenuGroups != null && config.MenuGroups.Count > 0)
                {
                    foreach (var group in config.MenuGroups)
                    {
                        editor.WriteMessage($"\n - 菜单组: {group.Title}, 包含 {group.Items?.Count ?? 0} 个子项");
                    }
                }
                
                // 显示自定义菜单
                editor.WriteMessage("\n正在显示自定义菜单...");
                CustomMenuPalette.Show(config);
                
                editor.WriteMessage("\n自定义菜单已成功添加！");
                return true;
            }
            catch (Exception ex)
            {
                editor.WriteMessage($"\n执行添加自定义菜单命令时发生错误: {ex.Message}");
                editor.WriteMessage($"\n错误堆栈: {ex.StackTrace}");
                return false;
            }
        }
    }
}
