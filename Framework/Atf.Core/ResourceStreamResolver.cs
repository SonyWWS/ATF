//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Reflection;
using System.Xml;

namespace Sce.Atf
{
    /// <summary>
    /// XML resolver that resolves resource names to streams, based on an assembly and
    /// resource namespace</summary>
    public class ResourceStreamResolver : XmlUrlResolver
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="assembly">Assembly containing the embedded resource</param>
        /// <param name="resourceNamespace">Namespace containing resources. Visual Studio uses
        /// the default namespace in the project file when embedding resources. For instance, 
        /// the string for the CircuitEditor sample would be "CircuitEditorSample".</param>
        public ResourceStreamResolver(Assembly assembly, string resourceNamespace)
        {
            m_assembly = assembly;
            m_rootPath = new Uri(Uri.UriSchemeFile + ":///" + resourceNamespace + "/");
        }

        /// <summary>
        /// Maps a URI to an object containing the actual resource</summary>
        /// <param name="absoluteUri">The URI returned from XmlResolver.ResolveUri(Uri,String)</param>
        /// <param name="role">Not used</param>
        /// <param name="returnType">Must be System.IO.Stream</param>
        /// <returns>A System.IO.Stream object or null if a type other than stream is specified</returns>
        public override object GetEntity(Uri absoluteUri, string role, Type returnType)
        {
            if (absoluteUri.IsFile)
            {
                string newFileName = absoluteUri.AbsolutePath.Replace('/', '.');
                newFileName = newFileName.Substring(1, newFileName.Length - 1); // remove leading "."
                return m_assembly.GetManifestResourceStream(newFileName);
            }
            return base.GetEntity(absoluteUri, role, returnType);
        }

        /// <summary>
        /// Resolves the absolute URI from the base and relative URIs</summary>
        /// <param name="baseUri">Unused</param>
        /// <param name="relativeUri">The URI to resolve. The URI can be absolute or relative. 
        /// If absolute, relativeUri effectively replaces the baseUri value. 
        /// If relative, it combines relativeUri with the root path to make an absolute URI.</param>
        /// <returns>A <see cref="T:System.Uri"></see> representing the absolute URI or null 
        /// if the relative URI cannot be resolved</returns>
        /// <exception cref="T:System.ArgumentException">relativeUri is null</exception>
        public override Uri ResolveUri(Uri baseUri, string relativeUri)
        {
            return new Uri(m_rootPath, relativeUri);
        }

        private readonly Assembly m_assembly;
        private readonly Uri m_rootPath;
    }
}
