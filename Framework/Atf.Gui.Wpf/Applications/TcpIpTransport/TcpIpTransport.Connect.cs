//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Net.Sockets;

namespace Sce.Atf.Wpf.Applications
{
    public partial class TcpIpTransport
    {
        /// <summary>
        /// Gets or sets a Connect timeout value.
        /// The default value is TimeSpan.Zero, which indicates that only one attempt will
        /// be made through underlying socket with no subsequent retries.
        /// Specifying TimeSpan.MaxValue indicates an infinite time-out period with infinite number of retries.
        /// Default value is 0.
        /// </summary>
        public TimeSpan ConnectTimeout { set; get; }

        /// <summary>
        /// Gets connect begin time
        /// </summary>
        private DateTime m_connectBeginTime;

        /// <summary>
        /// Initiate connection with the server
        /// </summary>
        /// <remarks>
        /// If this socket has previously been disconnected, then BeginConnect must
        /// be called on a thread that will not exit until the operation is complete.
        /// This is a limitation of the underlying provider.
        /// </remarks>
        public void BeginConnect()
        {
            try
            {
                m_connectBeginTime = DateTime.Now; // Connect started timestamp

                // Begin connect to the remote endpoint
                m_commandSocket.BeginConnect(m_remoteEndPoint, ConnectCallback, "CommandSocket");
            }
            catch (Exception ex)
            {
                Exception = ex;  // Remember in this object state and wrap if needed
                // This code will be called from the main thread, let it know.
                throw Exception;
            }
        }

        /// <summary>
        /// Connect callback. This method called asynchronously
        /// from the Socket as an answer to BeginConnect() call
        /// </summary>
        /// <param name="asyncResult">The status of an asynchronous operation</param>
        private void ConnectCallback(IAsyncResult asyncResult)
        {
            try
            {
                Exception connectException = null;
                try
                {
                    if (m_closed)
                        return;

                    if (asyncResult.AsyncState.Equals("CommandSocket"))
                    {
                        // Finalize socket connect and store possible exception
                        m_commandSocket.EndConnect(asyncResult);

                        if (m_commandSocket.Connected) // CONNECTED!
                        {
                            m_eventDataSocket.BeginConnect(m_remoteEndPoint, ConnectCallback, "EventDataSocket");
                            return;
                        }
                        else
                        {
                            connectException = new TransportException("Unable to connect");
                        }
                    }
                    else if (asyncResult.AsyncState.Equals("EventDataSocket"))
                    {
                        // Finalize socket connect and store possible exception
                        m_eventDataSocket.EndConnect(asyncResult);

                        if (m_eventDataSocket.Connected) // CONNECTED!
                        {
                            // Starts Send and Receive Threads
                            lock (m_stateLock)
                            {
                                SendThreadStart();
                                ReceiveThreadStart();
                            }

                            if (m_target != null)
                            {
                                m_target.IsConnected = true;
                            }

                            m_transportEvent.Set(); // Signal to protocol
                            return;
                        }

                        connectException = new TransportException("Unable to connect");
                    }
                }
                catch (SocketException ex)
                {
                    connectException = ex;
                }

                // We will be here if socket is not connected, Process Timeouts
                bool needReconnect = false;
                // Calculate elapsed time
                TimeSpan timeElapsed = DateTime.Now - m_connectBeginTime;
                if (TimeSpan.Zero == ConnectTimeout)
                {
                    // Single connect attempt with no retries
                }
                else if (ConnectTimeout == TimeSpan.MaxValue)
                {
                    needReconnect = true; // Just try to connect again
                }
                else if (timeElapsed < ConnectTimeout)
                {
                    needReconnect = true;
                }

                if (needReconnect) // Retry connect to the server
                {
                    if (!m_closed)
                        m_commandSocket.BeginConnect(m_remoteEndPoint, ConnectCallback, "CommandSocket");
                    return;
                }
                else // Make time out exception
                {
                    Exception = new TransportException(string.Format("Connection Timeout", connectException.Message));
                }
            }
            catch (SocketException ex)
            {
                Exception = ex;
            }
            catch (ObjectDisposedException ex) // Socket was disposed
            {
                Exception = ex;
            }

            m_transportEvent.Set(); // Signal for completed operation
        }
    }
}
