using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace dataflow_cs.Presentation.Views.Controls
{
    /// <summary>
    /// 自定义树控件，支持自定义绘制和鼠标交互效果
    /// </summary>
    public class CustomTreeView : TreeView
    {
        private TreeNode lastHoveredNode = null;

        public CustomTreeView()
        {
            this.DrawMode = TreeViewDrawMode.OwnerDrawAll;
            // 启用热跟踪
            this.HotTracking = true;
            this.ShowLines = false;
            this.ShowPlusMinus = false;
            this.FullRowSelect = true;
            this.HideSelection = false;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            this.DrawNode += CustomTreeView_DrawNode;
            this.MouseMove += CustomTreeView_MouseMove;
            this.MouseLeave += CustomTreeView_MouseLeave;
        }

        private void CustomTreeView_MouseLeave(object sender, EventArgs e)
        {
            this.SelectedNode = null;
        }

        private void CustomTreeView_MouseMove(object sender, MouseEventArgs e)
        {
            // 鼠标悬停时，获取节点并更新 Hot 状态（仅二级节点响应悬停高亮）
            TreeNode node = this.GetNodeAt(e.Location);
            if (node != lastHoveredNode)
            {
                lastHoveredNode = node;
                this.SelectedNode = node;
                // 如果希望只重绘当前节点区域，可以调用 Invalidate(node.Bounds);
            }
        }

        // 重写 OnNodeMouseHover 事件
        protected override void OnNodeMouseHover(TreeNodeMouseHoverEventArgs e)
        {
            base.OnNodeMouseHover(e);
            // 设置当前悬停节点为选中状态
            this.SelectedNode = e.Node;
        }

        private void CustomTreeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            // 判断节点状态并设置背景颜色
            Color bgColor = Color.Transparent;
            Color textColor = this.ForeColor;
            if (e.Node.IsSelected)
            {
                bgColor = Color.LightSkyBlue;
                textColor = Color.WhiteSmoke;
            }
            else if (e.State.HasFlag(TreeNodeStates.Hot))
            {
                //bgColor = Color.WhiteSmoke;
                //textColor = Color.WhiteSmoke;
            }

            using (SolidBrush bgBrush = new SolidBrush(bgColor))
            {
                e.Graphics.FillRectangle(bgBrush, e.Bounds);
            }

            int iconOffset = 0;
            // 绘制图标（如果存在且不是一级菜单）
            try
            {
                if (this.ImageList != null && !string.IsNullOrEmpty(e.Node.ImageKey) && 
                    this.ImageList.Images.ContainsKey(e.Node.ImageKey) && 
                    e.Node.Level > 0) // 仅为二级或更深层级菜单项显示图标
                {
                    Image nodeImage = this.ImageList.Images[e.Node.ImageKey];
                    if (nodeImage != null)
                    {
                        // 根据节点层级计算图标的X坐标，增加二级菜单的缩进
                        int iconX = e.Bounds.X + (e.Node.Level * this.Indent) + 5;
                        int iconY = e.Bounds.Y + (e.Bounds.Height - this.ImageList.ImageSize.Height) / 2;
                        e.Graphics.DrawImage(nodeImage, iconX, iconY, this.ImageList.ImageSize.Width, this.ImageList.ImageSize.Height);
                        iconOffset = this.ImageList.ImageSize.Width + 2; // 图标宽度加上间距
                    }
                }
            }
            catch
            {
                // 忽略图标绘制错误，确保不影响节点文本显示
            }

            // 如果节点有子节点，绘制自定义展开/收缩按钮
            if (e.Node.Nodes.Count > 0)
            {
                int btnSize = 12;
                int offsetX = e.Node.Level * this.Indent / 2; // 使用树控件的缩进值
                int btnX = e.Bounds.X + 5 + offsetX;
                int btnY = e.Bounds.Y + (e.Bounds.Height - btnSize) / 2;
                Rectangle btnRect = new Rectangle(btnX, btnY, btnSize, btnSize);
                DrawCross(e.Graphics, btnRect, e.Node.IsExpanded);
            }

            // 绘制节点文本，根据节点层级和按钮状态调整文本位置
            int textOffset = e.Node.Nodes.Count > 0 ? 20 : 0;
            // 二级菜单比一级菜单多缩进一些
            int levelIndent = e.Node.Level * this.Indent;
            Rectangle textRect = new Rectangle(
                e.Bounds.X + 5 + textOffset + levelIndent + iconOffset, 
                e.Bounds.Y, 
                e.Bounds.Width - (5 + textOffset + levelIndent + iconOffset), 
                e.Bounds.Height);

            // 为二级菜单使用较小的字体
            Font nodeFont;
            if (e.Node.Level > 0)
            {
                // 二级菜单使用较小的字体
                float smallerSize = this.Font.Size * 0.9f;
                nodeFont = new Font(this.Font.FontFamily, smallerSize, this.Font.Style);
            }
            else
            {
                // 一级菜单使用粗体
                nodeFont = new Font(this.Font, FontStyle.Bold);
            }

            // 使用适当的字体绘制文本
            TextRenderer.DrawText(e.Graphics, e.Node.Text, nodeFont, textRect, textColor, TextFormatFlags.VerticalCenter);
            
            // 释放临时创建的字体资源
            if (nodeFont != this.Font)
            {
                nodeFont.Dispose();
            }
        }

        private void DrawCross(Graphics g, Rectangle rect, bool expanded)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // 绘制十字形状
            using (Pen pen = new Pen(Color.DarkGray, 2))
            {
                // 绘制水平线
                int yMiddle = rect.Top + rect.Height / 2;
                g.DrawLine(pen, rect.Left, yMiddle, rect.Right, yMiddle);
                
                // 如果是收起状态，还需要绘制垂直线形成十字
                if (!expanded)
                {
                    int xMiddle = rect.Left + rect.Width / 2;
                    g.DrawLine(pen, xMiddle, rect.Top, xMiddle, rect.Bottom);
                }
            }
            
            g.SmoothingMode = SmoothingMode.Default;
        }
    }
} 