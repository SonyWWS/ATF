//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using System;
using System.Collections.Generic;


namespace Sce.Atf.Applications.NetworkTargetServices
{
    /// <summary>
    /// Event args for selected targets changing event</summary>
    public class SelectedTargetsChangedArgs : EventArgs
    {
        public SelectedTargetsChangedArgs(IEnumerable<TargetInfo> oldTargets, IEnumerable<TargetInfo> newTargets)
        {
            OldTargets = oldTargets;
            NewTargets = newTargets;
        }

        public readonly IEnumerable<TargetInfo> OldTargets;
        public readonly IEnumerable<TargetInfo> NewTargets;
    }

}
