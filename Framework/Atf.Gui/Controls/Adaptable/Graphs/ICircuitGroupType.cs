//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Interface for hierarchical circuit group element types</summary>
    /// <typeparam name="TElement">Element, must implement ICircuitElement</typeparam>
    /// <typeparam name="TWire">Wire, must implement IGraphEdge</typeparam>
    /// <typeparam name="TPin">Pin, must implement ICircuitPin</typeparam>
    public interface ICircuitGroupType<TElement, TWire, TPin> :
        IHierarchicalGraphNode<TElement, TWire, TPin>, ICircuitElementType
        where TElement : class, IGraphNode
        where TWire : class, IGraphEdge<TElement, TPin>
        where TPin : class, IEdgeRoute
    {
        /// <summary>
        /// Gets or sets whether subgraph expanded</summary>
        bool Expanded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the hierarchical container is automatically resized to display its entire contents.</summary>
        bool AutoSize { get; set; }


        /// <summary>
        /// Gets the subgraph's internal edges</summary>
        IEnumerable<TWire> SubEdges { get; }

        /// <summary>
        /// Gets the CircuitGroupInfo object which controls various options on this circuit group</summary>
        CircuitGroupInfo Info
        {
            get;
        }

    }
}
