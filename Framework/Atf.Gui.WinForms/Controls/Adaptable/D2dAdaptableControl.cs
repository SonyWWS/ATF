//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;

using Sce.Atf.Direct2D;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Adaptable control, a control with adapters (decorators). The adaptable control
    /// can be converted into any of its adapters using the IAdaptable.As method.</summary>
    public class D2dAdaptableControl : AdaptableControl, IPerformanceTarget
    {
        /// <summary>
        /// Constructor</summary>
        public D2dAdaptableControl()
        {
            DoubleBuffered = false;
            SetStyle(
               ControlStyles.ResizeRedraw |
               ControlStyles.AllPaintingInWmPaint |
               ControlStyles.Opaque |
               ControlStyles.UserPaint, true);

            // D2dHwndGraphics needs to be resized on size changed. Look at OnResize.
            m_d2dGraphics = D2dFactory.CreateD2dHwndGraphics(Handle);
        }

        /// <summary>
        /// Gets the Direct2D graphics device associated with this Control</summary>
        public D2dGraphics D2dGraphics
        {
            get { return m_d2dGraphics; }
        }

        /// <summary>
        /// Event that is raised when drawing on the Direct2D surface. Is raised after OnBeginDrawD2d
        /// and before OnEndDrawD2d. Listeners can use the D2dGraphics property to draw.</summary>
        public event EventHandler DrawingD2d;

        /// <summary>
        /// Gets or sets whether to suppress drawing</summary>
        /// <remarks>In few cases where changing large number of elements,
        /// it is important to suppress drawing. Otherwise the rendering control
        /// is updated per item--instead of per operation.</remarks>                
        public bool SuppressDraw
        {
            get;
            set;
        }

        /// <summary>
        /// Draws one complete 'frame' using Direct2D. Is equivalent to this Control receiving a Paint message.
        /// Raises the DrawingD2d event.</summary>
        public void DrawD2d()
        {
            if (SuppressDraw)
                return;

            OnBeginDrawD2d();
            OnDrawingD2d();
            OnEndDrawD2d();
        }

        /// <summary>
        /// Called to prepare the Direct2D graphics device for drawing, including clearing the back buffer
        /// render target and initializing the Transform property of the D2dGraphics instance.</summary>
        protected virtual void OnBeginDrawD2d()
        {
            D2dGraphics.BeginDraw();
            ITransformAdapter xform = this.As<ITransformAdapter>();
            D2dGraphics.Transform = xform == null ? Matrix3x2F.Identity : xform.Transform;
            D2dGraphics.Clear(BackColor);
            D2dGraphics.AntialiasMode = D2dAntialiasMode.Aliased;
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
        /// Performs custom actions before OnPaint is called</summary>
        /// <param name="e">Paint event args</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            DrawD2d();
        }

        /// <summary>
        /// Performs custom actions before OnPaintBackground is called</summary>
        /// <param name="pevent">Paint event args</param>
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Resize"></see> event</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data</param>
        protected override void OnResize(EventArgs e)
        {
            // Resize backbuffer to match control's client size.
            // Resize before calling base implementation.
            m_d2dGraphics.Resize(ClientSize);

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
                m_d2dGraphics.Dispose();
            }

            base.Dispose(disposing);
        }
        #region IPerformanceTarget Members

        /// <summary>
        /// Does the event whose duration is measured and then returns. Raising the
        /// EventOccurred event is optional.</summary>
        /// <example>For example, if we are monitoring the time it takes a window to redraw,
        /// this event would be the equivalent of Refresh() on a control.</example>
        void IPerformanceTarget.DoEvent()
        {
            DrawD2d();
        }

        /// <summary>
        /// Event that is raised on the object being measured to indicate that
        /// another timing sample should be taken, so that the number of events per second
        /// can be calculated.</summary>
        /// <remarks>For example, if we are monitoring the time it takes a window to redraw,
        /// this event might be raised for every Paint event on the control.</remarks>
        event EventHandler IPerformanceTarget.EventOccurred
        {
            add { DrawingD2d += value; }
            remove { DrawingD2d -= value; }
        }

        #endregion

        private readonly D2dHwndGraphics m_d2dGraphics;
    }
}
