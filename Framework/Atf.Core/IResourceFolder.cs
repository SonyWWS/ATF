//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf
{
    /// <summary>
    /// Folder containing a hierarchical structure of subfolders and resource URIs</summary>
    /// <remarks>Replaces ATF 2's IAssetFolder interface. Main difference: The new IResourceFolder
    /// only contains URIs rather than actual resources. All resolved resources are managed by a
    /// registered IResourceService MEF component.</remarks>
    public interface IResourceFolder
    {
        /// <summary>
        /// Gets a list of subfolders, if any. If there are no subfolders, an empty list is returned.
        /// Use Folders.Add (or similar) to add additional subfolders unless IList.IsReadOnly
        /// is true.</summary>
        IList<IResourceFolder> Folders { get; }

        /// <summary>
        /// Gets a list of Resource URIs contained in this folder. If none, an empty list is returned.
        /// Use ResourceUris.Add (or similar) to add additional URIs unless IList.IsReadOnly
        /// is true.</summary>
        IList<Uri> ResourceUris { get; }

        /// <summary>
        /// Gets the parent folder. Returns null if the current folder is the root of a folder tree.</summary>
        IResourceFolder Parent { get; }

        /// <summary>
        /// Gets whether the Name property is read-only. If true, setting the Name may not have any
        /// effect.</summary>
        bool ReadOnlyName { get; }

        /// <summary>
        /// Get or set the name of the folder</summary>
        string Name { get; set; }

        /// <summary>
        /// Creates a new IResourceFolder instance, but does not add it to the list of folders</summary>
        /// <returns>A new IResourceFolder instance or null if, for example, this folder is read-only
        /// and wouldn't be able to accept any new IResourceFolder objects</returns>
        IResourceFolder CreateFolder();
    }
}
