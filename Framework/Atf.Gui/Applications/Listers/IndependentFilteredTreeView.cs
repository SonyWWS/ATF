//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Filtered tree view with an independent ISelectionContext from the wrapped TreeView </summary>
    public class IndependentFilteredTreeView : BasicFilteredTreeView, ISelectionContext
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="treeView">Data context of ITreeView to apply a filter</param>
        /// <param name="filterFunc">Callback to determine if an item in the tree is filtered in (return true) or out</param>
        public IndependentFilteredTreeView(ITreeView treeView, Predicate<object> filterFunc)
            : base(treeView, filterFunc)
        {
            m_selection = new AdaptableSelection<object>();
            m_selection.Changing += (s, e) => SelectionChanging.Raise(this, e);
            m_selection.Changed += (s, e) => SelectionChanged.Raise(this, e); ;
        }

        /// <summary>
        /// Gets tree root</summary>
        public override object Root
        {
            get { return m_treeView.Root; }
        }

        /// <summary>
        /// Gets an adapter of the specified type or null</summary>
        /// <param name="type">Adapter type</param>
        /// <returns>Adapter of the specified type or null</returns>
        public override object GetAdapter(Type type)
        {
            if (typeof(IndependentFilteredTreeView).IsAssignableFrom(type))
                return this;
            if (type == typeof(IItemView))
                return InnerTreeView;

            return null; // Don't allow adaptation to anything else e.g. IObservable, ITransaction etc
        }

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

        private readonly AdaptableSelection<object> m_selection;
    }
}
