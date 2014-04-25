//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;
using SharpDX;
using SharpDX.Direct2D1;

namespace Sce.Atf.Direct2D
{
    /// <summary>
    /// Paints an area with a bitmap</summary>
    public class D2dBitmapBrush : D2dBrush
    {
        /// <summary>    
        /// Gets the bitmap source that this brush uses to paint</summary>            
        public D2dBitmap Bitmap
        {
            get { return m_bitmap; }
            set
            {
                m_bitmap = value;
                var brush = (BitmapBrush)NativeBrush;
                brush.Bitmap = m_bitmap.NativeBitmap;
            }
        }
        /// <summary>    
        /// Gets the method by which the brush horizontally tiles those areas that extend past its bitmap</summary>    
        /// <remarks>    
        /// Like all brushes, D2dBitmapBrush defines an infinite plane of content. Because bitmaps are finite,
        /// it relies on an extend mode to determine how the plane is filled horizontally and vertically.</remarks>            
        public D2dExtendMode ExtendModeX
        {
            get { return m_extendModeX; }
            set
            {
                m_extendModeX = value;
                var brush = (BitmapBrush)NativeBrush;
                brush.ExtendModeX = (ExtendMode)value;
            }
        }
        /// <summary>    
        /// Gets the method by which the brush vertically tiles those areas that extend past its bitmap</summary>    
        /// <remarks>    
        /// Like all brushes, D2dBitmapBrush defines an infinite plane of content. Because bitmaps are finite,
        /// it relies on an extend mode to determine how the plane is filled horizontally and vertically.</remarks>            
        public D2dExtendMode ExtendModeY
        {
            get { return m_extendModeY; }
            set
            {
                m_extendModeY = value;
                var brush = (BitmapBrush)NativeBrush;
                brush.ExtendModeY = (ExtendMode)value;
            }
        }

        /// <summary>    
        /// Gets the interpolation method used when the brush bitmap is scaled or rotated</summary>    
        /// <remarks>    
        /// This method gets the interpolation mode of a bitmap, which is specified by the <see cref="T:SharpDX.Direct2D1.BitmapInterpolationMode" /> enumeration type.
        /// D2D1_BITMAP_INTERPOLATION_MODE_NEAREST_NEIGHBOR represents nearest neighbor filtering. 
        /// It looks up the bitmap pixel nearest to the current rendering pixel and chooses its exact color.
        /// D2D1_BITMAP_INTERPOLATION_MODE_LINEAR represents linear filtering, and  interpolates a color from the four nearest bitmap pixels. 
        /// The interpolation mode of a bitmap also affects subpixel translations. In a subpixel translation, 
        /// linear interpolation positions the bitmap more precisely to the application request, but blurs the bitmap in the process.</remarks>            
        public D2dBitmapInterpolationMode InterpolationMode
        {
            get { return m_interpolationMode; }
            set
            {
                m_interpolationMode = value;
                var brush = (BitmapBrush)NativeBrush;
                brush.InterpolationMode = (BitmapInterpolationMode)value;
            }
        }


        /// <summary>
        /// Gets and sets location</summary>
        public PointF Location
        {
            get { return m_location; }
            set
            {
                m_location = value;
                var brush = (BitmapBrush)NativeBrush;
                Matrix3x2 trans = Matrix3x2.Identity;
                trans.M31 = value.X;
                trans.M32 = value.Y;
                brush.Transform = trans;
            }
        }

        internal D2dBitmapBrush(D2dGraphics owner, D2dBitmap bitmap)
            : base(owner)
        {
            m_bitmap = bitmap;

            m_extendModeX = D2dExtendMode.Clamp;
            m_extendModeY = D2dExtendMode.Clamp;
            m_location = new PointF(1, 1);
            m_interpolationMode = D2dBitmapInterpolationMode.Linear;
            Create();//to-do: it's dangerous to call a virtual method in a constructor; derived class may not be properly initialized!
        }

        /// <summary>
        /// Disposes of resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            base.Dispose(disposing);
        }

        internal override void Create()
        {
            if (NativeBrush != null)
                NativeBrush.Dispose();

            m_bitmap.Create();
            var props = new BitmapBrushProperties();
            props.InterpolationMode = BitmapInterpolationMode.Linear;
            NativeBrush = new BitmapBrush(Owner.D2dRenderTarget, m_bitmap.NativeBitmap, props);

            ExtendModeX = m_extendModeX;
            ExtendModeY = m_extendModeY;
            InterpolationMode = D2dBitmapInterpolationMode.Linear;
            Location = m_location;
            Bitmap = m_bitmap;
        }

        private D2dBitmap m_bitmap;
        private D2dExtendMode m_extendModeX;
        private D2dExtendMode m_extendModeY;
        private D2dBitmapInterpolationMode m_interpolationMode;
        private PointF m_location;
    }
}
