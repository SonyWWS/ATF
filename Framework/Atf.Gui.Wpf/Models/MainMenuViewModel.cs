//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;

using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// View model for an application's menus</summary>
    [ExportViewModel(Contracts.MainMenuViewModel)]
    public class MainMenuViewModel : NotifyPropertyChangedBase
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        [ImportingConstructor]
        public MainMenuViewModel(ICommandService commandService)
        {
            m_commandService = commandService;

            foreach (var menuDef in m_commandService.Menus)
                AddMenu(menuDef);

            m_commandService.MenuAdded += (s, e) => AddMenu(e.Item);
            m_commandService.CommandAdded += (s, e) => Invalidate(e.Item.MenuTag);
            m_commandService.CommandRemoved += (s, e) => Invalidate(e.Item.MenuTag);
        }

        /// <summary>
        /// Gets the application's menus</summary>
        public ObservableCollection<IMenu> Menus { get { return m_menus; } }

        private void AddMenu(MenuDef def)
        {
            var menu = new RootMenu(def);
            menu.RefreshRequested += (s, e) => RebuildMenu((Menu)s);

            if (menu.MenuTag is Sce.Atf.Applications.StandardMenu)
                m_menus.Add(menu);
            else
                m_menus.Insert(m_menus.Count - 2, menu); // insert custom menus before Window, Help
        }

        private void Invalidate(object menuTag)
        {
            IMenuItem menu = m_menus.FirstOrDefault<IMenu>(x => CommandComparer.TagsEqual(x.MenuTag, menuTag));
            if (menu != null)
                ((RootMenu)menu).Invalidate();
        }

        private void RebuildMenu(Menu root)
        {
            root.ChildCollection.Clear();

            foreach (var command in m_commandService.Commands.Where<ICommandItem>(x =>
                  CommandComparer.TagsEqual(x.MenuTag, root.MenuTag) && x.IsVisible(Sce.Atf.Applications.CommandVisibility.Menu)))
            {
                MenuUtil.BuildSubMenus(command as CommandItem, root);
            }

            MenuUtil.InsertGroupSeparators(root);
        }

        private ICommandService m_commandService;
        private readonly ObservableCollection<IMenu> m_menus = new ObservableCollection<IMenu>();
    }

}
