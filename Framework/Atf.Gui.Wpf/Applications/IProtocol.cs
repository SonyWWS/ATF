//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Interface for providing information about a protocol and interacting with
    /// targets connected via that protocol</summary>
    public interface IProtocol
    {
        /// <summary>
        /// Gets the name of the protocol</summary>
        string Name { get; }

        /// <summary>
        /// Gets the unique ID of the protocol</summary>
        string Id { get; }

        /// <summary>
        /// Gets the version of the protocol</summary>
        Version Version { get; }

        /// <summary>
        /// Get whether this protocol is capable of discovering targets</summary>
        bool CanFindTargets { get; }

        /// <summary>
        /// Discovers the available targets</summary>
        /// <returns>Available targets</returns>
        IEnumerable<ITarget> FindTargets();

        /// <summary>
        /// Create a new transport layer object</summary>
        /// <param name="target">Target for protocol</param>
        /// <returns>Transport layer object</returns>
        ITransportLayer CreateTransportLayer(ITarget target);

        /// <summary>
        /// Get whether this protocol is capable of dealing with user defined targets</summary>
        bool CanCreateUserTarget { get; }

        /// <summary>
        /// If CanCreateUserTarget is true, creates a new user target</summary>
        /// <param name="args">Target arguments</param>
        /// <returns>Created target</returns>
        ITarget CreateUserTarget(string[] args);

        /// <summary>
        /// Display a dialog allowing user to edit a target</summary>
        /// <param name="target">Target to edit</param>
        /// <returns>true if target modified</returns>
        bool EditUserTarget(ITarget target);
    }
}
