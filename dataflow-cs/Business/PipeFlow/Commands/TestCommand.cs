using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using dataflow_cs.Core.Services;
using dataflow_cs.Utils.CADUtils;
using dataflow_cs.Utils.Helpers;
using System;

namespace dataflow_cs.Business.PipeFlow.Commands
{
    /// <summary>
    /// 测试命令类
    /// </summary>
    public class TestCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "CsTest";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            try
            {
                editor.WriteMessage("\n开始执行测试命令...");
                
                // 调用原有的测试方法
                CsTest();
                
                editor.WriteMessage("\n测试命令执行完成。");
                return true;
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "执行测试命令时发生错误");
                return false;
            }
        }

        public static void CsTest()
        {
            using (var tr = UtilsCADActive.Database.TransactionManager.StartTransaction())
            {

                // 通过拾取获得一个块的ObjectId
                ObjectId blockId = UtilsCADActive.Editor.GetEntity("\n请选择一个块").ObjectId;
                //UtilsBlock.UtilsSetBlockXYScale(blockId, 1, 1);
                UtilsCADActive.Editor.WriteMessage("\n" + UtilsBlock.UtilsGetBlockLayer(blockId));
                //UtilsBlock.UtilsSetBlockRotatonInDegrees(blockId, 180.0);
                //UtilsCADActive.Editor.WriteMessage("\n" + UtilsCommon.UtilsGetNewTitleBlockInfoJObject(blockId)["projectnum"].ToString());
                //UtilsCADActive.Editor.WriteMessage("\n" + UtilsCommon.UtilsGetNewTitleBlockInfoJObject(blockId).UtilsGetStrValue("projectnum"));

                // 通过拾取获得一个多段线的ObjectId
                //Point3d point1 = UtilsCADActive.GetPointFromUser();
                //ObjectId polylineId = UtilsCADActive.Editor.GetEntity("\n请选择一个多段线").ObjectId;
                //ObjectId polylineId2 = UtilsCADActive.Editor.GetEntity("\n请选择一个多段线").ObjectId;

                //double firstPipeElevation = UtilsCommon.UtilsStringToDouble(UtilsCADActive.UtilsGetXData(polylineId, "pipeElevation"));
                //if (firstPipeElevation == 2.2)
                //{
                //    ed.WriteMessage("\n" + "good");
                //}
                //ed.WriteMessage("\n" + firstPipeElevation);

                // 完成任务：通过拾取获得一个Point3d
                //Point3d point1 = UtilsCADActive.GetPointFromUser();
                //Point3d point2 = UtilsCADActive.GetPointFromUser();
                //Polyline polyline = polylineId.GetObject(OpenMode.ForRead) as Polyline;
                //Polyline polyline2 = polylineId2.GetObject(OpenMode.ForRead) as Polyline;
                //List<Point3d> pts = UtilsGeometry.UtilsIntersectWith(polyline, polyline2, Intersect.OnBothOperands);

                //List<Point3d> intersectionPoints = UtilsGeometry.UtilsGetIntersectionPointsByBlockAndPolyLineNew(blockId, polylineId);

                //UtilsCADActive.Editor.WriteMessage("\n" + UtilsCommon.UtilsGetPipeInfo("S22XXX").GetPipeDiameter("0209-PL-1101-50-2J1-H5"));

                //PipeInfoHelper pipeInfo = UtilsCommon.UtilsGetPipeInfo("S22A03");
                //UtilsCADActive.Editor.WriteMessage("\n" + pipeInfo);
                //UtilsCADActive.Editor.WriteMessage("\n" + pipeInfo.GetPipeDiameter("PW030002-50-1M1-80"));
                //UtilsCADActive.Editor.WriteMessage("\n" + pipeInfo.GetPipeDiameter("0209-PL-1101-65-2J1-H5"));

                UtilsCADActive.Editor.WriteMessage("\n测试完成...");
                tr.Commit();
            }

        }
    }
} 