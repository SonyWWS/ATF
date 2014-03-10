//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// Collection utilities</summary>
    public static class CollectionUtil
    {
        /// <summary>
        /// Gets the item at a point in an ItemsControl, which is a Control that can be used to present a collection of items</summary>
        /// <param name="itemsControl">ItemsControl examined</param>
        /// <param name="p">Point on desired item</param>
        /// <returns>Item at a point in an ItemsControl</returns>
        public static object GetItemAtPoint(this ItemsControl itemsControl, Point p)
        {
            if (itemsControl is TreeView)
                return ((TreeView)itemsControl).GetItemAtPoint(p);

            var dep = itemsControl.InputHitTest(p) as DependencyObject;
            if (dep != null)
            {
                while (dep != null && dep != itemsControl)
                {
                    object data = itemsControl.ItemContainerGenerator.ItemFromContainer(dep);
                    if (data != DependencyProperty.UnsetValue)
                        return data;

                    dep = LogicalTreeHelper.GetParent(dep) ?? VisualTreeHelper.GetParent(dep);
                }
            }

            return null;
        }
    }
}
