//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace WpfPropertyEditor
{
    class ObjectSelectionContext: ISelectionContext
    {
           /// <summary>
        /// Constructor</summary>
        public ObjectSelectionContext()
        {
            m_selection = new AdaptableSelection<object>();

            m_selection.Changing += selection_Changing;
            m_selection.Changed += selection_Changed;
        }

        /// <summary>
        /// Gets the context's selection</summary>
        public AdaptableSelection<object> Selection
        {
            get { return m_selection; }
        }

        #region ISelectionContext Members

        /// <summary>
        /// Gets or sets the selected items</summary>
        IEnumerable<object> ISelectionContext.Selection
        {
            get { return m_selection; }
            set { m_selection.SetRange(value); }
        }

        /// <summary>
        /// Gets all selected items of the given type</summary>
        /// <typeparam name="T">Desired item type</typeparam>
        /// <returns>All selected items of the given type</returns>
        public IEnumerable<T> GetSelection<T>() where T : class
        {
            return m_selection.AsIEnumerable<T>();
        }

        /// <summary>
        /// Gets the last selected item as object</summary>
        public object LastSelected
        {
            get { return m_selection.LastSelected; }
        }

        /// <summary>
        /// Gets the last selected item of the given type, which may not be the same
        /// as the LastSelected item</summary>
        /// <typeparam name="T">Desired item type</typeparam>
        /// <returns>Last selected item of the given type</returns>
        public T GetLastSelected<T>() where T : class
        {
            return m_selection.GetLastSelected<T>();
        }

        /// <summary>
        /// Returns whether the selection contains the given item</summary>
        /// <param name="item">Item</param>
        /// <returns><c>True</c> if the selection contains the given item</returns>
        public bool SelectionContains(object item)
        {
            return m_selection.Contains(item);
        }

        /// <summary>
        /// Gets the number of items in the current selection</summary>
        int ISelectionContext.SelectionCount
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


        private void selection_Changing(object sender, EventArgs e)
        {
            SelectionChanging.Raise(this, e);
        }

        private void selection_Changed(object sender, EventArgs e)
        {
            SelectionChanged.Raise(this, e);
        }

        private readonly AdaptableSelection<object> m_selection;

    }
}
