//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;
using System.Linq;

using Tao.OpenGl;

using Devil = DevILSharp;
using DevILSharp;
using System.Runtime.InteropServices;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// IImageLoader class for loading images using Tao.DevIl. Supports all of the image formats of DevIl.
    /// DXT1, DXT3, and DXT5 are supported without decompressing them. Uncompressed formats are converted,
    /// if necessary, to either 24 bit BGR or 32 bit BGRA format for use by OpenGL.
    /// NOTE: As of DevIl.dll version 0.1.7.8, DXT1 textures with mipmaps seem to lead to OpenGL corruption.
    /// Use the DdsImageLoader instead for compressed *.dds textures. --Ron
    /// NOTE 2: There may be compatibility problems with VS2012. Turning off "Enable the Visual Studio hosting process"
    /// on your project's debugging settings seems to fix this problem. http://tracker.ship.scea.com/jira/browse/WWSATF-1307 </summary>
    /// <remarks>
    /// Add the following five native DLLs to your project and set your project platform to x86:
    ///   cg.dll, cgGL.dll, DevIL.dll, ILU.dll, ILUT.dll
    /// These five dlls can be found in wws_atf\ThirdParty\Tao.OpenGl .
    ///
    /// Supported file extensions include:
    /// .bmp
    /// .dds (DXT1, DXT3, DXT5, any of the uncompressed formats, with or without mipmaps, cube map, volume map)
    /// .gif
    /// .icns,.ico,.cur
    /// .jpg,jpe,jpeg
    /// .jp2
    /// .jng
    /// .png
    /// .psd
    /// .tga
    /// .tiff,.tif
    /// ...  and many more. Call the static GetSupportedExtensions() for all of them.</remarks>
    [Obsolete("Has memory corruption problems. https://github.com/SonyWWS/ATF/issues/9")]
    public class DevilImageLoader : IImageLoader
    {
        public static void InitDevil()
        {
            if (s_initialized) return;
            s_initialized = true;

            Bootstrap.Init();
            IL.Init();
            IL.Enable(EnableCap.AbsoluteOrigin);
            IL.OriginFunc(OriginMode.UpperLeft);
            IL.Enable(EnableCap.OverwriteExistingFile);
            IL.Enable(EnableCap.ConvertPalette);
        }

        /// <summary>
        /// Constructs a DevilImageLoader object that loads images implied by the given file
        /// extension. If that fails, the binary data is inspected to try to determine
        /// the true format.</summary>
        /// <param name="extension">The image file extension, including the leading '.', for
        /// example, ".dds"</param>
        public DevilImageLoader(string extension)
        {
            m_extension = extension;

            InitDevil();
        }

        /// <summary>
        /// Gets an array of all of the recognized file extensions for loading. The extensions
        /// are lower case, with the leading '.'.</summary>
        /// <returns>Array of file extensions, such as {".bmp",".gif", ...}</returns>
        static public string[] GetSupportedExtensions()
        {
            string[] extensions;
            try
            {
                //TODO: make the names in the below string array all start with lower case

                string[] devILSharpImageFormats = Enum.GetNames(typeof(Devil.ImageType));

                extensions = devILSharpImageFormats;
            }
            catch (BadImageFormatException)
            {
                extensions = new string[0];
            }

            return extensions;
        }

        // Since DevILSharp doesn't have Origin Mode Int set
        private static int ILOriginMode = 1539;

        /// <summary>
        /// Loads texture image from stream</summary>
        /// <param name="imageStream">Stream holding texture image</param>
        /// <param name="qualityLevel">Quality level parameter, cannot be negative 1, will find more info on this param shortly</param>
        /// <returns>Texture image</returns>
        public Image LoadImage(Stream imageStream)
        {
            // Has to be null-initialized due to no default Constructor
            Image image;

            Devil.IL.Init();

            ClearErrors();

            int devilImageId = Devil.IL.GenImage();

            try
            {
                Devil.IL.BindImage(devilImageId);
                CheckError();
                
                // Don't uncompress DXT1, DXT3 or DXT5 formats. OpenGl can handle them as-is.
                Devil.IL.SetInteger((int)Devil.CompressedDataFormat.KeepDxtData, 1);
                CheckError();

                // For targa files and others, we need to ensure a consistent origin.
                Devil.IL.Enable(EnableCap.AbsoluteOrigin);

                Devil.IL.SetInteger(ILOriginMode, (int)OriginMode.UpperLeft);
                CheckError();

                // Load from the memory stream in to DevIl's internal unmanaged memory.
                LoadFromStream(imageStream);

                // Determine the target file format. OpenGl supports DXT1, DXT3, and DXT5, so don't convert those.
                // Get those pixels out of DevIl and into a managed array of bytes.
                int width = Devil.IL.GetInteger((int)IntName.ImageWidth);
                int height = Devil.IL.GetInteger((int)IntName.ImageHeight);
                int numMipMaps = Devil.IL.GetInteger((int)IntName.ImageMipMapCount);
                if (numMipMaps == 0)
                    numMipMaps = 1;
                CheckError();

                byte[] targetBytes;
                int targetNumElements = 0;
                int targetOpenGlFormat = 0;

                int dxtFormat = Devil.IL.GetInteger((int)Devil.CompressedDataFormat.DxtcDataFormat);
                bool isDxt = (dxtFormat != (int)Devil.CompressedDataFormat.DxtNoCompression);

                if (isDxt)
                {
                    targetBytes = GetBytesFromDxt(dxtFormat, out targetNumElements, out targetOpenGlFormat);
                }
                else
                {
                    targetBytes = GetUncompressedBytes(width, height, out targetNumElements, out targetOpenGlFormat);
                }

                // Create our own Image object and return it.
                image = new Image(width, height, targetBytes, numMipMaps, targetOpenGlFormat, targetNumElements);

                // To-do:
                //  To properly support cube maps, we have to have a way of undoing the OpenGl state changes in
                //  TextureManager. Also, TextureManager.CompressedTextureLoadPixelDataCUBEMAP() crashed with
                //  a cube map in the AllTextureTypes.lvl sample level. Shen worked on this with Herbert Law.
                //bool isCubeMap = Il.ilGetInteger(Il.IL_IMAGE_CUBEFLAGS) != 0;
                //image.IsCubeMap = isCubeMap;
            }
            finally
            {
                Devil.IL.DeleteImage(devilImageId);
                CheckError();
            }

            return image;
        }

        private byte[] LoadFromStream(Stream imageStream)
        {
            // Load from memory into a new internal image structure. To determine the file type
            //  represented by this stream, use the file extension first, for maximum efficiency.
            // If the file extension is null or empty or not recognized, then 'type' is
            //  Il.IL_TYPE_UNKNOWN and DevIl will try to determine the format by examining the
            //  binary data. This is potentially much slower.
            int fileType = (int)ImageType.Unkwown;

            // Passing in null or empty strings places an error on the internal error stack which
            //  we would then have to clear. So, check first.
            if (!string.IsNullOrEmpty(m_extension))
                fileType = Array.IndexOf(Enum.GetValues(typeof(ImageType)),Devil.IL.TypeFromExt(m_extension));

            // Second try. If we have the actual file extension available, use that. Is much
            //  faster than doing the detective work of inspecting the binary data.
            if (fileType == (int)ImageType.Unkwown)
            {
                FileStream fileStream = imageStream as FileStream;
                if (fileStream != null)
                {
                    string extension = Path.GetExtension(fileStream.Name);
                    if (!string.IsNullOrEmpty(extension))
                        fileType = Array.IndexOf(Enum.GetValues(typeof(ImageType)), Devil.IL.TypeFromExt(m_extension));
                }
            }

            // The binary reader is somewhat faster than reading the bytes from the stream.
            int readSize = (int)(imageStream.Length - imageStream.Position);
            byte[] readBytes;
            using (BinaryReader binaryReader = new BinaryReader(imageStream))
            {
                readBytes = binaryReader.ReadBytes(readSize);
            }

            //Creating the IntPtr from the readBytes value
            IntPtr readBytesPtr = Marshal.AllocHGlobal(readBytes.Length);
            Marshal.Copy(readBytes, 0, readBytesPtr, readBytes.Length);
            Marshal.FreeHGlobal(readBytesPtr);

            // If the extension wasn't recognized, DevIl will likely still be able to figure
            //  out the actual format based on inspecting the binary data.
            if (!Devil.IL.LoadL((Devil.ImageType)fileType, readBytesPtr, readBytes.Length))
                throw new InvalidOperationException("DevilImageLoader failed to load image");
            CheckError();

            return readBytes;
        }

        private unsafe byte[] GetBytesFromDxt(int dxtFormat, out int targetNumElements, out int targetOpenGlFormat)
        {
            int targetSize = Devil.IL.GetDXTCData(IntPtr.Zero, 0, (Devil.CompressedDataFormat)dxtFormat);
            byte[] targetBytes = new byte[targetSize];

            fixed (byte* p = targetBytes)
            {
                IntPtr targetPtr = new IntPtr(p);
                int bytesCopied = Devil.IL.GetDXTCData(targetPtr, targetSize, (Devil.CompressedDataFormat)dxtFormat);
                if (bytesCopied != targetSize)
                    throw new InvalidOperationException("copied bytes didn't match expected #");
            }

            switch (dxtFormat)
            {
                case (int)Devil.CompressedDataFormat.Dxt1:
                    targetOpenGlFormat = Gl.GL_COMPRESSED_RGBA_S3TC_DXT1_EXT;
                    break;
                case (int)Devil.CompressedDataFormat.Dxt3:
                    targetOpenGlFormat = Gl.GL_COMPRESSED_RGBA_S3TC_DXT3_EXT;
                    break;
                case (int)Devil.CompressedDataFormat.Dxt5:
                    targetOpenGlFormat = Gl.GL_COMPRESSED_RGBA_S3TC_DXT5_EXT;
                    break;
                default:
                    // OpenGl doesn't support DXT2 or DXT4. DevIl does not seem to decompress DXT2 correctly
                    //  and will not load DXT4 currently. If it's important to a client, we could call
                    //  GetUncompressedBytes() at this point and hope for the best.
                    throw new InvalidOperationException("unsupported DXT format");
            }

            targetNumElements = 4;
            return targetBytes;
        }

        private unsafe byte[] GetUncompressedBytes(int width, int height, out int targetNumElements, out int targetOpenGlFormat)
        {
            targetNumElements = Devil.IL.GetInteger((int)Devil.IntName.ImageBytesPerPixel);
            if (targetNumElements != 3)
                targetNumElements = 4;

            int targetSize;
            targetSize = width * height * targetNumElements;

            int targetFormat;
            if (targetNumElements == 3)
            {
                targetFormat = (int)Devil.ChannelFormat.BGR;
                targetOpenGlFormat = Gl.GL_BGR;
            }
            else
            {
                targetFormat = (int)Devil.ChannelFormat.BGRA;
                targetOpenGlFormat = Gl.GL_BGRA;
            }

            byte[] targetBytes = new byte[targetSize];
            fixed (byte* p = targetBytes)
            {
                IntPtr targetPtr = new IntPtr(p);
                int bytesCopied = Devil.IL.CopyPixels(0, 0, 0, width, height, 1, (Devil.ChannelFormat)targetFormat, Devil.ChannelType.UnsignedByte, targetPtr);
                if (bytesCopied != targetSize)
                    throw new InvalidOperationException("copied bytes didn't match expected #");
            }

            return targetBytes;
        }

        private void CheckError()
        {
            var error = Devil.IL.GetError();
            if (error != ErrorCode.NoError)
            {
                throw new InvalidOperationException(string.Format("Devil error {0}", error));
            }
        }

        private void ClearErrors()
        {
            while (Devil.IL.GetError() != ErrorCode.NoError) ;
        }

        private readonly string m_extension;

        private static object s_devilLock = new object();
        private static bool s_initialized = false;
    }
}
