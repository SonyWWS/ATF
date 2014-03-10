//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Simple items control drag source behavior that only deals with the single item under the mouse,
    /// i.e., not for selection or multi-selection</summary>
    public class ItemsControlDragSourceBehavior : DragSourceBehavior<ItemsControl>
    {
        /// <summary>
        /// Performs custom processing on BeginDrag event</summary>
        /// <param name="e">A MouseEventArgs that contains the event data</param>
        protected override void BeginDrag(MouseEventArgs e)
        {
            var pos = e.GetPosition(AssociatedObject);
            var data = AssociatedObject.GetItemAtPoint(pos);
            if (data != null)
            {
                DragDrop.DoDragDrop(AssociatedObject, new[]{data}, DragDropEffects.All);
            }
        }
    }
}
