//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Simple implementation of auto-translate adapter, which tracks the mouse when enabled
    /// and adjusts the transform adapter</summary>
    public class AutoTranslateAdapter : ControlAdapter, IAutoTranslateAdapter, IDisposable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="transformAdapter">Transform adapter</param>
        public AutoTranslateAdapter(ITransformAdapter transformAdapter)
        {
            m_transformAdapter = transformAdapter;

            m_timer = new Timer();
            m_timer.Interval = 10;
            m_timer.Tick += timer_Tick;
        }

        /// <summary>
        /// Disposes of unmanaged resources</summary>
        public void Dispose()
        {
            m_timer.Dispose();
        }

        #region IAutoTranslateAdapter Members

        /// <summary>
        /// Gets or sets a value indicating if auto-translation is enabled</summary>
        public bool Enabled
        {
            get { return m_enabled; }
            set { m_enabled = value; }
        }

        #endregion

        /// <summary>
        /// Binds the adapter to the adaptable control. Called in the reverse order that the adapters
        /// were defined on the control.</summary>
        /// <param name="control">Adaptable control</param>
        protected override void BindReverse(AdaptableControl control)
        {
            control.MouseMove += control_MouseMove;
            control.MouseUp += control_MouseUp;
            control.MouseLeave += control_MouseLeave;
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            control.MouseMove -= control_MouseMove;
            control.MouseUp -= control_MouseUp;
            control.MouseLeave -= control_MouseLeave;
        }

        private void control_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_enabled)
            {
                m_mousePosition = new Point(e.X, e.Y);
                if (!AdaptedControl.ClientRectangle.Contains(m_mousePosition) &&
                    !m_timer.Enabled)
                {
                    m_timer.Start();
                }
            }
        }

        private void control_MouseUp(object sender, MouseEventArgs e)
        {
            m_timer.Stop();
        }

        private void control_MouseLeave(object sender, EventArgs e)
        {
            m_timer.Stop();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            m_timer.Stop();

            if (!AdaptedControl.ClientRectangle.Contains(m_mousePosition))
            {
                const int Speed = 10;

                int dx = 0;
                if (m_mousePosition.X < 0)
                    dx = Speed;
                else if (m_mousePosition.X > AdaptedControl.Width)
                    dx = -Speed;

                int dy = 0;
                if (m_mousePosition.Y < 0)
                    dy = Speed;
                else if (m_mousePosition.Y > AdaptedControl.Height)
                    dy = -Speed;

                PointF translation = m_transformAdapter.Translation;
                m_transformAdapter.Translation = new PointF(translation.X + dx, translation.Y + dy);

                m_timer.Start();
            }
        }

        private readonly ITransformAdapter m_transformAdapter;
        private readonly Timer m_timer;
        private Point m_mousePosition;
        private bool m_enabled;
    }
}
