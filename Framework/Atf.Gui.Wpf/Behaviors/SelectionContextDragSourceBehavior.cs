//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Linq;
using System.Windows;
using System.Windows.Input;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Drag source behavior for elements with a selection context, as their data context
    /// drag drop is initiated using current selection of items from the context</summary>
    public class SelectionContextDragSourceBehavior : DragSourceBehavior<FrameworkElement>
    {
        /// <summary>
        /// Performs custom processing on BeginDrag event</summary>
        /// <param name="e">A MouseEventArgs that contains the event data</param>
        protected override void BeginDrag(MouseEventArgs e)
        {
            // Get selection context from attached property
            var ctx = (ISelectionContext)AssociatedObject.GetValue(SelectionBehaviors.SelectionContextProperty);
            
            if (ctx == null)
            {
                // Fall back on data context if SelectionContextProperty not set
                ctx = AssociatedObject.DataContext.As<ISelectionContext>();
            }

            if (ctx != null)
            {
                object[] data = ctx.Selection.ToArray();
                if (data.Length > 0)
                {
                    DragDrop.DoDragDrop(AssociatedObject, data, DragDropEffects.All);
                }
            }
        }
    }
}
