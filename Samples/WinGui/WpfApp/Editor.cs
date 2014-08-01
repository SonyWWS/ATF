//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.Wpf.Applications;
using Sce.Atf.Wpf.Docking;

using WinGuiCommon;

using IControlHostClient = Sce.Atf.Wpf.Applications.IControlHostClient;
using IControlHostService = Sce.Atf.Wpf.Applications.IControlHostService;
using ControlInfo = Sce.Atf.Wpf.Applications.ControlInfo;

namespace WpfApp
{
    /// <summary>
    /// Editor class component that creates and saves application documents</summary>
    [Export(typeof(IDocumentClient))]
    [Export(typeof(Editor))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Editor : EditorBase, IDocumentClient, IInitializable, IControlHostClient
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

        private IControlHostService m_controlHostService;
        private IDocumentService m_documentService;
        private IContextRegistry m_contextRegistry;
        private IDocumentRegistry m_documentRegistry;
        private SchemaLoader m_schemaLoader;

        [Import(AllowDefault = true)]
        private ScriptingService m_scriptingService = null;

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            // Set the application icon. We need to convert the reource from
            // System.Drawing.Image to System.Windows.Media.ImageSource.
            System.Drawing.Image atfIcon = ResourceUtil.GetImage(Sce.Atf.Resources.AtfIconImage);
            MemoryStream stream = new MemoryStream();
            atfIcon.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(stream.ToArray());
            bitmapImage.EndInit();

            System.Windows.Application.Current.MainWindow.Icon = bitmapImage;

            if (m_scriptingService != null)
            {
                // load this assembly into script domain.
                m_scriptingService.LoadAssembly(typeof(WinGuiCommon.EditorBase).Assembly);
                m_scriptingService.ImportAllTypes("WinGuiCommon");

                m_scriptingService.SetVariable("editor", this);

                m_contextRegistry.ActiveContextChanged += delegate
                {
                    EditingContext editingContext = m_contextRegistry.GetActiveContext<EditingContext>();
                    IHistoryContext hist = m_contextRegistry.GetActiveContext<IHistoryContext>();
                    m_scriptingService.SetVariable("editingContext", editingContext);
                    m_scriptingService.SetVariable("hist", hist);
                };
            }

            if (m_controlHostService != null)
            {
                m_eventView.DataContext = m_eventViewModel;
                m_controlHostService.RegisterControl(new ControlDef
                {
                    Name = "Event Viewer".Localize(),
                    Description = "Viewer for event details".Localize(),
                    Id = "wpfApp1",
                    Group = StandardControlGroup.Bottom
                }, m_eventView, this);

                m_documentRegistry.ActiveDocumentChanged += ContextSelectionChanged;
            }
        }

        #endregion

        #region IDocumentClient Members

