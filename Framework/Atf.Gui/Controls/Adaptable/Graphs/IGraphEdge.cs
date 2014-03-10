//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Adaptable.Graphs
{
#if CS_4
    /// <summary>
    /// Interface for edges in a graph</summary>
    /// <typeparam name="TNode">Node type, must implement IGraphNode</typeparam>
    public interface IGraphEdge<out TNode>
#else
    public interface IGraphEdge<TNode>
#endif
        where TNode : class, IGraphNode
    {
        /// <summary>
        /// Gets edge's source node</summary>
        TNode FromNode
        {
            get;
        }

        /// <summary>
        /// Gets edge's destination node</summary>
        TNode ToNode
        {
            get;
        }

        /// <summary>
        /// Gets edge's label</summary>
        string Label
        {
            get;
        }
    }

#if CS_4
    /// <summary>
    /// Interface for routed edges in a graph. Routed edges connect nodes and
    /// have a defined source and destination route from and to the nodes.</summary>
    /// <typeparam name="TNode">Node type, must implement IGraphNode</typeparam>
    /// <typeparam name="TEdgeRoute">Edge route type, must implement IEdgeRoute</typeparam>
    public interface IGraphEdge<out TNode, out TEdgeRoute> : IGraphEdge<TNode>
#else
    public interface IGraphEdge<TNode, TEdgeRoute> : IGraphEdge<TNode>
#endif
        where TNode : class, IGraphNode
        where TEdgeRoute : class, IEdgeRoute
    {
        /// <summary>
        /// Gets the route taken from the source node</summary>
        TEdgeRoute FromRoute
        {
            get;
        }

        /// <summary>
        /// Gets the route taken to the destination node</summary>
        TEdgeRoute ToRoute
        {
            get;
        }
    }
}
