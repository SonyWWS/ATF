//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Interface for edge routes, which act as sources and destinations for
    /// graph edges</summary>
    public interface IEdgeRoute
    {
        /// <summary>
        /// Gets whether this route can accept multiple edges</summary>
        bool AllowFanIn
        {
            get;
        }

        /// <summary>
        /// Gets whether this route can accept multiple edges</summary>
        bool AllowFanOut
        {
            get;
        }
    }
}
