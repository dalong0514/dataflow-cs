using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using dataflow_cs.Utils.CADUtils;

namespace dataflow_cs.Utils.JigUtils
{
    internal class InsertBlockJig : EntityJig
    {
        private Point3d _insertionPoint; // 插入点
        private double _rotation; // 旋转角度
        private string _prompt; // 提示信息
        private bool _rotationRequested; // 是否请求旋转

        /// <summary>
        /// 初始化新的块插入拖拽对象
        /// </summary>
        /// <param name="position">初始插入位置</param>
        /// <param name="blockReference">块引用对象</param>
        /// <param name="rotation">初始旋转角度</param>
        /// <param name="prompt">提示信息</param>
        public InsertBlockJig(Point3d position, BlockReference blockReference, double rotation, string prompt)
            : base(blockReference)
        {
            _insertionPoint = position;
            _rotation = rotation;
            _prompt = prompt;
            _rotationRequested = false;
            Update();
        }

        /// <summary>
        /// 获取拖拽过程中需要更新的参数
        /// </summary>
        /// <param name="prompts">提示信息</param>
        /// <returns>更新状态</returns>
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            // 如果请求旋转，执行旋转并重置标志
            if (_rotationRequested)
            {
                _rotation += Math.PI / 2;
                _rotationRequested = false;
                return SamplerStatus.OK;
            }

            // 设置点选项
            JigPromptPointOptions pointOpts = new JigPromptPointOptions(_prompt);
            pointOpts.UseBasePoint = false;
            
            // 允许输入其他字符串，设置NoZeroResponseAccepted，屏蔽空格键
            pointOpts.UserInputControls = UserInputControls.AcceptOtherInputString | UserInputControls.NoZeroResponseAccepted;
            
            // 设置关键字
            pointOpts.Keywords.Clear();
            pointOpts.Keywords.Add("R");

            // 获取用户输入
            PromptPointResult result = prompts.AcquirePoint(pointOpts);
            
            // 如果用户输入关键字，执行旋转
            if (result.Status == PromptStatus.Keyword)
            {
                if (result.StringResult == "R")
                {
                    _rotation += Math.PI / 2;
                    return SamplerStatus.OK;
                }
                return SamplerStatus.NoChange;
            }
            
            // 检查位置是否变化
            if (result.Status == PromptStatus.OK)
            {
                if (_insertionPoint == result.Value)
                    return SamplerStatus.NoChange;

                _insertionPoint = result.Value;
                return SamplerStatus.OK;
            }
            
            return SamplerStatus.Cancel;
        }

