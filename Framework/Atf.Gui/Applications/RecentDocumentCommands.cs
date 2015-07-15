//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Input;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Service that provides menu commands to load recent documents</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(RecentDocumentCommands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class RecentDocumentCommands : ICommandClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="documentRegistry">Document registry</param>
        /// <param name="documentService">Document service</param>
        [ImportingConstructor]
        public RecentDocumentCommands(
            ICommandService commandService,
            IDocumentRegistry documentRegistry,
            IDocumentService documentService)
        {
            CommandService = commandService;
            documentRegistry.DocumentAdded += documentRegistry_DocumentAdded;
            m_documentService = documentService;
            documentRegistry.ActiveDocumentChanged += documentRegistry_ActiveDocumentChanged;
            m_recentDocuments.ItemRemoved += documentInfo_ItemRemoved;
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by setting up settings service and registering commands</summary>
        public virtual void Initialize()
        {
            if (m_settingsService != null)
            {
                BoundPropertyDescriptor recentDocumentCount =
                    new BoundPropertyDescriptor(this, () => RecentDocumentCount,
                        "Recent Files Count".Localize("Number of recent files to display in File Menu"), null,
                        "Number of recent files to display in File Menu".Localize());

                BoundPropertyDescriptor recentDocuments =
                    new BoundPropertyDescriptor(this, () => RecentDocumentsAsCsv, "RecentDocuments", null, null);

                // Using the type name so that derived classes don't lose old user settings
                m_settingsService.RegisterSettings("Sce.Atf.Applications.RecentDocumentCommands",
                    recentDocumentCount,
                    recentDocuments);

                m_settingsService.RegisterUserSettings(
                    "Documents".Localize(),
                    recentDocumentCount);
            }

            if (CommandService != null)
            {
                var commandInfo = new CommandInfo(Command.Pin,
                    StandardMenu.File,
                    null,
                    "Pin file".Localize("Pin active file to the recent files list"),
                    "Pin active file to the recent files list".Localize(),
                    Keys.None,
                    Resources.PinGreenImage,
                    CommandVisibility.Menu);

                CommandService.RegisterCommand(commandInfo, this);

                // Add an empty entry so the Recent Files menu shows up even if there are no 
                // files in the list. This gets removed as soon as a file is added.
                m_emptyMruCommandInfo = new CommandInfo(Command.EmptyMru,
                    StandardMenu.File,
                    StandardCommandGroup.FileRecentlyUsed,
                    "Recent Files".Localize() + "/(" + "empty".Localize() + ")",
                    "No entries in recent files list".Localize(),
                    Keys.None);
                CommandService.RegisterCommand(
                    m_emptyMruCommandInfo,
                    this);
            }
        }

        #endregion

        /// <summary>
        /// Gets the enumeration of information about recent documents</summary>
        public IEnumerable<RecentDocumentInfo> RecentDocuments
        {
            get { return m_recentDocuments; }
        }

        /// <summary>
        /// Gets or sets the list of file extensions that 
        /// can be shown in the recent documents list.
        /// If null (which is the default), all extensions are supported.</summary>
        public string[] RecentDocumentExtensions
        {
            get { return m_extensionFilter; }
            set { m_extensionFilter = value; }
        }

        /// <summary>
        /// The default number of recent documents that are saved</summary>
        public const int DefaultRecentDocumentCount = 8;

        /// <summary>
        /// The maximum number of recent documents that are saved</summary>
        public const int MaxRecentDocumentCount = 32;

        /// <summary>
        /// Gets or sets the number of recent documents that are retained</summary>
        // define a default value attribute so user can reset in the preference dialog
        [DefaultValue(DefaultRecentDocumentCount)]
        public int RecentDocumentCount
        {
            get { return m_recentDocuments.MaximumCount; }
            set
            {
                if (value < 1 || value > MaxRecentDocumentCount)
                    throw new ArgumentException("Must be between 1 and 32".Localize());

                m_recentDocuments.MaximumCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the recent documents as comma separated values</summary>
        public string RecentDocumentsAsCsv
        {
            get
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0",
                    System.Text.Encoding.UTF8.WebName, "yes"));
                XmlElement root = xmlDoc.CreateElement("DocumentServiceSettings");
                xmlDoc.AppendChild(root);

                foreach (RecentDocumentInfo info in m_recentDocuments.MostRecentOrder)
                {
                    if (info != null)
                    {
                        XmlElement infoElement = xmlDoc.CreateElement("info");
                        root.PrependChild(infoElement);

                        infoElement.SetAttribute("uri", info.Uri.LocalPath);
                        //infoElement.SetAttribute("type", info.Type); //it's a user-readable string which is localized
                        infoElement.SetAttribute("pinned", info.Pinned.ToString());
                    }
                }

                return xmlDoc.InnerXml;
            }
            set
            {
                // Remove existing documents, to unregister commands.
                // Copy array as we'll be modifying the collection
                RecentDocumentInfo[] array = m_recentDocuments.ToArray();
                foreach (RecentDocumentInfo info in array)
                    RemoveDocument(info);

                // Attempt to read the settings in the XML format.
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement root;
                try
                {
                    xmlDoc.LoadXml(value);
                    root = xmlDoc.DocumentElement;
                }
                catch (XmlException)
                {
                    root = null;
                }

                // Add the new info
                foreach (XmlNode info in root.GetElementsByTagName("info"))
                {
                    string uriString = info.Attributes["uri"].Value;
                    if (File.Exists(uriString))
                    {
                        Uri uri = new Uri(uriString, UriKind.RelativeOrAbsolute);
                        bool pinned = false;
                        XmlAttribute pinnedAttribute = info.Attributes["pinned"];
                        if (pinnedAttribute != null)
                            Boolean.TryParse(pinnedAttribute.Value, out pinned);
                        AddDocument(uri, null, pinned);
                    }
                }

                // There may not be an AutoDocumentService being used and/or the ActiveDocument may not
                //  be set by client code at start-up, so we should make sure the recently used documents appear.
                UpdateRecentFilesMenuItems();
            }
        }

        /// <summary>
        /// The maximum number of characters displayed when listing recent documents in menu.
        /// Any negative number indicates there is no maximum</summary>
        public virtual int MaxPathLength
        {
            get { return -1; }
        }

        #region ICommandClient Members

        /// <summary>
        /// Checks if the client can do the command</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>True if client can do the command</returns>
        public virtual bool CanDoCommand(object commandTag)
        {
            if (commandTag is RecentDocumentInfo) return true;

            if ((commandTag is Command) && ((Command)commandTag) == Command.Pin)
                return m_canPin; // Use a cached value to avoid file I/O spam.

            return false;
        }

        /// <summary>
        /// Does a command</summary>
        /// <param name="commandTag">Command</param>
        public virtual void DoCommand(object commandTag)
        {
            var iconClicked = false;
            if ((CommandService != null) && (CommandService is CommandServiceBase))
            {
                var commandServiceBase = CommandService as CommandServiceBase;
                iconClicked = commandServiceBase.IconClicked;
            }

            if ((commandTag is RecentDocumentInfo) && (!iconClicked))
            {
                // User clicked on a document in the MRU to open it.
                var info = (RecentDocumentInfo)commandTag;
                IDocument document = null;
                // info.Type is a user-readable string, so it may have been localized and so no
                //  longer matches. Try to find the client by matching the file extension.
                IDocumentClient client = FindClientFromUri(info.Uri);
                if (client != null && client.CanOpen(info.Uri))
                  document = m_documentService.OpenExistingDocument(client, info.Uri);
                
                // To-do: If the document can't be opened, it would probably be better to keep the
                //  document in the list and have a context menu command for removing the link, like
                //  Microsoft Word does.
                if (document == null &&
                    System.Diagnostics.Debugger.IsAttached == false)
                {
                    RemoveDocument(info);
                }
            }
            else
            {
                // User either clicked on the Pin command directly, or clicked the pin icon on a document 
                // entry in the MRU. Either way, treat this as a Pin command.
                RecentDocumentInfo info;
                if (commandTag is RecentDocumentInfo)
                {
                    info = (RecentDocumentInfo)commandTag;
                }
                else
                {
                    info = GetActiveRecentDocumentInfo();
                }

                if (info != null)
                {
                    info.Pinned = !info.Pinned;
                }
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state to update</param>
        public virtual void UpdateCommand(object commandTag, CommandState state)
        {
            bool invertPinImageOnMouseover = false;
            bool useGreenPin = true;
            var info = commandTag as RecentDocumentInfo;
            if (info != null)
            {
                // For the document entries in the MRU, the pin color reflects the pinned state of
                // the item. The exception is when the user mouses over the pin icon directly - then 
                // we invert the color of the pin to show that it's clickable and will modify the
                // pinned state of the document.
                invertPinImageOnMouseover = true;
                state.Text = info.Uri.LocalPath;
                if (!info.Pinned)
                {
                    useGreenPin = false;
                }
            }
            else if (commandTag is Command)
            {
                var command = (Command)commandTag;
                if (command == Command.Pin)
                {
                    // For the other pin commands, the pin color is the opposite of the active 
                    // document's pinned state.
                    var docInfo = GetActiveRecentDocumentInfo();
                    var stateText = "Pin active document".Localize();
                    if (docInfo != null)
                    {
                        useGreenPin = !docInfo.Pinned;
                        var docPath = docInfo.Uri.AbsolutePath;
                        if (MaxPathLength > 0 && docPath.Length > MaxPathLength)
                        {
                            docPath = docPath.Substring(docPath.Length - MaxPathLength);
                            while (!docPath.StartsWith("/"))
                                docPath = docPath.Substring(1);
                            docPath = "..." + docPath;
                        }
                        stateText = string.Format(
                            docInfo.Pinned ?
                                "Unpin {0}".Localize("{0} will be replaced with a file name") :
                                "Pin {0}".Localize("{0} will be replaced with a file name"),
                                docPath);
                    }
                    state.Text = stateText;
                }
                else if (command == Command.EmptyMru)
                {
                    return;
                }
            }

            var commandServiceBase = CommandService as CommandServiceBase;
            if (commandServiceBase != null)
            {
                var commandInfo = commandServiceBase.GetCommandInfo(commandTag);

                if (commandInfo != null)
                {
                    if (invertPinImageOnMouseover)
                    {
                        // Make sure user is mousing over this command.
                        if (commandServiceBase.MouseIsOverCommandIcon == commandInfo)
                        {
                            useGreenPin = !useGreenPin;
                        }
                    }

                    var imageName = useGreenPin ? Resources.PinGreenImage : Resources.PinGreyImage;
                    if (commandInfo.ImageName != imageName)
                    {
                        commandInfo.ImageName = imageName;
                        commandServiceBase.RefreshImage(commandInfo);
                    }
                }
            }
        }

        private RecentDocumentInfo GetActiveRecentDocumentInfo()
        {
            var infos = m_recentDocuments.Where(info => info.Uri.AbsolutePath == m_activeDocument);
            foreach (RecentDocumentInfo info in infos) // should just be one
            {
                return info;
            }
            return null;
        }

        #endregion

        /// <summary>
        /// Command tags that are used with CommandService</summary>
        public enum Command
        {
            /// <summary>
            /// "Pin file" command</summary>
            Pin,
            /// <summary>
            /// "Recent Files" command</summary>
            EmptyMru
        }

        private void documentInfo_ItemRemoved(object sender, ItemRemovedEventArgs<RecentDocumentInfo> e)
        {
            if (CommandService != null)
            {
                if (m_registeredRecentDocs.Contains(e.Item))
                {
                    CommandService.UnregisterCommand(e.Item, this);
                    m_registeredRecentDocs.Remove(e.Item);
                }
            }
        }

        // 'type' is the user-readable (and localizable) document type name; if it's null
        //  then it will be looked up from known document clients.
        private void AddDocument(Uri uri, string type, bool pinned)
        {
            if (type == null)
            {
                IDocumentClient client = FindClientFromUri(uri);
                type = client != null ? client.Info.FileType : string.Empty;
            }
            m_recentDocuments.ActiveItem = new RecentDocumentInfo(uri, type, pinned);
        }

        private IDocumentClient FindClientFromUri(Uri uri)
        {
            foreach (Lazy<IDocumentClient> client in m_documentClients)
                if (client.Value.CanOpen(uri))
                    return client.Value;
            return null;
        }

        private void documentRegistry_DocumentAdded(object sender, ItemInsertedEventArgs<IDocument> e)
        {
            IDocument document = e.Item;
            if (!File.Exists(document.Uri.LocalPath)) // only list documents that exist; exclude untitled new documents not saved yet
                return;

            if (CanAdd(document.Uri))
            {
                bool pinned = m_recentDocuments.GetPinnedState(document.Uri).GetValueOrDefault(false);
                AddDocument(document.Uri, document.Type, pinned);
                UpdateRecentFilesMenuItems();
            }
        }

        private void documentRegistry_ActiveDocumentChanged(object sender, EventArgs e)
        {
            var activeDocument = ((DocumentRegistry)sender).ActiveDocument;
            if (activeDocument != null)
            {
                m_activeDocument = activeDocument.Uri.AbsolutePath;
                m_canPin = File.Exists(m_activeDocument);
            }
            else
            {
                m_activeDocument = string.Empty;
                m_canPin = false;
            }

            UpdateRecentFilesMenuItems();
        }

        /// <summary>
        /// Updates recent file menu items</summary>
        protected virtual void UpdateRecentFilesMenuItems()
        {
            if (CommandService != null)
            {
                var newCommandInfos = new List<CommandInfo>();

                foreach (RecentDocumentInfo info in m_recentDocuments)
                {
                    if (m_registeredRecentDocs.Contains(info))
                    {
                        CommandService.UnregisterCommand(info, this);
                        m_registeredRecentDocs.Remove(info);
                    }
                }

                if (m_recentDocuments.Count > 0)
                {
                    // Remove the "empty" entry from the MRU, since we have at least one document now.
                    if (m_emptyMruCommandInfo != null)
                    {
                        CommandService.UnregisterCommand(m_emptyMruCommandInfo.CommandTag, this);
                        m_emptyMruCommandInfo = null;
                    }
                }

                foreach (RecentDocumentInfo info in m_recentDocuments.MostRecentOrder)
                {
                    if (!m_registeredRecentDocs.Contains(info))
                    {
                        string pathName = info.Uri.LocalPath;
                        // replace slashes, so command service doesn't create hierarchical menu; actual path
                        //  is set in our UpdateCommand method.
                        pathName = pathName.Replace("/", "-");
                        pathName = pathName.Replace("\\", "-");
                        var commandInfo = new CommandInfo(
                            info,
                            StandardMenu.File,
                            StandardCommandGroup.FileRecentlyUsed,
                            "Recent Files".Localize() + "/" + pathName,
                            "Open a recently used file".Localize(),
                            Keys.None);

                        commandInfo.ImageName = info.Pinned ? Resources.PinGreenImage : Resources.PinGreyImage;

                        commandInfo.ShortcutsEditable = false;
                        CommandService.RegisterCommand(
                            commandInfo,
                            this);
                        newCommandInfos.Add(commandInfo);
                        m_registeredRecentDocs.Add(info);
                    }
                }

                CommandInfos = newCommandInfos;
            }
        }

        /// <summary>
        /// Gets the command service</summary>
        protected ICommandService CommandService
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets command info</summary>
        protected IEnumerable<CommandInfo> CommandInfos
        {
            get;
            private set;
        }

        private void RemoveDocument(RecentDocumentInfo info)
        {
            m_recentDocuments.Remove(info);
        }

        private bool CanAdd(Uri fileUri)
        {
            bool extensionSupported = true;

            if (m_extensionFilter != null)
            {
                extensionSupported = false;
                string absolutePath = fileUri.AbsolutePath;
                foreach (string extension in m_extensionFilter)
                {
                    if (absolutePath.EndsWith(extension, true, CultureInfo.InvariantCulture))
                    {
                        extensionSupported = true;
                        break;
                    }
                }
            }

            return extensionSupported;
        }

        private readonly IDocumentService m_documentService;

#pragma warning disable 649 // Field is never assigned to and will always have its default value

        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService;

        [ImportMany]
        private Lazy<IDocumentClient>[] m_documentClients;

#pragma warning restore 649

        private string[] m_extensionFilter;
        private readonly PinnableActiveCollection<RecentDocumentInfo> m_recentDocuments =
            new PinnableActiveCollection<RecentDocumentInfo>(DefaultRecentDocumentCount);
        private readonly List<RecentDocumentInfo> m_registeredRecentDocs = new List<RecentDocumentInfo>(DefaultRecentDocumentCount);
        private string m_activeDocument;
        private bool m_canPin;
        private CommandInfo m_emptyMruCommandInfo = null;
    }
}
