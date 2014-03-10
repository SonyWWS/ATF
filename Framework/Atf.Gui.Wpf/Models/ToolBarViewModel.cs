//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;

using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// View model for an application's tool bars</summary>
    [ExportViewModel(Contracts.ToolBarViewModel)]
    public class ToolBarViewModel : NotifyPropertyChangedBase
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        [ImportingConstructor]
        public ToolBarViewModel(ICommandService commandService)
        {
            m_commandService = commandService;

            foreach (var menuDef in m_commandService.Menus)
                AddMenu(menuDef);

            foreach (var command in m_commandService.Commands)
                AddCommand(command);

            m_commandService.MenuAdded += (s, e) => AddMenu(e.Item);
            m_commandService.CommandAdded += (s, e) => AddCommand(e.Item);
            m_commandService.CommandRemoved += (s, e) => RemoveCommand(e.Item);
        }

        /// <summary>
        /// Gets the collection of menus attached to tool bar</summary>
        public ObservableCollection<IMenu> ToolBars { get { return m_toolbars; } }

        private void AddMenu(MenuDef def)
        {
            var menu = new RootMenu(def);
            if (menu.MenuTag is Sce.Atf.Applications.StandardMenu)
                m_toolbars.Add(menu);
            else
                m_toolbars.Insert(m_toolbars.Count - 2, menu); // insert custom menus before Window, Help
        }
        
        private void AddCommand(ICommandItem command)
        {
            if (command.IsVisible(Sce.Atf.Applications.CommandVisibility.Toolbar) && command.MenuTag != null)
            {
                var menu = GetMenu(command.MenuTag) as RootMenu;
                menu.ChildCollection.Add(command);
                menu.Invalidate();
            }
        }

        private void RemoveCommand(ICommandItem command)
        {
            if (command.IsVisible(Sce.Atf.Applications.CommandVisibility.Toolbar) && command.MenuTag != null)
            {
                var menu = GetMenu(command.MenuTag) as RootMenu;
                menu.ChildCollection.Remove(command);
                menu.Invalidate();
            }
        }

        private IMenu GetMenu(object menuTag)
        {
            return m_toolbars.FirstOrDefault<IMenu>(x => CommandComparer.TagsEqual(x.MenuTag, menuTag));
        }

        private ICommandService m_commandService;
        private readonly ObservableCollection<IMenu> m_toolbars = new ObservableCollection<IMenu>();
    }
}
