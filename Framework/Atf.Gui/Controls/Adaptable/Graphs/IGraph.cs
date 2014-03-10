//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
#if CS_4
    /// <summary>
    /// Interface for a graph, a collection of nodes with connecting edges</summary>
    public interface IGraph<out TNode, out TEdge, out TEdgeRoute>
#else
    public interface IGraph<TNode, TEdge, TEdgeRoute>
#endif
        where TNode : class, IGraphNode
        where TEdge : class, IGraphEdge<TNode, TEdgeRoute>
        where TEdgeRoute : class, IEdgeRoute
    {
        /// <summary>
        /// Gets the nodes in the graph</summary>
        IEnumerable<TNode> Nodes
        {
            get;
        }

        /// <summary>
        /// Gets the edges in the graph</summary>
        IEnumerable<TEdge> Edges
        {
            get;
        }
    }

    /// <summary>
    /// Extension methods to make working with IGraph easier</summary>
    public static class Graphs
    {
        /// <summary>
        /// Gets the edges that connect to the given node</summary>
        /// <typeparam name="TNode">Graph node</typeparam>
        /// <typeparam name="TEdge">Edge</typeparam>
        /// <typeparam name="TEdgeRoute">Edge route</typeparam>
        /// <param name="graph">Graph that contains this node</param>
        /// <param name="node">Node whose edges are returned</param>
        /// <returns>Edges that connect to the given node</returns>
        /// <remarks>In the future, this method could look for additional interfaces for improved performance</remarks>
        public static IEnumerable<TEdge> GetEdges<TNode, TEdge, TEdgeRoute>(this IGraph<TNode, TEdge, TEdgeRoute> graph, TNode node)
            where TNode : class, IGraphNode
            where TEdge : class, IGraphEdge<TNode, TEdgeRoute>
            where TEdgeRoute : class, IEdgeRoute
        {
            foreach (TEdge edge in graph.Edges)
                if (edge.FromNode == node || edge.ToNode == node)
                    yield return edge;
        }

        /// <summary>
        /// Gets the "outgoing" edges that connect to the given node. These are the edges whose FromNode
        /// matches the given node.</summary>
        /// <typeparam name="TNode">Graph node</typeparam>
        /// <typeparam name="TEdge">Edge</typeparam>
        /// <typeparam name="TEdgeRoute">Edge route</typeparam>
        /// <param name="graph">Graph that contains this node</param>
        /// <param name="node">Node</param>
        /// <returns>"Outgoing" edges that connect to the given node</returns>
        public static IEnumerable<TEdge> GetOutputEdges<TNode, TEdge, TEdgeRoute>(this IGraph<TNode, TEdge, TEdgeRoute> graph, TNode node)
            where TNode : class, IGraphNode
            where TEdge : class, IGraphEdge<TNode, TEdgeRoute>
            where TEdgeRoute : class, IEdgeRoute
        {
            foreach (TEdge edge in graph.GetEdges(node))
                if (edge.FromNode == node)
                    yield return edge;
        }

        /// <summary>
        /// Gets the "incoming" edges that connect to the given node. These are the edges whose ToNode
        /// matches the given node.</summary>
        /// <param name="graph">Graph that contains this node</param>
        /// <param name="node">Given node</param>
        /// <returns>"Incoming" edges that connect to the given node</returns>
        public static IEnumerable<TEdge> GetInputEdges<TNode, TEdge, TEdgeRoute>(this IGraph<TNode, TEdge, TEdgeRoute> graph, TNode node)
            where TNode : class, IGraphNode
            where TEdge : class, IGraphEdge<TNode, TEdgeRoute>
            where TEdgeRoute : class, IEdgeRoute
        {
            foreach (TEdge edge in graph.GetEdges(node))
                if (edge.ToNode == node)
                    yield return edge;
        }

        /// <summary>
        /// Gets the "outgoing" nodes that connect to the given node via an outgoing edge.
        /// An outgoing edge is an edge whose FromNode matches the given node.</summary>
        /// <param name="graph">Graph that contains this node</param>
        /// <param name="node">Node</param>
        /// <returns>"Outgoing" nodes that connect to the given node via an outgoing edge</returns>
        public static IEnumerable<TNode> GetOutputNodes<TNode, TEdge, TEdgeRoute>(this IGraph<TNode, TEdge, TEdgeRoute> graph, TNode node)
            where TNode : class, IGraphNode
            where TEdge : class, IGraphEdge<TNode, TEdgeRoute>
            where TEdgeRoute : class, IEdgeRoute
        {
            foreach (TEdge edge in graph.GetEdges(node))
                if (edge.FromNode == node)
                    yield return edge.ToNode;
        }

        /// <summary>
        /// Gets the "incoming" nodes that connect to the given node via an incoming edge.
        /// An incoming edge is an edge whose ToNode matches the given node.</summary>
        /// <param name="graph">Graph that contains this node</param>
        /// <param name="node">Node</param>
        /// <returns>"Incoming" nodes that connect to the given node via an incoming edge</returns>
        public static IEnumerable<TNode> GetInputNodes<TNode, TEdge, TEdgeRoute>(this IGraph<TNode, TEdge, TEdgeRoute> graph, TNode node)
            where TNode : class, IGraphNode
            where TEdge : class, IGraphEdge<TNode, TEdgeRoute>
            where TEdgeRoute : class, IEdgeRoute
        {
            foreach (TEdge edge in graph.GetEdges(node))
                if (edge.ToNode == node)
                    yield return edge.FromNode;
        }

        /// <summary>
        /// Gets the nodes that connect to the given node</summary>
        /// <param name="graph">Graph that contains this node</param>
        /// <param name="node">Node</param>
        /// <returns>Nodes that connect to the given node</returns>
        public static IEnumerable<TNode> GetNodes<TNode, TEdge, TEdgeRoute>(this IGraph<TNode, TEdge, TEdgeRoute> graph, TNode node)
            where TNode : class, IGraphNode
            where TEdge : class, IGraphEdge<TNode, TEdgeRoute>
            where TEdgeRoute : class, IEdgeRoute
        {
            foreach (TEdge edge in graph.GetEdges(node))
            {
                if (edge.FromNode == node)
                    yield return edge.ToNode;
                else if (edge.ToNode == node)
                    yield return edge.FromNode;
            }
        }
    }
}
