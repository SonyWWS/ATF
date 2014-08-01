//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Interface for discovery of targets by protocol</summary>
    public interface ITargetDiscovery
    {
        /// <summary>
        /// Gets the protocol ID</summary>
        string Id { get; }
        
        /// <summary>
        /// Gets the name of the protocol (e.g., TCP/IP)</summary>
        string ProtocolName { get; }

        /// <summary>
        /// Gets list of targets discovered for the specified protocol</summary>
        IEnumerable<ITarget> Targets { get; }
    }
}