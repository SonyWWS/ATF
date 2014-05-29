//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Adaptable.Graphs
{
#if CS_4
    /// <summary>
    /// Interface for a graph that can be edited by a control</summary>
    /// <typeparam name="TNode">IGraphNode node</typeparam>
    /// <typeparam name="TEdge">IGraphEdge edge</typeparam>
    /// <typeparam name="TEdgeRoute">IEdgeRoute edge route</typeparam>
    public interface IEditableGraph<in TNode, TEdge, in TEdgeRoute>
        where TNode : class, IGraphNode
        where TEdge : class, IGraphEdge<TNode, TEdgeRoute>
        where TEdgeRoute : class, IEdgeRoute
#else
    public interface IEditableGraph<TNode, TEdge, TEdgeRoute>
        where TNode : class, IGraphNode
        where TEdge : class, IGraphEdge<TNode, TEdgeRoute>
        where TEdgeRoute : class, IEdgeRoute
#endif
    {
        /// <summary>
        /// Returns whether two nodes can be connected. "from" and "to" refer to the corresponding
        /// properties in IGraphEdge, not to a dragging operation, for example.</summary>
        /// <param name="fromNode">"From" node</param>
        /// <param name="fromRoute">"From" edge route</param>
        /// <param name="toNode">"To" node</param>
        /// <param name="toRoute">"To" edge route</param>
        /// <returns>Whether the "from" node/route can be connected to the "to" node/route</returns>
        bool CanConnect(
            TNode fromNode,
            TEdgeRoute fromRoute,
            TNode toNode,
            TEdgeRoute toRoute);

        /// <summary>
        /// Connects the "from" node/route to the "to" node/route by creating an IGraphEdge whose
        /// "from" node is "fromNode", "to" node is "toNode", etc.</summary>
        /// <param name="fromNode">"From" node</param>
        /// <param name="fromRoute">"From" edge route</param>
        /// <param name="toNode">"To" node</param>
        /// <param name="toRoute">"To" edge route</param>
        /// <param name="existingEdge">Existing edge that is being reconnected, or null if new edge</param>
        /// <returns>New edge connecting the "from" node/route to the "to" node/route</returns>
        TEdge Connect(
            TNode fromNode,
            TEdgeRoute fromRoute,
            TNode toNode,
            TEdgeRoute toRoute,
            TEdge existingEdge);

        /// <summary>
        /// Gets whether the edge can be disconnected</summary>
        /// <param name="edge">Edge to disconnect</param>
        /// <returns>Whether the edge can be disconnected</returns>
        bool CanDisconnect(TEdge edge);

        /// <summary>
        /// Disconnects the edge</summary>
        /// <param name="edge">Edge to disconnect</param>
        void Disconnect(TEdge edge);
    }
}
