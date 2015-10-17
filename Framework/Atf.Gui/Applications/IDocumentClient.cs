//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for clients of IDocumentService components</summary>
    public interface IDocumentClient
    {
        /// <summary>
        /// Gets information about the document client, such as the file type and file
        /// extensions it supports, whether or not it allows multiple documents to be open,
        /// etc.</summary>
        DocumentClientInfo Info
        {
            get;
        }

        /// <summary>
        /// Returns whether the client can open or create a document at the given URI</summary>
        /// <param name="uri">Document URI</param>
        /// <returns><c>True</c> if the client can open or create a document at the given URI</returns>
        bool CanOpen(Uri uri);

        /// <summary>
        /// Opens or creates a document at the given URI</summary>
        /// <param name="uri">Document URI</param>
        /// <returns>Document, or null if the document couldn't be opened or created</returns>
        IDocument Open(Uri uri);

        /// <summary>
        /// Makes the document visible to the user</summary>
        /// <param name="document">Document to show</param>
        void Show(IDocument document);

        /// <summary>
        /// Saves the document at the given URI</summary>
        /// <param name="document">Document to save</param>
        /// <param name="uri">New document URI</param>
        void Save(IDocument document, Uri uri);

        /// <summary>
        /// Closes the document and removes any views of it from the UI</summary>
        /// <param name="document">Document to close</param>
        void Close(IDocument document);
    }
}
