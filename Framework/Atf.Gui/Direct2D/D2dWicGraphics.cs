//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using SharpDX.Direct2D1;
using SharpDX.WIC;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Sce.Atf.Direct2D
{
    public sealed class D2dWicGraphics : D2dGraphics
    {
        private readonly SharpDX.WIC.Bitmap m_wicBitmap;

        internal D2dWicGraphics(WicRenderTarget renderTarget, SharpDX.WIC.Bitmap wicBitmap)
            : base(renderTarget)
        {
            m_wicBitmap = wicBitmap;
        }

        /// <summary>
        /// Recreates the render target, if necessary, by calling SetRenderTarget.</summary>
        protected override void RecreateRenderTarget()
        {
            // do not recreate D2dBitmapGraphics let the user do that 
            // by handling RecreateResources event from the D2dGraphics 
            // that created this D2dBitmapGraphics
        }

        public System.Drawing.Bitmap Copy()
        {
            var bitmap = new System.Drawing.Bitmap(m_wicBitmap.Size.Width, m_wicBitmap.Size.Height);
            System.Drawing.Imaging.BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);
            m_wicBitmap.CopyPixels(data.Stride, data.Scan0, data.Height * data.Stride);
            bitmap.UnlockBits(data);
            return bitmap;
        }
    }
}
