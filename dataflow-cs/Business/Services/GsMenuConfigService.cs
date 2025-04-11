using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using dataflow_cs.Domain.ValueObjects;
using Newtonsoft.Json;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace dataflow_cs.Business.Services
{
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
                    
                    // 兼容旧版配置：如果没有Tabs但有MenuGroups，则自动转换
                    if ((config.Tabs == null || config.Tabs.Count == 0) && config.MenuGroups != null && config.MenuGroups.Any())
                    {
                        string[] tabNames = new string[] { "工艺流程", "设备布置", "二维配管" };
                        config.Tabs = new List<TabConfig>
                        {
                            new TabConfig
                            {
                                TabName = tabNames[0],
                                MenuGroups = config.MenuGroups
                            }
                        };
                    }
                    
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
            string[] tabNames = new string[] { "工艺流程", "设备布置", "二维配管" };
            
            return new MenuConfig
            {
                PaletteTitle = "数智设计-工艺",
                PaletteWidth = 250,
                PaletteHeight = 400,
                Tabs = new List<TabConfig>
                {
                    new TabConfig
                    {
                        TabName = tabNames[0],
                        MenuGroups = new List<MenuGroup>
                        {
                            new MenuGroup
                            {
                                Title = "工艺流程菜单1",
                                IconKey = "folder",
                                Items = new List<MenuItem>
                                {
                                    new MenuItem { Title = "工艺流程菜单1-1", IconKey = "本地生活", Command = "LINE" },
                                    new MenuItem { Title = "工艺流程菜单1-2", IconKey = "本地生活", Command = "CIRCLE" }
                                }
                            }
                        }
                    },
                    new TabConfig
                    {
                        TabName = tabNames[1],
                        MenuGroups = new List<MenuGroup>
                        {
                            new MenuGroup
                            {
                                Title = "设备布置菜单1",
                                IconKey = "folder",
                                Items = new List<MenuItem>
                                {
                                    new MenuItem { Title = "设备布置菜单1-1", IconKey = "编辑", Command = "RECTANGLE" },
                                    new MenuItem { Title = "设备布置菜单1-2", IconKey = "编辑", Command = "ARC" }
                                }
                            }
                        }
                    },
                    new TabConfig
                    {
                        TabName = tabNames[2],
                        MenuGroups = new List<MenuGroup>
                        {
                            new MenuGroup
                            {
                                Title = "二维配管菜单1",
                                IconKey = "folder",
                                Items = new List<MenuItem>
                                {
                                    new MenuItem { Title = "二维配管菜单1-1", IconKey = "second", Command = "TEXT" },
                                    new MenuItem { Title = "二维配管菜单1-2", IconKey = "second", Command = "MTEXT" }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
} 