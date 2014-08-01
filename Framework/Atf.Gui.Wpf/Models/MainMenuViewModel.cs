//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// View model for an application's menus</summary>
    [ExportViewModel(Contracts.MainMenuViewModel)]
    public class MainMenuViewModel : NotifyPropertyChangedBase
    {
        /// <summary>
        /// View models which are bound to in xaml
        /// </summary>
        public IEnumerable<IMenuModel> Menus
        {
            get
            {
                if (m_requiresRefresh)
                {
                    m_requiresRefresh = false;
                    m_rootMenuModels = new ObservableCollection<IMenuModel>();

                    if (m_menuDefinitions != null
                        && m_menuDefinitions.Length > 0
                        && m_menuItems != null
                        && m_menuItems.Length > 0)
                    {

                        foreach (IMenu menuDef in m_menuDefinitions)
                        {
                            m_rootMenuModels.Add(new RootMenuModel(menuDef));
                        }

                        var menuItems = new List<IMenuItem>(m_menuItems);
                        menuItems.Sort(new CommandComparer());

                        foreach (IMenuItem menuItem in menuItems.Where(x => x.IsVisible))
                        {
                            var rootMenuModel = m_rootMenuModels.FirstOrDefault(
                                x => CommandComparer.TagsEqual(((RootMenuModel)x).MenuTag, menuItem.MenuTag)) as RootMenuModel;

                            if (rootMenuModel != null)
                                rootMenuModel.AddItem(menuItem);
                        }

                        // Must access ListCollectionView on UI thread
                        Application.Current.Dispatcher.InvokeIfRequired(delegate
                        {
                            var cvs = (ListCollectionView)CollectionViewSource.GetDefaultView(m_rootMenuModels);
                            cvs.CustomSort = RootMenuComparer.Instance;
                        });
                    }
                }
                return m_rootMenuModels;
            }
        }
        
        private ObservableCollection<IMenuModel> m_rootMenuModels;

        /// <summary>
        /// Imported root menu definitions
        /// </summary>
        [ImportMany(AllowRecomposition = true)]
        private IMenu[] MenuDefinitions
        {
            set
            {
                m_menuDefinitions = value;
                m_requiresRefresh = true;
                OnPropertyChanged(new PropertyChangedEventArgs("Menus"));
            }
        }
        private IMenu[] m_menuDefinitions;

        /// <summary>
        /// Imported menu items to place in menus
        /// </summary>
        [ImportMany(AllowRecomposition = true)]
        private IMenuItem[] MenuItems
        {
            set
            {
                m_menuItems = value;
                m_requiresRefresh = true;
                OnPropertyChanged(new PropertyChangedEventArgs("Menus"));
            }
        }
        private IMenuItem[] m_menuItems;

        private class RootMenuComparer : IComparer
        {

            public static RootMenuComparer Instance
            {
                get
                {
                    if (s_instance == null)
                        s_instance = new RootMenuComparer();
                    return s_instance;
                }
            }
            private static RootMenuComparer s_instance;

            #region IComparer Members

            public int Compare(object x, object y)
            {
                var menuX = x as RootMenuModel;
                var menuY = y as RootMenuModel;
                if (menuX != null && menuY != null)
                {
                    return CommandComparer.CompareTags(menuX.MenuTag, menuY.MenuTag);
                }
                return 0;
            }

            #endregion
        }

        private bool m_requiresRefresh;
    }

    /// <summary>
    /// Interface for menu view model
    /// This is the interface bound to in XAML
    /// This is exposed publicly so that users can restyle the application menus
    /// using styles and data templates
    /// </summary>
    public interface IMenuModel
    {
        string Text { get; }
        
        string Description { get; }

        ObservableCollection<object> Children { get; }

        bool IsVisible { get; }
    }

    /// <summary>
    /// Menu View Model. Used for sub menus and as a base class for root menus.
    /// </summary>
    public class MenuModel : NotifyPropertyChangedBase, IMenuModel
    {
        public MenuModel(MenuModel parent, string text, string description)
        {
            m_parent = parent;
            m_text = text;
            m_description = description;
        }

        #region IMenuModel Members

        public string Text
        {
            get { return m_text; }
        }

        public string Description
        {
            get { return m_description; }
        }

        public ObservableCollection<object> Children
        {
            get { return GetChildren(); }
        }
                
        public bool IsVisible
        {
            get { return GetSubtree().OfType<ICommandItem>().Any(); }
        }

        #endregion

        public MenuModel Parent { get { return m_parent; } }

        protected IEnumerable<object> GetSubtree()
        {
            foreach (object child in Children)
            {
                yield return child;

                var menuModel = child as MenuModel;
                if(menuModel != null)
                {
                    foreach (object subChild in menuModel.GetSubtree())
                        yield return subChild;
                }
            }
        }

        protected virtual ObservableCollection<object> GetChildren()
        {
            return m_children;
        }

        private readonly string m_text;
        private readonly string m_description;
        private readonly ObservableCollection<object> m_children = new ObservableCollection<object>();
        private readonly MenuModel m_parent;

    }

    /// <summary>
    /// View model for a root menu
    /// </summary>
    internal class RootMenuModel : MenuModel
    {
        public RootMenuModel(IMenu def)
            : this(def.MenuTag, def.Text, def.Description)
        {
        }

        public RootMenuModel(object menuTag, string text, string description)
            : base(null, text, description)
        {
            MenuTag = menuTag;
        }

        public object MenuTag { get; private set; }

        public void AddItem(IMenuItem item)
        {
            m_items.Add(item);
            Invalidate();
        }

        public void RemoveItem(IMenuItem item)
        {
            if(m_items.Remove(item))
                Invalidate();
        }

        protected override ObservableCollection<object> GetChildren()
        {
            if (m_requiresRefresh)
            {
                m_requiresRefresh = false;
                Rebuild();
            }
            return base.GetChildren();
        }

        private void Invalidate()
        {
            m_requiresRefresh = true;
            OnPropertyChanged(s_childrenArgs);
            OnPropertyChanged(s_isVisibleArgs);
        }

        private void Rebuild()
        {
            var children = base.GetChildren();
            if (children.Count > 0)
                children.Clear();

            if (m_items.Count > 0)
            {
                foreach (var menuItem in m_items)
                    BuildSubMenus(menuItem);

                InsertGroupSeparators();
            }
        }

        private void BuildSubMenus(IMenuItem menuItem)
        {
            var children = base.GetChildren();
            MenuModel menu = this;

            foreach (string segment in menuItem.MenuPath)
            {
                // Try and find an existing submenu
                MenuModel subMenu = (MenuModel)children.FirstOrDefault(x => (x is MenuModel) && (((MenuModel)x).Text == segment));
                if (subMenu == null)
                {
                    // No existing submenu found - add a new one
                    subMenu = new MenuModel(menu, segment, segment);
                    children.Add(subMenu);
                }
                children = subMenu.Children;
                menu = subMenu;
            }

            children.Add(menuItem);
        }

        private void InsertGroupSeparators()
        {
            // Create depth first list of commands
            var commands = new List<Tuple<ICommandItem, MenuModel>>();
            GetCommandsInSubtree(this, commands);

            for (int i = 1; i < commands.Count; i++)
            {
                var previous = commands[i - 1];
                var current = commands[i];

                if (!CommandComparer.TagsEqual(previous.Item1.GroupTag, current.Item1.GroupTag))
                {
                    InsertSeparator(previous, current);
                }
            }
        }

        private static void GetCommandsInSubtree(MenuModel menuModel, IList<Tuple<ICommandItem, MenuModel>> commands)
        {
            foreach (object child in menuModel.Children)
            {
                if (child is ICommandItem)
                {
                    commands.Add(new Tuple<ICommandItem, MenuModel>((ICommandItem)child, menuModel));
                }
                else if (child is MenuModel)
                {
                    GetCommandsInSubtree((MenuModel)child, commands);
                }
            }
        }

        private static void InsertSeparator(Tuple<ICommandItem, MenuModel> previous, Tuple<ICommandItem, MenuModel> current)
        {
            // Get lineage of each
            MenuModel[] previousLineage = GetLineage(previous.Item2).Reverse().ToArray();
            MenuModel[] currentLineage = GetLineage(current.Item2).Reverse().ToArray();

            // Find lowest common ancestor
            // (Assumes common ancestor exists)
            int minLength = Math.Min(previousLineage.Length, currentLineage.Length);

            object insertBefore = null;
            IList<object> collection = null;
        
            for (int i = 0; i < minLength; i++)
            {
                if (previousLineage[i] != currentLineage[i])
                {
                    insertBefore = currentLineage[i];
                    collection = currentLineage[i - 1].Children;
                }
            }

            if (insertBefore == null)
            {
                if(minLength < currentLineage.Length)
                    insertBefore = currentLineage[minLength];
                else
                    insertBefore = current.Item1;

                collection = currentLineage[minLength - 1].Children;
            }

            int idx = collection.IndexOf(insertBefore);
            System.Diagnostics.Debug.Assert(idx >= 0);
            collection.Insert(idx, new Separator());
        }

        private static IEnumerable<MenuModel> GetLineage(MenuModel menuModel)
        {
            while(menuModel != null)
            {
                yield return menuModel;
                menuModel = menuModel.Parent;
            }
        }

        private readonly List<IMenuItem> m_items = new List<IMenuItem>();
        private bool m_requiresRefresh = true;

        private static readonly PropertyChangedEventArgs s_isVisibleArgs
            = ObservableUtil.CreateArgs<RootMenuModel>(x => x.IsVisible);
        private static readonly PropertyChangedEventArgs s_childrenArgs
            = ObservableUtil.CreateArgs<RootMenuModel>(x => x.Children);
    }

    /// <summary>
    /// View model for a menu separator
    /// </summary>
    internal class Separator : NotifyPropertyChangedBase
    {
    }
}
