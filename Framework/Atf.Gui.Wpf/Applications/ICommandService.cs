//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Windows.Input;

using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Interface for command services</summary>
    public interface ICommandService
    {
        /// <summary>
        /// Gets an enumeration of all registered commands</summary>
        IEnumerable<ICommandItem> Commands { get; }

        /// <summary>
        /// Gets an enumeration of all registered menus</summary>
        IEnumerable<MenuDef> Menus { get; }

        /// <summary>
        /// Creates and registers a command for a command client</summary>
        /// <param name="definition">Command definition</param>
        /// <param name="client">Command client</param>
        /// <returns>ICommandItem for command</returns>
        ICommandItem RegisterCommand(CommandDef definition, ICommandClient client);

        /// <summary>
        /// Unregisters a command for a command client</summary>
        /// <param name="command">ICommandItem for command</param>
        /// <param name="client">Client that handles the command</param>
        void UnregisterCommand(ICommandItem command, ICommandClient client);

        /// <summary>
        /// Registers a menu for the application</summary>
        /// <param name="definition">Menu definition</param>
        void RegisterMenu(MenuDef definition);

        /// <summary>
        /// Sets the active client that receives a command, for the case when multiple
        /// ICommandClient objects have registered for the same command tag (such as the
        /// StandardCommand.EditCopy enum, for example). Set to null to reduce the priority
        /// of the previously active client.</summary>
        /// <param name="client">Command client, null if client is deactivated</param>
        void SetActiveClient(ICommandClient client);

        /// <summary>
        ///  Suggests that ability of command to be executed by requeried</summary>
        void SuggestRequery();

        /// <summary>
        /// Looks up command, if any, for a command tag</summary>
        /// <param name="commandTag">Unique command tag</param>
        /// <returns>ICommandItem for command or default value for ICommandItem if no command for tag</returns>
        ICommandItem GetCommand(object commandTag);

        /// <summary>
        /// Event that is raised when a command is registered</summary>
        event EventHandler<ItemInsertedEventArgs<ICommandItem>> CommandAdded;

        /// <summary>
        /// Event that is raised when a command is unregistered</summary>
        event EventHandler<ItemRemovedEventArgs<ICommandItem>> CommandRemoved;

        /// <summary>
        /// Event that is raised when a menu is registered</summary>
        event EventHandler<ItemInsertedEventArgs<MenuDef>> MenuAdded;
    }

    /// <summary>
    /// Useful static / extension methods for ICommandRegistry</summary>
    public static class CommandRegistries
    {
        /// <summary>
        /// Registers a menu for the application</summary>
        /// <param name="commandRegistry">ICommandService</param>
        /// <param name="menuTag">Menu's unique ID</param>
        /// <param name="menuText">Command text as it appears in menu</param>
        /// <param name="description">Menu description</param>
        public static void RegisterMenu(
            this ICommandService commandRegistry, object menuTag, string menuText, string description)
        {
            Requires.NotNull(commandRegistry, "commandRegistry");
            var def = new MenuDef(menuTag, menuText, description);
            commandRegistry.RegisterMenu(def);
        }

        /// <summary>
        /// Creates and registers a command</summary>
        /// <param name="commandRegistry">ICommandService</param>
        /// <param name="commandTag">Unique command ID</param>
        /// <param name="menuTag">Unique ID for menu command attached to</param>
        /// <param name="groupTag">Unique ID for command's group</param>
        /// <param name="menuText">Command text as it appears in menu</param>
        /// <param name="description">Command description</param>
        /// <param name="client">ICommandClient</param>
        /// <returns>ICommandItem for command</returns>
        public static ICommandItem RegisterCommand(
            this ICommandService commandRegistry,
            object commandTag,
            object menuTag,
            object groupTag,
            string menuText,
            string description,
            ICommandClient client)
        {
            Requires.NotNull(commandRegistry, "commandRegistry");
            var def = new CommandDef(commandTag, menuTag, groupTag, menuText, description);
            return commandRegistry.RegisterCommand(def, client);
        }
        /// <summary>
        /// Creates and registers a command</summary>
        /// <param name="commandRegistry">ICommandService</param>
        /// <param name="commandTag">Unique command ID</param>
        /// <param name="menuTag">Unique ID for menu command attached to</param>
        /// <param name="groupTag">Unique ID for command's group</param>
        /// <param name="menuText">Command text as it appears in menu</param>
        /// <param name="description">Command description</param>
        /// <param name="shortcut">Command shortcut</param>
        /// <param name="imageSourceKey">Image resource for command</param>
        /// <param name="client">ICommandClient</param>
        /// <returns>ICommandItem for command</returns>
        public static ICommandItem RegisterCommand(
            this ICommandService commandRegistry,
            object commandTag,
            object menuTag,
            object groupTag,
            string menuText,
            string description,
            KeyGesture shortcut,
            object imageSourceKey,
            ICommandClient client)
        {
            Requires.NotNull(commandRegistry, "commandRegistry");
            var gestures = shortcut != null ? new InputGesture[]{shortcut} : null;
            var def = new CommandDef(commandTag, menuTag, groupTag, menuText, null, description, imageSourceKey, gestures, CommandVisibility.Default);
            return commandRegistry.RegisterCommand(def, client);
        }

        /// <summary>
        /// Creates and registers a command</summary>
        /// <param name="commandRegistry">ICommandService</param>
        /// <param name="commandTag">Unique command ID</param>
        /// <param name="menuTag">Unique ID for menu command attached to</param>
        /// <param name="groupTag">Unique ID for command's group</param>
        /// <param name="menuText">Command text as it appears in menu</param>
        /// <param name="description">Command description</param>
        /// <param name="shortcut">Command shortcut</param>
        /// <param name="imageSourceKey">Image resource for command</param>
        /// <param name="visibility">Flags indicating where command is visible: on toolbar, menus, etc.</param>
        /// <param name="client">ICommandClient</param>
        /// <returns>ICommandItem for command</returns>
        public static ICommandItem RegisterCommand(
            this ICommandService commandRegistry,
            object commandTag,
            object menuTag,
            object groupTag,
            string menuText,
            string description,
            KeyGesture shortcut,
            object imageSourceKey,
            CommandVisibility visibility,
            ICommandClient client)
        {
            Requires.NotNull(commandRegistry, "commandRegistry");
            var gestures = shortcut != null ? new InputGesture[] { shortcut } : null;
            var def = new CommandDef(commandTag, menuTag, groupTag, menuText, null, description, imageSourceKey, gestures, visibility);
            return commandRegistry.RegisterCommand(def, client);
        }
    }
}
