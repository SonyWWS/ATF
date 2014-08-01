//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface to keep track of the last item hit</summary>
    public interface ILastHitAware
    {
        /// <summary>
        /// Gets and sets the last hit item</summary>
        object LastHit { get; set; }
    }
}
