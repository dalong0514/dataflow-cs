using System;
using System.IO;
using Newtonsoft.Json;
using dataflow_cs.Domain.ValueObjects;
using dataflow_cs.Domain.Repositories.Interfaces;
using System.Collections.Generic;

namespace dataflow_cs.Infrastructure.Configuration
{
    /// <summary>
    /// 菜单配置仓储的文件实现
    /// </summary>
    public class FileMenuConfigRepository : IMenuConfigRepository
    {
        private readonly string _configFilePath;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="configFilePath">配置文件路径，如果为null则使用默认路径</param>
        public FileMenuConfigRepository(string configFilePath = null)
        {
            // 如果未指定路径，则使用默认配置文件路径
            _configFilePath = configFilePath ?? GetDefaultConfigPath();
        }

        /// <summary>
        /// 加载菜单配置
        /// </summary>
        /// <returns>菜单配置对象</returns>
        public MenuConfig Load()
        {
            try
            {
                // 确保配置文件目录存在
                string directory = Path.GetDirectoryName(_configFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 如果配置文件不存在，创建一个默认配置
                if (!File.Exists(_configFilePath))
                {
                    MenuConfig defaultConfig = CreateDefaultConfig();
                    Save(defaultConfig);
                    return defaultConfig;
                }

                // 读取配置文件内容
                string jsonContent = File.ReadAllText(_configFilePath);
                
                // 反序列化为MenuConfig对象
                MenuConfig config = JsonConvert.DeserializeObject<MenuConfig>(jsonContent);
                
                // 如果反序列化失败，返回默认配置
                if (config == null)
                {
                    return CreateDefaultConfig();
                }

                return config;
            }
            catch (Exception ex)
            {
                // 记录异常并返回默认配置
                // 实际项目应使用日志记录异常
                Console.WriteLine($"加载菜单配置时出错: {ex.Message}");
                return CreateDefaultConfig();
            }
        }

        /// <summary>
        /// 保存菜单配置
        /// </summary>
        /// <param name="config">要保存的菜单配置</param>
        public void Save(MenuConfig config)
        {
            try
            {
                // 确保配置文件目录存在
                string directory = Path.GetDirectoryName(_configFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 序列化MenuConfig对象为JSON
                string jsonContent = JsonConvert.SerializeObject(config, Formatting.Indented);
                
                // 写入配置文件
                File.WriteAllText(_configFilePath, jsonContent);
            }
            catch (Exception ex)
            {
                // 记录异常
                // 实际项目应使用日志记录异常
                Console.WriteLine($"保存菜单配置时出错: {ex.Message}");
                throw; // 重新抛出异常，让调用者知道保存失败
            }
        }

        /// <summary>
        /// 获取默认配置文件路径
        /// </summary>
        /// <returns>默认配置文件路径</returns>
        private string GetDefaultConfigPath()
        {
            // 在应用目录下创建config文件夹，并使用MenuConfig.json作为配置文件名
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(appDirectory, "config", "MenuConfig.json");
        }

        /// <summary>
        /// 创建默认菜单配置
        /// </summary>
        /// <returns>默认菜单配置</returns>
        private MenuConfig CreateDefaultConfig()
        {
            // 创建默认菜单配置
            MenuConfig config = new MenuConfig
            {
                PaletteTitle = "数智设计",
                PaletteWidth = 250,
                PaletteHeight = 400,
                MenuGroups = new List<MenuGroup>()
            };

            // 添加示例菜单组
            MenuGroup group1 = new MenuGroup
            {
                Title = "示例菜单组",
                IconKey = "folder",
                Items = new List<MenuItem>()
            };

            // 添加示例菜单项
            group1.Items.Add(new MenuItem
            {
                Title = "示例命令",
                IconKey = "command",
                Command = "LINE"
            });

            config.MenuGroups.Add(group1);
            
            return config;
        }
    }
} 