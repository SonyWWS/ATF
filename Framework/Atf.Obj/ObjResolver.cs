//Sony Computer Entertainment Confidential

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;

using Sce.Atf.Dom;

namespace Sce.Atf.Obj
{
    /// <summary>
    /// Resolves (e.g., loads from a file) resources from a URI</summary>
    [Export(typeof(ObjResolver))]
    [Export(typeof(IInitializable))]
    [Export(typeof(IResourceResolver))]
    public class ObjResolver : IInitializable, IResourceResolver
    {

        #region IInitializable Members

        public void Initialize()
        {
            if (Initialized)
                return;

            Initialized = true;

            Assembly assembly = Assembly.GetExecutingAssembly();
            m_loader.SchemaResolver = new ResourceStreamResolver(assembly, assembly.GetName().Name + "/schemas");
            m_loader.Load("obj.xsd");
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
            if (!fileName.EndsWith(".obj"))
                return null;

            DomNode domNode = null;
            try
            {
                using (Stream stream = File.OpenRead(fileName))
                {
                    ObjFile.PopulateDomNode(stream, ref domNode, uri);
                }
            }
            catch (IOException e)
            {
                Outputs.WriteLine(OutputMessageType.Warning, "Could not load resource: " + e.Message);
            }

            IResource resource = null;
            if (domNode != null)
            {
                resource = domNode.As<IResource>();
                if (resource != null)
                    resource.Uri = uri;
            }

            return resource;
        }

        #endregion

        private bool Initialized { get; set; }

        private readonly ObjSchemaTypeLoader m_loader = new ObjSchemaTypeLoader();
    }
}
