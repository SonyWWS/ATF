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
    public class LastHitAwareBehavior : Behavior<ItemsControl>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseDown += AssociatedObject_MouseDown;
            AssociatedObject.MouseUp += AssociatedObject_MouseUp;
            AssociatedObject.DragOver += AssociatedObject_DragOver;
            AssociatedObject.PreviewQueryContinueDrag += AssociatedObject_PreviewQueryContinueDrag;
        }

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

    public class MouseUtilities
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref Win32Point pt);

        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(IntPtr hwnd, ref Win32Point pt);

        public static Point CorrectGetPosition(Visual relativeTo)
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return relativeTo.PointFromScreen(new Point(w32Mouse.X, w32Mouse.Y));
        }
    }
}
