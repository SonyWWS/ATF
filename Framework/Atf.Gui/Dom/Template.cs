//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Adapts DomNode to a template, which is is a module that can be referenced into a circuit</summary>
    public abstract class Template : DomNodeAdapter, IReference<DomNode>
    {
        /// <summary>
        /// Gets or sets the user-visible name of the template</summary>
        public abstract string Name { get; set; }

        /// <summary>
        /// Gets or sets DomNode module that represents the template</summary>
        public abstract DomNode Model { get; set; }

        /// <summary>Gets or sets a globally unique identifier (GUID) that represents this template</summary>
        public abstract Guid Guid { get; set; }

        /// <summary>
        /// Returns true iff the template can reference the specified target item</summary>
        public abstract bool CanReference(DomNode item);
      
        /// <summary>
        /// Gets DomNode module that represents the template</summary>
        public DomNode Target
        {
            get { return Model; }
            set { throw new InvalidOperationException("Target cannot be reattached.") ; }
        }
    }
}
