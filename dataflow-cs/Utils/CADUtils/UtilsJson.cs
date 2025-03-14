using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DLCommonUtils.CADUtils
{
    public static class UtilsJson
    {
        public static string UtilsGetStrValue(this JObject root, string key)
        {
            if (root.ContainsKey(key) && !UtilsIsNullOrEmpty(root[key]))
            {
                return root[key].ToString();
            }
            else
                return string.Empty;
        }

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
