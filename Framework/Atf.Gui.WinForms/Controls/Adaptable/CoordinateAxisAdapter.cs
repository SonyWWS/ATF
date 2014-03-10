//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Adapter that renders horizontal and vertical coordinate axes</summary>
    public class CoordinateAxisAdapter : ControlAdapter
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="theme">Diagram theme</param>
        public CoordinateAxisAdapter(DiagramTheme theme)
        {
            m_theme = theme;
        }

        /// <summary>
        /// Gets or sets whether the horizontal axis is visible</summary>
        public bool HorizontalVisible
        {
            get { return m_horizontalVisible; }
            set
            {
                if (m_horizontalVisible != value)
                {
                    m_horizontalVisible = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the vertical axis is visible</summary>
        public bool VerticalVisible
        {
            get { return m_verticalVisible; }
            set
            {
                if (m_verticalVisible != value)
                {
                    m_verticalVisible = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the horizontal grid step size</summary>
        [DefaultValue(64)]
        public int HorizontalTickSpacing
        {
            get { return m_horizontalTickSpacing; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException();
                m_horizontalTickSpacing = value;

                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the vertical grid step size</summary>
        [DefaultValue(64)]
        public int VerticalTickSpacing
        {
            get { return m_verticalTickSpacing; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException();
                m_verticalTickSpacing = value;

                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the minimum spacing, in graph coordinates, between ticks. For example,
        /// 1.0f would prevent ticks from being drawn for fractional graph coordinates. The
        /// default is 1.0f.</summary>
        [DefaultValue(1.0f)]
        public float MinimumGraphStep
        {
            get { return m_minimumGraphStep; }
            set
            {
                if (m_minimumGraphStep != value)
                {
                    m_minimumGraphStep = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Binds the adapter to the adaptable control. Called in the order that the adapters
        /// were defined on the control.</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Bind(AdaptableControl control)
        {
            m_transformAdapter = control.As<ITransformAdapter>();
            m_canvasAdapter = control.As<ICanvasAdapter>();

            control.Paint += control_Paint;
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            control.Paint -= control_Paint;
        }

        private void control_Paint(object sender, PaintEventArgs e)
        {
            Matrix transform = new Matrix();
            if (m_transformAdapter != null)
                transform = m_transformAdapter.Transform;

            RectangleF clientRect = AdaptedControl.ClientRectangle;
            if (m_canvasAdapter != null)
                clientRect = m_canvasAdapter.WindowBounds;

            RectangleF canvasRect = GdiUtil.InverseTransform(transform, clientRect);

            if (m_horizontalVisible)
            {
                ChartUtil.DrawHorizontalScale(
                    transform, canvasRect, false, m_verticalTickSpacing, 0, m_theme.OutlinePen, m_theme.Font, m_theme.TextBrush, e.Graphics);
            }

            if (m_verticalVisible)
            {
                ChartUtil.DrawVerticalScale(
                    transform, canvasRect, true, m_horizontalTickSpacing, 0, m_theme.OutlinePen, m_theme.Font, m_theme.TextBrush, e.Graphics);
            }
        }

        private void Invalidate()
        {
            if (AdaptedControl != null)
                AdaptedControl.Invalidate();
        }

        private readonly DiagramTheme m_theme;
        private ITransformAdapter m_transformAdapter;
        private ICanvasAdapter m_canvasAdapter;
        private int m_horizontalTickSpacing = 64;
        private int m_verticalTickSpacing = 64;
        private float m_minimumGraphStep = 1.0f;
        private bool m_horizontalVisible = true;
        private bool m_verticalVisible = true;
    }
}
