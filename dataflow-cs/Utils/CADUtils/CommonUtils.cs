using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace dataflow_cs.Utils.CADUtils
{
    /// <summary>
    /// 提供通用工具方法，包括字符串处理、数值比较、图签信息处理等功能
    /// </summary>
    public static class CommonUtils
    {
        /// <summary>
        /// 更改对象的颜色
        /// </summary>
        /// <param name="objectId">对象的ObjectId</param>
        /// <param name="colorIndex">颜色索引</param>
        public static void ChangeColor(ObjectId objectId, int colorIndex)
        {
            Polyline polyline = objectId.GetObject(OpenMode.ForWrite) as Polyline;
            polyline.ColorIndex = colorIndex;
        }

        /// <summary>
        /// 将字符串转换为双精度浮点数
        /// </summary>
        /// <param name="stringContent">字符串内容</param>
        /// <returns>转换后的双精度浮点数，如果转换失败则返回NaN</returns>
        public static double StringToDouble(string stringContent)
        {
            double result;
            bool success = double.TryParse(stringContent, out result);

            if (success)
            {
                return result;
            }
            else
            {
                return double.NaN;
            }
        }

        /// <summary>
        /// 判断两个数值是否在给定容差范围内相等
        /// </summary>
        /// <param name="num1">第一个数值</param>
        /// <param name="num2">第二个数值</param>
        /// <param name="tolerance">容差</param>
        /// <returns>如果两个数值在容差范围内相等则返回true，否则返回false</returns>
        public static bool IsTwoNumEqual(double num1, double num2, double tolerance)
        {
            return Math.Abs(num1 - num2) < tolerance;
        }

        /// <summary>
        /// 获取图签信息
        /// </summary>
        /// <param name="objectId">图签对象的ObjectId</param>
        /// <returns>图签信息的JSON对象</returns>
        public static JObject GetNewTitleBlockInfoJObject(ObjectId objectId)
        {
            // 创建根JSON对象
            JObject root = new JObject();

            // 添加空的图纸信息部分
            root["DrawInfo"] = new JObject();

            using (Transaction trans = CadEnvironment.Database.TransactionManager.StartTransaction())
            {
                // 获取图签块
                BlockReference blockRef = trans.GetObject(objectId, OpenMode.ForRead) as BlockReference;

                if (blockRef != null)
                {
                    // 获取属性集合
                    AttributeCollection attCol = blockRef.AttributeCollection;

                    // 创建属性字典
                    Dictionary<string, string> attributeDict = new Dictionary<string, string>();

                    // 遍历所有属性
                    foreach (ObjectId attId in attCol)
                    {
                        AttributeReference attRef = trans.GetObject(attId, OpenMode.ForRead) as AttributeReference;

                        if (attRef != null)
                        {
                            string tagName = attRef.Tag.ToUpper();
                            string textString = attRef.TextString;

                            // 添加到属性字典
                            attributeDict[tagName] = textString;
                        }
                    }

                    // 设置图纸信息
                    JObject drawInfo = (JObject)root["DrawInfo"];

                    // 添加属性到图纸信息
                    foreach (var attribute in attributeDict)
                    {
                        drawInfo[attribute.Key] = attribute.Value;
                    }
                }

                trans.Commit();
            }

            return root;
        }

        /// <summary>
        /// 添加项目信息到图签信息中
        /// </summary>
        /// <param name="root">图签信息的JSON对象</param>
        /// <param name="dwgno">图纸编号</param>
        /// <returns>如果添加成功则返回true，否则返回false</returns>
        public static bool AddProjectInfoToNewTitleBlockInfo(JObject root, string dwgno)
        {
            if (root == null)
                return false;

            // 正则表达式匹配图纸编号格式
            Regex regex = new Regex(@"D-(\w+)-(\w+)-(\w+)");
            Match match = regex.Match(dwgno);

            if (match.Success)
            {
                // 创建项目信息部分
                JObject projectInfo = new JObject();
                
                // 设置各部分信息
                projectInfo["Project"] = match.Groups[1].Value;
                projectInfo["System"] = match.Groups[2].Value;
                projectInfo["Number"] = match.Groups[3].Value;
                
                // 添加到根对象
                root["ProjectInfo"] = projectInfo;
                
                return true;
            }

            return false;
        }

        /// <summary>
        /// 根据项目信息获取管道信息
        /// </summary>
        /// <param name="projectInfo">项目信息JSON字符串</param>
        /// <returns>管道信息辅助类实例</returns>
        public static PipeInfoHelper GetPipeInfo(string projectInfo)
        {
            if (string.IsNullOrEmpty(projectInfo))
                return null;

            JObject root;
            try
            {
                root = JObject.Parse(projectInfo);
            }
            catch
            {
                return null;
            }

            return new PipeInfoHelper(root);
        }
    }

    /// <summary>
    /// 管道信息辅助类
    /// </summary>
    public class PipeInfoHelper
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="root">管道信息的JSON对象</param>
        public PipeInfoHelper(JObject root)
        {
            JToken pipeField = root["PipeField"];
            PipeDiameterIndex = pipeField?["pipeDiameter"]?.Value<int>() ?? 0;
            InsulationThickIndex = pipeField?["insulationThick"]?.Value<int>() ?? 0;
            PipeClassIndex = pipeField?["pipeClass"]?.Value<int>() ?? 0;
            PipeCodeIndex = pipeField?["pipeCode"]?.Value<int>() ?? 0;
            PipeCodeNumIndex = pipeField?["pipeCodeNum"]?.Value<int>() ?? 0;
        }

        /// <summary>管道直径索引</summary>
        public int PipeDiameterIndex { get; set; }
        /// <summary>保温厚度索引</summary>
        public int InsulationThickIndex { get; set; }
        /// <summary>管道类别索引</summary>
        public int PipeClassIndex { get; set; }
        /// <summary>管道代码索引</summary>
        public int PipeCodeIndex { get; set; }
        /// <summary>管道代码数字索引</summary>
        public int PipeCodeNumIndex { get; set; }

        /// <summary>
        /// 获取管道类别
        /// </summary>
        /// <param name="pipeNum">管道编号</param>
        /// <returns>管道类别</returns>
        public string GetPipeClass(string pipeNum)
        {
            string[] elements = pipeNum.Split('-');
            if (elements.Length > PipeClassIndex)
            {
                return elements[PipeClassIndex];
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取管道直径
        /// </summary>
        /// <param name="pipeNum">管道编号</param>
        /// <returns>管道直径</returns>
        public string GetPipeDiameter(string pipeNum)
        {
            string[] elements = pipeNum.Split('-');
            if (elements.Length > PipeDiameterIndex)
            {
                return elements[PipeDiameterIndex];
            }
            return string.Empty;
        }
    }
} 