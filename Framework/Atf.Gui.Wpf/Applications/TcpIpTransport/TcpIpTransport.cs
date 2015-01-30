//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Temporary TCP/IP Transport layer implementation. This should be more flexible to allow
    /// users to override this default implementation.
    /// </summary>
    public partial class TcpIpTransport : ITransportLayer
    {
        #region Constructors

        /// <summary>
        /// Constructor with TcpIpTarget</summary>
        /// <param name="target">TcpIpTarget</param>
        public TcpIpTransport(TcpIpTarget target)
        {
            DisconnectMessage = null;
            if (target == null)
                throw new ArgumentException("target is null");

            IPAddress addr = null;
            if (!IPAddress.TryParse(target.IpAddress, out addr))
            {
                try
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(target.IpAddress);
                    if (hostEntry.AddressList.Length > 0)
                        addr = hostEntry.AddressList[0];
                }
                catch
                {
                    throw new ArgumentException("Invalid Host");
                }
            }

            m_target = target;
            m_remoteEndPoint = new IPEndPoint(addr, (int)target.Port);
            ConnectTimeout = TimeSpan.MaxValue; // connectTimeout;
            m_transportEvent = new AutoResetEvent(false);

            // Create a TCP/IP socket for request/response
            m_commandSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_eventDataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        /// Default destructor</summary>
        ~TcpIpTransport()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets Connection status
        /// </summary>
        public bool Connected
        {
            get { return m_commandSocket.Connected && m_eventDataSocket.Connected; }
        }

        /// <summary>
        /// Current target.
        /// </summary>
        private TcpIpTarget m_target;

        /// <summary>
        /// Remote end point to connect to
        /// </summary>
        private readonly IPEndPoint m_remoteEndPoint;

        /// <summary>
        /// TCP socket
        /// </summary>
        private readonly Socket m_commandSocket;

        /// <summary>
        /// TCP socket
        /// </summary>
        private readonly Socket m_eventDataSocket;

        /// <summary>
        /// Start time
        /// </summary>
        private DateTime m_onlineStartTime = DateTime.Now;

        /// <summary>
        /// Event object that must be set on any Transport Event
        /// </summary>
        public AutoResetEvent TransportEvent { get { return m_transportEvent; } }
        private readonly AutoResetEvent m_transportEvent = null;

        /// <summary>
        /// Protocol state
        /// </summary>
        private readonly object m_stateLock = new object();
        
        internal bool Closed
        {
            get { return m_closed; }
        }
        
        private volatile bool m_closed = false;

        /// <summary>
        /// Gets Disconnect Message
        /// </summary>
        public string DisconnectMessage { get; internal set; }

        #endregion Properties

        #region IDisposable Members

        /// <summary>
        /// Dispose of resources</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed) // Dispose has already been called
                return;

            m_disposed = true;

            // If disposing equals true, dispose managed resources
            if (disposing)
            {
                // Dispose managed resources
                if (m_commandSocket != null)
                    m_commandSocket.Dispose();

                if (m_eventDataSocket != null)
                    m_eventDataSocket.Dispose();
            }

            // Release unmanaged resources if disposing is false
        }

        bool m_disposed = false;

        #endregion

        #region Public Methods

        /// <summary>
        /// Close connection, end operations
        /// </summary>
        public void Close()
        {
            lock (m_stateLock)
            {
                m_closed = true; // Force to close all activity

                ConnectTimeout = TimeSpan.Zero;

                if (m_target != null)
                {
                    m_target.IsConnected = false;
                }
                
                // Release the socket
                if (m_commandSocket != null)
                {
                    if (m_commandSocket.Connected)
                    {
                        m_commandSocket.Shutdown(SocketShutdown.Both);
                        m_commandSocket.Disconnect(false);
                    }
                }

                if (m_eventDataSocket != null)
                {
                    if (m_eventDataSocket.Connected)
                    {
                        m_eventDataSocket.Shutdown(SocketShutdown.Both);
                        m_eventDataSocket.Disconnect(false);
                    }
                }

                // Stop Send and Receive Threads
                SendThreadStop();
                ReceiveThreadStop();
            }
        }

        /// <summary>
        /// Gets First Exception information, null if ther is no Exception
        /// </summary>
        public Exception Exception
        {
            get { return m_exception; }
            set
            {
                if (m_closed) // Ignore Exception if connection was closed manually
                    return;

                lock (m_stateLock)
                {
                    // Remember only the first Exception (source of problem)
                    if (m_exception != null)
                        return;

                    // All internal transport specific Exceptions must be represented outside as TransportException.
                    // Other must be represented as is.
                    if (value is SocketException)
                        value = new TransportException(value);

                    m_exception = value;
                    m_transportEvent.Set(); // Signal to protocol
                }
            }
        }
        private Exception m_exception = null;

        #endregion
    }

    /// <summary>
    /// The exception that is thrown when need to inform about an error
    /// with transport layer or transport packets
    /// </summary>
    public class TransportException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the exception class
        /// </summary>
        public TransportException() : base() { }

        /// <summary>
        /// Initializes a new instance of the exception class
        /// with a specified error message
        /// </summary>
        /// <param name="message">The error message</param>
        public TransportException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the exception class based on "SocketException".
        /// </summary>
        /// <param name="ex">Exception instance</param>
        public TransportException(Exception ex)
            : base(ex.Message, ex)
        {
        }
    }
}
