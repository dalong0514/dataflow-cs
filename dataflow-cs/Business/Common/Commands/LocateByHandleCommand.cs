using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using dataflow_cs.Business.PipeFlow.Views;
using dataflow_cs.Core.Services;
using dataflow_cs.Utils.CADUtils;
using dataflow_cs.Utils.Helpers;
using System;
using System.Windows;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;

namespace dataflow_cs.Business.Common.Commands
{
    /// <summary>
    /// 测试命令
    /// </summary>
    public class DLLocateByHandleCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLLocateByHandle";

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
                // 显示测试信息
                editor.WriteMessage("\n开始执行命令...");
                
                // 提示用户输入实体句柄
                PromptStringOptions pStrOpts = new PromptStringOptions("\n请输入实体句柄(例如:27E2BF): ");
                pStrOpts.AllowSpaces = false;
                PromptResult pStrRes = editor.GetString(pStrOpts);
                
                if (pStrRes.Status == PromptStatus.OK)
                {
                    string handle = pStrRes.StringResult;
                    return LocateEntityByHandle(handle);
                }

                return false;
            }
            catch (Exception ex)
            {
                editor.WriteMessage($"\n执行命令时发生错误: {ex.Message}");
                return false;
            }
        }
        
        
        /// <summary>
        /// 通过实体句柄定位到实体对象
        /// </summary>
        /// <param name="entityHandle">实体句柄字符串，如"27E2BF"</param>
        /// <returns>是否成功定位</returns>
        protected bool LocateEntityByHandle(string entityHandle)
        {
            try
            {
                // 获取编辑器和文档引用
                Editor editor = UtilsCADActive.Editor;
                Document doc = UtilsCADActive.Document;
                
                // 显示正在定位的句柄
                editor.WriteMessage($"\n正在定位实体，句柄为: {entityHandle}");
                
                // 将句柄字符串转换为Handle对象
                Handle handle = new Handle(Convert.ToInt64(entityHandle, 16));
                
                // 使用Database的TryGetObjectId方法获取ObjectId
                if (!doc.Database.TryGetObjectId(handle, out ObjectId id))
                {
                    editor.WriteMessage($"\n找不到句柄为 {entityHandle} 的实体对象");
                    return false;
                }
                
                if (!id.IsValid)
                {
                    editor.WriteMessage($"\n句柄为 {entityHandle} 的实体ID无效");
                    return false;
                }
                
                // 使用事务获取实体对象
                using (Transaction trans = doc.Database.TransactionManager.StartTransaction())
                {
                    DBObject dbObject = trans.GetObject(id, OpenMode.ForRead);
                    if (!(dbObject is Entity entity))
                    {
                        editor.WriteMessage($"\n句柄为 {entityHandle} 的对象不是实体");
                        return false;
                    }
                    
                    // 获取实体的几何范围
                    Extents3d extents = entity.GeometricExtents;

                    // 使用UtilsZoom中的ZoomObjects方法进行缩放
                    editor.ZoomObjects(new ObjectId[] { id });

                    // 高亮显示该实体
                    editor.SetImpliedSelection(new ObjectId[] { id });
                    
                    editor.WriteMessage($"\n已定位到句柄为 {entityHandle} 的实体");
                    trans.Commit();
                    return true;
                }
            }
            catch (Exception ex)
            {
                UtilsCADActive.Editor.WriteMessage($"\n定位实体时发生错误: {ex.Message}");
                return false;
            }
        }
    }
} 