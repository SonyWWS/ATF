//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications.WebServices
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

        /// <summary>
        /// Notification when part's imports have been satisfied</summary>
        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            if (m_mainWindow == null &&
                m_mainForm != null)
            {
                m_mainWindow = new MainFormAdapter(m_mainForm);
            }

            if (m_mainWindow == null)
                throw new InvalidOperationException("Can't get main window");

            m_dialogOwner = m_mainWindow.DialogOwner;
            m_mainWindow.Loaded += mainWindow_Loaded;
        }

        #endregion

        #region IInitializable Members

        /// <summary>
        /// Initializes component</summary>
        /// <returns><c>True</c> if component was initialized correctly</returns>
        void IInitializable.Initialize()
        {
            // check for assembly mapping attribute.
            // this attribute is used for bug-reporting and checking for update.
            Assembly assembly = Assembly.GetEntryAssembly();
            ProjectMappingAttribute mapAttr = (ProjectMappingAttribute)Attribute.GetCustomAttribute(assembly, typeof(ProjectMappingAttribute));
            m_assemblyMappingFound = (mapAttr != null && mapAttr.Mapping != null && mapAttr.Mapping.Trim().Length != 0);

            if (m_assemblyMappingFound)
            {
                // check for new update
                m_updateCheck = new VersionCheck();
                m_updateCheck.CheckComplete += updateCheck_CheckComplete;

                m_commandService.RegisterCommand( new CommandInfo(
                   Command.HelpCheckForUpdate,
                   StandardMenu.Help,
                   StandardCommandGroup.HelpUpdate,
                   "Check for update...".Localize(), 
                   "Check for product update".Localize()),
                   this);

                RegisterSettings();
            }
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Checks if the client can do the command</summary>
        /// <param name="tag">Command</param>
        /// <returns><c>True</c> if client can do the command</returns>
        public bool CanDoCommand(object tag)
        {
            return
                Command.HelpCheckForUpdate.Equals(tag);
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
        /// Registers user settings using the settings service</summary>
        protected virtual void RegisterSettings()
        {
            // register new setting for persisting update notification at startup.
            PropertyDescriptor[] userPrefs =
                    new PropertyDescriptor[]
                        {
                            new BoundPropertyDescriptor(this, () => CheckForUpdateAtStartup, "Check for update at startup".Localize(), null, "Check for product update at startup".Localize()),
                        };

            m_settingsService.RegisterSettings(
                (new Guid("{612B1EE5-D591-4707-91CB-E9F5890DDEB2}")).ToString(), userPrefs);
            m_settingsService.RegisterUserSettings("Application".Localize(), userPrefs);
        }

        /// <summary>
        /// Gets or sets the application's main form</summary>
        /// <remarks>Added for legacy support only; avoid using</remarks>
        protected Form MainForm
        {
            get { return m_mainForm; }
            set { m_mainForm = value; }
        }

        /// <summary>
        /// Gets or sets the application's command service</summary>
        /// <remarks>Added for legacy support only; avoid using</remarks>
        protected ICommandService CommandService
        {
            get { return m_commandService; }
            set { m_commandService = value; }
        }

        private void mainWindow_Loaded(object sender, EventArgs e)
        {
#if !DEBUG // don't do startup version check in debug builds; it can be annoying for developers
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
                string cr = Environment.NewLine;
                DialogResult result = MessageBox.Show(
                    m_dialogOwner,
                    "There is a newer version of this program available.".Localize() + cr +
                     string.Format("Your version is {0}.".Localize(), m_updateCheck.AppVersion) + cr +
                     string.Format("The most recent version is {0}.".Localize(), m_updateCheck.ServerVersion) + cr + cr +
                     "Would you like to download the latest version?".Localize(),
                    "Update".Localize("this is the title of a dialog box"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {

                    // open SourceForge page in web browser
                    try
                    {
                        Process p = new Process();
                        p.StartInfo.FileName = val;
                        p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                        p.Start();
                        p.Dispose();

                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(
                            m_dialogOwner,
                            "Cannot open url:".Localize() + val + ".\n" +
                            "Error".Localize() + ":" + e.Message,
                            "Error".Localize(),
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
            else if (m_notifyUserIfNoUpdate)
            {
                if (error)
                {
                    MessageBox.Show(m_dialogOwner, val, "Error".Localize(), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show(m_dialogOwner, "This software is up to date.".Localize(), "Updater".Localize(), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private enum Command
        {
            HelpCheckForUpdate,
        }

        [Import(AllowDefault = true)]
        private ICommandService m_commandService;

        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService;

        [Import(AllowDefault = true)]
        private IMainWindow m_mainWindow;

        [Import(AllowDefault = true)]
        private Form m_mainForm;

        private VersionCheck m_updateCheck;
        private IWin32Window m_dialogOwner;
        private bool m_checkForUpdateAtStartup = true;
        private bool m_assemblyMappingFound;
        private bool m_notifyUserIfNoUpdate;
    }
}
