//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Sce.Atf.Applications;
using Sce.Atf.Input;
using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Window layout service commands.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(WindowLayoutServiceCommandsBase))]
    [Export(typeof(WindowLayoutServiceCommands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class WindowLayoutServiceCommands : WindowLayoutServiceCommandsBase
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="windowLayoutService">Window layout service to use.</param>
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
                Group.WindowLayoutServiceCommandsBase,
                MenuName + "/" + MenuSaveLayoutAs,
                "Saves the layout to a new configuration".Localize(),
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            CommandService.RegisterCommand(
                Command.ManageLayouts,
                StandardMenu.Window,
                Group.WindowLayoutServiceCommandsBase,
                MenuName + "/" + MenuManageLayouts,
                "Manages the current layouts".Localize(), Keys.None,
                null,
                CommandVisibility.Menu,
                this);
        }

        #endregion

        #region ICommandClient Implementation

        /// <summary>
        /// Check if a command can be performed</summary>
        /// <param name="tag">Command</param>
        /// <returns>True if command can be performed otherwise false</returns>
        public override bool CanDoCommand(object tag)
        {
            return tag != null && (base.CanDoCommand(tag) || (tag is WindowLayoutServiceCommand));
        }

        /// <summary>
        /// Perform a command</summary>
        /// <param name="tag">Command</param>
        public override void DoCommand(object tag)
        {
            if (tag != null)
            {
                base.DoCommand(tag);

                if (!(tag is WindowLayoutServiceCommand))
                    return;

                var cmd = (WindowLayoutServiceCommand)tag;
                WindowLayoutService.CurrentLayout = cmd.LayoutName;
            }
        }

        /// <summary>
        /// Update a command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state</param>
        public override void UpdateCommand(object commandTag, CommandState state)
        {
        }

        #endregion

        /// <summary>
        /// Gets or sets the command service to use</summary>
        /// <remarks>If null no commands can be registered.</remarks>
        [Import(AllowDefault = true)]
        public Sce.Atf.Applications.ICommandService CommandService { get; set; }

        //private static KeyGesture[] ms_defaultShortcuts = new KeyGesture[]
        //{
        //    new KeyGesture(Key.NumPad0, ModifierKeys.Control),
        //    new KeyGesture(Key.NumPad1, ModifierKeys.Control),
        //    new KeyGesture(Key.NumPad2, ModifierKeys.Control),
        //    new KeyGesture(Key.NumPad3, ModifierKeys.Control),
        //    new KeyGesture(Key.NumPad4, ModifierKeys.Control),
        //    new KeyGesture(Key.NumPad5, ModifierKeys.Control),
        //    new KeyGesture(Key.NumPad6, ModifierKeys.Control),
        //    new KeyGesture(Key.NumPad7, ModifierKeys.Control),
        //    new KeyGesture(Key.NumPad8, ModifierKeys.Control),
        //    new KeyGesture(Key.NumPad9, ModifierKeys.Control),
        //};

        public override void ShowSaveLayoutAsDialog()
        {
            var vm = new WindowLayoutNewViewModel(WindowLayoutService.Layouts);
            var dialog = new WindowLayoutNewDialog(vm);

            if (dialog.ShowParentedDialog() != true)
                return;

            SaveLayoutAs(vm.LayoutName);

            // todo: Assign Shortcut
        }

        public override void ShowManageLayoutsDialog()
        {
            var layouts = new List<Pair<string, Keys>>();
            foreach (var layoutName in WindowLayoutService.Layouts)
            {
                Keys shortcut = Keys.None;
                {
                    IEnumerable<Keys> keys;
                    if (m_dictCommandKeys.TryGetValue(layoutName, out keys))
                        shortcut = keys.FirstOrDefault();
                }

                layouts.Add(new Pair<string, Keys>(layoutName, shortcut));
            }
            
            var vm = new ManageWindowLayoutsDialogViewModel(layouts);
            vm.ScreenshotDirectory = LayoutScreenshotDirectory;
            var dialog = new WindowLayoutManageDialog(vm);

            if (dialog.ShowParentedDialog() != true)
                return;

            // Re-associate shortcuts for proper persisting
            foreach (var kv in vm.RenamedLayouts)
            {
                IEnumerable<Keys> shortcuts;
                if (!m_dictCommandKeys.TryGetValue(kv.Key, out shortcuts))
                    continue;

                m_dictCommandKeys.Remove(kv.Key);
                m_dictCommandKeys[kv.Value] = shortcuts;
            }

            foreach (var kv in vm.RenamedLayouts)
                WindowLayoutService.RenameLayout(kv.Key, kv.Value);

            foreach (var layoutName in vm.DeletedLayouts)
                WindowLayoutService.RemoveLayout(layoutName);
        }

        /// <summary>
        /// Get a screenshot of the current application</summary>
        /// <returns>Screenshot of current application</returns>
        protected override System.Drawing.Image GetApplicationScreenshot()
        {
            var bitmapSource = ImageUtil.CaptureWindow(Application.Current.MainWindow);
            {
                return ImageUtil.BitmapFromSource(bitmapSource);
            }
        }

        protected override void OnWindowLayoutServiceLayoutsChanging()
        {
            if (CommandService == null)
                return;

            foreach (var kv in m_dictCommands)
            {
                // Save shortcuts for the command re-add that comes later
                m_dictCommandKeys[kv.Key] = kv.Value.Shortcuts;
                CommandService.UnregisterCommand(kv.Value.CommandTag, this);
            }

            m_dictCommands.Clear();
        }

        protected override void OnWindowLayoutServiceLayoutsChanged()
        {
            if (CommandService == null)
                return;

            foreach (var layoutName in WindowLayoutService.Layouts)
            {
                var cmd = new WindowLayoutServiceCommand(layoutName);

                Keys shortcut = Keys.None;
                {
                    // Restore the shortcut (if any)
                    IEnumerable<Keys> shortucts;
                    if (m_dictCommandKeys.TryGetValue(layoutName, out shortucts))
                        shortcut = shortucts.FirstOrDefault();
                }

                ICommandItem command = CommandService.RegisterCommand(
                        cmd,
                        StandardMenu.Window,
                        StandardCommandGroup.WindowLayoutItems,
                        MenuName + "/" + layoutName,
                        null,
                        shortcut, null,
                        CommandVisibility.Menu,
                        this).GetCommandItem();

                cmd.Command = command;
                m_dictCommands[layoutName] = command;
                m_dictCommandKeys[layoutName] = command.Shortcuts;
            }

            // Clean up any lingering items
            foreach (var layoutName in WindowLayoutService.Layouts)
            {
                if (!m_dictCommandKeys.ContainsKey(layoutName))
                    m_dictCommandKeys.Remove(layoutName);
            }
        }

        #region Private Classes

        private class WindowLayoutServiceCommand
        {
            public WindowLayoutServiceCommand(string layoutName)
            {
                LayoutName = layoutName;
            }

            public string LayoutName { get; private set; }

            public ICommandItem Command { get; set; }
        }

        #endregion

        private readonly Dictionary<string, IEnumerable<Keys>> m_dictCommandKeys =
            new Dictionary<string, IEnumerable<Keys>>(StringComparer.CurrentCulture);

        private readonly Dictionary<string, ICommandItem> m_dictCommands =
            new Dictionary<string, ICommandItem>(StringComparer.CurrentCulture);
    }
}
