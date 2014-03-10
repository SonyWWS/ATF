//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;

using Sce.Atf.VectorMath;

namespace Sce.Atf.Direct2D
{
    /// <summary>
    /// Control for Direct2D. Consider using D2dAdaptableControl instead.</summary>
    public class Direct2DControl : Control
    {
        /// <summary>
        /// Constructor</summary>
        public Direct2DControl()
        {
            SetStyle(
                ControlStyles.ResizeRedraw |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.Opaque |
                ControlStyles.UserPaint, true);

            // D2dHwndGraphics needs to be resized on size changed. Look at OnResize.
            D2dGraphics = D2dFactory.CreateD2dHwndGraphics(Handle);
        }

        /// <summary>
        /// Gets the Direct2D graphics device associated with this Control</summary>
        public D2dHwndGraphics D2dGraphics { get; private set; }

        /// <summary>
        /// Draws one complete 'frame' using Direct2D. Is equivalent to this Control receiving a Paint message.</summary>
        public void DrawD2d()
        {
            OnBeginDrawD2d();
            OnDrawingD2d();
            OnEndDrawD2d();
        }

        /// <summary>
        /// Event that is raised when drawing on the Direct2D surface. Is raised after OnBeginDrawD2d
        /// and before OnEndDrawD2d.</summary>
        public event EventHandler DrawingD2d;

        /// <summary>
        /// Called to prepare the Direct2D graphics device for drawing, including clearing the back buffer
        /// render target and initializing the Transform property of D2dGraphics.</summary>
        protected virtual void OnBeginDrawD2d()
        {
            D2dGraphics.BeginDraw();
            D2dGraphics.Transform = Matrix3x2F.Identity;
            D2dGraphics.Clear(BackColor);
        }

        /// <summary>
        /// Does the DrawingD2d drawing. Raises the DrawingD2d event.</summary>
        protected virtual void OnDrawingD2d()
        {
            DrawingD2d.Raise(this, EventArgs.Empty);
        }

        /// <summary>
        /// Ends drawing the current frame to the Direct2D back buffer render target</summary>
        protected virtual void OnEndDrawD2d()
        {
            D2dGraphics.EndDraw();
        }

        /// <summary>
        /// Performs custom actions on Paint event</summary>
        /// <param name="e">Paint event args</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            DrawD2d();
        }

        /// <summary>
        /// Performs custom actions on Resize event</summary>
        /// <param name="e">Event args</param>
        protected override void OnResize(EventArgs e)
        {
            // Resize backbuffer to match control's client size.
            // Resize before calling base implementation.
            D2dGraphics.Resize(ClientSize);

            base.OnResize(e);
        }

        /// <summary>
        /// Disposes of resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                D2dGraphics.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
