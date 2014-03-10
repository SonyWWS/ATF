//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Net;

namespace Sce.Atf.Applications.NetworkTargetServices
{
    /// <summary>
    /// Represents a named network endpoint</summary>
    public class Target : ICloneable
    {
       
        private int m_port = -1;
        private string m_name;
        private string m_host;        
        private object m_tag;
        private string m_protocol = "none";
        private bool m_selected;
        private bool m_connected;
        

        /// <summary>
        /// Constructs a new Target from name, host name, and port number</summary>
        /// <param name="name">Target name</param>
        /// <param name="host">URL or IP address</param>
        /// <param name="port">Port number</param>
        public Target(string name, string host, int port)
        {
            Set(name, host, port);            
        }
        /// <summary>
        /// Gets the target name</summary>
        public string Name
        {
            get { return m_name; }
        }

        internal void Set(string name, string host, int port)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(host))
                throw new ArgumentNullException();
            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
                throw new ArgumentOutOfRangeException();

            m_name = name;
            m_host = host;
            m_port = port;
        }
 
        /// <summary>
        /// Gets and sets a value indicating whether this target is selected</summary>
        public bool Selected
        {
            get { return m_selected; }
            set { m_selected = value; }
        }

        /// <summary>
        /// Gets and sets a value indicating if the target is connected</summary>
        public bool IsConnected
        {
            get { return m_connected; }
            set { m_connected = value; }
        }

        /// <summary>
        /// Gets and sets the Target protocol</summary>
        public string Protocol
        {
            get { return m_protocol; }
            set { m_protocol = value; }
        }
        /// <summary>
        /// Gets and sets the tag, an object associated with the target</summary>
        public object Tag
        {
            get { return m_tag; }
            set { m_tag = value; }
        }


        /// <summary>
        /// Gets host name for this target</summary>
        public string Host
        {
            get { return m_host; }
        }

        /// <summary>
        /// Gets the address as IPAdress</summary>
        public IPAddress IPAddress
        {
            get
            {
                IPAddress addr = null;
                if (!IPAddress.TryParse(m_host, out addr))
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(m_host);

                    // accept the first address that we can connect to.
                    foreach (IPAddress address in hostEntry.AddressList)
                    {
                        addr = address;
                        break;
                    }
                }
                return addr;
            }
        }

        /// <summary>
        /// Gets the endpoint port number for the current target</summary>
        public int Port
        {
            get { return m_port; }
        }

        /// <summary>
        /// Converts the instance to its string representation</summary>
        /// <returns>String representation of object</returns>
        public override string ToString()
        {
            return string.Format("{0} ({1} : {2})", m_name, m_host, m_port);
        }

        #region ICloneable Members

        /// <summary>
        /// Creates a new object that is a copy of the current instance</summary>
        /// <returns>New object that is a copy of the current instance</returns>
        public object Clone()
        {
            
            Target t = new Target(m_name, m_host,m_port);
            t.m_connected = m_connected;
            t.m_selected = m_selected;
            t.m_tag = m_tag;
            t.m_protocol = m_protocol;
            return t;
        }

        #endregion
    }

    

}
