//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapts DomNode to circuit modules, which is the base circuit element with pins.
    /// It maintains local name and bounds for faster
    /// circuit rendering during editing operations, like dragging modules and wires.</summary>
    public class Module : Element
    {
        /// <summary>
        /// Gets name attribute for module</summary>
        protected override AttributeInfo NameAttribute
        {
            get { return Schema.moduleType.nameAttribute; }
        }

        /// <summary>
        /// Gets label attribute on module</summary>
        protected override AttributeInfo LabelAttribute
        {
            get { return Schema.moduleType.labelAttribute; }
        }

        /// <summary>
        /// Gets x-coordinate position attribute for module</summary>
        protected override AttributeInfo XAttribute
        {
            get { return Schema.moduleType.xAttribute; }
        }

        /// <summary>
        /// Gets y-coordinate position attribute for module</summary>
        protected override AttributeInfo YAttribute
        {
            get { return Schema.moduleType.yAttribute; }
        }

        /// <summary>
        /// Gets visible attribute for module</summary>
        protected override AttributeInfo VisibleAttribute
        {
            get { return Schema.moduleType.visibleAttribute; }
        }

        /// <summary>
        /// Gets the optional AttributeInfo for the original GUID of template 
        /// if this module is a copy-instance of a template(and nothing else) </summary>
        protected override AttributeInfo SourceGuidAttribute
        {
            get { return Schema.moduleType.sourceGuidAttribute; }
        }

        /// <summary>
        /// Gets the optional AttributeInfo for storing whether or not unconnected
        /// pins should be displayed.</summary>
        protected override AttributeInfo ShowUnconnectedPinsAttribute
        {
            get { return Schema.moduleType.showUnconnectedPinsAttribute; }
        }

        protected override CircuitElementInfo CreateElementInfo()
        {
            return new ModuleElementInfo();
        }
    }
}
