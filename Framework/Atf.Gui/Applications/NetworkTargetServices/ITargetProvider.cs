//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;


namespace Sce.Atf.Applications.NetworkTargetServices
{
    /// <summary>
    /// Provides information about a particular kind of development device available on the network</summary>
    public interface ITargetProvider
    {
        /// <summary>
        /// Gets the provider's user-readable name</summary>
        string Name { get; }

        /// <summary>
        /// Retrieves the targets' data</summary>
        /// <param name="targetConsumer">The target consumer to retrieve the data for</param>
        /// <returns>Targets to consume on the target consumer</returns>
        IEnumerable<TargetInfo> GetTargets(ITargetConsumer targetConsumer);

        /// <summary>
        /// Gets whether you can create a new target using the CreateNew method</summary>
        bool CanCreateNew { get; }

        /// <summary>
        /// Creates a new target</summary>
        /// <remarks>Creates and returns a TargetInfo, but does not add it to the watched list</remarks>
        /// <returns>TargetInfo for new target</returns>
        TargetInfo CreateNew();

        /// <summary>
        /// Adds the target to the provider</summary>
        /// <param name="target">Target to be added to the provider</param>
        /// <returns>True iff the target is successfully added</returns>
        bool AddTarget(TargetInfo target);

        /// <summary>
        /// Removes the target from the provider</summary>
        /// <param name="target">Target to be removed from the provider</param>
        /// <returns>True iff the target is successfully removed</returns>
        bool Remove(TargetInfo target);
    }
}
