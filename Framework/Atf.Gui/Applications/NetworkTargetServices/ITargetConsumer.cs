//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Applications.NetworkTargetServices
{
    /// <summary>
    /// Class for objects that consume network target info</summary>
    public interface ITargetConsumer
    {
        /// <summary>
        /// Processes updated TargetInfos</summary>
        /// <param name="targetProvider">The data provider</param>
        /// <param name="targets">A sequence of targets</param>
        void TargetsChanged(ITargetProvider targetProvider, IEnumerable<TargetInfo> targets);

        /// <summary>
        /// Gets or sets selected targets</summary>
        /// <remarks>If this consumer is not a GUI interface, then
        /// return EmptyEnumerable and do nothing on the set.</remarks>
        IEnumerable<TargetInfo> SelectedTargets { get; set; }

        /// <summary>
        /// Gets all targets</summary>
        /// <remarks>If this consumer is not a GUI interface, then return EmptyEnumerable</remarks>
        IEnumerable<TargetInfo> AllTargets { get; }
    }
}
