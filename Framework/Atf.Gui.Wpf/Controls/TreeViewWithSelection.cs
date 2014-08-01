//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Tree view that adds the capabilities of ISelectionContext</summary>
    public class TreeViewWithSelection : ITreeView, IItemView, ISelectionContext
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="root">Root of the tree view. Should implement IItemView, too.</param>
        public TreeViewWithSelection(ITreeView root)
        {
            m_treeView = root;
            m_itemView = root as IItemView;
            m_selection = new AdaptableSelection<object>();
            m_selection.Changed += (s, e) => SelectionChanged.Raise(this, e);
            m_selection.Changing += (s, e) => SelectionChanging.Raise(this, e);
        }

        #region ITreeView Members

        /// <summary>
        /// Gets the root of the tree</summary>
        public object Root { get { return m_treeView.Root; } }

        /// <summary>
        /// Gets a list of the children of the specified node in the tree</summary>
        /// <param name="parent">Parent node to query for children</param>
        /// <returns>Children of the specified parent</returns>
        public virtual IEnumerable<object> GetChildren(object parent)
        {
            return m_treeView.GetChildren(parent);
        }

        #endregion

        #region IItemView Members

        /// <summary>
        /// Gets item's display information</summary>
        /// <param name="item">Item being displayed</param>
        /// <param name="info">Item info, to fill out</param>
        public void GetInfo(object item, ItemInfo info)
        {
            if (m_itemView != null)
                m_itemView.GetInfo(item, info);
        }

        #endregion

        #region ISelectionContext Members

        /// <summary>
        /// Gets or sets the enumeration of selected items</summary>
        public IEnumerable<object> Selection
        {
            get { return m_selection; }
            set { m_selection.SetRange(value); }
        }

        /// <summary>
        /// Gets the last selected item</summary>
        public object LastSelected
        {
            get { return m_selection.LastSelected; }
        }

        /// <summary>
        /// Gets all selected items of the given type.</summary>
        /// <typeparam name="T">Desired item type</typeparam>
        /// <returns>all selected items of the given type</returns>
        public IEnumerable<T> GetSelection<T>() where T : class
        {
            return m_selection.AsIEnumerable<T>();
        }

        /// <summary>
        /// Gets the last selected item of the given type; this may not be the same
        /// as the LastSelected item</summary>
        /// <typeparam name="T">Desired item type</typeparam>
        /// <returns>last selected item of the given type</returns>
        public T GetLastSelected<T>() where T : class
        {
            return m_selection.GetLastSelected<T>();
        }

        /// <summary>
        /// Returns a value indicating if the selection contains the given item</summary>
        /// <param name="item">Item</param>
        /// <returns>true, iff the selection contains the given item</returns>
        /// <remarks>Override to customize how items are compared for equality, eg. for
        /// tree views, the selection might be adaptable paths, in which case the override
        /// could compare the item to the last elements of the selected paths.</remarks>
        public virtual bool SelectionContains(object item)
        {
            return m_selection.Contains(item);
        }

        /// <summary>
        /// Gets the number of items in the current selection</summary>
        public int SelectionCount
        {
            get { return m_selection.Count; }
        }

        /// <summary>
        /// Event that is raised before the selection changes</summary>
        public event EventHandler SelectionChanging;

        /// <summary>
        /// Event that is raised after the selection changes</summary>
        public event EventHandler SelectionChanged;

        #endregion

        private readonly IItemView m_itemView;
        private readonly ITreeView m_treeView;
        private readonly AdaptableSelection<object> m_selection;
    }
}
