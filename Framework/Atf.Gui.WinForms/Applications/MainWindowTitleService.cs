//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Service that updates the main form's title to reflect the current document and its state</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(MainWindowTitleService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MainWindowTitleService : IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="mainWindow">Application's main window</param>
        /// <param name="documentRegistry">Document registry</param>
        [ImportingConstructor]
        public MainWindowTitleService(
            IMainWindow mainWindow,
            IDocumentRegistry documentRegistry)
        {
            m_mainWindow = mainWindow;

            m_documentRegistry = documentRegistry;
            m_documentRegistry.ActiveDocumentChanging += documentRegistry_ActiveDocumentChanging;
            m_documentRegistry.ActiveDocumentChanged += documentRegistry_ActiveDocumentChanged;
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            // implement IInitializable to force component into existence
        }

        #endregion

        private void documentRegistry_ActiveDocumentChanging(object sender, EventArgs e)
        {
            IDocument document = m_documentRegistry.ActiveDocument;
            if (document != null)
            {
                document.UriChanged -= ActiveDocument_UriChanged;
                document.DirtyChanged -= ActiveDocument_DirtyChanged;
            }
        }

        private void documentRegistry_ActiveDocumentChanged(object sender, EventArgs e)
        {
            IDocument document = m_documentRegistry.ActiveDocument;
            if (document != null)
            {
                document.UriChanged += ActiveDocument_UriChanged;
                document.DirtyChanged += ActiveDocument_DirtyChanged;
            }

            UpdateMainWindow(m_mainWindow, m_documentRegistry.ActiveDocument);
        }

        private void ActiveDocument_UriChanged(object sender, UriChangedEventArgs e)
        {
            UpdateMainWindow(m_mainWindow, m_documentRegistry.ActiveDocument);
        }

        private void ActiveDocument_DirtyChanged(object sender, EventArgs e)
        {
            UpdateMainWindow(m_mainWindow, m_documentRegistry.ActiveDocument);
        }

        /// <summary>
        /// Updates the main form as the active document changes</summary>
        /// <param name="mainWindow">Main application window</param>
        /// <param name="activeDocument">Active document</param>
        protected virtual void UpdateMainWindow(IMainWindow mainWindow, IDocument activeDocument)
        {
            string text = Application.ProductName + " - ";
            IDocument document = m_documentRegistry.ActiveDocument;
            if (document != null && document.Uri != null)
            {
                Uri uri = document.Uri;
                if (uri.IsFile)
                    text += document.Uri.LocalPath;
                else
                    text += document.Uri.ToString();

                if (document.Dirty)
                    text += "*";
            }
            mainWindow.Text = text;
        }

        private readonly IMainWindow m_mainWindow;
        private readonly IDocumentRegistry m_documentRegistry;
    }
}
