//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapter for circuit modules; maintains local name and bounds for faster
    /// circuit rendering during editing operations, like dragging modules and wires.</summary>
    public class Module : Element
    {

        protected override AttributeInfo NameAttribute
        {
            get { return Schema.moduleType.nameAttribute; }
        }

        protected override AttributeInfo LabelAttribute
        {
            get { return Schema.moduleType.labelAttribute; }
        }

        protected override AttributeInfo XAttribute
        {
            get { return Schema.moduleType.xAttribute; }
        }

        protected override AttributeInfo YAttribute
        {
            get { return Schema.moduleType.yAttribute; }
        }

        protected override AttributeInfo VisibleAttribute
        {
            get { return Schema.moduleType.visibleAttribute; }
        }

        /// <summary>
        /// Gets and sets  original GUID of the template if this module is a copy-instance of</summary>
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
