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
            //UpdateScrollbars(); //leads to endless updating
            
            // update canvas WindowBounds to account for scrollbars
            Rectangle windowBounds = AdaptedControl.ClientRectangle;
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
                m_updatingScrollbars = true;
                try
                {
                    m_transformAdapter.Translation = new PointF(m_transformAdapter.Translation.X, -m_vScrollBar.Value);
                }
                finally
                {
                    m_updatingScrollbars = false;
                }
            }

            OnScroll(EventArgs.Empty);

            AdaptedControl.Refresh();
        }

        private void hScrollBar_ValueChanged(object sender, EventArgs e)
        {
            if (!m_updatingScrollbars)
            {
                m_updatingScrollbars = true;
                try
                {
                    m_transformAdapter.Translation = new PointF(-m_hScrollBar.Value, m_transformAdapter.Translation.Y);
                }
                finally
                {
                    m_updatingScrollbars = false;
                }
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

        private void UpdateScrollbars()
        {
            if (m_updatingScrollbars)
                return;

            try
            {
                m_updatingScrollbars = true;

                // get view rectangle in Windows client coordinates
                Rectangle viewRect = AdaptedControl.ClientRectangle;

                // get canvas bounds in Windows client coordinates
                Rectangle canvasRect = m_canvasAdapter.Bounds; // start with world coordinates
                PointF scale = m_transformAdapter.Scale;

                // Calculate the negation of the translation, by allowing the translation to range far
                //  enough so that the edge of the canvas can get to the center of the screen.
                int minTransX = (int)(canvasRect.X * scale.X - viewRect.Width * 0.5f);
                int minTransY = (int)(canvasRect.Y * scale.Y - viewRect.Height * 0.5f);
                int maxTransX = (int)(canvasRect.Right * scale.X - viewRect.Width * 0.5f);
                int maxTransY = (int)(canvasRect.Bottom * scale.Y - viewRect.Height * 0.5f);
                canvasRect = new Rectangle(minTransX, minTransY, maxTransX - minTransX, maxTransY - minTransY);

                WinFormsUtil.UpdateScrollbars(
                    m_vScrollBar,
                    m_hScrollBar,
                    viewRect,
                    canvasRect);

                // Set the scrollbars' values to be the negation of the translation.
                if (!m_hScrollBar.Capture && !m_vScrollBar.Capture)
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
        }

        private readonly ITransformAdapter m_transformAdapter;
        private readonly ICanvasAdapter m_canvasAdapter;

        private readonly VScrollBar m_vScrollBar;
        private readonly HScrollBar m_hScrollBar;
        private bool m_updatingScrollbars;
    }
}
