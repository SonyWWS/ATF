//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Interface for graph adapters, which manage rendering and picking for a graph
    /// in the adapted control</summary>
    /// <typeparam name="TNode">Node type, must implement IGraphNode</typeparam>
    /// <typeparam name="TEdge">Edge type, must implement IGraphEdge</typeparam>
    /// <typeparam name="TEdgeRoute">Edge route type, must implement IEdgeRoute</typeparam>
    public interface IGraphAdapter<TNode, TEdge, TEdgeRoute>
        where TNode : class, IGraphNode
        where TEdge : class, IGraphEdge<TNode, TEdgeRoute>
        where TEdgeRoute : class, IEdgeRoute
    {
        /// <summary>
        /// Sets the rendering style for a diagram item. This style overrides the
        /// normal style used by the graph adapter.</summary>
        /// <param name="item">Rendered item</param>
        /// <param name="style">Diagram style to use for item</param>
        void SetStyle(object item, DiagramDrawingStyle style);

        /// <summary>
        /// Resets the rendering style for a diagram item</summary>
        /// <param name="item">Rendered item</param>
        void ResetStyle(object item);

        /// <summary>
        /// Gets the current rendering style for an item</summary>
        /// <param name="item">Rendered item</param>
        /// <returns>Rendering style set by SetStyle, Normal if no override is set</returns>
        DiagramDrawingStyle GetStyle(object item);

        /// <summary>
        /// Performs a picking operation on the graph with the point</summary>
        /// <param name="p">Hit test point</param>
        /// <returns>Hit record resulting from picking operation</returns>
        GraphHitRecord<TNode, TEdge, TEdgeRoute> Pick(Point p);
    }
}
