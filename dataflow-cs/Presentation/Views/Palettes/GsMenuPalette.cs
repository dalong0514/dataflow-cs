using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Windows;
using dataflow_cs.Domain.ValueObjects;
using dataflow_cs.Business.Services;
using dataflow_cs.Presentation.Views.Controls;
using dataflow_cs.Utils.CADUtils;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace dataflow_cs.Presentation.Views.Palettes
{
    /// <summary>
    /// 工艺专业自定义菜单面板
    /// </summary>
    public class GsMenuPalette
    {
        private static PaletteSet _paletteSet;
        // 保存面板引用以便刷新
        private static Panel _menuPanel;
        private static CustomTreeView _treeView;
        // 选项卡容器和选项卡面板
        private static TabControl _tabControl;
        private static List<CustomTreeView> _tabTreeViews;
        // 不再使用硬编码的标签页名称，改为从配置读取
        private static List<string> _tabNames = new List<string>();
        // 添加静态ImageList字段以便所有TreeView共享
        private static ImageList _sharedImageList;

        /// <summary>
        /// 显示工艺专业自定义菜单
        /// </summary>
        /// <returns>面板对象</returns>
        public static PaletteSet ShowCustomMenu()
        {
            try
            {
                // 每次都重新加载菜单配置，确保能获取最新修改
                MenuConfig config = GsMenuConfigService.LoadMenuConfig();
                
                // 从配置读取标签页名称
                _tabNames.Clear();
                if (config.Tabs != null && config.Tabs.Count > 0)
                {
                    foreach (var tab in config.Tabs)
                    {
                        _tabNames.Add(tab.TabName);
                    }
                }
                else
                {
                    // 如果配置中没有标签页，则使用默认标签页名称
                    _tabNames.AddRange(new string[] { "工艺流程", "设备布置", "二维配管" });
                }

                if (_paletteSet == null)
                {
                    // 创建共享的ImageList并只加载一次图标
                    _sharedImageList = new ImageList
                    {
                        ImageSize = new System.Drawing.Size(16, 16),
                        ColorDepth = ColorDepth.Depth32Bit
                    };
                    LoadAutoCADIcons(_sharedImageList);
                    
                    // 创建 PaletteSet 作为 AutoCAD 固定面板
                    _paletteSet = new PaletteSet(config.PaletteTitle)
                    {
                        Size = new System.Drawing.Size(config.PaletteWidth, config.PaletteHeight),
                        Style = PaletteSetStyles.ShowPropertiesMenu |
                                PaletteSetStyles.ShowCloseButton |
                                PaletteSetStyles.ShowAutoHideButton |
                                PaletteSetStyles.SingleColDock,
                        KeepFocus = false,
                        Dock = DockSides.Left,
                        DockEnabled = DockSides.Left
                    };

                    // 创建承载自定义菜单的 Panel，并设置上下间距
                    _menuPanel = new Panel
                    {
                        BackColor = System.Drawing.Color.LightGray,
                        Dock = DockStyle.Fill,
                        Padding = new Padding(5)
                    };

                    // 添加一个标题标签（与 TreeView 分开，留出间距）
                    // Label titleLabel = new Label
                    // {
                    //     Text = "数智设计-工艺",
                    //     Dock = DockStyle.Top,
                    //     Height = 30,
                    //     TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                    //     BackColor = System.Drawing.Color.Gray,
                    //     ForeColor = System.Drawing.Color.White
                    // };

                    // 创建选项卡控件
                    _tabControl = new TabControl
                    {
                        Dock = DockStyle.Fill,
                        Alignment = TabAlignment.Left,
                        DrawMode = TabDrawMode.OwnerDrawFixed
                    };

                    // 设置选项卡绘制事件，使其变为垂直显示文本
                    _tabControl.DrawItem += (sender, e) =>
                    {
                        Graphics g = e.Graphics;
                        TabPage tp = _tabControl.TabPages[e.Index];
                        Rectangle r = _tabControl.GetTabRect(e.Index);
                        StringFormat sf = new StringFormat();
                        sf.LineAlignment = StringAlignment.Center;
                        sf.Alignment = StringAlignment.Center;

                        // 设置选中与未选中的颜色
                        Brush brush = e.State == DrawItemState.Selected
                            ? new SolidBrush(Color.LightSkyBlue)
                            : new SolidBrush(Color.LightGray);
                        g.FillRectangle(brush, r);

                        // 旋转文本90度以垂直显示
                        g.TranslateTransform(r.Left + r.Width / 2, r.Top + r.Height / 2);
                        g.RotateTransform(90);
                        g.TranslateTransform(-(r.Left + r.Width / 2), -(r.Top + r.Height / 2));

                        // 绘制文本
                        g.DrawString(tp.Text, _tabControl.Font, Brushes.Black,
                            new PointF(r.Left + r.Width / 2, r.Top + r.Height / 2), sf);
                    };

                    _tabTreeViews = new List<CustomTreeView>();

                    // 初始化标签页
                    for (int i = 0; i < _tabNames.Count; i++)
                    {
                        string tabName = _tabNames[i];
                        TabPage tabPage = new TabPage(tabName);
                        tabPage.Padding = new Padding(3);

                        // 为每个选项卡创建一个自定义树控件
                        CustomTreeView tabTreeView = new CustomTreeView
                        {
                            Dock = DockStyle.Fill,
                            Font = new System.Drawing.Font("微软雅黑", 10),
                            BorderStyle = BorderStyle.FixedSingle,
                            Tag = i, // 保存选项卡索引，用于标识
                            Indent = 30, // 设置节点缩进值，使二级菜单明显缩进
                            ItemHeight = 24 // 增加节点高度，使界面更易读
                        };

                        // 使用共享的ImageList而不是为每个TreeView创建新的
                        tabTreeView.ImageList = _sharedImageList;

                        // 添加节点点击事件
                        tabTreeView.NodeMouseClick += (sender, e) =>
                        {
                            if (e.Node.Level == 0) // 一级菜单：切换展开/收缩
                            {
                                e.Node.Toggle();
                            }
                            else if (e.Node.Level == 1) // 二级菜单：执行命令
                            {
                                // 获取存储在Tag中的命令
                                string command = e.Node.Tag as string;
                                if (!string.IsNullOrEmpty(command))
                                {
                                    // 显示正在执行的命令
                                    Application.DocumentManager.MdiActiveDocument?.Editor
                                        .WriteMessage($"\n执行命令: {command}");

                                    // 执行AutoCAD命令
                                    UtilsCADActive.RunCommand(command);

                                    // 将焦点切换回AutoCAD主窗口
                                    Application.MainWindow.Focus();
                                }
                            }
                        };

                        // 将树控件加入到当前标签页
                        tabPage.Controls.Add(tabTreeView);
                        _tabTreeViews.Add(tabTreeView);

                        // 将标签页添加到选项卡控件
                        _tabControl.TabPages.Add(tabPage);
                    }

                    // 初始加载所有标签页的菜单
                    LoadAllTabMenus(config);

                    // 处理选项卡切换事件
                    _tabControl.SelectedIndexChanged += (sender, e) =>
                    {
                        // 选项卡切换时不再需要重新加载内容，因为每个标签页都有自己独立的菜单内容
                        int selectedIndex = _tabControl.SelectedIndex;
                        if (selectedIndex >= 0 && selectedIndex < _tabTreeViews.Count)
                        {
                            _treeView = _tabTreeViews[selectedIndex];
                        }
                    };

                    // 在标题标签下方添加搜索栏
                    Panel searchPanel = new Panel
                    {
                        Dock = DockStyle.Top,
                        Height = 30
                    };
                    
                    Label searchLabel = new Label
                    {
                        Text = "搜索命令：",
                        Dock = DockStyle.Left,
                        AutoSize = true,
                        TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                        Font = new System.Drawing.Font("微软雅黑", 10)
                    };
                    
                    TextBox searchBox = new TextBox
                    {
                        Dock = DockStyle.Fill,
                        Font = new System.Drawing.Font("微软雅黑", 10),
                        Text = "输入关键词搜索..."
                    };

                    // 搜索框焦点行为
                    searchBox.GotFocus += (sender, e) =>
                    {
                        if (searchBox.Text == "输入关键词搜索...")
                        {
                            searchBox.Text = "";
                        }
                    };

                    searchBox.LostFocus += (sender, e) =>
                    {
                        if (string.IsNullOrWhiteSpace(searchBox.Text))
                        {
                            searchBox.Text = "输入关键词搜索...";
                        }
                    };

                    // 添加搜索逻辑
                    searchBox.TextChanged += (sender, e) => 
                    {
                        if (searchBox.Text != "输入关键词搜索...")
                        {
                            FilterMenuItems(searchBox.Text);
                        }
                        else
                        {
                            // 当搜索框为默认文本时，恢复原始菜单
                            int selectedIndex = _tabControl.SelectedIndex;
                            if (selectedIndex >= 0 && selectedIndex < _tabTreeViews.Count)
                            {
                                _treeView = _tabTreeViews[selectedIndex];
                                if (config.Tabs != null && config.Tabs.Count > selectedIndex)
                                {
                                    _treeView.Nodes.Clear();
                                    AddMenuItemsFromConfig(_treeView, config.Tabs[selectedIndex].MenuGroups, false);
                                }
                                else if (selectedIndex == 0 && config.MenuGroups != null)
                                {
                                    _treeView.Nodes.Clear();
                                    AddMenuItemsFromConfig(_treeView, config.MenuGroups, false);
                                }
                            }
                        }
                    };

                    _menuPanel.Controls.Add(_tabControl);
                    _menuPanel.Controls.Add(searchPanel);
                    searchPanel.Controls.Add(searchBox);
                    searchPanel.Controls.Add(searchLabel);
                    // _menuPanel.Controls.Add(titleLabel);  // 删除顶部的大标题【数智设计-工艺】
                    // 将 Panel 添加到 PaletteSet
                    _paletteSet.Add("我的面板", _menuPanel);

                    // 停靠到 AutoCAD 左侧
                    _paletteSet.Dock = DockSides.Left;

                    _paletteSet.Visible = true;

                    _paletteSet.Dock = DockSides.Left;
                    _paletteSet.DockEnabled = DockSides.Left;

                    // 设置 PaletteSet 的位置与尺寸
                    var mainWindow = Application.MainWindow;
                    if (mainWindow != null)
                    {
                        int x = Convert.ToInt32(mainWindow.DeviceIndependentLocation.X);
                        int y = Convert.ToInt32(mainWindow.DeviceIndependentLocation.Y + mainWindow.DeviceIndependentSize.Height / 3);
                        int width = config.PaletteWidth;
                        int height = Convert.ToInt32(mainWindow.DeviceIndependentSize.Height * 0.75);
                        _paletteSet.SetLocation(new System.Drawing.Point(x, y));
                        _paletteSet.Size = new System.Drawing.Size(width, height);
                    }
                }
                else
                {
                    // 如果面板已存在，则刷新所有选项卡的菜单
                    LoadAllTabMenus(config);

                    // 更新面板标题和尺寸
                    _paletteSet.Text = config.PaletteTitle;
                    _paletteSet.Size = new System.Drawing.Size(config.PaletteWidth,
                        _paletteSet.Size.Height); // 只更新宽度，保持高度不变
                }

                _paletteSet.Visible = true;
                return _paletteSet;
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor
                    .WriteMessage($"\n显示自定义菜单时出错: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 加载所有标签页的菜单
        /// </summary>
        /// <param name="config">菜单配置</param>
        private static void LoadAllTabMenus(MenuConfig config)
        {
            // 确保两个集合数量相等
            if (config.Tabs == null || config.Tabs.Count == 0)
            {
                // 如果没有标签页配置，则使用MenuGroups为第一个标签页加载菜单
                if (_tabTreeViews != null && _tabTreeViews.Count > 0 && config.MenuGroups != null)
                {
                    var treeView = _tabTreeViews[0];
                    treeView.Nodes.Clear();
                    AddMenuItemsFromConfig(treeView, config.MenuGroups, true); // 只对第一个标签页启用调试输出
                }
                return;
            }

            // 加载每个标签页对应的菜单
            for (int i = 0; i < _tabTreeViews.Count && i < config.Tabs.Count; i++)
            {
                var treeView = _tabTreeViews[i];
                var tabConfig = config.Tabs[i];
                
                treeView.Nodes.Clear();
                AddMenuItemsFromConfig(treeView, tabConfig.MenuGroups, i == 0); // 只对第一个标签页启用调试输出
            }
        }

        /// <summary>
        /// 从配置加载菜单项
        /// </summary>
        /// <param name="treeView">树视图控件</param>
        /// <param name="menuGroups">菜单组列表</param>
        /// <param name="showDebugInfo">是否显示调试信息</param>
        private static void AddMenuItemsFromConfig(CustomTreeView treeView, List<MenuGroup> menuGroups, bool showDebugInfo = false)
        {
            treeView.Nodes.Clear();

            if (menuGroups == null)
                return;

            // 记录读取到的不同图标键，用于调试
            HashSet<string> allIconKeys = new HashSet<string>();
            
            foreach (var group in menuGroups)
            {
                // 创建一级菜单
                TreeNode groupNode = new TreeNode(group.Title);
                
                // 设置一级菜单图标
                string groupIconKey = group.IconKey;
                allIconKeys.Add(groupIconKey);
                
                // 确保ImageList中有这个图标
                if (treeView.ImageList != null && treeView.ImageList.Images.ContainsKey(groupIconKey))
                {
                    groupNode.ImageKey = groupIconKey;
                    groupNode.SelectedImageKey = groupIconKey;
                }
                else
                {
                    // 图标不存在时不设置图标
                    // Application.DocumentManager.MdiActiveDocument?.Editor
                    //     .WriteMessage($"\n一级菜单图标未找到: {groupIconKey}，不加载图标");
                }

                // 添加二级菜单
                foreach (var item in group.Items)
                {
                    TreeNode itemNode = new TreeNode(item.Title);
                    
                    // 设置二级菜单图标
                    string itemIconKey = item.IconKey;
                    allIconKeys.Add(itemIconKey);
                    
                    // 确保ImageList中有这个图标
                    if (treeView.ImageList != null && treeView.ImageList.Images.ContainsKey(itemIconKey))
                    {
                        itemNode.ImageKey = itemIconKey;
                        itemNode.SelectedImageKey = itemIconKey;
                    }
                    else
                    {
                        // 尝试不带扩展名的图标键
                        string itemIconKeyWithoutExt = Path.GetFileNameWithoutExtension(itemIconKey);
                        if (treeView.ImageList != null && treeView.ImageList.Images.ContainsKey(itemIconKeyWithoutExt))
                        {
                            itemNode.ImageKey = itemIconKeyWithoutExt;
                            itemNode.SelectedImageKey = itemIconKeyWithoutExt;
                        }
                        else
                        {
                            // // 图标不存在时不设置图标
                            // Application.DocumentManager.MdiActiveDocument?.Editor
                            //     .WriteMessage($"\n二级菜单图标未找到: {itemIconKey}，不加载图标");
                        }
                    }
                    
                    // 在Tag中存储命令，以便点击时执行
                    itemNode.Tag = item.Command;

                    groupNode.Nodes.Add(itemNode);
                }

                treeView.Nodes.Add(groupNode);
            }
            
            // 调试信息：只在指定时显示图标键信息
            if (showDebugInfo && treeView.ImageList != null)
            {
                // Application.DocumentManager.MdiActiveDocument?.Editor
                //     .WriteMessage($"\n菜单中使用的图标键: {string.Join(", ", allIconKeys)}");
                
                List<string> availableKeys = new List<string>();
                foreach (string key in treeView.ImageList.Images.Keys)
                {
                    availableKeys.Add(key);
                }
                
                // Application.DocumentManager.MdiActiveDocument?.Editor
                //     .WriteMessage($"\n可用的图标键: {string.Join(", ", availableKeys)}");
            }
        }

        /// <summary>
        /// 加载AutoCAD图标到图像列表
        /// </summary>
        /// <param name="imgList">图像列表</param>
        public static void LoadAutoCADIcons(ImageList imgList)
        {
            // 不需要再设置ImageSize和ColorDepth，已在创建时设置
            
            // 获取图标目录路径
            string iconsDir = GetIconsDirectory();
            if (string.IsNullOrEmpty(iconsDir))
            {
                Application.DocumentManager.MdiActiveDocument?.Editor
                    .WriteMessage($"\n未找到有效的图标目录");
                return;
            }
            
            // 记录找到的图标文件数量
            List<string> fileNames = Directory.EnumerateFiles(iconsDir).ToList();
            Application.DocumentManager.MdiActiveDocument?.Editor
                .WriteMessage($"\n正在从 {iconsDir} 加载图标，找到 {fileNames.Count} 个图标文件");
            
            // 添加默认文件夹图标
            try
            {
                Icon folderIcon = SystemIcons.Information;
                imgList.Images.Add("folder", folderIcon);
            }
            catch
            {
                // 忽略无法加载的图标
            }
            
            foreach (var file in fileNames)
            {
                string filename = Path.GetFileName(file);
                string extension = Path.GetExtension(file);
                string iconKeyWithoutExt = filename.Replace(extension, "");
                string iconKeyWithExt = filename; // 保留扩展名的键名
                
                try
                {
                    using (Bitmap originalImage = new Bitmap(file))
                    {
                        // 添加两个版本的图标键，一个带扩展名，一个不带扩展名
                        // 这样无论配置中是哪种格式都能找到图标
                        if (!imgList.Images.ContainsKey(iconKeyWithoutExt))
                        {
                            imgList.Images.Add(iconKeyWithoutExt, new Bitmap(originalImage));
                        }
                        
                        if (!imgList.Images.ContainsKey(iconKeyWithExt))
                        {
                            imgList.Images.Add(iconKeyWithExt, new Bitmap(originalImage));
                        }
                        
                        // Application.DocumentManager.MdiActiveDocument?.Editor
                        //     .WriteMessage($"\n成功加载图标: {filename}");
                    }
                }
                catch (Exception ex)
                {
                    Application.DocumentManager.MdiActiveDocument?.Editor
                        .WriteMessage($"\n加载图标 {filename} 失败: {ex.Message}");
                }
            }
            
            Application.DocumentManager.MdiActiveDocument?.Editor
                .WriteMessage($"\n图标加载完成，共加载 {imgList.Images.Count} 个图标");
        }

        /// <summary>
        /// 获取图标目录路径
        /// </summary>
        /// <returns>图标目录的完整路径，如果找不到则返回null</returns>
        private static string GetIconsDirectory()
        {
            try
            {
                // 首先尝试获取程序集所在目录
                string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string assemblyDir = Path.GetDirectoryName(assemblyLocation);
                
                // 检查当前程序集目录下是否有icons文件夹
                string iconsDir1 = Path.Combine(assemblyDir, "icons");
                if (Directory.Exists(iconsDir1))
                {
                    Application.DocumentManager.MdiActiveDocument?.Editor
                        .WriteMessage($"\n找到程序集目录下的图标目录: {iconsDir1}");
                    return iconsDir1;
                }
                
                // 回退到上一级目录查找icons目录
                string parentDir = Directory.GetParent(assemblyDir)?.FullName;
                if (parentDir != null)
                {
                    string iconsDir2 = Path.Combine(parentDir, "icons");
                    if (Directory.Exists(iconsDir2))
                    {
                        Application.DocumentManager.MdiActiveDocument?.Editor
                            .WriteMessage($"\n找到上级目录下的图标目录: {iconsDir2}");
                        return iconsDir2;
                    }
                }
                
                // 如果仍然找不到，尝试App域的基目录
                string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icons");
                if (Directory.Exists(baseDir))
                {
                    Application.DocumentManager.MdiActiveDocument?.Editor
                        .WriteMessage($"\n找到应用程序基目录下的图标目录: {baseDir}");
                    return baseDir;
                }
                
                // 如果以上方法都找不到，尝试检查固定路径作为最后的备选
                string legacyDir = @"D:\dataflowcad\dataflowcad\dataflowNet\DLNet\icons\";
                if (Directory.Exists(legacyDir))
                {
                    Application.DocumentManager.MdiActiveDocument?.Editor
                        .WriteMessage($"\n找到传统路径下的图标目录: {legacyDir}");
                    return legacyDir;
                }
                
                // 如果所有路径都找不到，返回null
                Application.DocumentManager.MdiActiveDocument?.Editor
                    .WriteMessage($"\n在所有可能的路径中均未找到icons目录");
                return null;
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor
                    .WriteMessage($"\n获取图标目录时出错: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 根据关键词过滤菜单项
        /// </summary>
        /// <param name="searchText">搜索关键词</param>
        public static void FilterMenuItems(string searchText)
        {
            if (_treeView == null) return;

            // 获取当前选中的标签页索引
            int selectedIndex = _tabControl.SelectedIndex;
            if (selectedIndex < 0) return;

            MenuConfig config = GsMenuConfigService.LoadMenuConfig();
            List<MenuGroup> menuGroups = null;

            // 从配置获取当前标签页的菜单组
            if (config.Tabs != null && config.Tabs.Count > selectedIndex)
            {
                menuGroups = config.Tabs[selectedIndex].MenuGroups;
            }
            else if (selectedIndex == 0)
            {
                menuGroups = config.MenuGroups;
            }

            if (menuGroups == null) return;

            _treeView.BeginUpdate();
            _treeView.Nodes.Clear();

            if (string.IsNullOrWhiteSpace(searchText) || searchText == "输入关键词搜索...")
            {
                // 重新加载原始菜单
                AddMenuItemsFromConfig(_treeView, menuGroups, false);
            }
            else
            {
                // 过滤菜单并显示匹配项
                foreach (var group in menuGroups)
                {
                    TreeNode groupNode = new TreeNode(group.Title);
                    
                    // 设置一级菜单图标
                    if (_treeView.ImageList != null && _treeView.ImageList.Images.ContainsKey(group.IconKey))
                    {
                        groupNode.ImageKey = group.IconKey;
                        groupNode.SelectedImageKey = group.IconKey;
                    }

                    bool groupHasMatch = false;

                    foreach (var item in group.Items)
                    {
                        if (item.Title.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            TreeNode itemNode = new TreeNode(item.Title);
                            
                            // 设置二级菜单图标
                            if (_treeView.ImageList != null && _treeView.ImageList.Images.ContainsKey(item.IconKey))
                            {
                                itemNode.ImageKey = item.IconKey;
                                itemNode.SelectedImageKey = item.IconKey;
                            }
                            else
                            {
                                // 尝试不带扩展名的图标键
                                string itemIconKeyWithoutExt = Path.GetFileNameWithoutExtension(item.IconKey);
                                if (_treeView.ImageList != null && _treeView.ImageList.Images.ContainsKey(itemIconKeyWithoutExt))
                                {
                                    itemNode.ImageKey = itemIconKeyWithoutExt;
                                    itemNode.SelectedImageKey = itemIconKeyWithoutExt;
                                }
                            }
                            
                            // 在Tag中存储命令，以便点击时执行
                            itemNode.Tag = item.Command;

                            groupNode.Nodes.Add(itemNode);
                            groupHasMatch = true;
                        }
                    }

                    // 只有当该组中存在匹配项时，才显示该组
                    if (groupHasMatch)
                    {
                        _treeView.Nodes.Add(groupNode);
                        groupNode.Expand(); // 自动展开包含匹配项的组
                    }
                }
            }

            _treeView.EndUpdate();
        }
    }
} 