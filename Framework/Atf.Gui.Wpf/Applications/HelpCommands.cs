//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using System.Windows.Input;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;
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
        /// Gets or sets whether to show context-sensitive help</summary>
        public bool ShowContextHelp { get; set; }

        /// <summary>
        /// Gets or sets the application help is obtained for</summary>
        public string ApplicationName 
        {
            get { return m_applicationName; }
            set
            {
                m_applicationName = value;
                m_aboutCommand.Text = "About".Localize() + " " + ApplicationName;
                m_aboutCommand.Description = m_aboutCommand.Text;
            }
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by registering Help command, setting default help path,
        /// and setting up Settings Service</summary>
        public void Initialize()
        {
            ShowContextHelp = true;
            m_commandService.RegisterCommand(s_helpCommand, this);
            m_aboutCommand = m_commandService.RegisterCommand(s_helpAboutCommand, this);

            for (int i = 0; i < kMaxContextHelpKeys; i++)
            {
                var commandItem = m_commandService.RegisterCommand(
                    new CommandDef(
                    new ContextMenuHelpTag() { Index = i },
                    null,
                    Groups.Help,
                    "Help".Localize(),
                    new string[] { "Help".Localize() },
                    "Help".Localize(),
                    null,
                    null,
                    Sce.Atf.Applications.CommandVisibility.None), this);

                m_contextMenuHelpCommands.Add(commandItem);
            }

            // Set up a default help path
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Help.chm");
            if (File.Exists(path))
                HelpFilePath = path;

            // Set up a default application name

            // Register help setting
            if(m_settingsService != null)
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
            var dlg = new AboutDialog();
            dlg.DataContext = new AboutDialogViewModel();
            dlg.Owner = Application.Current.MainWindow;
            dlg.ShowDialog();
        }

        #region Sce.Atf.Applications.ICommandClient Members

        /// <summary>
        /// Checks whether the client can do the command, if it handles it</summary>
        /// <param name="commandObj">Command to be done</param>
        /// <returns>True iff client can do the command</returns>
        public bool CanDoCommand(object commandObj)
        {
            ICommandItem command = commandObj as ICommandItem;
            Requires.NotNull(command, "Object specified is from class that doesn't implement ICommandItem.  Most likely, this is a not a command from WPF, and it should be.");
            if (command.CommandTag is Commands)
            {
                switch ((Commands)command.CommandTag)
                {
                    case Commands.Help: return HelpFilePath != null;
                    case Commands.HelpAbout: return true;
                }
            }
            else if (ShowContextHelp && command.CommandTag is ContextMenuHelpTag)
            {
                return m_lastContextMenuKeys != null
                    && HelpFilePath != null
                    && ((ContextMenuHelpTag)command.CommandTag).Index < m_lastContextMenuKeys.Length;
            }

            return false;
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandObj">Command to be done</param>
        public void DoCommand(object commandObj)
        {
            ICommandItem command = commandObj as ICommandItem;
            Requires.NotNull(command, "Object specified is from class that doesn't implement ICommandItem.  Most likely, this is a not a command from WPF, and it should be.");
            if (command.CommandTag is Commands)
            {
                switch ((Commands)command.CommandTag)
                {
                    case Commands.Help:
                        ShowHelp(System.Windows.Forms.HelpNavigator.TableOfContents);
                        break;
                    case Commands.HelpAbout:
                        ShowHelpAbout();
                        break;

                }
            }
            else if (command.CommandTag is ContextMenuHelpTag)
            {
                int index = ((ContextMenuHelpTag)command.CommandTag).Index;
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
        public void UpdateCommand(object commandTag, Sce.Atf.Applications.CommandState commandState) { }

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
                        m_contextMenuHelpCommands[i].Text = m_lastContextMenuKeys[i];
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
        private ICommandItem m_aboutCommand;

        private static CommandDef s_helpCommand = new CommandDef(
             Commands.Help,
             Sce.Atf.Applications.StandardMenu.Help,
             Groups.Help,
             "_Contents".Localize(),
             null,
             "Help Contents".Localize(),
             null,
             new InputGesture[]{new KeyGesture(Key.F1)},
             Sce.Atf.Applications.CommandVisibility.Menu);

        private static CommandDef s_helpAboutCommand = new CommandDef(
            Commands.HelpAbout,
            Sce.Atf.Applications.StandardMenu.Help,
            Groups.Help,
            "_About".Localize(), // TODO
            null,
            "Shows Information About Application".Localize(), // TODO
            null,
            null,
            Sce.Atf.Applications.CommandVisibility.Menu);

        private class ContextMenuHelpTag
        {
            public int Index { get; set; }
        }

        private List<ICommandItem> m_contextMenuHelpCommands = new List<ICommandItem>();
    }

}
