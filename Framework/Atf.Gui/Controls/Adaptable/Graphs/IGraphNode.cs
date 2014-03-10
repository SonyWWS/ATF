//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Interface for a node in a graph; nodes are connected by edges</summary>
    public interface IGraphNode
    {
        /// <summary>
        /// Gets the node name</summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// Gets the bounding rectangle for the node in world space (or local space if a
        /// hierarchy is involved, as with sub-circuits). The location portion should always
        /// be accurate, but the renderer should be queried for the size of the rectangle.
        /// See D2dGraphRenderer.GetBounds() or use ILayoutContext.GetBounds().</summary>
        Rectangle Bounds
        {
            get;
        }
    }
}
