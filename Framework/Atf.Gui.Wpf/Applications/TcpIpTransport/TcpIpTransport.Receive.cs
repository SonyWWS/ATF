//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Sce.Atf.Wpf.Applications
{
    public partial class TcpIpTransport
    {
        // Packet Header:
        // +-----------+-----------+-----------+-----------+
        // |     0     |     1     |     2     |     3     |
        // +-----------+-----------+-----------+-----------+
        // | Ticket    | MessageId | MessageLength         |
        // +-----------+-----------+-----------+-----------+
        private const ushort KPacketHeaderSize = 4; // 4 bytes total
        private const int KPacketHeaderLengthOffset = 2; // Message Length begins at byte #2

        #region Buffers

        private const int ReceiveBufferSize = 8192; // Receive buffer size

        // Max number of Received Packets waiting in Input Queue
        private const int InPacketMaxEnqueued = 10000;

        class SocketReceiver
        {
            public SocketReceiver(TcpIpTransport transport, Socket socket, string name = "")
            {
                m_name = name;
                m_transport = transport;
                m_socket = socket;
                m_callback = SocketReceiveCallback;
                m_packetComposeHandle = ThreadPool.RegisterWaitForSingleObject(m_packetComposeEvent, PacketComposeTask, null, -1, false);

                BeginReceive();
            }

            /// <summary>
            /// Initiate Async Receive process
            /// </summary>
            /// <remarks>Exceptions must be processed in caller code</remarks>
            public void BeginReceive()
            {
                if (m_callback == null || m_transport.Closed)
                    return; // Protocol not ready or closed

                // Begin receiving data from the remote host
                m_socket.BeginReceive(m_buffer, 0, ReceiveBufferSize, SocketFlags.None, m_callback, null);
            }

            public void Stop()
            {
                if (m_packetComposeHandle != null)
                {
                    m_packetComposeHandle.Unregister(null);
                    m_packetComposeHandle = null;
                }
            }

            /// <summary>
            /// Async Receive process callback for the socket.BeginReceive()
            /// </summary>
            /// <param name="asyncResult">The async operation status</param>
            private void SocketReceiveCallback(IAsyncResult asyncResult)
            {
                try
                {
                    // Read data from the remote host
                    int dataLength = m_socket.Connected ? m_socket.EndReceive(asyncResult) : 0;

                    if (dataLength > 0)
                    {
                        lock (m_streamLock)
                        {
                            long position = m_stream.Position;
                            m_stream.Seek(0, SeekOrigin.End);
                            m_stream.Write(m_buffer, 0, dataLength);
                            m_stream.Position = position;
                        }

                        // Launch task to compose packets
                        m_packetComposeEvent.Set();

                        // Check if we need receive another packet or wait while
                        // already received was processed
                        if (!m_transport.CanAddMorePackets())
                            return;

                        // Prepare to receive next bunch of data
                        BeginReceive();
                    }
                    else // 0 bytes received. Connection was closed by the remote
                    {
                        // Force to compose all received packets
                        PacketComposeTask(null, false);

                        // Check if there is a not completed packet in receiveStream
                        if (m_stream.Length > 0)
                        {
                            m_transport.Exception = new TransportException("Broken Packet");
                        }

                        // Let protocol know that Connection Closed
                        m_transport.DisconnectMessage = "Connection was closed";

                        // The signal is empty packet in queue, to process it in original order
                        m_transport.EnqueuePacket(new byte[0]);

                        m_transport.Exception = new Exception("Socket disconnected");
                    }
                }
                catch (Exception ex) // Remember exception
                {
                    m_transport.Exception = ex;
                }
            }

            /// <summary>
            /// Completing Received Transport Packets and place its in Incoming Queue
            /// </summary>
            /// <remarks>This method executed as a separate ThreadPool task</remarks>
            /// <param name="state">Execution context. Not used.</param>
            /// <param name="timedOut">true if WaitHandle timed out, false if it was signaled. Not used.</param>
            private void PacketComposeTask(object state, bool timedOut)
            {
                try
                {
                    bool packetsAppended = false;
                    lock (m_streamLock)
                    {
                        m_stream.Position = 0;
                        byte[] rawPacketHeader = new byte[KPacketHeaderSize];

                        while (m_stream.Length - m_stream.Position >= KPacketHeaderSize)
                        {
                            if (m_transport.Closed) // Do not do any work if transport already closed
                                return;

                            // Remember start position - used to calculate remaining byte to receive
                            long startPosition = m_stream.Position;

                            // Read packet header as raw bytes
                            m_stream.Read(rawPacketHeader, 0, KPacketHeaderSize);
                            m_stream.Position = startPosition;

                            // Read packet length from the packet Header
                            ushort packetLength = (ushort)IPAddress.NetworkToHostOrder(
                                BitConverter.ToInt16(rawPacketHeader, KPacketHeaderLengthOffset));
                            packetLength += KPacketHeaderSize;

                            // Are there enough bytes to read whole packet?
                            if (m_stream.Length - startPosition < packetLength)
                                break; // Not enough, end processing

                            // Got a whole packet!
                            // Read packet as raw bytes
                            byte[] packet = new byte[packetLength];
                            if (m_stream.Read(packet, 0, packetLength) != packetLength)
                                throw new TransportException("Unable to read data");

                            // Append to Incoming Packets buffer
                            m_transport.EnqueuePacket(packet);
                            packetsAppended = true;
                        }

                        // Cut processed packets out of the received stream
                        if (packetsAppended)
                        {
                            var trail = new MemoryStream();

                            // Check is there any remaining data in the stream?
                            int remainingBytes = (int)(m_stream.Length - m_stream.Position);
                            if (remainingBytes > 0)
                            {
                                // Note: Never use: return new MemoryStream(buffer);
                                byte[] buffer = new byte[remainingBytes];
                                m_stream.Read(buffer, 0, remainingBytes);
                                trail.Write(buffer, 0, remainingBytes);
                            }

                            m_stream = trail;
                        }
                    }

                    // Signal to protocol that new incoming packets was received
                    if (packetsAppended)
                    {
                        m_transport.SignalTransportEvent();
                    }
                }
                catch (Exception ex)
                {
                    m_transport.Exception = ex;
                }
            }

            /// <summary>
            /// The socket owner. </summary>
            private readonly TcpIpTransport m_transport;

            /// <summary>
            /// The debug name. </summary>
            private readonly string m_name;

            /// <summary>
            /// The socket owner. </summary>
            private readonly Socket m_socket;

            /// <summary>
            /// Receive Buffer for Async operations </summary>
            private readonly byte[] m_buffer = new byte[ReceiveBufferSize];

            /// <summary>
            /// Stream is reassigned so we need a different object for synchronization. </summary>
            private readonly object m_streamLock = new object();

            /// <summary>
            /// Stream filled with received but not processed yet data.
            /// All calls should be protected with lock, because this stream filled
            /// assynchronously in SocketReceiveCallback() callback. </summary>
            private MemoryStream m_stream = new MemoryStream();
            
            /// <summary>
            /// Precalculated socket.BeginReceive() callback value.
            /// null if socket receive sequence was not initiated yet. </summary>
            private readonly AsyncCallback m_callback = null;

            /// <summary>
            /// Packet Compose Event used to activate Packet Compose Task
            /// when transport packet have been received </summary>
            private readonly AutoResetEvent m_packetComposeEvent = new AutoResetEvent(false);

            /// <summary>
            /// Thread pool handle.</summary>
            private RegisteredWaitHandle m_packetComposeHandle;
        }

        private SocketReceiver m_comandReceiver;
        private SocketReceiver m_eventDataReceiver;

        /// <summary>
        /// Incoming Packets buffer
        /// </summary>
        /// <remarks>All inPackets calls must be synchornized with stateLock</remarks>
        private Queue<byte[]> m_inPackets = new Queue<byte[]>();

        // Packets object is reassigned so we need a different synchronization object.
        private object m_inPacketLock = new object();

        /// <summary>
        /// true if Receiving sequence stopped
        /// </summary>
        private bool m_inPacketStopped;

        internal void SignalTransportEvent()
        {
            m_transportEvent.Set();
        }

        internal bool CanAddMorePackets()
        {
            lock (m_inPacketLock)
            {
                if (m_inPackets.Count >= InPacketMaxEnqueued)
                {
                    m_inPacketStopped = true;
                    System.Diagnostics.Debug.WriteLine("Warning: Queued packet limit reached. Is there a lot of TTY?");
                }
            }

            return !m_inPacketStopped;
        }

        internal void EnqueuePacket(byte[] packet)
        {
            lock (m_inPacketLock)
            {
                m_inPackets.Enqueue(packet);
            }
        }

        /// <summary>
        /// Gets Received completed Transport Packets
        /// </summary>
        /// <remarks>If Connection was closed then empty packet placed in IncomingPackets
        /// and DisconnectMessage set to appropriate message</remarks>
        /// <returns>Incoming Packets</returns>
        public Queue<byte[]> GetIncomingPackets()
        {
            Queue<byte[]> rawPackets;
            bool needStartReceive = false;
            lock (m_inPacketLock)
            {
                rawPackets = m_inPackets;
                m_inPackets = new Queue<byte[]>();

                if (m_inPacketStopped)
                {
                    needStartReceive = true;
                    m_inPacketStopped = false;
                }
            }

            // Do not call SocketBeginReceive() in inPacketLock!
            // Sometime SocketReceiveCallback will be called immediately and inherit this lock.
            if (needStartReceive)
            {
                try
                {
                    if (m_comandReceiver != null)
                        m_comandReceiver.BeginReceive();
                    
                    if (m_eventDataReceiver != null)
                        m_eventDataReceiver.BeginReceive();
                }
                catch (Exception ex) // Remember exception
                {
                    Exception = ex;
                }
            }

            return rawPackets;
        }

        #endregion Buffers

        #region Async Receive Raw data into the stream

        /// <summary>
        /// Starts Receive Thread
        /// </summary>
        private void ReceiveThreadStart()
        {
            lock (m_stateLock)
            {
                // Initiate Socket Receive Operations
                if (m_comandReceiver == null)
                {
                    m_comandReceiver = new SocketReceiver(this, m_commandSocket, "Command");
                }
                
                if (m_eventDataReceiver == null)
                {
                    m_eventDataReceiver = new SocketReceiver(this, m_eventDataSocket, "Event");
                }
            }
        }

        /// <summary>
        /// Stops Receive Thread
        /// </summary>
        private void ReceiveThreadStop()
        {
            lock (m_stateLock)
            {
                if (m_comandReceiver != null)
                    m_comandReceiver.Stop();

                if (m_eventDataReceiver != null)
                    m_eventDataReceiver.Stop();
            }
        }

        //private void PacketComposeTask(object state, bool timedOut)
        //{
        //    try
        //    {
        //        var stream = state as Stream;

        //        bool packetsAppended = false;
        //        lock (stream)
        //        {
        //            stream.Position = 0;
        //            byte[] rawPacketHeader = new byte[KPacketHeaderSize];

        //            while (stream.Length - stream.Position >= KPacketHeaderSize)
        //            {
        //                if (m_closed) // Do not do any work if transport already closed
        //                    return;

        //                // Remember start position - used to calculate remaining byte to receive
        //                long startPosition = stream.Position;

        //                // Read packet header as raw bytes
        //                stream.Read(rawPacketHeader, 0, KPacketHeaderSize);
        //                stream.Position = startPosition;

        //                // Read packet length from the packet Header
        //                ushort packetLength = (ushort)IPAddress.NetworkToHostOrder(
        //                    BitConverter.ToInt16(rawPacketHeader, KPacketHeaderLengthOffset));
        //                packetLength += KPacketHeaderSize;

        //                // Are there enough bytes to read whole packet?
        //                if (stream.Length - startPosition < packetLength)
        //                    break; // Not enough, end processing

        //                // Got a whole packet!
        //                // Read packet as raw bytes
        //                byte[] packet = new byte[packetLength];
        //                if (stream.Read(packet, 0, packetLength) != packetLength)
        //                    throw new TransportException("Unable to read data");

        //                // Append to Incoming Packets buffer
        //                EnqueuePacket(packet);
        //                packetsAppended = true;
        //            }

        //            // Cut processed packets out of the received stream
        //            if (packetsAppended)
        //            {
        //                // Check is there any remaining data in the stream?
        //                int remainingBytes = (int)(stream.Length - stream.Position);
        //                if (remainingBytes > 0)
        //                {
        //                    byte[] buffer = new byte[remainingBytes];
        //                    stream.Read(buffer, 0, remainingBytes);
        //                    stream.Position = 0;
        //                    stream.Write(buffer, 0, remainingBytes);
        //                    stream.SetLength(remainingBytes);
        //                }
        //                else
        //                {
        //                    stream.Position = 0;
        //                    stream.SetLength(0);
        //                }
        //            }
        //        }

        //        // Signal to protocol that new incoming packets was received
        //        if (packetsAppended)
        //        {
        //            m_transportEvent.Set();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Exception = ex;
        //    }
        //}

        #endregion Async Receive Raw data into the stream
    }
}
