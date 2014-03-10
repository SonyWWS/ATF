//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// MenuInfo extension methods</summary>
    public static class MenuInfos
    {
        /// <summary>
        /// Obtains ToolStripMenuItem for MenuInfo</summary>
        /// <param name="menuInfo">MenuInfo</param>
        /// <returns>ToolStripMenuItem for MenuInfo</returns>
        public static ToolStripMenuItem GetMenuItem(this MenuInfo menuInfo)
        {
            if (menuInfo.CommandService == null)
                throw new NullReferenceException("MenuInfo has not been registered to a CommandService.");

            CommandService commandService = (CommandService)menuInfo.CommandService;
            if (commandService == null)
                throw new InvalidTransactionException("MenuInfo was registered to an ICommandService, but not specifically to Sce.Atf.Applications.CommandService.");

            ToolStripMenuItem menuItem = commandService.GetMenuToolStripItem(menuInfo);
            if (menuItem == null)
                throw new InvalidTransactionException("The MenuInfo specified has no ToolStripMenuItem associated with it, which should have been set up in CommandService.RegisterMenuInfo()");

            return menuItem;
        }

        /// <summary>
        /// Obtains ToolStrip for MenuInfo</summary>
        /// <param name="menuInfo">MenuInfo</param>
        /// <returns>ToolStrip for MenuInfo</returns>
        public static ToolStrip GetToolStrip(this MenuInfo menuInfo)
        {
            if (menuInfo.CommandService == null)
                throw new NullReferenceException("MenuInfo has not been registered to a CommandService.");

            CommandService commandService = (CommandService)menuInfo.CommandService;
            if (commandService == null)
                throw new InvalidTransactionException("MenuInfo was registered to an ICommandService, but not specifically to Sce.Atf.Applications.CommandService.");

            ToolStrip toolStrip = commandService.GetMenuToolStrip(menuInfo);
            if (toolStrip == null)
                throw new InvalidTransactionException("The MenuInfo specified has no ToolStrip associated with it, which should have been set up in (or before) CommandService.RegisterMenuInfo()");

            return toolStrip;
        }

        /// <summary>
        /// Sets ToolStrip for MenuInfo</summary>
        /// <param name="menuInfo">ToolStrip for MenuInfo</param>
        /// <param name="toolStrip">ToolStrip to set</param>
        /// <param name="commandService">Command service</param>
        public static void SetToolStrip(this MenuInfo menuInfo, ToolStrip toolStrip, CommandService commandService)
        {
            if (menuInfo.CommandService != null && menuInfo.CommandService != commandService)
                throw new NullReferenceException("MenuInfo has already been registered to a CommandService, and it is not the one specified.");

            if (toolStrip == null)
                throw new InvalidTransactionException("ToolStrip cannot be null");

            if (commandService == null)
                throw new InvalidTransactionException("CommandService cannot be null");

            commandService.SetMenuToolStrip(menuInfo, toolStrip);
        }
    }
}