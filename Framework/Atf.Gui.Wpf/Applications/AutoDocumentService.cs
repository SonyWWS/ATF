//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Service that auto-loads a document at application startup</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(AutoDocumentService))]
    [ExportMetadata("Order", 100)]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AutoDocumentService : IInitializable
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
            m_documentRegistry = documentRegistry;
            m_documentService = documentService;
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            if (m_mainWindow != null)
            {
                m_mainWindow.Loaded += mainWindow_Loaded;
                m_mainWindow.Closing += mainWindow_Closing;
            }

            if (m_settingsService != null)
            {
                var autoLoadDocuments =
                    new BoundPropertyDescriptor(this, () => AutoLoadDocuments,
                                                "Auto-load Documents".Localize(), null,
                                                "Load previously open documents on application startup".Localize());
                var autoNewDocument =
                    new BoundPropertyDescriptor(this, () => AutoNewDocument,
                                                "Auto New Document".Localize(
                                                    "Create a new empty document on application startup"), null,
                                                "Create a new empty document on application startup".Localize());
                var autoDocuments =
                    new BoundPropertyDescriptor(this, () => AutoDocuments, "AutoDocuments", null, null);

                m_settingsService.RegisterSettings(this, autoNewDocument, autoLoadDocuments, autoDocuments);
                m_settingsService.RegisterUserSettings("Documents".Localize(), autoNewDocument, autoLoadDocuments);
            }
        }

        #endregion

        /// <summary>
        /// Gets or sets whether documents are loaded on application startup</summary>
        public bool AutoLoadDocuments
        {
            get { return m_autoLoadDocuments; }
            set { m_autoLoadDocuments = value; }
        }

        /// <summary>
        /// Gets or sets whether new documents can be created on application startup</summary>
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

        private void mainWindow_Loaded(object sender, EventArgs e)
        {
            bool documentsOpen = m_documentRegistry.ActiveDocument != null;

            // auto-load documents only if there are none open yet
            if (m_autoLoadDocuments && !documentsOpen)
            {
                string[] uriStrings = m_openDocuments.Split(new[] { Path.PathSeparator },
                                                            StringSplitOptions.RemoveEmptyEntries);
                foreach (string uriString in uriStrings)
                {
                    Uri uri;
                    if (Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out uri))
                    {
                        foreach (var client in m_documentClients.GetValues())
                        {
                            try
                            {
                                // IDocumentClient.CanOpen() can return true if the document doesn't exist but can be
                                //  created. We shouldn't create a blank document with an old URI.
                                if ((!uri.IsAbsoluteUri || File.Exists(uri.LocalPath)) &&
                                    client.CanOpen(uri))
                                {
                                    m_documentService.OpenExistingDocument(client, uri);
                                    documentsOpen = true;
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

            // auto-new document only if there are none open yet
            if (m_autoNewDocument && !documentsOpen)
            {
                IDocumentClient autoDocumentClient = null;
                foreach (IDocumentClient client in m_documentClients.GetValues())
                {
                    if (m_masterClient == null && !client.Info.MultiDocument)
                    {
                        m_masterClient = client;
                        autoDocumentClient = client;
                    }
                    if (autoDocumentClient == null)
                        autoDocumentClient = client;
                }

                if (autoDocumentClient != null)
                {
                    m_autoDocument = m_documentService.OpenNewDocument(autoDocumentClient);
                    if (m_autoDocument != null)
                        m_autoDocument.DirtyChanged += autoDocument_DirtyChanged;
                }
            }
        }

        private void mainWindow_Closing(object sender, CancelEventArgs e)
        {
            var sb = new StringBuilder();
            char separator = Path.PathSeparator;
            foreach (var document in m_documentRegistry.Documents)
            {
                sb.Append(document.Uri);
                sb.Append(separator);
            }

            m_openDocuments = sb.ToString();
        }

        private void autoDocument_DirtyChanged(object sender, EventArgs e)
        {
            // user touched auto-created document; cut it loose
            m_autoDocument.DirtyChanged -= autoDocument_DirtyChanged;
            m_autoDocument = null;
        }

        private readonly IDocumentRegistry m_documentRegistry;
        private readonly IDocumentService m_documentService;

        [Import(AllowDefault = true)] private IMainWindow m_mainWindow = null;

        [Import(AllowDefault = true)] private ISettingsService m_settingsService = null;

        [ImportMany] private IEnumerable<Lazy<IDocumentClient>> m_documentClients = null;

        private IDocumentClient m_masterClient;

        private IDocument m_autoDocument;
        private string m_openDocuments = string.Empty;
        private bool m_autoLoadDocuments = true;
        private bool m_autoNewDocument = true;
    }
}
