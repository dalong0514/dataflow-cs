using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DLCommonUtils.ConstUtils
{
    public static class ConstURL
    {
        /// <summary>
        /// 基本接口
        /// </summary>
        /// 
#if false
        public const string BasicURL = "http://192.168.1.38:9093/";
#else
        public const string BasicURL = "http://192.168.1.38:9092/";

        //public const string BasicURL = "http://192.168.137.28:9093/";

#endif
        /// <summary>
        /// 梁有关接口
        /// </summary>
        public const string BeamURL = BasicURL + @"cad/beam/";
        /// <summary>
        /// 获取梁编号有关接口
        /// </summary>
        public const string BeamGetNumURL = BeamURL + @"getBeam";
        /// <summary>
        /// 管架有关接口
        /// </summary>
        public const string PipeRackURL = BasicURL + @"cad/ws/pipe/rack/";
        /// <summary>
        /// 管架数据存储接口
        /// </summary>
        public const string PipeRackSaveDataURL = PipeRackURL + @"saveCadData";
        /// <summary>
        /// 管架数据存储接口
        /// </summary>
        public const string PipeRackDeleteByProfileNumURL = PipeRackURL + @"deleteByProfileNum";
        /// <summary>
        /// 管架数据获取接口
        /// </summary>
        public const string PipeRackGetDataURL = PipeRackURL + @"getCadData?monomerNum={0}&projectNum={1}";
        /// <summary>
        /// 管架一览表数据获取接口
        /// </summary>
        public const string PipeRackTableGetDataURL = PipeRackURL + @"getHandleData?monomerNum={0}&projectNum={1}";
        /// <summary>
        /// 电气有关接口
        /// </summary>
        public const string ElectricUrl = BasicURL + @"cad/gs/electric/";
        /// <summary>
        /// 电气有关接口
        /// </summary>
        public const string ElectricGetListByProjectUrl = ElectricUrl + @"getListByProject?monomerNum={0}&projectNum={1}";
        /// <summary>
        /// 总管有关接口
        /// </summary>
        public const string TotalPipeUrl = BasicURL + @"cad/gs/pipe/line/";
        /// <summary>
        /// 根据来去向或管道号获取管道数据
        /// </summary>
        public const string PipelineGetDataWithFromToURL = TotalPipeUrl + @"getDataWithFromTo";
        /// <summary>
        /// 根据设备位号获取管道数据
        /// </summary>
        public const string TotalPipeGetDataWithTagURL = TotalPipeUrl + @"getDataWithTag";

        public const string TotalPipeGetPipeNumDataWithTagURL = TotalPipeUrl + @"getPipeNumDataWithTag";


        public const string PipelineTemplateURL = TotalPipeUrl + @"getPipelineTemplate";


        /// <summary>
        /// 污水池有关接口
        /// </summary>
        public const string SewageUrl = BasicURL + @"cad/ss/sewage/";

        public const string SewageGetSewagesByProjectUrl = SewageUrl + @"getSewagesByProject?monomerNum={0}&projectNum={1}";

        /// <summary>
        /// 获取设备是否是用电设备，成套设备
        /// </summary>
        public const string DsGetElectricEquiptUrl = BasicURL + @"cad/ds/low/vol/getElectricEquip";
        /// <summary>
        /// 获取电缆位号
        /// </summary>
        public const string DsGetCableTagUrl = BasicURL + @"cad/ds/low/vol/labelCable";
        /// <summary>
        /// 获取桥架规格
        /// </summary>
        public const string DsGetCrossBridgeSpecUrl = BasicURL + @"cad/ds/low/vol/getCrossBridgeSize";

        /// <summary>
        /// 建筑楼梯信息保存
        /// </summary>
        public const string JsStairSaveUrl = BasicURL + @"cad/jsStair/save";

        public const string JsGetByProjectNumMonomerNumUrl = BasicURL + @"cad/jsStair/getByProjectNumMonomerNum?monomerNum={0}&projectNum={1}";

        public const string TsNodeDetailUrl = BasicURL + @"cad/imageStorage/get?name";


        public const string NsGetCompositeMaterialListUrl = BasicURL + @"cad/ns/freeze/getCompositeMaterialList?monomerNum={0}&projectNum={1}";


        public const string NsGetEquipLoadListUrl = BasicURL + @"cad/ns/freeze/getEquipLoadList?monomerNum={0}&projectNum={1}";

        public static string GetAbsoluteURL(string subURL)
        {
            string url = BasicURL;
            if (url.EndsWith("/") && subURL.StartsWith("/"))
            {
                return url + subURL.Substring(1);
            }
            else if (url.EndsWith("/") && (!subURL.StartsWith("/")))
            {
                return url + subURL;
            }
            else if ((!url.EndsWith("/")) && (subURL.StartsWith("/")))
            {
                return url + subURL;
            }
            else
            {
                return url + "/" + subURL;
            }
        }
    }
}
