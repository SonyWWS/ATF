//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace SimpleDomNoXmlEditorSample
{
    /// <summary>
    /// Editor class that creates and saves event sequence documents. It also registers
    /// the event sequence ListView controls with the hosting service.</summary>
    [Export(typeof(IDocumentClient))]
    [Export(typeof(Editor))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Editor : IDocumentClient, IControlHostClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="controlHostService">Control host service</param>
        /// <param name="documentService">Document service</param>
        /// <param name="contextRegistry">Context registry</param>
        /// <param name="documentRegistry">Document registry</param>
        /// <param name="domTypes">DOM types</param>
        [ImportingConstructor]
        public Editor(
            IControlHostService controlHostService,
            IDocumentService documentService,
            IContextRegistry contextRegistry,
            IDocumentRegistry documentRegistry,
            DomTypes domTypes)
        {
            m_controlHostService = controlHostService;
            m_documentService = documentService;
            m_contextRegistry = contextRegistry;
            m_documentRegistry = documentRegistry;
        }

        private IControlHostService m_controlHostService;
        private IDocumentService m_documentService;
        private IContextRegistry m_contextRegistry;
        private IDocumentRegistry m_documentRegistry;

        // scripting related members
        [Import(AllowDefault = true)]
        private ScriptingService m_scriptingService = null;

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by setting up scripting service</summary>
        void IInitializable.Initialize()
        {
            if (m_scriptingService != null)
            {
                // load this assembly into script domain.
                m_scriptingService.LoadAssembly(GetType().Assembly);
                m_scriptingService.ImportAllTypes("SimpleDomNoXmlEditorSample");
                m_scriptingService.SetVariable("editor", this);
            }
        }

        #endregion

        #region IDocumentClient Members

        /// <summary>
        /// Gets information about the document client, such as the file type and file
        /// extensions it supports, whether or not it allows multiple documents to be open, etc.
        /// This document client does file operations on documents and persists application data.</summary>
        public DocumentClientInfo Info
        {
            get { return DocumentClientInfo; }
        }

        /// <summary>
        /// Information about the document client</summary>
        public static DocumentClientInfo DocumentClientInfo = new DocumentClientInfo(
            "Event Sequence".Localize(),
            new string[] { ".SimpleDomTxt" },
            Sce.Atf.Resources.DocumentImage,
            Sce.Atf.Resources.FolderImage,
            true);

        /// <summary>
        /// Returns whether the client can open or create a document at the given URI</summary>
        /// <param name="uri">Document URI</param>
        /// <returns>True iff the client can open or create a document at the given URI</returns>
        public bool CanOpen(Uri uri)
        {
            return DocumentClientInfo.IsCompatibleUri(uri);
        }

        /// <summary>
        /// Opens or creates a document at the given URI</summary>
        /// <param name="uri">Document URI</param>
        /// <returns>Document or null if the document couldn't be opened or created</returns>
        public IDocument Open(Uri uri)
        {
            DomNode node = null;
            string filePath = uri.LocalPath;
            string fileName = Path.GetFileName(filePath);

            if (File.Exists(filePath))
            {
                // read existing document using a custom file reader
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    node = EventSequenceDocument.Read(stream).DomNode;
                }
            }
            else
            {
                // create new document by creating a Dom node of the root type
                node = new DomNode(DomTypes.eventSequenceType.Type, DomTypes.eventSequenceRootElement);
            }

            EventSequenceDocument document = null;
            if (node != null)
            {
                // Initialize Dom extensions now that the data is complete
                node.InitializeExtensions();

                EventSequenceContext context = node.As<EventSequenceContext>();

                ControlInfo controlInfo = new ControlInfo(fileName, filePath, StandardControlGroup.Center);
                context.ControlInfo = controlInfo;

                // set document URI
                document = node.As<EventSequenceDocument>();
                document.Uri = uri;

                context.ListView.Tag = document;

                // show the ListView control
                m_controlHostService.RegisterControl(context.ListView, controlInfo, this);
            }

            return document;
        }

        /// <summary>
        /// Makes the document visible to the user</summary>
        /// <param name="document">Document to show</param>
        public void Show(IDocument document)
        {
            EventSequenceContext context = document.As<EventSequenceContext>();
            m_controlHostService.Show(context.ListView);
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
                EventSequenceDocument eventSequenceDocument = (EventSequenceDocument)document;
                EventSequenceDocument.Write(eventSequenceDocument, stream);
            }
        }

        /// <summary>
        /// Closes the document and removes any views of it from the UI</summary>
        /// <param name="document">Document to close</param>
        public void Close(IDocument document)
        {
            EventSequenceContext context = document.As<EventSequenceContext>();
            m_controlHostService.UnregisterControl(context.ListView);
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
        void IControlHostClient.Activate(Control control)
        {
            EventSequenceDocument document = control.Tag as EventSequenceDocument;
            if (document != null)
            {
                m_documentRegistry.ActiveDocument = document;

                EventSequenceContext context = document.As<EventSequenceContext>();
                m_contextRegistry.ActiveContext = context;
            }
        }

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another Control or "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        void IControlHostClient.Deactivate(Control control)
        {
        }

        /// <summary>
        /// Requests permission to close the client's Control.
        /// Allows user to save document before closing.</summary>
        /// <param name="control">Client Control to be closed</param>
        /// <returns>True if the Control can close, or false to cancel</returns>
        /// <remarks>
        /// 1. This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.
        /// 2. If true is returned, the IControlHostService calls its own
        /// UnregisterControl. The IControlHostClient has to call RegisterControl again
        /// if it wants to re-register this Control.</remarks>
        bool IControlHostClient.Close(Control control)
        {
            bool closed = true;

            EventSequenceDocument document = control.Tag as EventSequenceDocument;
            if (document != null)
            {
                closed = m_documentService.Close(document);
                if (closed)
                    m_contextRegistry.RemoveContext(document);
            }

            return closed;
        }

        #endregion
    }
}
