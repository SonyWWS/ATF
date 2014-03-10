//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models
{
    internal class Menu : MenuItemBase, IMenu
    {
        public Menu(IMenu parent, object menuTag, object groupTag, string text, string description)
            : base(menuTag, groupTag, text, description)
        {
            m_parent = parent;
        }

        #region IMenu

        public IEnumerable<IMenuItem> Children 
        { 
            get 
            {
                OnChildrenRequested();
                return m_children; 
            } 
        }

        public IEnumerable<ICommandItem> CommandsInSubtree
        {
            get 
            {
                OnChildrenRequested();

                foreach (var child in m_children)
                {
                    if (child is ICommandItem)
                    {
                        yield return (ICommandItem)child;
                    }
                    else if (child is IMenu)
                    {
                        foreach (var cmd in ((IMenu)child).CommandsInSubtree)
                            yield return cmd;
                    }
                }
            }
        }

        public IMenu Parent { get { return m_parent; } }

        #endregion

        /// <summary>
        /// Not for binding!</summary>
        public ObservableCollection<IMenuItem> ChildCollection { get { return m_children; } }

        protected virtual void OnChildrenRequested() 
        { 
        }

        private ObservableCollection<IMenuItem> m_children = new ObservableCollection<IMenuItem>();
        private readonly IMenu m_parent;

    }

    internal class RootMenu : Menu
    {
        public RootMenu(MenuDef def)
            : base(null, def.MenuTag, null, def.Text, def.Description)
        {
        }

        public void Invalidate()
        {
            m_requiresRefresh = true;
            OnPropertyChanged(s_childrenArgs);
            OnPropertyChanged(s_commandsInSubtreeArgs);
        }

        public event EventHandler RefreshRequested;

        protected override void OnChildrenRequested()
        {
            if (m_requiresRefresh)
            {
                m_requiresRefresh = false;
                RefreshRequested.Raise(this, EventArgs.Empty);
            }
        }

        private bool m_requiresRefresh = true;
        private static readonly PropertyChangedEventArgs s_commandsInSubtreeArgs
            = ObservableUtil.CreateArgs<RootMenu>(x => x.CommandsInSubtree);
        private static readonly PropertyChangedEventArgs s_childrenArgs
            = ObservableUtil.CreateArgs<Menu>(x => x.Children);
    }

    internal class Separator : MenuItemBase
    {
        public Separator()
            : base(null,null,null,null)
        {
        }
    }
}
