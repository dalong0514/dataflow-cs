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
