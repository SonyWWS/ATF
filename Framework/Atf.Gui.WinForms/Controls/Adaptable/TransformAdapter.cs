//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Adapter that defines a transform for the adapted control consisting of
    /// a scale followed by a translation. This transform affects how the canvas
    /// is viewed.</summary>
    public class TransformAdapter : ControlAdapter, ITransformAdapter
    {
        /// <summary>
        /// Gets the current transformation matrix</summary>
        public Matrix Transform
        {
            get { return m_transform; }
        }

        /// <summary>
        /// Gets the current transformation matrix, from world to Window client coordinates</summary>
        public PointF Translation
        {
            get { return new PointF(m_transform.OffsetX, m_transform.OffsetY); }
            set { SetTranslation(value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether constraint rules are followed when
        /// attempting any update to translation or scale</summary>
        public bool EnforceConstraints
        {
            set { m_enforceConstraints = value; }
            get { return m_enforceConstraints; }
        }
  

        /// <summary>
        /// Gets or sets the minimum values for x and y translation</summary>
        public PointF MinTranslation
        {
            get { return m_minTranslation; }
            set
            {
                m_minTranslation = value;
                SetTranslation(Translation); // check translation against new constraints
            }
        }

        /// <summary>
        /// Gets or sets the maximum values for x and y translation</summary>
        public PointF MaxTranslation
        {
            get { return m_maxTranslation; }
            set
            {
                m_maxTranslation = value;
                SetTranslation(Translation); // check translation against new constraints
            }
        }

        /// <summary>
        /// Gets or sets the scale</summary>
        public PointF Scale
        {
            get
            {
                float[] m = m_transform.Elements;
                return new PointF(m[0], m[3]);
            }
            set
            {
                SetScale(value);
            }
        }

        /// <summary>
        /// Gets or sets the minimum values for x and y scale</summary>
        public PointF MinScale
        {
            get { return m_minScale; }
            set
            {
                if (value.X <= 0 ||
                    value.X > m_maxScale.X ||
                    value.Y <= 0 ||
                    value.Y > m_maxScale.Y)
                {
                    throw new ArgumentException("minimum components must be > 0 and less than maximum");
                }

                m_minScale = value;
                SetScale(Scale); // check scale against new constraints
            }
        }

        /// <summary>
        /// Gets or sets the maximum values for x and y scale</summary>
        public PointF MaxScale
        {
            get { return m_maxScale; }
            set
            {
                if (value.X < m_minScale.X ||
                    value.Y < m_minScale.Y)
                {
                    throw new ArgumentException("maximum components must be greater than minimum");
                }

                m_maxScale = value;
                SetScale(Scale); // check scale against new constraints
            }
        }

        /// <summary>
        /// Gets whether the transform is constrained to uniform scale
        /// (same in x and y direction). The default is true.</summary>
        public bool UniformScale
        {
            get { return m_uniformScale; }
            set { m_uniformScale = value; }
        }

        /// <summary>
        /// Sets the transform to scale and translate. The scale is applied first, so that subscribers
        /// to the TransformChanged event can change the translation constraint.</summary>
        /// <param name="xScale">X scale</param>
        /// <param name="yScale">Y scale</param>
        /// <param name="xTranslation">X translation</param>
        /// <param name="yTranslation">Y translation</param>
        public void SetTransform(float xScale, float yScale, float xTranslation, float yTranslation)
        {
            if (m_settingTransform)
                return;
            try
            {
                m_settingTransform = true;

                bool transformChanged = false;
                float[] m = m_transform.Elements;

                PointF scale = EnforceConstraints ? this.ConstrainScale(new PointF(xScale, yScale)) : new PointF(xScale, yScale);
                if (m[0] != scale.X || m[3] != scale.Y)
                {
                    m_transform = new Matrix(scale.X, 0, 0, scale.Y, m_transform.OffsetX, m_transform.OffsetY);
                    OnTransformChanged(EventArgs.Empty);
                    TransformChanged.Raise(this, EventArgs.Empty);
                    transformChanged = true;
                }

                PointF translation = EnforceConstraints ? this.ConstrainTranslation(new PointF(xTranslation, yTranslation)) : new PointF(xTranslation, yTranslation);
                if (m[4] != translation.X || m[5] != translation.Y)
                {
                    m_transform = new Matrix(scale.X, 0, 0, scale.Y, translation.X, translation.Y);
                    OnTransformChanged(EventArgs.Empty);
                    TransformChanged.Raise(this, EventArgs.Empty);
                    transformChanged = true;
                }

                if (transformChanged)
                {
                    var d2dCtrl = AdaptedControl as D2dAdaptableControl;
                    if (d2dCtrl != null)
                        d2dCtrl.DrawD2d();
                    else if (AdaptedControl != null)
                        AdaptedControl.Invalidate();
                }
            }
            finally
            {
                m_settingTransform = false;
            }
        }
        
        /// <summary>
        /// Event that is raised after the transform changes</summary>
        public event EventHandler TransformChanged;

        /// <summary>
        /// Performs custom actions after transform changes</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnTransformChanged(EventArgs e)
        {
        }

        private void SetTranslation(PointF translation)
        {
            float[] m = m_transform.Elements;
            SetTransform(m[0], m[3], translation.X, translation.Y);
        }

        private void SetScale(PointF scale)
        {
            SetTransform(scale.X, scale.Y, m_transform.OffsetX, m_transform.OffsetY);
        }

        private Matrix m_transform = new Matrix();
        private PointF m_minTranslation = new PointF(float.MinValue, float.MinValue);
        private PointF m_maxTranslation = new PointF(float.MaxValue, float.MaxValue);
        private PointF m_minScale = new PointF(float.MinValue, float.MinValue);
        private PointF m_maxScale = new PointF(float.MaxValue, float.MaxValue);
        private bool m_uniformScale = true;
        private bool m_settingTransform;
        private bool m_enforceConstraints = true; //was 'true' in ATF 3.5
    }
}
