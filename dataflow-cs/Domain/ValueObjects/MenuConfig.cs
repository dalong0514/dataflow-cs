using System.Collections.Generic;

namespace dataflow_cs.Domain.ValueObjects
{
    /// <summary>
    /// 菜单配置
    /// </summary>
    public class MenuConfig
    {
        /// <summary>
        /// 面板标题
        /// </summary>
        public string PaletteTitle { get; set; }

        /// <summary>
        /// 面板宽度
        /// </summary>
        public int PaletteWidth { get; set; }

        /// <summary>
        /// 面板高度
        /// </summary>
        public int PaletteHeight { get; set; }

        /// <summary>
        /// 菜单组列表（向后兼容）
        /// </summary>
        public List<MenuGroup> MenuGroups { get; set; } = new List<MenuGroup>();

        /// <summary>
        /// 标签页配置
        /// </summary>
        public List<TabConfig> Tabs { get; set; } = new List<TabConfig>();
    }

    /// <summary>
    /// 标签页配置
    /// </summary>
    public class TabConfig
    {
        /// <summary>
        /// 标签页名称
        /// </summary>
        public string TabName { get; set; }

        /// <summary>
        /// 标签页下的菜单组
        /// </summary>
        public List<MenuGroup> MenuGroups { get; set; } = new List<MenuGroup>();
    }

    /// <summary>
    /// 菜单组(一级菜单)
    /// </summary>
    public class MenuGroup
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 图标键
        /// </summary>
        public string IconKey { get; set; }

        /// <summary>
        /// 菜单项集合(二级菜单)
        /// </summary>
        public List<MenuItem> Items { get; set; } = new List<MenuItem>();
    }

    /// <summary>
    /// 菜单项(二级菜单)
    /// </summary>
    public class MenuItem
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 图标键
        /// </summary>
        public string IconKey { get; set; }

        /// <summary>
        /// 执行命令
        /// </summary>
        public string Command { get; set; }
    }
} 