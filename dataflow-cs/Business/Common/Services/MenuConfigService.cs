using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using dataflow_cs.Business.Common.Models;
using Autodesk.AutoCAD.ApplicationServices;
using System.Collections.Generic;

namespace dataflow_cs.Business.Common.Services
{
    /// <summary>
    /// 菜单配置服务
    /// </summary>
    public class MenuConfigService
    {
        private static readonly string ConfigFileName = "MenuConfig.json";
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
                    
                    // 再向上一级查找
                    string grandParentDir = Directory.GetParent(parentDir)?.FullName;
                    if (grandParentDir != null)
                    {
                        string configDir3 = Path.Combine(grandParentDir, "config");
                        if (Directory.Exists(configDir3) && File.Exists(Path.Combine(configDir3, ConfigFileName)))
                        {
                            return configDir3;
                        }
                    }
                }
                
                // 如果没有找到配置文件，则使用程序集目录下的config目录
                string defaultConfigDir = Path.Combine(assemblyDir, "config");
                if (!Directory.Exists(defaultConfigDir))
                {
                    Directory.CreateDirectory(defaultConfigDir);
                }
                
                return defaultConfigDir;
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n获取配置目录时出错: {ex.Message}");
                
                // 兜底使用当前工作目录
                string fallbackDir = Path.Combine(Environment.CurrentDirectory, "config");
                if (!Directory.Exists(fallbackDir))
                {
                    Directory.CreateDirectory(fallbackDir);
                }
                return fallbackDir;
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
                    // 如果不存在，创建一个默认配置
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
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n读取菜单配置时出错: {ex.Message}");
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
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n成功保存菜单配置到 {ConfigFilePath}");
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n保存菜单配置时出错: {ex.Message}");
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
                PaletteTitle = "天正数智设计",
                PaletteWidth = 250,
                PaletteHeight = 400,
                MenuGroups = new List<MenuGroup>
                {
                    new MenuGroup
                    {
                        Title = "一级菜单 1",
                        IconKey = "folder",
                        Items = new List<MenuItem>
                        {
                            new MenuItem { Title = "二级菜单 1-1", IconKey = "本地生活", Command = "LINE" },
                            new MenuItem { Title = "二级菜单 1-2", IconKey = "本地生活", Command = "CIRCLE" }
                        }
                    },
                    new MenuGroup
                    {
                        Title = "一级菜单 2",
                        IconKey = "folder",
                        Items = new List<MenuItem>
                        {
                            new MenuItem { Title = "二级菜单 2-1", IconKey = "编辑", Command = "RECTANGLE" },
                            new MenuItem { Title = "二级菜单 2-2", IconKey = "编辑", Command = "ARC" }
                        }
                    },
                    new MenuGroup
                    {
                        Title = "一级菜单 3",
                        IconKey = "folder",
                        Items = new List<MenuItem>
                        {
                            new MenuItem { Title = "二级菜单 3-1", IconKey = "second", Command = "TEXT" }
                        }
                    }
                }
            };
        }
    }
} 