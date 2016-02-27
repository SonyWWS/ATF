//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sce.Atf.Input;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Menu/Toolbar command information. Is used either directly or indirectly with ICommandService to
    /// provide configuration information about how a command is presented to the user. Includes menu
    /// text, toolbar icons, shortcut keys, and visibility flags. The properties are "live" and when
    /// changed on a CommandInfo object that has been registered with a command service, the user
    /// interface is changed.</summary>
    public class CommandInfo
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandTag">Unique command identifier</param>
        /// <param name="menuTag">Unique menu identifier</param>
        /// <param name="groupTag">Unique group identifier</param>
        /// <param name="menuText">Menu text</param>
        /// <param name="description">Command description</param>
        public CommandInfo(
            object commandTag,
            object menuTag,
            object groupTag,
            string menuText,
            string description)
            : this(commandTag, menuTag, groupTag, menuText, description, Keys.None, null, CommandVisibility.Menu)
        {
        }

        /// <summary>
        /// Constructor setting the Visibility to menu</summary>
        /// <param name="commandTag">Unique command identifier</param>
        /// <param name="menuTag">Unique menu identifier</param>
        /// <param name="groupTag">Unique group identifier</param>
        /// <param name="menuText">Menu text</param>
        /// <param name="description">Command description</param>
        /// <param name="shortcut">Default keyboard shortcut. Use bitwise OR for key combos 
        /// (eg, "Keys.Ctrl | Keys.W"), or "Keys.None" for no shortcut.</param>
        /// <remarks>Added by RJG</remarks>
        public CommandInfo(
            object commandTag,
            object menuTag,
            object groupTag,
            string menuText,
            string description,
            Keys shortcut)
            : this(commandTag, menuTag, groupTag, menuText, description, shortcut, null, CommandVisibility.Menu)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="commandTag">Unique command identifier</param>
        /// <param name="menuTag">Unique menu identifier</param>
        /// <param name="groupTag">Unique group identifier</param>
        /// <param name="menuText">Menu text</param>
        /// <param name="description">Command description</param>
        /// <param name="shortcut">Default keyboard shortcut. Use bitwise OR for key combos 
        /// (eg, "Keys.Ctrl | Keys.W"), or "Keys.None" for no shortcut.</param>
        /// <param name="imageName">Name of image resource, or null</param>
        public CommandInfo(
            object commandTag,
            object menuTag,
            object groupTag,
            string menuText,
            string description,
            Keys shortcut,
            string imageName)
            : this(commandTag, menuTag, groupTag, menuText, description, shortcut, imageName, CommandVisibility.Default)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="commandTag">Unique command identifier</param>
        /// <param name="menuTag">Unique menu identifier</param>
        /// <param name="groupTag">Unique group identifier</param>
        /// <param name="menuText">Menu text</param>
        /// <param name="description">Command description</param>
        /// <param name="shortcuts">Default keyboard shortcuts - any collection implementing IEnumerable&lt;Keys&gt;. 
        /// Use bitwise OR for key-combos (eg, "Key.Control | Key.W"), or "Keys.None" for no shortcuts.</param>
        /// <param name="imageName">Name of image resource, or null</param>
        public CommandInfo(
            object commandTag,
            object menuTag,
            object groupTag,
            string menuText,
            string description,
            IEnumerable<Keys> shortcuts,
            string imageName)
            : this(commandTag, menuTag, groupTag, menuText, description, shortcuts, imageName, CommandVisibility.Default)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="commandTag">Unique command identifier</param>
        /// <param name="menuTag">Unique menu identifier</param>
        /// <param name="groupTag">Unique group identifier</param>
        /// <param name="menuText">Menu text</param>
        /// <param name="description">Command description</param>
        /// <param name="shortcut">Default keyboard shortcut. Use bitwise OR for key combos 
        /// (eg, "Keys.Ctrl | Keys.W"), or "Keys.None" for no shortcut.</param>
        /// <param name="imageName">Name of image resource, or null</param>
        /// <param name="visibility">Command visibility in menus and toolbars. CommandVisibility.Default is
        /// the default.</param>
        /// <param name="helpUrl">URL to open when the user presses F1 and the tool strip button has focus.
        /// A message will be added to the tooltip, to indicate that F1 help is available.</param>
        public CommandInfo(
            object commandTag,
            object menuTag,
            object groupTag,
            string menuText,
            string description,
            Keys shortcut,
            string imageName,
            CommandVisibility visibility,
            string helpUrl = null)
        {
            CommandTag = commandTag;
            MenuTag = menuTag;
            GroupTag = groupTag;
            MenuText = menuText;
            Description = description;
            DefaultShortcuts = new[] { shortcut };
            Shortcuts = new[] { shortcut };
            ImageName = imageName;
            Visibility = visibility;
            HelpUrl = helpUrl;
            
            ShortcutsEditable = true;
            ShortcutsChanged += (e, s) => RebuildShortcutKeyDisplayString();
            ShortcutsChanged.Raise(this, EventArgs.Empty);
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="commandTag">Unique command identifier</param>
        /// <param name="menuTag">Unique menu identifier</param>
        /// <param name="groupTag">Unique group identifier</param>
        /// <param name="menuText">Menu text</param>
        /// <param name="description">Command description</param>
        /// <param name="shortcuts">Default keyboard shortcuts - any collection implementing IEnumerable&lt;Keys&gt;.
        /// Use bitwise OR for key-combos (eg, "Key.Control | Key.W"), or "Keys.None" for no shortcuts.</param>
        /// <param name="imageName">Name of image resource, or null</param>
        /// <param name="visibility">Command visibility in menus and toolbars</param>
        /// <param name="helpUrl">URL to open when the user presses F1 and the tool strip button has focus.
        /// A message will be added to the tooltip, to indicate that F1 help is available.</param>
        public CommandInfo(
            object commandTag,
            object menuTag,
            object groupTag,
            string menuText,
            string description,
            IEnumerable<Keys> shortcuts,
            string imageName,
            CommandVisibility visibility,
            string helpUrl = null)
        {
            CommandTag = commandTag;
            MenuTag = menuTag;
            GroupTag = groupTag;
            MenuText = menuText;
            Description = description;
            DefaultShortcuts = shortcuts;
            Shortcuts = shortcuts;
            ImageName = imageName;
            Visibility = visibility;
            HelpUrl = helpUrl;
            
            ShortcutsEditable = true;
            ShortcutsChanged += (e, s) => RebuildShortcutKeyDisplayString();
            ShortcutsChanged.Raise(this, EventArgs.Empty);
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="commandTag">Unique command identifier</param>
        /// <param name="menuTag">Unique menu identifier</param>
        /// <param name="groupTag">Unique group identifier</param>
        /// <param name="menuText">Menu text</param>
        /// <param name="description">Command description</param>
        /// <param name="shortcut">Default keyboard shortcut. Use bitwise OR for key combos 
        /// (eg, "Keys.Ctrl | Keys.W"), or "Keys.None" for no shortcut.</param>
        /// <param name="imageKey">Key to identify image resource, or null</param>
        public CommandInfo(
            object commandTag,
            object menuTag,
            object groupTag,
            string menuText,
            string description,
            Keys shortcut,
            object imageKey)
            : this(commandTag, menuTag, groupTag, menuText, description, shortcut, imageKey, CommandVisibility.Default)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="commandTag">Unique command identifier</param>
        /// <param name="menuTag">Unique menu identifier</param>
        /// <param name="groupTag">Unique group identifier</param>
        /// <param name="menuText">Menu text</param>
        /// <param name="description">Command description</param>
        /// <param name="shortcut">Default keyboard shortcut. Use bitwise OR for key combos 
        /// (eg, "Keys.Ctrl | Keys.W"), or "Keys.None" for no shortcut.</param>
        /// <param name="imageKey">Key to identify image resource, or null</param>
        /// <param name="visibility">Command visibility in menus and toolbars</param>
        public CommandInfo(
            object commandTag,
            object menuTag,
            object groupTag,
            string menuText,
            string description,
            Keys shortcut,
            object imageKey,
            CommandVisibility visibility)
        {
            CommandTag = commandTag;
            MenuTag = menuTag;
            GroupTag = groupTag;
            MenuText = menuText;
            Description = description;
            DefaultShortcuts = new[] { shortcut };
            Shortcuts = new[] { shortcut };
            ImageName = imageKey as string;
            ImageKey = imageKey;
            Visibility = visibility;

            ShortcutsEditable = true;
            ShortcutsChanged.Raise(this, EventArgs.Empty);
        }


        /// <summary>
        /// Unique command identifier</summary>
        public readonly object CommandTag;

        /// <summary>
        /// Unique menu identifier</summary>
        public readonly object MenuTag;

        /// <summary>
        /// Unique group identifier</summary>
        public readonly object GroupTag;

        /// <summary>
        /// Initial menu text to display for command</summary>
        public readonly string MenuText;

        /// <summary>
        /// Event handler called when keyboard shortcuts have changed</summary>
        public event EventHandler ShortcutsChanged;

        /// <summary>
        /// Event handler that is called when the Visibility property has changed</summary>
        public event EventHandler VisibilityChanged;

        /// <summary>
        /// Event handler that is called when the owning command client (ICommandClient)
        /// may have changed the CanDoCommand() result or needs to have UpdateCommand()
        /// called. See EnableCheckCanDoEvent() and OnCheckCanDo().</summary>
        /// <remarks>This event is only applicable to WinForms version of CommandService.</remarks>
        public event EventHandler CheckCanDo;

        /// <summary>
        /// Gets the command clients that support the CheckCanDo event on this CommandInfo</summary>
        public IEnumerable<ICommandClient> CheckCanDoClients
        {
            get { return m_checkCanDoClients; }
        }

        /// <summary>
        /// Indicates whether or not the CheckCanDo event is supported. Call this before
        /// this CommandInfo is registered with a command service to have improved performance
        /// by saving the command service from having to poll the command client.</summary>
        /// <param name="client">Command client that will register this CommandInfo</param>
        /// <remarks>This method is only applicable to WinForms version of CommandService.</remarks>
        public void EnableCheckCanDoEvent(ICommandClient client)
        {
            m_checkCanDoClients.Add(client);
        }

        /// <summary>
        /// Raises the CheckCanDo event. ICommandClients can call EnableCheckCanDoEvent before
        /// registering this CommandInfo and then can call this method for greatly improved performance.</summary>
        /// <param name="client">Command client that registered this CommandInfo</param>
        /// <remarks>This method is only applicable to WinForms version of CommandService.</remarks>
        public void OnCheckCanDo(ICommandClient client)
        {
            if (!m_checkCanDoClients.Contains(client))
                throw new InvalidOperationException("Call EnableCheckCanDoEvent before calling OnCheckCanDo");
            CheckCanDo.Raise(this, EventArgs.Empty);
        }

        /// <summary>
        /// Check if the given key is a shortcut key</summary>        
        public bool IsShortcut(Keys key)
        {
            return m_shortcutSet.Contains(key);
        }


        /// <summary>
        /// Gets shortcut display string</summary>
        public string ShortcutKeyDisplayString
        {
            get;
            private set;
        }

        private void RebuildShortcutKeyDisplayString()
        {
            // rebuild on when ShortcutChanged event is raised.

            StringBuilder displayString = new StringBuilder();
            foreach (Keys k in Shortcuts)
            {
                if (k == Keys.None)
                    continue;

                if (displayString.Length > 0)
                    displayString.Append(" ; ");
                displayString.Append(KeysUtil.KeysToString(k, true));
            }

            ShortcutKeyDisplayString = displayString.ToString();
        }

        /// <summary>
        /// Gets or sets the collection of keyboard shortcuts that can activate this command.
        /// For key-combos, use bitwise OR (e.g. "Keys.Control | Keys.S" for Ctrl-S). 
        /// For no shortcuts, pass an enumeration containing only Keys.None.
        /// When setting, a copy is made of the enumeration.</summary>
        public IEnumerable<Keys> Shortcuts
        {
            get { return m_shortcuts; }
            set
            {
                m_shortcuts = new List<Keys>(value);
                m_shortcutSet = new HashSet<Keys>(value);
                ShortcutsChanged.Raise(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets whether the current shortcut keys were the original keys supplied in
        /// the constructor</summary>
        public bool ShortcutsAreDefault
        {
            get
            {
                if (IsEmptyOrNone(m_shortcuts) && IsEmptyOrNone(m_defaultShortcuts))
                    return true;
                return m_defaultShortcuts.SequenceEqual(m_shortcuts);
            }
        }

        /// <summary>
        /// Gets the default shortcut keys that were supplied in the constructor</summary>
        public IEnumerable<Keys> DefaultShortcuts
        {
            get { return m_defaultShortcuts; }
            private set { m_defaultShortcuts = new List<Keys>(value); }
        }

        /// <summary>
        /// Clear out the keyboard shortcuts used to activate this command.  
        /// Subsequently updates the display string.</summary>
        public void ClearShortcuts()
        {
            m_shortcuts.Clear();
            m_shortcutSet.Clear();
            ShortcutsChanged.Raise(this, EventArgs.Empty);
        }

        /// <summary>
        /// Add a keyboard shortcut to the list of keyboard shortcuts for activating this command.  
        /// Subsequently updates the display string.</summary>
        /// <param name="shortcut">Keys for shortcut</param>
        public void AddShortcut(Keys shortcut)
        {
            if (shortcut == Keys.None)
                return;

            m_shortcuts.Remove(Keys.None);
            m_shortcutSet.Remove(Keys.None);

            if (!m_shortcuts.Contains(shortcut))
            {
                m_shortcuts.Add(shortcut);
                m_shortcutSet.Add(shortcut);
            }

            ShortcutsChanged.Raise(this, EventArgs.Empty);
        }

        /// <summary>
        /// Remove a keyboard shortcut from the list of keyboard shortcuts for activating this command.  
        /// Subsequently updates the display string.</summary>
        /// <param name="shortcut">Keys for shortcut</param>
        public void RemoveShortcut(Keys shortcut)
        {
            if (shortcut == Keys.None)
                return;

            if (m_shortcuts.Remove(shortcut))
            {
                m_shortcutSet.Remove(shortcut);
                ShortcutsChanged.Raise(this, EventArgs.Empty);
            }
                
        }

        /// <summary>
        /// Gets or sets the ICommandService to which this command info is registered</summary>
        public ICommandService CommandService
        {
            get { return m_commandService; }
            set
            {
                if (m_commandService != null)
                    throw new InvalidOperationException("CommandInfo already has been registered");
                m_commandService = value;
            }
        }

        /// <summary>
        /// Name of image resource, or null</summary>
        public string ImageName;

        /// <summary>
        /// Image key of image resource, or null</summary>
        public object ImageKey;

        /// <summary>
        /// Actual text displayed in menu for this command</summary>
        public string DisplayedMenuText;

        /// <summary>
        /// Command description. Is used for the tooltip text and for the status service.</summary>
        public readonly string Description;

        /// <summary>
        /// Gets or sets the command visibility in menus and toolbars</summary>
        public CommandVisibility Visibility
        {
            get { return m_visibility; }
            set
            {
                if (m_visibility != value)
                {
                    m_visibility = value;
                    VisibilityChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets a URL that will be opened when the user presses F1 while the tooltip
        /// is showing on this command's icon in a toolstrip (if the URL is not null or empty.</summary>
        public string HelpUrl;

        /// <summary>
        /// Gets or sets a value indicating whether the item should automatically appear checked and unchecked when clicked</summary>    
        public bool CheckOnClick { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the defined shortcuts are editable</summary>
        /// <remarks>A false value will make the command not listed in CustomizeKeyboardDialog</remarks>
        public bool ShortcutsEditable { get; set; }
    

        /// <summary>
        /// A globally unique ID for this command. Is useful for a stable sort.</summary>
        public readonly int Index = s_count++;

        /// <summary>
        /// Standard File/Save command</summary>
        public static CommandInfo FileSave =
            new CommandInfo(
                StandardCommand.FileSave,
                StandardMenu.File,
                StandardCommandGroup.FileSave,
                "Save".Localize("Save the active file"),
                "Save the active file".Localize(),
                Keys.Control | Keys.S,
                Resources.SaveImage,
                CommandVisibility.Default,
                "https://github.com/SonyWWS/ATF/wiki/Using-Standard-Command-Components".Localize());

        /// <summary>
        /// Standard File/SaveAs command</summary>
        public static CommandInfo FileSaveAs =
            new CommandInfo(
                StandardCommand.FileSaveAs,
                StandardMenu.File,
                StandardCommandGroup.FileSave,
                "Save As ...".Localize("Save the active file under a new name"),
                "Save the active file under a new name".Localize(),
                Keys.None,
                Resources.SaveAsImage,
                CommandVisibility.Default,
                "https://github.com/SonyWWS/ATF/wiki/Using-Standard-Command-Components".Localize());

        /// <summary>
        /// Standard File/SaveAll command</summary>
        public static CommandInfo FileSaveAll =
            new CommandInfo(
                StandardCommand.FileSaveAll,
                StandardMenu.File,
                StandardCommandGroup.FileSave,
                "Save All".Localize("Saves all open files"),
                "Save all open files".Localize(),
                Keys.Control | Keys.Shift | Keys.S,
                Resources.SaveAllImage,
                CommandVisibility.Default,
                "https://github.com/SonyWWS/ATF/wiki/Using-Standard-Command-Components".Localize());

        /// <summary>
        /// Standard File/Close command</summary>
        public static CommandInfo FileClose =
            new CommandInfo(
                StandardCommand.FileClose,
                StandardMenu.File,
                StandardCommandGroup.FileSave,
                "Close".Localize("Close the active file"),
                "Close the active file".Localize(),
                new[] {(Keys.Control | Keys.W), // Ctrl-w *or* Ctrl-F4 will trigger this command
                            (Keys.Control | Keys.F4)},
                null,
                CommandVisibility.Menu);

        /// <summary>
        /// Standard File/Print command</summary>
        public static CommandInfo FilePrint =
            new CommandInfo(
                StandardCommand.Print,
                StandardMenu.File,
                StandardCommandGroup.FilePrint,
                "Print...".Localize("Print the active document"),
                "Print the active document".Localize(),
                Keys.Control | Keys.P,
                Resources.PrinterImage);

        /// <summary>
        /// Standard File/PageSetup command</summary>
        public static CommandInfo FilePageSetup =
            new CommandInfo(
                StandardCommand.PageSetup,
                StandardMenu.File,
                StandardCommandGroup.FilePrint,
                "Page Setup...".Localize("Set up page for printing"),
                "Set up page for printing".Localize(),
                Keys.None,
                Resources.PrinterPreferencesImage);

        /// <summary>
        /// Standard File/PrintPreview command</summary>
        public static CommandInfo FilePrintPreview =
            new CommandInfo(
                StandardCommand.PrintPreview,
                StandardMenu.File,
                StandardCommandGroup.FilePrint,
                "Print Preview...".Localize("Show a print preview of the active document"),
                "Show a print preview of the active document".Localize(),
                Keys.None,
                Resources.PrinterViewImage);

        /// <summary>
        /// Standard File/Exit command</summary>
        public static CommandInfo FileExit =
            new CommandInfo(
                StandardCommand.FileExit,
                StandardMenu.File,
                StandardCommandGroup.FileExit,
                "Exit".Localize("Exit the application"),
                "Exit Application".Localize());

        /// <summary>
        /// Standard Edit/Undo command</summary>
        public static CommandInfo EditUndo =
            new CommandInfo(
                StandardCommand.EditUndo,
                StandardMenu.Edit,
                StandardCommandGroup.EditUndo,
                "Undo".Localize("Undo the last change"),
                "Undo the last change".Localize(),
                Keys.Control | Keys.Z,
                Resources.UndoImage);

        /// <summary>
        /// Standard Edit/Redo command</summary>
        public static CommandInfo EditRedo =
            new CommandInfo(
                StandardCommand.EditRedo,
                StandardMenu.Edit,
                StandardCommandGroup.EditUndo,
                "Redo".Localize("Redo the last edit"),
                "Redo the last edit".Localize(),
                Keys.Control | Keys.Y,
                Resources.RedoImage);

        /// <summary>
        /// Standard Edit/Cut command</summary>
        public static CommandInfo EditCut =
            new CommandInfo(
                StandardCommand.EditCut,
                StandardMenu.Edit,
                StandardCommandGroup.EditCut,
                "Cut".Localize("Cut the selection and place it on the clipboard"),
                "Cut the selection and place it on the clipboard".Localize(),
                Keys.Control | Keys.X,
                Resources.CutImage);

        /// <summary>
        /// Standard Edit/Copy command</summary>
        public static CommandInfo EditCopy =
            new CommandInfo(
                StandardCommand.EditCopy,
                StandardMenu.Edit,
                StandardCommandGroup.EditCut,
                "Copy".Localize("Copy the selection and place it on the clipboard"),
                "Copy the selection and place it on the clipboard".Localize(),
                Keys.Control | Keys.C,
                Resources.CopyImage);

        /// <summary>
        /// Standard Edit/Paste command</summary>
        public static CommandInfo EditPaste =
            new CommandInfo(
                StandardCommand.EditPaste,
                StandardMenu.Edit,
                StandardCommandGroup.EditCut,
                "Paste".Localize("Paste the contents of the clipboard and make that the new selection"),
                "Paste the contents of the clipboard and make that the new selection".Localize(),
                Keys.Control | Keys.V,
                Resources.PasteImage);

        /// <summary>
        /// Standard Edit/Delete command</summary>
        public static CommandInfo EditDelete =
            new CommandInfo(
                StandardCommand.EditDelete,
                StandardMenu.Edit,
                StandardCommandGroup.EditCut,
                "Delete".Localize("Delete the selection"),
                "Delete the selection".Localize(),
                Keys.Delete,
                Resources.DeleteImage);

        /// <summary>
        /// Standard Edit/SelectAll command</summary>
        public static CommandInfo EditSelectAll =
            new CommandInfo(
                StandardCommand.EditSelectAll,
                StandardMenu.Edit,
                StandardCommandGroup.EditSelectAll,
                "Select All".Localize("Select all items"),
                "Select all items".Localize(),
                Keys.Control | Keys.A);

        /// <summary>
        /// Standard Edit/DeselectAll command</summary>
        public static CommandInfo EditDeselectAll =
            new CommandInfo(
                StandardCommand.EditDeselectAll,
                StandardMenu.Edit,
                StandardCommandGroup.EditSelectAll,
                "Deselect All".Localize("Deselect all items"),
                "Deselect all items".Localize());

        /// <summary>
        /// Standard Edit/InvertSelection command</summary>
        public static CommandInfo EditInvertSelection =
            new CommandInfo(
                StandardCommand.EditInvertSelection,
                StandardMenu.Edit,
                StandardCommandGroup.EditSelectAll,
                "Invert Selection".Localize("Select unselected items and deselect selected items"),
                "Select unselected items and deselect selected items".Localize());

        /// <summary>
        /// Standard Edit/Lock command</summary>
        public static CommandInfo EditLock =
            new CommandInfo(
                StandardCommand.EditLock,
                StandardMenu.Edit,
                StandardCommandGroup.EditOther,
                "Lock".Localize(),
                "Lock the selection to disable editing".Localize(),
                Keys.Control | Keys.L,
                Resources.LockImage);

        /// <summary>
        /// Standard Edit/Unlock command</summary>
        public static CommandInfo EditUnlock =
            new CommandInfo(
                StandardCommand.EditUnlock,
                StandardMenu.Edit,
                StandardCommandGroup.EditOther,
                "Unlock".Localize("Unlock the selection to enable editing"),
                "Unlock the selection to enable editing".Localize(),
                Keys.Control | Keys.Shift | Keys.L,
                Resources.UnlockImage);

        /// <summary>
        /// Standard User Interface Lock command</summary>
        public static CommandInfo UILock =
            new CommandInfo(
                StandardCommand.UILock,
                StandardMenu.Window,
                StandardCommandGroup.UILayout,
                "Lock UI Layout".Localize(),
                "Lock UI Layout".Localize(),
                Keys.None,
                Resources.UnlockUIImage,
                CommandVisibility.Default,
                "https://github.com/SonyWWS/ATF/wiki/Using-Standard-Command-Components".Localize());

        /// <summary>
        /// Standard Edit/Group command</summary>
        public static CommandInfo EditGroup =
            new CommandInfo(
                StandardCommand.EditGroup,
                StandardMenu.Edit,
                StandardCommandGroup.EditOther,
                "Group".Localize("Group the selection into a single item"),
                "Group the selection into a single item".Localize(),
                Keys.Control | Keys.G,
                Resources.GroupImage);



        /// <summary>
        /// Standard Edit/Ungroup command</summary>
        public static CommandInfo EditUngroup =
            new CommandInfo(
                StandardCommand.EditUngroup,
                StandardMenu.Edit,
                StandardCommandGroup.EditOther,
                "Ungroup".Localize("Ungroup any selected groups"),
                "Ungroup any selected groups".Localize(),
                Keys.Control | Keys.Shift | Keys.G,
                Resources.UngroupImage);

        /// <summary>
        /// Standard View/Hide command</summary>
        public static CommandInfo ViewHide =
            new CommandInfo(
                StandardCommand.ViewHide,
                StandardMenu.View,
                StandardCommandGroup.ViewShow,
                "Hide".Localize("Hide all selected objects"),
                "Hide all selected objects".Localize(),
                Keys.H, //Maya 8.5 default is ctrl+h
                Resources.HideImage);

        /// <summary>
        /// Standard View/Show command</summary>
        public static CommandInfo ViewShow =
            new CommandInfo(
                StandardCommand.ViewShow,
                StandardMenu.View,
                StandardCommandGroup.ViewShow,
                "Show".Localize("Show all selected objects"),
                "Show all selected objects".Localize(),
                Keys.Shift | Keys.H, //Maya 8.5 default is shift+h
                Resources.ShowImage);

        /// <summary>
        /// Standard View/Show Last command</summary>
        public static CommandInfo ViewShowLast =
            new CommandInfo(
                StandardCommand.ViewShowLast,
                StandardMenu.View,
                StandardCommandGroup.ViewShow,
                "Show Last Hidden".Localize("Show the last hidden object"),
                "Show the last hidden object".Localize(),
                Keys.Control | Keys.H,
                Resources.ShowLastImage);

        /// <summary>
        /// Standard View/Show All command</summary>
        public static CommandInfo ViewShowAll =
            new CommandInfo(
                StandardCommand.ViewShowAll,
                StandardMenu.View,
                StandardCommandGroup.ViewShow,
                "Show All".Localize("Show all hidden objects"),
                "Show all hidden objects".Localize(),
                Keys.Shift | Keys.R,
                Resources.ShowAllImage);

        /// <summary>
        /// Standard View/Show Isolate command</summary>
        public static CommandInfo ViewIsolate =
            new CommandInfo(
                StandardCommand.ViewIsolate,
                StandardMenu.View,
                StandardCommandGroup.ViewShow,
                "Isolate".Localize("Show only the selected objects and hide all others"),
                "Show only the selected objects and hide all others".Localize(),
                Keys.I,
                Resources.IsolateImage);

        /// <summary>
        /// Standard View/Frame Selection command</summary>
        public static CommandInfo ViewFrameSelection =
            new CommandInfo(
                StandardCommand.ViewFrameSelection,
                StandardMenu.View,
                StandardCommandGroup.ViewZoomIn,
                "Frame Selection".Localize("Frame selection in view"),
                "Frames all selected objects in the current view".Localize(),
                Keys.F,
                Resources.FitToSizeImage);

        /// <summary>
        /// Standard View/Frame All command</summary>
        public static CommandInfo ViewFrameAll =
            new CommandInfo(
                StandardCommand.ViewFrameAll,
                StandardMenu.View,
                StandardCommandGroup.ViewZoomIn,
                "Frame All".Localize("Frame all objects in view"),
                "Frames all objects in the current view".Localize(),
                Keys.Shift | Keys.F,
                null);

        /// <summary>
        /// Standard View/Zoom In command</summary>
        public static CommandInfo ViewZoomIn =
            new CommandInfo(
                StandardCommand.ViewZoomIn,
                StandardMenu.View,
                StandardCommandGroup.ViewZoomIn,
                "Zoom In".Localize("Zoom In"),
                "Zoom In".Localize(),
                Keys.Control | Keys.Oemplus,
                null);

        /// <summary>
        /// Standard View//Zoom Out command</summary>
        public static CommandInfo ViewZoomOut =
            new CommandInfo(
                StandardCommand.ViewZoomOut,
                StandardMenu.View,
                StandardCommandGroup.ViewZoomIn,
                "Zoom Out".Localize("Zoom Out"),
                "Zoom Out".Localize(),
                Keys.Control | Keys.OemMinus,
                null);

        /// <summary>
        /// Standard View//Zoom Reset command</summary>
        public static CommandInfo ViewZoomReset =
            new CommandInfo(
                StandardCommand.ViewZoomReset,
                StandardMenu.View,
                StandardCommandGroup.ViewZoomIn,
                "Zoom Reset".Localize("Zoom Reset"),
                "Zoom Reset".Localize(),
                Keys.Control | Keys.D0,
                null);

        /// <summary>
        /// Standard View//Zoom Extents command</summary>
        public static CommandInfo ViewZoomExtents =
            new CommandInfo(
                StandardCommand.ViewZoomExtents,
                StandardMenu.View,
                StandardCommandGroup.ViewZoomIn,
                "Fit In Active View".Localize("Pan and Zoom to center selection"),
                "Pan and Zoom to center selection".Localize(),
                Keys.F,
                Resources.FitToSizeImage);

        /// <summary>
        /// Standard Format/Align Lefts command</summary>
        public static CommandInfo FormatAlignLefts =
            new CommandInfo(
                StandardCommand.FormatAlignLefts,
                StandardMenu.Format,
                StandardCommandGroup.FormatAlign,
                "Align/Lefts".Localize("Align left sides of selected items"),
                "Align left sides of selected items".Localize(),
                Keys.None,
                Resources.AlignLeftsImage,
                CommandVisibility.Default,
                "https://github.com/SonyWWS/ATF/wiki/Using-Standard-Command-Components".Localize());

        /// <summary>
        /// Standard Format/Align Rights command</summary>
        public static CommandInfo FormatAlignRights =
            new CommandInfo(
                StandardCommand.FormatAlignRights,
                StandardMenu.Format,
                StandardCommandGroup.FormatAlign,
                "Align/Rights".Localize("Align right sides of selected items"),
                "Align right sides of selected items".Localize(),
                Keys.None,
                Resources.AlignRightsImage,
                CommandVisibility.Default,
                "https://github.com/SonyWWS/ATF/wiki/Using-Standard-Command-Components".Localize());

        /// <summary>
        /// Standard Format/Align Centers command</summary>
        public static CommandInfo FormatAlignCenters =
            new CommandInfo(
                StandardCommand.FormatAlignCenters,
                StandardMenu.Format,
                StandardCommandGroup.FormatAlign,
                "Align/Centers".Localize("Align centers of selected items"),
                "Align centers of selected items".Localize(),
                Keys.None,
                Resources.AlignCentersImage,
                CommandVisibility.Default,
                "https://github.com/SonyWWS/ATF/wiki/Using-Standard-Command-Components".Localize());

        /// <summary>
        /// Standard Format/Align Tops command</summary>
        public static CommandInfo FormatAlignTops =
            new CommandInfo(
                StandardCommand.FormatAlignTops,
                StandardMenu.Format,
                StandardCommandGroup.FormatAlign,
                "Align/Tops".Localize("Align tops of selected items"),
                "Align tops of selected items".Localize(),
                Keys.None,
                Resources.AlignTopsImage,
                CommandVisibility.Default,
                "https://github.com/SonyWWS/ATF/wiki/Using-Standard-Command-Components".Localize());

        /// <summary>
        /// Standard Format/Align Bottoms command</summary>
        public static CommandInfo FormatAlignBottoms =
            new CommandInfo(
                StandardCommand.FormatAlignBottoms,
                StandardMenu.Format,
                StandardCommandGroup.FormatAlign,
                "Align/Bottoms".Localize("Align bottoms of selected items"),
                "Align bottoms of selected items".Localize(),
                Keys.None,
                Resources.AlignBottomsImage,
                CommandVisibility.Default,
                "https://github.com/SonyWWS/ATF/wiki/Using-Standard-Command-Components".Localize());

        /// <summary>
        /// Standard Format/Align Middles command</summary>
        public static CommandInfo FormatAlignMiddles =
            new CommandInfo(
                StandardCommand.FormatAlignMiddles,
                StandardMenu.Format,
                StandardCommandGroup.FormatAlign,
                "Align/Middles".Localize(),
                "Align middles of selected items".Localize(),
                Keys.None,
                Resources.AlignMiddlesImage,
                CommandVisibility.Default,
                "https://github.com/SonyWWS/ATF/wiki/Using-Standard-Command-Components".Localize());

        /// <summary>
        /// Standard Format/Align To Grid command</summary>
        public static CommandInfo FormatAlignToGrid =
            new CommandInfo(
                StandardCommand.FormatAlignToGrid,
                StandardMenu.Format,
                StandardCommandGroup.FormatAlign,
                "Align/To Grid".Localize("Align selected items to x/y grid"),
                "Align selected items to x/y grid".Localize());

        /// <summary>
        /// Standard Format/Make Size Equal command</summary>
        public static CommandInfo FormatMakeSizeEqual =
            new CommandInfo(
                StandardCommand.FormatMakeSizeEqual,
                StandardMenu.Format,
                StandardCommandGroup.FormatAlign,
                "Size/Make Equal".Localize("Make selected items have the same size"),
                "Make selected items have the same size".Localize());

        /// <summary>
        /// Standard Format/Make Width Equal command</summary>
        public static CommandInfo FormatMakeWidthEqual =
            new CommandInfo(
                StandardCommand.FormatMakeWidthEqual,
                StandardMenu.Format,
                StandardCommandGroup.FormatAlign,
                "Size/Make Widths Equal".Localize("Make selected items have the same width"),
                "Make selected items have the same width".Localize());

        /// <summary>
        /// Standard Format/Make Height Equal command</summary>
        public static CommandInfo FormatMakeHeightEqual =
            new CommandInfo(
                StandardCommand.FormatMakeHeightEqual,
                StandardMenu.Format,
                StandardCommandGroup.FormatAlign,
                "Size/Make Heights Equal".Localize("Make selected items have the same height"),
                "Make selected items have the same height".Localize());

        /// <summary>
        /// Standard Format/Size To Grid command</summary>
        public static CommandInfo FormatSizeToGrid =
            new CommandInfo(
                StandardCommand.FormatSizeToGrid,
                StandardMenu.Format,
                StandardCommandGroup.FormatAlign,
                "Size/Size to Grid".Localize("Make selected items sizes align to x/y grid"),
                "Make selected items sizes align to x/y grid".Localize());

        /// <summary>
        /// Standard Window/Split Horizontally command</summary>
        public static CommandInfo WindowSplitHoriz =
            new CommandInfo(
                StandardCommand.WindowSplitHoriz,
                StandardMenu.Window,
                StandardCommandGroup.WindowSplit,
                "Split Horizontal".Localize("Split the window horizontally"),
                "Split the window horizontally".Localize());

        /// <summary>
        /// Standard Window/Split Vertically command</summary>
        public static CommandInfo WindowSplitVert =
            new CommandInfo(
                StandardCommand.WindowSplitVert,
                StandardMenu.Window,
                StandardCommandGroup.WindowSplit,
                "Split Vertical".Localize("Split the window vertically"),
                "Split the window vertically".Localize());

        /// <summary>
        /// Standard Window/Remove Split command</summary>
        public static CommandInfo WindowRemoveSplit =
            new CommandInfo(
                StandardCommand.WindowRemoveSplit,
                StandardMenu.Window,
                StandardCommandGroup.WindowSplit,
                "Remove Split".Localize("Remove the split"),
                "Remove the split".Localize());

        /// <summary>
        /// Standard Window/Tile Horizontal command</summary>
        public static CommandInfo WindowTileHorizontal =
            new CommandInfo(
                StandardCommand.WindowTileHorizontal,
                StandardMenu.Window,
                StandardCommandGroup.WindowTile,
                "Tile Horizontal".Localize("Tile Documents Horizontally"),
                "Tile the documents, as separate visible items, horizontally".Localize());

        /// <summary>
        /// Standard Window/Tile Vertical command</summary>
        public static CommandInfo WindowTileVertical =
            new CommandInfo(
                StandardCommand.WindowTileVertical,
                StandardMenu.Window,
                StandardCommandGroup.WindowTile,
                "Tile Vertical".Localize("Tile Documents Vertically"),
                "Tile the documents, as separate visible items, vertically".Localize());

        /// <summary>
        /// Standard Window/Tile Tabbed command</summary>
        public static CommandInfo WindowTileTabbed =
            new CommandInfo(
                StandardCommand.WindowTileTabbed,
                StandardMenu.Window,
                StandardCommandGroup.WindowTile,
                "Tile Overlapping".Localize("Tile Documents Overlapping"),
                "Tile the documents, all together as tabbed items, in the central region of the application".Localize());

        /// <summary>
        /// Standard Help/About command</summary>
        public static CommandInfo HelpAbout =
            new CommandInfo(
                StandardCommand.HelpAbout,
                StandardMenu.Help,
                StandardCommandGroup.HelpAbout,
                "&About".Localize("Get information about application"),
                "Get information about application".Localize());

        /// <summary>
        /// Gets the CommandInfo that was created for the given StandardCommand</summary>
        /// <param name="command">Command</param>
        /// <returns>CommandInfo for the given command</returns>
        /// <remarks>If registering commands, consider extension method on ICommandService
        /// that accepts a StandardCommand</remarks>
        public static CommandInfo GetStandardCommand(StandardCommand command)
        {
            switch (command)
            {
                case StandardCommand.FileClose: return FileClose;
                case StandardCommand.FileSave: return FileSave;
                case StandardCommand.FileSaveAs: return FileSaveAs;
                case StandardCommand.FileSaveAll: return FileSaveAll;
                case StandardCommand.FileExit: return FileExit;
                case StandardCommand.PageSetup: return FilePageSetup;
                case StandardCommand.PrintPreview: return FilePrintPreview;
                case StandardCommand.Print: return FilePrint;
                case StandardCommand.EditUndo: return EditUndo;
                case StandardCommand.EditRedo: return EditRedo;
                case StandardCommand.EditCut: return EditCut;
                case StandardCommand.EditCopy: return EditCopy;
                case StandardCommand.EditPaste: return EditPaste;
                case StandardCommand.EditDelete: return EditDelete;
                case StandardCommand.EditSelectAll: return EditSelectAll;
                case StandardCommand.EditDeselectAll: return EditDeselectAll;
                case StandardCommand.EditInvertSelection: return EditInvertSelection;
                case StandardCommand.EditLock: return EditLock;
                case StandardCommand.EditUnlock: return EditUnlock;
                case StandardCommand.EditGroup: return EditGroup;
                case StandardCommand.EditUngroup: return EditUngroup;
                case StandardCommand.ViewFrameSelection: return ViewFrameSelection;
                case StandardCommand.ViewFrameAll: return ViewFrameAll;
                case StandardCommand.ViewZoomIn: return ViewZoomIn;
                case StandardCommand.ViewZoomOut: return ViewZoomOut;
                case StandardCommand.ViewZoomExtents: return ViewZoomExtents;
                case StandardCommand.FormatAlignLefts: return FormatAlignLefts;
                case StandardCommand.FormatAlignRights: return FormatAlignRights;
                case StandardCommand.FormatAlignCenters: return FormatAlignCenters;
                case StandardCommand.FormatAlignTops: return FormatAlignTops;
                case StandardCommand.FormatAlignBottoms: return FormatAlignBottoms;
                case StandardCommand.FormatAlignMiddles: return FormatAlignMiddles;
                case StandardCommand.FormatAlignToGrid: return FormatAlignToGrid;
                case StandardCommand.FormatMakeSizeEqual: return FormatMakeSizeEqual;
                case StandardCommand.FormatMakeWidthEqual: return FormatMakeWidthEqual;
                case StandardCommand.FormatMakeHeightEqual: return FormatMakeHeightEqual;
                case StandardCommand.FormatSizeToGrid: return FormatSizeToGrid;
                case StandardCommand.WindowSplitHoriz: return WindowSplitHoriz;
                case StandardCommand.WindowSplitVert: return WindowSplitVert;
                case StandardCommand.WindowRemoveSplit: return WindowRemoveSplit;
                case StandardCommand.HelpAbout: return HelpAbout;
                case StandardCommand.UILock: return UILock;
                default:
                    throw new NotImplementedException();
            }
        }

        private bool IsEmptyOrNone(IList<Keys> shortcuts)
        {
            // Sometimes the IList can have multiple elements and one of them is Keys.None,
            //  so let's check them all.
            foreach (Keys key in shortcuts)
                if (key != Keys.None)
                    return false;
            return true;
        }

        private static int s_count;

        private List<Keys> m_shortcuts;
        private HashSet<Keys> m_shortcutSet = new HashSet<Keys>();
        private List<Keys> m_defaultShortcuts;
        private CommandVisibility m_visibility;
        private ICommandService m_commandService;
        private readonly HashSet<ICommandClient> m_checkCanDoClients = new HashSet<ICommandClient>();
    }
}
