using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace dataflow_cs.Utils.CADUtils
{
    /// <summary>
    /// AutoCAD视图缩放工具类
    /// </summary>
    public static class UtilsZoom
    {
        /// <summary>
        /// 扩展方法：视图的EyeToWorld转换矩阵
        /// </summary>
        public static Matrix3d EyeToWorld(this ViewTableRecord view)
        {
            if (view == null)
                throw new ArgumentNullException("view");

            return
                Matrix3d.Rotation(-view.ViewTwist, view.ViewDirection, view.Target) *
                Matrix3d.Displacement(view.Target - Point3d.Origin) *
                Matrix3d.PlaneToWorld(view.ViewDirection);
        }

        /// <summary>
        /// 扩展方法：视图的WorldToEye转换矩阵
        /// </summary>
        public static Matrix3d WorldToEye(this ViewTableRecord view)
        {
            return view.EyeToWorld().Inverse();
        }

        /// <summary>
        /// 缩放视图以适应指定的范围
        /// </summary>
        /// <param name="ed">编辑器</param>
        /// <param name="ext">几何范围</param>
        public static void ZoomExtents(this Editor ed, Extents3d ext)
        {
            if (ed == null)
                throw new ArgumentNullException("ed");

            using (ViewTableRecord view = ed.GetCurrentView())
            {
                ext.TransformBy(view.WorldToEye());
                view.Width = ext.MaxPoint.X - ext.MinPoint.X;
                view.Height = ext.MaxPoint.Y - ext.MinPoint.Y;
                view.CenterPoint = new Point2d(
                    (ext.MaxPoint.X + ext.MinPoint.X) / 2.0,
                    (ext.MaxPoint.Y + ext.MinPoint.Y) / 2.0);
                ed.SetCurrentView(view);
            }
        }

        /// <summary>
        /// 缩放视图以适应指定的范围，并应用缩放比例
        /// </summary>
        /// <param name="ed">编辑器</param>
        /// <param name="ext">几何范围</param>
        /// <param name="scale">缩放比例</param>
        public static void ZoomExtents(this Editor ed, Extents3d ext, double scale)
        {
            if (ed == null)
                throw new ArgumentNullException("ed");

            using (ViewTableRecord view = ed.GetCurrentView())
            {
                ext.TransformBy(view.WorldToEye());
                view.Width = (ext.MaxPoint.X - ext.MinPoint.X) * scale;
                view.Height = (ext.MaxPoint.Y - ext.MinPoint.Y) * scale;
                view.CenterPoint = new Point2d(
                    (ext.MaxPoint.X + ext.MinPoint.X) / 2.0,
                    (ext.MaxPoint.Y + ext.MinPoint.Y) / 2.0);
                ed.SetCurrentView(view);
            }
        }

        /// <summary>
        /// 缩放到指定中心点位置，并应用缩放比例
        /// </summary>
        /// <param name="ed">编辑器</param>
        /// <param name="center">中心点</param>
        /// <param name="scale">缩放比例</param>
        public static void ZoomCenter(this Editor ed, Point3d center, double scale = 1.0)
        {
            if (ed == null)
                throw new ArgumentNullException("ed");

            using (ViewTableRecord view = ed.GetCurrentView())
            {
                center = center.TransformBy(view.WorldToEye());
                view.Height /= scale;
                view.Width /= scale;
                view.CenterPoint = new Point2d(center.X, center.Y);
                ed.SetCurrentView(view);
            }
        }

        /// <summary>
        /// 缩放到整个图形范围
        /// </summary>
        /// <param name="ed">编辑器</param>
        public static void Zoom(this Editor ed)
        {
            if (ed == null)
                throw new ArgumentNullException("ed");

            Database db = ed.Document.Database;
            db.UpdateExt(false);

            try
            {
                Extents3d ext = (short)Application.GetSystemVariable("cvport") == 1 ?
                           new Extents3d(db.Pextmin, db.Pextmax) :
                           new Extents3d(db.Extmin, db.Extmax);
                ed.ZoomExtents(ext);
            }
            catch
            {
                // 忽略异常
            }
        }

        /// <summary>
        /// 缩放以显示指定的实体集合
        /// </summary>
        /// <param name="ed">编辑器</param>
        /// <param name="ids">实体ID集合</param>
        public static void ZoomObjects(this Editor ed, IEnumerable<ObjectId> ids)
        {
            if (ed == null)
                throw new ArgumentNullException("ed");

            using (Transaction tr = ed.Document.TransactionManager.StartTransaction())
            {
                Extents3d ext = ids
                    .Where(id => id.ObjectClass.IsDerivedFrom(RXObject.GetClass(typeof(Entity))))
                    .Select(id => ((Entity)tr.GetObject(id, OpenMode.ForRead)).GeometricExtents)
                    .Aggregate((e1, e2) => { e1.AddExtents(e2); return e1; });
                ed.ZoomExtents(ext);
                tr.Commit();
            }
        }
    }
} 