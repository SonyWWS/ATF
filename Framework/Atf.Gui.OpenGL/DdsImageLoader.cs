//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;
using System.Runtime.InteropServices;

//using Tao.OpenGl;
using OTK = OpenTK.Graphics;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// ImageLoader that can load DXT1/3/5 textures from a stream and convert to a texture image</summary>
    public class DdsImageLoader : IImageLoader
    {
        /// <summary>
        /// Loads texture image from stream</summary>
        /// <param name="imageStream">Stream holding texture image</param>
        /// <returns>Texture image</returns>
        public Image LoadImage(Stream imageStream)
        {
            using (BinaryReader reader = new BinaryReader(imageStream))
            {
                DDSURFACEDESC2 ddsd = new DDSURFACEDESC2();
                ddsd.Load(reader);

                int glPixelFormat;
                int elementsPerPixel;
                byte[] pixels = ReadPixels(ref ddsd, reader, out glPixelFormat, out elementsPerPixel);
                int mipMapCount = ddsd.HasMipMapCount ? ddsd.dwMipMapCount : 1;

                Image image = new Image(
                  ddsd.dwWidth,
                  ddsd.dwHeight,
                  pixels,
                  mipMapCount,
                  glPixelFormat,
                  elementsPerPixel);

                return image;
            }
        }

        private byte[] ReadPixels(ref DDSURFACEDESC2 ddsd, BinaryReader reader,
            out int glPixelFormat, out int elementsPerPixel)
        {
            // How big will the buffer need to be to load all of the pixel data including mip-maps?
            int bufferSize;
            int compressionFactor;
            PixelFormatConverter converter;
            CalculateImageSettings(
                ref ddsd,
                out compressionFactor,
                out glPixelFormat,
                out elementsPerPixel,
                out converter);

            DDSCAPS2 ddsCaps = ddsd.ddsCaps;
            bool isCubeMap = (ddsCaps.dwCaps2 & DDSURFACEDESC2.DDSCAPS2_CUBEMAP) != 0;
            int mipMapCount = ddsd.HasMipMapCount ? ddsd.dwMipMapCount : 1;
            byte[] pixels;

            if (isCubeMap)
            {   // image is a cubemap
                bufferSize = 0;
                for (int face = 0; face < 6; face++)
                {
                    int width = ddsd.dwWidth;
                    int height = ddsd.dwHeight;

                    for (int level = 0; level < mipMapCount; level++)
                    {
                        //calculate the bufferSize we are going to read
                        bufferSize += ((width + 3) >> 2) * ((height + 3) >> 2) * 8;
                        width = width >> 1;
                        if (width < 1)
                            width = 1;
                        height = height >> 1;
                        if (height < 1)
                            height = 1;
                    }
                }
                //read the data into pixels: note that all faces and mipmaps are being read
                pixels = reader.ReadBytes(bufferSize);
            }
            else
            {   // image is not a cubemap
                if (ddsd.HasPitch)
                {
                    if (mipMapCount > 1)
                        throw new NotSupportedException("please request support for DDS textures with pitch specified and more than one mipmap");
                    bufferSize = ddsd.lPitch * ddsd.dwHeight;
                }
                else if (ddsd.HasLinearSize && ddsd.dwLinearSize > 0 && (mipMapCount == 1 || compressionFactor > 0))
                {
                    if (mipMapCount > 1)
                        bufferSize = ddsd.dwLinearSize * compressionFactor;
                    else
                        bufferSize = ddsd.dwLinearSize;
                }
                else
                {
                    // Read until end of file. DDS files with multiple mipmaps come through here.
                    long cur = reader.BaseStream.Position;
                    long eof = reader.BaseStream.Length;
                    bufferSize = (int)(eof - cur);
                }
                pixels = reader.ReadBytes(bufferSize);
            }

            pixels = converter(pixels);
            return pixels;
        }

        private void CalculateImageSettings(
            ref DDSURFACEDESC2 ddsd,
            out int compressionFactor,
            out int glPixelFormat,
            out int elementsPerPixel,
            out PixelFormatConverter converter)
        {
            converter = DoNothingConverter;
            DDPIXELFORMAT pixelFormat = ddsd.ddpfPixelFormat;
            if ((pixelFormat.dwFlags & DDPIXELFORMAT.DDPF_RGB) != 0)
            {
                // Uncompressed DXT
                switch (pixelFormat.dwRGBBitCount)
                {
                    case 32:
                        elementsPerPixel = 4;
                        if (pixelFormat.HasAlphaRedGreenBlueMasks(
                            0xFF000000,
                            0x00FF0000,
                            0x0000FF00,
                            0x000000FF))
                        {
                            glPixelFormat = (int)OTK.OpenGL.PixelFormat.Bgra;
                        }
                        else if (pixelFormat.HasAlphaRedGreenBlueMasks(
                            0xFF000000,
                            0x000000FF,
                            0x0000FF00,
                            0x00FF0000))
                        {
                            glPixelFormat = (int)OTK.OpenGL.PixelFormat.Rgba;
                        }
                        else if (pixelFormat.HasRedGreenBlueMasks(
                            0x00FF0000,
                            0x0000FF00,
                            0x000000FF))
                        {
                            glPixelFormat = (int)OTK.OpenGL.PixelFormat.Bgr;
                            converter = Create32BitTo24BitConverter(0, 1, 2);
                            elementsPerPixel = 3;
                        }
                        else if (pixelFormat.HasRedGreenBlueMasks(
                            0x000000FF,
                            0x0000FF00,
                            0x00FF0000))
                        {
                            glPixelFormat = (int)OTK.OpenGL.PixelFormat.Rgb;
                            converter = Create32BitTo24BitConverter(0, 1, 2);
                            elementsPerPixel = 3;
                        }
                        else
                        {
                            glPixelFormat = (int)OTK.OpenGL.PixelFormat.Rgba;
                        }
                        break;
                    case 24:
                        elementsPerPixel = 3;
                        if (pixelFormat.HasRedGreenBlueMasks(
                            0x00FF0000,
                            0x0000FF00,
                            0x000000FF))
                        {
                            glPixelFormat = (int)OTK.OpenGL.PixelFormat.Bgr;
                        }
                        else
                        {
                            glPixelFormat = (int)OTK.OpenGL.PixelFormat.Rgb;
                        }
                        break;
                    default:
                        throw new NotSupportedException("unhandled pixel format in dds file");
                }
                compressionFactor = 0;
                return;
            }

            // compressed?
            switch (pixelFormat.dwFourCC)
            {
                case DDPIXELFORMAT.FOURCC_DXT1:
                    // DXT1's compression ratio is 8:1
                    compressionFactor = 2;
                    glPixelFormat = (int)OTK.OpenGL.PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
                    elementsPerPixel = 3;
                    break;

                case DDPIXELFORMAT.FOURCC_DXT3:
                    // DXT3's compression ratio is 4:1
                    compressionFactor = 4;
                    glPixelFormat = (int)OTK.OpenGL.PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
                    elementsPerPixel = 4;
                    break;

                case DDPIXELFORMAT.FOURCC_DXT5:
                    // DXT5's compression ratio is 4:1
                    compressionFactor = 4;
                    glPixelFormat = (int)OTK.OpenGL.PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
                    elementsPerPixel = 4;
                    break;

                default:
                    throw new NotSupportedException("Unsupported DXT format");
            }
        }

        private delegate byte[] PixelFormatConverter(byte[] source);

        private byte[] DoNothingConverter(byte[] source)
        {
            return source;
        }

        // Returns a delegate that maps each source pixel's 8 bit color channel to
        //  a destination 8 bit color channel using indices in the range [0,3] for
        //  the source channels.
        private PixelFormatConverter Create32BitConverter(
            int sourceColor1, int sourceColor2, int sourceColor3, int sourceColor4)
        {
            return delegate(byte[] source)
            {
                for (int i = 0; i < source.Length; i += 4)
                {
                    byte c1 = source[i + sourceColor1];
                    byte c2 = source[i + sourceColor2];
                    byte c3 = source[i + sourceColor3];
                    byte c4 = source[i + sourceColor4];
                    source[i] = c1;
                    source[i + 1] = c2;
                    source[i + 2] = c3;
                    source[i + 3] = c4;
                }
                return source;
            };
        }

        // Returns a delegate that converts each 32 bit pixel to a 24 bit pixel by
        //  mapping source 8-bit color channels to the destination 8-bit color channels.,
        //  Source indices are in the range [0,3] and specify which channel goes in to
        //  the first destination channel, 2nd, and 3rd.
        private PixelFormatConverter Create32BitTo24BitConverter(
            int sourceColor1, int sourceColor2, int sourceColor3)
        {
            return delegate(byte[] source)
            {
                int pixels = source.Length / 4;
                byte[] dest = new byte[pixels * 3];

                int sourceIndex = 0;
                int destIndex = 0;
                while (sourceIndex < source.Length)
                {
                    dest[destIndex] = source[sourceIndex + sourceColor1];
                    dest[destIndex + 1] = source[sourceIndex + sourceColor2];
                    dest[destIndex + 2] = source[sourceIndex + sourceColor3];

                    sourceIndex += 4;
                    destIndex += 3;
                }
                return dest;
            };
        }

        // This is public only to allow NativeTestHelpers to access it.
        // Making this 'internal' and marking this assembly with InternalsVisibleToAttribute
        //  would not work with Visual Studio's test runner and there are probably compile
        //  dependency problems, too.
        [StructLayout(LayoutKind.Explicit)]
        public struct DDSCAPS2
        {
            [FieldOffset(0)]
            public Int32 dwCaps;         // capabilities of surface wanted
            [FieldOffset(4)]
            public Int32 dwCaps2;
            [FieldOffset(8)]
            public Int32 dwCaps3;

            [FieldOffset(12)]
            public Int32 dwCaps4;
            [FieldOffset(12)]
            public Int32 dwVolumeDepth;

            public void Load(BinaryReader reader)
            {
                dwCaps = reader.ReadInt32();
                dwCaps2 = reader.ReadInt32();
                dwCaps3 = reader.ReadInt32();
                dwCaps4 = reader.ReadInt32();
            }
        }

        // This is public only to allow NativeTestHelpers to access it.
        [StructLayout(LayoutKind.Explicit)]
        public struct DDCOLORKEY
        {
            [FieldOffset(0)]
            public Int32 dwColorSpaceLowValue;   // low boundary of color space that is to
            // be treated as Color Key, inclusive
            [FieldOffset(4)]
            public Int32 dwColorSpaceHighValue;  // high boundary of color space that is
            // to be treated as Color Key, inclusive
        }

        // This is public only to allow NativeTestHelpers to access it.
        [StructLayout(LayoutKind.Explicit)]
        public struct DDPIXELFORMAT
        {
            [FieldOffset(0)]
            public Int32 dwSize;                 // size of structure
            [FieldOffset(4)]
            public Int32 dwFlags;                // pixel format flags
            [FieldOffset(8)]
            public Int32 dwFourCC;               // (FOURCC code)

            [FieldOffset(12)]
            public Int32 dwRGBBitCount;          // how many bits per pixel
            [FieldOffset(12)]
            public Int32 dwYUVBitCount;          // how many bits per pixel
            [FieldOffset(12)]
            public Int32 dwZBufferBitDepth;      // how many total bits/pixel in z buffer (including any stencil bits)
            [FieldOffset(12)]
            public Int32 dwAlphaBitDepth;        // how many bits for alpha channels
            [FieldOffset(12)]
            public Int32 dwLuminanceBitCount;    // how many bits per pixel
            [FieldOffset(12)]
            public Int32 dwBumpBitCount;         // how many bits per "buxel", total
            [FieldOffset(12)]
            public Int32 dwPrivateFormatBitCount;// Bits per pixel of private driver formats. Only valid in texture

            [FieldOffset(16)]
            public UInt32 dwRBitMask;             // mask for red bit
            [FieldOffset(16)]
            public UInt32 dwYBitMask;             // mask for Y bits
            [FieldOffset(16)]
            public UInt32 dwStencilBitDepth;      // how many stencil bits (note: dwZBufferBitDepth-dwStencilBitDepth is total Z-only bits)
            [FieldOffset(16)]
            public UInt32 dwLuminanceBitMask;     // mask for luminance bits
            [FieldOffset(16)]
            public UInt32 dwBumpDuBitMask;        // mask for bump map U delta bits
            [FieldOffset(16)]
            public UInt32 dwOperations;           // DDPF_D3DFORMAT Operations

            [FieldOffset(20)]
            public UInt32 dwGBitMask;             // mask for green bits
            [FieldOffset(20)]
            public UInt32 dwUBitMask;             // mask for U bits
            [FieldOffset(20)]
            public UInt32 dwZBitMask;             // mask for Z bits
            [FieldOffset(20)]
            public UInt32 dwBumpDvBitMask;        // mask for bump map V delta bits
            [FieldOffset(20)]
            public _MultiSampleCaps MultiSampleCaps;

            [FieldOffset(24)]
            public UInt32 dwBBitMask;             // mask for blue bits
            [FieldOffset(24)]
            public UInt32 dwVBitMask;             // mask for V bits
            [FieldOffset(24)]
            public UInt32 dwStencilBitMask;       // mask for stencil bits
            [FieldOffset(24)]
            public UInt32 dwBumpLuminanceBitMask; // mask for luminance in bump map

            [FieldOffset(28)]
            public UInt32 dwRGBAlphaBitMask;      // mask for alpha channel
            [FieldOffset(28)]
            public UInt32 dwYUVAlphaBitMask;      // mask for alpha channel
            [FieldOffset(28)]
            public UInt32 dwLuminanceAlphaBitMask;// mask for alpha channel
            [FieldOffset(28)]
            public UInt32 dwRGBZBitMask;          // mask for Z channel
            [FieldOffset(28)]
            public UInt32 dwYUVZBitMask;          // mask for Z channel

            [StructLayout(LayoutKind.Explicit)]
            public struct _MultiSampleCaps
            {
                [FieldOffset(0)]
                public Int16 wFlipMSTypes;       // Multisample methods supported via flip for this D3DFORMAT
                [FieldOffset(2)]
                public Int16 wBltMSTypes;        // Multisample methods supported via blt for this D3DFORMAT
            }

            public bool HasAlphaRedGreenBlueMasks(UInt32 alpha, UInt32 red, UInt32 green, UInt32 blue)
            {
                return
                    (dwFlags & (DDPIXELFORMAT.DDPF_RGB | DDPIXELFORMAT.DDPF_ALPHAPIXELS)) ==
                        (DDPIXELFORMAT.DDPF_RGB | DDPIXELFORMAT.DDPF_ALPHAPIXELS) &&
                    dwRGBAlphaBitMask == alpha &&
                    dwRBitMask == red &&
                    dwGBitMask == green &&
                    dwBBitMask == blue;
            }

            public bool HasRedGreenBlueMasks(UInt32 red, UInt32 green, UInt32 blue)
            {
                return
                    (dwFlags & DDPIXELFORMAT.DDPF_RGB) != 0 &&
                    dwRBitMask == red &&
                    dwGBitMask == green &&
                    dwBBitMask == blue;
            }

            public void Load(BinaryReader reader)
            {
                dwSize = reader.ReadInt32();
                if (dwSize != 32)
                    throw new InvalidDataException("DDPIXELFORMAT must be 32 bytes in DDS file specification");
                dwFlags = reader.ReadInt32();
                dwFourCC = reader.ReadInt32();
                dwRGBBitCount = reader.ReadInt32();
                dwRBitMask = reader.ReadUInt32();
                dwGBitMask = reader.ReadUInt32();
                dwBBitMask = reader.ReadUInt32();
                dwRGBAlphaBitMask = reader.ReadUInt32();
            }

            public const int Size = 32;

            public const int FOURCC_DXT1 = (int)'D' + ((int)'X' << 8) + ((int)'T' << 16) + ((int)'1' << 24);
            public const int FOURCC_DXT2 = (int)'D' + ((int)'X' << 8) + ((int)'T' << 16) + ((int)'2' << 24);
            public const int FOURCC_DXT3 = (int)'D' + ((int)'X' << 8) + ((int)'T' << 16) + ((int)'3' << 24);
            public const int FOURCC_DXT4 = (int)'D' + ((int)'X' << 8) + ((int)'T' << 16) + ((int)'4' << 24);
            public const int FOURCC_DXT5 = (int)'D' + ((int)'X' << 8) + ((int)'T' << 16) + ((int)'5' << 24);

            // dwFlags
            public const int DDPF_ALPHAPIXELS = 0x00000001;//The surface has alpha channel information in the pixel format.
            public const int DDPF_FOURCC = 0x00000004;//The FourCC code is valid.
            public const int DDPF_RGB = 0x00000040;//The RGB data in the pixel format structure is valid.
        }

        // This is public only to allow NativeTestHelpers to access it.
        [StructLayout(LayoutKind.Explicit)]
        public struct DDSURFACEDESC2
        {
            public const int Size = 124;
            [FieldOffset(0)]
            private Int32 m_dwSize;                 // size of the DDSURFACEDESC structure

            public int Flags { get { return m_dwFlags; } }
            [FieldOffset(4)]
            private Int32 m_dwFlags;                // determines what fields are valid

            public bool HasHeight { get { return IsSet(DDSD_HEIGHT); } }
            public Int32 dwHeight { get { CheckSet(DDSD_HEIGHT); return m_dwHeight; } }
            [FieldOffset(8)]
            private Int32 m_dwHeight;               // height of surface to be created

            public bool HasWidth { get { return IsSet(DDSD_WIDTH); } }
            public Int32 dwWidth { get { CheckSet(DDSD_WIDTH); return m_dwWidth; } }
            [FieldOffset(12)]
            private Int32 m_dwWidth;                // width of input surface

            public bool HasPitch { get { return IsSet(DDSD_PITCH); } }
            public Int32 lPitch { get { CheckSet(DDSD_PITCH); return m_lPitch; } }
            [FieldOffset(16)]
            private Int32 m_lPitch;                 // distance to start of next line (return value only)

            public bool HasLinearSize { get { return IsSet(DDSD_LINEARSIZE); } }
            public Int32 dwLinearSize { get { CheckSet(DDSD_LINEARSIZE); return m_dwLinearSize; } }
            [FieldOffset(16)]
            private Int32 m_dwLinearSize;           // Formless late-allocated optimized surface size

            public bool HasBackBufferCount { get { return IsSet(DDSD_BACKBUFFERCOUNT); } }
            public Int32 dwBackBufferCount { get { CheckSet(DDSD_BACKBUFFERCOUNT); return m_dwBackBufferCount; } }
            [FieldOffset(20)]
            private Int32 m_dwBackBufferCount;      // number of back buffers requested

            public bool HasDepth { get { return IsSet(DDSD_DEPTH); } }
            public Int32 dwDepth { get { CheckSet(DDSD_DEPTH); return m_dwDepth; } }
            [FieldOffset(20)]
            private Int32 m_dwDepth;                // the depth if this is a volume texture 

            public bool HasMipMapCount { get { return IsSet(DDSD_MIPMAPCOUNT); } }
            public Int32 dwMipMapCount { get { CheckSet(DDSD_MIPMAPCOUNT); return m_dwMipMapCount; } }
            [FieldOffset(24)]
            private Int32 m_dwMipMapCount;          // number of mip-map levels requested

            public bool HasRefreshRate { get { return IsSet(DDSD_REFRESHRATE); } }
            public Int32 dwRefreshRate { get { CheckSet(DDSD_REFRESHRATE); return m_dwRefreshRate; } }
            [FieldOffset(24)]
            private Int32 m_dwRefreshRate;          // refresh rate (used when display mode is described)

            [FieldOffset(24)]
            private Int32 m_dwSrcVBHandle;          // The source used in VB::Optimize

            public bool HasAlphaBitDepth { get { return IsSet(DDSD_ALPHABITDEPTH); } }
            public Int32 dwAlphaBitDepth { get { CheckSet(DDSD_ALPHABITDEPTH); return m_dwAlphaBitDepth; } }
            [FieldOffset(28)]
            private Int32 m_dwAlphaBitDepth;        // depth of alpha buffer requested

            [FieldOffset(32)]
            private Int32 m_dwReserved;             // reserved

            [FieldOffset(36)]
            private IntPtr m_lpSurface;              // pointer to the associated surface memory

            public bool HasCKDestOverlay { get { return IsSet(DDSD_CKDESTOVERLAY); } }
            public DDCOLORKEY ddckCKDestOverlay { get { CheckSet(DDSD_CKDESTOVERLAY); return m_ddckCKDestOverlay; } }
            #if X64
            [FieldOffset(44)]
            #elif X86
            [FieldOffset(40)]
            #endif
            private DDCOLORKEY m_ddckCKDestOverlay;      // color key for destination overlay use

            public bool HasEmptyFaceColor { get { return !IsSet(DDSD_CKDESTOVERLAY); } }
            public Int32 dwEmptyFaceColor { get { CheckNotSet(DDSD_CKDESTOVERLAY); return m_dwEmptyFaceColor; } }
            #if X64
            [FieldOffset(44)]
            #elif X86
            [FieldOffset(40)]
            #endif
            private Int32 m_dwEmptyFaceColor;       // Physical color for empty cubemap faces

            public bool HasCKDestBlt { get { return IsSet(DDSD_CKDESTBLT); } }
            public DDCOLORKEY ddckCKDestBlt { get { CheckSet(DDSD_CKDESTBLT); return m_ddckCKDestBlt; } }
            #if X64
            [FieldOffset(52)]
            #elif X86
            [FieldOffset(48)]
            #endif
            private DDCOLORKEY m_ddckCKDestBlt;          // color key for destination blt use

            public bool HasCKSrcOverlay { get { return IsSet(DDSD_CKSRCOVERLAY); } }
            public DDCOLORKEY ddckCKSrcOverlay { get { CheckSet(DDSD_CKSRCOVERLAY); return m_ddckCKSrcOverlay; } }
            #if X64
            [FieldOffset(60)]
            #elif X86
            [FieldOffset(56)]
            #endif
            private DDCOLORKEY m_ddckCKSrcOverlay;       // color key for source overlay use

            public bool HasCKSrcBlt { get { return IsSet(DDSD_CKSRCBLT); } }
            public DDCOLORKEY ddckCKSrcBlt { get { CheckSet(DDSD_CKSRCBLT); return m_ddckCKSrcBlt; } }
            #if X64
            [FieldOffset(68)]
            #elif X86
            [FieldOffset(64)]
            #endif
            private DDCOLORKEY m_ddckCKSrcBlt;           // color key for source blt use

            public bool HasPixelFormat { get { return IsSet(DDSD_PIXELFORMAT); } }
            public DDPIXELFORMAT ddpfPixelFormat { get { CheckSet(DDSD_PIXELFORMAT); return m_ddpfPixelFormat; } }
            #if X64
            [FieldOffset(76)]
            #elif X86
            [FieldOffset(72)]
            #endif
            private DDPIXELFORMAT m_ddpfPixelFormat;        // pixel format description of the surface

            public bool HasFVF { get { return IsSet(DDSD_FVF); } }
            public Int32 dwFVF { get { CheckSet(DDSD_FVF); return m_dwFVF; } }
            #if X64
            [FieldOffset(76)]
            #elif X86
            [FieldOffset(72)]
            #endif
            private Int32 m_dwFVF;                  // vertex format description of vertex buffers

            public bool HasCaps { get { return IsSet(DDSD_CAPS); } }
            public DDSCAPS2 ddsCaps { get { CheckSet(DDSD_CAPS); return m_ddsCaps; } }
            #if X64
            [FieldOffset(108)]
            #elif X86
            [FieldOffset(104)]
            #endif
            private DDSCAPS2 m_ddsCaps;                // direct draw surface capabilities

            public bool HasTextureStage { get { return IsSet(DDSD_TEXTURESTAGE); } }
            public Int32 dwTextureStage { get { CheckSet(DDSD_TEXTURESTAGE); return m_dwTextureStage; } }
            #if X64
            [FieldOffset(124)]
            #elif X86
            [FieldOffset(120)]
            #endif
            private Int32 m_dwTextureStage;         // stage in multitexture cascade

            //dwFlags
            public const int DDSD_CAPS = 0x00000001; //ddsCaps field is valid.
            public const int DDSD_HEIGHT = 0x00000002; //dwHeight field is valid.
            public const int DDSD_WIDTH = 0x00000004; //dwWidth field is valid.
            public const int DDSD_PITCH = 0x00000008; //lPitch is valid.
            public const int DDSD_BACKBUFFERCOUNT = 0x00000020; //dwBackBufferCount is valid.
            public const int DDSD_ZBUFFERBITDEPTH = 0x00000040; //dwZBufferBitDepth is valid.  (shouldnt be used in DDSURFACEDESC2)
            public const int DDSD_ALPHABITDEPTH = 0x00000080; //dwAlphaBitDepth is valid.
            public const int DDSD_LPSURFACE = 0x00000800; //lpSurface is valid.
            public const int DDSD_PIXELFORMAT = 0x00001000; //ddpfPixelFormat is valid.
            public const int DDSD_CKDESTOVERLAY = 0x00002000; //ddckCKDestOverlay is valid.
            public const int DDSD_CKDESTBLT = 0x00004000; //ddckCKDestBlt is valid.
            public const int DDSD_CKSRCOVERLAY = 0x00008000; //ddckCKSrcOverlay is valid.
            public const int DDSD_CKSRCBLT = 0x00010000; //ddckCKSrcBlt is valid.
            public const int DDSD_MIPMAPCOUNT = 0x00020000; //dwMipMapCount is valid. field is valid.
            public const int DDSD_REFRESHRATE = 0x00040000; //dwRefreshRate is valid
            public const int DDSD_LINEARSIZE = 0x00080000; //dwLinearSize is valid
            public const int DDSD_TEXTURESTAGE = 0x00100000; //dwTextureStage is valid
            public const int DDSD_FVF = 0x00200000; //dwFVF is valid
            public const int DDSD_SRCVBHANDLE = 0x00400000; //dwSrcVBHandle is valid
            public const int DDSD_DEPTH = 0x00800000; //dwDepth is valid
            public const int DDSD_ALL = 0x00fff9ee; //All input fields are valid.

            public const int DDSCAPS_COMPLEX = 0x00000008;
            public const int DDSCAPS_TEXTURE = 0x00001000;
            public const int DDSCAPS_MIPMAP = 0x00400000;

            public const int DDSCAPS2_CUBEMAP = 0x00000200;         // cube map mask
            public const int DDSCAPS2_CUBEMAP_POSITIVEX = 0x00000400;
            public const int DDSCAPS2_CUBEMAP_NEGATIVEX = 0x00000800;
            public const int DDSCAPS2_CUBEMAP_POSITIVEY = 0x00001000;
            public const int DDSCAPS2_CUBEMAP_NEGATIVEY = 0x00002000;
            public const int DDSCAPS2_CUBEMAP_POSITIVEZ = 0x00004000;
            public const int DDSCAPS2_CUBEMAP_NEGATIVEZ = 0x00008000;
            public const int DDSCAPS2_VOLUME = 0x00200000;

            // Ensures that this bit is set in dwFlags.
            public void CheckSet(int flag)
            {
                if ((m_dwFlags & flag) == 0)
                    throw new InvalidOperationException(string.Format("DDS texture did not have flag 0x{0:x} set.", flag));
            }

            // Ensures that this bit is NOT set in dwFlags.
            public void CheckNotSet(int flag)
            {
                if ((m_dwFlags & flag) != 0)
                    throw new InvalidOperationException(string.Format("DDS texture must not have flag 0x{0:x} set.", flag));
            }

            // Returns whether or not this bit is set in dwFlags.
            public bool IsSet(int flag)
            {
                return ((m_dwFlags & flag) != 0);
            }

            // Reads in the header, fills in this struct, and reads in all of the pixels.
            public void Load(BinaryReader reader)
            {
                byte[] header = reader.ReadBytes(4);
                if (header[0] != 'D' || header[1] != 'D' || header[2] != 'S' || header[3] != ' ')
                    throw new InvalidDataException("Not a DDS file");

                m_dwSize = reader.ReadInt32();
                if (m_dwSize != 124)
                    throw new InvalidDataException("DDSURFACEDESC2 must be 124 bytes in DDS file specification");

                // assign all the fields
                m_dwFlags = reader.ReadInt32();
                m_dwHeight = reader.ReadInt32();
                m_dwWidth = reader.ReadInt32();
                m_dwLinearSize = reader.ReadInt32();
                m_dwBackBufferCount = reader.ReadInt32();
                m_dwMipMapCount = reader.ReadInt32();
                m_dwAlphaBitDepth = reader.ReadInt32();
                m_dwReserved = reader.ReadInt32();
                m_lpSurface = (IntPtr)reader.ReadInt32();
                m_ddckCKDestOverlay.dwColorSpaceLowValue = reader.ReadInt32();
                m_ddckCKDestOverlay.dwColorSpaceHighValue = reader.ReadInt32();
                m_ddckCKDestBlt.dwColorSpaceLowValue = reader.ReadInt32();
                m_ddckCKDestBlt.dwColorSpaceHighValue = reader.ReadInt32();
                m_ddckCKSrcOverlay.dwColorSpaceLowValue = reader.ReadInt32();
                m_ddckCKSrcOverlay.dwColorSpaceHighValue = reader.ReadInt32();
                m_ddckCKSrcBlt.dwColorSpaceLowValue = reader.ReadInt32();
                m_ddckCKSrcBlt.dwColorSpaceHighValue = reader.ReadInt32();

                m_ddpfPixelFormat.Load(reader);

                m_ddsCaps.Load(reader);

                m_dwTextureStage = reader.ReadInt32();
            }
        }
    }
}
