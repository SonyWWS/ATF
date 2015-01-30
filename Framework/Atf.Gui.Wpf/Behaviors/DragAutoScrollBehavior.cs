//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Behavior to auto scroll when dragging near edge of the element, e.g., for a listview.
    /// Can be attached to any framework element, but element must contain a scrollviewer
    /// in its visual tree for this behavior to function.</summary>
    public class DragAutoScrollBehavior : Behavior<FrameworkElement>
    {
        /// <summary>
        /// Constructor</summary>
        public DragAutoScrollBehavior()
        {
            Tolerance = 10;
            Offset = 10;
            CanAutoScrollY = true;
        }

        /// <summary>
        /// Gets or sets scroll tolerance - pixels from edge of element that trigger a scroll</summary>
        public int Tolerance { get; set; }

        /// <summary>
        /// Gets or sets scroll speed - pixels per call to DragOver</summary>
        public int Offset { get; set; }

        /// <summary>
        /// Gets or sets whether to enable x scrolling (false by default)</summary>
        public bool CanAutoScrollX { get; set; }

        /// <summary>
        /// Gets or sets whether to enable y scrolling (true by default)</summary>
        public bool CanAutoScrollY { get; set; }

        /// <summary>
        /// Performs custom actions on behavior Attached event</summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            System.Windows.DragDrop.AddPreviewQueryContinueDragHandler(AssociatedObject, OnQueryContinueDrag);
        }

        /// <summary>
        /// Handle Detaching event</summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            System.Windows.DragDrop.RemovePreviewQueryContinueDragHandler(AssociatedObject, OnQueryContinueDrag);
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            m_scrollViewer = AssociatedObject.GetFrameworkElementByType<ScrollViewer>();
        }

        void OnQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (m_scrollViewer == null)
                return;
            
            var pos = MouseUtilities.CorrectGetPosition(AssociatedObject);

            if (CanAutoScrollY)
            {
                if (pos.Y < Tolerance)
                {
                    m_scrollViewer.ScrollToVerticalOffset(m_scrollViewer.VerticalOffset - Offset);
                }
                else if (pos.Y > AssociatedObject.ActualHeight - Tolerance)
                {
                    m_scrollViewer.ScrollToVerticalOffset(m_scrollViewer.VerticalOffset + Offset);
                }
            }

            if (CanAutoScrollX)
            {
                if (pos.X < Tolerance)
                {
                    m_scrollViewer.ScrollToHorizontalOffset(m_scrollViewer.HorizontalOffset - Offset);
                }
                else if (pos.X > AssociatedObject.ActualWidth - Tolerance)
                {
                    m_scrollViewer.ScrollToHorizontalOffset(m_scrollViewer.HorizontalOffset + Offset);
                }
            }
        }

        private ScrollViewer m_scrollViewer;
    }
}
