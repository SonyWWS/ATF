//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    class CircuitValidator : Sce.Atf.Controls.Adaptable.Graphs.CircuitValidator
    {
        protected override AttributeInfo ElementLabelAttribute
        {
            get { return Schema.moduleType.labelAttribute; }
        }

        protected override AttributeInfo PinNameAttributeAttribute
        {
            get { return Schema.pinType.nameAttribute; }
        }
    }
}
