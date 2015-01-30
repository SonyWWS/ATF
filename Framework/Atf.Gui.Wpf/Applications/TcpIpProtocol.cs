//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Class for providing information about the TcpIp protocol and interacting with
    /// targets connected via TcpIp</summary>
    [Export(typeof(IProtocol))]
    [Export(typeof(TcpIpProtocol))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TcpIpProtocol : IProtocol
    {
        /// <summary>
        /// Default constructor</summary>
        public TcpIpProtocol()
        {
            DefaultPortNumber = 4001;
        }

        /// <summary>
        /// Get or set default port number</summary>
        public uint DefaultPortNumber { get; set; }

        #region IProtocol Members

        /// <summary>
        /// Gets the name of the protocol</summary>
        public string Name { get { return "TCP/IP"; } }

        /// <summary>
        /// Gets the unique ID of the protocol</summary>
        public string Id { get { return ms_sId.ToString(); } }

        /// <summary>
        /// Gets the version of the protocol</summary>
        public Version Version { get { return SVersion; } }

        /// <summary>
        /// Get whether this protocol is capable of discovering targets</summary>
        public bool CanFindTargets
        {
            get { return m_targetDiscovery != null && m_targetDiscovery.Any(x => x.ProtocolName == Name); }
        }

        /// <summary>
        /// Discovers the available targets</summary>
        /// <returns>Available targets</returns>
        public IEnumerable<ITarget> FindTargets()
        {
            return m_targetDiscovery.SelectMany(x => x.Targets);
        }

        /// <summary>
        /// Create a new transport layer object</summary>
        /// <param name="target">Target for protocol</param>
        /// <returns>Transport layer object</returns>
        public ITransportLayer CreateTransportLayer(ITarget target)
        {
            return new TcpIpTransport(target as TcpIpTarget);
        }

        /// <summary>
        /// Get whether this protocol is capable of dealing with user defined targets</summary>
        public bool CanCreateUserTarget { get { return true; } }

        /// <summary>
        /// If CanCreateUserTarget is true, creates a new user target</summary>
        /// <param name="args">Target arguments</param>
        /// <returns>Created target</returns>
        public ITarget CreateUserTarget(string[] args = null)
        {
            string ip = "255.255.255.255";
            uint port = DefaultPortNumber;

            if (args != null)
            {
                if (args.Length > 0)
                    ip = args[0];
                if (args.Length > 1)
                    port = UInt32.Parse(args[1]);
            }
            return new TcpIpTarget("New Target", Id, Name, ip, port);
        }

        /// <summary>
        /// Display a dialog allowing user to edit a target</summary>
        /// <param name="target">Target to edit</param>
        /// <returns>true if target modified</returns>
        public bool EditUserTarget(ITarget target)
        {
            var t = target as TcpIpTarget;
            if (t == null)
                throw new ArgumentException("Can only edit TCP/IP targets");

            bool? res = DialogUtils.ShowDialogWithViewModel<TcpIpTargetEditDialog>(new TcpIpTargetEditDialogViewModel(t));
            
            return res.HasValue && res.Value;
        }

        #endregion

        [ImportMany]
        private IEnumerable<ITargetDiscovery> m_targetDiscovery = null;

        private static Guid ms_sId = new Guid(0xaa5de2d1, 0x4437, 0x4a50, 0x81, 0xbb, 0xd0, 0xa9, 0x5c, 0xf1, 0x6e, 0x8a);
        private static readonly Version SVersion = new Version(1, 0, 0);
    }
}
