//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for component that implements document commands such as New, Open,
    /// Save, SaveAs, SaveAll, Close, and CloseAll. See also the IDocumentClient
    /// interface.</summary>
    public interface IDocumentService
    {
        /// <summary>
        /// Opens a new document for the given client</summary>
        /// <param name="client">Document client</param>
        /// <returns>Document, opened by the given client</returns>
        IDocument OpenNewDocument(IDocumentClient client);

        /// <summary>
        /// Opens an existing document for the given client</summary>
        /// <param name="client">Document client</param>
        /// <param name="uri">Document URI, or null to present file open dialog to user</param>
        /// <returns>Document, opened by the given client</returns>
        IDocument OpenExistingDocument(IDocumentClient client, Uri uri);

        /// <summary>
        /// Saves the document under its current name</summary>
        /// <param name="document">Document to save</param>
        /// <returns><c>True</c> if document was successfully saved and false if the user cancelled
        /// or there was some kind of problem</returns>
        bool Save(IDocument document);

        /// <summary>
        /// Saves the document under a new name, chosen by the user</summary>
        /// <param name="document">Document to save</param>
        /// <returns><c>True</c> if document was successfully saved and false if the user cancelled
        /// or there was some kind of problem</returns>
        bool SaveAs(IDocument document);

        /// <summary>
        /// Saves all documents</summary>
        /// <param name="cancelOnFail">Value indicating if remaining saves should be cancelled
        /// if one fails</param>
        /// <returns><c>True</c> if all the documents were saved and false if the user cancelled
        /// or there was some kind of problem with one or more of the documents</returns>
        bool SaveAll(bool cancelOnFail);

        /// <summary>
        /// Closes the document</summary>
        /// <param name="document">Document to close</param>
        /// <returns><c>True</c> if close was not cancelled by user</returns>
        bool Close(IDocument document);

        /// <summary>
        /// Closes all documents</summary>
        /// <param name="masterDocument">Master document, or null if none. The master
        /// document is closed last.</param>
        /// <returns><c>True</c> if no close was cancelled by user</returns>
        bool CloseAll(IDocument masterDocument);

        /// <summary>
        /// Determines if the given document is untitled; i.e., the document has
        /// not been named by the user</summary>
        /// <param name="document">Document to check</param>
        /// <returns><c>True</c> if the document is untitled</returns>
        bool IsUntitled(IDocument document);

        /// <summary>
        /// Event that is raised after a document is opened</summary>
        event EventHandler<DocumentEventArgs> DocumentOpened;

        /// <summary>
        /// Event that is raised after a document is saved</summary>
        event EventHandler<DocumentEventArgs> DocumentSaved;

        /// <summary>
        /// Event that is raised after a document is closed</summary>
        event EventHandler<DocumentEventArgs> DocumentClosed;
    }

    /// <summary>
    /// Useful static and extension methods for document services</summary>
    public static class DocumentServices
    {
        /// <summary>
        /// Opens a document using the first document client that can open the given URI</summary>
        /// <param name="documentService">Document service</param>
        /// <param name="documentClients">Sequence of document clients to use</param>
        /// <param name="uri">URI of document</param>
        /// <returns>Document with the given URI or null if document couldn't be opened</returns>
        /// <remarks>Each document client is tried in order until one works or all fail</remarks>
        public static IDocument OpenExistingDocument(
            this IDocumentService documentService,
            IEnumerable<IDocumentClient> documentClients,
            Uri uri)
        {
            foreach (IDocumentClient documentClient in documentClients)
            {
                if (documentClient.CanOpen(uri))
                {
                    return documentService.OpenExistingDocument(documentClient, uri);
                }
            }

            return null;
        }
    }
}
