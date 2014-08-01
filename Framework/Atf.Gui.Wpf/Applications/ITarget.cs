//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Interface for getting Target information such as name, protocol, and connection status</summary>
    public interface ITarget : IEquatable<ITarget>
    {
        /// <summary>
        /// Gets the target name</summary>
        string Name { get; }

        /// <summary>
        /// Gets the target's host, for example, the IP address and port number for a TCP/IP target</summary>
        string Host { get; }

        /// <summary>
        /// Gets the target's hardware ID</summary>
        string HardwareId { get; }

        /// <summary>
        /// Gets the target's connection status</summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets the target's connection info</summary>
        string ConnectionInfo { get; }

        /// <summary>
        /// Gets target's power status</summary>
        string Status { get; }

        /// <summary>
        /// Gets target's protocol ID</summary>
        string ProtocolId { get; }

        /// <summary>
        /// Gets target's protocol name</summary>
        string ProtocolName { get; }
    }
}
