//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;
using System.Windows.Controls;

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// TreeView utilities containing TreeView extension methods</summary>
    public static class TreeViewUtil
    {
        /// <summary>
        /// Gets object in TreeView at a given point, if any</summary>
        /// <param name="treeView">TreeView</param>
        /// <param name="p">Point</param>
        /// <returns>Object in TreeView at a given point or null if none</returns>
        public static object GetItemAtPoint(this TreeView treeView, Point p)
        {
            var tvi = treeView.GetItemContainerAtPoint(p);

            ItemsControl parent = ItemsControl.ItemsControlFromItemContainer(tvi);

            if (parent != null)
            {
                object data = parent.ItemContainerGenerator.ItemFromContainer(tvi);
                if (data != DependencyProperty.UnsetValue)
                {
                    return data;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets TreeViewItem in TreeView at a given point, if any</summary>
        /// <param name="treeView">TreeView</param>
        /// <param name="p">Point</param>
        /// <returns>TreeViewItem in TreeView at a given point or null if none</returns>
        public static TreeViewItem GetItemContainerAtPoint(this TreeView treeView, Point p)
        {
            var dep = treeView.InputHitTest(p) as DependencyObject;
            if (dep != null)
            {
                return dep.FindAncestor<TreeViewItem>();
            }

            return null;
        }
    }
}
