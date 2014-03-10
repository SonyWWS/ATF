//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Interface for states in statechart diagrams that are non-pseudo-states</summary>
    public interface IComplexState<TNode, TEdge> : IHierarchicalGraphNode<TNode, TEdge, BoundaryRoute>
        where TNode : class, IState
        where TEdge : class, IGraphEdge<TNode, BoundaryRoute>
    {
        /// <summary>
        /// Gets the state's interior text</summary>
        string Text
        {
            get;
        }
    }
}
