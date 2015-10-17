//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Threading;
using Sce.Atf.Applications.Controls;
using Sce.Atf.Controls;

using Keys = Sce.Atf.Input.Keys;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Service to handle commands in menus and toolbars</summary>
    [Export(typeof(ICommandService))]
    [Export(typeof(IInitializable))]
    [Export(typeof(CommandService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CommandService : CommandServiceBase
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="mainForm">Main application form</param>
        [ImportingConstructor]
        public CommandService(Form mainForm)
        {
            m_mainForm = mainForm;
            m_mainForm.Load += mainForm_Load;

            m_mainMenuStrip = new MenuStrip();
            m_mainMenuStrip.Name = "Main Menu";
            m_mainMenuStrip.Dock = DockStyle.Top;
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by subscribing to event</summary>
        public override void Initialize()
        {
            base.Initialize();

            // Application idle event is a convenient time to update toolbar button state
            Application.Idle += Application_Idle;
        }

        #endregion

        #region ICommandService Members

        /// <summary>
        /// Registers a command for a command client</summary>
        /// <param name="info">Command description; standard commands are defined as static
        /// members on the CommandInfo class</param>
        /// <param name="client">Client that handles the command</param>
        public override void RegisterCommand(CommandInfo info, ICommandClient client)
        {
            base.RegisterCommand(info, client);
            if (info != null && client != null && info.CheckCanDoClients.Contains(client))
            {
                m_checkCanDoClients.Add(info, client);
                m_checkCanDoClientsToUpdate.Add(info);
            }
            m_commandsSorted = false;
        }

        /// <summary>
        /// Unregisters a command for a command client</summary>
        /// <param name="commandTag">Command tag that identifies CommandInfo used to register
        /// the command</param>
        /// <param name="client">Client that handles the command</param>
        public override void UnregisterCommand(object commandTag, ICommandClient client)
        {
            CommandInfo info = GetCommandInfo(commandTag);
            if (info != null && client != null && info.CheckCanDoClients.Contains(client))
            {
                m_checkCanDoClients.Remove(info, client);
                m_checkCanDoClientsToUpdate.Remove(info);
            }
            base.UnregisterCommand(commandTag, client);
            RemoveToolStripItem(commandTag);
        }

        /// <summary>
        /// Decrements the count of commands associated with the specified MenuInfo</summary>
        /// <param name="menuInfo">MenuInfo whose command count is decremented</param>
        protected override void DecrementMenuCommandCount(MenuInfo menuInfo)
        {
            base.DecrementMenuCommandCount(menuInfo);
            if (menuInfo.Commands == 0)
                m_menuToolStripItems[menuInfo].Visible = false;
        }

        /// <summary>
        /// Creates and returns a context (right click popup) menu.
        /// Does not raise any events.</summary>
        /// <param name="commandTags">Commands in menu; nulls indicate separators</param>
        /// <returns>ContextMenuStrip for context menu</returns>
        public ContextMenuStrip CreateContextMenu(IEnumerable<object> commandTags)
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            int itemCount;
            foreach (object commandTag in commandTags)
            {
                // check for separator
                if (commandTag == null)
                {
                    itemCount = contextMenu.Items.Count;
                    if (itemCount > 0 &&
                        !(contextMenu.Items[itemCount - 1] is ToolStripSeparator))
                    {
                        contextMenu.Items.Add(new ToolStripSeparator());
                    }
                    continue;
                }

                // add the command and sort it by groups since the last separator
                CommandInfo info = GetCommandInfo(commandTag);
                if (info != null && (info.Visibility & CommandVisibility.ContextMenu) != 0)
                {
                    // allow client to update command appearance
                    UpdateCommand(info);

                    var menuItem = info.GetMenuItem();
                    if (menuItem.Enabled || !ContextMenuAutoCompact)
                    {
                        ToolStripItemCollection commands = BuildSubMenus(contextMenu.Items, info);
                        ToolStripMenuItem clone = new ToolStripMenuItem();
                        clone.Text = menuItem.Text;
                        clone.Image = menuItem.Image;
                        clone.Name = menuItem.Name;
                        clone.Enabled = menuItem.Enabled;
                        clone.Checked = menuItem.Checked;
                        clone.Tag = menuItem.Tag;
                        clone.ToolTipText = menuItem.ToolTipText;
                        clone.ShortcutKeys = menuItem.ShortcutKeys;
                        clone.ShortcutKeyDisplayString = menuItem.ShortcutKeyDisplayString;
                        clone.Click += contextMenu_itemClick;
                        clone.ForeColor = m_mainMenuStrip.ForeColor;
                        clone.CheckOnClick = info.CheckOnClick;
                        commands.Add(clone);

                        MaintainSeparateGroups(commands, clone, info.GroupTag);
                    }
                }
            }

            // Remove trailing separator.
            itemCount = contextMenu.Items.Count;
            if (itemCount > 0 &&
                (contextMenu.Items[itemCount - 1] is ToolStripSeparator))
            {
                contextMenu.Items.RemoveAt(itemCount - 1);
            }

            SkinService.ApplyActiveSkin(contextMenu);

            return contextMenu;
        }

        /// <summary>
        /// Runs a context (right click popup) menu at the given screen point. Raises
        /// ContextMenuClosed events.</summary>
        /// <param name="commandTags">Commands in menu; nulls indicate separators</param>
        /// <param name="screenPoint">Point in screen coordinates</param>
        public override void RunContextMenu(IEnumerable<object> commandTags, Point screenPoint)
        {
            ContextMenuStrip contextMenu = CreateContextMenu(commandTags);
            contextMenu.Show(screenPoint);
            if (contextMenu.Visible)
                contextMenu.Closed += contextMenu_Closed;
            else
                contextMenu_Closed(contextMenu, new ToolStripDropDownClosedEventArgs(ToolStripDropDownCloseReason.CloseCalled));
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Checks whether the client can do the command if it handles it</summary>
        /// <param name="commandTag">Command to be done</param>
        /// <returns><c>True</c> if client can do the command</returns>
        public override bool CanDoCommand(object commandTag)
        {
            return CommandId.EditKeyboard.Equals(commandTag);
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandTag">Command to be done</param>
        public override void DoCommand(object commandTag)
        {
            if (CommandId.EditKeyboard.Equals(commandTag))
            {
                // Prepare a list of Shortcut objects to use to populate dialog box and to hold results
                var shortcuts = new List<CustomizeKeyboardDialog.Shortcut>();

                foreach (KeyValuePair<object, CommandInfo> kvp in m_commandsById)
                {
                    // skip over command without client 
                    if (GetClient(kvp.Value.CommandTag) == null )
                        continue;

                    if (!kvp.Value.ShortcutsEditable)
                        continue;

                    string commandPath = GetCommandPath(kvp.Value);
                    var shortcutInfo = new CustomizeKeyboardDialog.Shortcut(kvp.Value, commandPath);
                    shortcuts.Add(shortcutInfo);
                }

                CustomizeKeyboardDialog dialog = new CustomizeKeyboardDialog(shortcuts, m_reservedKeys);
                dialog.ShowDialog(m_mainForm);

                if (dialog.DialogResult == DialogResult.OK && dialog.Modified)
                {
                    // commit changes
                    m_shortcuts.Clear();
                    foreach (CustomizeKeyboardDialog.Shortcut shortcutInfo in shortcuts)
                    {
                        // We have to clear them all, then add them one-by-one.
                        shortcutInfo.Info.ClearShortcuts();
                        foreach (Keys keys in shortcutInfo.Keys)
                            SetShortcut(keys, shortcutInfo.Info);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Helper function to return properly sized images based on user preferences</summary>
        /// <param name="imageName">Image name</param>
        /// <returns>Image of given name</returns>
        public Image GetProperlySizedImage(string imageName)
        {
            Image image = null;

            if (!string.IsNullOrEmpty(imageName))
            {
                if (m_imageSize == ImageSizes.Size16x16)
                    image = ResourceUtil.GetImage16(imageName);
                else if (m_imageSize == ImageSizes.Size24x24)
                    image = ResourceUtil.GetImage24(imageName);
                else if (m_imageSize == ImageSizes.Size32x32)
                    image = ResourceUtil.GetImage32(imageName);
            }

            return image;
        }

        /// <summary>
        /// Handler for image size changed event for derived classes to override</summary>
        protected override void OnImageSizeChanged()
        {
            RefreshImages();
        }

        /// <summary>
        /// This function refreshes all images associated with registered commands.
        /// This is particularly useful when application image sizes change, and
        /// when a skin that modifies images is loaded.</summary>
        public override void RefreshImages()
        {
            foreach (CommandInfo info in m_commands)
            {
                RefreshImage(info);
            }
        }

        /// <summary>
        /// This function redraws a particular menu item's icon. This is useful if only a
        /// specific icon has been changed; for example, on mouseover.</summary>
        /// <param name="info">The command whose icon needs a refresh</param>
        public override void RefreshImage(CommandInfo info)
        {
            Image image = GetProperlySizedImage(info.ImageName);
            if (image != null)
            {
                var button = info.GetButton();
                button.AutoSize = true;
                button.ImageScaling = ToolStripItemImageScaling.None;
                button.Image = image;

                // Update the menu image too, but don't set AutoSize, as the menu icons
                // should always stay the same size.
                var menu = info.GetMenuItem();
                menu.Image = image;
            }
        }

        /// <summary>
        /// Occurs when the right-click context menu has closed. Is always raised after
        /// RunContextMenu returns.</summary>
        public event EventHandler ContextMenuClosed;

        /// <summary>
        /// Populates menu control and toolbar with all registered menus and commands</summary>
        public void BuildDefaultMenusAndToolbars()
        {
            // try to get toolstrip container for toolbars
            if (m_toolStripContainer == null)
            {
                foreach (Control control in m_mainForm.Controls)
                {
                    m_toolStripContainer = control as ToolStripContainer;
                    if (m_toolStripContainer != null)
                        break;
                }
            }

            // build menus (but not their contents)
            m_mainMenuStrip.SuspendLayout();
            m_mainMenuStrip.Items.Clear();

            foreach (MenuInfo menuInfo in m_menus)
            {
                ToolStripMenuItem menuItem = m_menuToolStripItems[menuInfo];
                menuItem.DropDownItems.Add("Dummy"); // to trigger drop down
                // we will build the menu just-in-time, when its drop down is opening
                menuItem.DropDownOpening += menuItem_DropDownOpening;
                // we will clear the menu when it's closed
                menuItem.DropDownClosed += menuItem_DropDownClosed;
                m_mainMenuStrip.Items.Add(menuItem);
            }

            m_mainMenuStrip.ResumeLayout();
            m_mainMenuStrip.PerformLayout();

            // build toolbars
            var toolStrips = new List<ToolStrip>();
            for (int i = m_menus.Count - 1; i >= 0; i--)
            {
                MenuInfo menuInfo = m_menus[i];
                ToolStrip toolStrip = menuInfo.GetToolStrip();
                toolStrips.Add(toolStrip);

                foreach (CommandInfo commandInfo in m_commands)
                {
                    if (TagsEqual(menuInfo.MenuTag, commandInfo.MenuTag))
                    {
                        if ((commandInfo.Visibility & CommandVisibility.Toolbar) != 0 &&
                            GetClient(commandInfo.CommandTag) != null)
                        {
                            ToolStripButton btn = commandInfo.GetButton();
                            if (commandInfo.CheckOnClick)
                            {
                                btn.CheckOnClick = true;
                                commandInfo.GetMenuItem().CheckOnClick = true;
                                btn.CheckedChanged += SynchronizeCheckedState;
                                commandInfo.GetMenuItem().CheckedChanged += SynchronizeCheckedState;
                            }
                            toolStrip.Items.Add(btn);
                        }
                    }
                }

                toolStrip.Dock = DockStyle.None;
                AddCustomizationDropDown(toolStrip);
                toolStrip.Visible = (toolStrip.Items.Count > 1);
            }

            if (m_toolStripContainer != null)
            {
                m_toolStripContainer.TopToolStripPanel.Controls.AddRange(toolStrips.ToArray());
                m_toolStripContainer.TopToolStripPanel.Controls.Add(m_mainMenuStrip);
            }
            else
            {
                m_mainForm.Controls.Add(m_mainMenuStrip);
            }
        }

        /// <summary>
        /// Forces an update for the command associated with the given tag.</summary>
        /// <param name="commandTag">Command's tag object</param>
        protected override void UpdateCommand(object commandTag)
        {
            CommandInfo info = GetCommandInfo(commandTag);
            if (info != null)
                UpdateCommand(info);

            //Console.WriteLine("forced update for: " + (info != null ? info.Description : "<unknown>"));
        }

        /// <summary>
        /// Force an update on a particular command</summary>
        /// <param name="info">Command to update</param>
        public void UpdateCommand(CommandInfo info)
        {
            ICommandClient client = GetClientOrActiveClient(info.CommandTag);
            UpdateCommand(info, client);
        }

        private void UpdateCommand(CommandInfo info, ICommandClient client)
        {
            if (m_mainForm.InvokeRequired)
            {
                m_mainForm.BeginInvoke(new Action<CommandInfo>(UpdateCommand), info);
                return;
            }

            ToolStripMenuItem menuItem;
            ToolStripButton menuButton;
            info.GetMenuItemAndButton(out menuItem, out menuButton);

            CommandState commandState = new CommandState();
            commandState.Text = info.DisplayedMenuText;
            commandState.Check = menuItem.Checked;

            bool enabled = false;
            if (client != null)
            {
                enabled = client.CanDoCommand(info.CommandTag);
                if (enabled)
                    client.UpdateCommand(info.CommandTag, commandState);
            }

            string menuText = commandState.Text.Trim();

            menuItem.Text = menuButton.Text = menuText;
            menuItem.Checked = menuButton.Checked = commandState.Check;
            menuItem.Enabled = menuButton.Enabled = enabled;
        }

        // Synchronize Checked state between the menu and button
        private void SynchronizeCheckedState(object sender, EventArgs e)
        {
            if (sender is ToolStripButton)
            {
                var checkedPair = m_commandControls.FirstOrDefault(x => x.Value.Button == sender);
                if (checkedPair.Value != null)
                {
                    checkedPair.Value.MenuItem.Checked = checkedPair.Value.Button.Checked;
                }

            }
            else if (sender is ToolStripMenuItem)
            {
                var checkedPair = m_commandControls.FirstOrDefault(x => x.Value.MenuItem == sender);
                if (checkedPair.Value != null)
                {
                    checkedPair.Value.Button.Checked = checkedPair.Value.MenuItem.Checked;
                }
            }
        }

        // wait until latest possible time to build menus and toolbars
        private void mainForm_Load(object sender, EventArgs e)
        {
            BuildDefaultMenusAndToolbars();
        }

        private void menuItem_DropDownOpening(object sender, EventArgs e)
        {
            // make sure commands are sorted
            if (!m_commandsSorted)
            {
                m_commands.Sort(new CommandComparer());
                m_commandsSorted = true;
            }
            
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            menuItem.DropDownItems.Clear();
            MenuInfo menuInfo = GetMenuInfo(menuItem.Tag);
            foreach (CommandInfo commandInfo in m_commands)
            {
                if (TagsEqual(commandInfo.MenuTag, menuInfo.MenuTag) &&
                    (commandInfo.Visibility & CommandVisibility.ApplicationMenu) != 0 &&
                    GetClient(commandInfo.CommandTag) != null)
                {
                    AddMenuCommand(menuItem.DropDownItems, commandInfo);
                    menuItem.DropDown.BackColor = m_mainMenuStrip.BackColor;
                }
            }
        }

        private void menuItem_DropDownClosed(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            menuItem.DropDownItems.Clear();
            menuItem.DropDownItems.Add("Dummy"); // to trigger drop down
        }

        private void item_Click(object sender, EventArgs e)
        {
            // See if the user clicked the icon portion of the menu item and set the IconClicked property that
            // interested commands can check.
            IconClicked = IsMouseOverIcon(sender as ToolStripItem);

            // clear status text
            if (m_statusService != null)
                m_statusService.ShowStatus(string.Empty);

            ToolStripItem item = sender as ToolStripItem;
            object tag = item.Tag;
            if (tag != null)
            {
                ICommandClient client = GetClientOrActiveClient(tag);
                if (client != null && client.CanDoCommand(tag))
                    client.DoCommand(tag);
            }

            IconClicked = false;
        }

        /// <summary>
        /// Gets or sets whether a context menu is triggering</summary>
        public static bool ContextMenuIsTriggering { get; set; }

        private void contextMenu_itemClick(object sender, EventArgs e)
        {
            ContextMenuIsTriggering = true;
            item_Click(sender, e);
            ContextMenuIsTriggering = false;
        }

        private void menuItem_MouseEnter(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            object tag = menuItem.Tag;

            if (tag != null)
            {
                CommandInfo commandInfo = m_commandsById[tag];
                if (m_statusService != null)
                    m_statusService.ShowStatus(commandInfo.Description);
            }
        }

        private void menuItem_MouseMove(object sender, MouseEventArgs e)
        {
            m_menuMouseLocation = e.Location;
            
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem != null)
            {
                object tag = menuItem.Tag;
                if ((tag != null) && (tag is IPinnable))
                {
                    if (IsMouseOverIcon(menuItem))
                        m_mouseIsOverCommandIcon = m_commandsById[tag];
                    else
                        m_mouseIsOverCommandIcon = null;

                    CommandState commandState = new CommandState(menuItem.Text, menuItem.Checked);
                    UpdatePinnableCommand(tag, commandState);
                }
            }
        }

        private void menuItem_MouseLeave(object sender, EventArgs e)
        {
            // clear status text
            if (m_statusService != null)
                m_statusService.ShowStatus(string.Empty);

            // Clear mouseover status
            m_menuMouseLocation = Point.Empty;
            m_mouseIsOverCommandIcon = null;

            var menuItem = sender as ToolStripMenuItem;
            if (menuItem != null)
            {
                object tag = menuItem.Tag;
                if ((tag != null) && (tag is IPinnable))
                {
                    CommandState commandState = new CommandState(menuItem.Text, menuItem.Checked);
                    UpdatePinnableCommand(tag, commandState);
                }
            }
        }

        /// <summary>
        /// Utility function to get the command's client and call UpdateCommand.</summary>
        /// <param name="commandTag">The command to update</param>
        /// <param name="state">Command's state</param>
        private void UpdatePinnableCommand(object commandTag, CommandState state)
        {
            ICommandClient client = GetClientOrActiveClient(commandTag);
            if (client != null && client.CanDoCommand(commandTag))
                client.UpdateCommand(commandTag, state);
        }

        /// <summary>
        /// Indicates which command's icon the mouse is currently over, or null if none. This can be used to
        /// modify a menu icon on mouseover.</summary>
        public override CommandInfo MouseIsOverCommandIcon
        {
            get { return m_mouseIsOverCommandIcon; }
        }

        /// <summary>
        /// Determines whether the mouse pointer is currently over the icon portion of the menu item</summary>
        private bool IsMouseOverIcon(ToolStripItem menuItem)
        {
            // NOTE: There doesn't appear to be a way to test against the actual current position and size of
            // the icon, as menuItem.Image.GetBounds() returns the size of the image that was assigned to the
            // menu, not the (possibly scaled) size it's actually displayed at. So this uses various other
            // settings to yield a "good enough" result. Should revisit at some point and see if there's a
            // more direct/precise way to do this.

            if ((menuItem != null) && (m_menuMouseLocation != Point.Empty))
            {
                if (menuItem.Image != null)
                {
                    var contentBounds = menuItem.ContentRectangle;
                    var iconBounds = new Rectangle(new Point(contentBounds.Left, contentBounds.Top), SystemInformation.MenuButtonSize);
                    if (m_menuMouseLocation.X > iconBounds.Left && m_menuMouseLocation.X <= iconBounds.Right)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Updates tool bar buttons when application goes idle</summary>
        private void Application_Idle(object sender, EventArgs e)
        {
            // Don't use 'foreach', in case commands are registered during UpdateCommand().
            for (int i = 0; i < m_commands.Count; ++i)
            {
                CommandInfo info = m_commands[i];
                ICommandClient client = GetClientOrActiveClient(info.CommandTag);
                if (client != null && m_checkCanDoClients.ContainsKeyValue(info, client))
                    continue;
                UpdateCommand(info, client);
            }

            // Update CommandInfos "on demand", in response to their ICommandClients.
            if (m_checkCanDoClientsToUpdate.Count > 0)
            {
                CommandInfo[] additionalInfos = m_checkCanDoClientsToUpdate.ToArray();
                foreach (CommandInfo info in additionalInfos)
                {
                    // duplicate CommandInfos don't get added to m_commands
                    if (info.CommandService != null)
                        UpdateCommand(info);
                }
                m_checkCanDoClientsToUpdate.Clear();
            }
        }

        private void OnCheckCanDo(object sender, EventArgs e)
        {
            m_checkCanDoClientsToUpdate.Add((CommandInfo) sender);
        }

        private void contextMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            if (ContextMenuClosed != null)
                ContextMenuClosed(this, e);
        }

        /// <summary>
        /// Adds a customization drop down menu to the tool strip. Check if this drop down button
        /// is already present, in case the standard menus are setup twice (which happens in Legacy applications).</summary>
        private void AddCustomizationDropDown(ToolStrip toolStrip)
        {
            string customizeText = "Customize".Localize();
            foreach (ToolStripItem item in toolStrip.Items)
                if (item.Text == customizeText)
                    return;

            ToolStripDropDownButton button = new ToolStripDropDownButton(customizeText);
            button.Name = toolStrip.Name;
            button.Overflow = ToolStripItemOverflow.Always;
            button.DropDownOpening += button_DropDownOpening;
            toolStrip.Items.Add(button);
        }

        private void button_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripDropDownButton button = sender as ToolStripDropDownButton;
            button.DropDownItems.Clear();
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.ItemClicked += contextMenu_DropDownItemClicked;
            contextMenu.Closing += contextMenu_Closing;

            ToolStrip toolStrip = button.Owner;
            foreach (ToolStripItem item in toolStrip.Items)
            {
                if (item is ToolStripSeparator)
                    continue;
                if (item == button) // can't customize customization!
                    continue;
                ToolStripMenuItem menuItem = new ToolStripMenuItem(item.Text);
                menuItem.Checked = item.Visible;
                menuItem.Tag = item;
                contextMenu.Items.Add(menuItem);
            }
            button.DropDown = contextMenu;
        }
        
        private void contextMenu_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripMenuItem menuItem = e.ClickedItem as ToolStripMenuItem;
            ToolStripItem item = e.ClickedItem.Tag as ToolStripItem;
            item.Visible = !item.Visible; // toggle
            menuItem.Checked = !menuItem.Checked;

        }

        private void contextMenu_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
            {
                // allow multiple items to be selected before closing menu
                e.Cancel = true;
            }
            else
            {
                // remove drop down menu
                ContextMenuStrip contextMenu = sender as ContextMenuStrip;
                ToolStripDropDownButton button = contextMenu.OwnerItem as ToolStripDropDownButton;
                button.DropDown = null;
            }
        }

        private void RemoveToolStripItem(object commandTag)
        {
            if (m_toolStripContainer != null)
            {
                RemoveToolStripItem(commandTag, m_toolStripContainer.LeftToolStripPanel);
                RemoveToolStripItem(commandTag, m_toolStripContainer.TopToolStripPanel);
                RemoveToolStripItem(commandTag, m_toolStripContainer.RightToolStripPanel);
                RemoveToolStripItem(commandTag, m_toolStripContainer.BottomToolStripPanel);
            }
        }

        private void RemoveToolStripItem(object commandTag, ToolStripPanel panel)
        {
            foreach (ToolStrip toolStrip in panel.Controls)
            {
                foreach (ToolStripItem item in toolStrip.Items)
                {
                    if (item.Tag == commandTag)
                    {
                        toolStrip.Items.Remove(item);
                        return; // there should be only one occurrence
                    }
                }
            }
        }

        // Adds a command to menus
        private void AddMenuCommand(ToolStripItemCollection commands, CommandInfo info)
        {
            // make sure all necessary sub-menus exist, and get command collection to hold this command
            commands = BuildSubMenus(commands, info);

            var menuItem = info.GetMenuItem();
            menuItem.BackColor = m_mainMenuStrip.BackColor;
            menuItem.ForeColor = m_mainMenuStrip.ForeColor;
            commands.Add(menuItem);

            MaintainSeparateGroups(commands, menuItem, info.GroupTag);
        }

        // Ensures that submenus exist to hold command
        private ToolStripItemCollection BuildSubMenus(ToolStripItemCollection commands, CommandInfo info)
        {
            string menuText = info.MenuText;
            string[] segments;
            if (menuText[0] == '@')
                segments = new[] { menuText.Substring(1, menuText.Length - 1) };
            else
                segments = menuText.Split(s_pathDelimiters, 8);

            for (int i = 0; i < segments.Length - 1; i++)
            {
                string segment = segments[i];
                ToolStripMenuItem subMenu = null;
                for (int j = 0; j < commands.Count; j++)
                {
                    if (segment == commands[j].Text)
                    {
                        subMenu = commands[j] as ToolStripMenuItem;
                        if (subMenu != null)
                            break;
                    }
                }
                if (subMenu == null)
                {
                    subMenu = new ToolStripMenuItem(segment);
                    subMenu.Name = segment;
                    commands.Add(subMenu);

                    MaintainSeparateGroups(commands, subMenu, info.GroupTag);
                }


                subMenu.BackColor = m_mainMenuStrip.BackColor;
                subMenu.ForeColor = m_mainMenuStrip.ForeColor;
                commands = subMenu.DropDownItems;
            }

            return commands;
        }

        // Maintains separators between different command groups
        private void MaintainSeparateGroups(ToolStripItemCollection commands, ToolStripItem item, object groupTag)
        {
            int index = commands.IndexOf(item);
            if (index > 0) // look for previous item
            {
                ToolStripItem prevItem = commands[index - 1];
                object prevTag = prevItem.Tag;
                while (prevTag == null)
                {
                    ToolStripMenuItem prevMenuItem = prevItem as ToolStripMenuItem;
                    if (prevMenuItem == null)
                        break;
                    ToolStripItemCollection prevItems = prevMenuItem.DropDownItems;
                    prevItem = prevItems[prevItems.Count - 1];
                    prevTag = prevItem.Tag;
                }

                // add a separator if the new command is from a different group
                CommandInfo prevInfo = GetCommandInfo(prevTag);
                if (prevInfo != null &&
                    !TagsEqual(groupTag, prevInfo.GroupTag))
                {
                    commands.Insert(index, new ToolStripSeparator());
                }
            }
        }

        /// <summary>
        /// Adds this MenuInfo object to m_menus field and creates a ToolStrip for it</summary>
        /// <param name="info">MenuInfo object to add</param>
        sealed protected override void RegisterMenuInfo(MenuInfo info)
        {
            base.RegisterMenuInfo(info);

            // If it wasn't already done, create a WinForms ToolStrip for this MenuInfo
            ToolStrip toolStrip;
            if (m_menuToolStrips.TryGetValue(info, out toolStrip) == false)
            {
                toolStrip = new ToolStripEx();
                toolStrip.MouseHover += ToolStripOnMouseHover;
                m_menuToolStrips.Add(info, toolStrip);
            }

            // build toolbar corresponding to menu
            {
                string str = info.MenuText.Replace("&", "");
                str = str.Replace(";", "");
                toolStrip.Name = str + "_toolbar";
                toolStrip.AllowItemReorder = true; // magic, to enable customization with Alt key
            }

            // build menu
            ToolStripMenuItem menuItem = new ToolStripMenuItem(info.MenuText);
            menuItem.Visible = false;
            menuItem.Name = info.MenuText + "_menu";
            menuItem.Tag = info.MenuTag;
            m_menuToolStripItems.Add(info, menuItem);
            
            // Associate the registered MenuInfo with this CommandService.  Only can be registered once.
            info.CommandService = this;
        }

        /// <summary>
        /// Processes the key as a command shortcut</summary>
        /// <param name="key">Key to process</param>
        /// <returns><c>True</c> if the key was processed as a command shortcut</returns>
        public override bool ProcessKey(Keys key)
        {
            if (key == Keys.F1 && m_lastHoveringToolStrip != null)
            {
                foreach (var item in m_lastHoveringToolStrip.Items)
                {
                    var button = item as ToolStripButton;
                    if (button != null)
                    {
                        if (button.Selected)
                        {
                            foreach (KeyValuePair<CommandInfo, CommandControls> commandAndControl in m_commandControls)
                            {
                                if (commandAndControl.Value.Button == button &&
                                    !string.IsNullOrEmpty(commandAndControl.Key.HelpUrl))
                                {
                                    // There doesn't seem to be a way to prevent the WM_HELP message that gets generated
                                    //  during the call to Process.Start(). We can't add a filter to every Control's
                                    //  WndProc. So, let's use a static way of stopping WebHelp for a little bit.
                                    WebHelp.SupressHelpRequests = true;
                                    Process.Start(commandAndControl.Key.HelpUrl);

                                    if (m_webHelpTimer == null)
                                    {
                                        m_webHelpTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };
                                        m_webHelpTimer.Tick +=
                                            (o, e) =>
                                            {
                                                WebHelp.SupressHelpRequests = false;
                                                m_webHelpTimer.Stop();
                                            };
                                    }
                                    m_webHelpTimer.Start();
                                    
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return base.ProcessKey(key);
        }

        private void ToolStripOnMouseHover(object sender, EventArgs eventArgs)
        {
            m_lastHoveringToolStrip = (ToolStripEx)sender;
        }

        // Update the WinForms controls when a command's shortcuts have changed
        static private void commandInfo_ShortcutsChanged(object sender, System.EventArgs e)
        {
            var info = (CommandInfo)sender;
            if (info == null)
                throw new InvalidOperationException("commandInfo_ShortcutsChanged() - sender was not a CommandInfo");

            info.RebuildShortcutDisplayString();
        }

        // Called when a command info has been updated. The toolbar buttons may need to be made
        // visible or invisible.
        private static void commandInfo_VisibilityChanged(object sender, System.EventArgs e)
        {
            var info = (CommandInfo)sender;
            ToolStripButton button = info.GetButton();
            if (button != null)
                button.Visible = (info.Visibility & CommandVisibility.Toolbar) != 0;

            // ToDo: Check parent? Look for the customizable drop-down button?
        }

        /// <summary>
        /// Registers a unique CommandInfo object</summary>
        /// <param name="info">CommandInfo object to register</param>
        protected override void RegisterCommandInfo(CommandInfo info)
        {
            m_commandsSorted = false;

            Image image = GetProperlySizedImage(info.ImageName);

            string uniqueId = GetCommandPath(info);

            // Create the WinForms controls to be associated with the registered MenuInfo
            var controls = new CommandControls(
                new ToolStripMenuItem(info.DisplayedMenuText, image),
                new ToolStripButton(info.MenuText, image)
                );

            m_commandControls.Add(info, controls);

            // Associate the registered MenuInfo with this CommandService.  Only can be registered once.
            info.CommandService = this;
            info.ShortcutsChanged += commandInfo_ShortcutsChanged;
            info.VisibilityChanged += commandInfo_VisibilityChanged;

            base.RegisterCommandInfo(info);

            ToolStripMenuItem menuItem = controls.MenuItem;
            menuItem.Name = uniqueId;
            menuItem.Tag = info.CommandTag;
            menuItem.MouseEnter += menuItem_MouseEnter;
            menuItem.MouseLeave += menuItem_MouseLeave;
            menuItem.MouseMove += menuItem_MouseMove;
            menuItem.Click += item_Click;

            ToolStripButton button = controls.Button;
            button.AutoSize = true;
            button.ImageScaling = ToolStripItemImageScaling.None;
            button.Name = uniqueId;
            button.DisplayStyle =
                (image != null) ? ToolStripItemDisplayStyle.Image : ToolStripItemDisplayStyle.Text;
            button.Tag = info.CommandTag;
            string toolTipText = info.Description;
            if (!string.IsNullOrEmpty(info.HelpUrl))
                toolTipText += Environment.NewLine + "Press F1 for more info".Localize();
            button.ToolTipText = toolTipText;
            button.Click += item_Click;

            // This method is only called once per unique CommandInfo, so there's no danger
            //  of subscribing to the same event multiple times.
            info.CheckCanDo += OnCheckCanDo;
        }

        /// <summary>
        /// Unregisters a unique CommandInfo object</summary>
        /// <param name="info">CommandInfo object to unregister</param>
        protected override void UnregisterCommandInfo(CommandInfo info)
        {
            info.CheckCanDo -= OnCheckCanDo;

            CommandControls controls;
            if (m_commandControls.TryGetValue(info, out controls))
            {
                controls.Dispose();
                m_commandControls.Remove(info);
            }

            info.ShortcutsChanged -= commandInfo_ShortcutsChanged;
            info.VisibilityChanged -= commandInfo_VisibilityChanged;

            base.UnregisterCommandInfo(info);
        }

        /// <summary>
        /// Increment menu command count</summary>
        /// <param name="menuTag">Menu's unique ID tag. Is null if there is no menu item.</param>
        /// <returns>MenuInfo for menu</returns>
        protected override MenuInfo IncrementMenuCommandCount(object menuTag)
        {
            MenuInfo menuInfo = base.IncrementMenuCommandCount(menuTag);

            if (menuInfo != null && menuInfo.Commands == 1)
                m_menuToolStripItems[menuInfo].Visible = true;

            return menuInfo;
        }

        /// <summary>
        /// Encapsulates WinForms controls instantiated for a MenuInfo instance</summary>
        public class MenuControls
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="menuItem">ToolStripMenuItem</param>
            /// <param name="toolStrip">ToolStrip</param>
            public MenuControls(ToolStripMenuItem menuItem, ToolStrip toolStrip)
            {
                MenuItem = menuItem;
                ToolStrip = toolStrip;
            }

            /// <summary>
            /// Gets ToolStripMenuItem in MenuControls</summary>
            public ToolStripMenuItem MenuItem { get; private set; }
            /// <summary>
            /// Gets ToolStripMenu in MenuControls</summary>
            public ToolStrip ToolStrip { get; private set; }
        }

        /// <summary>
        /// Encapsulates WinForms controls instantiated for a CommandInfo instance</summary>
        public class CommandControls : IDisposable
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="menuItem">ToolStripMenuItem</param>
            /// <param name="button">ToolStripButton</param>
            public CommandControls(ToolStripMenuItem menuItem, ToolStripButton button)
            {
                MenuItem = menuItem;
                Button = button;
            }

            /// <summary>
            /// Gets ToolStripMenuItem</summary>
            public ToolStripMenuItem MenuItem { get; private set; }

            /// <summary>
            /// Gets ToolStripButton</summary>
            public ToolStripButton Button { get; private set; }

            /// <summary>
            /// Disposes of resources</summary>
            public void Dispose()
            {
                if (MenuItem != null) MenuItem.Dispose();
                if (Button != null) Button.Dispose();
            }
        }

        /// <summary>
        /// Obtains the "dummy" WinForms ToolStripMenuItem for a given MenuInfo instance</summary>
        /// <param name="info">MenuInfo instance</param>
        /// <returns>"Dummy" ToolStripMenuItem for a given MenuInfo instance</returns>
        public ToolStripMenuItem GetMenuToolStripItem(MenuInfo info)
        {
            if (info == null)
                throw new NullReferenceException("MenuInfo argument cannot be null");

            return m_menuToolStripItems[info];
        }

        /// <summary>
        /// Gets the WinForms ToolStrip for a given MenuInfo instance</summary>
        /// <param name="info">MenuInfo instance</param>
        /// <returns>WinForms ToolStrip for given MenuInfo instance</returns>
        public ToolStrip GetMenuToolStrip(MenuInfo info)
        {
            if (info == null)
                throw new NullReferenceException("MenuInfo argument cannot be null");

            return m_menuToolStrips[info];
        }

        /// <summary>
        /// Sets the WinForms ToolStrip for a given MenuInfo instance. Prevents RegisterMenuInfo()
        /// from creating one for it.</summary>
        /// <param name="info">MenuInfo</param>
        /// <param name="toolStrip">ToolStrip</param>
        public void SetMenuToolStrip(MenuInfo info, ToolStrip toolStrip)
        {
            if (info == null)
                throw new NullReferenceException("MenuInfo argument cannot be null");

            if (toolStrip == null)
                throw new NullReferenceException("ToolStrip argument cannot be null");

            m_menuToolStrips.Add(info, toolStrip);
        }

        /// <summary>
        /// Gets the WinForms controls for a given CommandInfo instance</summary>
        /// <param name="info">CommandInfo instance</param>
        /// <returns>WinForms controls for given CommandInfo instance</returns>
        public CommandControls GetCommandControls(CommandInfo info)
        {
            if (info == null)
                throw new NullReferenceException("CommandInfo argument cannot be null");

            return m_commandControls[info];
        }
        
        private readonly Dictionary<MenuInfo, ToolStrip> m_menuToolStrips =
            new Dictionary<MenuInfo, ToolStrip>();

        private readonly Dictionary<MenuInfo, ToolStripMenuItem> m_menuToolStripItems =
            new Dictionary<MenuInfo, ToolStripMenuItem>();

        private readonly Dictionary<CommandInfo, CommandControls> m_commandControls =
            new Dictionary<CommandInfo, CommandControls>();

        //  The CheckCanDo event on these CommandInfos is supported by the associated
        //  non-null ICommandClients. The temporary to-update list is for one-time updates
        //  and as a side benefit, this throttles any over-active ICommandClients.
        private readonly Multimap<CommandInfo, ICommandClient> m_checkCanDoClients =
            new Multimap<CommandInfo, ICommandClient>();
        private readonly HashSet<CommandInfo> m_checkCanDoClientsToUpdate =
            new HashSet<CommandInfo>();

        private readonly Form m_mainForm;

        private ToolStripContainer m_toolStripContainer;
        private readonly MenuStrip m_mainMenuStrip;

        private bool m_commandsSorted;

        private Point m_menuMouseLocation = Point.Empty;
        private CommandInfo m_mouseIsOverCommandIcon;
        private ToolStripEx m_lastHoveringToolStrip;
        private DispatcherTimer m_webHelpTimer;
    }

    /// <summary>
    /// Useful extension methods for ICommandService that are specific to WinForms</summary>
    public static class WinFormsCommandServices
    {
        /// <summary>
        /// Registers a command for the command client</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="commandTag">Command's unique ID</param>
        /// <param name="menuTag">Containing menu's unique ID, or null</param>
        /// <param name="groupTag">Containing menu group's unique ID, or null</param>
        /// <param name="menuText">Command text as it appears in menu</param>
        /// <param name="description">Command description</param>
        /// <param name="shortcut">Command shortcut, or Keys.None if none</param>
        /// <param name="imageName">Text identifying image, or null if none</param>
        /// <param name="client">Client that performs command</param>
        /// <returns>CommandInfo object describing command</returns>
        public static CommandInfo RegisterCommand(
            this ICommandService commandService,
            object commandTag,
            object menuTag,
            object groupTag,
            string menuText,
            string description,
            System.Windows.Forms.Keys shortcut,
            string imageName,
            ICommandClient client)
        {
            CommandInfo info = new CommandInfo(commandTag, menuTag, groupTag, menuText, description, KeysInterop.ToAtf(shortcut), imageName);
            commandService.RegisterCommand(info, client);
            return info;
        }

        /// <summary>
        /// Registers a command for the command client</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="commandTag">Command's unique ID</param>
        /// <param name="menuTag">Containing menu's unique ID, or null</param>
        /// <param name="groupTag">Containing menu group's unique ID, or null</param>
        /// <param name="menuText">Command text as it appears in menu</param>
        /// <param name="description">Command description</param>
        /// <param name="shortcut">Command shortcut, or Keys.None if none</param>
        /// <param name="imageName">Text identifying image, or null if none</param>
        /// <param name="visibility">Value describing whether command is visible in menus and toolbars</param>
        /// <param name="client">Client that performs command</param>
        /// <returns>CommandInfo object describing command</returns>
        public static CommandInfo RegisterCommand(
            this ICommandService commandService,
            object commandTag,
            object menuTag,
            object groupTag,
            string menuText,
            string description,
            System.Windows.Forms.Keys shortcut,
            string imageName,
            CommandVisibility visibility,
            ICommandClient client)
        {
            CommandInfo info = new CommandInfo(commandTag, menuTag, groupTag, menuText, description, KeysInterop.ToAtf(shortcut), imageName, visibility);
            commandService.RegisterCommand(info, client);
            return info;
        }

        /// <summary>
        /// Processes the key as a command shortcut</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="key">Key to process</param>
        /// <returns><c>True</c> if the key was processed as a command shortcut</returns>
        static public bool ProcessKey(this ICommandService commandService, System.Windows.Forms.Keys key)
        {
            return commandService.ProcessKey(KeysInterop.ToAtf(key));
        }
    }
}
