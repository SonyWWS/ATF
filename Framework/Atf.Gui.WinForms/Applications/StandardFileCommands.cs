//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;

using Keys = Sce.Atf.Input.Keys;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Component that adds standard file commands: File/New, File/Open, File/Save,
    /// File/SaveAs, File/SaveAll, File/Close. This component requires an
    /// IFileDialogService for file dialogs; ATF provides a default implementation,
    /// FileDialogService. Use this, customize it, or provide your own implementation.</summary>
    [Export(typeof(IDocumentService))]
    [Export(typeof(StandardFileCommands))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class StandardFileCommands : IDocumentService, ICommandClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="documentRegistry">Document registry</param>
        /// <param name="fileDialogService">File dialog service</param>
        [ImportingConstructor]
        public StandardFileCommands(
            ICommandService commandService,
            IDocumentRegistry documentRegistry,
            IFileDialogService fileDialogService)
        {
            CommandService = commandService;
            DocumentRegistry = documentRegistry;
            FileDialogService = fileDialogService;
        }

        /// <summary>
        /// Flags that determine which commands should appear in the File menu</summary>
        [Flags]
        public enum CommandRegister
        {
            /// <summary>No File commands</summary>
            None = 0,
            /// <summary>File/New command</summary>
            FileNew = 1,
            /// <summary>File/Open command</summary>
            FileOpen = 2,
            /// <summary>File/Save command</summary>
            FileSave = 4,
            /// <summary>File/Save As command</summary>
            FileSaveAs = 8,
            /// <summary>File/Save All command</summary>
            FileSaveAll = 16,
            /// <summary>File/Close command</summary>
            FileClose = 32,
            /// <summary>All File commands-default</summary>
            Default = FileNew | FileOpen | FileSave | FileSaveAs | FileSaveAll | FileClose
        }


        /// <summary>
        /// Gets or sets a value indicating if a standard file command will be registered. Must
        /// be set before IInitialize is called.</summary>
        public CommandRegister RegisterCommands
        {
            get { return m_registerCommands; }
            set { m_registerCommands = value; }
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            // build dictionary to map file types to document clients
            m_typeToClientMap.Clear();
            foreach (Lazy<IDocumentClient> lazy in m_documentClients)
            {
                IDocumentClient client = lazy.Value;
                m_typeToClientMap.Add(client.Info.FileType, client);
            }

            // register standard file menu commands
            if ((RegisterCommands & CommandRegister.FileSave) == CommandRegister.FileSave)
                CommandService.RegisterCommand(CommandInfo.FileSave, this);
            if ((RegisterCommands & CommandRegister.FileSaveAs) == CommandRegister.FileSaveAs)
                CommandService.RegisterCommand(CommandInfo.FileSaveAs, this);
            if ((RegisterCommands & CommandRegister.FileSaveAll) == CommandRegister.FileSaveAll)
                CommandService.RegisterCommand(CommandInfo.FileSaveAll, this);
            if ((RegisterCommands & CommandRegister.FileClose) == CommandRegister.FileClose)
                CommandService.RegisterCommand(CommandInfo.FileClose, this);

            RegisterClientCommands();

            Initialize();
        }

        #endregion

        #region IDocumentService Members

        /// <summary>
        /// Opens a new document for the given client</summary>
        /// <param name="client">Document client</param>
        /// <returns>Document, opened by the given client, or null if the user cancelled or
        /// there was a problem</returns>
        /// <remarks>Exceptions during opening are caught and reported via OnOpenException.</remarks>
        public virtual IDocument OpenNewDocument(IDocumentClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            IDocument result = null;
            Uri uri = GetNewDocumentUri(client);
            if (uri != null)
            {
                result = SafeOpen(client, uri);

                // Consider the document untitled unless its file has been created by the client or
                //  unless the user has already chosen a filename.
                if (result != null)
                {
                    if (!m_newDocumentPaths.Contains(result.Uri.LocalPath) &&
                        !FileDialogService.PathExists(result.Uri.LocalPath))
                    {
                        m_untitledDocuments.Add(result);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Opens one or more existing documents for the given client</summary>
        /// <param name="client">Document client</param>
        /// <param name="uri">Document URI, or null to present file dialog to user</param>
        /// <returns>Last document opened by the given client, or null</returns>
        /// <remarks>Exceptions during opening are caught and reported via OnOpenException.</remarks>
        public virtual IDocument OpenExistingDocument(IDocumentClient client, Uri uri)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            string filter = client.Info.GetFilterString();
            string[] pathNames = null;
            if (uri != null)
            {
                pathNames = new[] { uri.ToString() };
            }
            else
            {
                FileDialogService.InitialDirectory = client.Info.InitialDirectory;
                FileDialogService.OpenFileNames(ref pathNames, filter);
            }

            IDocument document = null;
            if (pathNames != null)
            {
                foreach (string pathName in pathNames)
                {
                    Uri docUri = new Uri(pathName, UriKind.RelativeOrAbsolute);
                    IDocument openDocument = FindOpenDocument(docUri);
                    if (openDocument != null)
                    {
                        // Simply show the document. http://tracker.ship.scea.com/jira/browse/CORETEXTEDITOR-403
                        document = openDocument;
                        client.Show(openDocument);
                    }
                    else
                        document = SafeOpen(client, docUri);
                }
            }

            return document;
        }

        /// <summary>
        /// Saves the document under its current name</summary>
        /// <param name="document">Document to save</param>
        /// <returns>True if document was successfully saved and false if the user cancelled
        /// or there was some kind of problem</returns>
        /// <remarks>All exceptions are caught and reported via OnSaveException(). The
        /// IDocumentClient is responsible for the save operation and ensuring that any
        /// existing files are not corrupted if the save fails. Only documents that are dirty
        /// are really saved.</remarks>
        public virtual bool Save(IDocument document)
        {
            if (IsUntitled(document))
                return SaveAs(document);

            if (!document.Dirty)
                return true;

            return SafeSave(document, DocumentEventType.Saved);
        }

        /// <summary>
        /// Saves the document under a new name, chosen by the user</summary>
        /// <param name="document">Document to save</param>
        /// <returns>True if document was successfully saved and false if the user cancelled
        /// or there was some kind of problem</returns>
        /// <remarks>All exceptions are caught and reported via OnSaveException().</remarks>
        public virtual bool SaveAs(IDocument document)
        {
            IDocumentClient client = GetClient(document);

            string oldFilePath = document.Uri.LocalPath;
            string newFilePath = PromptUserForNewFilePath(client, oldFilePath, document);

            if (newFilePath != null)
            {
                // change file dialog service's initial directory
                FileDialogService.InitialDirectory = Path.GetDirectoryName(newFilePath);

                // restore old URI in case of failure
                Uri oldUri = document.Uri;
                Uri newUri = new Uri(newFilePath);
                document.Uri = newUri;

                bool success = SafeSave(document, DocumentEventType.SavedAs);

                if (success)
                {
                    // remove and replace the document to signal that this is a new document
                    DocumentRegistry.Remove(document);
                    DocumentRegistry.ActiveDocument = document;

                    m_untitledDocuments.Remove(document); // in case it was untitled
                    m_newDocumentPaths.Remove(oldFilePath);
                }
                else
                {
                    document.Uri = oldUri;
                }

                return success;
            }

            return false;
        }

        /// <summary>
        /// Saves the document to the specified URI</summary>
        /// <param name="document">Document to save</param>
        /// <param name="uri">New document URI</param>
        /// <returns>True if document was successfully saved and false if the user cancelled
        /// or there was some kind of problem</returns>
        /// <remarks>All exceptions are caught and reported via OnSaveException().</remarks>
        public virtual bool SaveAs(IDocument document, Uri uri)
        {
            string oldFilePath = document.Uri.LocalPath;

            // change file dialog service's initial directory
            FileDialogService.InitialDirectory = Path.GetDirectoryName(uri.LocalPath);

            // restore old URI in case of failure
            Uri oldUri = document.Uri;
            document.Uri = uri;

            bool success = SafeSave(document, DocumentEventType.SavedAs);

            if (success)
            {
                // remove and replace the document to signal that this is a new document
                DocumentRegistry.Remove(document);
                DocumentRegistry.ActiveDocument = document;

                m_untitledDocuments.Remove(document); // in case it was untitled
                m_newDocumentPaths.Remove(oldFilePath);
            }
            else
            {
                document.Uri = oldUri;
            }

            return success;
        }

        /// <summary>
        /// Saves all documents</summary>
        /// <param name="cancelOnFail">True means remaining saves should be cancelled
        /// if one fails</param>
        /// <returns>True if all the documents were saved and false if the user cancelled
        /// or there was some kind of problem with one or more of the documents</returns>
        public virtual bool SaveAll(bool cancelOnFail)
        {
            bool allSaved = true;
            foreach (IDocument document in DocumentRegistry.Documents)
            {
                if (!Save(document))
                {
                    allSaved = false;
                    if (cancelOnFail)
                        break;
                }
            }

            return allSaved;
        }

        /// <summary>
        /// Closes the document</summary>
        /// <param name="document">Document to close</param>
        /// <returns>True iff close was not cancelled by user</returns>
        public virtual bool Close(IDocument document)
        {
            if (document == null)
                return true;

            var args = new DocumentClosingEventArgs(document);
            DocumentClosing.Raise(this, args);

            bool closeConfirmed = args.Cancel == false && ConfirmClose(document);

            if (closeConfirmed)
            {
                IDocumentClient client = GetClient(document);
                OnDocumentClosing(document);

                client.Close(document);

                DocumentRegistry.Remove(document);
                m_untitledDocuments.Remove(document); // in case it was untitled
                m_newDocumentPaths.Remove(document.Uri.LocalPath); // probably not necessary, but seems like a good idea to clean-up

                OnDocumentClosed(document);
                DocumentClosed.Raise(this, new DocumentEventArgs(document, DocumentEventType.Closed));
            }

            return closeConfirmed;
        }

        /// <summary>
        /// Closes all documents</summary>
        /// <param name="masterDocument">Master document, or null if none. The master
        /// document is closed last.</param>
        /// <returns>True iff no close was cancelled by user</returns>
        public virtual bool CloseAll(IDocument masterDocument)
        {
            // Take a snapshot to avoid modifying the collection while enumerating
            List<IDocument> openDocs = new List<IDocument>(DocumentRegistry.Documents);

            // close all documents, and close master document last
            foreach (IDocument document in openDocs)
            {
                if (document != masterDocument)
                {
                    if (!Close(document))
                        return false;
                }
            }

            if (masterDocument != null)
                return Close(masterDocument);

            return true;
        }

        /// <summary>
        /// Determines if the given document represents an untitled document, i.e.,
        /// a new document that has not yet been saved</summary>
        /// <param name="document">Document whose URI is examined</param>
        /// <returns>True iff the document is untitled</returns>
        /// <remarks>The default behavior is that a document is considered untitled if
        /// it has never been saved.</remarks>
        public virtual bool IsUntitled(IDocument document)
        {
            if (document == null)
                throw new ArgumentNullException("document");

            return m_untitledDocuments.Contains(document);
        }

        /// <summary>
        /// Event that is raised after a document is opened</summary>
        public event EventHandler<DocumentEventArgs> DocumentOpened;

        /// <summary>
        /// Event that is raised before a document is saved</summary>
        public event EventHandler<DocumentEventArgs> DocumentSaving;

        /// <summary>
        /// Event that is raised after a document is saved</summary>
        public event EventHandler<DocumentEventArgs> DocumentSaved;

        /// <summary>
        /// Event thiat is raised before a document is closed. Can be cancelled.</summary>
        public event EventHandler<DocumentClosingEventArgs> DocumentClosing;

        /// <summary>
        /// Event that is raised after a document is closed</summary>
        public event EventHandler<DocumentEventArgs> DocumentClosed;

        #endregion

        /// <summary>
        /// Gets the sequence of untitled documents known to the service</summary>
        public IEnumerable<IDocument> UntitledDocuments
        {
            get { return m_untitledDocuments; }
        }

        /// <summary>
        /// Closes all open documents. Sets them to "non-dirty" to suppress
        /// user prompts.</summary>
        public void DiscardAll()
        {
            // Take a snapshot to avoid modifying the collection while enumerating
            List<IDocument> openDocs = new List<IDocument>(DocumentRegistry.Documents);

            // close all documents
            foreach (IDocument document in openDocs)
            {
                document.Dirty = false;
                if (!Close(document))
                {
                    throw new Exception("This code should never be reached");
                }
            }
        }

        /// <summary>
        /// Performs custom initialization</summary>
        protected virtual void Initialize()
        {
        }

        /// <summary>
        /// Performs any custom actions needed before loading a document</summary>
        /// <param name="uri">The URI of the document</param>
        /// <returns>True iff the document can be loaded</returns>
        protected virtual bool OnDocumentOpening(Uri uri)
        {
            return true;
        }

        /// <summary>
        /// Performs any custom actions needed after opening (loading) a document</summary>
        /// <param name="document">Opened (loaded) document</param>
        protected virtual void OnDocumentOpened(IDocument document)
        {
        }

        /// <summary>
        /// Performs any custom actions needed after a document save causes an exception</summary>
        /// <param name="ex">Open exception</param>
        /// <remarks>The default behavior is to swallow the exception and show an
        /// error message. Overriders should filter exceptions and rethrow those that
        /// aren't recoverable.</remarks>
        protected virtual void OnOpenException(Exception ex)
        {
            Outputs.WriteLine(
                OutputMessageType.Error,
                "There was a problem opening the file".Localize() + ": {0}",
                ex.Message);
        }

        /// <summary>
        /// Performs any custom actions needed before saving a document</summary>
        /// <param name="document">The document to save</param>
        /// <returns>True iff the document can be saved</returns>
        protected virtual bool OnDocumentSaving(IDocument document)
        {
            return true;
        }

        /// <summary>
        /// Performs any custom actions needed after saving a document</summary>
        /// <param name="document">Saved document</param>
        /// <returns>True iff custom actions completed successfully</returns>
        protected virtual bool OnDocumentSaved(IDocument document)
        {
            return true;
        }

        /// <summary>
        /// Performs any custom actions needed after a document save causes an exception</summary>
        /// <param name="ex">Save exception</param>
        /// <remarks>The default behavior is to swallow the exception and show an
        /// error message. Overriders should filter exceptions and rethrow those that
        /// are not recoverable.</remarks>
        protected virtual void OnSaveException(Exception ex)
        {
            Outputs.WriteLine(
                OutputMessageType.Error,
                "There was a problem saving the file".Localize() + ": {0}",
                ex.Message);
        }

        /// <summary>
        /// Performs any custom actions needed before closing a document</summary>
        /// <param name="document">The document being closed</param>
        protected virtual void OnDocumentClosing(IDocument document)
        {
        }

        /// <summary>
        /// Performs any custom actions needed after closing a document</summary>
        /// <param name="document">The document that was closed</param>
        protected virtual void OnDocumentClosed(IDocument document)
        {
        }

        /// <summary>
        /// Confirms closing document</summary>
        /// <param name="document">document to confirm</param>
        /// <returns>true if document can be closed</returns>
        protected virtual bool ConfirmClose(IDocument document)
        {
            if (document == null)
                return true;

            bool closeConfirmed = true;
            if (document.Dirty)
            {
                string message = string.Format("Save {0}?".Localize(), document.Uri.LocalPath);

                DialogResult result = FileDialogService.ConfirmFileClose(message);
                if (result == DialogResult.Yes)
                {
                    closeConfirmed = Save(document);
                }
                else if (result == DialogResult.No)
                {
                    // mark it clean, so user isn't prompted again (when the document's window
                    //  closes, for example).
                    document.Dirty = false;
                }
                else if (result == DialogResult.Cancel)
                {
                    closeConfirmed = false;
                }
            }
            return closeConfirmed;
        }

        /// <summary>
        /// Gets the path name for a new document</summary>
        /// <param name="client">Document client</param>
        /// <returns>URI representing the new path, or null if the user cancelled</returns>
        protected virtual Uri GetNewDocumentUri(IDocumentClient client)
        {
            Uri uri = null;
            string fileName = client.Info.NewDocumentName;
            if (client.Info.Extensions.Length > 1)
            {
                // Since there are multiple possible extensions, ask the user to pick a filename.
                string path = PromptUserForNewFilePath(client, fileName, null);
                if (path != null)
                {
                    try
                    {
                        if (File.Exists(path))
                            File.Delete(path);
                    }
                    catch (Exception e)
                    {
                        string message = string.Format(
                            "Failed to delete: {0}. Exception: {1}", path, e);
                        Outputs.WriteLine(OutputMessageType.Warning, message);
                    }

                    m_newDocumentPaths.Add(path);
                    uri = new Uri(path, UriKind.RelativeOrAbsolute);
                }
            }
            else if (client.Info.Extensions.Length == 1)
            {
                // Since there is only one possible extension, we can choose the new name (e.g., "Untitled.xml").
                string directoryName = client.Info.InitialDirectory;
                if (directoryName == null)
                    directoryName = FileDialogService.InitialDirectory;

                string extension = client.Info.Extensions[0];

                if (directoryName != null && extension != null)
                {
                    int suffix;
                    m_extensionSuffixes.TryGetValue(extension, out suffix);

                    // check the name to make sure there is no existing file with the same name
                    while (true)
                    {
                        string fullFileName = fileName;
                        if (suffix > 0)
                            fullFileName += "(" + (suffix + 1) + ")";

                        suffix++;

                        fullFileName += extension;

                        string fullPath = Path.Combine(directoryName, fullFileName);
                        if (!FileDialogService.PathExists(fullPath))
                        {
                            uri = new Uri(fullPath, UriKind.RelativeOrAbsolute);
                            break;
                        }
                    }

                    m_extensionSuffixes[extension] = suffix;
                }
            }

            return uri;
        }

        /// <summary>
        /// Finds the currently open document that matches the given URI</summary>
        /// <param name="uri">URI to find open document for</param>
        /// <returns>The currently open document or null, if no match is found</returns>
        protected IDocument FindOpenDocument(Uri uri)
        {
            foreach (IDocument openDocument in DocumentRegistry.Documents)
                if (openDocument.Uri.Equals(uri))
                    return openDocument;

            return null;
        }

        /// <summary>
        /// Gets the IDocumentClient that corresponds to a given document's Type. This document Type
        /// must have previously been found from an IDocumentClient.Info.FileType.</summary>
        /// <param name="document">Document whose Type property is used to find a matching IDocumentClient</param>
        /// <returns>IDocumentClient that corresponds to given document's Type</returns>
        /// <exception cref="InvalidOperationException">Document type without corresponding document client</exception>
        protected IDocumentClient GetClient(IDocument document)
        {
            IDocumentClient client;
            if (!m_typeToClientMap.TryGetValue(document.Type, out client))
            {
                throw new InvalidOperationException("Document type without corresponding document client");
            }
            return client;
        }

        /// <summary>
        /// Gets the IDocumentClient that is compatible with the given URI either because a file
        /// with that URI is already open or the URI's file extension is compatible with the
        /// IDocumentClient.Info.Extensions property</summary>
        /// <param name="uri">URI</param>
        /// <returns>Compatible IDocumentClient, or null if none is found</returns>
        protected IDocumentClient GetClient(Uri uri)
        {
            IDocument openDocument = FindOpenDocument(uri);
            if (openDocument != null)
                return GetClient(openDocument);

            string extension = Path.GetExtension(uri.LocalPath);

            foreach (Lazy<IDocumentClient> lazyClient in m_documentClients)
            {
                IDocumentClient client = lazyClient.Value;

                foreach (string ext in client.Info.Extensions)
                {
                    if (string.Compare(ext, extension, true) == 0)
                        return client;
                }
            }

            return null;
        }

        /// <summary>
        /// Shows the given message on the status control, if an IStatusService is available</summary>
        /// <param name="message">User-readable status message</param>
        protected void ShowStatus(string message)
        {
            if (m_statusService != null)
                m_statusService.ShowStatus(message);
        }

        private string PromptUserForNewFilePath(IDocumentClient client, string fileName, IDocument existingDocument)
        {
            string filePath = fileName;

            FileFilterBuilder fb = new FileFilterBuilder();
            string[] extensions = client.Info.Extensions;
            foreach (string extension in extensions)
                fb.AddFileType(client.Info.FileType, extension);
            if (extensions.Length > 1)
                fb.AddAllFilesWithExtensions();
            string filter = fb.ToString();

            FileDialogService.InitialDirectory = client.Info.InitialDirectory;
            if (FileDialogService.SaveFileName(ref filePath, filter) != DialogResult.OK)
                return null;

            if (!client.Info.IsCompatiblePath(filePath))
            {
                Outputs.WriteLine(OutputMessageType.Error, "File extension not supported".Localize());
                return null;
            }

            // Check that the document isn't already open
            Uri uri = new Uri(filePath);
            IDocument openDocument = FindOpenDocument(uri);
            if (openDocument != null &&
                openDocument != existingDocument)
            {
                Outputs.WriteLine(OutputMessageType.Error, "A file with that name is already open".Localize());
                return null;
            }

            return filePath;
        }

        // 'Safe' in the sense that all exceptions are caught (and reported via OnOpenException).
        private IDocument SafeOpen(IDocumentClient client, Uri uri)
        {
            IDocument document = null;
            try
            {
                if (client.CanOpen(uri))
                {
                    if (OnDocumentOpening(uri))
                    {
                        document = client.Open(uri);
                        if (document != null)
                        {
                            OnDocumentOpened(document);
                            DocumentOpened.Raise(this, new DocumentEventArgs(document, DocumentEventType.Opened));

                            DocumentRegistry.ActiveDocument = document;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // DAN: Added this - if an exception occurs during line:
                // DocumentRegistry.ActiveDocument = document; 
                // then we need to remove it
                if (DocumentRegistry.ActiveDocument == document)
                    DocumentRegistry.Remove(document);

                document = null;

                // Let's share the exception directly. We used to wrap it in another Exception
                //  object but this hides the actual exception in the error dialog.
                OnOpenException(ex);
            }

            return document;
        }

        // 'Safe' in the sense that all exceptions are caught (and reported via OnSaveException).
        // The implementer of IDocumentClient is responsible for ensuring that existing data is not lost
        //  if the save fails.
        protected bool SafeSave(IDocument document, DocumentEventType kind)
        {
            IsSaving = true;

            bool success = false;
            try
            {
                if (OnDocumentSaving(document))
                {
                    DocumentSaving.Raise(this, new DocumentEventArgs(document, kind));

                    IDocumentClient client = GetClient(document);
                    client.Save(document, document.Uri);

                    success = OnDocumentSaved(document);

                    if (success)
                    {
                        document.Dirty = false;
                        m_newDocumentPaths.Remove(document.Uri.LocalPath);
                        DocumentSaved.Raise(this, new DocumentEventArgs(document, kind));
                    }
                }
            }
            catch (Exception ex)
            {
                OnSaveException(ex);
            }

            IsSaving = false;

            return success;
        }

        #region ICommandClient Members

        /// <summary>
        /// Checks whether the client can do the command, if it handles it</summary>
        /// <param name="commandTag">Command to be done</param>
        /// <returns>True iff client can do the command</returns>
        public virtual bool CanDoCommand(object commandTag)
        {
            bool enabled = false;
            if (commandTag is StandardCommand)
            {
                switch ((StandardCommand)commandTag)
                {
                    case StandardCommand.FileClose:
                    case StandardCommand.FileSave:
                    case StandardCommand.FileSaveAs:
                    case StandardCommand.FileSaveAll:
                        enabled = DocumentRegistry.ActiveDocument != null;
                        break;
                }
            }
            else if (commandTag is FileCommandTag) // new, open command?
            {
                enabled = true;
            }

            return enabled;
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandTag">Command to be done</param>
        public virtual void DoCommand(object commandTag)
        {
            if (commandTag is StandardCommand)
            {
                IDocument activeDocument = DocumentRegistry.ActiveDocument;

                bool success;
                switch ((StandardCommand)commandTag)
                {
                    case StandardCommand.FileClose:
                        Close(activeDocument);
                        break;

                    case StandardCommand.FileSave:
                        success = Save(activeDocument);
                        if (success)
                            ShowStatus("Document Saved".Localize());
                        break;

                    case StandardCommand.FileSaveAs:
                        success = SaveAs(activeDocument);
                        if (success)
                            ShowStatus(string.Format("Document Saved As {0}".Localize(), activeDocument.Uri.LocalPath));
                        break;

                    case StandardCommand.FileSaveAll:
                        success = true;
                        List<IDocument> docs = new List<IDocument>(DocumentRegistry.Documents);
                        foreach (IDocument document in docs)
                        {
                            if (!Save(document))
                                success = false;
                        }
                        ShowStatus(
                            success ?
                            "All documents saved".Localize() :
                            "Couldn't save all documents".Localize());
                        break;
                }
            }
            else if (commandTag is FileCommandTag)
            {
                FileCommandTag fileCommandTag = (FileCommandTag)commandTag;
                IDocumentClient client = fileCommandTag.Editor;

                if (fileCommandTag.Command == Command.FileNew)
                    OpenNewDocument(client);
                else
                    OpenExistingDocument(client, null);
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="commandState">Command info to update</param>
        public virtual void UpdateCommand(object commandTag, CommandState commandState)
        {
        }

        #endregion

        /// <summary>
        /// True if a save operation is currently happening</summary>
        public bool IsSaving { get; private set; }

        /// <summary>
        /// Gets the command service used to register the standard file commands</summary>
        protected ICommandService CommandService { get; private set; }

        /// <summary>
        /// Gets the document registry used to iterate through open documents, and add and remove documents</summary>
        protected IDocumentRegistry DocumentRegistry { get; private set; }

        /// <summary>
        /// Gets the file dialog service used to present open and save dialog boxes</summary>
        protected IFileDialogService FileDialogService { get; private set; }

        private void RegisterClientCommands()
        {
            int clientCount = m_documentClients.Length;

            // for each document client, build file/new and file/open commands
            int index = 0;
            foreach (Lazy<IDocumentClient> lazy in m_documentClients)
            {
                IDocumentClient client = lazy.Value;

                Keys newShortcut = Keys.None;
                Keys openShortcut = Keys.None;

                if (index == 0)
                {
                    newShortcut = Keys.Control | Keys.N;
                    openShortcut = Keys.Control | Keys.O;
                }

                string newIconName = client.Info.NewIconName;
                if ((RegisterCommands & CommandRegister.FileNew) == CommandRegister.FileNew)
                {
                    CommandService.RegisterCommand(
                        new CommandInfo(
                            new FileCommandTag(Command.FileNew, client),
                            StandardMenu.File,
                            StandardCommandGroup.FileNew,
                            clientCount > 1 ?
                                "New".Localize("Name of a command") + '/' + client.Info.FileType :
                                string.Format("New {0}".Localize(), client.Info.FileType),
                            string.Format("Creates a new {0} document".Localize("{0} is the type of document to create"), client.Info.FileType),
                            newShortcut,
                            newIconName,
                            (!string.IsNullOrEmpty(newIconName)) ? CommandVisibility.All : CommandVisibility.Menu),
                        this);
                }

                string openIconName = client.Info.OpenIconName;
                if ((RegisterCommands & CommandRegister.FileOpen) == CommandRegister.FileOpen)
                {
                    CommandService.RegisterCommand(
                        new CommandInfo(
                            new FileCommandTag(Command.FileOpen, client),
                            StandardMenu.File,
                            StandardCommandGroup.FileNew,
                            clientCount > 1 ?
                                "Open".Localize("Name of a command") + '/' + client.Info.FileType :
                                string.Format("Open {0}".Localize(), client.Info.FileType),
                            string.Format("Open an existing {0} document".Localize(), client.Info.FileType),
                            openShortcut,
                            openIconName,
                            (!string.IsNullOrEmpty(openIconName)) ? CommandVisibility.All : CommandVisibility.Menu),
                        this);
                }

                index++;
            }
        }

        // custom commands for New, Open Document
        private enum Command
        {
            FileNew,
            FileOpen,
        }

        /// <summary>
        /// Associates command and client</summary>
        private struct FileCommandTag
        {
            public FileCommandTag(Command command, IDocumentClient client)
            {
                Command = command;
                Editor = client;
            }

            public Command Command;
            public readonly IDocumentClient Editor;
        }

        [Import(AllowDefault = true)]
        private IStatusService m_statusService;

        [ImportMany]
        private Lazy<IDocumentClient>[] m_documentClients;

        private readonly HashSet<IDocument> m_untitledDocuments = new HashSet<IDocument>();
        private readonly HashSet<string> m_newDocumentPaths = new HashSet<string>();
        private readonly Dictionary<string, IDocumentClient> m_typeToClientMap = new Dictionary<string, IDocumentClient>();
        private readonly Dictionary<string, int> m_extensionSuffixes = new Dictionary<string, int>();

        private CommandRegister m_registerCommands = CommandRegister.Default;
    }
}
