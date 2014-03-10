//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Dom
{
    public abstract class Template : DomNodeAdapter, IReference<DomNode>
    {
        /// <summary>
        /// Gets and sets the user-visible name of the template</summary>
        public abstract string Name { get; set; }

        /// <summary>
        /// Gets and sets DomNode model that represents the template</summary>
        public abstract DomNode Model { get; set; }

        /// <summary>Gets and sets  a globally unique identifier (GUID) that represents this template</summary>
        public abstract Guid Guid { get; set; }

        /// <summary>
        /// Returns true iff the template can reference the specified target item</summary>
        public abstract bool CanReference(DomNode item);
      
        public DomNode Target
        {
            get { return Model; }
            set { throw new InvalidOperationException("Target cannot be reattached.") ; }
        }
    }
}
