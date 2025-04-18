using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using dataflow_cs.Core.Services;
using dataflow_cs.Presentation.Views.Windows;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using dataflow_cs.Utils.CADUtils;
using dataflow_cs.Utils.JigUtils;
using dataflow_cs.Utils.ConstUtils;

namespace dataflow_cs.Business.Commands.GsLc
{
    internal class GsLcInsertAllElementBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertAllElementBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            // 使用拖拽插入方法并获取插入的块ID
            using (Transaction tr = database.TransactionManager.StartTransaction())
            {

                try
                {
                    // 引入工艺数据流组件块定义
                    ObjectId blockId = UtilsGeometry.UtilsImportBlockFromExternalDwg(ConstFileName.GsLcBlocksPath, ConstBlockName.GsLcAllBlocks);
                    if (blockId == ObjectId.Null)
                    {
                        editor.WriteMessage("\n导入块定义失败，请检查块文件路径和块名称。");
                        return false;
                    }
                    // 拾取一个点
                    PromptPointResult pointResult = editor.GetPoint("\n请选择插入点:");
                    if (pointResult.Status != PromptStatus.OK)
                    {
                        editor.WriteMessage("\n拾取点失败，请重新选择。");
                        return false;
                    }
                    UtilsBlock.UtilsInsertBlock(ConstBlockName.GsLcAllBlocks, pointResult.Value);
                    // 获取最后插入的块引用ID
                    SelectionSet selSet = UtilsSelectionSet.UtilsGetLastCreatedObject();
                    ObjectId insertedBlockId = selSet.GetObjectIds().FirstOrDefault();
                    UtilsBlock.UtilsExplodeBlock(insertedBlockId, tr);
                    // 方法2: 使用带连字符的命令版本以避免显示对话框
                    UtilsCADActive.Document.SendStringToExecute("WIPEOUT F OFF ", true, false, false);
                    
                    tr.Commit();
                }
                catch (System.Exception ex)
                {
                    editor.WriteMessage($"\n插入工艺数据流组件块时发生错误: {ex.Message}");
                    tr.Abort();
                }
            }
            return true;
        }
    }

    /// <summary>
    /// 通用块插入辅助类
    /// </summary>
    internal static class BlockInsertHelper
    {
        /// <summary>
        /// 通用块插入方法
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <param name="blockName">块名称</param>
        /// <param name="layerName">图层名称</param>
        /// <param name="rotation">旋转角度</param>
        /// <returns>是否插入成功</returns>
        public static bool InsertBlockGeneric(Editor editor, Database database, string blockName, string layerName, double rotation = 0)
        {
            try
            {
                // 引入块定义
                ObjectId blockId = UtilsGeometry.UtilsImportBlockFromExternalDwg(ConstFileName.GsLcBlocksPath, blockName);
                UtilsGeometry.UtilsImportLayerFromExternalDwg(ConstFileName.GsLcBlocksPath, layerName);
                
                if (blockId == ObjectId.Null)
                {
                    editor.WriteMessage($"\n导入块定义失败，请检查块文件路径和块名称。");
                    return false;
                }

                // 使用封装的拖拽插入方法（自动计算初始点）
                bool result = InsertBlockJig.DragAndInsertBlock(
                    editor,
                    database,
                    blockName,
                    blockId,
                    rotation,
                    layerName
                );

                return result;
            }
            catch (System.Exception ex)
            {
                editor.WriteMessage($"\n插入块时发生错误: {ex.Message}");
                return false;
            }
        }
    }

    internal class GsLcInsertGlobalBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertGlobalBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveBall,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertInstrumentPBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertInstrumentPBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcInstrumentP,
                ConstLayerName.GsLcLayerNameInstrument
            );
        }
    }

    internal class GsLcInsertInstrumentLBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertInstrumentLBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcInstrumentL,
                ConstLayerName.GsLcLayerNameInstrument
            );
        }
    }

    internal class GsLcInsertInstrumentSISBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertInstrumentSISBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcInstrumentSIS,
                ConstLayerName.GsLcLayerNameInstrument
            );
        }
    }

    internal class GsLcInsertValveBallTeeBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveBallTeeBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveBallTee,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveGlobeBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveGlobeBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveGlobe,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValvePressureReduceBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValvePressureReduceBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValvePressureReduce,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveCheckBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveCheckBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveCheck,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveCheckSwingBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveCheckSwingBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveCheckSwing,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveCockBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveCockBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveCock,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValvePlungerBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValvePlungerBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValvePlunger,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveNeedleBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveNeedleBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveNeedle,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveGateBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveGateBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveGate,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveFlapperBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveFlapperBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveFlapper,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveDiaphragmBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveDiaphragmBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveDiaphragm,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveButterflyBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveButterflyBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveButterfly,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveSafetyBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveSafetyBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveSafety,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveBlastBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveBlastBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveBlast,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveTrapBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveTrapBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveTrap,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveBreathBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveBreathBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveBreath,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveBreathFlameArrestBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveBreathFlameArrestBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveBreathFlameArrest,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveFlameArrestBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveFlameArrestBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveFlameArrest,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveMetalHoseBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveMetalHoseBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveMetalHose,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveFilterYBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveFilterYBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveFilterY,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveFilterTBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveFilterTBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveFilterT,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveFilterConeBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveFilterConeBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveFilterCone,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveFilterBasketBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveFilterBasketBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveFilterBasket,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveFlangeCoverBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveFlangeCoverBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveFlangeCover,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveRestrictOrificeSingleBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveRestrictOrificeSingleBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveRestrictOrificeSingle,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveGlassBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveGlassBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveGlass,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveBlindBoard8OffBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveBlindBoard8OffBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveBlindBoard8Off,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveBlindBoard8OnBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveBlindBoard8OnBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveBlindBoard8On,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveBlindBoardOffBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveBlindBoardOffBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveBlindBoardOff,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveBlindBoardOnBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveBlindBoardOnBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveBlindBoardOn,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValvePipeClassChangeBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValvePipeClassChangeBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValvePipeClassChange,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveReducerBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveReducerBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveReducer,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveFlexibleHoseBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveFlexibleHoseBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveFlexibleHose,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveGlobeRippleBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveGlobeRippleBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveGlobeRipple,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveOnOffFlapperBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveOnOffFlapperBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveOnOffFlapper,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveOnOffDiaphragmBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveOnOffDiaphragmBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveOnOffDiaphragm,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveOnOffButterflyBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveOnOffButterflyBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveOnOffButterfly,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveOnOffTeeDiaphragmBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveOnOffTeeDiaphragmBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveOnOffTeeDiaphragm,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveOnOffTeeBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveOnOffTeeBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveOnOffTee,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveControlTeeBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveControlTeeBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveControlTee,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveControlDiaphragmBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveControlDiaphragmBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveControlDiaphragm,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveAutoBottomBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveAutoBottomBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveAutoBottom,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveControlSelfOperateBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveControlSelfOperateBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveControlSelfOperate,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveControlBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveControlBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveControl,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertValveOnOffBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertValveOnOffBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcValveOnOff,
                ConstLayerName.GsLcLayerNameValve
            );
        }
    }

    internal class GsLcInsertInstrumentElementinterLockLogicBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertInstrumentElementinterLockLogicBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcinstrumentElementinterLockLogic,
                ConstLayerName.GsLcLayerNameInstrument
            );
        }
    }

    internal class GsLcInsertInstrumentElementinterLockLogicSISBlockCommand : CommandHandlerBase
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "DLGsLcInsertInstrumentElementinterLockLogicSISBlock";

        /// <summary>
        /// 执行命令核心逻辑
        /// </summary>
        /// <param name="editor">编辑器</param>
        /// <param name="database">数据库</param>
        /// <returns>命令执行结果</returns>
        protected override bool ExecuteCore(Editor editor, Database database)
        {
            return BlockInsertHelper.InsertBlockGeneric(
                editor,
                database,
                ConstBlockName.GsLcinstrumentElementinterLockLogicSIS,
                ConstLayerName.GsLcLayerNameInstrument
            );
        }
    }
}
