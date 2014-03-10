//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Interface that collects statistics</summary>
    public interface IQueryObjectStats
    {
        /// <summary>
        /// Gets object statistics</summary>
        /// <param name="stats">Statistics</param>
        void GetStats(ObjectStats stats);
    }
}
