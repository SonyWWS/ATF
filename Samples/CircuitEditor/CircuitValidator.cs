//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapter that tracks changes to transitions and updates their routing during validation.
    /// Update transitions on Ending event are part of the transactions themselves, 
    /// then validate all sub-graphs in the current document on Ended event. Requires
    /// Sce.Atf.Dom.ReferenceValidator to be available on the adapted DomNode.</summary>
    class CircuitValidator : Sce.Atf.Controls.Adaptable.Graphs.CircuitValidator
    {
        /// <summary>
        /// Gets module label attribute</summary>
        protected override AttributeInfo ElementLabelAttribute
        {
            get { return Schema.moduleType.labelAttribute; }
        }

        /// <summary>
        /// Gets pin name attribute</summary>
        protected override AttributeInfo PinNameAttributeAttribute
        {
            get { return Schema.pinType.nameAttribute; }
        }
    }
}
