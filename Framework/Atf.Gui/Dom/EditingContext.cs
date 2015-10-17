//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// An editing context, which is a history context with a selection, providing a basic
    /// self-contained editing context. There may be multiple EditingContexts in a
    /// document</summary>
    public class EditingContext : HistoryContext, ISelectionContext
    {
        /// <summary>
        /// Constructor</summary>
        public EditingContext()
        {
            m_selection = new AdaptableSelection<object>();
            m_selection.Changing += selection_Changing;
            m_selection.Changed += selection_Changed;
        }

        /// <summary>
        /// Gets the context's selection as a Selection object</summary>
        public Selection<object> Selection
        {
            get { return m_selection; }
        }

        /// <summary>
        /// Gets or sets whether or not selection changes are recorded and can be undone and redone</summary>
        public bool RecordSelectionChanges { get; set; }

        #region ISelectionContext Members

        /// <summary>
        /// Gets or sets the enumeration of selected items</summary>
        IEnumerable<object> ISelectionContext.Selection
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
        /// Obtains all selected items of the given type</summary>
        /// <typeparam name="T">Desired item type</typeparam>
        /// <returns>All selected items of the given type</returns>
        public IEnumerable<T> GetSelection<T>() where T : class
        {
            return m_selection.AsIEnumerable<T>();
        }

        /// <summary>
        /// Obtains the last selected item of the given type; this may not be the same
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
        /// <remarks>Override to customize how items are compared for equality, e.g., for
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

        private void selection_Changing(object sender, EventArgs e)
        {
            SelectionChanging.Raise(this, e);

            if (RecordSelectionChanges && Recording && !InTransaction && !UndoingOrRedoing)
                m_prevSelection = m_selection.ToArray();
        }

        private void selection_Changed(object sender, EventArgs e)
        {
            if (RecordSelectionChanges && Recording && !InTransaction && !UndoingOrRedoing)
                History.Add(new SetSelectionCommand(this, m_prevSelection, m_selection.ToArray()));
            SelectionChanged.Raise(this, e);
        }

        private readonly AdaptableSelection<object> m_selection;
        private IEnumerable<object> m_prevSelection;
    }
}
