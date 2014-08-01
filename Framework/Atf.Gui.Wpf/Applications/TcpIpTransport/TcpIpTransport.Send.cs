//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace Sce.Atf.Wpf.Applications
{
    public partial class TcpIpTransport
    {
        private Thread m_sendThread;

        private AutoResetEvent m_outPacketNewEvent = new AutoResetEvent(false);
        private AutoResetEvent m_outPacketSentEvent = new AutoResetEvent(false);
        private AutoResetEvent m_sendThreadStopEvent = new AutoResetEvent(false);

        private Queue<OutPacket> m_outPackets = new Queue<OutPacket>();
        private object m_outPacketsLock = new object();

        /// <summary>
        /// Begin Async Send operation
        /// </summary>
        /// <param name="data">Transport Packet to Send</param>
        /// <remarks>Exceptions must be processed in the caller code</remarks>
        public void BeginSend(byte[] data)
        {
            // Place output packet in the queue
            lock (m_outPacketsLock)
            {
                m_outPackets.Enqueue(new OutPacket(data));
            }

            // And signal to the SendThread
            m_outPacketNewEvent.Set();
        }

        /// <summary>
        /// Starts Send Thread
        /// </summary>
        private void SendThreadStart()
        {
            lock (m_stateLock)
            {
                if (m_sendThread == null)
                {
                    m_sendThread = new Thread(SendThread);
                    // Make send thread background - this thread will not prevent application to close.
                    m_sendThread.IsBackground = true;
                    m_sendThread.Start();
                }
            }
        }

        /// <summary>
        /// Stops Send Thread
        /// </summary>
        private void SendThreadStop()
        {
            lock (m_stateLock)
            {
                if (m_sendThread != null)
                {
                    m_sendThreadStopEvent.Set();

                    // Wait for threads to finish their activity
                    if (m_sendThread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
                        m_sendThread.Join();
                    m_sendThread = null;
                }
            }
        }

        /// <summary>
        /// Thread used to Send output transport packets
        /// </summary>
        private void SendThread()
        {
            try
            {
                WaitHandle[] waitNew = new WaitHandle[] { m_sendThreadStopEvent, m_outPacketNewEvent };
                WaitHandle[] waitSent = new WaitHandle[] { m_sendThreadStopEvent, m_outPacketSentEvent };
                while (WaitHandle.WaitAny(waitNew) != 0) // While not Stop
                {
                    // Move queued packets to local queue
                    Queue<OutPacket> packetsToSend;
                    lock (m_outPacketsLock)
                    {
                        packetsToSend = m_outPackets;
                        m_outPackets = new Queue<OutPacket>();
                    }

                    // Send packets
                    while (packetsToSend.Count > 0)
                    {
                        OutPacket packet = packetsToSend.Dequeue();
                        do
                        {
                            m_commandSocket.BeginSend( // Start sending packet
                                packet.Data,                        // buffer
                                packet.BytesSent,                   // offset
                                packet.Length - packet.BytesSent,   // size
                                SocketFlags.None,                   // socketFlags
                                SendCallback,                       // callback
                                packet);                            // state

                            // Suspend thread until packet will be sent or connection will be stopped
                            if (WaitHandle.WaitAny(waitSent) == 0)
                                throw new TransportException("ErrProtocolConnectionClosed");
                        } while (packet.BytesSent < packet.Length);
                    }
                }
            }
            catch (Exception ex) // Remember Exception
            {
                Exception = ex;
            }
        }

        /// <summary>
        /// Send callback for the Socket.BeginSend()
        /// </summary>
        /// <param name="ar">The status of an asynchronous operation</param>
        private void SendCallback(IAsyncResult ar)
        {
            // Retrieve the the packet being send
            OutPacket outPacket = (OutPacket)ar.AsyncState;

            try
            {
                // Complete sending the data to the remote host
                int bytesSent = m_commandSocket.EndSend(ar);

                // Adjust total bytes sent count for packet
                outPacket.BytesSent += bytesSent;
            }
            catch (Exception ex) // Remember exception
            {
                Exception = ex;
                // Do not try to send this packet again
                outPacket.BytesSent = outPacket.Length;
            }
            finally
            {
                m_outPacketSentEvent.Set(); // Resume send thread
            }
        }

        #region OutPacket class

        private class OutPacket
        {
            public OutPacket(byte[] packet)
            {
                Data = packet;
                Length = packet.Length;
            }

            public readonly byte[] Data;
            public readonly int Length;

            public int BytesSent = 0;
        }

        #endregion OutPacket class
    }
}
