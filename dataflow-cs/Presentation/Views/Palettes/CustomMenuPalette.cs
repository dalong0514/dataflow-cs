using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Windows;
using dataflow_cs.Infrastructure.AutoCAD.Services;
using dataflow_cs.Domain.ValueObjects;
using dataflow_cs.Presentation.Views.Controls;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace dataflow_cs.Presentation.Views.Palettes
{
    /// <summary>
    /// 自定义菜单面板
    /// </summary>
    public class CustomMenuPalette
    {
        private static PaletteSet _paletteSet;
        private static Panel _menuPanel;
        private static CustomTreeView _treeView;
        
        /// <summary>
        /// 显示自定义菜单面板
        /// </summary>
        /// <param name="config">菜单配置</param>
        /// <returns>面板对象</returns>
        public static PaletteSet Show(MenuConfig config)
        {
            try
            {
                // 记录调试信息
                var editor = Application.DocumentManager.MdiActiveDocument?.Editor;
                editor?.WriteMessage("\n开始显示自定义菜单面板...");
                editor?.WriteMessage($"\n配置标题: {config.PaletteTitle}");
                editor?.WriteMessage($"\n配置菜单组数量: {config.MenuGroups?.Count ?? 0}");
                
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
                        Text = "数智设计",
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

                    // 先将TreeView添加到面板，再设置ImageList和加载菜单项
                    _menuPanel.Controls.Add(_treeView);
                    _menuPanel.Controls.Add(titleLabel);

                    // 设置 ImageList（示例，实际应加载真实图标）
                    ImageList imgList = new ImageList();
                    imgList.ColorDepth = ColorDepth.Depth32Bit; // 使用32位颜色深度，提高图标质量
                    LoadMenuIcons(imgList);
                    
                    // 先将TreeView添加到控件集合后再设置ImageList，避免界面刷新问题
                    _treeView.ImageList = imgList;

                    // 添加从配置加载菜单项的调试信息
                    editor?.WriteMessage("\n准备加载菜单项...");
                    AddMenuItemsFromConfig(_treeView, config);
                    editor?.WriteMessage($"\n菜单项加载完成，共加载 {_treeView.Nodes.Count} 个一级菜单");

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
                                AutoCADService.RunCommand(command);
                            }
                        }
                    };

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
                        editor?.WriteMessage("\n刷新现有菜单...");
                        _treeView.Nodes.Clear();
                        AddMenuItemsFromConfig(_treeView, config);
                    }
                    
                    // 更新面板标题和尺寸
                    _paletteSet.Text = config.PaletteTitle;
                    _paletteSet.Size = new System.Drawing.Size(config.PaletteWidth, 
                        _paletteSet.Size.Height); // 只更新宽度，保持高度不变
                }

                _paletteSet.Visible = true;
                editor?.WriteMessage("\n自定义菜单面板显示完成");
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
            var editor = Application.DocumentManager.MdiActiveDocument?.Editor;
            editor?.WriteMessage($"\n开始加载菜单项，菜单组数量: {config.MenuGroups?.Count ?? 0}");
            
            treeView.Nodes.Clear();

            if (config.MenuGroups == null || config.MenuGroups.Count == 0)
            {
                editor?.WriteMessage("\n警告：配置中没有菜单组数据！");
                return;
            }

            foreach (var group in config.MenuGroups)
            {
                editor?.WriteMessage($"\n添加菜单组: {group.Title}, 子项数量: {group.Items?.Count ?? 0}");
                
                // 创建一级菜单
                TreeNode groupNode = new TreeNode(group.Title);
                groupNode.ImageKey = group.IconKey;
                groupNode.SelectedImageKey = group.IconKey;

                // 添加二级菜单
                if (group.Items != null)
                {
                    foreach (var item in group.Items)
                    {
                        editor?.WriteMessage($"\n  - 添加菜单项: {item.Title}, 命令: {item.Command}");
                        
                        TreeNode itemNode = new TreeNode(item.Title);
                        itemNode.ImageKey = item.IconKey;
                        itemNode.SelectedImageKey = item.IconKey;
                        // 在Tag中存储命令，以便点击时执行
                        itemNode.Tag = item.Command;
                        
                        groupNode.Nodes.Add(itemNode);
                    }
                }

                treeView.Nodes.Add(groupNode);
            }
            
            editor?.WriteMessage($"\n菜单项加载完成，共加载 {treeView.Nodes.Count} 个一级菜单");
        }

        /// <summary>
        /// 加载菜单图标
        /// </summary>
        /// <param name="imgList">图标列表</param>
        private static void LoadMenuIcons(ImageList imgList)
        {
            try
            {
                imgList.ImageSize = new System.Drawing.Size(16, 16);
                
                // 使用系统图标作为默认图标，不使用using块避免图标被释放
                try
                {
                    Icon folderIcon = SystemIcons.Information;
                    imgList.Images.Add("folder", (Icon)folderIcon.Clone());
                }
                catch (Exception ex)
                {
                    AutoCADService.WriteMessage($"\n添加folder图标时出错: {ex.Message}");
                }

                try
                {
                    Icon appIcon = SystemIcons.Application;
                    imgList.Images.Add("command", (Icon)appIcon.Clone());
                }
                catch (Exception ex)
                {
                    AutoCADService.WriteMessage($"\n添加command图标时出错: {ex.Message}");
                }
                
                // 尝试从应用目录下的icons文件夹加载图标
                string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icons");
                if (Directory.Exists(baseDir))
                {
                    List<string> fileNames = Directory.EnumerateFiles(baseDir).ToList();
                    AutoCADService.WriteMessage($"\n正在从 {baseDir} 加载图标，找到 {fileNames.Count} 个图标文件");
                    
                    foreach (var file in fileNames)
                    {
                        string filename = Path.GetFileName(file);
                        string extension = Path.GetExtension(file);
                        try
                        {
                            // 使用new Bitmap创建图像的副本，避免图像被释放的问题
                            using (Bitmap originalImage = new Bitmap(file))
                            {
                                string iconKey = filename.Replace(extension, "");
                                imgList.Images.Add(iconKey, new Bitmap(originalImage));
                                AutoCADService.WriteMessage($"\n成功加载图标: {iconKey}");
                            }
                        }
                        catch (Exception ex)
                        {
                            AutoCADService.WriteMessage($"\n加载图标 {filename} 时出错: {ex.Message}");
                            // 继续处理下一个图标
                        }
                    }
                }
                else
                {
                    AutoCADService.WriteMessage($"\n未找到图标目录: {baseDir}");
                    
                    // 尝试使用固定路径作为备用选项
                    string backupDir = @"C:\Users\chen-jun\Desktop\ico\";
                    if (Directory.Exists(backupDir))
                    {
                        AutoCADService.WriteMessage($"\n尝试从备用路径加载图标: {backupDir}");
                        List<string> fileNames = Directory.EnumerateFiles(backupDir).ToList();
                        foreach (var file in fileNames)
                        {
                            string filename = Path.GetFileName(file);
                            string extension = Path.GetExtension(file);
                            try
                            {
                                // 使用new Bitmap创建图像的副本，避免图像被释放的问题
                                using (Bitmap originalImage = new Bitmap(file))
                                {
                                    string iconKey = filename.Replace(extension, "");
                                    imgList.Images.Add(iconKey, new Bitmap(originalImage));
                                }
                            }
                            catch (Exception ex)
                            {
                                AutoCADService.WriteMessage($"\n加载备用图标 {filename} 时出错: {ex.Message}");
                                // 继续处理下一个图标
                            }
                        }
                    }
                    else
                    {
                        AutoCADService.WriteMessage($"\n备用图标路径也不存在: {backupDir}");
                    }
                }
                
                AutoCADService.WriteMessage($"\n图标加载完成，共加载 {imgList.Images.Count} 个图标");
            }
            catch (Exception ex)
            {
                AutoCADService.WriteMessage($"\n加载图标时出现异常: {ex.Message}");
            }
        }
    }
} 