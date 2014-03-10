//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for component that tracks documents</summary>
    public interface IDocumentRegistry
    {
        /// <summary>
        /// Gets and sets the active document. When getting, the result may be null if there are
        /// no open documents. When setting, the value must not be null, and if the document is
        /// not already on the list of open documents, it is added.</summary>
        IDocument ActiveDocument
        {
            get;
            set;
        }

        /// <summary>
        /// Obtains the active document as the given type</summary>
        /// <typeparam name="T">Desired document type</typeparam>
        /// <returns>Active document as the given type, or null</returns>
        T GetActiveDocument<T>()
            where T : class;

        /// <summary>
        /// Returns the most recently active document of the given type; this may not be the
        /// same as the ActiveDocument</summary>
        /// <typeparam name="T">Desired document type</typeparam>
        /// <returns>Most recently active document of the given type, or null</returns>
        T GetMostRecentDocument<T>()
            where T : class;

        /// <summary>
        /// Event that is raised before the active document changes</summary>
        event EventHandler ActiveDocumentChanging;

        /// <summary>
        /// Event that is raised after the active document changes</summary>
        event EventHandler ActiveDocumentChanged;

        /// <summary>
        /// Gets the open documents, in order of least-recently-active to the active document</summary>
        IEnumerable<IDocument> Documents
        {
            get;
        }

        /// <summary>
        /// Event that is raised after a document is added; it becomes the active document</summary>
        event EventHandler<ItemInsertedEventArgs<IDocument>> DocumentAdded;

        /// <summary>
        /// Event that is raised after a document is removed</summary>
        event EventHandler<ItemRemovedEventArgs<IDocument>> DocumentRemoved;

        /// <summary>
        /// Removes the document</summary>
        /// <param name="document">Document to remove</param>
        void Remove(IDocument document);
    }

    /// <summary>
    /// Extension methods for IDocumentRegistry</summary>
    public static class DocumentRegistries
    {
        /// <summary>
        /// Gets the document associated with the given URI</summary>
        /// <param name="registry">Document registry</param>
        /// <param name="uri">URI that might match an IDocument's Uri property</param>
        /// <returns>A document with a matching URI or null if it wasn't found</returns>
        public static IDocument GetDocument(this IDocumentRegistry registry, Uri uri)
        {
            foreach (IDocument document in registry.Documents)
                if (document.Uri == uri)
                    return document;
            return null;
        }
    }
}
