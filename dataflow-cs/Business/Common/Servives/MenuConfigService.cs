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
            // 默认在应用程序目录下创建config文件夹
            string appDir = Path.GetDirectoryName(Application.ExecutablePath);
            string configDir = Path.Combine(appDir, "config");
            
            // 确保目录存在
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }
            
            return configDir;
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
                return JsonConvert.DeserializeObject<MenuConfig>(json);
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