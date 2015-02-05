//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapts DomNode to sub-circuit, which is created when mastering</summary>
    [Obsolete("Circuit groups and circuit templates have replaced mastered circuits")]
    class SubCircuit : Sce.Atf.Controls.Adaptable.Graphs.SubCircuit
    {
        /// <summary>
        /// Gets ChildInfo for modules in subcircuit</summary>
        protected override ChildInfo ElementChildInfo
        {
            get { return Schema.circuitType.moduleChild; }
        }

        /// <summary>
        /// Gets ChildInfo for connections (wires) in subcircuit</summary>
        protected override ChildInfo WireChildInfo
        {
            get { return Schema.circuitType.connectionChild; }
        }

        /// <summary>
        /// Gets name attribute for subcircuit</summary>
        protected override AttributeInfo NameAttribute
        {
            get { return Schema.subCircuitType.nameAttribute; }
        }

        /// <summary>
        /// Gets ChildInfo for input pins in subcircuit</summary>
        protected override ChildInfo InputChildInfo
        {
            get { return Schema.subCircuitType.inputChild; }
        }

        /// <summary>
        /// Gets ChildInfo for output pins in subcircuit</summary>
        protected override ChildInfo OutputChildInfo
        {
            get { return Schema.subCircuitType.outputChild; }
        }
    }
}
