//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.IO;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.Wpf.Applications;
using Sce.Atf.Wpf.Docking;

using IControlHostClient = Sce.Atf.Wpf.Applications.IControlHostClient;
using IControlHostService = Sce.Atf.Wpf.Applications.IControlHostService;
using ControlInfo = Sce.Atf.Wpf.Applications.ControlInfo;

namespace SimpleDomEditorWpfSample
{
    /// <summary>
    /// Editor class that creates and saves event sequence documents. It also registers
    /// the event sequence view controls with the hosting service.
    /// This document client handles file operations, such as saving and closing a document, and
    /// handles application data persistence.</summary>
    [Export(typeof(IDocumentClient))]
    [Export(typeof(IInitializable))]
    [Export(typeof(Editor))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Editor : IDocumentClient, IControlHostClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="controlHostService">Control host service</param>
        /// <param name="documentService">Document service</param>
        /// <param name="contextRegistry">Context registry</param>
        /// <param name="documentRegistry">Document registry</param>
        /// <param name="schemaLoader">Schema loader</param>
        [ImportingConstructor]
        public Editor(
            IControlHostService controlHostService,
            IDocumentService documentService,
            IContextRegistry contextRegistry,
            IDocumentRegistry documentRegistry,
            SchemaLoader schemaLoader)
        {
            m_controlHostService = controlHostService;
            m_documentService = documentService;
            m_contextRegistry = contextRegistry;
            m_documentRegistry = documentRegistry;
            m_schemaLoader = schemaLoader;
        }

        #region IInitializable Members

        /// <summary>
        /// Completes initialization</summary>
        void IInitializable.Initialize()
        {
            // Set the application icon. We need to convert the resource from
            // System.Drawing.Image to System.Windows.Media.ImageSource.
            System.Drawing.Image atfIcon = Sce.Atf.ResourceUtil.GetImage(Sce.Atf.Resources.AtfIconImage);
            System.Windows.Application.Current.MainWindow.Icon = Sce.Atf.Wpf.ResourceUtil.ConvertWinFormsImage(atfIcon);
        }
        
        #endregion

        #region IDocumentClient Members

        /// <summary>
        /// Gets information about the document client, such as the file type and file
        /// extensions it supports, whether or not it allows multiple documents to be open, etc.</summary>
        public DocumentClientInfo Info
        {
            get { return DocumentClientInfo; }
        }

        /// <summary>
        /// Information about the document client</summary>
        public static DocumentClientInfo DocumentClientInfo = new DocumentClientInfo(
            "Event Sequence".Localize(),
            new string[] { ".xml", ".esq" },
            Sce.Atf.Resources.DocumentImage,
            Sce.Atf.Resources.FolderImage,
            true);

        /// <summary>
        /// Returns whether the client can open or create a document at the given URI</summary>
        /// <param name="uri">Document URI</param>
        /// <returns><c>True</c> if the client can open or create a document at the given URI</returns>
        public bool CanOpen(Uri uri)
        {
            return DocumentClientInfo.IsCompatibleUri(uri);
        }

        /// <summary>
        /// Opens or creates a document at the given URI</summary>
        /// <param name="uri">Document URI</param>
        /// <returns>Document, or null if the document couldn't be opened or created</returns>
        public IDocument Open(Uri uri)
        {
            DomNode node = null;
            string filePath = uri.LocalPath;
            string fileName = Path.GetFileName(filePath);

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
                node = new DomNode(Schema.eventSequenceType.Type, Schema.eventSequenceRootElement);
            }

            EventSequenceDocument document = null;
            if (node != null)
            {
                // Initialize Dom extensions now that the data is complete
                node.InitializeExtensions();

                EventSequenceContext context = node.As<EventSequenceContext>();

                ControlInfo controlInfo = new ControlInfo(Path.Combine(filePath, fileName),
                    StandardControlGroup.Center,
                    new DockContent(null, null), this);
                context.ControlInfo = controlInfo;

                // set document URI
                document = node.As<EventSequenceDocument>();
                document.Uri = uri;

                context.Document = document;

                // show the document editor
                // This line requires references to System.Drawing and System.Windows.Forms. Would really like to remove those dependencies!
                m_controlHostService.RegisterControl(context.View, 
                    fileName, 
                    "Event sequence document", 
                    StandardControlGroup.Center, 
                    Path.Combine(filePath, fileName), 
                    this);
            }

            return document;
        }
        
