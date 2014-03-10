//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Component that tracks documents</summary>
    [Export(typeof(IDocumentRegistry))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DocumentRegistry : IDocumentRegistry
    {
        /// <summary>
        /// Constructor</summary>
        public DocumentRegistry()
        {
            m_documents = new AdaptableActiveCollection<IDocument>();
            m_documents.ActiveItemChanging += documents_ActiveItemChanging;
            m_documents.ActiveItemChanged += documents_ActiveItemChanged;
            m_documents.ItemAdded += documents_ItemAdded;
            m_documents.ItemRemoved += documents_ItemRemoved;
        }

        #region IDocumentRegistry Members

        /// <summary>
        /// Gets and sets the active document. When getting, the result may be null if there are
        /// no open documents. When setting, the value must not be null, and if the document is
        /// not already on the list of open documents, it is added.</summary>
        public IDocument ActiveDocument
        {
            get { return m_documents.ActiveItem; }
            set { m_documents.ActiveItem = value; }
        }

        /// <summary>
        /// Gets the active document as the given type</summary>
        /// <typeparam name="T">Desired document type</typeparam>
        /// <returns>Active document as the given type, or null</returns>
        public T GetActiveDocument<T>()
            where T : class
        {
            return m_documents.ActiveItem.As<T>();
        }

        /// <summary>
        /// Gets the most recently active document of the given type; this may not be the
        /// same as the ActiveDocument</summary>
        /// <typeparam name="T">Desired document type</typeparam>
        /// <returns>Most recently active document of the given type, or null</returns>
        public T GetMostRecentDocument<T>()
            where T : class
        {
            foreach (IDocument document in m_documents.MostRecentOrder)
            {
                T result = document.As<T>();
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <summary>
        /// Event that is raised before the ActiveDocument property changes</summary>
        public event EventHandler ActiveDocumentChanging;

        /// <summary>
        /// Event that is raised after the ActiveDocument property has changed</summary>
        public event EventHandler ActiveDocumentChanged;

        /// <summary>
        /// Gets the open documents, in order of least-recently-active to the active document</summary>
        public IEnumerable<IDocument> Documents
        {
            get { return m_documents.AsIEnumerable<IDocument>(); }
        }

        /// <summary>
        /// Event that is raised after a document is added; it becomes the active document</summary>
        public event EventHandler<ItemInsertedEventArgs<IDocument>> DocumentAdded;

        /// <summary>
        /// Event that is raised after a document is removed</summary>
        public event EventHandler<ItemRemovedEventArgs<IDocument>> DocumentRemoved;

        /// <summary>
        /// Removes the document</summary>
        /// <param name="document">Document to remove</param>
        public void Remove(IDocument document)
        {
            m_documents.Remove(document);
        }

        #endregion

        private void documents_ActiveItemChanging(object sender, EventArgs e)
        {
            ActiveDocumentChanging.Raise(this, e);
        }

        private void documents_ActiveItemChanged(object sender, EventArgs e)
        {
            ActiveDocumentChanged.Raise(this, e);
        }

        private void documents_ItemAdded(object sender, ItemInsertedEventArgs<IDocument> e)
        {
            DocumentAdded.Raise(this, e);
        }

        private void documents_ItemRemoved(object sender, ItemRemovedEventArgs<IDocument> e)
        {
            DocumentRemoved.Raise(this, e);
        }

        private readonly AdaptableActiveCollection<IDocument> m_documents;
    }
}
