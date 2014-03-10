//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for a source control context</summary>
    /// <remarks>Users of this interface can use the IResource to examine URIs and adapt
    /// the IResource to IDocument to track a document's dirty flag. See
    /// Sce.Atf.Applications.SourceControlCommands.</remarks>
    public interface ISourceControlContext
    {
        /// <summary>
        /// Gets an enumeration of the resources under source control</summary>
        /// <remarks>Implementors, consider allowing IResource to implement or be adaptable
        /// to IDocument</remarks>
        IEnumerable<IResource> Resources
        {
            get;
        }
    }
}
