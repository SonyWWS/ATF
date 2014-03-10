//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Adapter that adds horizontal and vertical scrollbars to the adapted control.
    /// Requires an ITransformAdapter and ICanvasAdapter.</summary>
    public class ScrollbarAdapter : ControlAdapter
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="transformAdapter">Transform adapter</param>
        /// <param name="canvasAdapter">Canvas adapter</param>
        public ScrollbarAdapter(ITransformAdapter transformAdapter, ICanvasAdapter canvasAdapter)
        {
            m_transformAdapter = transformAdapter;
            m_transformAdapter.TransformChanged += transformAdapter_TransformChanged;

            m_canvasAdapter = canvasAdapter;
            m_canvasAdapter.BoundsChanged += canvasAdapter_BoundsChanged;
            m_canvasAdapter.WindowBoundsChanged += canvasAdapter_WindowBoundsChanged;

            m_vScrollBar = new VScrollBar();
            m_vScrollBar.Dock = DockStyle.Right;
            m_vScrollBar.ValueChanged += vScrollBar_ValueChanged;

            m_hScrollBar = new HScrollBar();
            m_hScrollBar.Dock = DockStyle.Bottom;
            m_hScrollBar.ValueChanged += hScrollBar_ValueChanged;
        }

        /// <summary>
        /// Binds the adapter to the adaptable control. Called in the order that the adapters
        /// were defined on the control.</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Bind(AdaptableControl control)
        {
            control.Painting += control_BeforePaint;

            control.Controls.Add(m_vScrollBar);
            control.Controls.Add(m_hScrollBar);
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            control.Painting -= control_BeforePaint;

            control.Controls.Remove(m_vScrollBar);
            control.Controls.Remove(m_hScrollBar);
        }

        /// <summary>
        /// Callback to update the scrollbars when the transform changes</summary>
        private void transformAdapter_TransformChanged(object sender, EventArgs e)
        {
            UpdateScrollbars();
        }

        private void canvasAdapter_WindowBoundsChanged(object sender, EventArgs e)
        {
            UpdateScrollbars();
        }

        private void canvasAdapter_BoundsChanged(object sender, EventArgs e)
        {
            UpdateScrollbars();
        }

        private void control_BeforePaint(object sender, EventArgs e)
        {
            Rectangle windowBounds = UpdateScrollbars();

            // update canvas WindowBounds to account for scrollbars
            Rectangle canvasWindowBounds = m_canvasAdapter.WindowBounds;
            if (m_hScrollBar.Enabled)
                windowBounds.Height = Math.Min(canvasWindowBounds.Height, windowBounds.Height - m_hScrollBar.Height);
            if (m_vScrollBar.Enabled)
                windowBounds.Width = Math.Min(canvasWindowBounds.Width, windowBounds.Width - m_vScrollBar.Width);

            m_canvasAdapter.WindowBounds = windowBounds;
        }

        private void vScrollBar_ValueChanged(object sender, EventArgs e)
        {
            if (!m_updatingScrollbars)
            {
                m_scroll.Y = -m_vScrollBar.Value;
                m_transformAdapter.Translation = new PointF(m_transformAdapter.Translation.X, m_scroll.Y);
            }

            OnScroll(EventArgs.Empty);

            AdaptedControl.Refresh();
        }

        private void hScrollBar_ValueChanged(object sender, EventArgs e)
        {
            if (!m_updatingScrollbars)
            {
                m_scroll.X = -m_hScrollBar.Value;
                m_transformAdapter.Translation = new PointF(m_scroll.X, m_transformAdapter.Translation.Y);
            }

            OnScroll(EventArgs.Empty);

            AdaptedControl.Refresh();
        }

        /// <summary>
        /// Performs custom actions after scrolling</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnScroll(EventArgs e)
        {
        }

        private Rectangle UpdateScrollbars()
        {
            // get canvas size, in client coordinates
            Size canvasSize = m_canvasAdapter.Bounds.Size;
            PointF scale = m_transformAdapter.Scale;
            Size canvasSizeInPixels = new Size(
                (int)(canvasSize.Width * scale.X),
                (int)(canvasSize.Height * scale.Y));

            // get window bounds to allow for scrollbars without obscuring any of canvas
            Rectangle windowBounds = AdaptedControl.ClientRectangle;

            try
            {
                m_updatingScrollbars = true;

                WinFormsUtil.UpdateScrollbars(
                    m_vScrollBar,
                    m_hScrollBar,
                    windowBounds.Size,
                    canvasSizeInPixels);

                if (!m_hScrollBar.Capture &&
                    !m_vScrollBar.Capture)
                {
                    PointF translation = m_transformAdapter.Translation;
                    m_hScrollBar.Value = Math.Min(Math.Max(m_hScrollBar.Minimum, -(int)translation.X), m_hScrollBar.Maximum);
                    m_vScrollBar.Value = Math.Min(Math.Max(m_vScrollBar.Minimum, -(int)translation.Y), m_vScrollBar.Maximum);
                }
            }
            finally
            {
                m_updatingScrollbars = false;
            }

            return windowBounds;
        }

        private readonly ITransformAdapter m_transformAdapter;
        private readonly ICanvasAdapter m_canvasAdapter;

        private Point m_scroll; //the translation in Transform which is also the negative scrollbar values.
        private readonly VScrollBar m_vScrollBar;
        private readonly HScrollBar m_hScrollBar;
        private bool m_updatingScrollbars;
    }
}
