//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;
using System;
using System.Text;
using Wws.LiveConnect;

namespace Sce.Atf
{
    /// <summary>
    /// This service exposes WWS SDK's LiveConnect, which provides a way to communicate between
    /// applications that are on the same subnet, using automated discovery and zero-configuration.
    /// Messages are broadcast and listened to on named channels. LiveConnectService hard-codes
    /// the channel to be "ATF_Global".
    /// Bonjour must be installed first. Bonjour64.msi can be installed for 64-bit Windows operating
    /// systems and Bonjour.msi can be installed for 32-bit Windows operating systems. Apps that use
    /// this service should choose 32 bit mode or 64 bit mode, but not AnyCPU.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(LiveConnectService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class LiveConnectService : IInitializable
    {
        #region IInitializable Members

        /// <summary>
        /// Initialize instance</summary>
        public void Initialize()
        {
            CommonInit();
        }

        #endregion

        /// <summary>
        /// Broadcasts a message</summary>
        /// <param name="message">Message to broadcast</param>
        public void Send(string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            Client.Send(AtfGlobalChannel, AtfStringMessage, bytes);
        }

        /// <summary>
        /// Broadcasts an array of bytes</summary>
        /// <param name="bytes">Array of bytes to broadcast</param>
        public void Send(byte[] bytes)
        {
            Client.Send(AtfGlobalChannel, AtfByteArrayMessage, bytes);
        }

        /// <summary>
        /// Sends a text string to a particular process that previously broadcast a message</summary>
        /// <param name="senderId">The sender ID from a previous LiveConnectMessageArgs</param>
        /// <param name="message">Message to send</param>
        /// <returns>An error code. 0 means "success". Otherwise, it means that senderId
        /// is unrecognized.</returns>
        public uint SendTo(uint senderId, string message)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);
            return Client.SendTo(senderId, AtfGlobalChannel, AtfStringMessage, bytes);
        }

        /// <summary>
        /// Sends a byte array to a particular process that previously broadcast a message</summary>
        /// <param name="senderId">The sender ID from a previous LiveConnectMessageArgs</param>
        /// <param name="bytes">Message to send</param>
        /// <returns>An error code. 0 means "success". Otherwise, it means that senderId
        /// is unrecognized.</returns>
        public uint SendTo(uint senderId, byte[] bytes)
        {
            return Client.SendTo(senderId, AtfGlobalChannel, AtfByteArrayMessage, bytes);
        }

        /// <summary>
        /// Event for receiving broadcasted messages</summary>
        public event EventHandler<LiveConnectMessageArgs> MessageReceived;

        /// <summary>
        /// Event args for a LiveConnect broadcasted message</summary>
        public class LiveConnectMessageArgs : EventArgs
        {
            /// <summary>
            /// Gets the message payload as a string. Is null if the payload was not sent as a string
            /// using LiveConnectService.</summary>
            public string MessageString
            {
                get
                {
                    if (m_messageString == null)
                    {
                        if (m_messageId == AtfStringMessageHash)
                            m_messageString = Encoding.UTF8.GetString(MessageBytes, 0, MessageBytes.Length);
                    }
                    return m_messageString;
                }
            }

            /// <summary>
            /// Gets the message payload as a byte array</summary>
            public byte[] MessageBytes
            {
                get;
                private set;
            }

            /// <summary>
            /// Returns whether or not this message's ID matches the given 'id' parameter</summary>
            /// <param name="id">ID</param>
            /// <returns><c>True</c> if message's ID matches the given 'id' parameter</returns>
            /// <remarks>This may be useful when receiving messages from non-ATF apps that are using
            /// the WWS SDK's LiveConnect directly</remarks>
            public bool CheckMessageId(string id)
            {
                return Client.FNV1Hash(id) == m_messageId;
            }

            /// <summary>
            /// Gets the sender's ID. This can be used to respond directly to the sender.</summary>
            /// <remarks>This ID is typically the hash code, using FNV1Hash, of the sender's name</remarks>
            public uint SenderId
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the computer name and port number that sent this message</summary>
            public string SenderName
            {
                get { return Client.GetServiceNameForID(SenderId); }
            }

            internal LiveConnectMessageArgs(uint senderId, uint messageId, byte[] data)
            {
                SenderId = senderId;
                m_messageId = messageId;
                MessageBytes = data;
            }

            private readonly uint m_messageId;
            private string m_messageString;
        }

        /// <summary>
        /// Common intialization</summary>
        protected void CommonInit()
        {
            string[] groups = new string[] { AtfGlobalChannel };
            Errors error = (Errors)Client.Init(null, groups);
            if (error == Errors.LIVECONNECT_SUCCESS)
            {
                Client.Subscribe(AtfGlobalChannel, GotMessage, null);
            }
            else if (OutputWriter != null)
            {
                string msg;
                if (error == Errors.LIVECONNECT_TOOMANYADVERTS)
                    msg = "The Live Connect Service failed to initialize due to too many advertisers.".Localize();
                else if (error == Errors.LIVECONNECT_MDNSREGISTERFAIL)
                    msg = "The Live Connect Service failed to initialize because Bonjour is either not installed or the Windows service, 'Bonjour Service', is not running.".Localize();
                else
                    msg = "The Live Connect Service failed to initialize for some unknown reason: ".Localize() + error + '.';
                msg += Environment.NewLine;
                OutputWriter.Write(OutputMessageType.Warning, msg);
            }
        }

        private void GotMessage(uint status, uint senderId, uint groupId, uint messageId, byte[] data, object context)
        {
            if (MessageReceived != null)
            {
                var args = new LiveConnectMessageArgs(senderId, messageId, data);
                MessageReceived.Invoke(this, args);
            }
        }

        static LiveConnectService()
        {
            AtfStringMessageHash = Client.FNV1Hash(AtfStringMessage);
        }

        private enum Errors
        {
            LIVECONNECT_SUCCESS = 0,
            LIVECONNECT_ERROR_WINSOCK = 90001,
            LIVECONNECT_ERROR_BINDPORT,
            LIVECONNECT_ERROR_LISTEN,
            LIVECONNECT_TOOMANYSERVERS,
            LIVECONNECT_NOHOST,
            LIVECONNECT_CONNECT,
            LIVECONNECT_NOTREADY,
            LIVECONNECT_TOOMANYSUBS,
            LIVECONNECT_TOOMANYADVERTS,
            LIVECONNECT_NOCONNECTION,
            LIVECONNECT_MDNSREGISTERFAIL,
            LIVECONNECT_NOSUBSCRIPTION,
        };

        /// <summary>
        /// Gets or sets the <see cref="Sce.Atf.IOutputWriter"/> to use</summary>
        [Import(AllowDefault = true)]
        protected IOutputWriter OutputWriter { get; set; }

        private const string AtfGlobalChannel = "ATF_Global";
        private const string AtfStringMessage = "ATF_String";
        private const string AtfByteArrayMessage = "ATF_ByteArray";
        private static uint AtfStringMessageHash;
    }
}
