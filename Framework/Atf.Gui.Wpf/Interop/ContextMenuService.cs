//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Input;

using Sce.Atf.Wpf.Applications;
using Sce.Atf.Wpf.Models;

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
        #region IContextMenuService

        /// <summary>
        /// Returns a context menu containing commands with commandTags</summary>
        /// <param name="commandTags">Command tags for commands to include on menu</param>
        /// <returns>ContextMenu</returns>
        public ContextMenu GetContextMenu(IEnumerable<object> commandTags)
        {
            m_commandService.SuggestRequery();

            var menu = new ContextMenu();
            menu.SetResourceReference(ContextMenu.StyleProperty, Resources.MenuStyleKey);
            //menu.Style = (Style)Application.Current.FindResource(Resources.MenuStyleKey);

            // Generate view model
            List<ICommandItem> commands = new List<ICommandItem>();
            foreach (var tag in commandTags)
            {
                var command = m_commandService.GetCommand(tag);
                if (command != null)
                {
                    if (!AutoCompact || ((ICommand)command).CanExecute(command))
                        commands.Add(command);
                }
            }
            commands.Sort(new CommandComparer());

            var dummyRootMenu = new Sce.Atf.Wpf.Models.Menu(null, null, null, null, null);
            foreach (var command in commands)
                MenuUtil.BuildSubMenus(command, dummyRootMenu);

            MenuUtil.InsertGroupSeparators(dummyRootMenu);
            menu.ItemsSource = dummyRootMenu.ChildCollection;

            return menu;
        }

        /// <summary>
        /// Gets or sets auto menu compacting in which only commands that can execute are displayed</summary>
        public bool AutoCompact { get; set; }

        #endregion

        [Import]
        private ICommandService m_commandService = null;
    }
}
