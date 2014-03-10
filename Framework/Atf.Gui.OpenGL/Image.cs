//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using Tao.OpenGl;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Pixel formats for textures</summary>
    public enum PixelFormat
    {
        /// <summary>
        /// Alpha, Red, Green, Blue in 32 bits</summary>
        ARGB32,

        /// <summary>
        /// Red, Green, Blue in 24 bits</summary>
        RGB24,

        /// <summary>
        /// DXT 1 Compressed</summary>
        DXT1,

        /// <summary>
        /// DXT 3 Compressed</summary>
        DXT3,

        /// <summary>
        /// DXT 5 Compressed</summary>
        DXT5,

        /// <summary>
        /// Unknown format</summary>
        UNKNOWN
    }

    /// <summary>
    /// Texture image</summary>
    public class Image
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="width">The width</param>
        /// <param name="height">The height</param>
        /// <param name="pixels">Pixel data</param>
        /// <param name="levels">Number of mipmap levels</param>
        /// <param name="pixelFormat">The OpenGL pixel format, e.g., Gl.GL_RGBA</param>
        /// <param name="elementsPerPixel">The elements per pixel</param>
        public Image(int width, int height, byte[] pixels, int levels, int pixelFormat, int elementsPerPixel)
        {
            m_width = width;
            m_height = height;
            m_pixels = pixels;
            m_levels = levels;
            m_pixelFormat = pixelFormat;
            m_elementsPerPixel = elementsPerPixel;
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="width">The width</param>
        /// <param name="height">The height</param>
        /// <param name="pixels">Pixel data</param>
        /// <param name="levels">Number of mipmap levels</param>
        /// <param name="pixelFormat">The pixel format</param>
        /// <param name="elementsPerPixel">The elements per pixel</param>
        public Image(int width, int height, byte[] pixels, int levels, PixelFormat pixelFormat, int elementsPerPixel)
        {
            m_width = width;
            m_height = height;
            m_pixels = pixels;
            m_levels = levels;
            PixelFormat = pixelFormat;
            m_elementsPerPixel = elementsPerPixel;
        }

        /// <summary>
        /// Gets the image width</summary>
        public int Width
        {
            get { return m_width; }
        }

        /// <summary>
        /// Gets the image height</summary>
        public int Height
        {
            get { return m_height; }
        }

        /// <summary>
        /// Gets or sets the image pixel data</summary>
        public byte[] Pixels
        {
            get { return m_pixels; }
            set { m_pixels = value; }
        }

        /// <summary>
        /// Gets the number of image mipmap levels</summary>
        public int Levels
        {
            get { return m_levels; }
        }

        /// <summary>
        /// Gets the image OpenGL pixel format</summary>
        public int OpenGlPixelFormat
        {
            get { return m_pixelFormat; }
        }

        /// <summary>
        /// Gets or sets whether image is for DDS cube map</summary>
        public bool IsCubeMap
        {
            get { return m_cubemap; }
            set { m_cubemap = value; }
        }

        /// <summary>
        /// Gets or sets the pixel format</summary>
        public PixelFormat PixelFormat
        {
            get
            {
                switch (m_pixelFormat)
                {
                    case Gl.GL_BGRA:
                        return PixelFormat.ARGB32;

                    case Gl.GL_BGR:
                        return PixelFormat.RGB24;

                    case Gl.GL_COMPRESSED_RGBA_S3TC_DXT1_EXT:
                        return PixelFormat.DXT1;

                    case Gl.GL_COMPRESSED_RGBA_S3TC_DXT3_EXT:
                        return PixelFormat.DXT3;

                    case Gl.GL_COMPRESSED_RGBA_S3TC_DXT5_EXT:
                        return PixelFormat.DXT5;
                }

                return PixelFormat.UNKNOWN;
            }
            set
            {
                switch (value)
                {
                    case PixelFormat.ARGB32:
                        m_pixelFormat = Gl.GL_BGRA;
                        break;

                    case PixelFormat.RGB24:
                        m_pixelFormat = Gl.GL_BGR;
                        break;

                    case PixelFormat.DXT1:
                        m_pixelFormat = Gl.GL_COMPRESSED_RGBA_S3TC_DXT1_EXT;
                        break;

                    case PixelFormat.DXT3:
                        m_pixelFormat = Gl.GL_COMPRESSED_RGBA_S3TC_DXT3_EXT;
                        break;

                    case PixelFormat.DXT5:
                        m_pixelFormat = Gl.GL_COMPRESSED_RGBA_S3TC_DXT5_EXT;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the elements per pixel in image</summary>
        public int ElementsPerPixel
        {
            get { return m_elementsPerPixel;  }
        }

        /// <summary>
        /// Creates a bitmap from this image</summary>
        /// <returns>Bitmap created from this image</returns>
        public Bitmap CreateBitmap()
        {
            // this image is compact, but Bitmap must have each row be a multiple of 4.
            int bitmapStride = (((ElementsPerPixel * Width) + 3) >> 2) << 2;
            int bitmapBufferSize = bitmapStride * Height;

            // convert if necessary, making 'imagePixels' match what Bitmap needs.
            byte[] imagePixels;
            int imageStride = ElementsPerPixel * Width;
            if (imageStride == bitmapStride)
            {
                imagePixels = Pixels;
            }
            else
            {
                imagePixels = new byte[bitmapBufferSize];
                byte[] source = Pixels;
                for (int row = 0; row < Height; row++)
                {
                    for (int col = 0; col < imageStride; col++)
                    {
                        imagePixels[bitmapStride * row + col] = source[imageStride * row + col];
                    }
                }
            }

            // determine the System pixel format from the Scea pixel format. 
            System.Drawing.Imaging.PixelFormat format = System.Drawing.Imaging.PixelFormat.Undefined;
            switch (PixelFormat)
            {
                case PixelFormat.ARGB32:
                    format = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                    break;
                case PixelFormat.RGB24:
                    format = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
                    break;
                default:
                    return null;
            }

            // create the Bitmap with a pixel buffer of the correct size and then copy in ours
            //  from managed memory ('imagePixels') to the unmanaged memory.
            Bitmap bitmap = new Bitmap(Width, Height, format);
            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, Width, Height),
                ImageLockMode.ReadWrite,
                format);
            try
            {
                Marshal.Copy(imagePixels, 0, data.Scan0, bitmapBufferSize);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            return bitmap;
        }

        private readonly int m_width;
        private readonly int m_height;
        private byte[] m_pixels;
        private readonly int m_levels;
        private int m_pixelFormat;
        private readonly int m_elementsPerPixel;
        private bool m_cubemap = false;
    }
}
