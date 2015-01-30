//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Atf.Collada
{
    /// <summary>
    /// This service resolves COLLADA resources</summary>
    [Export(typeof(ColladaResolver))]
    [Export(typeof(IInitializable))]
    [Export(typeof(IResourceResolver))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ColladaResolver : IInitializable, IResourceResolver
    {
     
        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by loading schema</summary>
        public void Initialize()
        {
            if (m_initialized)
                return;

            m_initialized = true;

            Assembly assembly = Assembly.GetExecutingAssembly();
            m_loader.SchemaResolver = new ResourceStreamResolver(assembly, "Sce.Atf.Collada/schemas");
            m_loader.Load("collada.xsd");
        }

        #endregion

        #region IResourceResolver Members

        /// <summary>
        /// Attempts to resolve (e.g., load from a file) the resource associated with the given URI</summary>
        /// <param name="uri">Resource URI</param>
        /// <returns>The resolved resource or null if there was a failure of some kind</returns>
        public IResource Resolve(Uri uri)
        {            
            string fileName = PathUtil.GetCanonicalPath(uri);
            if (!fileName.EndsWith(".dae"))
                return null;

            DomNode domNode = null;
            try
            {
                using (Stream stream = File.OpenRead(fileName))
                {
                    var persister = new ColladaXmlPersister(m_loader);
                    domNode = persister.Read(stream, uri);
                }
            }
            catch (IOException e)
            {
                Outputs.WriteLine(OutputMessageType.Warning, "Could not load resource: " + e.Message);
            }
            
            IResource resource = domNode.As<IResource>();
            if (resource != null)
                resource.Uri = uri;

            return resource;
        }

        #endregion

        private bool m_initialized;
        private readonly ColladaSchemaTypeLoader m_loader = new ColladaSchemaTypeLoader();
    }
}
