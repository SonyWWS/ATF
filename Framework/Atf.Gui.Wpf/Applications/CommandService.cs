//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;

using Sce.Atf.Applications;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Service that handles commands in menus and toolbars</summary>
    [Export(typeof(ICommandService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CommandService : Sce.Atf.Wpf.Applications.ICommandService
    {
        public CommandService()
        {
            RegisterMenu(StandardMenus.File);
            RegisterMenu(StandardMenus.Edit);
            RegisterMenu(StandardMenus.View);
            RegisterMenu(StandardMenus.Modify);
            RegisterMenu(StandardMenus.Format);
            RegisterMenu(StandardMenus.Window);
            RegisterMenu(StandardMenus.Help);
        }

        #region ICommandService Members

        /// <summary>
        /// Creates and registers a command for a command client</summary>
        /// <param name="def">Command definition</param>
        /// <param name="client">Command client</param>
        /// <returns>ICommandItem for command</returns>
        public ICommandItem RegisterCommand(CommandDef def, ICommandClient client)
        {
            Requires.NotNull(client, "client");

            // Problem - what about same command tag registered in several places with different shortcuts
            // submenus etc?
            ICommandItem command;
            if (!m_commandsLookup.TryGetValue(def.CommandTag, out command))
            {
                if (!CommandIsUnique(def.MenuTag, def.CommandTag))
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Duplicate menu/command combination. CommandTag: {0}, MenuTag: {1}, GroupTag: {2}, MenuText: {3}",
                            def.CommandTag, def.GroupTag, def.MenuTag, def.Text));
                }

                command = new CommandItem(def, CanDoCommand, CommandExecuted);
                m_commands.Add(command);
                m_commands.Sort(new CommandComparer());
                m_commandsLookup.Add(command.CommandTag, command);
                int index = m_commands.IndexOf(command);
                CommandAdded.Raise<ItemInsertedEventArgs<ICommandItem>>(this, new ItemInsertedEventArgs<ICommandItem>(index, command));
            }

            m_commandClients.Add(def.CommandTag, client);

            return command;
        }

        /// <summary>
        /// Creates and registers a command for a command client</summary>
        /// <param name="command">ICommand defining a command</param>
        /// <returns>ICommandItem for command</returns>
        /// <exception cref="NotImplementedException">Not implemented</exception>
        public ICommandItem RegisterCommand(ICommand command)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Unregisters a command for a command client</summary>
        /// <param name="command">ICommandItem for command</param>
        /// <param name="client">Client that handles the command</param>
        public void UnregisterCommand(ICommandItem command, ICommandClient client)
        {
            if (command.CommandTag is StandardCommand)
                return;

            if (client == null)
                m_commandClients.Remove(command.CommandTag);
            else
                m_commandClients.Remove(command.CommandTag, client);

            if (!m_commandClients.TryGetFirst(command.CommandTag, out client))
            {
                m_commandsLookup.Remove(command.CommandTag);
                int index = m_commands.IndexOf(command);
                m_commands.Remove(command);
                CommandRemoved.Raise<ItemRemovedEventArgs<ICommandItem>>(this, new ItemRemovedEventArgs<ICommandItem>(index, command));
            }
        }

        /// <summary>
        /// Registers a menu for the application.
        /// NOTE: This could be replaced with MEF composition?</summary>
        /// <param name="definition">Menu definition</param>
        public void RegisterMenu(MenuDef definition)
        {
            Requires.NotNull(definition.MenuTag, "menuTag");
            if(m_menus.Any<MenuDef>(x => x.MenuTag == definition.MenuTag))
                throw new ArgumentException("Menu already registered");

            m_menus.Add(definition);
            int index = m_menus.IndexOf(definition);
            MenuAdded.Raise<ItemInsertedEventArgs<MenuDef>>(this, new ItemInsertedEventArgs<MenuDef>(index, definition));
        }

        /// <summary>
        /// Sets the active client that receives a command, for the case when multiple
        /// ICommandClient objects have registered for the same command tag (such as the
        /// StandardCommand.EditCopy enum, for example). Set to null to reduce the priority
        /// of the previously active client.</summary>
        /// <param name="client">Command client, null if client is deactivated</param>
        public void SetActiveClient(ICommandClient client)
        {
            List<object> commandTags = new List<object>(m_commandClients.Keys);

            // 'client' being null is an indication to pop the most recently active client
            if (client == null && m_activeClient != null)
            {
                // make sure previous client will NOT be the last registered for its command tags
                foreach (object commandTag in commandTags)
                {
                    if (m_commandClients.ContainsKeyValue(commandTag, m_activeClient))
                        m_commandClients.AddFirst(commandTag, m_activeClient);
                }
            }

            m_activeClient = client;

            if (m_activeClient != null)
            {
                // make sure client will be the last registered for its command tags
                foreach (object commandTag in commandTags)
                {
                    if (m_commandClients.ContainsKeyValue(commandTag, client))
                        m_commandClients.Add(commandTag, client);
                }
            }
        }

        /// <summary>
        ///  Suggests that ability of command to be executed by requeried.
        ///  Forces the System.Windows.Input.CommandManager to raise the System.Windows.Input.CommandManager.RequerySuggested event. 
        ///  This event occurs when the System.Windows.Input.CommandManager detects conditions that might change the ability of a command to execute.</summary>
        public void SuggestRequery()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Looks up command, if any, for a command tag</summary>
        /// <param name="commandTag">Unique command tag</param>
        /// <returns>ICommandItem for command or default value for ICommandItem if no command for tag</returns>
        public ICommandItem GetCommand(object commandTag)
        {
            ICommandItem command;
            m_commandsLookup.TryGetValue(commandTag, out command);
            return command;
        }

        /// <summary>
        /// Gets an enumeration of all registered commands</summary>
        public IEnumerable<ICommandItem> Commands { get { return m_commands; } }

        /// <summary>
        /// Gets an enumeration of all registered menus</summary>
        public IEnumerable<MenuDef> Menus { get { return m_menus; } }

        /// <summary>
        /// Event that is raised when a command is registered</summary>
        public event EventHandler<ItemInsertedEventArgs<ICommandItem>> CommandAdded;

        /// <summary>
        /// Event that is raised when a command is unregistered</summary>
        public event EventHandler<ItemRemovedEventArgs<ICommandItem>> CommandRemoved;

        /// <summary>
        /// Event that is raised when a menu is registered</summary>
        public event EventHandler<ItemInsertedEventArgs<MenuDef>> MenuAdded;

        #endregion

        private void CommandExecuted(ICommandItem command)
        {
            if (command != null)
            {
                var client = GetClient(command.CommandTag);
                if(client != null)
                    client.DoCommand(command);
            }
        }

        private bool CanDoCommand(object commandObj)
        {
            ICommandItem command = commandObj as ICommandItem;
            Requires.NotNull(command, "Object specified does not implement ICommandItem.");

            if (command != null)
            {
                var client = GetClient(command.CommandTag);
                if(client != null)
                    return client.CanDoCommand(command);
            }
            return false;
        }

        private ICommandClient GetClient(object commandtag)
        {
            // return last registered client
            ICommandClient client;
            m_commandClients.TryGetLast(commandtag, out client);
            return client;
        }

        private ICommandClient GetClientOrActive(object commandtag)
        {
            var client = GetClient(commandtag);
            if (client == null)
                client = m_activeClient;
            return client;
        }

        private bool CommandIsUnique(object menuTag, object commandTag)
        {
            // check for the same menu tag and menu commandTag, which should catch most accidental
            //  duplication
            foreach (var info in m_commands)
            {
                if (GetClient(info.CommandTag) == null)
                    continue;

                if (CommandComparer.TagsEqual(info.MenuTag, menuTag) && info.CommandTag == commandTag)
                    return false;
            }

            return true;
        }

        private List<ICommandItem> m_commands = new List<ICommandItem>();
        private Dictionary<object, ICommandItem> m_commandsLookup = new Dictionary<object, ICommandItem>();
        private List<MenuDef> m_menus = new List<MenuDef>();
        private Multimap<object, ICommandClient> m_commandClients = new Multimap<object, ICommandClient>();
        private ICommandClient m_activeClient;
    }
}
