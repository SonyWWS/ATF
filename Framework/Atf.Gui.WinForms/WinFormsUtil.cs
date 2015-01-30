//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf
{
    /// <summary>
    /// Windows Forms utilities.</summary>
    public static class WinFormsUtil
    {
        /// <summary>
        /// Returns whether the given rectangle, in screen coordinates, is visible in any screen's
        /// working area (the monitor's visible area minus task bars and docked windows)</summary>
        /// <param name="rect">Rectangle</param>
        /// <returns>Whether the given rectangle is visible</returns>
        public static bool IsOnScreen(Rectangle rect)
        {
            using (Region region = new Region())
            {
                region.MakeEmpty();
                foreach (Screen screen in Screen.AllScreens)
                    region.Union(screen.WorkingArea);

                rect.Inflate(-Margin, -Margin);
                return region.IsVisible(rect);
            }
        }

        /// <summary>
        /// Calculates the minimum distance squared between the starting rectangle and the target,
        /// or returns int.MaxValue if the target rectangle is not visible in the given direction</summary>
        /// <param name="startRect">Starting rectangle</param>
        /// <param name="arrow">The direction to look in. Must be Up, Right, Down, or Left.</param>
        /// <param name="targetRect">Destination rectangle, to be tested against</param>
        /// <returns>The distance minimum squared between the rectangles, or int.MaxValue</returns>
        public static int CalculateDistance(Rectangle startRect, Keys arrow, Rectangle targetRect)
        {
            // Transform the problem so that the appropriate two edges of the two rectangles
            //  are rotated to be parallel to the x-axis with the target having a greater y than
            //  the starting edge if it is in front of the starting edge. In all cases, 'left'
            //  will be <= 'right'. And startY <= targetY if the target is visible.
            int startLeft, startRight, startY;
            int targetLeft, targetRight, targetY, targetFarY;
            switch (arrow)
            {
                case Keys.Up:
                    startLeft = startRect.Left; startRight = startRect.Right; startY = -startRect.Top;
                    targetLeft = targetRect.Left; targetRight = targetRect.Right; targetY = -targetRect.Bottom;
                    targetFarY = -targetRect.Top;
                    break;
                case Keys.Right:
                    startLeft = startRect.Top; startRight = startRect.Bottom; startY = startRect.Right;
                    targetLeft = targetRect.Top; targetRight = targetRect.Bottom; targetY = targetRect.Left;
                    targetFarY = targetRect.Right;
                    break;
                case Keys.Down:
                    startLeft = startRect.Left; startRight = startRect.Right; startY = startRect.Bottom;
                    targetLeft = targetRect.Left; targetRight = targetRect.Right; targetY = targetRect.Top;
                    targetFarY = targetRect.Bottom;
                    break;
                case Keys.Left:
                    startLeft = startRect.Top; startRight = startRect.Bottom; startY = -startRect.Left;
                    targetLeft = targetRect.Top; targetRight = targetRect.Bottom; targetY = -targetRect.Right;
                    targetFarY = -targetRect.Right;
                    break;
                default:
                    throw new ArgumentException("'arrow' must be a single arrow key");
            }

            // Try to exclude the target from this quadrant.
            if (startY > targetFarY)
                return int.MaxValue;

            int farthestDistY = targetFarY - startY;
            if (targetRight < startLeft - farthestDistY ||
                targetLeft > startRight + farthestDistY)
                return int.MaxValue;

            // The target is definitely in this quadrant. Find the distance squared.
            int nearestDistY = targetY - startY;
            if (targetRight < startLeft)
            {
                int distX = startLeft - targetRight;
                return distX * distX + nearestDistY * nearestDistY;
            }
            if (targetLeft > startRight)
            {
                int distX = targetLeft - startRight;
                return distX * distX + nearestDistY * nearestDistY;
            }
            return nearestDistY * nearestDistY;
        }

        /// <summary>
        /// Gets the currently focused control</summary>
        /// <returns>The currently focused control</returns>
        public static Control GetFocusedControl()
        {
            Control focusedControl = null;
            // To get hold of the focused control:
            IntPtr focusedHandle = User32.GetFocus();
            if (focusedHandle != IntPtr.Zero)
                // Note that if the focused Control is not a .Net control, then this will return null.
                focusedControl = Control.FromHandle(focusedHandle);
            return focusedControl;
        }

        /// <summary>
        /// Performs the standard WinForms check for an illegal cross-thread call on a control.
        /// See: http://andyclymer.blogspot.com/2006/07/custom-controls-and-cross-_115218128668993214.html </summary>
        /// <param name="control">Control to be checked</param>
        /// <remarks>Throws InvalidOperationException exception if illegal cross-thread call.</remarks>
        /// <exception cref="InvalidOperationException">Illegal cross-thread call</exception>
        public static void CheckForIllegalCrossThreadCall(Control control)
        {
            if (Control.CheckForIllegalCrossThreadCalls &&
                control.InvokeRequired)
            {
                throw new InvalidOperationException("Illegal cross-thread call");
            }
        }

        /// <summary>
        /// Uses a Control's Invoke() method to call the given delegate, if necessary</summary>
        /// <param name="control">The Control that may have been created on a thread other than the current thread</param>
        /// <param name="action">The action to perform, e.g., () => { control.Refresh(); }</param>
        public static void InvokeIfRequired(Control control, Action action)
        {
            if (control.InvokeRequired)
                control.Invoke(action);
            else
                action();
        }

        /// <summary>
        /// Updates vertical and horizontal scrollbars to correspond to the current visible and canvas
        /// dimensions</summary>
        /// <param name="vScrollBar">Vertical scrollbar, or null if none</param>
        /// <param name="hScrollBar">Horizontal scrollbar, or null if none</param>
        /// <param name="visibleSize">Size of view of canvas</param>
        /// <param name="canvasSize">Size of canvas</param>
        public static void UpdateScrollbars(
            VScrollBar vScrollBar,
            HScrollBar hScrollBar,
            Size visibleSize,
            Size canvasSize)
        {
            if (vScrollBar != null)
            {
                int height = visibleSize.Height;
                if (hScrollBar != null && hScrollBar.Visible)
                    height -= hScrollBar.Height;
                height = Math.Max(1, height);

                int vScrollDistance = Math.Max(0, canvasSize.Height - height);

                vScrollBar.Visible = (vScrollDistance > 0);

                int largeChange = height;
                vScrollBar.LargeChange = largeChange;
                vScrollBar.Minimum = 0;
                int maximum = vScrollDistance + largeChange;
                vScrollBar.Maximum = maximum;
                vScrollBar.Value = Math.Min(vScrollBar.Value, vScrollDistance);
            }

            if (hScrollBar != null)
            {
                int width = visibleSize.Width;
                if (vScrollBar != null && vScrollBar.Visible)
                    width -= vScrollBar.Width;
                width = Math.Max(1, width);

                int hScrollDistance = Math.Max(0, canvasSize.Width - width);

                hScrollBar.Visible = (hScrollDistance > 0);

                int largeChange = width;
                hScrollBar.LargeChange = largeChange;
                hScrollBar.Minimum = 0;
                int maximum = hScrollDistance + largeChange;
                hScrollBar.Maximum = maximum;
                hScrollBar.Value = Math.Min(hScrollBar.Value, hScrollDistance);
            }
        }

        /// <summary>
        /// Updates vertical and horizontal scrollbars to correspond to the given visible and canvas
        /// areas. The scrollbars' Minimum and Maximum properties will be set to fit the union of the
        /// content and visible areas. Negative coordinates are supported. The Value property of the
        /// scrollbars will be automatically capped if necessary to be between Minimum and Maximum.</summary>
        /// <param name="vScrollBar">Vertical scrollbar, or null if none</param>
        /// <param name="hScrollBar">Horizontal scrollbar, or null if none</param>
        /// <param name="visibleArea">Size of view of canvas</param>
        /// <param name="contentArea">Size of canvas</param>
        /// <returns>The visible area, after considering scrollbar visibility</returns>
        public static Rectangle UpdateScrollbars(
            VScrollBar vScrollBar,
            HScrollBar hScrollBar,
            Rectangle visibleArea,
            Rectangle contentArea)
        {
            Rectangle canvas = Rectangle.Union(visibleArea, contentArea);

            int numPixelsOffScreenX = Math.Max(0, canvas.Width - visibleArea.Width);
            int numPixelsOffScreenY = Math.Max(0, canvas.Height - visibleArea.Height);

            // find out for sure if we need a horizontal scrollbar

            bool horizontalBarNeeded = (numPixelsOffScreenX > 0);
            // is the presence of the vertical bar going to make or break the need for the horizontal bar?
            if (vScrollBar != null && (contentArea.Right > visibleArea.Right - vScrollBar.Width))
            {
                // we're in the gray area for needing a horizontal scroll bar
                // check the vertical bar
                if (numPixelsOffScreenY > 0)
                    horizontalBarNeeded = true;
            }

            // Adjust the view space with the known scrollbar
            if (hScrollBar != null && horizontalBarNeeded)
            {
                visibleArea.Height -= hScrollBar.Height;
                canvas = Rectangle.Union(visibleArea, contentArea);
                numPixelsOffScreenY = Math.Max(0, canvas.Height - visibleArea.Height);
            }

            // now decide if we need vertical scrollbar
            if (vScrollBar != null)
            {
                if (numPixelsOffScreenY > 0)
                {
                    visibleArea.Width -= vScrollBar.Width;
                    canvas = Rectangle.Union(visibleArea, contentArea);
                    numPixelsOffScreenX = Math.Max(0, canvas.Width - visibleArea.Width);

                    vScrollBar.Visible = true;

                    vScrollBar.LargeChange = Math.Max(visibleArea.Height, 1);
                    vScrollBar.SmallChange = Math.Max(1, vScrollBar.LargeChange / 10);
                    vScrollBar.Minimum = canvas.Top;
                    vScrollBar.Maximum = canvas.Bottom;
                }
                else
                {
                    vScrollBar.Visible = false;
                }
            }

            // and now that the vertical bar has been decided, we can setup the horizontal properties correctly
            if (hScrollBar != null)
            {
                if (numPixelsOffScreenX > 0)
                {
                    hScrollBar.Visible = true;

                    hScrollBar.LargeChange = Math.Max(visibleArea.Width, 1);
                    hScrollBar.SmallChange = Math.Max(1, hScrollBar.LargeChange / 10);
                    hScrollBar.Minimum = canvas.Left;
                    hScrollBar.Maximum = canvas.Right;
                }
                else
                {
                    hScrollBar.Visible = false;
                }
            }

            return visibleArea;
        }

        /// <summary>
        /// Updates the specified parts of 'origRect'</summary>
        /// <param name="origRect">The original rectangle</param>
        /// <param name="newRect">The source for the parts specified in 'parts'</param>
        /// <param name="parts">Flags specifying which parts of 'origRect' should be updated</param>
        /// <returns>The updated rectangle</returns>
        public static Rectangle UpdateBounds(Rectangle origRect, Rectangle newRect, BoundsSpecified parts)
        {
            Rectangle updatedOriginal = origRect;

            if ((parts & BoundsSpecified.X) != 0)
                updatedOriginal.X = newRect.X;
            if ((parts & BoundsSpecified.Y) != 0)
                updatedOriginal.Y = newRect.Y;
            if ((parts & BoundsSpecified.Width) != 0)
                updatedOriginal.Width = newRect.Width;
            if ((parts & BoundsSpecified.Height) != 0)
                updatedOriginal.Height = newRect.Height;

            return updatedOriginal;
        }

        /// <summary>
        /// Delegate for the WindowCreated and WindowDestroyed events</summary>
        /// <param name="form">The form that was created or destroyed</param>
        public delegate void FormEventHandler(Form form);

        /// <summary>
        /// Event that is raised every time a top-level window is created by the current process.
        /// Can be subscribed to multiple times by the same event handler, but only one event will
        /// be raised per unique event handler.</summary>
        public static event FormEventHandler WindowCreated
        {
            add
            {
                if (!s_windowCreatedHandlers.Contains(value))
                    s_windowCreatedHandlers.Add(value);
                CheckShellProc();
            }
            remove
            {
                s_windowCreatedHandlers.Remove(value);
                CheckShellProc();
            }
        }

        /// <summary>
        /// Event that is raised every time a top-level window is destroyed by the current process.
        /// Can be subscribed to multiple times by the same event handler, but only one event will
        /// be raised per unique event handler.</summary>
        public static event FormEventHandler WindowDestroyed
        {
            add
            {
                if (!s_windowDestroyedHandlers.Contains(value))
                    s_windowDestroyedHandlers.Add(value);
                CheckShellProc();
            }
            remove
            {
                s_windowDestroyedHandlers.Remove(value);
                CheckShellProc();
            }
        }

        /// <summary>
        /// Event that is raised every time a managed Control is created by the current process.
        /// Can be subscribed to multiple times by the same event handler, but only one event will
        /// be raised per unique event handler.</summary>
        public static event ControlEventHandler ControlCreated
        {
            add
            {
                if (!s_controlCreatedHandlers.Contains(value))
                    s_controlCreatedHandlers.Add(value);
                CheckShellProc();
            }
            remove
            {
                s_controlCreatedHandlers.Remove(value);
                CheckShellProc();
            }
        }

        /// <summary>
        /// Event that is raised every time a top-level window is destroyed by the current process.
        /// Can be subscribed to multiple times by the same event handler, but only one event will
        /// be raised per unique event handler.</summary>
        public static event ControlEventHandler ControlDestroyed
        {
            add
            {
                if (!s_controlDestroyedHandlers.Contains(value))
                    s_controlDestroyedHandlers.Add(value);
                CheckShellProc();
            }
            remove
            {
                s_controlDestroyedHandlers.Remove(value);
                CheckShellProc();
            }
        }

        private static void OnWindowCreated(Form form)
        {
            foreach (FormEventHandler handler in s_windowCreatedHandlers)
                handler.Invoke(form);
        }

        private static void OnWindowDestroyed(Form form)
        {
            foreach (FormEventHandler handler in s_windowDestroyedHandlers)
                handler.Invoke(form);
        }

        private static void OnControlCreated(Control control)
        {
            foreach (ControlEventHandler handler in s_controlCreatedHandlers)
                handler.Invoke(null, new ControlEventArgs(control));
        }

        private static void OnControlDestroyed(Control control)
        {
            foreach (ControlEventHandler handler in s_controlDestroyedHandlers)
                handler.Invoke(null, new ControlEventArgs(control));
        }

        private static int NumShellListeners
        {
            get
            {
                return
                    s_windowCreatedHandlers.Count +
                    s_windowDestroyedHandlers.Count +
                    s_controlCreatedHandlers.Count +
                    s_controlDestroyedHandlers.Count;
            }
        }

        // Hooks or unhooks into the current desktop, depending on how many listeners there are.
        // Can be called multiple times, but will only create one hook.
        // Listeners must be added or removed first, and then call this method.
        private static void CheckShellProc()
        {
            if (s_windowsHookHandle == IntPtr.Zero &&
                NumShellListeners > 0)
            {
                #pragma warning disable 612,618
                int currentThreadId = AppDomain.GetCurrentThreadId();
                #pragma warning restore 612,618

                // The WH_SHELL hook only reports top-level unowned windows, so it won't catch all the various Forms
                //  that are created, like for user preferences, etc. The WH_CBT hook, for computer-based training
                //  programs, seems to work well enough.
                s_windowsHookHandle =
                    User32.SetWindowsHookEx(User32.HookType.WH_CBT, s_callbackDelegate, IntPtr.Zero, currentThreadId);
            }
            else if (s_windowsHookHandle != IntPtr.Zero &&
                NumShellListeners == 0)
            {
                User32.UnhookWindowsHookEx(s_windowsHookHandle);
                s_windowsHookHandle = IntPtr.Zero;
            }
        }

        private static int ShellHookCallback(int code, int wParam, int lParam)
        {
            int result = User32.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
            if (code < 0)
            {
                //We need to call CallNextHookEx without further processing
                //and return the value returned by CallNextHookEx
                return result;
            }

            if (code == (int)User32.CbtEvents.HCBT_CREATEWND)
            {
                // This event occurs before a Form object exists, so calling Control.FromHandle()
                //  always returns null (based on a small amount of testing). So, let's cache
                //  this handle and look for the Form later, so that we can raise the WindowCreated event.
                s_handlesCreatedBeforeForms.Add(wParam);
            }
            else if (code == (int)User32.CbtEvents.HCBT_DESTROYWND)
            {
                // Raise the WindowDestroyed event unless we never even got around to raising the
                //  WindowCreated event.
                if (!s_handlesCreatedBeforeForms.Remove(wParam))
                {
                    Control control = GetControlFromHandle(wParam);
                    if (control != null)
                    {
                        OnControlDestroyed(control);
                        var form = control as Form;
                        if (form != null)
                            OnWindowDestroyed(form);
                    }
                }
            }
            // This is a form of polling, waiting for the Form to be created so that we can raise
            //  the WindowCreated event. Is this the best way?
            else if (
                (code == (int)User32.CbtEvents.HCBT_ACTIVATE ||
                code == (int)User32.CbtEvents.HCBT_SETFOCUS) &&
                s_handlesCreatedBeforeForms.Count > 0)
            {
                // To avoid recursively modifying s_handlesCreatedBeforeForms, copy it first.
                int[] handleInts = s_handlesCreatedBeforeForms.ToArray();
                s_handlesCreatedBeforeForms.Clear();
                
                // Try to find the Forms.
                foreach (int handleInt in handleInts)
                {
                    Control control = GetControlFromHandle(handleInt);
                    if (control != null)
                    {
                        OnControlCreated(control);
                        var form = control as Form;
                        if (form != null)
                            OnWindowCreated(form);
                    }
                }
            }

            //return the value returned by CallNextHookEx
            return result;
        }

        private static Control GetControlFromHandle(int handleInt)
        {
            var handle = new IntPtr(handleInt);
            var control = Control.FromHandle(handle);

            // The following seems to be able to catch ToolTips which do not derive from Controls.
            //if (control == null)
            //{
            //    // The following gets practically all Controls created. Might be useful. No NativeWindow
            //    //  is ever found for the native file-open dialog box, created by the call to GetOpenFileName().
            //    //  MessageBox.Show() also never causes a managed Form or Control or NativeWindow to be created.
            //    NativeWindow nativeWindow = NativeWindow.FromHandle(handle);
            //    if (nativeWindow != null)
            //    {
            //        //It's difficult to pre-calculate this. Look for System.Windows.Forms.Control+ControlNativeWindow.
            //        // I think we'd have to search loaded assemblies for System.Windows.Forms, and then we could do a
            //        //  search for System.Windows.Forms.Control+ControlNativeWindow. Note that sometimes 'nativeWindow'
            //        //  is not actually a ControlNativeWindow (like for tooltips for some reason) and so 'field' can
            //        //  be null.
            //        FieldInfo field = nativeWindow.GetType().GetField(
            //            "control", BindingFlags.Instance | BindingFlags.NonPublic);
            //        if (field != null)
            //        {
            //            object controlObj = field.GetValue(nativeWindow);
            //            control = controlObj as Control;
            //        }
            //    }
            //}

            return control;
        }

        // What's the minimum # of pixels of the rectangle that have to be visible
        //  in order to count as being on-screen?
        private const int Margin = 10;

        private static readonly List<FormEventHandler> s_windowCreatedHandlers = new List<FormEventHandler>();
        private static readonly List<FormEventHandler> s_windowDestroyedHandlers = new List<FormEventHandler>();
        private static readonly List<ControlEventHandler> s_controlCreatedHandlers = new List<ControlEventHandler>();
        private static readonly List<ControlEventHandler> s_controlDestroyedHandlers = new List<ControlEventHandler>();
        private static readonly List<int> s_handlesCreatedBeforeForms = new List<int>();
        
        //We need to prevent this delegate from being garbage collected because only native code calls it
        private static readonly User32.WindowsHookCallback s_callbackDelegate = ShellHookCallback;
        private static IntPtr s_windowsHookHandle = IntPtr.Zero;
    }
}
