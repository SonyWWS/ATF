//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Base class for window layout service commands</summary>
    public abstract class WindowLayoutServiceCommandsBase : IInitializable, ICommandClient
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="windowLayoutService">Window layout service to use</param>
        protected WindowLayoutServiceCommandsBase(IWindowLayoutService windowLayoutService)
        {
            WindowLayoutService = windowLayoutService;
            WindowLayoutService.LayoutsChanging += WindowLayoutServiceLayoutsChanging;
            WindowLayoutService.LayoutsChanged += WindowLayoutServiceLayoutsChanged;
        }

        #region IInitializable Implementation

        /// <summary>
        /// Finish initializing component</summary>
        public virtual void Initialize()
        {
            if (SettingsPathsProvider == null)
                return;

            try
            {
                string path =
                    Path.GetDirectoryName(SettingsPathsProvider.SettingsPath) +
                    Path.DirectorySeparatorChar + LayoutDirectoryName;

                LayoutScreenshotDirectory =
                    Directory.Exists(path)
                        ? new DirectoryInfo(path)
                        : Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                Outputs.WriteLine(
                    OutputMessageType.Error,
                    "{0}: Exception setting layout screenshot directory: {1}",
                    this, ex.Message);
            }
        }

        #endregion

        #region ICommandClient Implementation

        /// <summary>
        /// Checks if a command can be performed</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>True iff command can be performed</returns>
        public virtual bool CanDoCommand(object commandTag)
        {
            return commandTag is Command;
        }

        /// <summary>
        /// Performs a command</summary>
        /// <param name="commandTag">Command</param>
        public virtual void DoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return;

            var cmd = (Command)commandTag;
            switch (cmd)
            {
                case Command.SaveLayoutAs:
                    ShowSaveLayoutAsDialog();
                    break;

                case Command.ManageLayouts:
                    ShowManageLayoutsDialog();
                    break;
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state</param>
        public virtual void UpdateCommand(object commandTag, CommandState state)
        {
        }

        #endregion

        /// <summary>
        /// Gets or sets the settings paths provider to use</summary>
        /// <remarks>If null, screenshots cannot be saved</remarks>
        [Import(AllowDefault = true)]
        public ISettingsPathsProvider SettingsPathsProvider { get; set; }

        /// <summary>
        /// Saves the current layout as a name (and creates a screenshot)</summary>
        /// <param name="layoutName">Layout name to save as</param>
        /// <returns>True iff saved</returns>
        public virtual bool SaveLayoutAs(string layoutName)
        {
            if (string.IsNullOrEmpty(layoutName))
                return false;

            // Allow overwriting. This is only possible if
            // the layout name isn't already in use.
            WindowLayoutService.RemoveLayout(layoutName);

            WindowLayoutService.CurrentLayout = layoutName;
            SaveLayoutScreenshot(layoutName, LayoutScreenshotDirectory, this);

            return true;
        }

        /// <summary>
        /// Show the 'Save Layout As' dialog</summary>
        public abstract void ShowSaveLayoutAsDialog();

        /// <summary>
        /// Show the 'Manage Layouts' dialog</summary>
        public abstract void ShowManageLayoutsDialog();

        /// <summary>
        /// Commands that can be registered enum</summary>
        protected enum Command
        {
            /// <summary>"Save layout as" command</summary>
            SaveLayoutAs,
            /// <summary>"Manage layouts" command</summary>
            ManageLayouts,
        }

        /// <summary>
        /// Command group to use for layout related commands</summary>
        protected enum Group
        {
            /// <summary>"Base group</summary>
            WindowLayoutServiceCommandsBase,
        }

        /// <summary>
        /// Gets the window layout service</summary>
        public IWindowLayoutService WindowLayoutService { get; private set; }

        /// <summary>
        /// Gets the directory where screenshots are saved</summary>
        public DirectoryInfo LayoutScreenshotDirectory { get; private set; }

        /// <summary>
        /// Returns a screenshot of the current application</summary>
        /// <returns>Screenshot of current application</returns>
        protected abstract Image GetApplicationScreenshot();

        /// <summary>
        /// Method called when window layout service's layouts changing event has fired</summary>
        protected abstract void OnWindowLayoutServiceLayoutsChanging();

        /// <summary>
        /// Method called when window layout service's layouts changed event has fired</summary>
        protected abstract void OnWindowLayoutServiceLayoutsChanged();

        private void WindowLayoutServiceLayoutsChanging(object sender, EventArgs e)
        {
            OnWindowLayoutServiceLayoutsChanging();
        }

        private void WindowLayoutServiceLayoutsChanged(object sender, EventArgs e)
        {
            OnWindowLayoutServiceLayoutsChanged();
        }

        private static void SaveLayoutScreenshot(string name, DirectoryInfo dir, WindowLayoutServiceCommandsBase wlscb)
        {
            if (string.IsNullOrEmpty(name))
                return;

            if (dir == null)
                return;

            if (wlscb == null)
                return;

            try
            {
                if (!Directory.Exists(dir.FullName))
                    return;

                string path =
                    Path.Combine(
                        dir.FullName + Path.DirectorySeparatorChar,
                        name + ScreenshotExtension);

                using (var bmp = wlscb.GetApplicationScreenshot())
                {
                    using (var stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Write))
                    {
                        bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                }
            }
            catch (Exception ex)
            {
                Outputs.WriteLine(
                    OutputMessageType.Error,
                    "{0}: Exception saving layout screenshot: {1}",
                    wlscb, ex.Message);
            }
        }

        /// <summary>Menu separator character</summary>
        protected const string MenuSeparator = "/";
        /// <summary>Menu name</summary>
        protected static readonly string MenuName = "Layouts".Localize();
        /// <summary>Menu item command for "Save layout as"</summary>
        protected static readonly string MenuSaveLayoutAs = "Save Layout As...".Localize();
        /// <summary>Menu item command for "Manage Layouts"</summary>
        protected static readonly string MenuManageLayouts = "Manage Layouts...".Localize();
        /// <summary>Directory name where layouts are saved</summary>
        protected const string LayoutDirectoryName = "Layouts";

        /// <summary>Screenshot file extension</summary>
        public const string ScreenshotExtension = ".jpg";
    }
}