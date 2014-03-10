//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.IO;

using Tao.OpenGl;

namespace Sce.Atf.Rendering.OpenGL
{
    /// <summary>
    /// Manages texture bindings in OpenGL, and associates groups of textures with
    /// a user defined context</summary>
    public class TextureManager
    {
        /// <summary>
        /// Gets the image loader registry</summary>
        public ImageLoaderRegistry ImageLoaderRegistry
        {
            get { return m_imageLoaderRegistry; }
        }

        /// <summary>
        /// Binds a texture to the given path</summary>
        /// <param name="image">Path to image file containing texture</param>
        /// <returns>OpenGL name of bound texture</returns>
        public int GetTextureName(string image)
        {
            return GetTextureName(image, s_defaultContext);
        }

        /// <summary>
        /// Binds a texture to the given path and context</summary>
        /// <param name="image">Path to image file containing texture</param>
        /// <param name="contextId">Context ID of texture</param>
        /// <returns>OpenGL name of bound texture or -1 if texture could not be found/loaded</returns>
        public int GetTextureName(string image, object contextId)
        {
            return GetTextureName(image, contextId, false);
        }

        /// <summary>
        /// Binds a texture to the given path and context</summary>
        /// <param name="image">Path to image file containing texture</param>
        /// <param name="contextId">Context ID of texture</param>
        /// <param name="enableflipimage">True to enable flipping the image</param>
        /// <returns>OpenGL name of bound texture or -1 if texture could not be found/loaded</returns>
        public int GetTextureName(string image, object contextId, bool enableflipimage)
        {
            int name = -1;

            lock (this)
            {
                TextureContext context = FindContext(contextId);
                if (context == null)
                {
                    context = new TextureContext(contextId);
                    m_texCollections.Add(context);
                }

                if (!context.NameMap.TryGetValue(image, out name))
                {
                    TextureInfo texData;
                    texData = new TextureInfo();
                    texData.EnableFlipImage = enableflipimage;
                    name = BuildImage(image, texData);

                    // When textures can't be found, let's note that fact so as to avoid
                    //  hundreds or thousands of IO exceptions that are then printed in
                    //  OutputService's window.
                    context.NameMap.Add(image, name);

                    if (name >= 0)
                    {
                        //defensive programming -- http://sf.ship.scea.com/sf/go/artf21789
                        if (!context.TextureDataMap.ContainsKey(name))
                        {
                            context.TextureDataMap.Add(name, texData);
                            context.Names.Add(name);
                        }
                    }
                }
            }

            return name;
        }

        /// <summary>
        /// Gets the texture data for the given texture binding</summary>
        /// <param name="textureName">OpenGL name of bound texture</param>
        /// <returns>TextureInfo for the bound texture</returns>
        public TextureInfo GetTextureInfo(int textureName)
        {
            return GetTextureInfo(textureName, s_defaultContext);
        }

        /// <summary>
        /// Gets the texture data for the given texture binding</summary>
        /// <param name="textureName">OpenGL name of bound texture</param>
        /// <param name="contextId">Context ID of texture</param>
        /// <returns>TextureInfo for the bound texture</returns>
        public TextureInfo GetTextureInfo(int textureName, object contextId)
        {
            TextureInfo result = null;
            TextureContext texCollection = FindContext(contextId);
            if (texCollection != null)
                texCollection.TextureDataMap.TryGetValue(textureName, out result);
            return result;
        }

        /// <summary>
        /// Destroys all texture bindings for the default context</summary>
        public void DestroyTextures()
        {
            DestroyTextures(s_defaultContext);
        }

        /// <summary>
        /// Destroys all texture bindings for the given context</summary>
        /// <param name="contextId">Context ID of textures to destroy</param>
        public void DestroyTextures(object contextId)
        {
            TextureContext texCollection = FindContext(contextId);
            if (texCollection != null)
            {
                if (texCollection.Names.Count > 0)
                {
                    int[] texs = new int[texCollection.Names.Count];
                    texCollection.Names.CopyTo(texs, 0);
                    Gl.glDeleteTextures(texs.Length, texs);
                    Util3D.ReportErrors();
                }

                m_texCollections.Remove(texCollection);
            }
        }

        private TextureContext FindContext(object contextId)
        {
            foreach (TextureContext texCollection in m_texCollections)
                if (texCollection.ContextId == contextId)
                    return texCollection;

            return null;
        }

