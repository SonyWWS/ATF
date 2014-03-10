//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf.Controls;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Adds the Help/About command, which displays a dialog box with a description
    /// of the application (specified by a derived class) plus the ATF version number</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(HelpAboutCommand))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class HelpAboutCommand : ICommandClient, IInitializable
    {
        /// <summary>
        /// Shows the About dialog box</summary>
        /// <remarks>Derived classes should copy this method and customize it. For an example
        /// of loading a rich text file that is an embedded resource, see the SimpleDomEditor sample.
        /// It is recommended to display the ATF version number to aid in diagnosing
        /// problems. (Pass in 'true' to AboutDialog's constructor or use AtfVersion.)</remarks>
        protected virtual void ShowHelpAbout()
        {
            var richTextBox = new RichTextBox();
            richTextBox.BorderStyle = BorderStyle.None;
            richTextBox.ReadOnly = true;
            richTextBox.Text = "An application built using the Authoring Tools Framework".Localize();

            string appURL = "https://github.com/SonyWWS/ATF/wiki";
            var dialog = new AboutDialog(
                "About this Application".Localize(), appURL, richTextBox, null, null, true);
            dialog.ShowDialog();
        }

        /// <summary>
        /// Gets and sets the command service used to register this command</summary>
        [Import]
        protected ICommandService CommandService;

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            CommandService.RegisterCommand(CommandInfo.HelpAbout, this);
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Checks if the client can do the command</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>True iff client can do the command</returns>
        public bool CanDoCommand(object commandTag)
        {
            return
                StandardCommand.HelpAbout.Equals(commandTag);
        }

        /// <summary>
        /// Does a command</summary>
        /// <param name="commandTag">Command</param>
        public void DoCommand(object commandTag)
        {
            if (StandardCommand.HelpAbout.Equals(commandTag))
            {
                ShowHelpAbout();
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state to update</param>
        public void UpdateCommand(object commandTag, Sce.Atf.Applications.CommandState state)
        {
        }

        #endregion
    }
}
