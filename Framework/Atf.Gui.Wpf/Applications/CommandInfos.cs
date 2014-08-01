//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Extension methods for Sce.Atf.Applications.CommandInfo that are used with WPF</summary>
    public static class CommandInfos
    {
        /// <summary>
        /// Gets the ICommandItem associated with this CommandInfo</summary>
        /// <param name="commandInfo">CommandInfo</param>
        /// <returns>The command item</returns>
        public static ICommandItem GetCommandItem(this CommandInfo commandInfo)
        {
            if (commandInfo.CommandService == null)
                throw new NullReferenceException("CommandInfo has not been registered to a CommandService.");

            var commandService = (CommandService)commandInfo.CommandService;
            if (commandService == null)
                throw new InvalidTransactionException("CommandInfo was registered to an ICommandService, but not specifically to a CommandService.");

            ICommandItem commanditem = commandService.GetCommand(commandInfo.CommandTag);
            if (commanditem == null)
                throw new InvalidTransactionException("CommandService to which CommandInfo thinks it's registered has no record of it.");

            return commanditem;
        }
    }
}
