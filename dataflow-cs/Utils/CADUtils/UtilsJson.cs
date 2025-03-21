using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dataflow_cs.Utils.CADUtils
{
    /// <summary>
    /// JSON操作工具类，提供JSON对象的扩展方法
    /// </summary>
    public static class UtilsJson
    {
        /// <summary>
        /// 从JObject中获取指定键的字符串值
        /// </summary>
        /// <param name="root">JSON对象</param>
        /// <param name="key">要获取值的键</param>
        /// <returns>键对应的字符串值，如果键不存在或值为空则返回空字符串</returns>
        public static string UtilsGetStrValue(this JObject root, string key)
        {
            if (root.ContainsKey(key) && !UtilsIsNullOrEmpty(root[key]))
            {
                return root[key].ToString();
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// 判断JToken是否为null或空值
        /// </summary>
        /// <param name="token">要检查的JToken</param>
        /// <returns>如果token为null、空数组、空对象、空字符串或JTokenType.Null则返回true</returns>
        public static bool UtilsIsNullOrEmpty(this JToken token)
        {
            return (token == null) ||
                   (token.Type == JTokenType.Array && !token.HasValues) ||
                   (token.Type == JTokenType.Object && !token.HasValues) ||
                   (token.Type == JTokenType.String && token.ToString() == string.Empty) ||
                   (token.Type == JTokenType.Null);
        }
    }
}
