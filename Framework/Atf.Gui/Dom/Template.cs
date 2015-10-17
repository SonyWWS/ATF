//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Represents a type of template that appears in a special editor like the TemplateLister.
    /// A template references a particular instance of data (the Target property), while adding
    /// additional information about that reference (the Name and Guid properties).</summary>
    /// <remarks>Templates have been used in circuits, but are not circuit-specific. A template
    /// type might be used to create a template instance by dragging and dropping.</remarks>
    public abstract class Template : DomNodeAdapter, IReference<DomNode>
    {
        /// <summary>
        /// Gets or sets the user-visible name of the template</summary>
        public abstract string Name { get; set; }

        /// <summary>
        /// Gets or sets a globally unique identifier (GUID) that represents this template</summary>
        public abstract Guid Guid { get; set; }

        #region IReference members

        /// <summary>
        /// Returns <c>True</c> if the template can reference the specified target item</summary>
        /// <param name="item">Target item</param>
        /// <returns><c>True</c> if template can reference specified target item</returns>
        public abstract bool CanReference(DomNode item);

        /// <summary>
        /// Gets or sets the DomNode that is the referenced data. Only set if CanReference()
        /// returns true.</summary>
        public abstract DomNode Target { get; set; }

        #endregion
      }
}
