//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Reflection;
using Sce.Atf.Applications;
using Sce.Atf.Input;

namespace Sce.Atf.Wpf.Applications.WebServices
{
    /// <summary>
    /// User feedback service for displaying a dialog box that allows the user to submit a
    /// bug report to SHIP</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(UserFeedbackService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class UserFeedbackService : IInitializable, ICommandClient
    {
        /// <summary>
        /// Shows the feedback form as a modal dialog box</summary>
        public void ShowFeedbackForm()
        {
            var vm = new FeedbackFormViewModel();
            var feedbackForm = new FeedbackForm();
            feedbackForm.ShowDialog();
        }

        #region IInitializable

        /// <summary>
        /// Initializes plugin</summary>
        /// <returns>true, if plugin was initialized correctly</returns>
        void IInitializable.Initialize()
        {
            // check for assembly mapping attribute.
            // this attribute is used for bug-reporting and checking for update.
            var assembly = Assembly.GetEntryAssembly();
            var mapAttr = (ProjectMappingAttribute) Attribute.GetCustomAttribute(assembly, typeof(ProjectMappingAttribute));
            var assemblyMappingFound = (mapAttr != null && mapAttr.Mapping != null && mapAttr.Mapping.Trim().Length != 0);

            if (assemblyMappingFound)
            {
                if (m_commandService != null)
                {
                    m_commandService.RegisterCommand(
                        Command.HelpSendFeedback,
                        StandardMenu.Help,
                        StandardCommandGroup.HelpUpdate,
                        "Send Feedback...".Localize(),
                        "Report bug or request feature".Localize(),
                        Keys.None,
                        null,
                        CommandVisibility.Menu,
                        this);
                }
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
            return Command.HelpSendFeedback.Equals(tag);
        }

        /// <summary>
        /// Do a command</summary>
        /// <param name="tag">Command</param>
        public void DoCommand(object tag)
        {
            if (Command.HelpSendFeedback.Equals(tag))
            {
                ShowFeedbackForm();
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

        private enum Command
        {
            HelpSendFeedback,
        }

        [Import(AllowDefault = true)]
        private ICommandService m_commandService = null;
    }
}
