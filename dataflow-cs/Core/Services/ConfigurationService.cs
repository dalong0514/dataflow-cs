using dataflow_cs.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml.Linq;

namespace dataflow_cs.Core.Services
{
    /// <summary>
    /// 配置服务实现类，使用XML文件存储配置
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private readonly string _configFilePath;
        private XDocument _configDocument;
        private Dictionary<string, object> _configCache;
        private static ConfigurationService _instance;

        /// <summary>
        /// 获取配置服务单例
        /// </summary>
        public static ConfigurationService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ConfigurationService();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 私有构造函数，确保单例模式
        /// </summary>
        private ConfigurationService()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string configDirectory = Path.Combine(appDataPath, "DataFlowCS", "Config");
            
            if (!Directory.Exists(configDirectory))
            {
                Directory.CreateDirectory(configDirectory);
            }

            _configFilePath = Path.Combine(configDirectory, "DataFlowConfig.xml");
            _configCache = new Dictionary<string, object>();
            
            InitializeConfig();
        }

        /// <summary>
        /// 初始化配置文件
        /// </summary>
        private void InitializeConfig()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    _configDocument = XDocument.Load(_configFilePath);
                }
                else
                {
                    // 创建默认配置文件
                    _configDocument = new XDocument(
                        new XElement("Configuration",
                            new XElement("Settings")
                        )
                    );
                    _configDocument.Save(_configFilePath);
                }

                // 加载到缓存
                LoadConfigToCache();
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "初始化配置文件失败");
                
                // 创建一个新的配置文档
                _configDocument = new XDocument(
                    new XElement("Configuration",
                        new XElement("Settings")
                    )
                );
                _configCache = new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// 将配置加载到缓存
        /// </summary>
        private void LoadConfigToCache()
        {
            _configCache.Clear();
            
            var settings = _configDocument.Root.Element("Settings");
            if (settings != null)
            {
                foreach (var element in settings.Elements())
                {
                    string key = element.Name.LocalName;
                    string value = element.Value;
                    _configCache[key] = value;
                }
            }
        }

        /// <summary>
        /// 获取字符串配置
        /// </summary>
        public string GetString(string key, string defaultValue = "")
        {
            if (_configCache.ContainsKey(key) && _configCache[key] != null)
            {
                return _configCache[key].ToString();
            }
            return defaultValue;
        }

        /// <summary>
        /// 获取整数配置
        /// </summary>
        public int GetInt(string key, int defaultValue = 0)
        {
            if (_configCache.ContainsKey(key) && _configCache[key] != null)
            {
                if (int.TryParse(_configCache[key].ToString(), out int value))
                {
                    return value;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// 获取双精度浮点数配置
        /// </summary>
        public double GetDouble(string key, double defaultValue = 0.0)
        {
            if (_configCache.ContainsKey(key) && _configCache[key] != null)
            {
                if (double.TryParse(_configCache[key].ToString(), out double value))
                {
                    return value;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// 获取布尔型配置
        /// </summary>
        public bool GetBool(string key, bool defaultValue = false)
        {
            if (_configCache.ContainsKey(key) && _configCache[key] != null)
            {
                if (bool.TryParse(_configCache[key].ToString(), out bool value))
                {
                    return value;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// 设置配置值
        /// </summary>
        public void SetValue(string key, object value)
        {
            _configCache[key] = value;
            
            var settings = _configDocument.Root.Element("Settings");
            var element = settings.Element(key);
            
            if (element != null)
            {
                element.Value = value.ToString();
            }
            else
            {
                settings.Add(new XElement(key, value.ToString()));
            }
        }

        /// <summary>
        /// 保存配置变更
        /// </summary>
        public bool SaveChanges()
        {
            try
            {
                _configDocument.Save(_configFilePath);
                return true;
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "保存配置文件失败");
                return false;
            }
        }

        /// <summary>
        /// 重载配置
        /// </summary>
        public void Reload()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    _configDocument = XDocument.Load(_configFilePath);
                    LoadConfigToCache();
                }
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "重新加载配置文件失败");
            }
        }
    }
} 