        /// <summary>
        /// Gets information about the document client, such as the file type and file
        /// extensions it supports, whether or not it allows multiple documents to be open,
        /// etc.</summary>
        public DocumentClientInfo Info
        {
            get { return DocumentClientInfo; }
        }

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
                node = new DomNode(Schema.winGuiCommonDataType.Type, Schema.winGuiCommonDataRootElement);
            }

            WinGuiWpfDataDocument document = null;
            if (node != null)
            {
                // Initialize Dom extensions now that the data is complete
                node.InitializeExtensions();

                WinGuiWpfDataContext context = node.As<WinGuiWpfDataContext>();
                context.SelectionChanged += ContextSelectionChanged;

                ControlInfo controlInfo = new ControlInfo(Path.Combine(filePath, fileName), StandardControlGroup.Center, new Sce.Atf.Wpf.Docking.DockContent(null, null), this);
                context.ControlInfo = controlInfo;

                // set document URI
                document = node.As<WinGuiWpfDataDocument>();
                document.Uri = uri;

                context.ListView.Tag = document;

                // If the document is empty, add some data so there's something to look at
                if (!context.Items.Any(typeof(object)))
                {
                    DomNode domNode = new DomNode(Schema.eventType.Type);
                    domNode.SetAttribute(Schema.eventType.nameAttribute, "First Event");
                    domNode.SetAttribute(Schema.eventType.durationAttribute, 100);
                    var dataObject = new System.Windows.DataObject(new object[] { domNode });
                    context.Insert(dataObject);

                    domNode = new DomNode(Schema.eventType.Type);
                    domNode.SetAttribute(Schema.eventType.nameAttribute, "Second Event");
                    domNode.SetAttribute(Schema.eventType.durationAttribute, 200);
                    var dataObject2 = new System.Windows.DataObject(new object[] { domNode });
                    context.Insert(dataObject2);
                }

                // show the ListView control
                ControlInfo registeredControlInfo = (ControlInfo)m_controlHostService.RegisterControl(context.ListView, 
                    fileName, 
                    "WPF Sample file", 
                    controlInfo.Group, 
                    fileName, 
                    this);

                if (registeredControlInfo.DockContent != null)
                {
                    registeredControlInfo.DockContent.Closing += DockContent_Closing;
                }
            }

            return document;
        }

        /// <summary>
        /// Makes the document visible to the user</summary>
        /// <param name="document">Document to show</param>
        public void Show(IDocument document)
        {
            WinGuiWpfDataContext context = Adapters.As<WinGuiWpfDataContext>(document);
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
                DomXmlWriter writer = new DomXmlWriter(m_schemaLoader.TypeCollection);
                WinGuiWpfDataDocument eventSequenceDocument = (WinGuiWpfDataDocument)document;
                writer.Write(eventSequenceDocument.DomNode, stream, uri);
            }
        }

        /// <summary>
        /// Closes the document and removes any views of it from the UI</summary>
        /// <param name="document">Document to close</param>
        public void Close(IDocument document)
        {
            WinGuiWpfDataContext context = Adapters.As<WinGuiWpfDataContext>(document);
            m_controlHostService.UnregisterContent(context.ListView);
            context.ControlInfo = null;
            context.SelectionChanged -= ContextSelectionChanged;

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
            WinGuiWpfDataDocument document = null;

            if (control is System.Windows.Forms.Control)
            {
                document = ((System.Windows.Forms.Control)control).Tag as WinGuiWpfDataDocument;
            }

            if (document != null)
            {
                m_documentRegistry.ActiveDocument = document;

                WinGuiWpfDataContext context = document.As<WinGuiWpfDataContext>();
                m_contextRegistry.ActiveContext = context;
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
        /// Requests permission to close the client's Control</summary>
        /// <param name="control">Client Control to be closed</param>
        /// <returns>True if the Control can close, or false to cancel</returns>
        /// <remarks>
        /// 1. This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.
        /// 2. If true is returned, the IControlHostService calls its own
        /// UnregisterControl. The IControlHostClient has to call RegisterControl again
        /// if it wants to re-register this Control.</remarks>
        bool IControlHostClient.Close(object control, bool mainWindowClosing)
        {
            bool closed = true;

            if (control is Control)
            {
                WinGuiWpfDataDocument document = ((Control)control).Tag as WinGuiWpfDataDocument;
                if (document != null)
                {
                    closed = m_documentService.Close(document);
                    if (closed)
                        m_contextRegistry.RemoveContext(document);
                }
            }
            return closed;
        }

        #endregion

        private void ContextSelectionChanged(object sender, EventArgs e)
        {
            // Display the properties of the most recently selected event in the currently active document.
            if ((m_documentRegistry != null) && (m_documentRegistry.ActiveDocument != null))
            {
                WinGuiWpfDataContext dataContext = m_documentRegistry.ActiveDocument.As<WinGuiWpfDataContext>();
                m_eventViewModel.Event = dataContext.Selection.LastSelected.As<WinGuiCommon.Event>();
            }
        }

        private void DockContent_Closing(object sender, Sce.Atf.Wpf.Docking.ContentClosedEventArgs args)
        {
            var dockContent = sender as DockContent;
            if (sender == null) return;

            var control = dockContent.Content as System.Windows.Forms.Control;
            if (control == null) return;

            var document = control.Tag as WinGuiWpfDataDocument;
            if (document != null)
            {
                Close(document);
            }
        }

        private EventView m_eventView = new EventView();
        private EventViewModel m_eventViewModel = new EventViewModel();
    }
}
