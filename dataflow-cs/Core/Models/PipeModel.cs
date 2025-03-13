using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;

namespace dataflow_cs.Core.Models
{
    /// <summary>
    /// 管道模型类，表示一个管道及其属性
    /// </summary>
    public class PipeModel
    {
        /// <summary>
        /// 管道ID
        /// </summary>
        public string PipeId { get; set; }
        
        /// <summary>
        /// 管道编号
        /// </summary>
        public string PipeNumber { get; set; }
        
        /// <summary>
        /// 管道名称
        /// </summary>
        public string PipeName { get; set; }
        
        /// <summary>
        /// 管道线ObjectId
        /// </summary>
        public ObjectId PipeLineObjectId { get; set; }
        
        /// <summary>
        /// 管道标记ObjectId
        /// </summary>
        public ObjectId PipeTagObjectId { get; set; }
        
        /// <summary>
        /// 管道直径
        /// </summary>
        public double Diameter { get; set; }
        
        /// <summary>
        /// 管道高程
        /// </summary>
        public double Elevation { get; set; }
        
        /// <summary>
        /// 管道材料
        /// </summary>
        public string Material { get; set; }
        
        /// <summary>
        /// 管道长度
        /// </summary>
        public double Length { get; set; }
        
        /// <summary>
        /// 起点坐标
        /// </summary>
        public Point3d StartPoint { get; set; }
        
        /// <summary>
        /// 终点坐标
        /// </summary>
        public Point3d EndPoint { get; set; }
        
        /// <summary>
        /// 关联管件列表
        /// </summary>
        public List<ObjectId> PipeFittings { get; set; }
        
        /// <summary>
        /// 附加属性字典
        /// </summary>
        public Dictionary<string, string> Properties { get; set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public PipeModel()
        {
            PipeFittings = new List<ObjectId>();
            Properties = new Dictionary<string, string>();
        }
        
        /// <summary>
        /// 通过管道线和标记创建管道模型
        /// </summary>
        /// <param name="pipeLineObjectId">管道线ObjectId</param>
        /// <param name="pipeTagObjectId">管道标记ObjectId</param>
        public PipeModel(ObjectId pipeLineObjectId, ObjectId pipeTagObjectId) : this()
        {
            PipeLineObjectId = pipeLineObjectId;
            PipeTagObjectId = pipeTagObjectId;
        }
        
        /// <summary>
        /// 获取管道中心点
        /// </summary>
        /// <returns>中心点坐标</returns>
        public Point3d GetCenterPoint()
        {
            if (StartPoint != null && EndPoint != null)
            {
                return new Point3d(
                    (StartPoint.X + EndPoint.X) / 2,
                    (StartPoint.Y + EndPoint.Y) / 2,
                    (StartPoint.Z + EndPoint.Z) / 2
                );
            }
            return Point3d.Origin;
        }
        
        /// <summary>
        /// 计算管道长度
        /// </summary>
        /// <returns>管道长度</returns>
        public double CalculateLength()
        {
            if (StartPoint != null && EndPoint != null)
            {
                Length = StartPoint.DistanceTo(EndPoint);
                return Length;
            }
            return 0;
        }
        
        /// <summary>
        /// 添加属性
        /// </summary>
        /// <param name="key">属性名</param>
        /// <param name="value">属性值</param>
        public void AddProperty(string key, string value)
        {
            if (!string.IsNullOrEmpty(key))
            {
                Properties[key] = value;
            }
        }
        
        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="key">属性名</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>属性值</returns>
        public string GetProperty(string key, string defaultValue = "")
        {
            if (Properties.ContainsKey(key))
            {
                return Properties[key];
            }
            return defaultValue;
        }
    }
} 