//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Net;
using System.Xml;
using System.Xml.Serialization;

using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Class representing a TCP/IP target with observable properties</summary>
    [Serializable]
    public class TcpIpTarget : NotifyPropertyChangedBase, ITarget, IXmlSerializable
    {
        /// <summary>
        /// Default constructor - required for serialization</summary>
        public TcpIpTarget()
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Friendly name</param>
        /// <param name="protocol">Protocol ID</param>
        /// <param name="protocolName">Protocol name</param>
        /// <param name="ip">IP Address</param>
        /// <param name="port">Port number</param>
        public TcpIpTarget(string name, string protocol, string protocolName, string ip, uint port)
        {
            Name = name;
            ProtocolId = protocol;
            ProtocolName = protocolName;
            IpAddress = ip;
            Port = port;
        }

        /// <summary>
        /// Port to use for target communication</summary>
        public uint Port
        {
            get { return m_port; }
            set
            {
                if (value > 65535)
                    throw new ArgumentOutOfRangeException("Enter a port between 0 and 65535".Localize());

                m_port = value;
                OnPropertyChanged(s_portArgs);
                UpdateHost();
            }
        }

        /// <summary>
        /// Target's IP address</summary>
        public string IpAddress
        {
            get { return m_ipAddress.ToString(); }
            set
            {
                IPAddress addr = null;
                if (!IPAddress.TryParse(value, out addr))
                {
                    throw new ArgumentException("Invalid Host");
                }

                m_ipAddress = addr;
                OnPropertyChanged(s_ipAddressArgs);
                UpdateHost();
            }
        }

        #region ITarget Members

        /// <summary>
        /// Target's friendly name</summary>
        public string Name
        {
            get { return m_name; }
            set
            {
                m_name = value;
                OnPropertyChanged(s_nameArgs);
            }
        }

        /// <summary>
        /// Gets the target's host, for example, the IP address and port number for a TCP/IP target</summary>
        public string Host { get; protected set; }

        /// <summary>
        /// Gets the target's hardware ID</summary>
        public string HardwareId { get; protected set; }

        /// <summary>
        /// Gets the target's connection status</summary>
        public bool IsConnected
        {
            get { return m_isConnected; }
            internal set { m_isConnected = value; }
        }

        /// <summary>
        /// Gets the target's connection info</summary>
        public string ConnectionInfo { get; protected set; }

        /// <summary>
        /// Gets target's power status</summary>
        public string Status { get; protected set; }

        /// <summary>
        /// Gets target's protocol ID</summary>
        public string ProtocolId { get; protected set; }

        /// <summary>
        /// Gets target's protocol name</summary>
        public string ProtocolName { get; protected set; }

        #endregion

        #region IEquatable Members

        /// <summary>
        /// Test equality against another ITarget</summary>
        /// <param name="other">The ITarget to compare against</param>
        /// <returns><c>True</c> if both ITargets have the same Host and HardwareId, otherwise false</returns>
        public bool Equals(ITarget other)
        {
            return (Host == other.Host) && (HardwareId == other.HardwareId);
        }

        /// <summary>
        /// Test equality against an object</summary>
        /// <param name="obj">The object to compare against</param>
        /// <returns>If the object is an ITarget, returns Equals((ITarget)obj). Otherwise returns false.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as ITarget;
            if (other != null)
                return Equals(other);
            return false;
        }

        /// <summary>
        /// Gets a hash code based on the Host and HardwareId properties</summary>
        /// <returns>Hash code based on the Host and HardwareId properties</returns>
        public override int GetHashCode()
        {
            int hash = 0;
            if (Host != null)
                hash ^= Host.GetHashCode();
            
            if (HardwareId != null)
                hash ^= HardwareId.GetHashCode();
            
            return hash;
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>
        /// Does nothing.</summary>
        /// <returns>null</returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Restores the target's properties from XML</summary>
        /// <param name="reader">XmlReader to use</param>
        public void ReadXml(XmlReader reader)
        {
            Name = reader.GetAttribute("name");
            ProtocolId = reader.GetAttribute("protocol");
            ProtocolName = reader.GetAttribute("protocolname");
            IpAddress = reader.GetAttribute("ip");
            Port = uint.Parse(reader.GetAttribute("port"));
        }

        /// <summary>
        /// Serializes the target's properties to XML</summary>
        /// <param name="writer">XmlWriter to use</param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("name", Name);
            writer.WriteAttributeString("ip", IpAddress);
            writer.WriteAttributeString("port", Port.ToString());
            writer.WriteAttributeString("protocol", ProtocolId);
            writer.WriteAttributeString("protocolname", ProtocolName);
        }

        #endregion

        /// <summary>
        /// ToString implementation that just returns the target Name</summary>
        /// <returns>Target's Name</returns>
        public override string ToString()
        {
            return Name;
        }

        private void UpdateHost()
        {
            Host = IpAddress + " : " + Port;
            OnPropertyChanged(s_hostArgs);
        }

        private static readonly PropertyChangedEventArgs s_nameArgs
            = ObservableUtil.CreateArgs<TcpIpTarget>(x => x.Name);

        private static readonly PropertyChangedEventArgs s_hostArgs
            = ObservableUtil.CreateArgs<TcpIpTarget>(x => x.Host);

        private static readonly PropertyChangedEventArgs s_portArgs
            = ObservableUtil.CreateArgs<TcpIpTarget>(x => x.Port);

        private static readonly PropertyChangedEventArgs s_ipAddressArgs
            = ObservableUtil.CreateArgs<TcpIpTarget>(x => x.IpAddress);

        private string m_name;
        private uint m_port;
        private IPAddress m_ipAddress;

        [NonSerialized]
        private bool m_isConnected;
    }
}
