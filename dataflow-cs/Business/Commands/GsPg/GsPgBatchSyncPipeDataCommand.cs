using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using dataflow_cs.Core.Models;
using dataflow_cs.Core.Services;
using dataflow_cs.Utils.CADUtils;
using dataflow_cs.Utils.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


namespace dataflow_cs.Business.Commands.GsPg
{
    /// <summary>
    /// 批量同步管道数据命令
    /// </summary>
    public class GsPgBatchSyncPipeDataCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DPS";

        /// <summary>
        /// 已处理的多段线ID列表
        /// </summary>
        public static List<ObjectId> _processedPolylineIds = new List<ObjectId>();

        /// <summary>
        /// 已处理的管道弯头对象ID列表
        /// </summary>
        public static List<ObjectId> _processedPipeElbowObjectIds = new List<ObjectId>();

        /// <summary>
        /// 已处理的双线管道对象ID列表
        /// </summary>
        public static List<ObjectId> _processedDoublePipeLineIds = new List<ObjectId>();

        /// <summary>
        /// 已处理的双线管道弯头对象ID列表
        /// </summary>
        public static List<ObjectId> _processedDoublePipeElbowObjectIds = new List<ObjectId>();

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            try
            {
                // 调用批量同步管道数据的方法
                GsPgBatchSynPipeData();
                return true;
            }
            catch (Exception ex)
            {
                editor.WriteMessage($"\n执行批量同步管道数据命令时发生错误: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 判断管道元素是否在管道线上
        /// </summary>
        /// <param name="basePoint">基准点</param>
        /// <param name="pipeLineObjectId">管道线对象ID</param>
        /// <returns>是否在管道线上</returns>
        public static bool IsPipeElementOnPipeLine(Point3d basePoint, ObjectId pipeLineObjectId)
        {
            return UtilsGeometry.UtilsIsPointOnPolyline(basePoint, pipeLineObjectId, 5);

        }

        /// <summary>
        /// 判断管道元素是否在管道线端点上
        /// </summary>
        /// <param name="basePoint">基准点</param>
        /// <param name="pipeLineObjectId">管道线对象ID</param>
        /// <returns>是否在管道线端点上</returns>
        public static bool IsPipeElementOnPipeLineEnds(Point3d basePoint, ObjectId pipeLineObjectId)
        {
            return UtilsGeometry.UtilsIsPointOnPolylineEnds(basePoint, pipeLineObjectId, 5);

        }

        /// <summary>
        /// 获取管道数据
        /// </summary>
        /// <param name="pipeNumObjectId">管道编号对象ID</param>
        /// <returns>管道数据字典</returns>
        public static Dictionary<string, string> GsPgGetPipeData(ObjectId pipeNumObjectId)
        {
            Dictionary<string, string> propertyValueDictList = UtilsBlock.UtilsGetAllPropertyDictList(pipeNumObjectId);
            // Not setting the Elevation attribute value can conveniently avoid synchronously modifying the auxiliary arrow block elevation on other pipelines
            return new Dictionary<string, string>
            {
                { "pipeNum", propertyValueDictList["PIPENUM"] },
                { "pipeElevation", propertyValueDictList["ELEVATION"] }
            };
        }

        /// <summary>
        /// 通过管道元素获取其所在的管道线
        /// </summary>
        /// <param name="pipeElementObjectId">管道元素对象ID</param>
        /// <param name="pipeLineObjectIds">管道线对象ID列表</param>
        /// <returns>管道元素所在的管道线对象ID</returns>
        public static ObjectId GsPgGetPipeLineByOnPL(ObjectId pipeElementObjectId, List<ObjectId> pipeLineObjectIds)
        {
            pipeLineObjectIds = pipeLineObjectIds.Where(x => IsPipeElementOnPipeLine(UtilsBlock.UtilsGetBlockBasePoint(pipeElementObjectId), x)).ToList();
            if (pipeLineObjectIds.Count != 0)
            {
                return pipeLineObjectIds[0];
            }
            return ObjectId.Null;
        }

        /// <summary>
        /// 通过管道元素获取其所在的管道线端点
        /// </summary>
        /// <param name="pipeElementObjectId">管道元素对象ID</param>
        /// <param name="pipeLineObjectIds">管道线对象ID列表</param>
        /// <returns>管道元素所在的管道线端点对象ID</returns>
        public static ObjectId GsPgGetPipeLineByOnPLEnd(ObjectId pipeElementObjectId, List<ObjectId> pipeLineObjectIds)
        {
            pipeLineObjectIds = pipeLineObjectIds.Where(x => IsPipeElementOnPipeLineEnds(UtilsBlock.UtilsGetBlockBasePoint(pipeElementObjectId), x)).ToList();
            if (pipeLineObjectIds.Count != 0)
            {
                return pipeLineObjectIds[0];
            }
            return ObjectId.Null;
        }

        /// <summary>
        /// 获取管道元素所在的所有管道线
        /// </summary>
        /// <param name="pipeElementObjectId">管道元素对象ID</param>
        /// <param name="pipeLineObjectIds">管道线对象ID列表</param>
        /// <returns>管道元素所在的所有管道线对象ID列表</returns>
        public static List<ObjectId> GsPgGetPipeLinesByOnPL(ObjectId pipeElementObjectId, List<ObjectId> pipeLineObjectIds)
        {
            pipeLineObjectIds = pipeLineObjectIds.Where(x => IsPipeElementOnPipeLine(UtilsBlock.UtilsGetBlockBasePoint(pipeElementObjectId), x)).ToList();
            return pipeLineObjectIds;
        }

        /// <summary>
        /// 获取管道元素所在的所有双管道块
        /// </summary>
        /// <param name="pipeElementObjectId">管道元素对象ID</param>
        /// <param name="doublePipeLineObjectIds">双管道块对象ID列表</param>
        /// <returns>管道元素所在的所有双管道块对象ID列表</returns>
        public static List<ObjectId> GsPgGetDoublePipeLinesByOnPL(ObjectId pipeElementObjectId, List<ObjectId> doublePipeLineObjectIds)
        {
            doublePipeLineObjectIds = doublePipeLineObjectIds.Where(x => UtilsGeometry.UtilsIsPointWithRectangleBlock(UtilsBlock.UtilsGetBlockBasePoint(pipeElementObjectId), x)).ToList();
            return doublePipeLineObjectIds;
        }

        /// <summary>
        /// 获取管道元素所在的所有管道线端点
        /// </summary>
        /// <param name="pipeElementObjectId">管道元素对象ID</param>
        /// <param name="pipeLineObjectIds">管道线对象ID列表</param>
        /// <returns>管道元素所在的所有管道线端点对象ID列表</returns>
        public static List<ObjectId> GsPgGetPipeLinesByOnPLEnd(ObjectId pipeElementObjectId, List<ObjectId> pipeLineObjectIds)
        {
            pipeLineObjectIds = pipeLineObjectIds.Where(x => IsPipeElementOnPipeLineEnds(UtilsBlock.UtilsGetBlockBasePoint(pipeElementObjectId), x)).ToList();
            return pipeLineObjectIds;
        }

        /// <summary>
        /// 通过相交获取其他管道线
        /// </summary>
        /// <param name="pipeLineObjectId">当前管道线对象ID</param>
        /// <param name="pipeLineObjectIds">管道线对象ID列表</param>
        /// <param name="allElbowObjectIds">所有弯头对象ID列表</param>
        /// <param name="pipeData">管道数据</param>
        /// <returns>相交的其他管道线对象ID列表</returns>
        public static List<ObjectId> GsPgGetOtherPipeLineByInterset(ObjectId pipeLineObjectId, List<ObjectId> pipeLineObjectIds, List<ObjectId> allElbowObjectIds, Dictionary<string, string> pipeData)
        {
            List<ObjectId> otherPipeLineObjectIds = new List<ObjectId>();
            allElbowObjectIds.Where(x => IsPipeElementOnPipeLineEnds(UtilsBlock.UtilsGetBlockBasePoint(x), pipeLineObjectId))
                .ToList()
                .ForEach(x =>
                {
                    UtilsBlock.UtilsSetPropertyValueByDictData(x, pipeData);
                    // The elbow that require synchronization have been incorporated into the global variables
                    _processedPipeElbowObjectIds.Add(x);
                    ObjectId result = GsPgGetPipeLineByOnPLEnd(x, pipeLineObjectIds);
                    // 2024-01-24 如果弯头块在终止图层 0DataFlow-GsPgPipeLineDPSBreak 上，则数据无法通过该弯头传递给其他管道
                    if (result != ObjectId.Null && UtilsBlock.UtilsGetBlockLayer(x) != "0DataFlow-GsPgPipeLineDPSBreak")
                    {
                        otherPipeLineObjectIds.Add(result);
                    }
                });
            return otherPipeLineObjectIds;
        }

        /// <summary>
        /// 修改管道箭头辅助属性值
        /// </summary>
        /// <param name="pipeLineObjectId">管道线对象ID</param>
        /// <param name="allPipeArrowAssistObjectIds">所有管道箭头辅助对象ID列表</param>
        /// <param name="pipeData">管道数据</param>
        public static void GsPgChangePipeArrowAssistPropertyValue(ObjectId pipeLineObjectId, List<ObjectId> allPipeArrowAssistObjectIds, Dictionary<string, string> pipeData)
        {
            allPipeArrowAssistObjectIds.Where(x => IsPipeElementOnPipeLine(UtilsBlock.UtilsGetBlockBasePoint(x), pipeLineObjectId))
                .ToList()
                .ForEach(x =>
                {
                    UtilsBlock.UtilsSetPropertyValueByDictData(x, pipeData);
                    // key logic: the other pipelines elevation should be based on the current auxiliary arrow block elevation
                    UtilsCADActive.UtilsAddOneXData(pipeLineObjectId, "pipeElevation", UtilsBlock.UtilsGetPropertyValueByPropertyName(x, "elevation"));
                });
        }

        /// <summary>
        /// 获取管道直径
        /// </summary>
        /// <param name="pipeNum">管道编号</param>
        /// <param name="pipeInfo">管道信息辅助类</param>
        /// <returns>管道直径</returns>
        public static string GsPgGetPipeDiameter(string pipeNum, PipeInfoHelper pipeInfo)
        {
            string pipeDiameter = pipeInfo.GetPipeDiameter(pipeNum);
            //string pipeDiameter = pipeNum.Split('-').ElementAtOrDefault(1);

            if (pipeDiameter == null || !Regex.IsMatch(pipeDiameter, @"^\d+$"))
            {
                return string.Empty;
            }
            else
            {
                return pipeDiameter;
            }
        }

        /// <summary>
        /// 获取管道弯头直径
        /// </summary>
        /// <param name="pipeDiameter">管道直径</param>
        /// <param name="calMultiple">计算倍数</param>
        /// <returns>管道弯头直径</returns>
        public static string GsPgGetPipeElbowDiameter(string pipeDiameter, double calMultiple)
        {
            double result = UtilsCommon.UtilsStringToDouble(pipeDiameter) * calMultiple;
            return result.ToString();
        }

        /// <summary>
        /// 修改阀门属性值
        /// </summary>
        /// <param name="pipeLineObjectId">管道线对象ID</param>
        /// <param name="allValveObjectIds">所有阀门对象ID列表</param>
        /// <param name="pipeData">管道数据</param>
        /// <param name="pipeInfo">管道信息辅助类</param>
        public static void GsPgChangeValvePropertyValue(ObjectId pipeLineObjectId, List<ObjectId> allValveObjectIds, Dictionary<string, string> pipeData, PipeInfoHelper pipeInfo)
        {
            allValveObjectIds.Where(x => IsPipeElementOnPipeLine(UtilsBlock.UtilsGetBlockBasePoint(x), pipeLineObjectId))
                .ToList()
                .ForEach(x =>
                {

                    string pipeDiater = GsPgGetPipeDiameter(pipeData["pipeNum"], pipeInfo);
                    if (pipeDiater != string.Empty)
                    {
                        Dictionary<string, string> propertyDict = new Dictionary<string, string>()
                        {
                            { "sideview-DN", pipeDiater },
                            { "topview-DN", pipeDiater }
                        };
                        UtilsBlock.UtilsSetDynamicPropertyValueByDictData(x, propertyDict);
                    }

                });
        }

        /// <summary>
        /// 同步一个管道辅助的管道元素
        /// </summary>
        /// <param name="pipeData">管道数据</param>
        /// <param name="pipeLineObjectIds">管道线对象ID列表</param>
        /// <param name="allPipeLineObjectIds">所有管道线对象ID列表</param>
        /// <param name="allElbowObjectIds">所有弯头对象ID列表</param>
        /// <param name="allPipeArrowAssistObjectIds">所有管道箭头辅助对象ID列表</param>
        /// <param name="allValveObjectIds">所有阀门对象ID列表</param>
        /// <param name="pipeInfo">管道信息辅助类</param>
        public static void GsPgSynPipeElementForOnePipeAssist(Dictionary<string, string> pipeData, List<ObjectId> pipeLineObjectIds, List<ObjectId> allPipeLineObjectIds, List<ObjectId> allElbowObjectIds, List<ObjectId> allPipeArrowAssistObjectIds, List<ObjectId> allValveObjectIds, PipeInfoHelper pipeInfo)
        {
            if (pipeLineObjectIds != null)
            {
                pipeLineObjectIds.ForEach(x =>
                {
                    UtilsCADActive.UtilsAddXData(x, pipeData);
                    // for test
                    //UtilsPolyline.UtilsChangeColor(x, 1);
                    GsPgChangePipeArrowAssistPropertyValue(x, allPipeArrowAssistObjectIds, pipeData);
                    GsPgChangeValvePropertyValue(x, allValveObjectIds, pipeData, pipeInfo);
                    // the key logic: remove the current polyline
                    allPipeLineObjectIds = allPipeLineObjectIds.Where(xx => xx != x).ToList();
                    List<ObjectId> otherPipeLineObjectIds = GsPgGetOtherPipeLineByInterset(x, allPipeLineObjectIds, allElbowObjectIds, pipeData);

                    if (otherPipeLineObjectIds != null)
                    {
                        GsPgSynPipeElementForOnePipeAssist(pipeData, otherPipeLineObjectIds, allPipeLineObjectIds, allElbowObjectIds, allPipeArrowAssistObjectIds, allValveObjectIds, pipeInfo);
                    }
                    _processedPolylineIds.Add(x);
                });

            }
        }

        /// <summary>
        /// 同步一个管道辅助的双线管道元素
        /// </summary>
        /// <param name="pipeData">管道数据</param>
        /// <param name="doublePipeLineObjectIds">双线管道对象ID列表</param>
        /// <param name="allDoublePipeLineObjectIds">所有双线管道对象ID列表</param>
        /// <param name="allDoubleElbowObjectIds">所有双线弯头对象ID列表</param>
        /// <param name="allPipeArrowAssistObjectIds">所有管道箭头辅助对象ID列表</param>
        /// <param name="allValveObjectIds">所有阀门对象ID列表</param>
        /// <param name="pipeInfo">管道信息辅助类</param>
        public static void GsPgSynDoublePipeElementForOnePipeAssist(Dictionary<string, string> pipeData, List<ObjectId> doublePipeLineObjectIds, List<ObjectId> allDoublePipeLineObjectIds, List<ObjectId> allDoubleElbowObjectIds, List<ObjectId> allPipeArrowAssistObjectIds, List<ObjectId> allValveObjectIds, PipeInfoHelper pipeInfo)
        {
            if (doublePipeLineObjectIds != null)
            {
                string pipeDiameter = GsPgGetPipeDiameter(pipeData["pipeNum"], pipeInfo);
                
                doublePipeLineObjectIds.ForEach(x =>
                {
                    // 为双线管道添加XData
                    UtilsCADActive.UtilsAddXData(x, pipeData);
                    
                    // 根据管径更新双线管道的状态
                    Dictionary<string, string> statusDict = new Dictionary<string, string>()
                    {
                        { "status", GsPgGetDoubleLineStatusByPipeDiameter(pipeDiameter) }
                    };
                    UtilsBlock.UtilsSetDynamicPropertyValueByDictData(x, statusDict);
                    
                    // 从所有双线管道列表中移除当前处理的管道
                    allDoublePipeLineObjectIds = allDoublePipeLineObjectIds.Where(xx => xx != x).ToList();
                    
                    // 修改双线管道上的箭头辅助块属性
                    GsPgSynchroniseOneDoubleLinePipeArrowAssist(pipeData, x, allPipeArrowAssistObjectIds);
                    
                    // 获取相交的其他双线管道
                    List<ObjectId> otherDoubleLineObjectIds = GsPgGetOtherDoubleLinePipeLineByInterset(x, allDoublePipeLineObjectIds, allDoubleElbowObjectIds, pipeData);
                    
                    // 递归处理相交的其他双线管道
                    if (otherDoubleLineObjectIds != null && otherDoubleLineObjectIds.Count > 0)
                    {
                        GsPgSynDoublePipeElementForOnePipeAssist(pipeData, otherDoubleLineObjectIds, allDoublePipeLineObjectIds, allDoubleElbowObjectIds, allPipeArrowAssistObjectIds, allValveObjectIds, pipeInfo);
                    }
                    _processedDoublePipeLineIds.Add(x);
                });
            }
        }

        /// <summary>
        /// 根据管道直径获取双线管道状态
        /// </summary>
        /// <param name="diameter">管道直径</param>
        /// <returns>双线管道状态</returns>
        private static string GsPgGetDoubleLineStatusByPipeDiameter(string diameter)
        {
            switch (diameter)
            {
                case "250": return "DN250";
                case "300": return "DN300";
                case "350": return "DN350";
                case "400": return "DN400";
                case "450": return "DN450";
                case "500": return "DN500";
                case "550": return "DN550";
                case "600": return "DN600";
                default: return "DN250";
            }
        }

        /// <summary>
        /// 修改双线管道上的箭头辅助块属性
        /// </summary>
        /// <param name="pipeData">管道数据</param>
        /// <param name="doublePipeLineObjectId">双线管道对象ID</param>
        /// <param name="allPipeArrowAssistObjectIds">所有管道箭头辅助对象ID列表</param>
        private static void GsPgSynchroniseOneDoubleLinePipeArrowAssist(Dictionary<string, string> pipeData, ObjectId doublePipeLineObjectId, List<ObjectId> allPipeArrowAssistObjectIds)
        {
            // 过滤掉管道数据中的elevation属性
            Dictionary<string, string> filteredPipeData = pipeData.Where(x => x.Key != "elevation").ToDictionary(x => x.Key, x => x.Value);

            // 获取双线管道的两个端点
            List<Point3d> doubleLinePipePtList = GetDoubleLinePipePtList(doublePipeLineObjectId);
            
            // 找出位于双线管道上的所有箭头辅助块
            List<ObjectId> pipeArrowAssistObjectIds = allPipeArrowAssistObjectIds
                .Where(x => UtilsGeometry.UtilsIsPointInLineByDistance(UtilsBlock.UtilsGetBlockBasePoint(x), doubleLinePipePtList[0], doubleLinePipePtList[1], 5))
                .ToList();
            
            if (pipeArrowAssistObjectIds.Count > 0)
            {
                // 为每个箭头辅助块更新属性
                pipeArrowAssistObjectIds.ForEach(x =>
                {
                    // 更新箭头辅助块的属性
                    UtilsBlock.UtilsSetPropertyValueByDictData(x, filteredPipeData);
                    
                    // 获取箭头辅助块的elevation属性值并更新双线管道的elevation属性
                    string elevation = UtilsBlock.UtilsGetPropertyValueByPropertyName(x, "elevation");
                    Dictionary<string, string> elevationDict = new Dictionary<string, string>() { { "elevation", elevation } };
                    UtilsBlock.UtilsSetPropertyValueByDictData(doublePipeLineObjectId, elevationDict);
                });
            }
        }

        /// <summary>
        /// 获取双线管道的两个端点
        /// </summary>
        /// <param name="entityId">双线管道对象ID</param>
        /// <returns>端点列表</returns>
        private static List<Point3d> GetDoubleLinePipePtList(ObjectId entityId)
        {
            Point3d firPt = UtilsBlock.UtilsGetBlockBasePoint(entityId);
            
            // 获取动态块属性集合
            BlockReference blockRef = entityId.GetObject(OpenMode.ForRead) as BlockReference;
            DynamicBlockReferencePropertyCollection props = blockRef.DynamicBlockReferencePropertyCollection;
            
            // 找到"length"属性并获取其值
            double pipeLength = 0;
            foreach (DynamicBlockReferenceProperty prop in props)
            {
                if (string.Equals(prop.PropertyName, "length", StringComparison.OrdinalIgnoreCase))
                {
                    pipeLength = Convert.ToDouble(prop.Value);
                    break;
                }
            }
            
            double pipeAngle = UtilsBlock.UtilsGetBlockRotaton(entityId);
            
            Point3d senPt = new Point3d(
                firPt.X + pipeLength * Math.Cos(pipeAngle),
                firPt.Y + pipeLength * Math.Sin(pipeAngle),
                firPt.Z
            );
            
            return new List<Point3d> { firPt, senPt };
        }

        /// <summary>
        /// 获取相交的其他双线管道
        /// </summary>
        /// <param name="doublePipeLineObjectId">当前双线管道对象ID</param>
        /// <param name="allDoublePipeLineObjectIds">所有双线管道对象ID列表</param>
        /// <param name="allDoubleElbowObjectIds">所有双线弯头对象ID列表</param>
        /// <param name="pipeData">管道数据</param>
        /// <returns>相交的其他双线管道对象ID列表</returns>
        private static List<ObjectId> GsPgGetOtherDoubleLinePipeLineByInterset(ObjectId doublePipeLineObjectId, List<ObjectId> allDoublePipeLineObjectIds, List<ObjectId> allDoubleElbowObjectIds, Dictionary<string, string> pipeData)
        {
            List<ObjectId> resultList = new List<ObjectId>();
            List<Point3d> twoEndPtList = GetDoubleLinePipePtList(doublePipeLineObjectId);
            
            // 找出位于双线管道端点附近的弯头
            List<ObjectId> doubleLinePipeElementElbowObjectIds = allDoubleElbowObjectIds
                .Where(x => {
                    Point3d elbowPosition = UtilsBlock.UtilsGetBlockBasePoint(x);
                    return UtilsGeometry.UtilsIsPointNearPoint(elbowPosition, twoEndPtList[0], 10) || 
                           UtilsGeometry.UtilsIsPointNearPoint(elbowPosition, twoEndPtList[1], 10);
                })
                .ToList();
            
            // 筛选掉位于禁止数据传递图层上的弯头
            doubleLinePipeElementElbowObjectIds = doubleLinePipeElementElbowObjectIds
                .Where(x => UtilsBlock.UtilsGetBlockLayer(x) != "0DataFlow-GsPgPipeLineDPSBreak")
                .ToList();
            
            // 为每个弯头设置属性
            doubleLinePipeElementElbowObjectIds.ForEach(x => {
                UtilsBlock.UtilsSetPropertyValueByDictData(x, pipeData);
                // The elbow that require synchronization have been incorporated into the global variables
                _processedDoublePipeElbowObjectIds.Add(x);
                
                // 找出与弯头相交的其他双线管道
                List<ObjectId> intersectDoublePipeLines = allDoublePipeLineObjectIds
                    .Where(xx => {
                        Point3d elbowPosition = UtilsBlock.UtilsGetBlockBasePoint(x);
                        List<Point3d> doublePipePtList = GetDoubleLinePipePtList(xx);
                        return UtilsGeometry.UtilsIsPointNearPoint(elbowPosition, doublePipePtList[0], 10) || 
                               UtilsGeometry.UtilsIsPointNearPoint(elbowPosition, doublePipePtList[1], 10);
                    })
                    .ToList();
                
                resultList.AddRange(intersectDoublePipeLines);
            });
            
            return resultList;
        }

        /// <summary>
        /// 根据点位置获取斜角旋转角度
        /// </summary>
        /// <param name="xDiff">X差值</param>
        /// <param name="yDiff">Y差值</param>
        /// <returns>旋转角度</returns>
        private static int GetObliqueRotationBasedOnPointPosition(double xDiff, double yDiff)
        {
            if (xDiff > 0 && yDiff > 0) return 45;
            if (xDiff < 0 && yDiff < 0) return 225;
            if (xDiff < 0 && yDiff > 0) return 90;
            return 275;
        }

        /// <summary>
        /// 根据点位置获取旋转角度
        /// </summary>
        /// <param name="xDiff">X差值</param>
        /// <param name="yDiff">Y差值</param>
        /// <returns>旋转角度</returns>
        private static int GetRotationBasedOnPointPosition(double xDiff, double yDiff)
        {
            if (xDiff > 0 && yDiff > 0) return 0;
            if (xDiff < 0 && yDiff < 0) return 180;
            if (xDiff < 0 && yDiff > 0) return 90;
            return 270;
        }

        /// <summary>
        /// 获取交叉点
        /// </summary>
        /// <param name="elbowObjectId">弯头对象ID</param>
        /// <param name="pipeLineObjectId1">管道线1对象ID</param>
        /// <param name="pipeLineObjectId2">管道线2对象ID</param>
        /// <returns>两个交叉点</returns>
        private static (Point3d, Point3d) GetCrossPoints(ObjectId elbowObjectId, ObjectId pipeLineObjectId1, ObjectId pipeLineObjectId2)
        {
            List<Point3d> firstPoint3ds = UtilsGeometry.UtilsGetIntersectionPointsByBlockAndPolyLine(elbowObjectId, pipeLineObjectId1);
            var firstCrossPoint = firstPoint3ds != null ? firstPoint3ds.FirstOrDefault() : Point3d.Origin;
            List<Point3d> secondPoint3ds = UtilsGeometry.UtilsGetIntersectionPointsByBlockAndPolyLine(elbowObjectId, pipeLineObjectId2);
            var secondCrossPoint = secondPoint3ds != null ? secondPoint3ds.FirstOrDefault() : Point3d.Origin;

            return (firstCrossPoint, secondCrossPoint);
        }

        /// <summary>
        /// 获取水平和垂直点
        /// </summary>
        /// <param name="basePoint">基准点</param>
        /// <param name="firstCrossPoint">第一个交叉点</param>
        /// <param name="secondCrossPoint">第二个交叉点</param>
        /// <returns>水平点和垂直点</returns>
        private static (Point3d, Point3d) GetHorizontalAndVerticalPoints(Point3d basePoint, Point3d firstCrossPoint, Point3d secondCrossPoint)
        {
            return UtilsGeometry.UtilsIsLineHorizontal(basePoint, firstCrossPoint, 5.0)
                ? (firstCrossPoint, secondCrossPoint)
                : (secondCrossPoint, firstCrossPoint);
        }

        /// <summary>
        /// 根据弯头类型设置旋转角度
        /// </summary>
        /// <param name="elbowObjectId">弯头对象ID</param>
        /// <param name="elbowType">弯头类型</param>
        /// <param name="xDiff">X差值</param>
        /// <param name="yDiff">Y差值</param>
        private static void SetRotationBasedOnElbowType(ObjectId elbowObjectId, string elbowType, double xDiff, double yDiff)
        {
            if (elbowType == "elbow90")
            {
                int rotation = GetRotationBasedOnPointPosition(xDiff, yDiff);
                UtilsBlock.UtilsSetBlockRotatonInDegrees(elbowObjectId, rotation);
            }
            else if (elbowType == "elbow45")
            {
                int rotation = GetObliqueRotationBasedOnPointPosition(xDiff, yDiff);
                UtilsBlock.UtilsSetBlockRotatonInDegrees(elbowObjectId, rotation);
            }
        }

        /// <summary>
        /// 同步弯头旋转角度
        /// </summary>
        /// <param name="elbowObjectId">弯头对象ID</param>
        /// <param name="pipeLineObjectId1">管道线1对象ID</param>
        /// <param name="pipeLineObjectId2">管道线2对象ID</param>
        /// <param name="elbowType">弯头类型</param>
        public static void GsPgSynElbowRotation(ObjectId elbowObjectId, ObjectId pipeLineObjectId1, ObjectId pipeLineObjectId2, string elbowType)
        {
            var basePoint = UtilsBlock.UtilsGetBlockBasePoint(elbowObjectId);
            var (firstCrossPoint, secondCrossPoint) = GetCrossPoints(elbowObjectId, pipeLineObjectId1, pipeLineObjectId2);

            if (firstCrossPoint != Point3d.Origin && secondCrossPoint != Point3d.Origin)
            {
                var (horizontalPoint, verticalPoint) = GetHorizontalAndVerticalPoints(basePoint, firstCrossPoint, secondCrossPoint);
                var xDiff = horizontalPoint.X - basePoint.X;
                var yDiff = verticalPoint.Y - basePoint.Y;

                SetRotationBasedOnElbowType(elbowObjectId, elbowType, xDiff, yDiff);
            }
        }

        /// <summary>
        /// 获取管道高程和交叉点
        /// </summary>
        /// <param name="elbowObjectId">弯头对象ID</param>
        /// <param name="pipeLineObjectIds">管道线对象ID列表</param>
        /// <returns>第一管道高程、第二管道高程、第一交叉点列表、第二交叉点列表</returns>
        private static (double, double, List<Point3d>, List<Point3d>) GetPipeElevationAndIntersectionPoints(ObjectId elbowObjectId, List<ObjectId> pipeLineObjectIds)
        {
            double firstPipeElevation = UtilsCommon.UtilsStringToDouble(UtilsCADActive.UtilsGetXData(pipeLineObjectIds[0], "pipeElevation"));
            double secondPipeElevation = UtilsCommon.UtilsStringToDouble(UtilsCADActive.UtilsGetXData(pipeLineObjectIds[1], "pipeElevation"));
            List<Point3d> firstIntersectionPoints = UtilsGeometry.UtilsGetIntersectionPointsByBlockAndPolyLine(elbowObjectId, pipeLineObjectIds[0]);
            List<Point3d> secondIntersectionPoints = UtilsGeometry.UtilsGetIntersectionPointsByBlockAndPolyLine(elbowObjectId, pipeLineObjectIds[1]);
            return (firstPipeElevation, secondPipeElevation, firstIntersectionPoints, secondIntersectionPoints);
        }

        /// <summary>
        /// 处理不同角度的弯头
        /// </summary>
        /// <param name="elbowObjectId">弯头对象ID</param>
        /// <param name="pipeDiater">管道直径</param>
        /// <param name="pipeLineObjectIds">管道线对象ID列表</param>
        /// <param name="intersectionAngle">交叉角度</param>
        private static void HandleElbowWithDifferentAngles(ObjectId elbowObjectId, string pipeDiater, List<ObjectId> pipeLineObjectIds, double intersectionAngle)
        {
            if (UtilsCommon.UtilsIsTwoNumEqual(intersectionAngle, 90, 2) || UtilsCommon.UtilsIsTwoNumEqual(intersectionAngle, 270, 2))
            {
                UtilsBlock.UtilsSetDynamicPropertyValueByDictData(elbowObjectId, new Dictionary<string, string>() { { "status", "elbow90" } });
                if (pipeDiater != string.Empty)
                {
                    UtilsBlock.UtilsSetDynamicPropertyValueByDictData(elbowObjectId, new Dictionary<string, string>() { { "radius90", GsPgGetPipeElbowDiameter(pipeDiater, 1.5) } }); 
                }
                GsPgSynElbowRotation(elbowObjectId, pipeLineObjectIds[0], pipeLineObjectIds[1], "elbow90");
            }
            else if (UtilsCommon.UtilsIsTwoNumEqual(intersectionAngle, 135, 2) || UtilsCommon.UtilsIsTwoNumEqual(intersectionAngle, 225, 2))
            {
                UtilsBlock.UtilsSetDynamicPropertyValueByDictData(elbowObjectId, new Dictionary<string, string>() { { "status", "elbow45" } });
                if (pipeDiater != string.Empty)
                {
                    UtilsBlock.UtilsSetDynamicPropertyValueByDictData(elbowObjectId, new Dictionary<string, string>() { { "radius90", GsPgGetPipeElbowDiameter(pipeDiater, 0.633) } });
                }
                GsPgSynElbowRotation(elbowObjectId, pipeLineObjectIds[0], pipeLineObjectIds[1], "elbow45");
            }
        }

        /// <summary>
        /// 处理不同高程的弯头
        /// </summary>
        /// <param name="elbowObjectId">弯头对象ID</param>
        /// <param name="firstPipeElevation">第一管道高程</param>
        /// <param name="secondPipeElevation">第二管道高程</param>
        /// <param name="pipeLineObjectIds">管道线对象ID列表</param>
        private static void HandleElbowWithDifferentElevation(ObjectId elbowObjectId, double firstPipeElevation, double secondPipeElevation, List<ObjectId> pipeLineObjectIds)
        {
            UtilsBlock.UtilsSetDynamicPropertyValueByDictData(elbowObjectId, new Dictionary<string, string>() { { "status", "elbowdown" } });
            if (firstPipeElevation > secondPipeElevation)
            {
                SetBlockRotationByIntersectionPoint(elbowObjectId, pipeLineObjectIds[0]);
            }
            else
            {
                SetBlockRotationByIntersectionPoint(elbowObjectId, pipeLineObjectIds[1]);
            }
        }

        /// <summary>
        /// 处理不同高程的三通
        /// </summary>
        /// <param name="elbowObjectId">弯头对象ID</param>
        /// <param name="firstPipeElevation">第一管道高程</param>
        /// <param name="secondPipeElevation">第二管道高程</param>
        /// <param name="pipeLineObjectIds">管道线对象ID列表</param>
        /// <param name="firstIntersectionPointsNum">第一交叉点数量</param>
        private static void HandleTeeWithDifferentElevation(ObjectId elbowObjectId, double firstPipeElevation, double secondPipeElevation, List<ObjectId> pipeLineObjectIds, int firstIntersectionPointsNum)
        {
            if (firstPipeElevation > secondPipeElevation && firstIntersectionPointsNum == 2)
            {
                UtilsBlock.UtilsSetDynamicPropertyValueByDictData(elbowObjectId, new Dictionary<string, string>() { { "status", "teeup" } });
                SetBlockRotationByIntersectionPoint(elbowObjectId, pipeLineObjectIds[0]);
            }
            else if (firstPipeElevation < secondPipeElevation && firstIntersectionPointsNum == 2)
            {
                UtilsBlock.UtilsSetDynamicPropertyValueByDictData(elbowObjectId, new Dictionary<string, string>() { { "status", "teedown" } });
                SetBlockRotationByIntersectionPoint(elbowObjectId, pipeLineObjectIds[1]);
            }
            else if (firstPipeElevation < secondPipeElevation && firstIntersectionPointsNum == 1)
            {
                UtilsBlock.UtilsSetDynamicPropertyValueByDictData(elbowObjectId, new Dictionary<string, string>() { { "status", "teeup" } });
                SetBlockRotationByIntersectionPoint(elbowObjectId, pipeLineObjectIds[1]);
            }
            else if (firstPipeElevation > secondPipeElevation && firstIntersectionPointsNum == 1)
            {
                UtilsBlock.UtilsSetDynamicPropertyValueByDictData(elbowObjectId, new Dictionary<string, string>() { { "status", "teedown" } });
                SetBlockRotationByIntersectionPoint(elbowObjectId, pipeLineObjectIds[0]);
            }
        }

        /// <summary>
        /// 根据交叉点设置块旋转角度
        /// </summary>
        /// <param name="elbowObjectId">弯头对象ID</param>
        /// <param name="pipeLineObjectId">管道线对象ID</param>
        private static void SetBlockRotationByIntersectionPoint(ObjectId elbowObjectId, ObjectId pipeLineObjectId)
        {
            List<Point3d> crossPoints = UtilsGeometry.UtilsGetIntersectionPointsByBlockAndPolyLine(elbowObjectId, pipeLineObjectId);
            if (crossPoints.Count > 0)
            {
                Point3d basePoint = UtilsBlock.UtilsGetBlockBasePoint(elbowObjectId);
                Point3d crossPoint = crossPoints[0];
                UtilsBlock.UtilsSetBlockRotatonInDegrees(elbowObjectId, UtilsGeometry.UtilsGetAngleByTwoPoint(basePoint, crossPoint));
            }
        }

        /// <summary>
        /// 同步管道弯头状态
        /// </summary>
        /// <param name="pipeData">管道数据字典</param>
        /// <param name="pipeInfo">管道信息辅助类</param>
        /// <remarks>
        /// 该方法处理所有已处理的管道弯头，根据其位置和相交的管道线设置其状态和旋转角度。
        public static void GsPgSynPipeElbowStatus(Dictionary<string, string> pipeData, PipeInfoHelper pipeInfo)
        {
            string pipeDiater = GsPgGetPipeDiameter(pipeData["pipeNum"], pipeInfo);
            _processedPipeElbowObjectIds = _processedPipeElbowObjectIds.Distinct().ToList();
            _processedPolylineIds = _processedPolylineIds.Distinct().ToList();

            _processedPipeElbowObjectIds.ForEach(x =>
            {
                // 2024-01-24 设计人员可能会镜像弯头块，之前仅仅在HandleElbowWithDifferentAngles中重置XY比例，现在在这里整体重置
                UtilsBlock.UtilsSetBlockXYScale(x, 1, 1);
                List<ObjectId> pipeLineObjectIds = _processedPolylineIds.Where(xx => IsPipeElementOnPipeLineEnds(UtilsBlock.UtilsGetBlockBasePoint(x), xx)).ToList();
                List<ObjectId> teePipeLineObjectIds = _processedPolylineIds.Where(xx => IsPipeElementOnPipeLine(UtilsBlock.UtilsGetBlockBasePoint(x), xx)).ToList();
                if (pipeLineObjectIds.Count() == 2)
                {
                    var (firstPipeElevation, secondPipeElevation, firstIntersectionPoints, secondIntersectionPoints) = GetPipeElevationAndIntersectionPoints(x, pipeLineObjectIds);
                    if (firstIntersectionPoints != null && secondIntersectionPoints != null)
                    {
                        Point3d basePoint = UtilsBlock.UtilsGetBlockBasePoint(x);
                        Point3d firstIntersectionPoint = firstIntersectionPoints[0];
                        Point3d secondIntersectionPoint = secondIntersectionPoints[0];

                        double intersectionAngle = UtilsGeometry.UtilsGetAngleByThreePoint(basePoint, firstIntersectionPoint, secondIntersectionPoint);
                        if (firstPipeElevation == secondPipeElevation)
                        {
                            HandleElbowWithDifferentAngles(x, pipeDiater, pipeLineObjectIds, intersectionAngle);
                        }
                        else
                        {
                            HandleElbowWithDifferentElevation(x, firstPipeElevation, secondPipeElevation, pipeLineObjectIds);
                        }
                    }
                }
                else if (teePipeLineObjectIds.Count() == 2)
                {
                    var (firstPipeElevation, secondPipeElevation, firstIntersectionPoints, secondIntersectionPoints) = GetPipeElevationAndIntersectionPoints(x, teePipeLineObjectIds);
                    if (firstIntersectionPoints != null && secondIntersectionPoints != null)
                    {
                        Point3d basePoint = UtilsBlock.UtilsGetBlockBasePoint(x);
                        Point3d firstIntersectionPoint = firstIntersectionPoints[0];
                        Point3d secondIntersectionPoint = secondIntersectionPoints[0];

                        if (firstIntersectionPoints.Count + secondIntersectionPoints.Count == 3)
                        {
                            HandleTeeWithDifferentElevation(x, firstPipeElevation, secondPipeElevation, teePipeLineObjectIds, firstIntersectionPoints.Count);
                        }

                    }
                }
            });
        }

        /// <summary>
        /// 同步双线管道弯头状态
        /// </summary>
        /// <param name="pipeData">管道数据字典</param>
        /// <param name="pipeInfo">管道信息辅助类</param>
        public static void GsPgSynDoublePipeElbowStatus(Dictionary<string, string> pipeData, PipeInfoHelper pipeInfo)
        {
            string pipeDiameter = GsPgGetPipeDiameter(pipeData["pipeNum"], pipeInfo);
            _processedDoublePipeElbowObjectIds = _processedDoublePipeElbowObjectIds.Distinct().ToList();
            _processedDoublePipeLineIds = _processedDoublePipeLineIds.Distinct().ToList();

            // to to

        }

        /// <summary>
        /// 获取项目编号
        /// </summary>
        /// <param name="allGeYuanDrawObjectIds">所有的GeYuan绘图对象ID列表</param>
        /// <returns>项目编号</returns>
        public static string GsPgGetProjectNum(List<ObjectId> allGeYuanDrawObjectIds)
        {
            string projectNum = string.Empty;
            allGeYuanDrawObjectIds.ForEach(x =>
            {
                string result = UtilsCommon.UtilsGetNewTitleBlockInfoJObject(x).UtilsGetStrValue("projectnum");
                if (result != string.Empty)
                {
                    projectNum = result;
                }
            });
            return projectNum;
        }

        /// <summary>
        /// 批量同步管道数据
        /// </summary>
        public static void GsPgBatchSynPipeData()
        {
            using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {
                // 清空全局变量
                _processedPipeElbowObjectIds.Clear();
                _processedPolylineIds.Clear();
                _processedDoublePipeLineIds.Clear();
                _processedDoublePipeElbowObjectIds.Clear();

                UtilsCADActive.Editor.WriteMessage("\n请选择绿色箭头辅助管道块");
                List<ObjectId> pipeNumObjectIds = UtilsBlock.UtilsGetObjectIdsBySelectByBlockName("GsPgPipeElementArrowAssist").ToList();

                List<ObjectId> allBlockIds = UtilsBlock.UtilsGetAllBlockObjectIds();
                List<ObjectId> allPolylineObjectIds = UtilsPolyline.UtilsGetAllObjectIdsByLayerName("0DataFlow-GsPgPipeLine*");

                List<string> blockNameList = new List<string> { "GeYuanFrame", "GsPgPipeElementElbow", "GsPgPipeElementArrowAssist", "GsPgValve", "GsPgPipeElementDoublePipeLine", "GsPgPipeElementElbowDoubleLine" };
                Dictionary<string, List<ObjectId>> allObjectIdsGroups = UtilsBlock.UtilsGetAllObjectIdsGroupsByBlockNameList(allBlockIds, blockNameList, false);
                List<ObjectId> allGeYuanDrawObjectIds = allObjectIdsGroups["GeYuanFrame"];
                List<ObjectId> allPipeElbowObjectIds = allObjectIdsGroups["GsPgPipeElementElbow"];
                List<ObjectId> allPipeArrowAssistObjectIds = allObjectIdsGroups["GsPgPipeElementArrowAssist"];
                List<ObjectId> allValveObjectIds = allObjectIdsGroups["GsPgValve"];
                // 2025-04-10 新增双管道块和双管道弯头块
                List<ObjectId> allDoublePipeLineObjectIds = allObjectIdsGroups["GsPgPipeElementDoublePipeLine"];
                List<ObjectId> allDoublePipeElbowObjectIds = allObjectIdsGroups["GsPgPipeElementElbowDoubleLine"];

                string projectNum = GsPgGetProjectNum(allGeYuanDrawObjectIds);
                PipeInfoHelper pipeInfo = UtilsCommon.UtilsGetPipeInfo(projectNum);

                pipeNumObjectIds.ForEach(x =>
                {
                    List<ObjectId> pipeLineObjectIds = GsPgGetPipeLinesByOnPL(x, allPolylineObjectIds);
                    List<ObjectId> doublePipeLineObjectIds = GsPgGetDoublePipeLinesByOnPL(x, allDoublePipeLineObjectIds);
                    Dictionary<string, string> pipeData = GsPgGetPipeData(x);
                    GsPgSynPipeElementForOnePipeAssist(pipeData, pipeLineObjectIds, allPolylineObjectIds, allPipeElbowObjectIds, allPipeArrowAssistObjectIds, allValveObjectIds, pipeInfo);
                    GsPgSynDoublePipeElementForOnePipeAssist(pipeData, doublePipeLineObjectIds, allDoublePipeLineObjectIds, allDoublePipeElbowObjectIds, allPipeArrowAssistObjectIds, allValveObjectIds, pipeInfo);
                    GsPgSynPipeElbowStatus(pipeData, pipeInfo);
                    // GsPgSynDoublePipeElbowStatus(pipeData, pipeInfo);
                    UtilsCADActive.Editor.WriteMessage("\n" + pipeData["pipeNum"] + "数据已同步...");
                });


                UtilsCADActive.Editor.WriteMessage("\n同步数据完成...");

                tr.Commit();
            }
        }

    }
} 