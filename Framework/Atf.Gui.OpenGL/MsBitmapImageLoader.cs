//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;
using System.Runtime.InteropServices;

//using Tao.OpenGl;
using OTK = OpenTK.Graphics;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// IImageLoader class for loading images using System.Drawing.Bitmap</summary>
    public class MsBitmapImageLoader : IImageLoader
    {
        /// <summary>
        /// Loads texture image from stream</summary>
        /// <param name="imageStream">Stream holding texture image</param>
        /// <returns>Texture image</returns>
        public Image LoadImage(Stream imageStream)
        {
            Image image = null;
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(imageStream);

            // Temporary fix to support other pixel formats
            if (bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb && bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format24bppRgb)
            {
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(bitmap);
                bmp.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);
                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp);
                g.DrawImage(bitmap, 0, 0);
                g.Dispose();
                bitmap = bmp;
            }

            using (bitmap)
            {    
                int glPixelFormat = CalculateGlPixelFormat(bitmap);
                int pixelDepth = (glPixelFormat == (int)OTK.OpenGL.PixelFormat.Bgra) ? 4 : 3;
                
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
                System.Drawing.Imaging.BitmapData bitmapData = 
                    bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

                int byteCount = bitmap.Width*bitmap.Height*pixelDepth;
                byte[] pixels = new byte[byteCount];

                Marshal.Copy(bitmapData.Scan0, pixels, 0, byteCount);
                
                bitmap.UnlockBits(bitmapData);
                
                image = new Image(bitmap.Width, bitmap.Height, pixels, 1, glPixelFormat, pixelDepth);
            }

            if (image == null)
                throw new NotImplementedException("Image couldn't be loaded by MsBitmapImageLoader.");

            return image;
        }

        private int CalculateGlPixelFormat(System.Drawing.Bitmap bitmap)
        {
            switch (bitmap.PixelFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return (int)OTK.OpenGL.PixelFormat.Bgra;
                    
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return (int)OTK.OpenGL.PixelFormat.Bgr;
                    
                default:
                    throw new NotSupportedException("Unknown bitmap pixel depth '" + 
                                                    bitmap.PixelFormat.ToString() + "'.");
            }
        }
    }
}
