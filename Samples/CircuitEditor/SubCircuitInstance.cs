//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapts DomNode to sub-circuit instance, which is an instance of a mastered SubCircuit</summary>
    [Obsolete("Circuit groups and circuit templates have replaced mastered circuits")]
    public class SubCircuitInstance : Sce.Atf.Controls.Adaptable.Graphs.SubCircuitInstance
    {
        /// <summary>
        /// Gets name attribute of Module</summary>
        protected override AttributeInfo NameAttribute
        {
            get { return Schema.moduleType.nameAttribute; }
        }

        /// <summary>
        /// Gets label attribute of Module</summary>
        protected override AttributeInfo LabelAttribute
        {
            get { return Schema.moduleType.labelAttribute; }
        }

        /// <summary>
        /// Gets x-coordinate position attribute of Module</summary>
        protected override AttributeInfo XAttribute
        {
            get { return Schema.moduleType.xAttribute; }
        }

        /// <summary>
        /// Gets y-coordinate position attribute of Module</summary>
        protected override AttributeInfo YAttribute
        {
            get { return Schema.moduleType.yAttribute; }
        }

        /// <summary>
        /// Gets visible attribute of Module</summary>
        protected override AttributeInfo VisibleAttribute
        {
            get { return Schema.moduleType.visibleAttribute; }
        }

        /// <summary>
        /// Gets type attribute of sub-circuit instance.
        /// A sub-circuit instance is a module whose type is defined by a sub-circuit (master).</summary>
        protected override AttributeInfo TypeAttribute
        {
            get { return Schema.subCircuitInstanceType.typeAttribute; }
        }
    }
}
