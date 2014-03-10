//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;

using Sce.Atf.Input;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for service that presents commands in menu and toolbar controls</summary>
    public interface ICommandService
    {
        /// <summary>
        /// Registers a menu for the application</summary>
        /// <param name="info">Menu description; standard menus are defined as static members
        /// on the MenuInfo class</param>
        void RegisterMenu(MenuInfo info);

        /// <summary>
        /// Registers a command for a command client</summary>
        /// <param name="info">Command description; standard commands are defined as static
        /// members on the CommandInfo class</param>
        /// <param name="client">Client that handles the command</param>
        void RegisterCommand(CommandInfo info, ICommandClient client);

        /// <summary>
        /// Unregisters a command for a command client</summary>
        /// <param name="commandTag">Command tag that identifies CommandInfo used to register
        /// the command</param>
        /// <param name="client">Client that handles the command</param>
        void UnregisterCommand(object commandTag, ICommandClient client);

        /// <summary>
        /// Displays a context menu at the point, in screen coordinates</summary>
        /// <param name="commandTags">Commands to display in menu</param>
        /// <param name="screenPoint">Point, in screen coordinates, of menu top
        /// left corner</param>
        void RunContextMenu(IEnumerable<object> commandTags, Point screenPoint);

        /// <summary>
        /// Sets the active client that receives a command for the case when multiple
        /// ICommandClient objects have registered for the same command tag (such as the
        /// StandardCommand.EditCopy enum, for example). Set to null to reduce the priority
        /// of the previously active client.</summary>
        /// <param name="client">Command client, null if client is deactivated</param>
        void SetActiveClient(ICommandClient client);

        /// <summary>
        /// Reserves a shortcut key, so it is not available as a command shortcut</summary>
        /// <param name="key">Reserved key</param>
        /// <param name="reason">Reason why key is reserved, to display to user</param>
        void ReserveKey(Keys key, string reason);

        /// <summary>
        /// Attempts to process the key as a command shortcut</summary>
        /// <param name="key">Key to process</param>
        /// <returns>True iff the key was processed as a command shortcut</returns>
        bool ProcessKey(Keys key);

        /// <summary>
        /// Event that is raised when processing a key; clients can subscribe to this event
        /// to intercept certain hot keys for custom handling</summary>
        event EventHandler<KeyEventArgs> ProcessingKey;
    }

    /// <summary>
    /// Useful static and extension methods for ICommandService</summary>
    public static class CommandServices
    {
        /// <summary>
        /// Registers a menu for the application</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="menuTag">Menu's unique ID</param>
        /// <param name="menuText">Menu text</param>
        /// <param name="description">Menu description</param>
        /// <returns>MenuInfo object describing menu</returns>
        public static MenuInfo RegisterMenu(
            this ICommandService commandService, object menuTag, string menuText, string description)
        {
            MenuInfo info = new MenuInfo(menuTag, menuText, description);
            commandService.RegisterMenu(info);
            return info;
        }

        /// <summary>
        /// Registers a command for the command client</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="commandTag">Command's unique ID</param>
        /// <param name="menuTag">Containing menu's unique ID, or null</param>
        /// <param name="groupTag">Containing menu group's unique ID, or null</param>
        /// <param name="menuText">Command text as it appears in menu</param>
        /// <param name="description">Command description</param>
        /// <param name="client">Client that performs command</param>
        /// <returns>CommandInfo object describing command</returns>
        public static CommandInfo RegisterCommand(
            this ICommandService commandService,
            object commandTag,
            object menuTag,
            object groupTag,
            string menuText,
            string description,
            ICommandClient client)
        {
            CommandInfo info = new CommandInfo(commandTag, menuTag, groupTag, menuText, description);
            commandService.RegisterCommand(info, client);
            return info;
        }

        /// <summary>
        /// Registers a command for the command client</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="commandTag">Command's unique ID</param>
        /// <param name="menuTag">Containing menu's unique ID, or null</param>
        /// <param name="groupTag">Containing menu group's unique ID, or null</param>
        /// <param name="menuText">Command text as it appears in menu</param>
        /// <param name="description">Command description</param>
        /// <param name="shortcut">Command shortcut, or Keys.None if none</param>
        /// <param name="imageName">Text identifying image, or null if none</param>
        /// <param name="client">Client that performs command</param>
        /// <returns>CommandInfo object describing command</returns>
        public static CommandInfo RegisterCommand(
            this ICommandService commandService,
            object commandTag,
            object menuTag,
            object groupTag,
            string menuText,
            string description,
            Keys shortcut,
            string imageName,
            ICommandClient client)
        {
            CommandInfo info = new CommandInfo(commandTag, menuTag, groupTag, menuText, description, shortcut, imageName);
            commandService.RegisterCommand(info, client);
            return info;
        }

        /// <summary>
        /// Registers a command for the command client</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="commandTag">Command's unique ID</param>
        /// <param name="menuTag">Containing menu's unique ID, or null</param>
        /// <param name="groupTag">Containing menu group's unique ID, or null</param>
        /// <param name="menuText">Command text as it appears in menu</param>
        /// <param name="description">Command description</param>
        /// <param name="shortcut">Command shortcut, or Keys.None if none</param>
        /// <param name="imageName">Text identifying image, or null if none</param>
        /// <param name="visibility">Whether command is visible in menus and toolbars</param>
        /// <param name="client">Client that performs command</param>
        /// <returns>CommandInfo object describing command</returns>
        public static CommandInfo RegisterCommand(
            this ICommandService commandService,
            object commandTag,
            object menuTag,
            object groupTag,
            string menuText,
            string description,
            Keys shortcut,
            string imageName,
            CommandVisibility visibility,
            ICommandClient client)
        {
            CommandInfo info = new CommandInfo(commandTag, menuTag, groupTag, menuText, description, shortcut, imageName, visibility);
            commandService.RegisterCommand(info, client);
            return info;
        }

        /// <summary>
        /// Registers a command for the command client</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="commandTag">Command's unique ID</param>
        /// <param name="visibility">Whether command is visible in menus and toolbars</param>
        /// <param name="client">Client that performs command</param>
        /// <returns>Menu/Toolbar command information</returns>
        public static CommandInfo RegisterCommand(
            this ICommandService commandService,
            StandardCommand commandTag,
            CommandVisibility visibility,
            ICommandClient client)
        {
            CommandInfo info = CommandInfo.GetStandardCommand(commandTag);
            commandService.RegisterCommand(info, client);
            info.Visibility = visibility;
            return info;
        }
    }
}
