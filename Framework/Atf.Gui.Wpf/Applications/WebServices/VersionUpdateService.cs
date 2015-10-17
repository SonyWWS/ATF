//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reflection;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Input;
using Sce.Atf.Wpf.Controls;

namespace Sce.Atf.Wpf.Applications.WebServices
{
    /// <summary>
    /// Component to update to latest version on SHIP</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(VersionUpdateService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class VersionUpdateService : ICommandClient, IInitializable, IPartImportsSatisfiedNotification
    {
        /// <summary>
        /// Gets and sets a value indicating if application checks for an update at startup</summary>
        [DefaultValue(true)]
        public bool CheckForUpdateAtStartup
        {
            get { return m_checkForUpdateAtStartup; }
            set { m_checkForUpdateAtStartup = value; }
        }

        #region IPartImportsSatisfiedNotification Members

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            m_mainWindow.Loaded += mainWindow_Loaded;
        }

        #endregion

        #region IInitializable Members

        /// <summary>
        /// Initializes plugin</summary>
        /// <returns>true, if plugin was initialized correctly</returns>
        void IInitializable.Initialize()
        {
            // check for assembly mapping attribute.
            // this attribute is used for bug-reporting and checking for update.
            var assembly = Assembly.GetEntryAssembly();
            var mapAttr =
                (ProjectMappingAttribute) Attribute.GetCustomAttribute(assembly, typeof(ProjectMappingAttribute));
            m_assemblyMappingFound = (mapAttr != null && mapAttr.Mapping != null && mapAttr.Mapping.Trim().Length != 0);

            if (m_assemblyMappingFound)
            {
                // check for new update
                m_updateCheck = new VersionCheck();
                m_updateCheck.CheckComplete += updateCheck_CheckComplete;

                if (m_commandService != null)
                {
                    m_commandService.RegisterCommand(
                        Command.HelpCheckForUpdate,
                        StandardMenu.Help, 
                        StandardCommandGroup.HelpUpdate,
                        "Check for update...".Localize(),
                        "Check for product update".Localize(), 
                        Keys.None,
                        null,
                        CommandVisibility.Menu,
                        this);
                }

                RegisterSettings();
            }
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Checks if the client can do the command</summary>
        /// <param name="tag">Command</param>
        /// <returns>true, if client can do the command</returns>
        public bool CanDoCommand(object tag)
        {
            return Command.HelpCheckForUpdate.Equals(tag);
        }

        /// <summary>
        /// Do a command</summary>
        /// <param name="tag">Command</param>
        public void DoCommand(object tag)
        {
            if (Command.HelpCheckForUpdate.Equals(tag))
            {
                m_notifyUserIfNoUpdate = true;
                m_updateCheck.Check(false);
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state to update</param>
        public void UpdateCommand(object commandTag, CommandState state)
        {
        }

        #endregion

        /// <summary>
        /// Uses the settings service to register user settings</summary>
        protected virtual void RegisterSettings()
        {
            // register new setting for persisting update notification at startup.
            var userPrefs =
                new PropertyDescriptor[]
                    {
                        new BoundPropertyDescriptor(this, () => CheckForUpdateAtStartup,
                                                    "Check for update at startup".Localize(), null,
                                                    "Check for product update at startup".Localize()),
                    };

            m_settingsService.RegisterSettings(
                (new Guid("{612B1EE5-D591-4707-91CB-E9F5890DDEB2}")).ToString(), userPrefs);
            m_settingsService.RegisterUserSettings("Application".Localize(), userPrefs);
        }

        /// <summary>
        /// Gets or sets the application's command service</summary>
        /// <remarks>Added for legacy support only; avoid using</remarks>
        protected ICommandService CommandService { get; set; }

        private void mainWindow_Loaded(object sender, EventArgs e)
        {
#if true
            //!DEBUG // don't do startup version check in debug builds; it can be annoying for developers
            if (m_assemblyMappingFound && m_checkForUpdateAtStartup)
            {
                m_updateCheck.Check(true);
            }
#endif
        }

        private void updateCheck_CheckComplete(string val, bool error)
        {
            // if there is no error and new version is available, display message
            if (!error && !string.IsNullOrEmpty(val))
            {
                string message = "There is a newer version of this program available." + Environment.NewLine +
                                 string.Format("Your version is {0}.".Localize(), m_updateCheck.AppVersion) +
                                 Environment.NewLine +
                                 string.Format("The most recent version is {0}.".Localize(), m_updateCheck.ServerVersion) +
                                 Environment.NewLine +
                                 Environment.NewLine +
                                 "Would you like to download the latest version?".Localize();

                var dialog = new MessageBoxDialog("Update".Localize(), message, System.Windows.MessageBoxButton.YesNo,
                                                  System.Windows.MessageBoxImage.Information);

                if (dialog.ShowDialog() == true)
                {
                    // open SourceForge page in web browser
                    try
                    {
                        var p = new Process();
                        p.StartInfo.FileName = val;
                        p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                        p.Start();
                        p.Dispose();

                    }
                    catch (Exception e)
                    {
                        WpfMessageBox.Show(
                            "Cannot open url:".Localize() + val + ".\n" +
                            "Error".Localize() + ":" + e.Message,
                            "Error".Localize(),
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Error);
                    }
                }
            }
            else if (m_notifyUserIfNoUpdate)
            {
                if (error)
                {
                    WpfMessageBox.Show(val, "Error".Localize(), System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
                else
                {
                    WpfMessageBox.Show("This software is up to date.".Localize(), "Updater".Localize(),
                                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            }
        }

        private enum Command
        {
            HelpCheckForUpdate,
        }

        [Import(AllowDefault = true)]
        private ICommandService m_commandService = null;

        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService = null;

        [Import(AllowDefault = true)]
        private IMainWindow m_mainWindow = null;

        private VersionCheck m_updateCheck;
        private bool m_checkForUpdateAtStartup = true;
        private bool m_assemblyMappingFound;
        private bool m_notifyUserIfNoUpdate;
    }
}
