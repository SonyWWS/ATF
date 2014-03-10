using System.Collections.Generic;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
#if CS_4
    public interface IEditableGraphContainer<in TNode, TEdge, in TEdgeRoute>:
        IEditableGraph<TNode, TEdge, TEdgeRoute>
        where TNode : class, IGraphNode
        where TEdge : class, IGraphEdge<TNode, TEdgeRoute>
        where TEdgeRoute : class, IEdgeRoute
#else
    public interface IEditableGraphContainer<TNode, TEdge, TEdgeRoute>:
        IEditableGraph<TNode, TEdge, TEdgeRoute>
        where TNode : class, IGraphNode
        where TEdge : class, IGraphEdge<TNode, TEdgeRoute>
        where TEdgeRoute : class, IEdgeRoute
#endif
    {
        /// <summary>
        /// Can move the given nodes into the new container</summary>    
        /// <param name="newParent">the new container</param>
        /// <param name="movingObjects">nodes to move</param>
        bool CanMove(object newParent, IEnumerable<object> movingObjects);

        /// <summary>
        /// Move the given nodes into the container
        /// </summary>
        /// <param name="newParent">the new container</param>
        /// <param name="movingObjects">nodes to move</param>
        void Move(object newParent, IEnumerable<object> movingObjects);

        bool CanResize(object container, DiagramBorder borderPart);

        void Resize(object container, int newWidth, int newHeight);
    }
}
