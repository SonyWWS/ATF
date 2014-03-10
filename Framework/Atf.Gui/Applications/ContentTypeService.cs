//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Service that provides a mapping from URI extension to content type string</summary>
    [Export(typeof(ContentTypeService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ContentTypeService
    {
        /// <summary>
        /// Constructor</summary>
        public ContentTypeService()
        {
            m_extensionMap = new Dictionary<string, string>();
        }

        /// <summary>
        /// Sets the content type for an extension</summary>
        /// <param name="extension">Extension</param>
        /// <param name="type">Content type for extension</param>
        public void SetContentType(string extension, string type)
        {
            if (!m_extensionMap.ContainsKey(extension))
                m_extensionMap.Add(extension, type);
        }

        /// <summary>
        /// Gets the content type of the extension</summary>
        /// <param name="extension">Extension of content</param>
        /// <returns>Content type of the extension</returns>
        public string GetContentType(string extension)
        {
            extension = extension.TrimStart('.');
            string result;
            m_extensionMap.TryGetValue(extension, out result);
            return result;
        }

        /// <summary>
        /// Gets the content type of the URI</summary>
        /// <param name="uri">URI of content</param>
        /// <returns>Content type of the URI</returns>
        public string GetContentType(Uri uri)
        {
            string extension = Path.GetExtension(uri.LocalPath);
            return GetContentType(extension);
        }

        /// <summary>
        /// Gets all extensions registered with the service</summary>
        /// <returns>All extensions registered with the service</returns>
        public IEnumerable<string> GetAllExtensions()
        {
            return m_extensionMap.Keys;
        }

        /// <summary>
        /// Gets all extensions for the given content type</summary>
        /// <param name="type">Content type</param>
        /// <returns>All extensions for the given content type</returns>
        public ICollection<string> GetAllExtensions(string type)
        {
            List<string> result = new List<string>();
            foreach (KeyValuePair<string, string> pair in m_extensionMap)
                if (pair.Value == type)
                    result.Add(pair.Key);

            return result;
        }

        private readonly Dictionary<string, string> m_extensionMap;
    }
}
