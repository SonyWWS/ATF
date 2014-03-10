//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf
{
    /// <summary>
    /// Interface for references to resources. The Uri property is added to facilitate resolving
    /// and unresolving the target resource.</summary>
    public interface IResourceReference : IReference<IResource>
    {
        /// <summary>
        /// Gets or sets the URI of the target resource</summary>
        /// <remarks>It is up to the implementer to decide whether changing the URI
        /// immediately updates the target resource. Clients should check the documentation
        /// (or code) of the implementing class to find out.</remarks>
        Uri Uri { get; set; }
    }
}
