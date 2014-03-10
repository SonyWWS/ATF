//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Atf.Atgi
{
    /// <summary>
    /// Resource resolver for ATGI files. This class should be multi-thread safe after initialization has
    /// occurred.</summary>
    /// <remarks>The thumbnail generator uses this class on a separate thread, so the Resolve() method needs
    /// to be multi-thread safe.</remarks>
    [Export(typeof(AtgiResolver))]
    [Export(typeof(IInitializable))]
    [Export(typeof(IResourceResolver))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AtgiResolver : IInitializable, IResourceResolver
    {
        #region IInitializable Members

        public void Initialize()
        {
            if (m_initialized)
                return;

            m_initialized = true;

            Assembly assembly = Assembly.GetExecutingAssembly();
            m_loader.SchemaResolver = new ResourceStreamResolver(assembly, assembly.GetName().Name + "/schemas");
            m_loader.Load("atgi.xsd");
        }

        #endregion

        #region IResourceResolver Members

        /// <summary>
        /// Attempts to resolve (e.g., load from a file) the resource associated with the given URI</summary>
        /// <param name="uri">Resource URI</param>
        /// <returns>The resolved resource or null if there was a failure of some kind</returns>
        public IResource Resolve(Uri uri)
        {
            DomNode domNode = null;
            try
            {
                string fileName;
                if (uri.IsAbsoluteUri)
                    fileName = PathUtil.GetCanonicalPath(uri);
                else
                    fileName = uri.OriginalString;

                if (!fileName.EndsWith(".atgi"))
                    return null; // unable to resolve this asset

                using (Stream stream = File.OpenRead(fileName))
                {
                    if (stream != null)
                    {
                        var persister = new AtgiXmlPersister(m_loader);
                        domNode = persister.Read(stream, uri);
                    }
                }
            }
            catch (System.IO.IOException e)
            {
                Outputs.WriteLine(OutputMessageType.Warning, "Could not load resource: " + e.Message);
            }

            IResource resource = Adapters.As<IResource>(domNode);
            if (resource != null)
                resource.Uri = uri;

            return resource;
        }

        #endregion

        private bool m_initialized;
        private AtgiSchemaTypeLoader m_loader = new AtgiSchemaTypeLoader();
    }
}
