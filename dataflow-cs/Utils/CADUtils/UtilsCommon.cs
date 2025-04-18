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
using dataflow_cs.Utils.ConstUtils;

namespace dataflow_cs.Utils.CADUtils
{
    /// <summary>
    /// AutoCAD通用工具类，提供各种通用操作方法
    /// </summary>
    public static class UtilsCommon
    {

        /// <summary>
        /// 将字符串转换为双精度浮点数
        /// </summary>
        /// <param name="stringContent">要转换的字符串</param>
        /// <returns>转换后的双精度浮点数，转换失败时返回NaN</returns>
        public static double UtilsStringToDouble(string stringContent)
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
        /// 判断两个数字在指定容差范围内是否相等
        /// </summary>
        /// <param name="num1">第一个数</param>
        /// <param name="num2">第二个数</param>
        /// <param name="tolerance">容差值</param>
        /// <returns>如果两数之差小于容差值则返回true</returns>
        public static bool UtilsIsTwoNumEqual(double num1, double num2, double tolerance)
        {
            return Math.Abs(num1 - num2) < tolerance;
        }

        /// <summary>
        /// 获取图签块信息并转换为JSON对象
        /// </summary>
        /// <param name="objectId">图签块的ObjectId</param>
        /// <returns>包含图签信息的JSON对象，如果获取失败则返回null</returns>
        public static JObject UtilsGetNewTitleBlockInfoJObject(ObjectId objectId)
        {
            BlockReference br = objectId.GetObject(OpenMode.ForRead) as BlockReference;
            BlockTableRecord btr = br.BlockTableRecord.GetObject(OpenMode.ForRead) as BlockTableRecord;
            Dictionary<string, string> titleBlockInfoDic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            BlockTableRecordEnumerator btre = btr.GetEnumerator();
            while (btre.MoveNext())
            {
                ObjectId id = btre.Current;
                try
                {
                    Entity ent = id.GetObject(OpenMode.ForRead) as Entity;
                    if (ent == null)
                        continue;

                    if (ent is MText mText)
                    {
                        string xdata = UtilsCADActive.UtilsGetXData(ent.Id, "GeYuanXDATA");
                        if (xdata != null)
                            titleBlockInfoDic[xdata] = mText.Contents;
                    }
                    else if (ent is DBText dBText)
                    {
                        string xdata = UtilsCADActive.UtilsGetXData(ent.Id, "GeYuanXDATA");
                        if (xdata != null)
                            titleBlockInfoDic[xdata] = dBText.TextString;
                    }
                }
                catch
                {

                }
            }

            if (titleBlockInfoDic.Count == 0 || !titleBlockInfoDic.ContainsKey("picno"))
                return null;

            JObject root = new JObject();
            root["data_class"] = "projectinfo";
            string dwgno = titleBlockInfoDic["picno"];

            UtilsAddProjectInfoToNewTitleBlockInfo(root, dwgno);

            if (!titleBlockInfoDic.ContainsKey("picname"))
                return null;

            root["drawname"] = titleBlockInfoDic["picname"];
            root["projectname"] = titleBlockInfoDic["prjname"];
            root["monomername"] = titleBlockInfoDic["itemname"];
            root["projectmanager"] = titleBlockInfoDic["itemperson"];
            root["professionalmanager"] = titleBlockInfoDic["specialmanager"];
            root["designer"] = titleBlockInfoDic["designer"];
            root["checker"] = titleBlockInfoDic["proofreader"];
            root["verifier"] = titleBlockInfoDic["reviewer"];
            root["approver"] = titleBlockInfoDic["reader"];
            if (titleBlockInfoDic.ContainsKey("pic_bl"))
                root["scale"] = titleBlockInfoDic["pic_bl"];
            if (titleBlockInfoDic.ContainsKey("pt"))
                root["pt"] = titleBlockInfoDic["pt"];
            if (titleBlockInfoDic.ContainsKey("ct"))
                root["ct"] = titleBlockInfoDic["ct"];


            return root;
        }

