//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
//using System.Drawing;

using System;
using GdiPixelFormat = System.Drawing.Imaging.PixelFormat;

using SharpDX;
//using SharpDX.Direct2D1;

namespace Sce.Atf.Direct2D
{
    /// <summary>
    /// Represents a bitmap that has been bound to a D2dGraphics object</summary>
    public class D2dBitmap : D2dResource
    {
        /// <summary>
        /// Gets the size, in (pixels), of the bitmap</summary>
        public System.Drawing.Size PixelSize
        {
            get
            {
                return new System.Drawing.Size(m_nativeBitmap.PixelSize.Width
                , m_nativeBitmap.PixelSize.Height);
            }
        }

        /// <summary>
        /// Gets the size, in device-independent pixels (DIPs), of the bitmap.
        /// A DIP is 1/96  of an inch. To retrieve the size in device pixels, use the
        /// D2dBitmap.PixelSize.</summary>
        public System.Drawing.SizeF Size
        {
            get { return new System.Drawing.SizeF(m_nativeBitmap.Size.Width, m_nativeBitmap.Size.Height); }
        }

        /// <summary>
        /// Gets the GdiBitmap that is used as backup copy
        /// or null if there is none.</summary>
        /// <remarks>
        /// Do not dispose the returned bitmap.
        /// If the bitmap is modified then call update()
        /// </remarks>
        public System.Drawing.Bitmap GdiBitmap
        {
            get { return m_bitmap; }
        }

        /// <summary>
        /// Updates native bitmap from GdiBitmap.
        /// Call this method to force update when GdiBitmap is changed
        /// outside this class.</summary>
        public void Update()
        {
            if (m_bitmap == null)
                return; // m_bitmap can be null when this is a bitmapgraphics.

            // lock the m_bitmap and get pointer to the first pixel.
            System.Drawing.Imaging.BitmapData
                data = m_bitmap.LockBits(new System.Drawing.Rectangle(0, 0, m_bitmap.Width, m_bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly,
                m_bitmap.PixelFormat);
            m_nativeBitmap.CopyFromMemory(data.Scan0, data.Stride);            
            m_bitmap.UnlockBits(data);
        }

        /// <summary>
        /// Copy raw memory into the bitmap.</summary>
        /// <param name="bytes">Data to copy</param>
        /// <param name="stride">Stride, or pitch, of source bitmap</param>
        /// <remarks>Pitch = pixel width * bytes per pixel + memory padding</remarks>
        public void CopyFromMemory(byte[] bytes, int stride)
        {
            m_nativeBitmap.CopyFromMemory(bytes, stride);
        }

        internal D2dBitmap(D2dGraphics owner, SharpDX.Direct2D1.Bitmap bmp)
        {            
            m_nativeBitmap = bmp;
            m_owner = owner;
            m_bitmap = null;
            m_owner.RecreateResources += RecreateResources;
            m_rtNumber = owner.RenderTargetNumber;
        }

        internal D2dBitmap(D2dGraphics owner, System.Drawing.Bitmap bmp)
        {
            if (bmp.PixelFormat != GdiPixelFormat.Format32bppPArgb)
                throw new System.ArgumentException("pixel format must be GdiPixelFormat.Format32bppPArgb");                

            m_owner = owner;
            m_bitmap = bmp;
            Create();
            m_owner.RecreateResources += RecreateResources;
            m_rtNumber = owner.RenderTargetNumber;
        }

        private void RecreateResources(object sender, System.EventArgs e)
        {
            // validation before recreating resources
            if (IsDisposed)
            {// should not recreate disposed resources
                m_owner.RecreateResources -= RecreateResources;
                return;
            }

            if (m_rtNumber == m_owner.RenderTargetNumber)
            {// this resource does not need to be recreated
                return;
            }

            if (m_bitmap == null)
            {                
                Dispose();
            }
            else
            {
                Create();
                m_rtNumber = m_owner.RenderTargetNumber;
            }
        }

        internal SharpDX.Direct2D1.Bitmap NativeBitmap
        {
            get { return m_nativeBitmap; }
        }

        /// <summary>
        /// Disposes of unmanaged resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            // m_nativeBitmap is comobject dispose have different meaning.
            m_owner.RecreateResources -= RecreateResources;

            if (m_nativeBitmap != null)
            {
                m_nativeBitmap.Dispose();
                m_nativeBitmap = null;
            }

            if (m_bitmap != null)
            {
                m_bitmap.Dispose();
                m_bitmap = null;
            }

            base.Dispose(disposing);
        }

        internal void Create()
        {
            if (m_bitmap == null)
                return;

            if (m_nativeBitmap != null)
            {
                m_nativeBitmap.Dispose();
                m_nativeBitmap = null;
            }
            
            var props = new SharpDX.Direct2D1.BitmapProperties();
            props.DpiX = m_bitmap.HorizontalResolution;
            props.DpiY = m_bitmap.VerticalResolution;            
            props.PixelFormat = new SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied);
            m_nativeBitmap = new SharpDX.Direct2D1.Bitmap(m_owner.D2dRenderTarget, new Size2(m_bitmap.Width, m_bitmap.Height), props);                        
            Update();
        }
        
        private SharpDX.Direct2D1.Bitmap m_nativeBitmap;
        private System.Drawing.Bitmap m_bitmap;
        private readonly D2dGraphics m_owner;
        private uint m_rtNumber; // rt number of the owner at ctor time
    }


    /// <summary>
    /// Specifies the algorithm that is used when images are scaled or rotated</summary>
    public enum D2dBitmapInterpolationMode
    {
        /// <summary>
        /// Use the exact color of the nearest bitmap pixel to the current rendering
        /// pixel.</summary>
        NearestNeighbor = (int)SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor,

        /// <summary>        
        /// Interpolate a color from the four bitmap pixels that are the nearest to the
        /// rendering pixel.</summary>
        Linear = (int)SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
    }
}
