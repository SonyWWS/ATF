//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Basic implementation of a canvas adapter, which defines the canvas
    /// bounds and visible window size</summary>
    public class CanvasAdapter : ControlAdapter, ICanvasAdapter, ILayoutConstraint
    {
        /// <summary>
        /// Constructor</summary>
        public CanvasAdapter()
        {
        }

        /// <summary>
        /// Constructor with canvas bounds</summary>
        /// <param name="bounds">Canvas bounds</param>
        public CanvasAdapter(Rectangle bounds)
        {
            m_bounds = bounds;
        }

        /// <summary>
        /// Callback to custom update translation maximum and minimum</summary>
        public Action<CanvasAdapter> UpdateTranslateMinMax; 

        /// <summary>
        /// Gets or sets the canvas bounds</summary>
        public Rectangle Bounds
        {
            get { return m_bounds; }
            set
            {
                if (m_bounds != value)
                {
                    m_bounds = value;

                    UpdateTranslationConstraints();

                    OnBoundsChanged(EventArgs.Empty);
                    BoundsChanged.Raise(this, EventArgs.Empty);

                    AdaptedControl.Invalidate();
                }
            }
        }

        /// <summary>
        /// Event that is raised after the canvas bounds change</summary>
        public event EventHandler BoundsChanged;

        /// <summary>
        /// Performs custom actions after the bounds change</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnBoundsChanged(EventArgs e)
        {
        }

        /// <summary>
        /// Gets or sets the window's bounding rectangle, which defines the visible
        /// area of the canvas</summary>
        public Rectangle WindowBounds
        {
            get { return m_windowBounds; }
            set
            {
                if (m_windowBounds != value)
                {
                    m_windowBounds = value;

                    UpdateTranslationConstraints();

                    OnWindowBoundsChanged(EventArgs.Empty);
                    WindowBoundsChanged.Raise(this, EventArgs.Empty);

                    AdaptedControl.Invalidate();
                }
            }
        }

        /// <summary>
        /// Event that is raised after the window bounds change</summary>
        public event EventHandler WindowBoundsChanged;

        /// <summary>
        /// Performs custom actions after the window bounds change</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnWindowBoundsChanged(EventArgs e)
        {
        }

        private void UpdateTranslationConstraints()
        {
            if (m_transformAdapter != null)
            {
                if (UpdateTranslateMinMax != null)
                {
                    UpdateTranslateMinMax(this);
                    return;
                }

                m_scale = m_transformAdapter.Scale;

                RectangleF boundsInPixels = new RectangleF(
                    m_bounds.X * m_scale.X,
                    m_bounds.Y * m_scale.Y,
                    m_bounds.Width * m_scale.X,
                    m_bounds.Height * m_scale.Y);
                m_transformAdapter.MaxTranslation = new PointF(
                    boundsInPixels.X,
                    boundsInPixels.Y);
                m_transformAdapter.MinTranslation = new PointF(
                    -Math.Max(0, boundsInPixels.Width - m_windowBounds.Width),
                    -Math.Max(0, boundsInPixels.Height - m_windowBounds.Height));
            }
        }

        #region ILayoutConstraint Members

        /// <summary>
        /// Gets the displayable name of the constraint</summary>
        string ILayoutConstraint.Name
        {
            get { return "Canvas Bounds".Localize(); }
        }

        /// <summary>
        /// Gets or sets whether the constraint is enabled</summary>
        bool ILayoutConstraint.Enabled
        {
            get { return m_constraintEnabled; }
            set { m_constraintEnabled = value; }
        }

        /// <summary>
        /// Applies constraint to bounding rectangle of canvas</summary>
        /// <param name="bounds">Unconstrained bounding rectangle</param>
        /// <param name="specified">Flags indicating which parts of bounding rectangle are meaningful</param>
        /// <returns>Constrained bounding rectangle</returns>
        Rectangle ILayoutConstraint.Constrain(Rectangle bounds, BoundsSpecified specified)
        {
            if (m_constraintEnabled)
            {
                if (bounds.X < m_bounds.X)
                    bounds.X = m_bounds.X;
                else if (bounds.X >= m_bounds.Right)
                    bounds.X = m_bounds.Right - 1;
                if (bounds.Y < m_bounds.Y)
                    bounds.Y = m_bounds.Y;
                else if (bounds.Y >= m_bounds.Bottom)
                    bounds.Y = m_bounds.Bottom - 1;
            }
            return bounds;
        }

        #endregion

        /// <summary>
        /// Binds the adapter to the adaptable control. Called in the order that the adapters
        /// were defined on the control.</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Bind(AdaptableControl control)
        {
            m_windowBounds = control.ClientRectangle;

            m_transformAdapter = control.As<ITransformAdapter>();
            if (m_transformAdapter != null)
                m_transformAdapter.TransformChanged += transformAdapter_TransformChanged;

            control.ClientSizeChanged += control_ClientSizeChanged;
        }

        private void transformAdapter_TransformChanged(object sender, EventArgs e)
        {
            PointF newScale = m_transformAdapter.Scale;
            if (m_scale != newScale)
            {
                UpdateTranslationConstraints();
            }
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            if (m_transformAdapter != null)
                m_transformAdapter.TransformChanged -= transformAdapter_TransformChanged;

            control.ClientSizeChanged -= control_ClientSizeChanged;
        }

        private void control_ClientSizeChanged(object sender, EventArgs e)
        {
            WindowBounds = AdaptedControl.ClientRectangle;
        }

        private ITransformAdapter m_transformAdapter;
        private Rectangle m_bounds;
        private Rectangle m_windowBounds;
        private PointF m_scale;
        private bool m_constraintEnabled = true; //was 'true' in ATF 3.5
    }
}
