//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Interface for resolving resources to thumbnails</summary>
    public interface IThumbnailResolver
    {
        /// <summary>
        /// Resolves resource to a thumbnail image</summary>
        /// <param name="resourceUri">Resource URI to resolve</param>
        /// <returns>Thumbnail image</returns>
        object Resolve(Uri resourceUri);
    }
}
