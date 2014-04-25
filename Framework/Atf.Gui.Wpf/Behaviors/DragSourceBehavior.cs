//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Base drag source behavior</summary>
    /// <typeparam name="T">Owner type</typeparam>
    public abstract class DragSourceBehavior<T> : AdaptableBehavior<T>
        where T : FrameworkElement
    {
        /// <summary>
        /// Performs custom processing on BeginDrag event</summary>
        /// <param name="e">A MouseEventArgs that contains the event data</param>
        protected abstract void BeginDrag(MouseEventArgs e);

        #region Overrides

        /// <summary>
        /// Raises behavior Attached event and performs custom actions</summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDown);
            AssociatedObject.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(OnMouseLeftButtonUp);
            AssociatedObject.MouseMove += new MouseEventHandler(OnMouseMove);

            System.Windows.DragDrop.AddPreviewQueryContinueDragHandler(AssociatedObject, new QueryContinueDragEventHandler(OnQueryContinueDrag));
        }

        #endregion

        #region Event Handlers

        void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject original = e.OriginalSource as DependencyObject;

            // Ignore drag operations if the mouse is inside a range control( e.g., Scrollbar, ProgressBar, Slider, etc.) 
            if (original.FindAncestor<RangeBase>() != null)
                return;

            m_dragStartPoint = new Point?(e.GetPosition(AssociatedObject));
        }

        void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            m_dragStartPoint = null;
        }

        void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (m_dragStartPoint.HasValue)
            {
                var pos = e.GetPosition(AssociatedObject);
                if (IsDragStarted(pos))
                {
                    BeginDrag(e);
                    m_dragStartPoint = null;
                    e.Handled = true;
                }
            }
        }

        void OnQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (e.EscapePressed)
            {
                e.Action = DragAction.Cancel;
                m_dragStartPoint = null;
                e.Handled = true;
            }
        }
               
        #endregion

        private bool IsDragStarted(Point pos)
        {
            bool hGesture = Math.Abs(pos.X - m_dragStartPoint.Value.X) > SystemParameters.MinimumHorizontalDragDistance;
            bool vGesture = Math.Abs(pos.Y - m_dragStartPoint.Value.Y) > SystemParameters.MinimumVerticalDragDistance;
            return hGesture | vGesture;
        }

        private Point? m_dragStartPoint = null;

    }
}
