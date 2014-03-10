using System;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// ListView GUI for displaying the results of a DomNode search</summary>
	public abstract class SearchResultsListView : ListView, IResultsUI
	{
        /// <summary>
        /// Constructor</summary>
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
        /// Constructor</summary>
        /// <param name="queryResultContext">Query results context</param>
        /// <param name="contextRegistry">Context registry</param>
		public SearchResultsListView(IQueryableResultContext queryResultContext, IContextRegistry contextRegistry)
			: this(contextRegistry)
		{
			Bind(queryResultContext);
		}

		#region IResultsControl members
		/// <summary>
		/// Binds this SearchControl to a data set (that is wrapped in a class implementing IQueryableContext)</summary>
		public void Bind(IQueryableResultContext queryResultContext)
		{
			if (m_queryResultContext != null)
				m_queryResultContext.ResultsChanged -= queryResultContext_ResultsChanged;

			m_queryResultContext = queryResultContext;

			if (m_queryResultContext != null)
				m_queryResultContext.ResultsChanged += queryResultContext_ResultsChanged;
		}

        /// <summary>
        /// Gets or sets the control</summary>
		public Control Control { get { return this; } }

        /// <summary>
        /// Event that is raised when the UI changes</summary>
		public abstract event EventHandler UIChanged;
		#endregion

		/// <summary>
		/// Callback called when user has selected a new search result item</summary>
		/// <param name="e">Event arguments</param>
		protected override void OnItemSelectionChanged(ListViewItemSelectionChangedEventArgs e)
		{
			object tag = e.Item.Tag;
			if (m_contextRegistry != null)
			{
				ISelectionContext selectionContext = Adapters.As<ISelectionContext>(m_contextRegistry.ActiveContext);
				if (selectionContext != null)
					SelectionContexts.Set(selectionContext, tag);
			}
			base.OnItemSelectionChanged(e);
		}

		/// <summary>
		/// Callback called when the query result context has updated its results. Rebuilds the ListView display from it.</summary>
		/// <param name="sender">Sender of the event</param>
		/// <param name="e">Event arguments</param>
		private void queryResultContext_ResultsChanged(object sender, System.EventArgs e)
		{
			UpdateResults();
		}

        /// <summary>
        /// Update UI with search results</summary>
		protected abstract void UpdateResults();

        /// <summary>
        /// Gets the query results context</summary>
        /// <exception cref="InvalidOperationException">If search results UI isn't bound to a data set</exception>
		protected IQueryableResultContext QueryResultContext
		{
			get
			{
				if (m_queryResultContext == null)
					throw new InvalidOperationException("Search Results UI isn't bound to a data set.");
				return m_queryResultContext;
			}
		}

		private IQueryableResultContext m_queryResultContext = null;
		private IContextRegistry m_contextRegistry;
	}
}
