//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using System.Windows.Input;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Input;
using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Provides main application help command and context specific context menu help commands</summary>
    [Export(typeof(HelpCommands))]
    [Export(typeof(Sce.Atf.Applications.ICommandClient))]
    [Export(typeof(Sce.Atf.Applications.IContextMenuCommandProvider))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class HelpCommands : Sce.Atf.Applications.ICommandClient, IInitializable, Sce.Atf.Applications.IContextMenuCommandProvider
    {
        /// <summary>
        /// Help commands enum</summary>
        public enum Commands
        {
            /// <summary>
            /// General Help command</summary>
            Help,
            /// <summary>
            /// Release Notes Help command</summary>
            HelpReleaseNotes,
            /// <summary>
            /// Help About...</summary>
            HelpAbout,
        }

        /// <summary>
        /// Enum for Help command group</summary>
        public enum Groups
        {
            /// <summary>Help command</summary>
            Help
        }

        private const int kMaxContextHelpKeys = 10;

        [Import]
        private ICommandService m_commandService = null;

        [Import(AllowDefault=true)]
        private Sce.Atf.Applications.ISettingsService m_settingsService = null;

        /// <summary>
        /// Gets or sets the help file path</summary>
        public string HelpFilePath { get; set; }

        /// <summary>
        /// Gets or sets the help file path</summary>
        public string ReleaseNotesFilePath { get; set; }

        /// <summary>
        /// Gets or sets whether to make ShowContextHelp user setting visible</summary>
        public bool EnableContextHelpUserSetting
        {
            get { return m_enableContextHelpUserSetting; }
            set { m_enableContextHelpUserSetting = value; }
        }
        private bool m_enableContextHelpUserSetting = true;

        /// <summary>
        /// Gets or sets whether to show context-sensitive help</summary>
        public bool ShowContextHelp
        {
            get { return m_showContextHelp; }
            set { m_showContextHelp = value; }
        }
        private bool m_showContextHelp = true;

        /// <summary>
        /// Gets or sets the application help is obtained for</summary>
        public string ApplicationName 
        {
            get { return m_applicationName; }
            set
            {
                m_applicationName = value;
                var commandItem = s_helpAboutCommand.GetCommandItem();
                commandItem.Text = "_About".Localize() + " " + ApplicationName;
                commandItem.Description = commandItem.Text;
            }
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by registering Help command, setting default help path,
        /// and setting up Settings Service</summary>
        public void Initialize()
        {
            m_commandService.RegisterCommand(s_helpCommand, this);
            m_commandService.RegisterCommand(s_helpAboutCommand, this);

            if (!string.IsNullOrEmpty(ReleaseNotesFilePath))
            {
                m_commandService.RegisterCommand(s_releaseNotesCommand, this);
            }

            for (int i = 0; i < kMaxContextHelpKeys; i++)
            {
                var tag = new ContextMenuHelpTag() { Index = i };

                CommandInfo info = m_commandService.RegisterCommand(
                    tag,
                    null,
                    Groups.Help,
                    "Help/Help".Localize() + " " + (i + 1),
                    "Help".Localize(),
                    Keys.None,
                    null,
                    Sce.Atf.Applications.CommandVisibility.None, this);

                m_contextMenuHelpCommands.Add(info);
            }

            // Set up a default help path
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Help.chm");
            if (File.Exists(path))
                HelpFilePath = path;

            // Set up a default application name

            // Register help setting
            if(m_settingsService != null && EnableContextHelpUserSetting)
            {
                m_settingsService.RegisterUserSettings("Help".Localize(), 
                   new BoundPropertyDescriptor(this, () => ShowContextHelp, "Show Context Help".Localize(), "Help".Localize(), "Uncheck this to hide help commands in context menus".Localize()));
            }
        }


        #endregion

        /// <summary>
        /// Shows the application help file</summary>
        public void ShowHelp()
        {
            if (HelpFilePath != null)
                System.Windows.Forms.Help.ShowHelp(null, HelpFilePath);
        }

        /// <summary>
        /// Shows the application help file</summary>
        public void ShowReleaseNotes()
        {
            if (ReleaseNotesFilePath != null)
                System.Windows.Forms.Help.ShowHelp(null, ReleaseNotesFilePath);
        }

        /// <summary>
        /// Shows the application help file</summary>
        /// <param name="helpNavigator">Help navigator</param>
        public void ShowHelp(System.Windows.Forms.HelpNavigator helpNavigator)
        {
            System.Windows.Forms.Help.ShowHelp(null, HelpFilePath, helpNavigator);
        }

        /// <summary>
        /// Shows the application help file</summary>
        /// <param name="keyword">Keyword in help file</param>
        public void ShowHelp(string keyword)
        {
            ShowHelp(System.Windows.Forms.HelpNavigator.KeywordIndex, keyword);
        }

        /// <summary>
        /// Shows the application help file</summary>
        /// <param name="helpNavigator">Help navigator</param>
        /// <param name="keyword">Keyword in help file</param>
        public void ShowHelp(System.Windows.Forms.HelpNavigator helpNavigator, string keyword)
        {
            if (keyword == null)
            {
                ShowHelp(helpNavigator);
            }
            else if (HelpFilePath != null)
            {
                System.Windows.Forms.Help.ShowHelp(null, HelpFilePath, helpNavigator, keyword);
            }
        }

        /// <summary>
        /// Shows the application's About... dialog</summary>
        public virtual void ShowHelpAbout()
        {
            DialogUtils.ShowDialogWithViewModel<AboutDialog, AboutDialogViewModel>();
        }

        #region Sce.Atf.Applications.ICommandClient Members

        /// <summary>
        /// Checks whether the client can do the command, if it handles it</summary>
        /// <param name="tag">Command to be done</param>
        /// <returns>True iff client can do the command</returns>
        public bool CanDoCommand(object tag)
        {
            if (tag is Commands)
            {
                switch ((Commands)tag)
                {
                    case Commands.Help: return HelpFilePath != null;
                    case Commands.HelpReleaseNotes: return ReleaseNotesFilePath != null;
                    case Commands.HelpAbout: return true;
                }
            }
            else if (ShowContextHelp && tag is ContextMenuHelpTag)
            {
                return m_lastContextMenuKeys != null
                    && HelpFilePath != null
                    && ((ContextMenuHelpTag)tag).Index < m_lastContextMenuKeys.Length;
            }

            return false;
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="tag">Command to be done</param>
        public void DoCommand(object tag)
        {
            if (tag is Commands)
            {
                switch ((Commands)tag)
                {
                    case Commands.Help:
                        ShowHelp(System.Windows.Forms.HelpNavigator.TableOfContents);
                        break;
                    case Commands.HelpReleaseNotes:
                        ShowReleaseNotes();
                        break;
                    case Commands.HelpAbout:
                        ShowHelpAbout();
                        break;

                }
            }
            else if (tag is ContextMenuHelpTag)
            {
                int index = ((ContextMenuHelpTag)tag).Index;
                if (index < m_lastContextMenuKeys.Length)
                {
                    string key = m_lastContextMenuKeys[index];
                    ShowHelp(key);
                }
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="commandState">Command info to update</param>
        public void UpdateCommand(object commandTag, CommandState commandState) { }

        #endregion

        #region IContextMenuCommandProvider Members

        /// <summary>
        /// Gets command tags for context menu (right click) commands</summary>
        /// <param name="context">Context containing target object</param>
        /// <param name="target">Right clicked object, or null if none</param>
        /// <returns>Command tags for context menu</returns>
        public IEnumerable<object> GetCommands(object context, object target)
        {
            if (ShowContextHelp && HelpFilePath != null)
            {
                m_lastContextMenuKeys = TryGetHelpKeys(target);

                if (m_lastContextMenuKeys == null)
                {
                    m_lastContextMenuKeys = TryGetHelpKeys(context);
                }

                if (m_lastContextMenuKeys != null)
                {
                    for (int i = 0; i < m_lastContextMenuKeys.Length; i++ )
                    {
                        ICommandItem command = m_contextMenuHelpCommands[i].GetCommandItem();
                        command.Text = m_lastContextMenuKeys[i];
                        yield return m_contextMenuHelpCommands[i].CommandTag;
                    }
                }
            }
        }

        #endregion

        private string[] TryGetHelpKeys(object target)
        {
            var adaptable = target as IAdaptable;

            if (adaptable != null)
            {
                // Walk up Dom Hierarchy until we find a help key
                var ctx = adaptable.As<IHelpContext>();
                if (ctx != null)
                {
                    return ctx.GetHelpKeys();
                }
            }

            return null;
        }

        private string[] m_lastContextMenuKeys;
        private string m_applicationName;

        private static readonly CommandInfo s_helpCommand = new CommandInfo(
             Commands.Help,
             StandardMenu.Help,
             Groups.Help,
             "_Contents".Localize(),
             "Help Contents".Localize(),
             Keys.F1,
             null,
             CommandVisibility.Menu);

        private static readonly CommandInfo s_releaseNotesCommand = new CommandInfo(
             Commands.HelpReleaseNotes,
             StandardMenu.Help,
             Groups.Help,
             "_Release Notes".Localize(),
             "Release Notes".Localize(),
             Keys.None,
             null,
             CommandVisibility.Menu);

        private static readonly CommandInfo s_helpAboutCommand = new CommandInfo(
            Commands.HelpAbout,
            StandardMenu.Help,
            Groups.Help,
            "_About".Localize(),
            "Shows Information About Application".Localize(),
            Keys.None,
            null,
            CommandVisibility.Menu);

        private class ContextMenuHelpTag
        {
            public int Index { get; set; }
        }

        private readonly List<CommandInfo> m_contextMenuHelpCommands = new List<CommandInfo>();
    }

}
