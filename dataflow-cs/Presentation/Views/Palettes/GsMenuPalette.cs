using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Windows;
using dataflow_cs.Domain.ValueObjects;
using dataflow_cs.Infrastructure.AutoCAD.Services;
using dataflow_cs.Business.Services;
using dataflow_cs.Presentation.Views.Controls;
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
        private static string[] _tabNames = new string[] { "工艺流程", "设备布置", "二维配管" };

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
                                    AutoCADService.RunCommand(command);
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

        /// <summary>
        /// 加载AutoCAD图标
        /// </summary>
        /// <param name="imgList">图标列表</param>
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
} 