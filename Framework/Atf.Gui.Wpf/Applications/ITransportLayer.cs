//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Threading;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Interface for transport layer
    /// </summary>
    public interface ITransportLayer : IDisposable
    {
        /// <summary>
        /// Begin Async connect to a server
        /// </summary>
        void BeginConnect();

        /// <summary>
        /// Begin Async Send transport packet to a server
        /// </summary>
        /// <param name="data">Transport packet to send</param>
        void BeginSend(byte[] data);

        /// <summary>
        /// Close connection, end operations
        /// </summary>
        void Close();

        /// <summary>
        /// Gets Incoming transport packets
        /// </summary>
        /// <remarks>If Connection was closed then empty packet placed in IncomingPackets
        /// and DisconnectMessage set to appropriate message</remarks>
        /// <returns>Queue with received transport packets</returns>
        Queue<byte[]> GetIncomingPackets();

        /// <summary>
        /// Gets Connection status
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Gets First Exception information, null if ther is no Exception
        /// </summary>
        Exception Exception { get; }

        /// <summary>
        /// Gets or sets a Connect timeout value.
        /// The default value is TimeSpan.Zero, which indicates that only one attempt will
        /// be made through underlying socket with no subsequent retries.
        /// Specifying TimeSpan.MaxValue indicates an infinite time-out period with infinite number of retries.
        /// </summary>
        TimeSpan ConnectTimeout { set; get; }

        /// <summary>
        /// Gets Disconnect Message
        /// </summary>
        string DisconnectMessage { get; }

        /// <summary>
        /// Event object that must be set on any Transport Event
        /// </summary>
        AutoResetEvent TransportEvent { get; }
    }
}
