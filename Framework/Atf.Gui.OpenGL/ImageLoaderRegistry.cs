//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// A registry that maps image file extensions to IImageLoaders.  
    /// Once initialized, the registry can be queried for the appropriate object to load a given
    /// image file from disk.</summary>
    public class ImageLoaderRegistry
    {
        /// <summary>
        /// Default constructor, registering what is hopefully every image file extension you could ever want</summary>
        public ImageLoaderRegistry()
        {
            RegisterImageLoader(".tga", new TargaImageLoader());

            RegisterImageLoader(".bmp", new MsBitmapImageLoader());
            RegisterImageLoader(".gif", new MsBitmapImageLoader());
            RegisterImageLoader(".png", new MsBitmapImageLoader());
            RegisterImageLoader(".jpg", new MsBitmapImageLoader());
            RegisterImageLoader(".jpeg", new MsBitmapImageLoader());
            RegisterImageLoader(".tiff", new MsBitmapImageLoader());
            RegisterImageLoader(".tif", new MsBitmapImageLoader());

            RegisterImageLoader(".dds", new DdsImageLoader());
        }

        /// <summary>
        /// Registers and associates the supplied image loader with the given file extension.
        /// Throws exception if the current file extension is already registered.</summary>
        /// <param name="fileExtension">File extension (with leading ".") to register</param>
        /// <param name="loader">IImageLoader object to associate with fileExtension</param>
        /// <exception cref="InvalidOperationException">Loader is already registered with the extension</exception>
        public void RegisterImageLoader(string fileExtension, IImageLoader loader)
        {
            string upperCase= fileExtension.ToUpperInvariant();
            if (m_imageLoaders.ContainsKey(upperCase))
                throw new InvalidOperationException(
                    "Loader is already registered under extension '" + fileExtension + "'");

            m_imageLoaders.Add(upperCase, loader);
        }

        /// <summary>
        /// Unregisters the image loader (if any) associated with the given file extension.
        /// Does not throw an exception if there is no matching loader registered.</summary>
        /// <param name="fileExtension">File extension (with leading ".") to register</param>
        public void UnregisterImageLoader(string fileExtension)
        {
            m_imageLoaders.Remove(fileExtension.ToUpperInvariant());
        }

        /// <summary>
        /// Retrieves the IImageLoader associated with the supplied file extension or null if
        /// not found. Does not throw an exception if there is no matching loader registered.</summary>
        /// <param name="imageExtension">File extension (with leading ".") to retrieve loader by</param>
        /// <returns>Associated IImageLoader or null if not found</returns>
        public IImageLoader TryGetLoader(string imageExtension)
        {
            IImageLoader loader;
            m_imageLoaders.TryGetValue(imageExtension.ToUpperInvariant(), out loader);
            return loader;
        }

        /// <summary>
        /// Retrieves the IImageLoader associated with the supplied file extension.
        /// Throws exception if not found.</summary>
        /// <param name="imageExtension">File extension (with leading ".") to retrieve loader by</param>
        /// <returns>Associated IImageLoader</returns>
        /// <exception cref="KeyNotFoundException">No loader has been registered with extension</exception>
        public IImageLoader GetLoader(string imageExtension)
        {
            IImageLoader loader;
            if (m_imageLoaders.TryGetValue(imageExtension.ToUpperInvariant(), out loader) == false)
                throw new KeyNotFoundException("No loader has been registered with extension '" +
                                               imageExtension + "'");

            return loader;
        }

        private readonly Dictionary<string, IImageLoader> m_imageLoaders = new Dictionary<string, IImageLoader>();
    }
}
