//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for thumbnail resolvers</summary>
    public interface IThumbnailResolver
    {
        /// <summary>
        /// Resolves resource to a thumbnail image</summary>
        /// <param name="resourceUri">Resource URI to resolve</param>
        /// <returns>Thumbnail image</returns>
        Image Resolve(Uri resourceUri);
    }
}
