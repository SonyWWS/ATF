//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Text;

namespace Sce.Atf.Applications.NetworkTargetServices
{

    /// <summary>
    /// Generic command packet structure</summary>
    public class TCPCommand
    {
        public int m_opcode;
        public int m_payloadSize;
        public byte[] m_payload;
    };

    /// <summary>
    /// High level command/packet based protocol for TCP</summary>
    public class TcpCommandClient
    {
        #region Events
        /// <summary>
        /// Event that is raised when data is ready to be read</summary>
        public event CommandReadyHandler CommandReady;

        /// <summary>
        /// Event that is raised when there is an unhandled exception</summary>
        public event ExceptonHandler UnHandledException;

        /// <summary>
        /// Callback when data is ready</summary>
        /// <param name="sender">Sender</param>
        /// <param name="command">Command packet</param>
        public delegate void CommandReadyHandler(object sender, TCPCommand command);

        /// <summary>
        /// Callback when there is an unhandled exception</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Exception</param>
        public delegate void ExceptonHandler(object sender, Exception e);

        #endregion

        /// <summary>
        /// Creates command client that parses traffic on the given socket</summary>
        /// <param name="socket">TCP socket to listen to</param>
        /// <param name="maxPayloadSize">Max payload size</param>
        public TcpCommandClient(TargetTcpSocket socket, int maxPayloadSize)
        {
            m_socket = socket;
            m_socket.DataReady += DataReadyHandler;
            m_socket.UnHandledException += ExceptionHandler;
            m_maxPayloadSize = maxPayloadSize;
            m_payloadBytes = new byte[m_maxPayloadSize];
            m_numBytesReceivedThisCommand = 0;
            m_payloadSizeThisCommand = 0;               
        }

        /// <summary>
        /// Reset the socket. If we get disconnected, we must reset the payload bytes, etc.</summary>
        public void Reset()
        {
            m_numBytesReceivedThisCommand = 0;
            m_payloadSizeThisCommand = 0;
        }

        /// <summary>
        /// Encodes and sends the command and its payload</summary>
        /// <param name="command">Command packet</param>
        public void Send(TCPCommand command)
        {
            byte[] opcodeBytes = EncodeInt(command.m_opcode);
            byte[] payloadSizeBytes = EncodeInt(command.m_payloadSize);
            m_socket.Send(opcodeBytes);
            m_socket.Send(payloadSizeBytes);
            m_socket.Send(command.m_payload);
        }

        private void DataReadyHandler(object sender, byte[] buf)
        {
            // parse out the bytes and when we have enough for a complete command
            // dispatch the command and reset the buf
            for (int i=0; i<buf.Length; i++)
            {
                // Separate opcode, payload size, and payload
                if (m_numBytesReceivedThisCommand < 4)
                {
                    m_opcodeBytes[m_numBytesReceivedThisCommand] = buf[i];
                }
                else if (m_numBytesReceivedThisCommand < 8)
                {
                    m_payloadSizeBytes[m_numBytesReceivedThisCommand - 4] = buf[i];
                }
                else
                {
                    m_payloadBytes[m_numBytesReceivedThisCommand - 8] = buf[i];
                    m_payloadSizeThisCommand++;
                }

                m_numBytesReceivedThisCommand++;

                // Create the tcp command header if we have enough data
                if (m_numBytesReceivedThisCommand == 8)
                {
                    DecodeInt(m_opcodeBytes,0,out m_tcpCommand.m_opcode);
                    DecodeInt(m_payloadSizeBytes,0,out m_tcpCommand.m_payloadSize);
                }

                // If we have a fully formed command, dispatch it
                if (m_numBytesReceivedThisCommand == m_tcpCommand.m_payloadSize + 8)
                {
                    CommandReadyHandler chr = CommandReady;
                    if (chr != null)
                    {
                        m_tcpCommand.m_payload = m_payloadBytes;
                        chr(this,m_tcpCommand);
                    }
                    m_numBytesReceivedThisCommand = 0;
                    m_payloadSizeThisCommand = 0;
                }
            }
        }

        // Very crude way to wrap the unhandled exception so it LOOKS like .NET event args
        private void ExceptionHandler(object sender, Exception e)
        {
            ExceptonHandler eh = UnHandledException;
            if (eh != null)
                eh(this,e);
        }

        #region Data Encode and Decode Utilities
        //-----------------------------------------------------------------------------
        // All of the Decode functions return the number of bytes that were consumed in
        // the decoding of the element
        //-----------------------------------------------------------------------------

