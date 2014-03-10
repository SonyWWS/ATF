//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.IO;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.CurveEditing;
using Sce.Atf.Dom;


namespace DomTreeEditorSample
{
    /// <summary>
    /// DOM tree editor, responsible for loading and saving UI documents. The editor
    /// can only open one document at a time.</summary>
    [Export(typeof(IDocumentClient))]
    [Export(typeof(Editor))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Editor : IDocumentClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="contextRegistry">Context registry</param>
        /// <param name="documentRegistry">Document registry</param>
        /// <param name="treeLister">Tree lister, which displays a tree view of UI data</param>
        /// <param name="schemaLoader">Schema loader</param>
        [ImportingConstructor]
        public Editor(
            IContextRegistry contextRegistry,
            IDocumentRegistry documentRegistry,
            TreeLister treeLister,
            SchemaLoader schemaLoader)
        {
            m_contextRegistry = contextRegistry;
            m_documentRegistry = documentRegistry;
            m_documentRegistry.ActiveDocumentChanged += new EventHandler(documentRegistry_ActiveDocumentChanged);
            m_schemaLoader = schemaLoader;
            m_treeLister = treeLister;
        }

        [Import(AllowDefault = true)]
        private DomExplorer m_domExplorer = null;

        // scripting related members
        [Import(AllowDefault = true)]
        private ScriptingService m_scriptingService = null;

        #region IInitializable
        
        void IInitializable.Initialize()
        {
            if (m_scriptingService != null)
            {
                // load this assembly into script domain.
                m_scriptingService.LoadAssembly(GetType().Assembly);
                m_scriptingService.ImportAllTypes("DomTreeEditorSample");
                m_scriptingService.ImportAllTypes("Sce.Atf.Controls.Adaptable.Graphs");
                m_scriptingService.SetVariable("editor", this);
                m_scriptingService.SetVariable("treeLister", m_treeLister);

                m_contextRegistry.ActiveContextChanged += delegate
                {
                    EditingContext editingContext = m_contextRegistry.GetActiveContext<EditingContext>();
                    IHistoryContext hist = m_contextRegistry.GetActiveContext<IHistoryContext>();
                    m_scriptingService.SetVariable("editingContext", editingContext);
                    m_scriptingService.SetVariable("hist", hist);
                };
            }

            if (m_curveEditor != null)
            {
                Curve.EnforceCurveLimits = true;
                m_curveEditor.Control.AutoComputeCurveLimitsEnabled = false;
                m_curveEditor.Control.CurvesChanged += (sender, e) =>
                {
                    m_curveEditor.Control.FitAll();
                };            
            }
        }

        #endregion

        #region IDocumentClient Members

        /// <summary>
        /// Gets information about the document client, such as the file type and file
        /// extensions it supports, whether or not it allows multiple documents to be open, etc.</summary>
        public DocumentClientInfo Info
        {
            get { return s_info; }
        }

        /// <summary>
        /// Returns whether the client can open or create a document at the given URI</summary>
        /// <param name="uri">Document URI</param>
        /// <returns>True iff the client can open or create a document at the given URI</returns>
        public bool CanOpen(Uri uri)
        {
            return s_info.IsCompatibleUri(uri);
        }

        /// <summary>
        /// Info describing our document type</summary>
        private static DocumentClientInfo s_info =
            new DocumentClientInfo(
                Localizer.Localize("UI"),   // file type
                ".uif",                      // file extension
                null,                       // "new document" icon
                null);                      // "open document" icon

        /// <summary>
        /// Opens or creates a document at the given URI</summary>
        /// <param name="uri">Document URI</param>
        /// <returns>Document, or null if the document couldn't be opened or created</returns>
        public IDocument Open(Uri uri)
        {
            DomNode node = null;
            string filePath = uri.LocalPath;

            if (File.Exists(filePath))
            {
                // read existing document using standard XML reader
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    DomXmlReader reader = new DomXmlReader(m_schemaLoader);
                    node = reader.Read(stream, uri);
                }
            }
            else
            {
                // create new document by creating a Dom node of the root type defined by the schema
                node = new DomNode(UISchema.UIType.Type, UISchema.UIRootElement);
                UI ui = node.As<UI>();
                ui.Name = "UI";
            }

            Document document = null;
            if (node != null)
            {
                // Initialize Dom extensions now that the data is complete; after this, all Dom node
                //  adapters will have been bound to their underlying Dom node.
                node.InitializeExtensions();

                // get the root node's UIDocument adapter
                document = node.As<Document>();
                document.Uri = uri;

                // only allow 1 open document at a time
                Document activeDocument = m_documentRegistry.GetActiveDocument<Document>();
                if (activeDocument != null)
                    Close(activeDocument);
            }

            return document;
        }

        /// <summary>
        /// Makes the document visible to the user</summary>
        /// <param name="document">Document to show</param>
        public void Show(IDocument document)
        {
            // set the active document and context; as there is only one editing context in
            //  a document, the document is also a context.
            m_contextRegistry.ActiveContext = document;
            m_documentRegistry.ActiveDocument = document;
        }

        /// <summary>
        /// Saves the document at the given URI</summary>
        /// <param name="document">Document to save</param>
        /// <param name="uri">New document URI</param>
        public void Save(IDocument document, Uri uri)
        {
            Document doc = document as Document;
            string filePath = uri.LocalPath;
            FileMode fileMode = File.Exists(filePath) ? FileMode.Truncate : FileMode.OpenOrCreate;
            using (FileStream stream = new FileStream(filePath, fileMode))
            {
                DomXmlWriter writer = new DomXmlWriter(m_schemaLoader.TypeCollection);
                writer.Write(doc.DomNode, stream, uri);
            }
        }

        /// <summary>
        /// Closes the document and removes any views of it from the UI</summary>
        /// <param name="document">Document to close</param>
        public void Close(IDocument document)
        {
            m_contextRegistry.RemoveContext(document);
            m_documentRegistry.Remove(document);
        }

        #endregion

        private void documentRegistry_ActiveDocumentChanged(object sender, EventArgs e)
        {
            if (m_domExplorer != null)
                m_domExplorer.Root = m_documentRegistry.GetActiveDocument<DomNode>();
        }

        private IContextRegistry m_contextRegistry;
        private IDocumentRegistry m_documentRegistry;
        private TreeLister m_treeLister;
        private SchemaLoader m_schemaLoader;

        [Import(AllowDefault = true)]
        private CurveEditor m_curveEditor = null;
    }
}
