//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using Sce.Atf;
using Sce.Atf.Applications;

namespace SimpleDomEditorSample
{
    /// <summary>
    /// Class that implements ISelectionContext to interface with a Selection<!--object-->;
    /// The name 'SelectionContext' is already being used for a similar, but DomNode specific, class.
    /// Hence the name 'GenericSelectionContext'.</summary>
    public class GenericSelectionContext : ISelectionContext
    {
        /// <summary>
        /// Constructor</summary>
        public GenericSelectionContext()
        {
            m_selection = new Selection<object>();
            m_selection.Changed += selection_Changed;

            // suppress compiler warning
            if (SelectionChanging == null) return;
        }

        #region ISelectionContext Members

        /// <summary>
        /// Gets or sets the selected items</summary>
        public IEnumerable<object> Selection
        {
            get { return m_selection; }
            set { m_selection.SetRange(value); }
        }

        /// <summary>
        /// Returns all selected items of the given type</summary>
        /// <typeparam name="T">Desired item type</typeparam>
        /// <returns>All selected items of the given type</returns>
        public IEnumerable<T> GetSelection<T>()
                    where T : class
        {
            return m_selection.AsIEnumerable<T>();
        }

        /// <summary>
        /// Gets the last selected item</summary>
        public object LastSelected
        {
            get { return m_selection.LastSelected; }
        }

        /// <summary>
        /// Gets the last selected item of the given type; this may not be the same
        /// as the LastSelected item</summary>
        /// <typeparam name="T">Desired item type</typeparam>
        /// <returns>Last selected item of the given type</returns>
        public T GetLastSelected<T>()
                    where T : class
        {
            return m_selection.GetLastSelected<T>();
        }

        /// <summary>
        /// Returns whether the selection contains the given item</summary>
        /// <param name="item">Item</param>
        /// <returns>True iff the selection contains the given item</returns>
        public bool SelectionContains(object item)
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

        /// <summary>
        /// Callback that triggers when Selection.Set has been called. Raises SelectionChanging event.</summary>
        private void selection_Changing(object sender, EventArgs e)
        {
            SelectionChanging.Raise(this, EventArgs.Empty);
        }

        /// <summary>
        /// Callback that triggers when Selection.Set has been called. Raises SelectionChanged event.</summary>
        protected void selection_Changed(object sender, EventArgs e)
        {
            SelectionChanged.Raise(this, EventArgs.Empty);
        }

        /// <summary>
        /// Current selection</summary>
        protected Selection<object> m_selection;
    }
}
