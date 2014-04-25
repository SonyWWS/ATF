//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Thumbnail resolver for image resources</summary>
    [Export(typeof(ImageThumbnailResolver))]
    [Export(typeof(IThumbnailResolver))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ImageThumbnailResolver : IThumbnailResolver, IInitializable
    {
        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component</summary>
        public void Initialize()
        {
        }

        #endregion
        
        /// <summary>
        /// Resolves Resource to a thumbnail image</summary>
        /// <param name="resourceUri">Resource URI to resolve</param>
        /// <returns>Thumbnail image</returns>
        public Image Resolve(Uri resourceUri)
        {

            string path = ThumbnailService.GetResourcePath(resourceUri);
            if (path == null)
                return null;

            Bitmap bitmap = null;
            string extension = Path.GetExtension(path);
            if (extension.EndsWith("jpg", StringComparison.InvariantCultureIgnoreCase) ||
                extension.EndsWith("bmp", StringComparison.InvariantCultureIgnoreCase) ||
                extension.EndsWith("png", StringComparison.InvariantCultureIgnoreCase) ||
                extension.EndsWith("tif", StringComparison.InvariantCultureIgnoreCase) ||
                extension.EndsWith("gif", StringComparison.InvariantCultureIgnoreCase))
            {
                using (FileStream strm = File.OpenRead(path))
                {
                    bitmap = new Bitmap(strm);
                    strm.Close();
                }
            }

            return bitmap;
        }
    }
}
