//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Threading;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Behavior to auto expand TreeView items when the mouse is hovered over them
    /// during a drag and drop operation</summary>
    public class TreeViewDragHoverExpandBehavior : Behavior<TreeView>
    {
        /// <summary>
        /// Constructor</summary>
        public TreeViewDragHoverExpandBehavior()
        {
            m_timer = new DispatcherTimer(TimeSpan.FromMilliseconds(1000), DispatcherPriority.Normal, TimerElapsed, Dispatcher.CurrentDispatcher);
            m_timer.Stop();
        }

        /// <summary>
        /// Raises behavior Attached event and performs custom actions</summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.PreviewDragOver += AssociatedObject_PreviewDragOver;
            AssociatedObject.PreviewDragLeave += AssociatedObject_PreviewDragLeave;
        }

        /// <summary>
        /// Handle Detaching event and performs custom actions</summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.PreviewDragOver -= AssociatedObject_PreviewDragOver;
            AssociatedObject.PreviewDragLeave -= AssociatedObject_PreviewDragLeave;
        }

        /// <summary>
        /// Get or set time delay before expanding item</summary>
        public int ExpandDelay 
        {
            get { return m_timer.Interval.Milliseconds; }
            set { m_timer.Interval = TimeSpan.FromMilliseconds(value); } 
        }

        /// <summary>
        /// Performs custom actions on PreviewDragOver events of Behavior's AssociatedObject, a TreeViewItem</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">DragEventArgs containing event data</param>
        void AssociatedObject_PreviewDragOver(object sender, DragEventArgs e)
        {
            var pos = e.GetPosition(AssociatedObject);
            TreeViewItem tvi = AssociatedObject.GetItemContainerAtPoint(pos);

            if (tvi != m_lastHoveredItem)
            {
                m_timer.Stop();
                m_lastHoveredItem = tvi;

                if (m_lastHoveredItem != null && !m_lastHoveredItem.IsExpanded)
                    m_timer.Start();
            }
        }

        /// <summary>
        /// Performs custom actions on PreviewDragLeave events of Behavior's AssociatedObject, a TreeViewItem</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">DragEventArgs containing event data</param>
        void AssociatedObject_PreviewDragLeave(object sender, DragEventArgs e)
        {
            m_lastHoveredItem = null;
        }

        void TimerElapsed(object sender, EventArgs e)
        {
            m_timer.Stop();

            if (m_lastHoveredItem != null)
            {
                m_lastHoveredItem.IsExpanded = true;
                m_lastHoveredItem = null;
            }
        }

        private TreeViewItem m_lastHoveredItem;
        private DispatcherTimer m_timer;
    }
}
