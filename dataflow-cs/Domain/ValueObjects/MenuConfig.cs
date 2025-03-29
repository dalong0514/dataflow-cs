using System.Collections.Generic;

namespace dataflow_cs.Domain.ValueObjects
{
    /// <summary>
    /// 菜单配置模型
    /// </summary>
    public class MenuConfig
    {
        /// <summary>
        /// 面板标题
        /// </summary>
        public string PaletteTitle { get; set; } = "数智设计";
        
        /// <summary>
        /// 面板宽度
        /// </summary>
        public int PaletteWidth { get; set; } = 250;
        
        /// <summary>
        /// 面板高度
        /// </summary>
        public int PaletteHeight { get; set; } = 400;
        
        /// <summary>
        /// 菜单列表
        /// </summary>
        public List<MenuGroup> MenuGroups { get; set; } = new List<MenuGroup>();

        /// <summary>
        /// 创建菜单配置的工厂方法
        /// </summary>
        public static MenuConfig Create(string title, int width, int height)
        {
            return new MenuConfig
            {
                PaletteTitle = title,
                PaletteWidth = width,
                PaletteHeight = height,
                MenuGroups = new List<MenuGroup>()
            };
        }
    }

    /// <summary>
    /// 一级菜单组
    /// </summary>
    public class MenuGroup
    {
        /// <summary>
        /// 菜单标题
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// 菜单图标
        /// </summary>
        public string IconKey { get; set; } = "folder";
        
        /// <summary>
        /// 子菜单项
        /// </summary>
        public List<MenuItem> Items { get; set; } = new List<MenuItem>();
    }

    /// <summary>
    /// 菜单项（二级菜单）
    /// </summary>
    public class MenuItem
    {
        /// <summary>
        /// 菜单标题
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// 菜单图标
        /// </summary>
        public string IconKey { get; set; }
        
        /// <summary>
        /// 点击执行的AutoCAD命令
        /// </summary>
        public string Command { get; set; }
    }
} 