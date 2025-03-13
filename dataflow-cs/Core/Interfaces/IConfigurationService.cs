using System;
using System.Collections.Generic;

namespace dataflow_cs.Core.Interfaces
{
    /// <summary>
    /// 配置服务接口，定义配置项的读取和保存方法
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>
        /// 获取字符串配置项
        /// </summary>
        /// <param name="key">配置项键名</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值，如果未找到则返回默认值</returns>
        string GetString(string key, string defaultValue = "");

        /// <summary>
        /// 获取整数配置项
        /// </summary>
        /// <param name="key">配置项键名</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值，如果未找到则返回默认值</returns>
        int GetInt(string key, int defaultValue = 0);

        /// <summary>
        /// 获取双精度浮点数配置项
        /// </summary>
        /// <param name="key">配置项键名</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值，如果未找到则返回默认值</returns>
        double GetDouble(string key, double defaultValue = 0.0);

        /// <summary>
        /// 获取布尔型配置项
        /// </summary>
        /// <param name="key">配置项键名</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值，如果未找到则返回默认值</returns>
        bool GetBool(string key, bool defaultValue = false);

        /// <summary>
        /// 设置配置项值
        /// </summary>
        /// <param name="key">配置项键名</param>
        /// <param name="value">配置项值</param>
        void SetValue(string key, object value);

        /// <summary>
        /// 保存配置更改
        /// </summary>
        /// <returns>是否保存成功</returns>
        bool SaveChanges();

        /// <summary>
        /// 重新加载配置
        /// </summary>
        void Reload();
    }
} 