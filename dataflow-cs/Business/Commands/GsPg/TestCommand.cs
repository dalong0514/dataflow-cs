using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using dataflow_cs.Core.Services;
using dataflow_cs.Utils.CADUtils;
using dataflow_cs.Utils.Helpers;
using System;
using System.Windows;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace dataflow_cs.Business.Commands.GsPg
{
    /// <summary>
    /// 测试命令
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
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            // 显示测试信息
            editor.WriteMessage("\n开始执行测试命令...");

            // 测试UtilsGetAllBlockSelectionSetByCrossingWindow
            TestUtilsGetAllBlockSelectionSetByCrossingWindow(editor, database);
            return true;

        }
        
        protected bool TestUtilsGetAllBlockSelectionSetByCrossingWindow(Editor editor, Database database)
        {
            try
            {
                // 提示用户框选区域
                editor.WriteMessage("\n请框选一个区域...");
                
                // 在AutoCAD中框选范围创建一个Extents3d extents
                PromptSelectionOptions selOpts = new PromptSelectionOptions();
                selOpts.MessageForAdding = "请选择区域内的对象: ";
                selOpts.AllowDuplicates = false;
                
                PromptSelectionResult selResult = editor.GetSelection(selOpts);
                if (selResult.Status != PromptStatus.OK)
                {
                    editor.WriteMessage("\n用户取消了选择操作。");
                    return false;
                }
                
                // 获取选择集的范围
                Extents3d extents = new Extents3d();
                using (Transaction tr = database.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId id in selResult.Value.GetObjectIds())
                    {
                        Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                        if (ent != null)
                        {
                            extents.AddExtents(ent.GeometricExtents);
                        }
                    }
                    tr.Commit();
                }
                
                editor.WriteMessage($"\n已选择区域，范围: ({extents.MinPoint.X:F2},{extents.MinPoint.Y:F2}) 到 ({extents.MaxPoint.X:F2},{extents.MaxPoint.Y:F2})");

                try
                {
                    List<ObjectId> objectIds = new List<ObjectId>();
                    SelectionSet selSet = UtilsSelectionSet.UtilsGetAllBlockSelectionSetByCrossingWindow(extents);
                    objectIds = selSet.GetObjectIds().ToList();
                    // objectIds = UtilsBlock.UtilsGetAllObjectIdsByBlockNameByCrossingWindow(extents, "InstrumentP", true);
                    editor.WriteMessage($"\n找到 {objectIds.Count} 个块，块名称为: {string.Join(", ", objectIds.Select(id => UtilsBlock.UtilsGetBlockName(id)))}");
                    // editor.WriteMessage($"\n找到 {objectIds.Count} 个块");   
                
                    // 检查是否找到了块
                    if (objectIds.Count > 0)
                    {
                        editor.WriteMessage("\n开始更改块的图层...");
                        
                        // 调用函数将找到的块的图层更改为"0DataFlow-GsLcValveFreeze"
                        using (Transaction trans = database.TransactionManager.StartTransaction())
                        {
                            try
                            {
                                foreach (ObjectId id in objectIds)
                                {
                                    // 打开块引用进行写入
                                    UtilsBlock.UtilsChangeBlockLayerName(id, "0DataFlow-GsLcValveFreeze");
                                }
                                
                                // 提交事务
                                trans.Commit();
                                editor.WriteMessage($"\n成功将 {objectIds.Count} 个块的图层更改为 '0DataFlow-GsLcValveFreeze'");
                            }
                            catch (Exception ex)
                            {
                                editor.WriteMessage($"\n更改图层时发生错误: {ex.Message}");
                                trans.Abort();
                            }
                        }
                    }
                    else
                    {
                        editor.WriteMessage("\n未找到需要更改图层的块");
                    }
                }
                catch (Exception ex)
                {
                    editor.WriteMessage($"\n查找块时发生错误: {ex.Message}");
                }
                return true;
            }
            catch (Exception ex)
            {
                editor.WriteMessage($"\n执行测试命令时发生错误: {ex.Message}");
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