        /// <summary>
        /// 更新拖拽实体的属性
        /// </summary>
        /// <returns>是否更新成功</returns>
        protected override bool Update()
        {
            BlockReference blockRef = Entity as BlockReference;
            if (blockRef != null)
            {
                blockRef.Position = _insertionPoint;
                blockRef.Rotation = _rotation;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取当前拖拽的实体
        /// </summary>
        /// <returns>实体对象</returns>
        public Entity GetEntity()
        {
            return Entity;
        }
        
        /// <summary>
        /// 获取当前旋转角度
        /// </summary>
        public double Rotation
        {
            get { return _rotation; }
        }
        
        /// <summary>
        /// 获取当前插入点
        /// </summary>
        public Point3d InsertionPoint
        {
            get { return _insertionPoint; }
        }
        
        /// <summary>
        /// 请求旋转，将在下一次采样中执行
        /// </summary>
        public void RequestRotation()
        {
            _rotationRequested = true;
        }

        /// <summary>
        /// 使用拖拽方式交互式插入块
        /// </summary>
        /// <param name="editor">当前编辑器</param>
        /// <param name="database">当前数据库</param>
        /// <param name="blockName">要插入的块名称</param>
        /// <param name="blockId">块定义的ObjectId</param>
        /// <param name="initialPoint">初始插入点</param>
        /// <param name="initialRotation">初始旋转角度（弧度）</param>
        /// <param name="layerName">块所在图层（默认为"0"）</param>
        /// <param name="prompt">拖拽过程中的提示（默认为"请选择插入点或输入[旋转(R)]:"）</param>
        /// <param name="escapeMessage">用户取消时显示的消息（默认为"命令已取消。"）</param>
        /// <param name="successMessage">插入成功后显示的消息模板（默认为"{0}已插入，继续拖动放置新的{0}，输入\"R\"可旋转，ESC退出"）</param>
        /// <param name="rotationMessage">旋转操作后显示的消息模板（默认为"{0}已旋转，当前角度: {1}度"）</param>
        /// <returns>是否成功完成至少一次插入操作</returns>
        public static bool DragAndInsertBlock(
            Editor editor,
            Database database,
            string blockName,
            ObjectId blockId,
            Point3d initialPoint,
            double initialRotation = 0,
            string layerName = "0",
            string prompt = "请选择插入点或输入[旋转(R)]:",
            string escapeMessage = "命令已取消。",
            string successMessage = "{0}已插入，继续拖动放置新的{0}，输入\"R\"可旋转，ESC退出",
            string rotationMessage = "{0}已旋转，当前角度: {1}度")
        {
            if (blockId == ObjectId.Null)
            {
                editor.WriteMessage("\n无效的块定义。");
                return false;
            }

            // 为防止事务嵌套，先声明但不立即启动事务
            Transaction tr = null;
            bool hasInsertedAtLeastOnce = false;
            double rotation = initialRotation;

            try
            {
                // 进入交互循环
                while (true)
                {
                    try
                    {
                        // 开始新事务
                        tr = database.TransactionManager.StartTransaction();

                        // 创建新的块参照用于拖拽
                        BlockReference sourceBr = new BlockReference(initialPoint, blockId);
                        sourceBr.Layer = layerName;
                        sourceBr.Rotation = rotation;

                        // 创建拖拽对象并显示
                        InsertBlockJig jig = new InsertBlockJig(initialPoint, sourceBr, rotation, prompt);

                        // 执行拖拽
                        PromptResult jigResult = editor.Drag(jig);

                        // 用户确定了位置
                        if (jigResult.Status == PromptStatus.OK)
                        {
                            // 获取块定义名
                            string currentBlockName = blockName;
                            if (blockId != ObjectId.Null)
                            {
                                BlockTableRecord blockDef = tr.GetObject(blockId, OpenMode.ForRead) as BlockTableRecord;
                                if (blockDef != null)
                                {
                                    currentBlockName = blockDef.Name;
                                }
                            }

                            ObjectId newBlockId = UtilsBlock.UtilsInsertBlock(
                                currentBlockName,
                                jig.InsertionPoint,
                                1.0, 1.0, 1.0, // 使用默认缩放比例
                                jig.Rotation, // 使用jig中的旋转角度
                                layerName, // 设置指定图层
                                tr // 传入当前事务
                            );

                            // 提交事务，使块立即显示
                            tr.Commit();
                            tr = null;

                            // 标记已成功插入至少一次
                            hasInsertedAtLeastOnce = true;

                            // 更新初始点为当前位置，方便连续插入
                            initialPoint = jig.InsertionPoint;
                            rotation = jig.Rotation; // 保持当前旋转角度

                            // 显示成功消息
                            editor.WriteMessage("\n" + string.Format(successMessage, blockName));
                        }
                        // 用户输入了关键字
                        else if (jigResult.Status == PromptStatus.Keyword)
                        {
                            // 获取更新后的旋转角度
                            rotation = jig.Rotation;

                            // 放弃当前事务
                            tr.Dispose();
                            tr = null;

                            // 显示旋转消息
                            editor.WriteMessage("\n" + string.Format(rotationMessage, blockName, Math.Round(rotation * 180 / Math.PI)));
                        }
                        // 用户取消或按ESC
                        else
                        {
                            if (tr != null)
                            {
                                tr.Dispose();
                                tr = null;
                            }
                            editor.WriteMessage("\n" + escapeMessage);
                            break;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        editor.WriteMessage($"\n执行操作时发生错误: {ex.Message}");
                        if (tr != null)
                        {
                            tr.Dispose();
                            tr = null;
                        }
                    }
                }

                return hasInsertedAtLeastOnce;
            }
            catch (System.Exception ex)
            {
                editor.WriteMessage($"\n拖拽插入块时发生错误: {ex.Message}");
                if (tr != null)
                {
                    tr.Dispose();
                }
                return hasInsertedAtLeastOnce;
            }
        }

        /// <summary>
        /// 使用拖拽方式交互式插入块（自动计算初始点）
        /// </summary>
        /// <param name="editor">当前编辑器</param>
        /// <param name="database">当前数据库</param>
        /// <param name="blockName">要插入的块名称</param>
        /// <param name="blockId">块定义的ObjectId</param>
        /// <param name="initialRotation">初始旋转角度（弧度），默认为0</param>
        /// <param name="layerName">块所在图层（默认为"0"）</param>
        /// <param name="prompt">拖拽过程中的提示（默认为"请选择插入点或输入[旋转(R)]:"）</param>
        /// <param name="escapeMessage">用户取消时显示的消息（默认为"命令已取消。"）</param>
        /// <param name="successMessage">插入成功后显示的消息模板（默认为"{0}已插入，继续拖动放置新的{0}，输入\"R\"可旋转，ESC退出"）</param>
        /// <param name="rotationMessage">旋转操作后显示的消息模板（默认为"{0}已旋转，当前角度: {1}度"）</param>
        /// <returns>是否成功完成至少一次插入操作</returns>
        public static bool DragAndInsertBlock(
            Editor editor,
            Database database,
            string blockName,
            ObjectId blockId,
            double initialRotation = 0,
            string layerName = "0",
            string prompt = "请选择插入点或输入[旋转(R)]:",
            string escapeMessage = "命令已取消。",
            string successMessage = "{0}已插入，继续拖动放置新的{0}，输入\"R\"可旋转，ESC退出",
            string rotationMessage = "{0}已旋转，当前角度: {1}度")
        {
            // 自动获取初始点坐标（从原点转换到WCS）
            Point3d initialPoint = Point3d.Origin;
            
            // 从UCS坐标系转换到WCS坐标系
            Autodesk.AutoCAD.ApplicationServices.Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                Matrix3d ucsToWcs = doc.Editor.CurrentUserCoordinateSystem;
                initialPoint = initialPoint.TransformBy(ucsToWcs);
            }
            
            // 调用原方法
            return DragAndInsertBlock(
                editor,
                database,
                blockName,
                blockId,
                initialPoint,
                initialRotation,
                layerName,
                prompt,
                escapeMessage,
                successMessage,
                rotationMessage
            );
        }
        
    }
}