        private int BuildImage(string uri, TextureInfo textureInfo)
        {
            int name = -1;

            // wrapped in this nasty try/catch because bad memory is touched when we're
            // rendering to a thumbnail (different glContext).  driver bug?  :_(
            try
            {
                string filename = uri.Replace(@"file:\\\", "");

                string ext = Path.GetExtension(filename);
                IImageLoader loader = m_imageLoaderRegistry.GetLoader(ext);
                Image image;

                using (Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                    image = loader.LoadImage(stream);

                name = TryCreateOpenGlTextureFromImage(image, textureInfo);
            }
            catch (System.IO.IOException e)
            {
                // excessive error messages adds minutes to a large level that is missing textures
                if (m_missingTextureReports++ < 10)
                    Outputs.WriteLine(OutputMessageType.Error,e.Message);
            }
            catch (Exception e)
            {
                Outputs.WriteLine(OutputMessageType.Error, e.Message);
                Outputs.WriteLine(OutputMessageType.Info, e.StackTrace);
            }
            
            return name;
        }

        private int TryCreateOpenGlTextureFromImage(Image image, TextureInfo textureInfo)
        {
            textureInfo.Format = image.OpenGlPixelFormat;
            textureInfo.Width = image.Width;
            textureInfo.Height = image.Height;
            textureInfo.Components = image.ElementsPerPixel;

            // Relax pixel data alignment restrictions down from 4 to 1
            Gl.glPixelStorei(Gl.GL_UNPACK_ALIGNMENT, 1);

            int textureName;
            Gl.glGenTextures(1, out textureName);

            if (image.IsCubeMap)
            {
                Gl.glEnable(Gl.GL_TEXTURE_CUBE_MAP);
                Gl.glBindTexture(Gl.GL_TEXTURE_CUBE_MAP, textureName);
                // Initialize bound texture state
                Gl.glTexParameteri(Gl.GL_TEXTURE_CUBE_MAP, Gl.GL_TEXTURE_WRAP_R, Gl.GL_CLAMP_TO_EDGE);
                Gl.glTexParameteri(Gl.GL_TEXTURE_CUBE_MAP, Gl.GL_TEXTURE_WRAP_S, Gl.GL_CLAMP_TO_EDGE);
                Gl.glTexParameteri(Gl.GL_TEXTURE_CUBE_MAP, Gl.GL_TEXTURE_WRAP_T, Gl.GL_CLAMP_TO_EDGE);
                Gl.glTexParameteri(Gl.GL_TEXTURE_CUBE_MAP, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
                Gl.glTexParameteri(Gl.GL_TEXTURE_CUBE_MAP, Gl.GL_TEXTURE_MAX_LEVEL, image.Levels - 1);
                textureInfo.EnableFlipImage = false;
            }
            else
            {
                Gl.glBindTexture(Gl.GL_TEXTURE_2D, textureName);
                // Initialize bound texture state
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
            }

            // If EnableFlipImage, we want to flip the image up side down
            if (textureInfo.EnableFlipImage)
            {
                int pixel_size = image.Pixels.Length; 
                byte[] new_pixels = new byte[pixel_size];
                for (int i=0; i<image.Height; i++)
                {
                    int source_i_offset = (image.Height - i - 1) * image.Width * image.ElementsPerPixel;
                    int destin_i_offset = (i) * image.Width * image.ElementsPerPixel;
                    for (int j = 0; j < image.Width; j++)
                    {
                        int j_offset = j * image.ElementsPerPixel;
                        for (int k = 0; k < image.ElementsPerPixel; k++)
                        {
                            new_pixels[destin_i_offset + j_offset + k] = image.Pixels[source_i_offset + j_offset + k];
                        }
                    }
                }
                image.Pixels = new_pixels;
            }

            if ((image.PixelFormat == PixelFormat.DXT1) ||
                (image.PixelFormat == PixelFormat.DXT3) ||
                (image.PixelFormat == PixelFormat.DXT5))
            {
                int levels = image.Levels;
                if ((levels > 1) && CompressedTextureContainsAllMipmaps(image))
                {
                    Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST_MIPMAP_LINEAR);
                }
                else
                {
                    levels = 1;
                    Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
                }

                if (image.IsCubeMap)
                    CompressedTextureLoadPixelDataCUBEMAP(image, image.Levels);
                else 
                    CompressedTextureLoadPixelData(image, levels);
            }
            else
            {
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST_MIPMAP_LINEAR);
                Glu.gluBuild2DMipmaps(
                    Gl.GL_TEXTURE_2D,
                    image.ElementsPerPixel,
                    image.Width,
                    image.Height,
                    image.OpenGlPixelFormat,
                    Gl.GL_UNSIGNED_BYTE,
                    image.Pixels);
            }
            Util3D.ReportErrors();
            return textureName;
        }

        private bool CompressedTextureContainsAllMipmaps(Image image)
        {
            // Check we have a full mipchain which is compulsory for glCompressedTexImage2DARB
            // If not, then we limit the texture to be non-mipmapped
            int longestSide = (image.Width > image.Height) ? image.Width : image.Height;
            int levelsInFullChain = 1;
            while (longestSide != 1)
            {
                longestSide /= 2;
                levelsInFullChain++;
            }

            return (image.Levels == levelsInFullChain);
        }

        private unsafe void CompressedTextureLoadPixelData(Image image, int levels)
        {
            IntPtr glCompressedTexImage2DARB =
                Gl.GetFunctionPointerForExtensionMethod("glCompressedTexImage2DARB");
            if (glCompressedTexImage2DARB == IntPtr.Zero)
                    return;

            int nBlockSize;

            if (image.OpenGlPixelFormat == Gl.GL_COMPRESSED_RGBA_S3TC_DXT1_EXT)
                nBlockSize = 8;
            else
                nBlockSize = 16;

            int nWidth = image.Width;
            int nHeight = image.Height;
            int nSize;
            int nOffset = 0;

            // Load the mip-map levels
            fixed (byte* pixels = image.Pixels)
            {
                for (int i = 0; i < levels; ++i)
                {
                    if (nWidth == 0)
                        nWidth = 1;
                    if (nHeight == 0)
                        nHeight = 1;

                    nSize = ((nWidth + 3) / 4) * ((nHeight + 3) / 4) * nBlockSize;

                    Gl.glCompressedTexImage2DARB(
                        Gl.GL_TEXTURE_2D,
                        i,
                        image.OpenGlPixelFormat,
                        nWidth,
                        nHeight,
                        0,
                        nSize,
                        new IntPtr(pixels + nOffset));

                    nOffset += nSize;

                    // Half the image size for the next mip-map level...
                    nWidth = (nWidth / 2);
                    nHeight = (nHeight / 2);
                }
            }
            Util3D.ReportErrors();
        }

        /// <summary>
        /// Loads a compressed texture using a call to glCompressedTexImage2DARB() as CUBE_MAP</summary>
        /// <param name="image">The Image</param>
        /// <param name="levels">Mipmap levels</param>
        private unsafe void CompressedTextureLoadPixelDataCUBEMAP(Image image, int levels)
        {
            IntPtr glCompressedTexImage2DARB =
                Gl.GetFunctionPointerForExtensionMethod("glCompressedTexImage2DARB");
            if (glCompressedTexImage2DARB == IntPtr.Zero)
                return;

            int nBlockSize;

            if (image.OpenGlPixelFormat == Gl.GL_COMPRESSED_RGBA_S3TC_DXT1_EXT)
                nBlockSize = 8;
            else
                nBlockSize = 16;

            int offset = 0;
            // Load the mip-map levels
            for (int face = 0; face < 6; face++)
            {
                int nWidth = image.Width;
                int nHeight = image.Height;
                for (int i = 0; i < levels; ++i)
                {
                    if (nWidth == 0)
                        nWidth = 1;
                    if (nHeight == 0)
                        nHeight = 1;

                    //                    nSize = ((nWidth + 3) / 4) * ((nHeight + 3) / 4) * nBlockSize;                    
                    int nSize = ((nWidth + 3) >> 2) * ((nHeight + 3) >> 2) * nBlockSize;

                    byte[] pixel = new byte[nSize];
                    for (int o = offset, j=0; o < offset + nSize; o++, j++)
                    {
                        pixel[j] = image.Pixels[o];
                    }
                    Gl.glCompressedTexImage2DARB(
                        Gl.GL_TEXTURE_CUBE_MAP_POSITIVE_X + face,
                        i,
                        image.OpenGlPixelFormat,
                        nWidth,
                        nHeight,
                        0,
                        nSize,
                        pixel);

                    offset += nSize;

                    // Half the image size for the next mip-map level...
                    nWidth = (nWidth >> 1);
                    nHeight = (nHeight >> 1);
                }
            }
            Util3D.ReportErrors();
        }

        /// <summary>
        /// Holds a related group of textures</summary>
        private class TextureContext
        {
            public TextureContext(object contextId)
            {
                ContextId = contextId;
            }

            public readonly object ContextId;
            public readonly Dictionary<String, int> NameMap = new Dictionary<string, int>();
            public readonly Dictionary<int, TextureInfo> TextureDataMap = new Dictionary<int, TextureInfo>();
            public readonly List<int> Names = new List<int>();
        }

        private readonly List<TextureContext> m_texCollections = new List<TextureContext>();
        private readonly ImageLoaderRegistry m_imageLoaderRegistry = new ImageLoaderRegistry();
        private int m_missingTextureReports;//to avoid overloading the OutputService window
        
        private static readonly object s_defaultContext = new object();
    }
}
