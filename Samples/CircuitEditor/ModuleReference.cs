//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using System;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapter for a reference instance of a module template</summary>
    public class ModuleReference : ElementReference, IReference<Module>
    {
        protected override AttributeInfo GuidRefAttribute
        {
            get { return Schema.moduleTemplateRefType.guidRefAttribute; }
        }

        #region IReference<Module>  memebers

        bool IReference<Module>.CanReference(Module item)
        {
            return true;
        }

        Module IReference<Module>.Target
        {
            get { return Template.Target.As<Module>(); }
            set
            {
                throw new InvalidOperationException("The group template determines the target");
            }
        }

        #endregion

        /// <summary>
        /// Gets the name attribute for module instance</summary>
        protected override AttributeInfo NameAttribute
        {
            get { return Schema.moduleType.nameAttribute; }
        }

        /// <summary>
        /// Gets the label attribute for module instance</summary>
        protected override AttributeInfo LabelAttribute
        {
            get { return Schema.moduleType.labelAttribute; }
        }

        /// <summary>
        /// Gets the x-coordinate position attribute for module instance</summary>
        protected override AttributeInfo XAttribute
        {
            get { return Schema.moduleType.xAttribute; }
        }

        /// <summary>
        /// Gets the y-coordinate position attribute for module instance</summary>
        protected override AttributeInfo YAttribute
        {
            get { return Schema.moduleType.yAttribute; }
        }

        /// <summary>
        /// Gets the visible attribute for module instance</summary>
        protected override AttributeInfo VisibleAttribute
        {
            get { return Schema.moduleType.visibleAttribute; }
        }
    }
}
