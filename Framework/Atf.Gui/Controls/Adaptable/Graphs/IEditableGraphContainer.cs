using System.Collections.Generic;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Interface for container of graph objects. Its methods allow moving items in and out of the container and resizing it.</summary>
    /// <typeparam name="TNode">IGraphNode object</typeparam>
    /// <typeparam name="TEdge">IGraphEdge object</typeparam>
    /// <typeparam name="TEdgeRoute">IEdgeRoute object</typeparam>
    public interface IEditableGraphContainer<in TNode, TEdge, in TEdgeRoute>:
        IEditableGraph<TNode, TEdge, TEdgeRoute>
        where TNode : class, IGraphNode
        where TEdge : class, IGraphEdge<TNode, TEdgeRoute>
        where TEdgeRoute : class, IEdgeRoute
    {
        /// <summary>
        /// Can move the given nodes into the new container</summary>    
        /// <param name="newParent">New container</param>
        /// <param name="movingObjects">Nodes to move</param>
        /// <returns><c>True</c> if can move nodes into new container</returns>
        bool CanMove(object newParent, IEnumerable<object> movingObjects);

        /// <summary>
        /// Move the given nodes into a container</summary>
        /// <param name="newParent">New container</param>
        /// <param name="movingObjects">Nodes to move</param>
        void Move(object newParent, IEnumerable<object> movingObjects);

        /// <summary>
        /// Can a container be resized</summary>
        /// <param name="container">Container to resize</param>
        /// <param name="borderPart">Part of border to resize</param>
        /// <returns><c>True</c> if the container border can be resized</returns>
        bool CanResize(object container, DiagramBorder borderPart);

        /// <summary>
        /// Resize a container</summary>
        /// <param name="container">Container to resize</param>
        /// <param name="newWidth">New container width</param>
        /// <param name="newHeight">New container height</param>
        void Resize(object container, int newWidth, int newHeight);
    }
}
