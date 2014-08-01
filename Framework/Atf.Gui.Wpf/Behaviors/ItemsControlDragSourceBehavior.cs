//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Controls.Adaptable;

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
            object data = AssociatedObject.GetItemAtPoint(pos);
            if (data != null)
            {
                object[] dataArray = new[] { data };

                DragDropEffects effects = DragDropEffects.Move;
                var instancingContext = AssociatedObject.DataContext.As<IInstancingContext>();
                if (instancingContext != null && instancingContext.CanCopy())
                    effects |= DragDropEffects.Copy;

                var converter = AssociatedObject.DataContext.As<IDragDropConverter>();
                if(converter != null)
                {
                    dataArray = converter.Convert(dataArray).ToArray();
                }

                System.Windows.DragDrop.DoDragDrop(AssociatedObject, dataArray, effects);
            }
        }
    }
}
