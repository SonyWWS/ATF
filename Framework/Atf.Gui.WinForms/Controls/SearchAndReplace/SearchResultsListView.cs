using System;
using System.Windows.Forms;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// ListView GUI for displaying the results of a DomNode search</summary>
    public abstract class SearchResultsListView : ListView, IResultsUI
    {
        /// <summary>
        /// Constructor with IContextRegistry</summary>
        /// <param name="contextRegistry">Context registry</param>
        public SearchResultsListView(IContextRegistry contextRegistry)
        {
            // Configure ListView
            View = View.Details;
            LabelEdit = false;
            AllowColumnReorder = false;
            CheckBoxes = false;
            FullRowSelect = true;
            MultiSelect = false;
            GridLines = true;
            Sorting = SortOrder.Ascending;

            m_contextRegistry = contextRegistry;
        }

        /// <summary>
        /// Constructor with IQueryableResultContext and IContextRegistry</summary>
        /// <param name="queryResultContext">Query result context</param>
        /// <param name="contextRegistry">Context registry</param>
        public SearchResultsListView(IQueryableResultContext queryResultContext, IContextRegistry contextRegistry)
            : this(contextRegistry)
        {
            Bind(queryResultContext);
        }

        #region IResultsControl members
        /// <summary>
        /// Binds this search Control to a data set (that is wrapped in a class implementing IQueryableContext)</summary>
        public void Bind(IQueryableResultContext queryResultContext)
        {
            ClearResults();

            if (m_queryResultContext != null)
                m_queryResultContext.ResultsChanged -= queryResultContext_ResultsChanged;

            m_queryResultContext = queryResultContext;

            if (m_queryResultContext != null)
                m_queryResultContext.ResultsChanged += queryResultContext_ResultsChanged;
        }

        /// <summary>
        /// Gets actual client-defined GUI Control</summary>
        public Control Control { get { return this; } }

        /// <summary>
        /// Event raised by client when UI has graphically changed</summary>
        public abstract event EventHandler UIChanged;
        #endregion

        /// <summary>
        /// Performs custom actions on MouseDown events</summary>
        /// <param name="e">Mouse event args</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            // If there is an ISelectionContext implemented for the type of our search results,
            // and the mouse click was on one of the search result list items,
            // then have that ISelectionContext select the instance associated with the clicked search result.
            // Additionally, if the type of our search results implements an ISubSelectionContext, have 
            // that ISubSelectionContext select the instance associated with the subitem of the clicked search results
            // 
             // (For instance, when each search result ListItem is a DomNode, and contains a SubItem for every DomNode 
            // property that matched, this would allow both setting ISelectionContext to the DomNode associated with
            // the clicked ListItem, and setting ISubSelectionContext to the property associated with the clicked SubItem
            // in the row)
            if (m_queryResultContext != null)
            {
                // Is the query result type associated with m_queryResultContext have a way of specifying a selection was made?
                ISelectionContext selectionContext = m_queryResultContext.As<ISelectionContext>();
                if (selectionContext != null)
                {
                    // We can specify a selection was made.  Now check if a selection was actually made.
                    ListViewHitTestInfo hitTestInfo = HitTest(e.Location);
                    if (hitTestInfo != null && hitTestInfo.Item != null)
                    {
                        // A selection was made in the search results.  Specify this selection through ISelectionContext.
                        object tag = hitTestInfo.Item.Tag;
                        selectionContext.Set(tag);

                        // Does the query result type also have a way to specify a sub-selection?
                        ISubSelectionContext subSelectionContext = m_queryResultContext.As<ISubSelectionContext>();
                        if (subSelectionContext != null)
                        {
                            object selectedTag = null;
                            if (hitTestInfo.SubItem != null && hitTestInfo.SubItem.Tag != null)
                                selectedTag = hitTestInfo.SubItem.Tag; // Specific sub-item clicked, use it
                            else
                            {
                                // No sub-item clicked, select first sub-item if the selected item has any
                                foreach (var obj in hitTestInfo.Item.SubItems)
                                {
                                    var subItem = obj as ListViewItem.ListViewSubItem;
                                    if (subItem != null && subItem.Tag != null)
                                    {
                                        selectedTag = subItem.Tag;
                                        break;
                                    }
                                }
                            }

                            if (selectedTag != null)
                                subSelectionContext.Set(selectedTag);
                            else
                                subSelectionContext.Clear();
                        }
                    }
                    else // no selection
                        selectionContext.Clear();
                }
            }
        }

        /// <summary>
        /// Callback called when the query result context has updated its results. Rebuilds the ListView display from results.</summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        private void queryResultContext_ResultsChanged(object sender, System.EventArgs e)
        {
            UpdateResults();
        }

        /// <summary>
        /// Clears the previous result list</summary>
        protected abstract void ClearResults();

        /// <summary>
        /// Updates the result list</summary>
        protected abstract void UpdateResults();

        /// <summary>
        /// Gets the query result context</summary>
        protected IQueryableResultContext QueryResultContext
        {
            get
            {
                if (m_queryResultContext == null)
                    throw new InvalidOperationException("Search Results UI isn't bound to a data set.");
                return m_queryResultContext;
            }
        }

        private IQueryableResultContext m_queryResultContext;
        private IContextRegistry m_contextRegistry;
    }
}
