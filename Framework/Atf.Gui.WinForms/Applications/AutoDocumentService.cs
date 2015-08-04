//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Windows.Forms;

using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Service that auto-loads a document at application startup</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(AutoDocumentService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AutoDocumentService : IInitializable, IPartImportsSatisfiedNotification
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="documentRegistry">Document registry used to get and set the active document and to
        /// know which documents are open when the main form closes</param>
        /// <param name="documentService">Document service used to open previously opened documents and to
        /// close the auto-generated new document</param>
        [ImportingConstructor]
        public AutoDocumentService(
            IDocumentRegistry documentRegistry,
            IDocumentService documentService)
        {
            DocumentRegistry = documentRegistry;
            DocumentService = documentService;
        }

        #region IPartImportsSatisfiedNotification Members

        /// <summary>
        /// Notification when part's imports have been satisfied</summary>
        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            if (m_mainWindow == null &&
                m_mainForm != null)
            {
                m_mainWindow = new MainFormAdapter(m_mainForm);
            }

            if (m_mainWindow == null)
                throw new InvalidOperationException("Can't get main window");

            m_mainWindow.Loaded += mainWindow_Loaded;
            m_mainWindow.Closing += mainWindow_Closing;
        }

        #endregion

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            if (m_settingsService != null)
            {
                BoundPropertyDescriptor autoLoadDocuments =
                    new BoundPropertyDescriptor(this, () => AutoLoadDocuments,
                        "Auto-load Documents".Localize(), null,
                        "Load previously open documents on application startup".Localize());
                BoundPropertyDescriptor autoNewDocument =
                    new BoundPropertyDescriptor(this, () => AutoNewDocument,
                        "Auto New Document".Localize("Create a new empty document on application startup"), null,
                        "Create a new empty document on application startup".Localize());
                BoundPropertyDescriptor autoDocuments =
                    new BoundPropertyDescriptor(this, () => AutoDocuments, "AutoDocuments", null, null);

                m_settingsService.RegisterSettings(this, autoNewDocument, autoLoadDocuments, autoDocuments);

                m_settingsService.RegisterUserSettings("Documents".Localize(), autoNewDocument, autoLoadDocuments);
            }
        }

        #endregion

        /// <summary>
        /// Gets or sets whether documents are loaded on application startup</summary>
        [DefaultValue(true)]
        public bool AutoLoadDocuments
        {
            get { return m_autoLoadDocuments; }
            set { m_autoLoadDocuments = value; }
        }

        /// <summary>
        /// Gets or sets whether new documents can be created on application startup</summary>
        [DefaultValue(true)]
        public bool AutoNewDocument
        {
            get { return m_autoNewDocument; }
            set { m_autoNewDocument = value; }
        }

        /// <summary>
        /// Gets or sets a string of documents to be opened automatically on application startup</summary>
        public string AutoDocuments
        {
            get { return m_openDocuments; }
            set { m_openDocuments = value; }
        }

        /// <summary>
        /// Gets or sets the document clients</summary>
        [ImportMany]
        protected IEnumerable<Lazy<IDocumentClient>> DocumentClients;

        /// <summary>
        /// Gets the document service that was passed into the constructor</summary>
        protected readonly IDocumentService DocumentService;

        /// <summary>
        /// The document registry that was passed into the constructor</summary>
        protected readonly IDocumentRegistry DocumentRegistry;

        /// <summary>
        /// Is called once, when the application starts and if AutoNewDocument is true, to
        /// automatically create new documents.</summary>
        /// <remarks>By default, only one new document will be used and only if there are no
        /// documents open already. If there are multiple document clients available, the first
        /// single-document client will be used.</remarks>
        protected virtual void CreateNewDocuments()
        {
            // Automatically create a new document only if there are none open yet.
            if (DocumentRegistry.ActiveDocument == null)
            {
                IDocumentClient autoDocumentClient = null;
                foreach (IDocumentClient client in DocumentClients.GetValues())
                {
                    if (!client.Info.MultiDocument)
                    {
                        autoDocumentClient = client;
                        break;
                    }
                    if (autoDocumentClient == null)
                        autoDocumentClient = client;
                }

                if (autoDocumentClient != null)
                    DocumentService.OpenNewDocument(autoDocumentClient);
            }
        }

        private void mainWindow_Loaded(object sender, EventArgs e)
        {
            // Automatically load documents only if there are none open yet.
            if (m_autoLoadDocuments && DocumentRegistry.ActiveDocument == null)
            {
                bool autoLoadDocs = true;
                if (m_commandLineArgsService != null)
                {
                    if (m_commandLineArgsService.Parameters.Count > 0) // commandLineArgsService takes precedence
                        autoLoadDocs = false;
                }

                if (autoLoadDocs)
                {
                    string[] uriStrings = m_openDocuments.Split(new[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string uriString in uriStrings)
                    {
                        Uri uri;
                        if (Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out uri))
                        {
                            foreach (IDocumentClient client in DocumentClients.GetValues())
                            {
                                try
                                {
                                    // IDocumentClient.CanOpen() can return true if the document doesn't exist but can be
                                    //  created. We shouldn't create a blank document with an old URI.
                                    if ((!uri.IsAbsoluteUri || File.Exists(uri.LocalPath)) &&
                                        client.CanOpen(uri))
                                    {
                                        DocumentService.OpenExistingDocument(client, uri);
                                        break;
                                    }
                                }
                                catch
                                {
                                    // It's difficult to know all the possible exceptions that can be thrown by Uri.LocalPath
                                    //  and File.Exists. 'uriString' could be anything, since it came from a user settings
                                    //  file. We don't want to bring down the app over this.
                                }
                            }
                        }
                    }
                }
            }

            if (AutoNewDocument)
                CreateNewDocuments();
        }

        private void mainWindow_Closing(object sender, CancelEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            char separator = Path.PathSeparator;
            foreach (IDocument document in DocumentRegistry.Documents)
            {
                sb.Append(document.Uri);
                sb.Append(separator);
            }

            m_openDocuments = sb.ToString();
        }

        [Import(AllowDefault = true)]
        private IMainWindow m_mainWindow;

        [Import(AllowDefault = true)]
        private Form m_mainForm;

        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService;

        [Import(AllowDefault = true)]
        private CommandLineArgsService m_commandLineArgsService;

        private string m_openDocuments = string.Empty;
        private bool m_autoLoadDocuments = true;
        private bool m_autoNewDocument = true;
    }
}
