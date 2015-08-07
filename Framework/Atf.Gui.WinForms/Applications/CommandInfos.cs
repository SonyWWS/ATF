//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;

using Keys = Sce.Atf.Input.Keys;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Extension methods for Sce.Atf.Applications.CommandInfo that are used with WinForms</summary>
    public static class CommandInfos
    {
        /// <summary>
        /// Gets the ToolStripMenuItem associated with this CommandInfo that was previously registered
        /// to the WinForms Sce.Atf.Applications.CommandService</summary>
        /// <param name="commandInfo">CommandInfo</param>
        /// <returns>ToolStripMenuItem associated with given CommandInfo</returns>
        public static ToolStripMenuItem GetMenuItem(this CommandInfo commandInfo)
        {
            return GetCommandControls(commandInfo).MenuItem;
        }

        /// <summary>
        /// Gets the ToolStripButton associated with this CommandInfo that was previously registered
        /// to the WinForms Sce.Atf.Applications.CommandService</summary>
        /// <param name="commandInfo">CommandInfo</param>
        /// <returns>ToolStripButton associated with given CommandInfo</returns>
        public static ToolStripButton GetButton(this CommandInfo commandInfo)
        {
            return GetCommandControls(commandInfo).Button;
        }

        /// <summary>
        /// Gets the ToolStripMenuItem and ToolStripButton associated with this CommandInfo that was
        /// previously registered to the WinForms Sce.Atf.Applications.CommandService</summary>
        /// <param name="commandInfo">CommandInfo</param>
        /// <param name="menuItem">ToolStripMenuItem associated with given CommandInfo</param>
        /// <param name="button">ToolStripButton associated with given CommandInfo</param>
        public static void GetMenuItemAndButton(this CommandInfo commandInfo, out ToolStripMenuItem menuItem, out ToolStripButton button)
        {
            CommandService.CommandControls controls = GetCommandControls(commandInfo);
            menuItem = controls.MenuItem;
            button = controls.Button;
        }

        private static CommandService.CommandControls GetCommandControls(this CommandInfo commandInfo)
        {
            if (commandInfo.CommandService == null)
                throw new InvalidOperationException("CommandInfo has not been registered to a CommandService.");

            var commandService = (CommandService)commandInfo.CommandService;
            if (commandService == null)
                throw new InvalidOperationException("CommandInfo was registered to an ICommandService, but not specifically to a WinFormsCommandService.");

            CommandService.CommandControls commandControls = commandService.GetCommandControls(commandInfo);
            if (commandControls == null)
                throw new InvalidOperationException("WinForms CommandService to which CommandInfo thinks it's registered has no record of it.");

            return commandControls;
        }

        /// <summary>
        /// Builds a string from the list of keyboard shortcuts for activating this command.
        /// Should be called after any change is made to the keyboard shortcuts list.</summary>
        /// <param name="commandInfo">CommandInfo for command</param>
        public static void RebuildShortcutDisplayString(this CommandInfo commandInfo)
        {
            var menuItem = commandInfo.GetMenuItem();
            if (menuItem == null)
                return;

            string displayString = string.Empty;
            foreach (Keys k in commandInfo.Shortcuts)
            {
                if (k == Keys.None)
                    continue;

                if (displayString != string.Empty)
                    displayString += " ; ";

                displayString += Sce.Atf.KeysUtil.KeysToString(k, true);
            }
            menuItem.ShortcutKeyDisplayString = displayString;
        }
    }
}
