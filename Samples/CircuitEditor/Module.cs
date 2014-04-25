//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
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
        /// Gets or sets original GUID of template if this module is a copy-instance of a template</summary>
        public Guid SourceGuid
        {
            get
            {
                var guidValue = DomNode.GetAttribute(Schema.moduleType.sourceGuidAttribute) as string;
                if (string.IsNullOrEmpty(guidValue))
                    return Guid.Empty;
                return new Guid(guidValue);
            }
            set
            {
                DomNode.SetAttribute(Schema.moduleType.sourceGuidAttribute, value.ToString());
            }
        }
    }
}
