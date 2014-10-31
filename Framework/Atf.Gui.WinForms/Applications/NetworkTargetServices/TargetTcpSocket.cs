//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Sce.Atf.Applications.NetworkTargetServices
{

    /// <summary>
    /// High level client TCP socket</summary>
    public class TargetTcpSocket
    {
        #region Events
        /// <summary>
        /// Event that is raised when the requested connection is established</summary>
        public event ConnectHandler   Connected;

        /// <summary>
        /// Event that is raised when the connection is terminated</summary>
        public event ConnectHandler   Disconnected;

        /// <summary>
        /// Event that is raised when data is ready to be read</summary>
        public event DataReadyHandler DataReady;

        /// <summary>
        /// Event that is raised when there is an unhandled exception</summary>
        public event ExceptonHandler  UnHandledException;

        /// <summary>
        /// Callback when a connection is made</summary>
        /// <param name="sender">Sender</param>
        /// <param name="target">Target</param>
        public delegate void ConnectHandler(object sender, Target target);

        /// <summary>
        /// Callback when data is ready</summary>
        /// <param name="sender">Sender</param>
        /// <param name="buffer">Data</param>
        public delegate void DataReadyHandler(object sender, byte[] buffer);

        /// <summary>
        /// Callback when there is an unhandled exception</summary>
        /// <param name="sender">Sender</param>
        /// <param name="ex">Exception</param>
        public delegate void ExceptonHandler(object sender, Exception ex);

        #endregion

        /// <summary>
        /// Creates a client TCP socket using an already connected socket with a default
        /// maximum message size</summary>
        /// <param name="s">An already connected socket</param>
        public TargetTcpSocket(Socket s)
            : this(5000)
        {
            if (s == null)
                throw new ArgumentNullException();
            m_theSocket = s;
            if (!IsConnected)
                throw new Exception("only accept connected socket");
            IPEndPoint endpt = (IPEndPoint)s.RemoteEndPoint;
            m_curTarget = new Target("client", endpt.Address.ToString(), endpt.Port);
            m_curTarget.IsConnected = true;
        }

        /// <summary>
        /// Default constructor</summary>
        public TargetTcpSocket()
            : this(5000)
        {
        }

        /// <summary>
        /// Constructor with message size</summary>
        /// <param name="maximumMessageSize">Maximum size of TCP/IP message payloads, in bytes</param>
        /// <remarks>Large blocks of data passed to Send()are broken down into multiple separate
        /// messages of 'maximumMessageSize'.</remarks>
        public TargetTcpSocket(int maximumMessageSize)
        {
            m_cctx = SynchronizationContext.Current;
            if (m_cctx == null)
            {
                throw new Exception("The instance of this class can only be created on a thread"
                    + "that has WindowsFormsSynchronizationContext, ie GUI thread");
            }

            m_theSocket = null;
            m_curTarget = null;
            m_recieveClb = new AsyncCallback(ReceiveClb);
            m_connectClb = new AsyncCallback(ConnectClb);
            m_ConnectionInProgress = false;
            MessageSize = maximumMessageSize;
        }

        /// <summary>
        /// Gets or sets the maximum number of bytes of data to be sent at once on the TCP socket</summary>
        public int MessageSize
        {
            get { return m_messageSize; }
            set
            {
                if (value < 1)
                    throw new ArgumentException("Invalid arg");
                lock (m_syncSocket)
                {
                    m_messageSize = value;
                }
            }
        }

        /// <summary>
        /// Gets whether the socket is connected</summary>
        public bool IsConnected
        {
            get
            {
                // PJO: this fixes a deadlock encountered in SLED when
                // connected to a DevKit and the DevKit is reset/reloaded.
                //
                // This was initially fixed in ATF 2.6 & 2.7 but somehow
                // got removed for ATF 2.8.
                if (m_theSocket == null)
                    return false;

                lock (m_syncSocket)
                {
                    // Must have lock or m_theSocket can be set to null in between condition statement
                    //  and getting the 'Connected' property.
                    return m_theSocket != null && m_theSocket.Connected;
                }
            }
        }

        /// <summary>
        /// Connects using IPEndPoint</summary>
        /// <param name="target">Target</param>
        public void Connect(Target target)
        {
            lock (m_syncSocket)
            {
                if (m_ConnectionInProgress)
                    return;
            }

            if (IsConnected)
                return;

            lock (m_syncSocket)
            {
                if (m_theSocket != null)
                {
                    m_theSocket.Close();
                    m_theSocket = null;
                    m_curTarget = null;
                }
            }

            // try to connect;
            try
            {
                lock (m_syncSocket)
                {
                    IPAddress ipaddr = target.IPAddress;
                    m_theSocket = new Socket(ipaddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    m_theSocket.Blocking = true;
                    m_theSocket.SendTimeout = 5000;

                    //m_theSocket.ExclusiveAddressUse = true;
                    m_theSocket.BeginConnect(ipaddr,target.Port, m_connectClb,target);
                    m_ConnectionInProgress = true;
                }
                
            }
            catch (Exception ex)
            {
                lock (m_syncSocket)
                {
                    if (m_theSocket != null)
                    {
                        m_theSocket.Close();
                        m_theSocket = null;
                        m_curTarget = null;
                    }
                }
                OnUnHandledException(ex);
            }
        }

        /// <summary>
        /// Sends the whole array of bytes to the connected server, breaking it up into multiple
        /// messages if necessary</summary>
        /// <param name="data">Data to send</param>
        public void Send(byte[] data)
        {
            Send(data, data.Length);
        }

        /// <summary>
        /// Sends the data to the connected server, breaking it up into multiple messages if
        /// necessary</summary>
        /// <param name="data">Data to send</param>
        /// <param name="size">Number of bytes to send, starting with data[0]</param>
        public void Send(byte[] data, int size)
        {
            try
            {
                lock (m_syncSocket)
                {
                    if (m_theSocket == null || !m_theSocket.Connected)
                        throw new Exception("The socket is not connected. Please use Connect method to establish a connection");

                    int i = 0;
                    for (i = 0; i <= size - m_messageSize; i += m_messageSize)
                        m_theSocket.Send(data, i, m_messageSize, SocketFlags.None);

                    int remain = size - i;
                    if (remain > 0)
                        m_theSocket.Send(data, i, remain, SocketFlags.None);
                }
            }
            catch (Exception ex)
            {
                lock (m_syncSocket)
                {
                    if (m_theSocket != null && !m_theSocket.Connected)
                    {
                        m_theSocket.Close();
                        m_theSocket = null;
                        OnDisconnected();
                        m_curTarget = null;
                    }
                }
                OnUnHandledException(ex);
            }
        }

        /// <summary>
        /// Disconnects from the server</summary>
        public void Disconnect()
        {
            if (IsConnected)
            {
                lock (m_syncSocket)
                {
                    m_theSocket.Shutdown(SocketShutdown.Both);
                    m_theSocket.Close();
                    m_theSocket = null;
                    OnDisconnected();
                    m_curTarget = null;
                }
            }
        }

        /// <summary>
        /// Starts asynchronous receive</summary>
        public void StartReceive()
        {
            try
            {
                byte[] buf = new byte[m_messageSize];

                if (IsConnected)
                {
                    lock (m_syncSocket)
                    {
                        m_theSocket.BeginReceive(buf, 0, m_messageSize, SocketFlags.None, m_recieveClb, buf);
                    }
                }               
            }
            catch (Exception ex)
            {
                lock (m_syncSocket)
                {
                    if (m_theSocket != null && !m_theSocket.Connected)
                    {
                        m_theSocket.Close();
                        m_theSocket = null;
                        OnDisconnected();
                        m_curTarget = null;
                    }
                }
                OnUnHandledException(ex);
            }
        }


        #region Private Methods

        
        /// <summary>
        /// Receive callback</summary>
        /// <param name="ar">Operation status passed from begin/receive</param>
        private void ReceiveClb(IAsyncResult ar)
        {
            try
            {
                byte[] data = ar.AsyncState as byte[];
                int nBytes = 0;
                if (IsConnected)
                {
                    lock (m_syncSocket)
                    {
                        nBytes = m_theSocket.EndReceive(ar);
                    }
                }
                if (nBytes > 0)
                {
                    byte[] tmpBuf = new byte[nBytes];
                    Buffer.BlockCopy(data,0,tmpBuf,0, nBytes);
                    DataReadyHandler dr = DataReady;
                    if (dr != null)
                    {
                        // raise data ready event on the gui thread.
                        m_cctx.Post(delegate
                        {
                            dr(this, tmpBuf);
                        },null);
                    }
                }
                StartReceive();
            }
            catch (Exception ex)
            {
                bool disconnected = false;
                lock (m_syncSocket)
                {
                    if (m_theSocket != null && !m_theSocket.Connected)
                    {
                        m_theSocket.Close();
                        m_theSocket = null;
                        disconnected = true;
                    }
                }
                if (disconnected)
                {
                    OnDisconnected();
                    m_curTarget = null;
                }
                OnUnHandledException(ex);
            }
        }

        private void ConnectClb(IAsyncResult ar)
        {
            try
            {
                lock (m_syncSocket)
                {
                    m_theSocket.EndConnect(ar);
                    m_ConnectionInProgress = false;
                }
                OnConnect((Target)ar.AsyncState);
                StartReceive();
            }
            catch (Exception ex)
            {
                lock (m_syncSocket)
                {
                    m_ConnectionInProgress = false;
                    if (m_theSocket != null)
                    {
                        m_theSocket.Close();
                        m_theSocket = null;
                        m_curTarget = null;
                    }
                }
                OnUnHandledException(ex);
            }

        }
        /// <summary>
        /// Raises unhandled exception event on the subscriber's thread.
        /// If there is no subscriber, display message box.</summary>
        /// <param name="ex">Exception</param>
        private void OnUnHandledException(Exception ex)
        {
            ExceptonHandler handler = UnHandledException;
            if (handler == null)
            return;
            m_cctx.Send(delegate
            {
                handler(this, ex);
            }, null);
            
        }

        private void OnDisconnected()
        {
            ConnectHandler handler = Disconnected;
            m_curTarget.IsConnected = false;
            lock (m_syncSocket)
            {
                m_ConnectionInProgress = false;
            }
            if (handler != null)
            {
                m_cctx.Send(delegate
                {
                    handler(this, m_curTarget);
                }, null);
            }
        }

        private void OnConnect(Target trg)
        {
            ConnectHandler handler = Connected;
            m_curTarget = trg;
            m_curTarget.IsConnected = true;

            if (handler != null)
            {
                m_cctx.Send(delegate
                {
                    handler(this, trg);
                }, null);
            }
        }

        #endregion

        private volatile Socket m_theSocket;  // the only socket object.
        private volatile Target m_curTarget;   // the ip:port of the server for the current conneciton.
        private readonly AsyncCallback m_recieveClb;   // recieve callback.
        private readonly AsyncCallback m_connectClb;   // recieve callback.        
        private readonly SynchronizationContext m_cctx;
        private volatile object m_syncSocket = new object(); // used to synchronization.
        private int m_messageSize;
        private volatile bool m_ConnectionInProgress;
    }
}
