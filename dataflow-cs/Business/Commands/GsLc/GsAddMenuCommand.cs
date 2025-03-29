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
using dataflow_cs.Business.Common.Helpers;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Panel = System.Windows.Forms.Panel;
using Newtonsoft.Json;

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

        private static PaletteSet _paletteSet;
        // 保存面板引用以便刷新
        private static Panel _menuPanel;
        private static CustomTreeView _treeView;
        // 选项卡容器和选项卡面板
        private static TabControl _tabControl;
        private static List<CustomTreeView> _tabTreeViews;
        private static string[] _tabNames = new string[] { "工艺流程", "设备布置", "二维配管" };

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
                ShowCustomMenu();

                editor.WriteMessage("\n自定义菜单已成功添加！");
                return true;
            }
            catch (Exception ex)
            {
                editor.WriteMessage($"\n执行添加自定义菜单命令时发生错误: {ex.Message}");
                return false;
            }
        }

        public static PaletteSet ShowCustomMenu()
        {
            try
            {
                // 每次都重新加载菜单配置，确保能获取最新修改
                MenuConfig config = GsMenuConfigService.LoadMenuConfig();

                if (_paletteSet == null)
                {
                    // 创建 PaletteSet 作为 AutoCAD 固定面板
                    _paletteSet = new PaletteSet(config.PaletteTitle)
                    {
                        Size = new System.Drawing.Size(config.PaletteWidth, config.PaletteHeight),
                        Style = PaletteSetStyles.ShowPropertiesMenu |
                                PaletteSetStyles.ShowCloseButton |
                                PaletteSetStyles.ShowAutoHideButton |
                                PaletteSetStyles.SingleColDock,
                        KeepFocus = true,
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
                    Label titleLabel = new Label
                    {
                        Text = "数智设计-工艺",
                        Dock = DockStyle.Top,
                        Height = 30,
                        TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                        BackColor = System.Drawing.Color.Gray,
                        ForeColor = System.Drawing.Color.White
                    };

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

                    // 初始化三个选项卡页面
                    foreach (string tabName in _tabNames)
                    {
                        TabPage tabPage = new TabPage(tabName);
                        tabPage.Padding = new Padding(3);

                        // 为每个选项卡创建一个自定义树控件
                        CustomTreeView tabTreeView = new CustomTreeView
                        {
                            Dock = DockStyle.Fill,
                            Font = new System.Drawing.Font("微软雅黑", 10),
                            BorderStyle = BorderStyle.FixedSingle
                        };

                        // 设置 ImageList（所有选项卡共用同一个图像列表）
                        ImageList imgList = new ImageList();
                        LoadAutoCADIcons(imgList);
                        tabTreeView.ImageList = imgList;

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
                                    AutoCADCommandHelper.RunCommand(command);
                                }
                            }
                        };

                        // 将树控件加入到当前标签页
                        tabPage.Controls.Add(tabTreeView);
                        _tabTreeViews.Add(tabTreeView);

                        // 将标签页添加到选项卡控件
                        _tabControl.TabPages.Add(tabPage);
                    }

                    // 初始加载第一个选项卡的菜单
                    if (_tabTreeViews.Count > 0)
                    {
                        _treeView = _tabTreeViews[0]; // 设置第一个树为主树视图
                        AddMenuItemsFromConfig(_treeView, config);
                    }

                    // 处理选项卡切换事件
                    _tabControl.SelectedIndexChanged += (sender, e) =>
                    {
                        // 选项卡切换时可以加载对应的内容
                        int selectedIndex = _tabControl.SelectedIndex;
                        if (selectedIndex >= 0 && selectedIndex < _tabTreeViews.Count)
                        {
                            _treeView = _tabTreeViews[selectedIndex];
                            // 如果该选项卡没有内容，可以加载对应的菜单
                            if (_treeView.Nodes.Count == 0)
                            {
                                AddMenuItemsFromConfig(_treeView, config);
                            }
                        }
                    };

                    _menuPanel.Controls.Add(_tabControl);
                    _menuPanel.Controls.Add(titleLabel);
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
                    if (_tabTreeViews != null && _tabTreeViews.Count > 0)
                    {
                        foreach (var treeView in _tabTreeViews)
                        {
                            treeView.Nodes.Clear();
                            AddMenuItemsFromConfig(treeView, config);
                        }
                    }

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
        /// 从配置加载菜单项
        /// </summary>
        /// <param name="treeView">树视图控件</param>
        /// <param name="config">菜单配置</param>
        private static void AddMenuItemsFromConfig(CustomTreeView treeView, MenuConfig config)
        {
            treeView.Nodes.Clear();

            foreach (var group in config.MenuGroups)
            {
                // 创建一级菜单
                TreeNode groupNode = new TreeNode(group.Title);
                groupNode.ImageKey = group.IconKey;
                groupNode.SelectedImageKey = group.IconKey;

                // 添加二级菜单
                foreach (var item in group.Items)
                {
                    TreeNode itemNode = new TreeNode(item.Title);
                    itemNode.ImageKey = item.IconKey;
                    itemNode.SelectedImageKey = item.IconKey;
                    // 在Tag中存储命令，以便点击时执行
                    itemNode.Tag = item.Command;

                    groupNode.Nodes.Add(itemNode);
                }

                treeView.Nodes.Add(groupNode);
            }
        }

        public static void LoadAutoCADIcons(ImageList imgList)
        {
            imgList.ImageSize = new System.Drawing.Size(16, 16);
            string baseDir = @"C:\Users\chen-jun\Desktop\ico\";
            if (!Directory.Exists(baseDir))
                return;
            List<string> fileNames = Directory.EnumerateFiles(baseDir).ToList();
            foreach (var file in fileNames)
            {
                string filename = Path.GetFileName(file);
                string extension = Path.GetExtension(file);
                try
                {
                    System.Drawing.Image image = System.Drawing.Image.FromFile(file);
                    imgList.Images.Add(filename.Replace(extension, ""), image);
                }
                catch
                {
                    // 忽略无法加载的图标
                }
            }
        }
    }

    // 自定义树控件，支持自定义绘制和鼠标交互效果
    public class CustomTreeView : TreeView
    {
        private TreeNode lastHoveredNode = null;

        public CustomTreeView()
        {
            this.DrawMode = TreeViewDrawMode.OwnerDrawAll;
            // 启用热跟踪
            this.HotTracking = true;
            this.ShowLines = false;
            this.ShowPlusMinus = false;
            this.FullRowSelect = true;
            this.HideSelection = false;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            this.DrawNode += CustomTreeView_DrawNode;
            this.MouseMove += CustomTreeView_MouseMove;
            this.MouseLeave += CustomTreeView_MouseLeave;
        }

        private void CustomTreeView_MouseLeave(object sender, EventArgs e)
        {
            this.SelectedNode = null;
        }

        private void CustomTreeView_MouseMove(object sender, MouseEventArgs e)
        {
            // 鼠标悬停时，获取节点并更新 Hot 状态（仅二级节点响应悬停高亮）
            TreeNode node = this.GetNodeAt(e.Location);
            if (node != lastHoveredNode)
            {
                lastHoveredNode = node;
                this.SelectedNode = node;
                // 如果希望只重绘当前节点区域，可以调用 Invalidate(node.Bounds);
            }
        }

        // 重写 OnNodeMouseHover 事件
        protected override void OnNodeMouseHover(TreeNodeMouseHoverEventArgs e)
        {
            base.OnNodeMouseHover(e);
            // 设置当前悬停节点为选中状态
            this.SelectedNode = e.Node;
        }

        private void CustomTreeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            // 判断节点状态并设置背景颜色
            System.Drawing.Color bgColor = System.Drawing.Color.Transparent;
            System.Drawing.Color textColor = this.ForeColor;
            if (e.Node.IsSelected)
            {
                bgColor = System.Drawing.Color.LightSkyBlue;
                textColor = System.Drawing.Color.WhiteSmoke;
            }
            else if (e.State.HasFlag(TreeNodeStates.Hot))
            {
                //bgColor = System.Drawing.Color.WhiteSmoke;
                //textColor = System.Drawing.Color.WhiteSmoke;
            }

            using (System.Drawing.SolidBrush bgBrush = new System.Drawing.SolidBrush(bgColor))
            {
                e.Graphics.FillRectangle(bgBrush, e.Bounds);
            }

            int iconOffset = 0;
            // 绘制图标（如果存在）
            if (this.ImageList != null && !string.IsNullOrEmpty(e.Node.ImageKey))
            {
                System.Drawing.Image nodeImage = this.ImageList.Images[e.Node.ImageKey];
                if (nodeImage != null)
                {
                    // 根据节点层级计算图标的X坐标，适当留出边距
                    int iconX = e.Bounds.X + 5 /*+ (e.Node.Level * 20) + 5*/;
                    int iconY = e.Bounds.Y + (e.Bounds.Height - this.ImageList.ImageSize.Height) / 2;
                    e.Graphics.DrawImage(nodeImage, iconX, iconY, this.ImageList.ImageSize.Width, this.ImageList.ImageSize.Height);
                    iconOffset = this.ImageList.ImageSize.Width + 2; // 图标宽度加上间距
                }
            }

            // 如果节点有子节点，绘制自定义展开/收缩按钮
            if (e.Node.Nodes.Count > 0)
            {
                int btnSize = 12;
                int offsetX = e.Node.Level * 20;
                int btnX = e.Bounds.X + 5 + offsetX;
                int btnY = e.Bounds.Y + (e.Bounds.Height - btnSize) / 2;
                System.Drawing.Rectangle btnRect = new System.Drawing.Rectangle(btnX, btnY, btnSize, btnSize);
                DrawTriangle(e.Graphics, btnRect, e.Node.IsExpanded);
            }

            // 绘制节点文本，左侧留出按钮空间
            int textOffset = 5 + (e.Node.Nodes.Count > 0 ? 20 : 0) + (e.Node.Level * 20);
            System.Drawing.Rectangle textRect = new System.Drawing.Rectangle(e.Bounds.X + textOffset, e.Bounds.Y, e.Bounds.Width - textOffset, e.Bounds.Height);
            System.Windows.Forms.TextRenderer.DrawText(e.Graphics, e.Node.Text, this.Font, textRect, textColor, System.Windows.Forms.TextFormatFlags.VerticalCenter);
        }

        private void DrawTriangle(System.Drawing.Graphics g, System.Drawing.Rectangle rect, bool expanded)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            System.Drawing.Point[] trianglePoints;
            if (expanded)
            {
                // ▼ 下三角形
                trianglePoints = new System.Drawing.Point[]
                {
                    new System.Drawing.Point(rect.Left, rect.Top),
                    new System.Drawing.Point(rect.Right, rect.Top),
                    new System.Drawing.Point(rect.Left + rect.Width / 2, rect.Bottom)
                };
            }
            else
            {
                // ► 右三角形
                trianglePoints = new System.Drawing.Point[]
                {
                    new System.Drawing.Point(rect.Left, rect.Top),
                    new System.Drawing.Point(rect.Right, rect.Top + rect.Height / 2),
                    new System.Drawing.Point(rect.Left, rect.Bottom)
                };
            }
            using (System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(System.Drawing.Color.DarkGray))
            {
                g.FillPolygon(brush, trianglePoints);
            }
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
        }
    }

    /// <summary>
    /// 工艺专业菜单配置服务
    /// </summary>
    public class GsMenuConfigService
    {
        private static readonly string ConfigFileName = "GsMenuConfig.json";
        private static string ConfigFilePath => Path.Combine(GetConfigDirectory(), ConfigFileName);

        /// <summary>
        /// 获取配置文件目录路径
        /// </summary>
        /// <returns>配置文件目录路径</returns>
        private static string GetConfigDirectory()
        {
            try
            {
                // 首先尝试获取程序集所在目录
                string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string assemblyDir = Path.GetDirectoryName(assemblyLocation);
                
                // 检查当前程序集目录下是否有config文件夹
                string configDir1 = Path.Combine(assemblyDir, "config");
                if (Directory.Exists(configDir1) && File.Exists(Path.Combine(configDir1, ConfigFileName)))
                {
                    return configDir1;
                }
                
                // 回退到上一级目录查找config目录
                string parentDir = Directory.GetParent(assemblyDir)?.FullName;
                if (parentDir != null)
                {
                    string configDir2 = Path.Combine(parentDir, "config");
                    if (Directory.Exists(configDir2) && File.Exists(Path.Combine(configDir2, ConfigFileName)))
                    {
                        return configDir2;
                    }
                }
                
                // 默认在应用程序目录下创建config文件夹
                string appDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string configDir = Path.Combine(appDir, "config");
                
                // 确保目录存在
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }
                
                return configDir;
            }
            catch
            {
                // 出错时返回默认应用程序目录下的config
                string appDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                return Path.Combine(appDir, "config");
            }
        }

        /// <summary>
        /// 读取菜单配置
        /// </summary>
        /// <returns>菜单配置对象</returns>
        public static MenuConfig LoadMenuConfig()
        {
            try
            {
                // 检查配置文件是否存在
                if (!File.Exists(ConfigFilePath))
                {
                    // 如果不存在，创建一个默认配置并使用公共服务保存
                    var defaultConfig = CreateDefaultConfig();
                    SaveMenuConfig(defaultConfig);
                    return defaultConfig;
                }

                // 从文件读取JSON
                string json = File.ReadAllText(ConfigFilePath);
                try
                {
                    var config = JsonConvert.DeserializeObject<MenuConfig>(json);
                    Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n成功从 {ConfigFilePath} 加载菜单配置");
                    return config;
                }
                catch (Exception jsonEx)
                {
                    Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\nJSON解析错误: {jsonEx.Message}");
                    return CreateDefaultConfig();
                }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n读取工艺专业菜单配置时出错: {ex.Message}");
                return CreateDefaultConfig();
            }
        }

        /// <summary>
        /// 保存菜单配置
        /// </summary>
        /// <param name="config">菜单配置对象</param>
        public static void SaveMenuConfig(MenuConfig config)
        {
            try
            {
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(ConfigFilePath, json);
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n成功保存工艺专业菜单配置到 {ConfigFilePath}");
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n保存工艺专业菜单配置时出错: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 创建默认配置
        /// </summary>
        /// <returns>默认菜单配置</returns>
        private static MenuConfig CreateDefaultConfig()
        {
            return new MenuConfig
            {
                PaletteTitle = "数智设计-工艺",
                PaletteWidth = 250,
                PaletteHeight = 400,
                MenuGroups = new List<MenuGroup>
                {
                    new MenuGroup
                    {
                        Title = "工艺一级菜单1",
                        IconKey = "folder",
                        Items = new List<dataflow_cs.Domain.ValueObjects.MenuItem>
                        {
                            new dataflow_cs.Domain.ValueObjects.MenuItem { Title = "工艺二级菜单1-1", IconKey = "本地生活", Command = "LINE" },
                            new dataflow_cs.Domain.ValueObjects.MenuItem { Title = "工艺二级菜单1-2", IconKey = "本地生活", Command = "CIRCLE" }
                        }
                    },
                    new MenuGroup
                    {
                        Title = "工艺一级菜单2",
                        IconKey = "folder",
                        Items = new List<dataflow_cs.Domain.ValueObjects.MenuItem>
                        {
                            new dataflow_cs.Domain.ValueObjects.MenuItem { Title = "工艺二级菜单2-1", IconKey = "编辑", Command = "RECTANGLE" },
                            new dataflow_cs.Domain.ValueObjects.MenuItem { Title = "工艺二级菜单2-2", IconKey = "编辑", Command = "ARC" }
                        }
                    }
                }
            };
        }
    }
}
