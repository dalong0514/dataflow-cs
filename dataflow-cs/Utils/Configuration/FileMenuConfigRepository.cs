using System;
using System.IO;
using Newtonsoft.Json;
using dataflow_cs.Domain.ValueObjects;
using dataflow_cs.Domain.Repositories.Interfaces;
using System.Collections.Generic;

namespace dataflow_cs.Utils.Configuration
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
        /// 获取当前使用的配置文件路径
        /// </summary>
        /// <returns>配置文件路径</returns>
        public string GetConfigFilePath()
        {
            return _configFilePath;
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
            try
            {
                const string configFileName = "MenuConfig.json";
                
                // 首先尝试获取程序集所在目录
                string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string assemblyDir = Path.GetDirectoryName(assemblyLocation);
                
                // 检查当前程序集目录下是否有config文件夹和配置文件
                string configDir1 = Path.Combine(assemblyDir, "config");
                string configPath1 = Path.Combine(configDir1, configFileName);
                if (Directory.Exists(configDir1) && File.Exists(configPath1))
                {
                    return configPath1;
                }
                
                // 回退到上一级目录查找config目录
                string parentDir = Directory.GetParent(assemblyDir)?.FullName;
                if (parentDir != null)
                {
                    string configDir2 = Path.Combine(parentDir, "config");
                    string configPath2 = Path.Combine(configDir2, configFileName);
                    if (Directory.Exists(configDir2) && File.Exists(configPath2))
                    {
                        return configPath2;
                    }
                }
                
                // 尝试从AppDomain的BaseDirectory查找
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string configDir3 = Path.Combine(baseDir, "config");
                string configPath3 = Path.Combine(configDir3, configFileName);
                if (Directory.Exists(configDir3) && File.Exists(configPath3))
                {
                    return configPath3;
                }
                
                // 如果都未找到，返回默认路径
                return Path.Combine(assemblyDir, "config", configFileName);
            }
            catch
            {
                // 出错时返回默认应用程序目录下的配置文件
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                return Path.Combine(appDirectory, "config", "MenuConfig.json");
            }
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
                PaletteWidth = 300,
                PaletteHeight = 500,
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