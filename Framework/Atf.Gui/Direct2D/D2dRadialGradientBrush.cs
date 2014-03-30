//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;

using SharpDX;
using SharpDX.Direct2D1;

namespace Sce.Atf.Direct2D
{
    /// <summary>
    /// Paints an area with a radial gradient</summary>
    public class D2dRadialGradientBrush : D2dBrush
    {        
        /// <summary>    
        /// Gets and sets the center of the gradient ellipse</summary>            
        public PointF Center
        {
            get { return m_center; }
            set
            {
                m_center = value;
                var brush = (RadialGradientBrush)NativeBrush;
                brush.Center = new SharpDX.DrawingPointF(value.X, value.Y);
            }
        }

        /// <summary>    
        /// Gets and sets the offset of the gradient origin relative to the gradient 
        /// ellipse's center</summary>            
        public PointF GradientOriginOffset
        {
            get { return m_gradientOriginOffset;}
            set
            {
                m_gradientOriginOffset = value;
                var brush = (RadialGradientBrush)NativeBrush;
                brush.GradientOriginOffset = new SharpDX.DrawingPointF(value.X, value.Y);
            }
        }

        /// <summary>    
        /// Gets and sets the x-radius of the gradient ellipse</summary>    
        /// <unmanaged>float ID2D1RadialGradientBrush::GetRadiusX()</unmanaged>
        public float RadiusX
        {
            get { return m_radiusX;}
            set
            {
                m_radiusX = value;
                var brush = (RadialGradientBrush)NativeBrush;
                brush.RadiusX = value;
            }
        }

        /// <summary>    
        /// Gets and sets the y-radius of the gradient ellipse</summary>            
        public float RadiusY
        {
            get { return m_radiusY; }
            set
            {
                m_radiusY = value;
                var brush = (RadialGradientBrush)NativeBrush;
                brush.RadiusY = value;
            }
        }

        internal D2dRadialGradientBrush(D2dGraphics owner, PointF center,
            PointF gradientOriginOffset,
            float radiusX,
            float radiusY,
            params D2dGradientStop[] gradientStops)
            : base(owner)
        {
            m_center = center;
            m_gradientOriginOffset = gradientOriginOffset;
            m_radiusX = radiusX;
            m_radiusY = radiusY;            
            m_gradientStops = new D2dGradientStop[gradientStops.Length];
            Array.Copy(gradientStops, m_gradientStops, m_gradientStops.Length);
            Create();//to-do: it's dangerous to call a virtual method in a constructor; derived class may not be properly initialized!
        }

        internal override void Create()
        {
            if (NativeBrush != null)
                NativeBrush.Dispose();

            var stops = new GradientStop[m_gradientStops.Length];
            for (int s = 0; s < m_gradientStops.Length; s++)
            {
                stops[s].Color = m_gradientStops[s].Color.ToColor4();
                stops[s].Position = m_gradientStops[s].Position;
            }

            var props = new RadialGradientBrushProperties
            {
                Center = m_center.ToSharpDX(), 
                GradientOriginOffset = m_gradientOriginOffset.ToSharpDX(), 
                RadiusX = m_radiusX, 
                RadiusY = m_radiusY
            };

            using (var stopcol = new GradientStopCollection(Owner.D2dRenderTarget, stops, Gamma.StandardRgb, ExtendMode.Clamp))
            {
                NativeBrush = new RadialGradientBrush(Owner.D2dRenderTarget, props, stopcol);
            }
        }

        private PointF m_center;
        private PointF m_gradientOriginOffset;
        private float m_radiusX;
        private float m_radiusY;
        private D2dGradientStop[] m_gradientStops;
    }
}