        /// <summary>
        /// Makes the document visible to the user</summary>
        /// <param name="document">Document to show</param>
        public void Show(IDocument document)
        {
            EventSequenceContext context = document.As<EventSequenceContext>();
            m_controlHostService.Show(context.View);
        }

        /// <summary>
        /// Saves the document at the given URI</summary>
        /// <param name="document">Document to save</param>
        /// <param name="uri">New document URI</param>
        public void Save(IDocument document, Uri uri)
        {
            string filePath = uri.LocalPath;
            FileMode fileMode = File.Exists(filePath) ? FileMode.Truncate : FileMode.OpenOrCreate;
            using (FileStream stream = new FileStream(filePath, fileMode))
            {
                DomXmlWriter writer = new DomXmlWriter(m_schemaLoader.TypeCollection);
                EventSequenceDocument eventSequenceDocument = (EventSequenceDocument)document;
                writer.Write(eventSequenceDocument.DomNode, stream, uri);
            }
        }

        /// <summary>
        /// Closes the document and removes any views of it from the UI</summary>
        /// <param name="document">Document to close</param>
        public void Close(IDocument document)
        {
            EventSequenceContext context = document.As<EventSequenceContext>();
            m_controlHostService.UnregisterContent(context.View);
            context.ControlInfo = null;

            // close all active EditingContexts in the document
            foreach (DomNode node in context.DomNode.Subtree)
                foreach (EditingContext editingContext in node.AsAll<EditingContext>())
                    m_contextRegistry.RemoveContext(editingContext);

            // close the document
            m_documentRegistry.Remove(document);
        }

        #endregion

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the Control gets focus, or a parent "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        void IControlHostClient.Activate(object control)
        {
            var view = control as EventSequenceView;
            if (view != null)
            {
                var context = view.DataContext as EventSequenceContext;
                if (context != null)
                {
                    EventSequenceDocument document = context.Document;
                    if (document != null)
                    {
                        m_documentRegistry.ActiveDocument = document;
                        m_contextRegistry.ActiveContext = context;
                    }
                }
            }
        }

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another Control or "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        void IControlHostClient.Deactivate(object control)
        {
        }

        /// <summary>
        /// Requests permission to close the client's Control. Allows user to save document before it closes.</summary>
        /// <param name="control">Client Control to be closed</param>
        /// <param name="mainWindowClosing"><c>True</c> if the main window is closing</param>
        /// <returns><c>True</c> if the Control can close, or false to cancel</returns>
        /// <remarks>
        /// 1. This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.
        /// 2. If true is returned, the IControlHostService calls its own
        /// UnregisterControl. The IControlHostClient has to call RegisterControl again
        /// if it wants to re-register this Control.</remarks>
        bool IControlHostClient.Close(object control, bool mainWindowClosing)
        {
            bool closed = true;

            var view = control as EventSequenceView;
            if (view != null)
            {
                var context = view.DataContext as EventSequenceContext;
                if (context != null)
                {
                    EventSequenceDocument document = context.Document;
                    if (document != null)
                    {
                        closed = m_documentService.Close(document);
                        if (closed)
                            m_contextRegistry.RemoveContext(document);
                    }
                }
            }
            return closed;
        }

        #endregion

        private IControlHostService m_controlHostService;
        private IDocumentService m_documentService;
        private IContextRegistry m_contextRegistry;
        private IDocumentRegistry m_documentRegistry;
        private SchemaLoader m_schemaLoader;
    }
}
