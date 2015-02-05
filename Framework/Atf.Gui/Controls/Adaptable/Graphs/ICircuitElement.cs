//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Interface for circuit elements, which have a type defining their pins and appearance</summary>
    public interface ICircuitElement : IGraphNode
    {
        /// <summary>
        /// Gets information describing the circuit element's appearance, inputs, and
        /// outputs</summary>
        ICircuitElementType Type
        {
            get;
        }

        /// <summary>
        /// Gets additional information and options about this circuit element</summary>
        /// <value>
        /// The element information</value>
        CircuitElementInfo ElementInfo { get; }
    }
}
