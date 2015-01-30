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
        /// Selection context dependency property</summary>
        public static readonly DependencyProperty SelectionContextProperty =
           DependencyProperty.Register("SelectionContext", typeof(ISelectionContext), typeof(SelectionContextDragSourceBehavior), new PropertyMetadata(default(ISelectionContext)));

        /// <summary>
        /// Get or set selection context dependency property</summary>
        public ISelectionContext SelectionContext
        {
            get { return (ISelectionContext)GetValue(SelectionContextProperty); }
            set { SetValue(SelectionContextProperty, value); }
        }

        /// <summary>
        /// Begin drag operation</summary>
        /// <param name="e">Mouse event arguments</param>
        protected override void BeginDrag(MouseEventArgs e)
        {
            var ctx = SelectionContext;
            
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
                    DragDropEffects effects = DragDropEffects.Move;

                    var instancingContext = AssociatedObject.DataContext.As<IInstancingContext>();
                    if (instancingContext != null && instancingContext.CanCopy())
                        effects |= DragDropEffects.Copy;

                    System.Windows.DragDrop.DoDragDrop(AssociatedObject, data, effects);
                }
            }
        }
    }
}
