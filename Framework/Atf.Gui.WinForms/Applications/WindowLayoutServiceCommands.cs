//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf.Controls;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Component for window layout service commands</summary>
    /// <remarks>Provides menu options and GUI's for managing and using layouts</remarks>
    [Export(typeof(IInitializable))]
    [Export(typeof(WindowLayoutServiceCommandsBase))]
    [Export(typeof(WindowLayoutServiceCommands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class WindowLayoutServiceCommands : WindowLayoutServiceCommandsBase
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="windowLayoutService">Window layout service to use</param>
        [ImportingConstructor]
        public WindowLayoutServiceCommands(IWindowLayoutService windowLayoutService)
            : base(windowLayoutService)
        {
        }

        #region IInitializable Implementation

        /// <summary>
        /// Initialize</summary>
        public override void Initialize()
        {
            base.Initialize();

            if (CommandService == null)
                return;

            CommandService.RegisterCommand(
                Command.SaveLayoutAs,
                StandardMenu.Window,
                StandardCommandGroup.UILayout,
                string.Format("{0}{1}{2}", MenuName, MenuSeparator, MenuSaveLayoutAs),
                "Save layout as...".Localize(),
                Input.Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            CommandService.RegisterCommand(
                Command.ManageLayouts,
                StandardMenu.Window,
                StandardCommandGroup.UILayout,
                string.Format("{0}{1}{2}", MenuName, MenuSeparator, MenuManageLayouts),
                "Manage layouts...".Localize(),
                Input.Keys.None,
                null,
                CommandVisibility.Menu,
                this);
        }

        #endregion

        #region ICommandClient Implementation

        /// <summary>
        /// Check if a command can be performed</summary>
        /// <param name="commandTag">Command</param>
        /// <returns><c>True</c> if command can be performed</returns>
        public override bool CanDoCommand(object commandTag)
        {
            return base.CanDoCommand(commandTag) || (commandTag is WindowLayoutServiceCommand);
        }

        /// <summary>
        /// Performs a command</summary>
        /// <param name="commandTag">Command</param>
        public override void DoCommand(object commandTag)
        {
            base.DoCommand(commandTag);

            if (!(commandTag is WindowLayoutServiceCommand))
                return;

            var cmd = (WindowLayoutServiceCommand)commandTag;
            WindowLayoutService.CurrentLayout = cmd.LayoutName;
        }

        /// <summary>
        /// Updates a command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state</param>
        public override void UpdateCommand(object commandTag, CommandState state)
        {
            if (!(commandTag is WindowLayoutServiceCommand))
                return;

            var cmd = (WindowLayoutServiceCommand)commandTag;
            state.Check = WindowLayoutService.IsCurrent(cmd.LayoutName);
        }

        #endregion

        /// <summary>
        /// Gets or sets the main form</summary>
        [Import(AllowDefault = true)]
        public MainForm MainForm { get; set; }

        /// <summary>
        /// Gets or sets the command service to use</summary>
        /// <remarks>If null, no commands can be registered.</remarks>
        [Import(AllowDefault = true)]
        public ICommandService CommandService { get; set; }

        /// <summary>
        /// Show the 'save layout as' dialog</summary>
        /// <remarks>Programmatic method for showing the 'save layout as' dialog</remarks>
        public override void ShowSaveLayoutAsDialog()
        {
            using (var dialog = new WindowLayoutNewDialog())
            {
                // Allow user to save-as an existing item
                //// This will allow the dialog to check for duplicates
                //dialog.ExistingLayoutNames = WindowLayoutService.Layouts;

                TryUseMainFormIcon(MainForm, dialog);

                if (dialog.ShowDialog(MainForm) != DialogResult.OK)
                    return;

                SaveLayoutAs(dialog.LayoutName);
            }
        }

        /// <summary>
        /// Shows the manage layouts dialog</summary>
        /// <remarks>Programmatic method for showing the 'manage layouts' dialog</remarks>
        public override void ShowManageLayoutsDialog()
        {
            using (var dialog = new WindowLayoutManageDialog())
            {
                dialog.ScreenshotDirectory = LayoutScreenshotDirectory;
                dialog.LayoutNames = WindowLayoutService.Layouts;

                TryUseMainFormIcon(MainForm, dialog);

                dialog.ShowDialog(MainForm);

                // Re-associate shortcuts for proper persisting
                foreach (var kv in dialog.RenamedLayouts)
                {
                    IEnumerable<Input.Keys> shortcuts;
                    if (!m_dictCommandKeys.TryGetValue(kv.Key, out shortcuts))
                        continue;

                    m_dictCommandKeys.Remove(kv.Key);
                    m_dictCommandKeys[kv.Value] = shortcuts;
                }

                foreach (var kv in dialog.RenamedLayouts)
                    WindowLayoutService.RenameLayout(kv.Key, kv.Value);

                foreach (var layoutName in dialog.DeletedLayouts)
                    WindowLayoutService.RemoveLayout(layoutName);
            }
        }

        /// <summary>
        /// Method called when the window layout service's layouts changing event is triggered</summary>
        protected override void OnWindowLayoutServiceLayoutsChanging()
        {
            if (CommandService == null)
                return;

            foreach (var kv in m_dictCommands)
            {
                // Save shortcuts for the command re-add that comes later
                if (kv.Value.CommandInfo.Shortcuts.Any())
                    m_dictCommandKeys[kv.Key] = kv.Value.CommandInfo.Shortcuts;

                CommandService.UnregisterCommand(kv.Value, this);
            }

            m_dictCommands.Clear();
        }

        /// <summary>
        /// Method called when the window layout service's layouts changed event is triggered</summary>
        protected override void OnWindowLayoutServiceLayoutsChanged()
        {
            if (CommandService == null)
                return;

            foreach (var layoutName in WindowLayoutService.Layouts)
            {
                var cmd = new WindowLayoutServiceCommand(layoutName);

                var shortcut = Input.Keys.None;
                {
                    // Restore the shortcut (if any)
                    IEnumerable<Input.Keys> keys;
                    if (m_dictCommandKeys.TryGetValue(layoutName, out keys))
                        shortcut = keys.First();
                }

                cmd.CommandInfo =
                    CommandService.RegisterCommand(
                        cmd,
                        StandardMenu.Window,
                        StandardCommandGroup.UILayout,
                        string.Format("{0}{1}{2}", MenuName, MenuSeparator, layoutName),
                        layoutName,
                        shortcut,
                        null,
                        CommandVisibility.Menu,
                        this);

                m_dictCommands[layoutName] = cmd;
                m_dictCommandKeys[layoutName] = cmd.CommandInfo.Shortcuts;
            }

            // Clean up any lingering items
            foreach (var layoutName in WindowLayoutService.Layouts)
            {
                if (!m_dictCommandKeys.ContainsKey(layoutName))
                    m_dictCommandKeys.Remove(layoutName);
            }
        }

        /// <summary>
        /// Gets a screenshot of the current application</summary>
        /// <returns>Screenshot of current application</returns>
        protected override System.Drawing.Image GetApplicationScreenshot()
        {
            if (MainForm == null)
                return null;

            var image = GdiUtil.CaptureWindow(MainForm.Handle);
            MainForm.Invalidate(true);

            return image;
        }

        private static void TryUseMainFormIcon(Form mainForm, Form dialog)
        {
            if (mainForm == null)
                return;

            if (mainForm.Icon == null)
                return;

            dialog.Icon = mainForm.Icon;
        }

        #region Private Classes

        private class WindowLayoutServiceCommand
        {
            public WindowLayoutServiceCommand(string layoutName)
            {
                LayoutName = layoutName;
            }

            public string LayoutName { get; private set; }

            public CommandInfo CommandInfo { get; set; }
        }

        #endregion

        private readonly Dictionary<string, IEnumerable<Input.Keys>> m_dictCommandKeys =
            new Dictionary<string, IEnumerable<Input.Keys>>(StringComparer.CurrentCulture);

        private readonly Dictionary<string, WindowLayoutServiceCommand> m_dictCommands =
            new Dictionary<string, WindowLayoutServiceCommand>(StringComparer.CurrentCulture);
    }
}