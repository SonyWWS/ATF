//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Xml;

namespace Sce.Atf
{
    /// <summary>
    /// XML resolver that resolves file names to streams, based on a root directory
    /// path</summary>
    public class FileStreamResolver : XmlUrlResolver
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="rootPath">Path to folder containing files</param>
        public FileStreamResolver(string rootPath)
        {
            m_rootPath = new Uri(Uri.UriSchemeFile + ":///" + rootPath + "/");
        }

        /// <summary>
        /// Resolves the absolute URI from the base and relative URIs</summary>
        /// <param name="baseUri">The base URI used to resolve the relative URI</param>
        /// <param name="relativeUri">The URI to resolve. The URI can be absolute or relative. 
        /// If absolute, this value effectively replaces the baseUri value. 
        /// If relative, it combines with the baseUri to make an absolute URI.</param>
        /// <returns>
        /// A <see cref="T:System.Uri"></see> representing the absolute URI or null if the relative URI cannot be resolved</returns>
        /// <exception cref="T:System.ArgumentException"> relativeUri is null</exception>
        public override Uri ResolveUri(Uri baseUri, string relativeUri)
        {
            Uri absoluteUri = new Uri(m_rootPath, relativeUri);
            if (absoluteUri.IsFile)
            {
                string fileName = PathUtil.GetCulturePath(absoluteUri.LocalPath);
                absoluteUri = new Uri(fileName);
            }
            return absoluteUri;
        }

        private readonly Uri m_rootPath;
    }
}
