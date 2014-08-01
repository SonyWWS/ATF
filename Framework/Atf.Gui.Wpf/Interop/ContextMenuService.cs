//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Input;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Applications;
using Sce.Atf.Wpf.Models;
using CommandService = Sce.Atf.Wpf.Applications.CommandService;

namespace Sce.Atf.Wpf.Interop
{
    /// <summary>
    /// Creates context menus.
    /// This class adapts Sce.Atf.Wpf.Applications.IContextMenuService to Sce.Atf.Applications.CommandService. 
    /// This allows legacy code to be run in a WPF based application.
    /// Expect WPF based applications to bind their context menus directly to static commands or commands on view model.</summary>
    [Export(typeof(IContextMenuService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ContextMenuService : IContextMenuService
    {
        public ContextMenuService()
        {
            AutoCompact = true;
        }

        #region IContextMenuService

        /// <summary>
        /// Returns a context menu containing commands with commandTags</summary>
        /// <param name="commandTags">Command tags for commands to include on menu</param>
        /// <returns>ContextMenu</returns>
        public ContextMenu GetContextMenu(IEnumerable<object> commandTags)
        {
            CommandManager.InvalidateRequerySuggested();

            var menu = new ContextMenu();
            menu.SetResourceReference(ContextMenu.StyleProperty, Resources.ContextMenuStyleKey);

            // Generate view model
            var commands = new List<ICommandItem>();
            foreach (object tag in commandTags)
            {
                var command = m_commandService.GetCommand(tag);
                if (command != null)
                {
                    if (!AutoCompact || command.CanExecute(null))
                        commands.Add(command);
                }
                else
                {
                    // Allow direct display of ICommandItems which have not been registered
                    // with the command service
                    var commandItem = tag as ICommandItem;
                    if (commandItem != null && (!AutoCompact || commandItem.CanExecute(null)))
                        commands.Add(commandItem);
                }
            }
            commands.Sort(new CommandComparer());

            var dummyRootMenu = new RootMenuModel(null, null, null);
            foreach (ICommandItem command in commands)
                dummyRootMenu.AddItem(command);

            menu.ItemsSource = dummyRootMenu.Children;

            return menu;
        }

        /// <summary>
        /// Gets or sets auto menu compacting in which only commands that can execute are displayed</summary>
        public bool AutoCompact { get; set; }

        #endregion

        [Import]
        private CommandService m_commandService = null;
    }
}
