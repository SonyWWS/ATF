//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;
using System.Text;

using Tao.OpenGl;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Loads the 1.x header info and payload from any Targa file and creates an Sce.Atf.Rendering.Image
    /// from it. See format at http://www.fileformat.info/format/tga/egff.htm and more details at
    /// http://www.scs.fsu.edu/~burkardt/pdf/targa.pdf </summary>
    public class TargaImageLoader : IImageLoader
    {
        /// <summary>
        /// Loads texture image from stream</summary>
        /// <param name="imageStream">Stream holding texture image</param>
        /// <returns>Texture image</returns>
        public Image LoadImage(Stream imageStream)
        {
            TgaHeader header;
            byte[] pixels;
            
            using (BinaryReader reader = new BinaryReader(imageStream))
            {
                header = ReadHeader(reader);
                ImageDataType dataType = (ImageDataType) header.dataType;
                
                switch ((ImageDataType)header.dataType)
                {
                    case ImageDataType.UncompressedUnmappedColor:
                    case ImageDataType.RleCompressedUnmappedColor:
                    case ImageDataType.UncompressedMappedColor:
                        break;

                    default:
                        throw new NotImplementedException("Unsupported Targa image type '" + dataType.ToString() + "'");
                }

                ReadIdString(reader, header);

                byte[] colorMap = ReadColorMap(reader, header);               
                pixels = ReadPixels(reader, header, colorMap);
            }

            int pixelFormat = CalculateOpenGlPixelFormat(header);
            int elementsPerPixel = GetPixelDepth(header) / 8;
            
            return new Image(header.imageWidth, header.imageHeight, pixels, 1, pixelFormat, elementsPerPixel);
        }

        private int GetPixelDepth(TgaHeader header)
        {
            ColorMapType colorMapType = (ColorMapType)header.colorMapType;
            return (colorMapType == ColorMapType.ColorMapPresent) ? header.colorMapDepth : header.bitsPerPixel;
        }

        private enum ImageDataType
        {
            NoImagePresent = 0,
            UncompressedMappedColor = 1,
            UncompressedUnmappedColor = 2,
            UncompressedBlackAndWhite = 3,
            RleCompressedMappedColor = 9,
            RleCompressedUnmappedColor = 10,
            RleCompressedBlackAndWhite = 11,
            HuffmanDeltaRleCompressedMappedColor = 32,
            HuffmanDeltaRleCompressed4PassMappedColor = 33
        }

        private enum ColorMapType
        {
            NoColorMapPresent = 0,
            ColorMapPresent = 1,
        }
        
        private class TgaHeader
        {
            public byte idLength;
            public byte colorMapType;
            public byte dataType;
            public Int16 colorMapStart;
            public Int16 colorMapLength;
            public byte colorMapDepth;
            public Int16 xOrigin;
            public Int16 yOrigin;
            public Int16 imageWidth;
            public Int16 imageHeight;
            public byte bitsPerPixel;
            public byte imageDescriptor;
        }

        private TgaHeader ReadHeader(BinaryReader reader)
        {
            TgaHeader header = new TgaHeader();
            header.idLength = reader.ReadByte();
            header.colorMapType = reader.ReadByte();
            header.dataType = reader.ReadByte();
            header.colorMapStart = reader.ReadInt16();
            header.colorMapLength = reader.ReadInt16();
            header.colorMapDepth = reader.ReadByte();
            header.xOrigin = reader.ReadInt16();
            header.yOrigin = reader.ReadInt16();
            header.imageWidth = reader.ReadInt16();
            header.imageHeight = reader.ReadInt16();
            header.bitsPerPixel = reader.ReadByte();
            header.imageDescriptor = reader.ReadByte();

            return header;
        }

        private string ReadIdString(BinaryReader reader, TgaHeader header)
        {
            byte[] idString = reader.ReadBytes(header.idLength);

            ASCIIEncoding encoding = new ASCIIEncoding();
            return encoding.GetString(idString, 0, idString.Length);
        }

        private byte[] ReadColorMap(BinaryReader reader, TgaHeader header)
        {
            if ((ImageDataType)header.dataType != ImageDataType.UncompressedMappedColor)
                return null;

            int bytesPerEntry = header.colorMapDepth/8;
            byte[] colorMap = reader.ReadBytes(bytesPerEntry*header.colorMapLength);

            return colorMap;
        }

        private byte[] ReadPixels(BinaryReader reader, TgaHeader header, byte[] colorMap)
        {
            if (header.bitsPerPixel == 16)
                throw new NotImplementedException("16bpp RGB unpacking is not supported yet.");

            int bytesPerPixel = header.bitsPerPixel / 8;
            int pixelCount = header.imageHeight * header.imageWidth;
            int expectedPayloadSize = pixelCount * bytesPerPixel;

            byte[] pixelPayload;

            if ((ImageDataType)header.dataType == ImageDataType.RleCompressedUnmappedColor)
            {
                int payloadSize = (int)(reader.BaseStream.Length - reader.BaseStream.Position);
                pixelPayload = reader.ReadBytes(payloadSize);
                pixelPayload = DecompressRle(pixelPayload, header);
            }
            else
            {
                // ignore footer
                pixelPayload = reader.ReadBytes(expectedPayloadSize);
            }

            if (pixelPayload.Length != expectedPayloadSize)
            {
                throw new InvalidDataException("Targa payload size (" +
                                               pixelPayload.Length +
                                               ") does not match expected size (" +
                                               expectedPayloadSize + ")");
            }

            if ((ColorMapType)header.colorMapType == ColorMapType.ColorMapPresent)
                pixelPayload = ExpandPayloadToUnmappedPixels(header, pixelPayload, colorMap);

            if (YOriginIsAtBottomOfImage(header.imageDescriptor))
                InvertImageAroundY(pixelPayload, header);

            return pixelPayload;
        }

        private bool YOriginIsAtBottomOfImage(byte imageDescriptor)
        {
            return ((imageDescriptor & 0x20) == 0);
        }

        private byte[] DecompressRle(byte[] rleCompressedPixels, TgaHeader header)
        {
            int bytesPerPixel = GetPixelDepth(header) / 8;
            int decompressedByteCount = header.imageWidth * header.imageHeight * bytesPerPixel;
            byte[] decompressedPixels = new byte[decompressedByteCount];

            using (BinaryReader reader = new BinaryReader(new MemoryStream(rleCompressedPixels)))
            using (BinaryWriter writer = new BinaryWriter(new MemoryStream(decompressedPixels)))
            {
                while (writer.BaseStream.Position != writer.BaseStream.Length)
                {
                    byte packetHeader = reader.ReadByte();

                    int pixelCountThisPayload = (packetHeader & 0x7F) + 1;
                    if (PacketPreceedsRlePayload(packetHeader))
                    {
                        byte[] runColor = reader.ReadBytes(bytesPerPixel);
                        while (pixelCountThisPayload > 0)
                        {
                            writer.Write(runColor);
                            --pixelCountThisPayload;
                        }
                    }
                    else
                    {
                        // copy straight through
                        int payloadSizeInBytes = pixelCountThisPayload * bytesPerPixel;
                        byte[] payload = reader.ReadBytes(payloadSizeInBytes);
                        writer.Write(payload);
                    }
                }
            }

            return decompressedPixels;
        }

        private bool PacketPreceedsRlePayload(byte packetHeader)
        {
            return ((packetHeader & 0x80) != 0);
        }

        private byte[] ExpandPayloadToUnmappedPixels(TgaHeader header, byte[] pixelPayload, byte[] colorMap)
        {
            int mappedPixelSize = header.bitsPerPixel/8;
            if (mappedPixelSize != 1)
                throw new NotSupportedException("Mapped pixel elements greater than 8bpp are not supported yet.");

            int unmappedPixelSize = header.colorMapDepth/8;
            if (unmappedPixelSize != 3 && unmappedPixelSize != 4)
                throw new NotSupportedException("Unmapped pixel elements other than 24bpp and 32bpp are not supported yet.");
            
            int pixelCount = header.imageHeight*header.imageWidth;
            
            byte[] unmappedPixels = new byte[unmappedPixelSize*pixelCount];
            
            MemoryStream unmappedPixelStream = new MemoryStream(unmappedPixels);

            for (int i = 0; i < pixelCount; ++i)
            {
                int index = pixelPayload[i];
                int mapEntry = index*unmappedPixelSize;
                
                if (unmappedPixelSize == 3)
                {
                    unmappedPixelStream.WriteByte(colorMap[mapEntry]);
                    unmappedPixelStream.WriteByte(colorMap[mapEntry + 1]);
                    unmappedPixelStream.WriteByte(colorMap[mapEntry + 2]);
                }
                else
                {
                    unmappedPixelStream.WriteByte(colorMap[mapEntry]);
                    unmappedPixelStream.WriteByte(colorMap[mapEntry + 1]);
                    unmappedPixelStream.WriteByte(colorMap[mapEntry + 2]);
                    unmappedPixelStream.WriteByte(colorMap[mapEntry + 3]);
                }
            }

            return unmappedPixels;
        }
        
        private void InvertImageAroundY(byte[] pixels, TgaHeader header)
        {
            int bytesPerPixel = GetPixelDepth(header) / 8;
            int bytesPerRow = header.imageWidth * bytesPerPixel;

            byte[] row = new byte[bytesPerRow];

            int topOffset = 0;
            int bottomOffset = bytesPerRow * (header.imageHeight - 1);
            int totalRowsToReflect = header.imageHeight / 2;
            for (int i = 0; i < totalRowsToReflect; ++i)
            {
                Buffer.BlockCopy(pixels, topOffset, row, 0, bytesPerRow);
                Buffer.BlockCopy(pixels, bottomOffset, pixels, topOffset, bytesPerRow);
                Buffer.BlockCopy(row, 0, pixels, bottomOffset, bytesPerRow);

                topOffset += bytesPerRow;
                bottomOffset -= bytesPerRow;
            }
        }
        
        private int CalculateOpenGlPixelFormat(TgaHeader header)
        {
            int bpp = GetPixelDepth(header);
            switch (bpp)
            {
                case 24:
                    return Gl.GL_BGR;
                    
                case 32:
                    return Gl.GL_BGRA;
                    
                default:
                    throw new InvalidOperationException("Unknown pixel depth.");
            }
        }
    }
}
