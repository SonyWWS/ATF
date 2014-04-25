//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

using ModelViewerSample.Rendering;

namespace ModelViewerSample
{
    /// <summary>
    /// 3D model viewer. Imports available IResourceResolver objects and uses them to try to load
    /// and display model files.</summary>
    [Export(typeof(IDocumentClient))]    
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ModelViewer : IDocumentClient, IInitializable
    {
        /// <summary>
        /// Construct an instance of ModelViewer</summary>
        /// <param name="documentRegistry">The document registry used to clear old documents, because
        /// we don't enable the Close command and we don't have a multiple document interface</param>
        [ImportingConstructor]
        public ModelViewer(IDocumentRegistry documentRegistry)
        {
            string[] exts = { ".atgi", ".dae" };
            m_info = new DocumentClientInfo("3D Model", exts, null, null, false);
            m_documentRegistry = documentRegistry;
        }

        #region IInitializable Members

        /// <summary>
        /// Initializes the object by initializing DOM adapters for types</summary>
        void IInitializable.Initialize()
        {

            // Define DOM adapters for ATGI and Collada node types
            Register<RenderTransform>(Sce.Atf.Atgi.Schema.nodeType.Type);            
            Register<RenderPrimitives>(Sce.Atf.Atgi.Schema.vertexArray_primitives.Type);

            Register<RenderTransform>(Sce.Atf.Collada.Schema.node.Type);
            Register<RenderPrimitives>(Sce.Atf.Collada.Schema.polylist.Type);
            Register<RenderPrimitives>(Sce.Atf.Collada.Schema.triangles.Type);
            Register<RenderPrimitives>(Sce.Atf.Collada.Schema.trifans.Type);
            Register<RenderPrimitives>(Sce.Atf.Collada.Schema.tristrips.Type);

            if (m_scriptingService != null)
                m_scriptingService.SetVariable("viewer", this);
        }

        #endregion

        #region IDocumentClient Members

        /// <summary>
        /// Gets information about the document client, such as the file type and file
        /// extensions it supports, whether or not it allows multiple documents to be open,
        /// etc.</summary>
        DocumentClientInfo IDocumentClient.Info
        {
            get { return m_info; }
        }

        /// <summary>
        /// Returns whether the client can open or create a document at the given URI</summary>
        /// <param name="uri">Document URI</param>
        /// <returns>True iff the client can open or create a document at the given URI</returns>
        bool IDocumentClient.CanOpen(Uri uri)
        {
            return m_info.IsCompatibleUri(uri);
        }

        /// <summary>
        /// Opens or creates a document at the given URI</summary>
        /// <param name="uri">Document URI</param>
        /// <returns>Document, or null if the document couldn't be opened or created</returns>        
        IDocument IDocumentClient.Open(Uri uri)
        {
            foreach (IResourceResolver resolver in m_resolvers)
            {
                DomResource res = resolver.Resolve(uri) as DomResource;
                if (res != null)
                {
                    // Remove any previously opened documents. http://tracker.ship.scea.com/jira/browse/WWSATF-1422
                    var documents = new List<IDocument>(m_documentRegistry.Documents);
                    foreach (IDocument document in documents)
                        m_documentRegistry.Remove(document);

                    return new ModelDocument(res.DomNode, uri);
                }
            }

            return null;            
        }

        /// <summary>
        /// Makes the document visible to the user</summary>
        /// <param name="document">Document to show</param>
        /// <remarks>This application only shows one document at a time and always shows the last opened document</remarks>
        void IDocumentClient.Show(IDocument document)
        {
            
        }

        /// <summary>
        /// Saves the document at the given URI</summary>
        /// <param name="document">Document to save</param>
        /// <param name="uri">New document URI</param>
        /// <remarks>This application does not enable the Save command, because no modifications are allowed</remarks>
        void IDocumentClient.Save(IDocument document, Uri uri)
        {
            
        }

        /// <summary>
        /// Closes the document and removes any views of it from the UI</summary>
        /// <param name="document">Document to close</param>
        /// <remarks>This application does not enable the Close command. The last opened model is always displayed.</remarks>
        void IDocumentClient.Close(IDocument document)
        {
            
        }

        #endregion


        private static void Register<T>(DomNodeType nodeType) where T : new()
        {
            nodeType.Define(new ExtensionInfo<T>());
        }


        [ImportMany]
        private IEnumerable<IResourceResolver> m_resolvers;

        // scripting related members
        [Import(AllowDefault = true)]
        private ScriptingService m_scriptingService;

        private DocumentClientInfo m_info;
        private IDocumentRegistry m_documentRegistry;
    }
}