        /// <summary>
        /// 根据图纸编号向JSON对象中添加项目信息
        /// </summary>
        /// <param name="root">要添加信息的JSON对象</param>
        /// <param name="dwgno">图纸编号</param>
        /// <returns>添加成功返回true，失败返回false</returns>
        public static bool UtilsAddProjectInfoToNewTitleBlockInfo(JObject root, string dwgno)
        {
            root["dwgno"] = dwgno;
            string[] dwgNoInfo = dwgno.Split('-');
            if (dwgNoInfo.Length < 2)
                return false;

            string projectnum = string.Empty;
            if (Regex.IsMatch(dwgNoInfo[0], ".*[a-zA-Z]{2}$"))
                projectnum = dwgNoInfo[0].Substring(0, dwgNoInfo[0].Length - 2);
            else
                projectnum = dwgNoInfo[0];
            root["projectnum"] = projectnum;
            root["monomernum"] = dwgNoInfo[1];
            return true;
        }

        /// <summary>
        /// 获取管道信息
        /// </summary>
        /// <param name="projectInfo">项目信息</param>
        /// <returns>管道信息辅助类实例</returns>
        public static PipeInfoHelper UtilsGetPipeInfo(string projectInfo)
        {
            JArray jArray = new JArray();
            JObject root = new JObject();
            root["monomerNum"] = "";
            root["projectNum"] = projectInfo;
            root["tagDataList"] = new JArray();
            jArray.Add(root);

            string json = UtilsWeb.DoPost(ConstURL.PipelineTemplateURL, jArray.ToString());
            if (string.IsNullOrEmpty(json))
            {
                json = "{\r\n  \"PL1101-50-2J1-H5\": {\r\n    \"pipeDiameter\": 1,\r\n    \"insulationThick\": 3,\r\n    \"pipeClass\": 2,\r\n    \"pipeCode\": 0,\r\n    \"pipeCodeNum\": 0\r\n  },\r\n  \"PL1101-50-2M5/15-2A1-H5\": {\r\n    \"insulationThick\": 2,\r\n    \"pipeDiameterClass\": 1,\r\n    \"pipeCode\": 0,\r\n    \"pipeCodeNum\": 0\r\n  }\r\n}";
            }
            //return null;

            root = (JObject)JsonConvert.DeserializeObject(json);
            JObject r = (JObject)root.First.First;

            return new PipeInfoHelper(r);
        }

    }

    /// <summary>
    /// 管道信息辅助类，用于解析和获取管道相关信息
    /// </summary>
    public class PipeInfoHelper
    {
        /// <summary>
        /// 通过JSON对象初始化管道信息辅助类
        /// </summary>
        /// <param name="root">包含管道信息的JSON对象</param>
        public PipeInfoHelper(JObject root)
        {
            PipeDiameterIndex = root.Value<int>("pipeDiameter");
            InsulationThickIndex = root.Value<int>("insulationThick");
            PipeClassIndex = root.Value<int>("pipeClass");
            PipeCodeIndex = root.Value<int>("pipeCode");
            PipeCodeNumIndex = root.Value<int>("pipeCodeNum");
        }
        /// <summary>
        /// 管道直径索引
        /// </summary>
        public int PipeDiameterIndex { get; set; }
        /// <summary>
        /// 保温厚度索引
        /// </summary>
        public int InsulationThickIndex { get; set; }
        /// <summary>
        /// 管道等级索引
        /// </summary>
        public int PipeClassIndex { get; set; }
        /// <summary>
        /// 管道代码索引
        /// </summary>
        public int PipeCodeIndex { get; set; }
        /// <summary>
        /// 管道代码数字索引
        /// </summary>
        public int PipeCodeNumIndex { get; set; }

        /// <summary>
        /// 从管道编号中获取管道等级
        /// </summary>
        /// <param name="pipeNum">管道编号</param>
        /// <returns>管道等级，未找到则返回空字符串</returns>
        public string GetPipeClass(string pipeNum)
        {
            string[] strs = pipeNum.Split('-');
            if (strs.Length > PipeClassIndex)
                return strs[PipeClassIndex];
            else
                return string.Empty;
        }

        /// <summary>
        /// 从管道编号中获取管道直径
        /// </summary>
        /// <param name="pipeNum">管道编号</param>
        /// <returns>管道直径，未找到则返回空字符串</returns>
        public string GetPipeDiameter(string pipeNum)
        {
            string[] strs = pipeNum.Split('-');
            if (strs.Length > PipeDiameterIndex)
                return strs[PipeDiameterIndex];
            else
                return string.Empty;
        }
    }
} 