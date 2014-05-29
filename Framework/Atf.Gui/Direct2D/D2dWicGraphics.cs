//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using SharpDX.Direct2D1;

namespace Sce.Atf.Direct2D
{
    /// <summary>
    /// A Direct2D Windows Imaging Component (WIC). See D2dFactory.CreateWicGraphics().</summary>
    public sealed class D2dWicGraphics : D2dGraphics
    {
        private readonly SharpDX.WIC.Bitmap m_wicBitmap;

        internal D2dWicGraphics(WicRenderTarget renderTarget, SharpDX.WIC.Bitmap wicBitmap)
            : base(renderTarget)
        {
            m_wicBitmap = wicBitmap;
        }

        /// <summary>
        /// Recreates the render target, if necessary, by calling SetRenderTarget</summary>
        protected override void RecreateRenderTarget()
        {
            // Do not recreate D2dWicGraphics. Let the user do that
            // by handling the RecreateResources event from the D2dGraphics 
            // that created this D2dWicGraphics.
        }

        /// <summary>
        /// Returns a System.Drawing.Bitmap object that is a copy of the image stored in this
        /// D2dWicGraphics object.</summary>
        /// <returns>System.Drawing.Bitmap copy of image</returns>
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
