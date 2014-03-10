//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;

using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Component that implements the standard Edit menu's Cut, Copy, Paste, and Delete commands</summary>
    [Export(typeof(StandardEditCommands))]
    [Export(typeof(IContextMenuCommandProvider))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class StandardEditCommands : ICommandClient, IContextMenuCommandProvider, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        [ImportingConstructor]
        public StandardEditCommands(ICommandService commandService, IContextRegistry contextRegistry)
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            // Register edit menu commands
            m_commandService.RegisterCommand(CommandInfo.EditCut, this);
            m_commandService.RegisterCommand(CommandInfo.EditCopy, this);
            m_commandService.RegisterCommand(CommandInfo.EditPaste, this);
            m_commandService.RegisterCommand(CommandInfo.EditDelete, this);
        }

        #endregion

        /// <summary>
        /// Gets or sets a value that determines whether to use the system clipboard; default
        /// is false</summary>
        public static bool UseSystemClipboard
        {
            get { return s_useSystemClipboard; }
            set { s_useSystemClipboard = value; }
        }

        /// <summary>
        /// Gets or sets the set of strings that represent system clipboard formats that
        /// should be ignored</summary>
        /// <remarks>For example, without this filter set, if UseSystemClipboard is true
        /// and MS Word has a bit of text in the clipboard, an ExternalException can
        /// be thrown that forces the application to shut down.</remarks>
        /// <remarks>Setting to null simply uses an empty list.</remarks>
        public static ICollection<string> SystemClipboardFormatsToIgnore
        {
            get { return s_clipboardFormatsToIgnore; }
            set { s_clipboardFormatsToIgnore = value ?? new List<string>(); }
        }

        /// <summary>
        /// Gets or sets the set of strings that represent system clipboard formats that
        /// should be accepted</summary>
        /// <remarks>Setting to null simply uses an empty list.</remarks>
        /// <remarks>If empty, all clipboard formats are accepted.</remarks>
        public static ICollection<string> AcceptableSystemClipboardFormats
        {
            get { return s_clipboardAcceptableFormats; }
            set { s_clipboardAcceptableFormats = value ?? new List<string>(); }
        }

        /// <summary>
        /// Gets or sets the data on the clipboard. If UseSystemClipboard is true, this
        /// is the system clipboard.</summary>
        public IDataObject Clipboard
        {
            get
            {
                if (s_useSystemClipboard)
                {
                    // Merge the system clipboard and the local clipboard because some editors may not
                    //  be able to serialize their data for the system clipboard yet other editors
                    //  within this same app may be able to.
                    try
                    {
                        // Calling GetDataObject on the system clipboard should be done only when necessary.
                        //  This is the cause of the infamous "chime" bug.
                        // See: http://sf.ship.scea.com/sf/go/artf37352 and http://sf.ship.scea.com/sf/go/artf37411 and
                        //  http://www.ericbt.com/Blog/138
                        uint currentClipboardNum = User32.GetClipboardSequenceNumber();
                        if (currentClipboardNum != s_clipboardNum)
                        {
                            s_clipboardNum = currentClipboardNum;
                            IDataObject systemClipboard = System.Windows.Forms.Clipboard.GetDataObject();
                            if (systemClipboard != null)
                            {
                                foreach (string format in systemClipboard.GetFormats())
                                {
                                    if (!ShouldWeCopySystemClipboardObjectToLocalClipboard(format))
                                        continue;

                                    object data;
                                    try
                                    {
                                        // Was seeing crashes from the GetData() call locally
                                        // so in an effort to figure out which format was causing
                                        // the crashes I added a bit of code to log the format
                                        // prior to the GetData() call and then after the GetData()
                                        // call. (PJO)
                                        LogClipboardFormat(format, true);
                                        data = systemClipboard.GetData(format, true);
                                    }
                                    finally
                                    {
                                        LogClipboardFormat(format, false);
                                    }

                                    s_clipboard.SetData(format, data);
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // no recovery possible
                    }
                }

                return s_clipboard;
            }
            set
            {
                if (s_useSystemClipboard)
                {
                    // add new data to the system clipboard
                    try
                    {
                        System.Windows.Forms.Clipboard.SetDataObject(value);
                        s_clipboardNum = User32.GetClipboardSequenceNumber();
                    }
                    catch (Exception)
                    {
                        // no recovery possible
                    }
                }

                s_clipboard = value;
            }
        }

        /// <summary>
        /// Event that is raised before copying from the current instancing context</summary>
        public static event EventHandler Copying;

        /// <summary>
        /// Event that is raised after copying from the current instancing context</summary>
        public static event EventHandler Copied;

        /// <summary>
        /// Event that is raised after pasting from the clipboard</summary>
        public static event EventHandler Pasted;

        /// <summary>
        /// Event that is raised before deleting from the current instancing context</summary>
        public static event EventHandler Deleting;

        /// <summary>
        /// Event that is raised after deleting from the current instancing context</summary>
        public static event EventHandler Deleted;

        /// <summary>
        /// Returns whether the active context can paste from the clipboard</summary>
        /// <returns>True iff the active context can paste from the clipboard</returns>
        public bool CanPaste()
        {
            IInstancingContext instancingContext = m_contextRegistry.GetActiveContext<IInstancingContext>();
            return
                instancingContext != null &&
                instancingContext.CanInsert(Clipboard);
        }

        /// <summary>
        /// Returns a value indicating if the active context can copy the selection to the clipboard</summary>
        /// <returns>True iff the active context can copy to the clipboard</returns>
        public bool CanCopy()
        {
            IInstancingContext instancingContext = m_contextRegistry.GetActiveContext<IInstancingContext>();
            return
                instancingContext != null &&
                instancingContext.CanCopy();
        }

        /// <summary>
        /// Returns a value indicating if the active context can delete the selection</summary>
        /// <returns>True iff the active context can delete the selection</returns>
        public bool CanDelete()
        {
            IInstancingContext instancingContext = m_contextRegistry.GetActiveContext<IInstancingContext>();
            return
                instancingContext != null &&
                instancingContext.CanDelete();
        }

        /// <summary>
        /// Copies the active context's selection to the clipboard</summary>
        public void Copy()
        {
            IInstancingContext instancingContext = m_contextRegistry.GetActiveContext<IInstancingContext>();
            if (instancingContext != null &&
                instancingContext.CanCopy())
            {
                object rawObject = instancingContext.Copy();
                IDataObject dataObject = rawObject as IDataObject ?? new DataObject(rawObject);

                OnCopying(EventArgs.Empty);

                Clipboard = dataObject;

                OnCopied(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Copies the active context's selection to the clipboard and deletes it</summary>
        public void Cut()
        {
            Copy();
            Delete("Cut".Localize("Cut the selection"));
        }

        /// <summary>
        /// Deletes the active context's selection</summary>
        public void Delete()
        {
            Delete("Delete".Localize());
        }

        /// <summary>
        /// Paste's the clipboard into the active context</summary>
        public void Paste()
        {
            IInstancingContext instancingContext = m_contextRegistry.GetActiveContext<IInstancingContext>();
            if (instancingContext != null &&
                instancingContext.CanInsert(Clipboard))
            {
                ITransactionContext transactionContext = instancingContext.As<ITransactionContext>();
                transactionContext.DoTransaction(
                    delegate
                    {
                        instancingContext.Insert(Clipboard);
                    }, CommandInfo.EditPaste.MenuText);

                OnPasted(EventArgs.Empty);
            }         
        }

        #region ICommandClient Members

        /// <summary>
        /// Checks whether the client can do the command, if it handles it</summary>
        /// <param name="commandTag">Command to be done</param>
        /// <returns>True iff client can do the command</returns>
        bool ICommandClient.CanDoCommand(object commandTag)
        {
            bool canDo = false;

            IInstancingContext instancingContext = m_contextRegistry.GetActiveContext<IInstancingContext>();
            switch ((StandardCommand)commandTag)
            {
                case StandardCommand.EditCut:
                    canDo =
                        instancingContext != null &&
                        instancingContext.CanCopy() &&
                        instancingContext.CanDelete();
                    break;

                case StandardCommand.EditDelete:
                    canDo =
                        instancingContext != null &&
                        instancingContext.CanDelete();
                    break;

                case StandardCommand.EditCopy:
                    canDo =
                        instancingContext != null &&
                        instancingContext.CanCopy();
                    break;

                case StandardCommand.EditPaste:
                    canDo =
                        instancingContext != null &&
                        CanPaste();
                    break;
            }

            return canDo;
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandTag">Command to be done</param>
        void ICommandClient.DoCommand(object commandTag)
        {
            switch ((StandardCommand)commandTag)
            {
                case StandardCommand.EditCut:
                    Cut();
                    break;

                case StandardCommand.EditDelete:
                    Delete();
                    break;

                case StandardCommand.EditCopy:
                    Copy();
                    break;

                case StandardCommand.EditPaste:
                    Paste();
                    break;
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="commandState">Command info to update</param>
        void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
        {
        }

        #endregion

        #region IContextMenuCommandProvider Members

        /// <summary>
        /// Gets tags for context menu (right click) commands</summary>
        /// <param name="context">Context containing target object</param>
        /// <param name="clicked">Right clicked object, or null if none</param>
        IEnumerable<object> IContextMenuCommandProvider.GetCommands(object context, object clicked)
        {
            ISelectionContext selectionContext = context.As<ISelectionContext>();
            IInstancingContext instancingContext = context.As<IInstancingContext>();
            if ((selectionContext != null) && (instancingContext != null))
            {
                return new object[]
                    {
                        StandardCommand.EditCut,
                        StandardCommand.EditCopy,
                        StandardCommand.EditPaste,
                        StandardCommand.EditDelete,
                    };
            }

            return EmptyEnumerable<object>.Instance;
        }

        #endregion

        /// <summary>
        /// Raises the Copying event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnCopying(EventArgs e)
        {
            Copying.Raise(this, e);
        }

        /// <summary>
        /// Raises the Copied event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnCopied(EventArgs e)
        {
            Copied.Raise(this, e);
        }

        /// <summary>
        /// Raises the Pasted event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnPasted(EventArgs e)
        {
            Pasted.Raise(this, e);
        }

        /// <summary>
        /// Raises the Deleting event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnDeleting(EventArgs e)
        {
            Deleting.Raise(this, e);
        }

        /// <summary>
        /// Raises the Deleted event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnDeleted(EventArgs e)
        {
            Deleted.Raise(this, e);
        }

        private void Delete(string commandName)
        {
            OnDeleting(EventArgs.Empty);

            IInstancingContext instancingContext = m_contextRegistry.GetActiveContext<IInstancingContext>();
            if (instancingContext != null &&
                instancingContext.CanDelete())
            {
                ITransactionContext transactionContext = instancingContext.As<ITransactionContext>();
                transactionContext.DoTransaction(
                    delegate
                    {
                        instancingContext.Delete();

                        ISelectionContext selectionContext = instancingContext.As<ISelectionContext>();
                        if (selectionContext != null)
                            selectionContext.Clear();
                    }, commandName);
            }

            OnDeleted(EventArgs.Empty);
        }

        protected ICommandService CommandService { get { return m_commandService; } }
        protected IContextRegistry ContextRegistry { get { return m_contextRegistry; } }

        private readonly ICommandService m_commandService;
        private readonly IContextRegistry m_contextRegistry;

        #region Clipboard Format Logging

        private class ClipboardLoggingData
        {
            public ClipboardLoggingData()
            {
                LogFile =
                    string.Format(
                        LogFileFormatString,
                        DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            }

            public bool AddFormat(string format)
            {
                if (m_formats.Contains(format))
                    return false;

                m_formats.Add(format);
                return true;
            }

            public string LogFile { get; private set; }

            private readonly List<string> m_formats =
                new List<string>();

            private const string LogFileFormatString = "AtfClipboardLogging_{0}.log";
        }

        private static ClipboardLoggingData s_clipboardLoggingData;

        /// <summary>
        /// Logs clipboard formats to a file</summary>
        /// <remarks>Only works if CLIPBOARD_LOGGING is defined</remarks>
        /// <param name="format">Clipboard format</param>
        /// <param name="start">Mark as beginning in string</param>
        [System.Diagnostics.ConditionalAttribute("CLIPBOARD_LOGGING")]
        private static void LogClipboardFormat(string format, bool start)
        {
            if (s_clipboardLoggingData == null)
                s_clipboardLoggingData = new ClipboardLoggingData();

            var formatToWrite = string.Format("{0} Format: {1}", start ? "[BEG]" : "[END]", format);

            if (!s_clipboardLoggingData.AddFormat(formatToWrite))
                return;

            try
            {
                using (var stream = File.Open(s_clipboardLoggingData.LogFile, FileMode.Append, FileAccess.Write, FileShare.Write))
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.WriteLine(formatToWrite);
                        writer.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        #endregion

        /// <summary>
        /// Constructor</summary>
        static StandardEditCommands()
        {
            // Make sure that they are not both zero when starting up.
            s_clipboardNum = User32.GetClipboardSequenceNumber() - 1;
        }

        /// <summary>
        /// Checks if should copy system clipboard object to local clipboard</summary>
        /// <remarks>If not using the system clipboard, return value is always false.
        /// Really long function name but wanted to be explicit!</remarks>
        /// <param name="dataFormat">Data format to check</param>
        /// <returns>True iff the system clipboard object should be copied to the local clipboard</returns>
        private static bool ShouldWeCopySystemClipboardObjectToLocalClipboard(string dataFormat)
        {
            // Determine whether an object should be copied from
            // the system clipboard to the local clipboard

            // If not using the system clipboard we don't care
            if (!UseSystemClipboard)
                return false;
            
            // If the accept list isn't empty we check it first
            if (AcceptableSystemClipboardFormats.Count > 0)
            {
                // Always copy the item if it's in the accept list
                if (AcceptableSystemClipboardFormats.Contains(dataFormat))
                    return true;
            }

            // Fallback and only copy the item if it's not in the ignore list
            return !SystemClipboardFormatsToIgnore.Contains(dataFormat);
        }

        private static bool s_useSystemClipboard;
        private static IDataObject s_clipboard = new DataObject();
        private static uint s_clipboardNum;
        private static ICollection<string> s_clipboardFormatsToIgnore =
            new[]
            {
                "EnhancedMetafile",     // due to MS Word causing an ExternalException exception
                "Link Source",          // due to MS Word causing an ExternalException exception
                "Hyperlink",            // due to MS Word causing an ExternalException exception

                // Some additional formats exlucded based on this site:
                // http://bytes.com/topic/c-sharp/answers/636880-how-change-ms-word-data-byte

                "MetaFilePict", 
                "Embed Source",
                "Link Source Descriptor",
                "ObjectLink"
            }; // If this set gets large, consider using HashSet

        private static ICollection<string> s_clipboardAcceptableFormats =
            new[]
            {
                DataFormats.FileDrop,
                DataFormats.CommaSeparatedValue,
                DataFormats.Html,
                DataFormats.OemText,
                DataFormats.Rtf,
                DataFormats.Serializable,
                DataFormats.Text,
                DataFormats.UnicodeText
            };
    }
}
