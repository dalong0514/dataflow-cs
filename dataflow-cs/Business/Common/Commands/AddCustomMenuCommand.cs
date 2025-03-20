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
using dataflow_cs.Business.Common.Models;
using dataflow_cs.Business.Common.Services;
using dataflow_cs.Business.Common.Helpers;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Panel = System.Windows.Forms.Panel;

namespace dataflow_cs.Business.Common.Commands
{
    /// <summary>
    /// 添加自定义菜单命令
    /// </summary>
    public class DLAddCustomMenuCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLAddCustomMenu";

        private static PaletteSet _paletteSet;
        // 保存面板引用以便刷新
        private static Panel _menuPanel;
        private static CustomTreeView _treeView;

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
                MenuConfig config = MenuConfigService.LoadMenuConfig();
                
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
                        Text = "固定面板",
                        Dock = DockStyle.Top,
                        Height = 30,
                        TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                        BackColor = System.Drawing.Color.Gray,
                        ForeColor = System.Drawing.Color.White
                    };

                    // 创建自定义树控件，用于实现多级菜单及交互效果
                    _treeView = new CustomTreeView
                    {
                        Dock = DockStyle.Fill,
                        Font = new System.Drawing.Font("微软雅黑", 10),
                        BorderStyle = BorderStyle.FixedSingle
                    };

                    // 设置 ImageList（示例，实际应加载真实图标）
                    ImageList imgList = new ImageList();
                    LoadAutoCADIcons(imgList);
                    _treeView.ImageList = imgList;

                    // 从配置添加菜单
                    AddMenuItemsFromConfig(_treeView, config);

                    // 一级菜单点击时展开/收缩，二级菜单点击时触发命令
                    _treeView.NodeMouseClick += (sender, e) =>
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

                    _menuPanel.Controls.Add(_treeView);
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
                    // 如果面板已存在，则刷新菜单
                    if (_treeView != null)
                    {
                        _treeView.Nodes.Clear();
                        AddMenuItemsFromConfig(_treeView, config);
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
}
