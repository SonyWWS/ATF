//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;

namespace Sce.Atf.Input
{
    /// <summary>
    /// Provides data for the MouseUp, MouseDown, and MouseMove events</summary>
    public class MouseEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        protected MouseEventArgs() { }

        /// <summary>
        /// Constructor with parameters</summary>
        /// <param name="button">Mouse button</param>
        /// <param name="clicks">Number of times button was pressed and released</param>
        /// <param name="x">X-coordinate</param>
        /// <param name="y">Y-coordinate</param>
        /// <param name="delta">Signed count of the number of detents the mouse wheel has rotated</param>
        public MouseEventArgs(MouseButtons button, int clicks, int x, int y, int delta)
        {
            Button = button;
            Clicks = clicks;
            X = x;
            Y = y;
            Delta = delta;
        }

        /// <summary> 
        /// Gets which mouse button was pressed</summary>
        public MouseButtons Button
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the number of times the mouse button was pressed and released</summary> 
        public int Clicks
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the x-coordinate of a mouse click</summary>
        public int X
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the y-coordinate of a mouse click</summary>
        public int Y
        {
            get;
            private set;
        }

        /// <summary> 
        /// Gets a signed count of the number of detents the mouse wheel has rotated</summary> 
        public int Delta
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the point location of the mouse during the event</summary>
        public Point Location
        {
            get { return new Point(X, Y); }
        }
    }
}