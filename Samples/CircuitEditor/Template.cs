﻿//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapts DomNode to a template, which is is a module that can be referenced into a circuit</summary>
    public class Template : Sce.Atf.Dom.Template
    {
        /// <summary>
        /// Gets and sets the user-visible name of the template</summary>
        public override string Name
        {
            get { return (string)DomNode.GetAttribute(Schema.templateType.labelAttribute); }
            set { DomNode.SetAttribute(Schema.templateType.labelAttribute, value); }
        }

        /// <summary>
        /// Gets or sets DomNode module that represents the template</summary>
        public override DomNode Target
        {
            get { return GetChild<DomNode>(Schema.templateType.moduleChild); }
            set
            {
                SetChild(Schema.templateType.moduleChild, value);
                if (value != null) // initialize  model name
                {
                    var module = Target.Cast<Module>();
                    Name = module.Name;
                    if (string.IsNullOrEmpty(Name))
                        Name = module.Type.Name;

                }
            }
        }

        /// <summary>Gets and sets  a globally unique identifier (GUID) that represents this template</summary>
        public override Guid Guid
        {
            get { return new Guid((string)DomNode.GetAttribute(Schema.templateType.guidAttribute)); }
            set { DomNode.SetAttribute(Schema.templateType.guidAttribute, value.ToString()); }
        }

        /// <summary>
        /// Returns <c>True</c> if the template can reference the specified target item</summary>
        public override bool CanReference(DomNode item)
        {
            return item.Is<Module>();
        }
    }
}
