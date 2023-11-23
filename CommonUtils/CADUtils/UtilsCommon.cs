using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DLCommonUtils.CADUtils;
using DLCommonUtils.ConstUtils;
using Autodesk.AutoCAD.ApplicationServices;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace DLCommonUtils.CADUtils
{

    public static class UtilsCommnon
    {
        public static void UtilsChangeColor(ObjectId objectId, int colorIndex)
        {
            Polyline polyline = objectId.GetObject(OpenMode.ForWrite) as Polyline;
            polyline.ColorIndex = colorIndex;
        }

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

        public static bool UtilsIsTwoNumEqual(double num1, double num2, double tolerance)
        {
            return Math.Abs(num1 - num2) < tolerance;
        }

        /// <summary>
        /// 获取图签信息
        /// </summary>
        /// <returns></returns>
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

    public class PipeInfoHelper
    {
        public PipeInfoHelper(JObject root)
        {
            PipeDiameterIndex = root.Value<int>("pipeDiameter");
            InsulationThickIndex = root.Value<int>("insulationThick");
            PipeClassIndex = root.Value<int>("pipeClass");
            PipeCodeIndex = root.Value<int>("pipeCode");
            PipeCodeNumIndex = root.Value<int>("pipeCodeNum");
        }
        public int PipeDiameterIndex { get; set; }
        public int InsulationThickIndex { get; set; }
        public int PipeClassIndex { get; set; }
        public int PipeCodeIndex { get; set; }
        public int PipeCodeNumIndex { get; set; }

        public string GetPipeClass(string pipeNum)
        {
            string[] strs = pipeNum.Split('-');
            if (strs.Length > PipeClassIndex)
                return strs[PipeClassIndex];
            else
                return string.Empty;
        }

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
