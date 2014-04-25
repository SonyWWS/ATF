//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Commands for offering web-based help, so that a specific web page is opened if the
    /// user presses F1. See WebHelp for offering context-sensitive help.</summary>
    /// <example>
    /// CompositionBatch batch = new CompositionBatch();
    /// batch.AddPart(new WebHelpCommands("https://github.com/SonyWWS/ATF/wiki/ATF-Circuit-Editor-Sample").Localize());
    /// container.Compose(batch); </example>
    [Export(typeof(IInitializable))]
    [Export(typeof(WebHelpCommands))]
    public class WebHelpCommands : ICommandClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        [ImportingConstructor]
        public WebHelpCommands()
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="url">URL that will be opened by the default browser if the user presses F1
        /// or uses the menu command. If null or empty, then this functionality is disabled.</param>
        public WebHelpCommands(string url)
        {
            Url = url;
        }

        /// <summary>
        /// Gets or sets the URL that will be opened by the default browser if the user presses F1. Setting
        /// to null or the empty string will cause nothing to happen when F1 is pressed.</summary>
        public string Url
        {
            get;
            set;
        }

        #region IInitializable Members

        /// <summary>
        /// Initializes this component</summary>
        public virtual void Initialize()
        {
            CommandInfo helpCommandInfo =
                CommandService.RegisterCommand(Commands.OpenHelpPage, StandardMenu.Help,
                StandardCommandGroup.HelpAbout, "Online Help".Localize(),
                "Opens an online help page for this app".Localize(), Keys.F1, null,
                CommandVisibility.ApplicationMenu, this);

            // We need to listen to the MenuItem's Click event, rather than the normal and otherwise
            //  better way of using DoCommand, because the currently active Control might have a
            //  WebHelp attached and it should take precedence if the user presses F1 but not if the
            //  user uses the help menu command.
            CommandService.CommandControls controls = CommandService.GetCommandControls(helpCommandInfo);
            controls.MenuItem.Click += MenuItemOnClick;
            m_webHelp = MainForm.AddHelp(Url);
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Checks if the client can do the command</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>True iff client can do the command</returns>
        public virtual bool CanDoCommand(object commandTag)
        {
            return !string.IsNullOrEmpty(Url);
        }

        /// <summary>
        /// Does a command</summary>
        /// <param name="commandTag">Command</param>
        public virtual void DoCommand(object commandTag)
        {
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="commandState">Command state to update</param>
        public virtual void UpdateCommand(object commandTag, CommandState commandState)
        {
        }

        #endregion

        /// <summary>
        /// Our command tags</summary>
        protected enum Commands
        {
            /// <summary>
            /// Open help page command</summary>
            OpenHelpPage
        }

        /// <summary>
        /// Gets or sets the command service. Setting must be done before Initialize() is called.
        /// It needs to be the concrete CommandService and not ICommandService.</summary>
        [Import]
        protected CommandService CommandService
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the main window. It must be set before calling Initialize().</summary>
        [Import]
        protected Form MainForm
        {
            get;
            set;
        }

        private void MenuItemOnClick(object sender, EventArgs eventArgs)
        {
            m_webHelp.OpenUrl();
        }

        private WebHelp m_webHelp;
    }
}
