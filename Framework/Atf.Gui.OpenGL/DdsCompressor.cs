//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.IO;

using Tao.OpenGl;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// DDS Compression types</summary>
    public enum CompressionType
    {
        /// <summary>
        /// DXT 1</summary>
        DXT1,

        /// <summary>
        /// DXT 3</summary>
        DXT3,

        /// <summary>
        /// DXT 5</summary>
        DXT5
    }

    /// <summary>
    /// DDS Image compressor</summary>
    public class DdsCompressor
    {
        /// <summary>
        /// Compresses the specified image</summary>
        /// <param name="image">The image</param>
        /// <param name="type">The type</param>
        /// <returns>Compressed image</returns>
        static public Image Compress(Image image, CompressionType type)
        {
            Image ddsImage = null;

            MemoryStream memStream = new MemoryStream();

            // Switch on the format
            DDSUtils.TextureFormat format = 0;
            switch (type)
            {
                case CompressionType.DXT1:
                    format = DDSUtils.TextureFormat.kDXT1;
                    break;

                case CompressionType.DXT3:
                    format = DDSUtils.TextureFormat.kDXT3;
                    break;

                case CompressionType.DXT5:
                    format = DDSUtils.TextureFormat.kDXT5;
                    break;
            }


            if (image.OpenGlPixelFormat == Gl.GL_BGRA)
            {
                DDSUtils.ImageConverter.CompressARGB(image.Width,
                    image.Height,
                    image.Pixels,
                    format,
                    memStream);
            }
            else
            {
                DDSUtils.ImageConverter.CompressRGB(image.Width,
                     image.Height,
                     image.Pixels,
                     format,
                     memStream);
            }

            // Rewind
            memStream.Seek(0, SeekOrigin.Begin);

            var loader = new DdsImageLoader();
            ddsImage = loader.LoadImage(memStream);

            return ddsImage;
        }
    }
}
