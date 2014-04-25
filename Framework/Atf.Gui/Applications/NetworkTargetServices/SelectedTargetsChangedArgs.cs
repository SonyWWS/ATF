//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using System;
using System.Collections.Generic;


namespace Sce.Atf.Applications.NetworkTargetServices
{
    /// <summary>
    /// Event args for selected targets changing event</summary>
    public class SelectedTargetsChangedArgs : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="oldTargets">Previous targets</param>
        /// <param name="newTargets">New targets</param>
        public SelectedTargetsChangedArgs(IEnumerable<TargetInfo> oldTargets, IEnumerable<TargetInfo> newTargets)
        {
            OldTargets = oldTargets;
            NewTargets = newTargets;
        }

        /// <summary>
        /// Previous targets</summary>
        public readonly IEnumerable<TargetInfo> OldTargets;
        /// <summary>
        /// New targets</summary>
        public readonly IEnumerable<TargetInfo> NewTargets;
    }

}
