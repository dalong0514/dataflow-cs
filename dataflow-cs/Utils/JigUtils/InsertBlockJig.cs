using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

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
    }
}
