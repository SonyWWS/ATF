//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Interface for hierarchical nodes in a graph, which contain sub-nodes</summary>
    /// <typeparam name="TNode">Node, must implement IGraphNode</typeparam>
    /// <typeparam name="TEdge">Edge, must implement IGraphEdge</typeparam>
    /// <typeparam name="TEdgeRoute">Edge route, must implement IEdgeRoute</typeparam>
    public interface IHierarchicalGraphNode<out TNode, TEdge, TEdgeRoute> : IGraphNode
        where TNode : class, IGraphNode
        where TEdge : class, IGraphEdge<TNode, TEdgeRoute>
        where TEdgeRoute : class, IEdgeRoute
    {
        /// <summary>
        /// Gets the sequence of nodes that are children of this hierarchical graph node</summary>
        IEnumerable<TNode> SubNodes
        {
            get;
        }
    }
}
