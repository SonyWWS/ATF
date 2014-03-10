//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Reflection;

namespace Sce.Atf.Applications.WebServices
{
    /// <summary>
    /// User feedback service for displaying a dialog box that allows the user to submit a
    /// bug report to SHIP. Note that this does not work with JIRA, only with the old SourceForge trackers.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(UserFeedbackService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class UserFeedbackService : IInitializable, ICommandClient
    {
        /// <summary>
        /// Shows the feedback form as a modal dialog box</summary>
        public void ShowFeedbackForm()
        {
            FeedbackForm feedbackForm = new FeedbackForm(false);
            feedbackForm.Text = "Send Feedback".Localize();
            feedbackForm.ShowDialog();
        }

        /// <summary>
        /// Gets or sets the command service used to display the user feedback command in a menu and on a toolbar</summary>
        [Import(AllowDefault = true)]
        public ICommandService CommandService
        {
            get { return m_commandService; }
            set { m_commandService = value; }
        }

        #region IInitializable

        /// <summary>
        /// Initializes plugin</summary>
        /// <returns>true, iff plugin was initialized correctly</returns>
        void IInitializable.Initialize()
        {
            // check for assembly mapping attribute.
            // this attribute is used for bug-reporting and checking for update.
            Assembly assembly = Assembly.GetEntryAssembly();
            ProjectMappingAttribute mapAttr =
                (ProjectMappingAttribute)Attribute.GetCustomAttribute(assembly, typeof(ProjectMappingAttribute));
            m_assemblyMappingFound = (mapAttr != null && mapAttr.Mapping != null && mapAttr.Mapping.Trim().Length != 0);

            if (m_assemblyMappingFound)
            {
                m_commandService.RegisterCommand( new CommandInfo(
                  Command.HelpSendFeedback,
                  StandardMenu.Help,
                  StandardCommandGroup.HelpUpdate,
                  "Send Feedback...".Localize(),
                  "Report bug or request feature".Localize()),
                  this);
            }
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Checks if the client can do the command</summary>
        /// <param name="tag">Command</param>
        /// <returns>True if client can do the command</returns>
        public bool CanDoCommand(object tag)
        {
            return
                Command.HelpSendFeedback.Equals(tag);
        }

        /// <summary>
        /// Does a command</summary>
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

        private ICommandService m_commandService;
        private bool m_assemblyMappingFound;
    }
}
