//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;

using SharpDX;
using SharpDX.Direct2D1;


namespace Sce.Atf.Direct2D
{
    /// <summary>
    /// Paints an area with a linear gradient</summary>
    public class D2dLinearGradientBrush : D2dBrush
    {
        /// <summary>
        /// Gets and sets the starting coordinates of the linear gradient</summary>
        /// <remarks>
        /// The start point and end point are described in the brush's space and are
        /// mapped to the D2dGraphics when the brush is used. If there is a non-identity
        /// D2dGraphics transform, the brush's start point and end point are also transformed.</remarks>        
        public PointF StartPoint 
        {
            get { return m_start; }
            set
            {
                m_start = value;
                var brush = (LinearGradientBrush)NativeBrush;
                brush.StartPoint = value.ToSharpDX();
            }
        }
       
        /// <summary>
        /// Gets and sets the ending coordinates of the linear gradient</summary>
        /// <remarks>
        /// The start point and end point are described in the brush's space and are
        /// mapped to the D2dGraphics when the brush is used. If there is a non-identity
        /// brush transform or D2dGraphics transform, the brush's start point and end
        /// point are also transformed.</remarks>        
        public PointF EndPoint 
        {
            get { return m_end; }
            set
            {
                m_end = value;
                var brush = (LinearGradientBrush)NativeBrush;
                brush.EndPoint = value.ToSharpDX();
            }
        }

        internal D2dLinearGradientBrush(D2dGraphics owner, PointF pt1,
            PointF pt2,
            D2dGradientStop[] gradientStops,
            D2dExtendMode extendMode,
            D2dGamma gamma)
            : base(owner)
        {
            m_start = pt1;
            m_end = pt2;            
            m_gradientStops = new D2dGradientStop[gradientStops.Length];
            Array.Copy(gradientStops, m_gradientStops, m_gradientStops.Length);

            m_gamma = gamma;
            m_extendMode = extendMode;
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

            using (var stopcol = new GradientStopCollection(Owner.D2dRenderTarget, stops, (Gamma)m_gamma, (ExtendMode)m_extendMode))
            {
                var props = new LinearGradientBrushProperties 
                {
                    StartPoint = m_start.ToSharpDX(), 
                    EndPoint = m_end.ToSharpDX()
                };
                NativeBrush = new LinearGradientBrush(Owner.D2dRenderTarget, props, stopcol);
            }
        }

        private PointF m_start;
        private PointF m_end;
        private readonly D2dGradientStop[] m_gradientStops;
        private readonly D2dExtendMode m_extendMode;
        private readonly D2dGamma m_gamma;
    }
   
    /// <summary>
    /// Struct that contains the position and color of a gradient stop</summary>
    /// <remarks>
    /// Gradient stops can be specified in any order if they are at different positions.
    /// Two stops may share a position. In this case, the first stop specified is
    /// treated as the "low" stop (nearer 0.0f) and subsequent stops are treated
    /// as "higher" (nearer 1.0f). This behavior is useful if a caller wants an instant
    /// transition in the middle of a stop. Typically, there are at least two points
    /// in a collection, although creation with only one stop is permitted. For example,
    /// one point is at position 0.0f, another point is at position 1.0f, and additional
    /// points are distributed in the [0, 1] range. When the gradient progression
    /// is beyond the range of [0, 1], the stops are stored, but may affect the gradient.
    /// When drawn, the [0, 1] range of positions is mapped to the brush, in a brush-dependent
    /// way. For details, see D2dLinearGradientBrush and D2dRadialGradientBrush.
    /// Gradient stops with a position outside the [0, 1] range cannot be seen explicitly,
    /// but they can still affect the colors produced in the [0, 1] range. For example,
    /// a two-stop gradient {{0.0f, Black}, {2.0f, White}} is indistinguishable visually
    /// from {{0.0f, Black}, {1.0f, Mid-level gray}}. Also, the colors are clamped
    /// before interpolation.</remarks>
    public struct D2dGradientStop
    {
        /// <summary>
        /// Construct D2dGradientStop from specified color and postion</summary>
        /// <param name="color">The color of the gradient stop</param>
        /// <param name="position">A value that indicates the relative position of the
        /// gradient stop in the brush. This value must be in the [0.0f, 1.0f] range if
        /// the gradient stop is to be seen explicitly.</param>
        public D2dGradientStop(System.Drawing.Color color, float position)
        {
            Color = color;
            Position = position;
        }

        /// <summary>
        /// The color of the gradient stop</summary>
        public System.Drawing.Color Color;
                
        /// <summary>
        /// A value that indicates the relative position of the gradient stop in the
        /// brush. This value must be in the [0.0f, 1.0f] range if the gradient stop
        /// is to be seen explicitly.</summary>
        public float Position;
    }
    
    /// <summary>
    /// Specifies which gamma is used for interpolation</summary>
    /// <remarks>
    /// Interpolating in a linear gamma space (Linear) can avoid changes
    /// in perceived brightness caused by the effect of gamma correction in spaces
    /// where the gamma is not 1.0, such as the default sRGB color space, where the
    /// gamma is 2.2.</remarks>
    public enum D2dGamma
    {
        /// <summary>
        /// Interpolation is performed in the standard RGB (sRGB) gamma</summary>
        StandardRgb = 0,

        /// <summary>
        /// Interpolation is performed in the linear-gamma color space</summary>
        Linear = 1,
    }
}
