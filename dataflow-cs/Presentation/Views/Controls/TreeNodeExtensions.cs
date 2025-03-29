using System.Windows.Forms;

namespace dataflow_cs.Presentation.Views.Controls
{
    /// <summary>
    /// TreeNode扩展方法
    /// </summary>
    public static class TreeNodeExtensions
    {
        /// <summary>
        /// Toggle展开或收缩节点
        /// </summary>
        public static void Toggle(this TreeNode node)
        {
            if (node == null) return;
            
            if (node.IsExpanded)
                node.Collapse();
            else
                node.Expand();
        }
    }
} 