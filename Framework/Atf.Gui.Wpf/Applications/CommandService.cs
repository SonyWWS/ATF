//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Windows.Input;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Service to handle commands in menus and toolbars</summary>
    [Export(typeof(CommandService))]
    [Export(typeof(ICommandService))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CommandService : CommandServiceBase, IPartImportsSatisfiedNotification
    {
        /// <summary>
        /// Constructor that registers standard menus</summary>
        public CommandService()
        {
            // Need to cache this handler to stop it being GCed
            // http://msdn.microsoft.com/en-us/library/system.windows.input.commandmanager.requerysuggested.aspx
            m_cachedRequerySuggestedHandler = CommandManager_RequerySuggested;
        }

        /// <summary>
        /// Finish initializing component</summary>
        public override void Initialize()
        {
            // Prevent register of user settings, keyboard shorcut commands etc for now
            //base.Initialize();
            CommandManager.RequerySuggested += m_cachedRequerySuggestedHandler;
        }

        /// <summary>
        /// Get command from tag</summary>
        /// <param name="commandTag">Command tag</param>
        /// <returns>ICommandItem representing a command</returns>
        public ICommandItem GetCommand(object commandTag)
        {
            CommandItem command;
            m_commandsLookup.TryGetValue(commandTag, out command);
            return command;
        }

        #region Overrides

        /// <summary>
        /// Registers menu info</summary>
        /// <param name="info">MenuInfo to register</param>
        protected override sealed void RegisterMenuInfo(MenuInfo info)
        {
            base.RegisterMenuInfo(info);

            // Add an IToolBar to composition
            if (m_composer != null)
                ExportMenuModel(info);
        }

        /// <summary>
        /// Registers a unique CommandInfo object</summary>
        /// <param name="info">CommandInfo to register</param>
        protected override void RegisterCommandInfo(CommandInfo info)
        {
            // Embedded image resources will not be available as WPF app resources
            // If image resource does not exist we need to create it and add it to app resources
            object imageResourceKey = null;
            if ((info.ImageKey == null) && (!string.IsNullOrEmpty(info.ImageName)))
            {
                imageResourceKey = Sce.Atf.Wpf.ResourceUtil.GetKeyFromImageName(info.ImageName);
                info.ImageKey = imageResourceKey;
            }

            base.RegisterCommandInfo(info);

            // Associate the registered MenuInfo with this CommandService.  Only can be registered once.
            info.CommandService = this;

            // If command has not been previously registered then create 
            // a CommandItem for it and add it to the composition
            CommandItem command;
            if (!m_commandsLookup.TryGetValue(info.CommandTag, out command))
            {
                command = new CommandItem(info, CanExecuteCommand, ExecuteCommand);
                m_commandsLookup.Add(command.CommandTag, command);

                if (m_composer != null)
                    command.ComposablePart = m_composer.AddPart(command);
            }
        }

        /// <summary>
        /// Unregisters a unique CommandInfo object</summary>
        /// <param name="commandTag">CommandInfo object to unregister</param>
        /// <param name="client">ICommandClient of command</param>
        public override void UnregisterCommand(object commandTag, ICommandClient client)
        {
            base.UnregisterCommand(commandTag, client);

            // If there are no more clients associated with this command
            // then remove it
            if (!m_commandClients.TryGetFirst(commandTag, out client))
            {
                ICommandItem command = m_commandsLookup[commandTag];
                m_commandsLookup.Remove(commandTag);

                // Remove from composition
                var commandItem = command as CommandItem;
                if (commandItem != null && commandItem.ComposablePart != null)
                {
                    m_composer.RemovePart(commandItem.ComposablePart);
                    commandItem.ComposablePart = null;
                }
            }
        }

        /// <summary>
        /// Runs a context (right click popup) menu at the given screen point</summary>
        /// <exception cref="NotSupportedException"> always because not supported</exception>
        /// <param name="commandTags">Commands in menu; nulls indicate separators</param>
        /// <param name="screenPoint">Point in screen coordinates</param>
        public override void RunContextMenu(IEnumerable<object> commandTags, System.Drawing.Point screenPoint)
        {
            throw new NotSupportedException("Use IContextMenuService");
        }

        #endregion

        #region IPartImportsSatisfiedNotification Members

        /// <summary>
        /// Notification when part's imports have been satisfied</summary>
        public void OnImportsSatisfied()
        {
            foreach (var menuInfo in m_menus)
                ExportMenuModel(menuInfo);
        }

        #endregion

        private bool CanExecuteCommand(ICommandItem command)
        {
            if (command != null)
            {
                var client = GetClient(command.CommandTag);
                if (client != null)
                    return client.CanDoCommand(command.CommandTag);
            }
            return false;
        }

        private void ExecuteCommand(ICommandItem command)
        {
            if (command != null)
            {
                var client = GetClient(command.CommandTag);
                if(client != null)
                    client.DoCommand(command.CommandTag);
            }
        }

        private void ExportMenuModel(MenuInfo menuInfo)
        {
            // Add toolbar/menu model
            var model = new MenuToolBarModel(menuInfo);
            model.ComposablePart = m_composer.AddPart(model);
        }

        private void CommandManager_RequerySuggested(object sender, EventArgs e)
        {
            // Code to help with backwards compatibility with ICommandClients
            // which use the winforms way of updating command text etc
            // instead of just directly setting these properties on the ICommandItem object
            foreach (var commandItem in m_commandsLookup.Values)
                LegacyUpdateCommand(commandItem);
        }

        private void LegacyUpdateCommand(ICommandItem item)
        {
            ICommandClient client = GetClientOrActiveClient(item.CommandTag);
            if (client != null)
            {
                var commandState = new CommandState { Text = item.Text, Check = item.IsChecked };
                client.UpdateCommand(item.CommandTag, commandState);
                item.Text = commandState.Text.Trim();
                item.IsChecked = commandState.Check;
            }
        }

        [Import]
        private IComposer m_composer = null;
        private readonly Dictionary<object, CommandItem> m_commandsLookup = new Dictionary<object, CommandItem>();
        private readonly EventHandler m_cachedRequerySuggestedHandler;

        [Export(typeof(IToolBar))]
        [Export(typeof(Sce.Atf.Wpf.IMenu))]
        internal class MenuToolBarModel : IToolBar, Sce.Atf.Wpf.IMenu
        {
            public MenuToolBarModel(MenuInfo menuInfo)
            {
                MenuInfo = menuInfo;
            }

            public MenuInfo MenuInfo { get; private set; }

            public ComposablePart ComposablePart { get; set; }

            #region IToolBar Members

            public object Tag
            {
                get { return MenuInfo.MenuTag; }
            }

            #endregion

            #region IMenu Members

            public object MenuTag
            {
                get { return MenuInfo.MenuTag; }
            }

            public string Text
            {
                get { return MenuInfo.MenuText; }
            }

            public string Description
            {
                get { return MenuInfo.Description; }
            }

            #endregion
        }
 
    }
}
