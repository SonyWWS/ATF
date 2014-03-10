//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    public interface IHierarchicalCircuitElementType<TElement, TWire, TPin> :
        IHierarchicalGraphNode<TElement, TWire, TPin>, ICircuitElementType
        where TElement : class, ICircuitElement
        where TWire : class, IGraphEdge<TElement, TPin>
        where TPin : class, ICircuitPin
    {
        bool Expanded { get; set; }

        /// <summary>
        /// Gets the subgraph's internal edges</summary>
        IEnumerable<TWire> Edges { get; }
    }
}
