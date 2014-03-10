//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Threading;

using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// More complex version of standard double click to edit label behavior.
    /// This one requires a single click on a focused item
    /// but waits for a set time to check that no double click occurs.</summary>
    public class TreeViewItemLabelEditBehavior : Behavior<TreeViewItem>
    {
        /// <summary>
        /// Raises behavior Attached event and performs custom actions</summary>
        protected override void OnAttached()
        {
            AssociatedObject.PreviewMouseDown += new MouseButtonEventHandler(AssociatedObject_PreviewMouseDown);
            AssociatedObject.PreviewMouseUp += new MouseButtonEventHandler(AssociatedObject_PreviewMouseUp);
            AssociatedObject.LostFocus += new RoutedEventHandler(AssociatedObject_LostFocus);
            AssociatedObject.MouseDoubleClick += new MouseButtonEventHandler(AssociatedObject_MouseDoubleClick);
        }

        /// <summary>
        /// Performs custom actions on MouseDoubleClick events of Behavior's AssociatedObject, a TreeViewItem</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">MouseButtonEventArgs containing event data</param>
        void AssociatedObject_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            StopTimer();
        }

        /// <summary>
        /// Performs custom actions on LostFocus events of Behavior's AssociatedObject, a TreeViewItem</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">RoutedEventArgs containing event data</param>
        void AssociatedObject_LostFocus(object sender, RoutedEventArgs e)
        {
            StopTimer();
        }

        /// <summary>
        /// Performs custom actions on PreviewMouseDown events of Behavior's AssociatedObject, a TreeViewItem</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">RoutedEventArgs containing event data</param>
        void AssociatedObject_PreviewMouseDown(object sender, RoutedEventArgs e)
        {
            // Kind of a hack - assumes the precence of a TextBlock in the TreeViewItem data template
            if (e.OriginalSource.GetType() == typeof(TextBlock))
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed && AssociatedObject.IsFocused)
                {
                    m_mouseDown = true;
                }
                else
                {
                    StopTimer();
                }
            }
        }

        private bool m_mouseDown;

        /// <summary>
        /// Performs custom actions on PreviewMouseUp events of Behavior's AssociatedObject, a TreeViewItem</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">MouseButtonEventArgs containing event data</param>
        void AssociatedObject_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (m_mouseDown == true)
            {
                m_mouseDown = false;

                if (AssociatedObject.IsMouseOver && AssociatedObject.IsFocused)
                {
                    if (m_timer == null)
                    {
                        m_timer = new DispatcherTimer();
                        m_timer.Interval = TimeSpan.FromMilliseconds(s_doubleClickTime + 100);
                        m_timer.Tick += new EventHandler(Tick);
                        m_timer.Start();
                    }
                }
                else
                {
                    StopTimer();
                }
            }
        }

        /// <summary>
        /// Method called after first click and waiting to check that no double click occurs</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">EventArgs containing event data</param>
        void Tick(object sender, EventArgs e)
        {
            StopTimer();

            if (AssociatedObject.IsFocused && AssociatedObject.IsMouseOver)
            {
                var data = AssociatedObject.DataContext as Node;
                if (data != null)
                    data.IsInLabelEditMode = true;
            }
        }

        private void StopTimer()
        {
            // Stop timer
            if (m_timer != null)
            {
                m_timer.Tick -= Tick;
                m_timer.Stop();
                m_timer = null;
            }
        }

        private DispatcherTimer m_timer;

        private static uint s_doubleClickTime = GetDoubleClickTime();

        [DllImport("user32.dll")]
        static extern uint GetDoubleClickTime();
    }
}
