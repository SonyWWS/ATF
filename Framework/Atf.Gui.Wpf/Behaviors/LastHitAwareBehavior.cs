//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Behavior for last item hit</summary>
    public class LastHitAwareBehavior : Behavior<ItemsControl>
    {
        /// <summary>
        /// Handle Attached event by subscribing to mouse events</summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseDown += AssociatedObject_MouseDown;
            AssociatedObject.MouseUp += AssociatedObject_MouseUp;
            AssociatedObject.DragOver += AssociatedObject_DragOver;
            AssociatedObject.PreviewQueryContinueDrag += AssociatedObject_PreviewQueryContinueDrag;
        }

        /// <summary>
        /// Handle Detaching event by unsubscribing from mouse events</summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
            AssociatedObject.MouseUp -= AssociatedObject_MouseUp;
            AssociatedObject.DragOver -= AssociatedObject_DragOver;
            AssociatedObject.PreviewQueryContinueDrag -= AssociatedObject_PreviewQueryContinueDrag;
        }

        void AssociatedObject_PreviewQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            SetLastHit();
        }

        void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
            SetLastHit();
        }

        void AssociatedObject_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetLastHit();
        }

        void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetLastHit();
        }

        private void SetLastHit()
        {
            var ctx = AssociatedObject.DataContext.As<ILastHitAware>();
            if (ctx != null)
                ctx.LastHit = AssociatedObject.GetItemAtMousePoint();
        }
    }

    /// <summary>
    /// Mouse utility functions</summary>
    public class MouseUtilities
    {
        /// <summary>
        /// Move the cursor to specified screen coordinates. For more details, see
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms648394%28v=vs.85%29.aspx .</summary>
        /// <param name="x">X screen coordinate to move cursor to</param>
        /// <param name="y">Y screen coordinate to move cursor to</param>
        /// <returns>Nonzero iff successful</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref User32.POINT pt);

        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(IntPtr hwnd, ref User32.POINT pt);

        /// <summary>
        /// Get corrected cursor coordinates relative to Visual</summary>
        /// <param name="relativeTo">Visual to get coordinates relative to</param>
        /// <returns>Cursor position in current coordinate system of the Visual</returns>
        public static Point CorrectGetPosition(Visual relativeTo)
        {
            var w32Mouse = new User32.POINT();
            GetCursorPos(ref w32Mouse);
            return relativeTo.PointFromScreen(new Point(w32Mouse.X, w32Mouse.Y));
        }
    }
}
