//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    class SubCircuit : Sce.Atf.Controls.Adaptable.Graphs.SubCircuit
    {
        protected override ChildInfo ElementChildInfo
        {
            get { return Schema.circuitType.moduleChild; }
        }

        protected override ChildInfo WireChildInfo
        {
            get { return Schema.circuitType.connectionChild; }
        }

        protected override AttributeInfo NameAttribute
        {
            get { return Schema.subCircuitType.nameAttribute; }
        }

        protected override ChildInfo InputChildInfo
        {
            get { return Schema.subCircuitType.inputChild; }
        }

        protected override ChildInfo OutputChildInfo
        {
            get { return Schema.subCircuitType.outputChild; }
        }
    }
}