        /// <summary>
        /// Decode byte data to int data</summary>
        /// <param name="data">Array of byte data to be decoded</param>
        /// <param name="startIndex">Index at which to start decoding data</param>
        /// <param name="value">Decoded data</param>
        /// <returns>Number of bytes consumed in decoding the element</returns>
        /// <remarks>Incoming data on the wire is always big-endian.</remarks>
        public static int DecodeInt(byte[] data, int startIndex, out int value)
        {
            byte[] temp = new byte[4];
            // swizzle
            temp[0] = data[startIndex + 3];
            temp[1] = data[startIndex + 2];
            temp[2] = data[startIndex + 1];
            temp[3] = data[startIndex + 0];
            value = System.BitConverter.ToInt32(temp, 0);
            return 4; // The number of bytes consumed
        }

        /// <summary>
        /// Decode byte data to UInt64 data</summary>
        /// <param name="data">Array of byte data to be decoded</param>
        /// <param name="startIndex">Index at which to start decoding data</param>
        /// <param name="value">Decoded data</param>
        /// <returns>Number of bytes consumed in decoding the element</returns>
        /// <remarks>Incoming data on the wire is always big-endian.</remarks>
        public static int DecodeUInt64(byte[] data, int startIndex, out UInt64 value)
        {
            byte[] temp = new byte[8];
            // swizzle
            temp[0] = data[startIndex + 7];
            temp[1] = data[startIndex + 6];
            temp[2] = data[startIndex + 5];
            temp[3] = data[startIndex + 4];
            temp[4] = data[startIndex + 3];
            temp[5] = data[startIndex + 2];
            temp[6] = data[startIndex + 1];
            temp[7] = data[startIndex + 0];
            value = System.BitConverter.ToUInt64(temp, 0);
            return 8; // The number of bytes consumed
        }

        /// <summary>
        /// Decode byte data to single-precision floating point data</summary>
        /// <param name="data">Array of byte data to be decoded</param>
        /// <param name="startIndex">Index at which to start decoding data</param>
        /// <param name="value">Decoded data</param>
        /// <returns>Number of bytes consumed in decoding the element</returns>
        /// <remarks>Incoming data on the wire is always big-endian.</remarks>
        public static int DecodeFloat(byte[] data, int startIndex, out float value)
        {
            byte[] temp = new byte[4];
            // swizzle
            temp[0] = data[startIndex + 3];
            temp[1] = data[startIndex + 2];
            temp[2] = data[startIndex + 1];
            temp[3] = data[startIndex + 0];
            value = System.BitConverter.ToSingle(temp, 0);
            return 4;
        }

        /// <summary>
        /// Decode byte data to string data</summary>
        /// <param name="data">Array of byte data to be decoded</param>
        /// <param name="startIndex">Index at which to start decoding data</param>
        /// <param name="value">Decoded data</param>
        /// <returns>Number of bytes consumed in decoding the element</returns>
        /// <remarks>Strings are encoded Pascal style.</remarks>
        public static int DecodeString(byte[] data, int startIndex, out string value)
        {
            int strLen;
            startIndex += DecodeInt(data,startIndex,out strLen);
            value = Encoding.ASCII.GetString(data, startIndex, strLen);
            return (strLen + 4);
        }

        /// <summary>
        /// Encode int to byte data</summary>
        /// <param name="val">Int to be encoded</param>
        /// <returns>Array of byte converted data, which is 4 bytes long</returns>
        /// <remarks>We always encode into big-endian for transmission over wire.</remarks>
        public static byte[] EncodeInt(int val)
        {
            byte[] temp = System.BitConverter.GetBytes(val);
            byte[] retval = new byte[4];
            // swizzle
            retval[0] = temp[3];
            retval[1] = temp[2];
            retval[2] = temp[1];
            retval[3] = temp[0];
            return retval;
        }
        #endregion

        private readonly TargetTcpSocket m_socket;

        // We use these members to accumulate a command for dispatch
        private int m_numBytesReceivedThisCommand;
        private int m_payloadSizeThisCommand;
        private readonly byte[] m_payloadBytes;
        private readonly byte[] m_opcodeBytes = new byte[4];
        private readonly byte[] m_payloadSizeBytes = new byte[4];
        private readonly TCPCommand m_tcpCommand = new TCPCommand();
        private readonly int m_maxPayloadSize;
    }
}
