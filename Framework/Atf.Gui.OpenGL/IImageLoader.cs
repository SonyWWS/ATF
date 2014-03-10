//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.IO;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Interface for texture image loaders</summary>
    public interface IImageLoader
    {
        /// <summary>
        /// Loads texture image from stream</summary>
        /// <param name="imageStream">Stream holding texture image</param>
        /// <returns>Texture image</returns>
        Image LoadImage(Stream imageStream);
    }
}
