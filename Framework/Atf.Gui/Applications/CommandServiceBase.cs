//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Xml;

using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Input;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Service to handle commands in menus and toolbars</summary>
    public abstract class CommandServiceBase : ICommandService, ICommandClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        public CommandServiceBase()
        {
            // create built-in menus and commands first
            RegisterMenuInfo(MenuInfo.File);
            RegisterMenuInfo(MenuInfo.Edit);
            RegisterMenuInfo(MenuInfo.View);
            RegisterMenuInfo(MenuInfo.Modify);
            RegisterMenuInfo(MenuInfo.Format);
            RegisterMenuInfo(MenuInfo.Window);
            RegisterMenuInfo(MenuInfo.Help);
        }

        #region IInitializable Members

        public virtual void Initialize()
        {
            if (m_settingsService != null)
            {
                // create setting to store command shortcuts
                m_settingsService.RegisterSettings(this,
                    new BoundPropertyDescriptor(
                        this, () => CommandShortcuts, "Keyboard Shortcuts".Localize(), null, null)
                );

                PropertyDescriptor[] userPrefs = new PropertyDescriptor[]
                {
                    // setting that allows user to set toolbar image size.
                    new BoundPropertyDescriptor(
                        this, () => UserSelectedImageSize, "Command Icon Size".Localize(), null, "Size of icons on Toolbar buttons".Localize())
                };
                m_settingsService.RegisterSettings(this, userPrefs);
                SettingsServices.RegisterUserSettings(m_settingsService, "Application".Localize(), userPrefs);
            }

            // Register our own "edit keyboard" command
            this.RegisterCommand(
                CommandId.EditKeyboard,
                StandardMenu.Edit,
                StandardCommandGroup.EditPreferences,
                "Keyboard Shortcuts".Localize() + " ...",
                "Customize keyboard shortcuts".Localize(),
                this);
        }

        #endregion

        #region ICommandService Members

        /// <summary>
        /// Registers the menu for the application and gives it a tool strip</summary>
        /// <param name="menuInfo">Menu description; standard menus are defined as static members
        /// of the MenuInfo class</param>
        public void RegisterMenu(MenuInfo menuInfo)
        {
            RegisterMenuInfo(menuInfo);
        }

        /// <summary>
        /// Registers a command for a command client</summary>
        /// <param name="info">Command description; standard commands are defined as static
        /// members of the CommandInfo class</param>
        /// <param name="client">Client that handles the command</param>
        public virtual void RegisterCommand(CommandInfo info, ICommandClient client)
        {
            if (client == null)
                throw new InvalidOperationException("Command has no client");

            CommandInfo duplicate = GetCommandInfo(info.CommandTag);
            if (duplicate == null)
            {
                if (!CommandIsUnique(info.MenuTag, info.MenuText))
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Duplicate menu/command combination. CommandTag: {0}, MenuTag: {1}, GroupTag: {2}, MenuText: {3}",
                            info.CommandTag, info.GroupTag, info.MenuTag, info.MenuText));
                }

                RegisterCommandInfo(info);
            }

            IncrementMenuCommandCount(info.MenuTag);
            m_commandClients.Add(info.CommandTag, client);
        }

        /// <summary>
        /// Unregisters a command for a command client</summary>
        /// <param name="commandTag">Command tag that identifies CommandInfo used to register
        /// the command</param>
        /// <param name="client">Client that handles the command</param>
        public virtual void UnregisterCommand(object commandTag, ICommandClient client)
        {
            if (client == null)
                m_commandClients.Remove(commandTag);
            else
                m_commandClients.Remove(commandTag, client);

            CommandInfo info = GetCommandInfo(commandTag);
            if (info == null)
                return;
            UnregisterCommandInfo(info);

            if (info.MenuTag != null)
            {
                MenuInfo menuInfo = GetMenuInfo(info.MenuTag);
                if (menuInfo != null)
                    DecrementMenuCommandCount(menuInfo);
            }
        }

        /// <summary>
        /// Displays a context (right-click popup) menu at the given screen point. Raises
        /// the ContextMenuClosed events.</summary>
        /// <param name="commandTags">Commands in menu, nulls indicate separators</param>
        /// <param name="screenPoint">Point in screen coordinates</param>
        public abstract void RunContextMenu(IEnumerable<object> commandTags, Point screenPoint);

        /// <summary>
        /// Sets the active client that receives a command for the case when multiple
        /// ICommandClient objects have registered for the same command tag (such as the
        /// StandardCommand.EditCopy enum, for example). Set to null to reduce the priority
        /// of the previously active client.</summary>
        /// <param name="client">Command client, null if client is deactivated</param>
        public void SetActiveClient(ICommandClient client)
        {
            List<object> commandTags = new List<object>(m_commandClients.Keys);

            // 'client' being null is an indication to pop the most recently active client
            if (client == null && m_activeClient != null)
            {
                // make sure previous client will NOT be the last registered for its command tags
                foreach (object commandTag in commandTags)
                {
                    if (m_commandClients.ContainsKeyValue(commandTag, m_activeClient))
                        m_commandClients.AddFirst(commandTag, m_activeClient);
                }
            }

            m_activeClient = client;

            if (m_activeClient != null)
            {
                // make sure client will be the last registered for its command tags
                foreach (object commandTag in commandTags)
                {
                    if (m_commandClients.ContainsKeyValue(commandTag, client))
                        m_commandClients.Add(commandTag, client);
                }
            }
        }

        /// <summary>
        /// Reserves a shortcut key, so it is not available as command shortcut</summary>
        /// <param name="key">Reserved key</param>
        /// <param name="reason">Reason why key is reserved to display to user</param>
        public void ReserveKey(Keys key, string reason)
        {
            if (key == Keys.None)
                throw new ArgumentException("key");
            if (reason == null)
                throw new ArgumentNullException("reason");

            // add or update key to reserved keys.
            key = KeysUtil.NumPadToNum(key);
            if (m_reservedKeys.ContainsKey(key))
            {
                m_reservedKeys[key] = reason;
            }
            else
            {
                m_reservedKeys[key] = reason;
                EraseShortcut(key);
            }
        }

        /// <summary>
        /// Processes the key as a command shortcut</summary>
        /// <param name="key">Key to process</param>
        /// <returns>True iff the key was processed as a command shortcut</returns>
        public virtual bool ProcessKey(Keys key)
        {
            KeyEventArgs keyEventArgs = new KeyEventArgs(key);
            ProcessingKey.Raise(this, keyEventArgs);
            if (keyEventArgs.Handled)
                return true;

            Keys shortcut = KeysUtil.NumPadToNum(key);

            //if there is no key, return
            if (shortcut == Keys.None)
                return false;

            //if the key is not a registered shortcut, return
            object tag;
            if (!m_shortcuts.TryGetValue(shortcut, out tag))
                return false;

            //Is there a client, and if so, can the client do the command?
            ICommandClient client = GetClient(tag);
            if (client == null)
                client = m_activeClient;
            if (client == null || !client.CanDoCommand(tag))
                return false;

            // do the command
            client.DoCommand(tag);
            return true;
        }

        /// <summary>
        /// Event that is raised when processing a key; clients can subscribe to this event
        /// to intercept certain hot keys for custom handling</summary>
        public event EventHandler<KeyEventArgs> ProcessingKey;

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Checks whether the client can do the command, if it handles it</summary>
        /// <param name="commandTag">Command to be done</param>
        /// <returns>True iff client can do the command</returns>
        public virtual bool CanDoCommand(object commandTag) { return false; }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandTag">Command to be done</param>
        public virtual void DoCommand(object commandTag) { }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state to update</param>
        public virtual void UpdateCommand(object commandTag, CommandState state) { }

        #endregion

        /// <summary>
        /// Image sizes for toolbar icons</summary>
        public enum ImageSizes
        {
            /// <summary>
            /// 16 x 16 Icon</summary>
            Size16x16,

            /// <summary>
            /// 24 x 24 Icon</summary>
            Size24x24,

            /// <summary>
            /// 32 x 32 Icon</summary>
            Size32x32
        }

        /// <summary>
        /// Gets and sets toolbar image size</summary>
        [DefaultValue(ImageSizes.Size24x24)]
        public ImageSizes UserSelectedImageSize
        {
            get { return m_imageSize; }
            set
            {
                if (m_imageSize != value)
                {
                    m_imageSize = value;
                    OnImageSizeChanged();
                }
            }
        }

        /// <summary>
        /// Handler for image size changed event for derived classes to override</summary>
        protected virtual void OnImageSizeChanged() { }

        /// <summary>
        /// Gets and sets XML string representing command/shortcut pairs</summary>
        public string CommandShortcuts
        {
            get
            {
                // generate xml string consisting of command, shortcut pairs
                // use menu text as a unique id since it is more easily serialized
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
                XmlElement root = xmlDoc.CreateElement("Shortcuts");
                xmlDoc.AppendChild(root);

                foreach (CommandInfo info in m_commands)
                {
                    if (IsUnregistered(info))
                        continue;

                    // We don't want to save shortcuts that are at their default value since this
                    //  prevents the default from being changed programmatically or via DefaultSettings.xml.
                    //  http://forums.ship.scea.com/jive/thread.jspa?messageID=51034
                    if (info.ShortcutsAreDefault)
                        continue;

                    string commandPath = GetCommandPath(info);
                    int numShortcuts = 0;
                    foreach (Keys k in info.Shortcuts)
                    {
                        XmlElement elem = xmlDoc.CreateElement("shortcut");
                        elem.SetAttribute("name", commandPath);
                        elem.SetAttribute("value", k.ToString());
                        root.AppendChild(elem);
                        numShortcuts++;
                    }

                    if (numShortcuts < 1)
                    {
                        XmlElement elem = xmlDoc.CreateElement("shortcut");
                        elem.SetAttribute("name", commandPath);
                        elem.SetAttribute("value", Keys.None.ToString());
                        root.AppendChild(elem);
                    }
                }

                return xmlDoc.InnerXml;
            }
            set
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(value);
                XmlNodeList nodes = xmlDoc.DocumentElement.SelectNodes("shortcut");
                if (nodes == null || nodes.Count == 0)
                    return;

                Dictionary<string, CommandInfo> commands = new Dictionary<string, CommandInfo>(m_commands.Count);
                foreach (CommandInfo info in m_commands)
                {
                    if (IsUnregistered(info))
                        continue;

                    string commandPath = GetCommandPath(info);
                    commands.Add(commandPath, info);
                }

                Dictionary<CommandInfo, CommandInfo> changedCommands = new Dictionary<CommandInfo, CommandInfo>(m_commands.Count);

                // m_shortcuts contains the default shortcuts currently. We need to override the defaults with
                //  the user's preferences. The preference file does not necessarily contain all shortcuts and
                //  some of the shortcuts may be blank (i.e., Keys.None).
                foreach (XmlElement elem in nodes)
                {
                    string strCmdTag = elem.GetAttribute("name"); //the command tag or "path", made up of menu and command name
                    string strShortcut = elem.GetAttribute("value");

                    if (commands.ContainsKey(strCmdTag))
                    {
                        // Blow away any old shortcuts before adding the first new one
                        CommandInfo cmdInfo = commands[strCmdTag];
                        if (!changedCommands.ContainsKey(cmdInfo))
                        {
                            List<Keys> shortcuts = new List<Keys>(cmdInfo.Shortcuts);
                            foreach (Keys k in shortcuts)
                                EraseShortcut(k);
                            changedCommands.Add(cmdInfo, cmdInfo);
                        }
                        Keys shortcut = (Keys)Enum.Parse(typeof(Keys), strShortcut);
                        shortcut = KeysUtil.NumPadToNum(shortcut);
                        SetShortcut(shortcut, commands[strCmdTag]);
                    }
                }
            }
        }

        /// <summary>
        /// Gets and sets whether the context menu should automatically remove disabled items</summary>
        public bool ContextMenuAutoCompact
        {
            get { return m_contextMenuAutoCompact; }
            set { m_contextMenuAutoCompact = value; }
        }

        /// <summary>
        /// Obtains the registered menu info whose menu tag object equals the menuTag parameter</summary>
        /// <param name="menuTag">Menu's unique ID to compare against known menu tags</param>
        /// <returns>The corresponding matching menu info, or null, if no match was found</returns>
        public MenuInfo GetMenuInfo(object menuTag)
        {
            MenuInfo result = null;
            foreach (MenuInfo menuInfo in m_menus)
            {
                if (menuInfo.MenuTag.Equals(menuTag))
                {
                    result = menuInfo;
                    break;
                }
            }

            return result;
        }


        /// <summary>
        /// Obtains menu info for a context menu if the menu tag is not null and can be converted
        /// to a non-empty string</summary>
        /// <remarks>The menu tag is optional for context menus, but defining one allows the client
        /// application to register commands with the same menu texts in different contexts.</remarks>
        /// <param name="menuTag">Menu's unique ID (convertable to non-empty string)</param>
        /// <returns>Menu info for a context menu</returns>
        private MenuInfo GetContextMenuInfo(object menuTag)
        {
            MenuInfo result = null;

            if (menuTag != null)
            {
                string menuText = menuTag.ToString();
                if (!string.IsNullOrEmpty(menuText))
                    result = new MenuInfo(menuTag, menuText, string.Empty);
            }

            return result;
        }

        /// <summary>
        /// Returns the registered CommandInfo whose command tag equals a given object</summary>
        /// <param name="commandTag">The object to compare against known CommandInfo objects</param>
        /// <returns>The corresponding registered CommandInfo, or null, if no match was found</returns>
        public CommandInfo GetCommandInfo(object commandTag)
        {
            CommandInfo result = null;
            if (commandTag != null)
            {
                m_commandsById.TryGetValue(commandTag, out result);
            }

            return result;
        }

        /// <summary>
        /// Gets the registered command info objects</summary>
        /// <returns></returns>
        public IEnumerable<CommandInfo> GetCommandInfos()
        {
            return m_commands;
        }

        /// <summary>
        /// Gets the command client for the given command tag, or null if none exists. If multiple
        /// command clients registered a command with this tag, the most recent command client
        /// is returned.</summary>
        /// <param name="commandtag">Command tag</param>
        /// <returns>Command client for given command tag</returns>
        public ICommandClient GetClient(object commandtag)
        {
            ICommandClient client;
            m_commandClients.TryGetLast(commandtag, out client);
            return client;
        }

        /// <summary>
        /// Registers menu info</summary>
        /// <param name="info">MenuInfo to register</param>
        /// <remarks>Adds this MenuInfo object to m_menus field and creates a tool strip for it.
        /// WARNING: This virtual method is called within the constructor. ONLY
        /// CLASSES THAT DIRECTLY DERIVE FROM COMMANDSERVICEBASE SHOULD OVERRIDE IT
        /// AND SHOULD DO SO USING THE 'SEALED' KEYWORD.</remarks>
        protected virtual void RegisterMenuInfo(MenuInfo info)
        {
            MenuInfo addedInfo = GetMenuInfo(info.MenuTag);
            if (addedInfo != null)
                throw new InvalidOperationException("Menu object '" + info.MenuTag + "' was already added");

            if (info.MenuTag is StandardMenu)
                m_menus.Add(info);
            else
                m_menus.Insert(m_menus.Count - 2, info); // insert custom menus before Window, Help
        }

        /// <summary>
        /// Registers a unique CommandInfo object</summary>
        /// <param name="info">CommandInfo to register</param>
        protected virtual void RegisterCommandInfo(CommandInfo info)
        {
            string menuText = info.MenuText;
            if (string.IsNullOrEmpty(menuText))
                throw new ArgumentException("menuText is null or empty");

            int textStart = 1;
            // for non-literal menu text, get last segment of path
            if (menuText[0] != '@')
            {
                // a little subtle here, if there's no separator, -1 bumps textStart back to 0
                textStart += menuText.LastIndexOfAny(s_pathDelimiters);
            }
            string displayedMenuText = menuText.Substring(textStart, menuText.Length - textStart);

            info.DisplayedMenuText = displayedMenuText;

            m_commands.Add(info);

            m_commandsById[info.CommandTag] = info;

            foreach (Keys k in info.Shortcuts)
                SetShortcut(k, info);
        }

        /// <summary>
        /// Unregisters a unique CommandInfo object</summary>
        /// <param name="info">CommandInfo to unregister</param>
        protected virtual void UnregisterCommandInfo(CommandInfo info)
        {
            m_commandsById.Remove(info.CommandTag);
            m_commands.Remove(info);
        }

        
        /// <summary>
        /// Sets shortcut</summary>
        /// <param name="shortcut">Shortcut keys</param>
        /// <param name="info">CommandInfo corresponding to shortcut keys</param>
        /// <remarks>Keeps m_shortcuts field, the menu item, and the CommandInfo in sync with regards to shortcuts
        /// and ensures that each shortcut is unique</remarks>
        protected void SetShortcut(Keys shortcut, CommandInfo info)
        {
            shortcut = KeysUtil.NumPadToNum(shortcut);

            // if shortcut is reserved then do not set it.
            if (m_reservedKeys.ContainsKey(shortcut))
            {
                Outputs.WriteLine(OutputMessageType.Warning, "cannot assign " + KeysUtil.KeysToString(shortcut, true) +
                    " to " + GetCommandPath(info) + " it is reserved for " + m_reservedKeys[shortcut]);

                info.RemoveShortcut(shortcut);

                // erase shortcut if exist.
                EraseShortcut(shortcut);
                return;
            }

            info.AddShortcut(shortcut);

            if (shortcut != Keys.None)
            {
                // If the shortcut already exists for a different command, then erase the old commands's shortcut.
                if (m_shortcuts.ContainsKey(shortcut) &&
                    m_shortcuts[shortcut] != info.CommandTag)
                {
                    object existingCommandTag = m_shortcuts[shortcut];
                    if (m_commandsById.ContainsKey(existingCommandTag))
                    {
                        CommandInfo existingInfo = m_commandsById[existingCommandTag];
                        existingInfo.RemoveShortcut(shortcut);
                    }
                }

                m_shortcuts[shortcut] = info.CommandTag;
            }
        }

        private void EraseShortcut(Keys shortcut)
        {
            // If the shortcut already exists then erase it. 
            if (m_shortcuts.ContainsKey(shortcut))
            {
                object existingCommandTag = m_shortcuts[shortcut];
                if (m_commandsById.ContainsKey(existingCommandTag))
                {
                    CommandInfo existingInfo = m_commandsById[existingCommandTag];
                    existingInfo.RemoveShortcut(shortcut);
                    m_shortcuts.Remove(shortcut);
                }
            }
        }

        private bool CommandIsUnique(object menuTag, string menuText)
        {
            // check for the same menu tag and menu text, which should catch most accidental
            //  duplication
            foreach (CommandInfo info in m_commands)
            {
                if (IsUnregistered(info))
                    continue;

                if (TagsEqual(info.MenuTag, menuTag) && info.MenuText == menuText)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Increments menu command count</summary>
        /// <param name="menuTag">Menu's unique ID</param>
        /// <returns>MenuInfo object corresponding to menu tag</returns>
        protected virtual MenuInfo IncrementMenuCommandCount(object menuTag)
        {
            MenuInfo menuInfo = null;
            // update menu's command count
            if (menuTag != null)
            {
                menuInfo = GetMenuInfo(menuTag);
                if (menuInfo != null)
                    menuInfo.Commands++;
            }
            return menuInfo;
        }

        /// <summary>
        /// Decrements the count of commands associated with the specified MenuInfo</summary>
        /// <param name="menuInfo">MenuInfo for menu's command count to decrement</param>
        protected virtual void DecrementMenuCommandCount(MenuInfo menuInfo)
        {
            menuInfo.Commands--;
        }

        /// <summary>
        /// Gets command path string for given command</summary>
        /// <param name="commandInfo">CommandInfo for command</param>
        /// <returns>String representing command path in menu hierarchy</returns>
        public string GetCommandPath(CommandInfo commandInfo)
        {
            string result = commandInfo.MenuText;

            MenuInfo menuInfo = GetMenuInfo(commandInfo.MenuTag);
            if (menuInfo == null)
                menuInfo = GetContextMenuInfo(commandInfo.MenuTag);

            if (menuInfo != null)
                result = menuInfo.MenuText + "/" + result;

            return result;
        }

        /// <summary>
        /// Tests if command is unregistered</summary>
        /// <param name="info">CommandInfo for command</param>
        /// <returns>True iff command is unregistered</returns>
        protected bool IsUnregistered(CommandInfo info)
        {
            return GetClient(info.CommandTag) == null;
        }

        private static int Compare(CommandInfo x, CommandInfo y)
        {
            int result = CompareTags(x.MenuTag, y.MenuTag);
            if (result == 0)
                result = CompareTags(x.GroupTag, y.GroupTag);
            if (result == 0)
                result = CompareTags(x.CommandTag, y.CommandTag);
            // finally use either the displayed menu or text registration index to ensure a stable sort
            if (result == 0)
            {
                if (x.GroupTag != null && m_defaultSortByMenuLabel.Contains(x.GroupTag))
                    result = CompareTags(x.DisplayedMenuText, y.DisplayedMenuText);
                else
                    result = x.Index - y.Index;
            }

            return result;
        }

        private static int CompareTags(object tag1, object tag2)
        {
            bool tag1First = false, tag2First = false, tag1Last = false, tag2Last = false;
            if (tag1 != null)
            {
                tag1First = m_beginningTags.Contains(tag1);
                tag1Last = m_endingTags.Contains(tag1);
            }
            if (tag2 != null)
            {
                tag2First = m_beginningTags.Contains(tag2);
                tag2Last = m_endingTags.Contains(tag2);
            }

            if (tag1First && !tag2First)
                return -1;

            if (tag2First && !tag1First)
                return 1;

            if (tag1Last && !tag2Last)
                return 1;

            if (tag2Last && !tag1Last)
                return -1;

            if (tag1 is Enum && tag2 is Enum)
                return ((int)tag1).CompareTo((int)tag2);

            if (tag1 is Enum)
                return -1;

            if (tag2 is Enum)
                return 1;

            if (tag1 is string && tag2 is string)
                return StringUtil.CompareNaturalOrder((string) tag1, (string) tag2);

            IComparable comparable1 = tag1 as IComparable;
            IComparable comparable2 = tag2 as IComparable;
            if (comparable1 != null)
            {
                int result = comparable1.CompareTo(tag2);
                if (result != 0)
                    return result;
            }
            if (comparable2 != null)
            {
                int result = comparable2.CompareTo(tag1);
                if (result != 0)
                    return result;
            }

            return 0;
        }

        /// <summary>
        /// Tests equality of menu tags</summary>
        /// <param name="tag1">Menu 1 tag</param>
        /// <param name="tag2">Menu 2 tag</param>
        /// <returns>True iff tags are equal</returns>
        protected static bool TagsEqual(object tag1, object tag2)
        {
            if (tag1 == null)
                return tag2 == null;

            return tag1.Equals(tag2);
        }

        /// <summary>
        /// Comparer for sorting commands by menu, group, and command tags</summary>
        protected class CommandComparer : IComparer<CommandInfo>
        {
            #region IComparer<CommandInfo> Members

            /// <summary>
            /// Compare method for commands</summary>
            /// <param name="x">Command 1 CommandInfo</param>
            /// <param name="y">Command 2 CommandInfo</param>
            /// <returns>-1 if Command 1 before Command 2, 0 if commands identical, 1 if Command 1 after Command 2</returns>
            public int Compare(CommandInfo x, CommandInfo y)
            {
                return CommandServiceBase.Compare(x, y);
            }

            #endregion
        }

        /// <summary>
        /// Derived classes can override this to redraw the menu and toolbar icons. This is useful if any
        /// registered command's icon can be changed at runtime.</summary>
        public virtual void RefreshImages() { }

        /// <summary>
        /// Derived classes can override this to redraw a particular menu item's icon.</summary>
        /// <param name="commandInfo">The command whose icon needs a refresh</param>
        public virtual void RefreshImage(CommandInfo commandInfo) { }

        /// <summary>
        /// Indicates whether the user clicked the icon/image portion of the menu item.</summary>
        public bool IconClicked { get; set; }

        /// <summary>
        /// Indicates which command's icon the mouse is currently over, or null if none. This can be used to
        /// modify a menu icon on mouseover.</summary>
        public virtual CommandInfo MouseIsOverCommandIcon { get; private set; }

        /// <summary>
        /// Class constructor. Does standard command ordering.</summary>
        static CommandServiceBase()
        {
            // force standard and framework items into their places at beginning or end
            m_endingTags.Add(StandardCommand.FileClose);

            m_beginningTags.Add(StandardCommand.FileSave);
            m_beginningTags.Add(StandardCommand.FileSaveAs);
            m_beginningTags.Add(StandardCommand.FileSaveAll);

            m_beginningTags.Add(StandardCommand.EditUndo);
            m_beginningTags.Add(StandardCommand.EditRedo);

            m_beginningTags.Add(StandardCommand.EditCut);
            m_beginningTags.Add(StandardCommand.EditCopy);
            m_beginningTags.Add(StandardCommand.EditPaste);
            m_endingTags.Add(StandardCommand.EditDelete);

            m_beginningTags.Add(StandardCommand.EditSelectAll);
            m_beginningTags.Add(StandardCommand.EditDeselectAll);
            m_beginningTags.Add(StandardCommand.EditInvertSelection);

            m_beginningTags.Add(StandardCommand.EditGroup);
            m_beginningTags.Add(StandardCommand.EditUngroup);
            m_beginningTags.Add(StandardCommand.EditLock);
            m_beginningTags.Add(StandardCommand.EditUnlock);

            m_beginningTags.Add(StandardCommand.ViewZoomIn);
            m_beginningTags.Add(StandardCommand.ViewZoomOut);
            m_beginningTags.Add(StandardCommand.ViewZoomExtents);

            m_beginningTags.Add(StandardCommand.WindowSplitHoriz);
            m_beginningTags.Add(StandardCommand.WindowSplitVert);
            m_beginningTags.Add(StandardCommand.WindowRemoveSplit);

            m_endingTags.Add(StandardCommand.HelpAbout);

            m_beginningTags.Add(StandardCommandGroup.FileNew);
            m_beginningTags.Add(StandardCommandGroup.FileSave);
            m_beginningTags.Add(StandardCommandGroup.FileOther);
            m_endingTags.Add(StandardCommandGroup.FileRecentlyUsed);
            m_endingTags.Add(StandardCommandGroup.FileExit);

            m_beginningTags.Add(StandardCommandGroup.EditUndo);
            m_beginningTags.Add(StandardCommandGroup.EditCut);
            m_beginningTags.Add(StandardCommandGroup.EditSelectAll);
            m_beginningTags.Add(StandardCommandGroup.EditGroup);
            m_beginningTags.Add(StandardCommandGroup.EditOther);
            m_endingTags.Add(StandardCommandGroup.EditPreferences);

            m_beginningTags.Add(StandardCommandGroup.ViewZoomIn);
            m_beginningTags.Add(StandardCommandGroup.ViewControls);

            m_beginningTags.Add(StandardCommandGroup.WindowLayout);
            m_beginningTags.Add(StandardCommandGroup.WindowSplit);
            m_endingTags.Add(StandardCommandGroup.WindowDocuments);

            m_endingTags.Add(StandardCommandGroup.HelpAbout);

            m_beginningTags.Add(CommandId.FileRecentlyUsed1);
            m_beginningTags.Add(CommandId.FileRecentlyUsed2);
            m_beginningTags.Add(CommandId.FileRecentlyUsed3);
            m_beginningTags.Add(CommandId.FileRecentlyUsed4);

            m_endingTags.Add(StandardCommand.FileExit);

            m_endingTags.Add(CommandId.EditPreferences);
            m_endingTags.Add(CommandId.EditDocumentPreferences);

            // Force subitems with the same menu and group to sort themselves by menu name, rather than creation index
            m_defaultSortByMenuLabel.Add(StandardCommandGroup.WindowDocuments);
        }

        /// <summary>
        /// Sets StatusService</summary>
        /// <remarks>Used in ATF2.9. There could be a cleaner way to do that.</remarks>
        public void SetStatusService(IStatusService statusService)
        {
            if (m_statusService != null)
                return;
            m_statusService = statusService;
        }

        [Import(AllowDefault = true)]
        protected IStatusService m_statusService;

        [Import(AllowDefault = true)]
        protected ISettingsService m_settingsService;

        protected ImageSizes m_imageSize = ImageSizes.Size24x24;

        protected List<MenuInfo> m_menus =
            new List<MenuInfo>();

        protected List<CommandInfo> m_commands =
            new List<CommandInfo>();

        protected Dictionary<object, CommandInfo> m_commandsById =
            new Dictionary<object, CommandInfo>();

        protected Multimap<object, ICommandClient> m_commandClients =
            new Multimap<object, ICommandClient>();

        protected Dictionary<Keys, object> m_shortcuts =
            new Dictionary<Keys, object>();

        protected Dictionary<Keys, string> m_reservedKeys =
            new Dictionary<Keys, string>();

        protected ICommandClient m_activeClient;

        private bool m_contextMenuAutoCompact = true;

        protected static char[] s_pathDelimiters = new[] { '/', '\\' };

        private static readonly HashSet<object> m_beginningTags = new HashSet<object>();
        private static readonly HashSet<object> m_endingTags = new HashSet<object>();
        private static readonly HashSet<object> m_defaultSortByMenuLabel = new HashSet<object>();
    }
}
