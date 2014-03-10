//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Component that adds the File/Exit command that will close the application's
    /// main form</summary>
    [Export(typeof(StandardFileExitCommand))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class StandardFileExitCommand : ICommandClient, IInitializable, IPartImportsSatisfiedNotification
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        [ImportingConstructor]
        public StandardFileExitCommand(ICommandService commandService)
        {
            m_commandService = commandService;
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
        }

        #endregion

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            m_commandService.RegisterCommand(CommandInfo.FileExit, this);
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Checks whether the client can do the command, if it handles it</summary>
        /// <param name="commandTag">Command to be done</param>
        /// <returns>True iff client can do the command</returns>
        bool ICommandClient.CanDoCommand(object commandTag)
        {
            return
                StandardCommand.FileExit.Equals(commandTag);
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandTag">Command to be done</param>
        void ICommandClient.DoCommand(object commandTag)
        {
            if (StandardCommand.FileExit.Equals(commandTag))
            {
                m_mainWindow.Close();
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

        [Import(AllowDefault = true)]
        private IMainWindow m_mainWindow;

        [Import(AllowDefault = true)]
        private Form m_mainForm;

        private readonly ICommandService m_commandService;
    }
}
