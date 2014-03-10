//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Class for specifying the root of a query tree</summary>
    public class QueryRoot : QueryNode
    {
        /// <summary>
        /// Event that is raised when search text is entered</summary>
        public event EventHandler SearchTextEntered;
        /// <summary>
        /// Event that is raised when replace text is entered</summary>
        public event EventHandler ReplaceTextEntered;
        /// <summary>
        /// Event that is raised when an QueryOption option changed</summary>
        public event EventHandler OptionChanged;

        /// <summary>
        /// Constructor</summary>
        public QueryRoot() { }

        /// <summary>
        /// Register 'search text changed' event to node</summary>
        /// <param name="queryTextInput">QueryTextInput that event is registered to</param>
        public void RegisterSearchQueryTextInput(QueryTextInput queryTextInput)
        {
            queryTextInput.TextEntered += childNode_SearchTextEntered;
            queryTextInput.TextChanged += childNode_SearchTextChanged;
        }

        /// <summary>
        /// Register 'search button clicked' event to node</summary>
        /// <param name="queryButton">QueryButton that event is registered to</param>
        public void RegisterSearchButtonPress(QueryButton queryButton)
        {
            queryButton.Clicked += childNode_SearchTextEntered;
        }

        /// <summary>
        /// Register 'replace button clicked' event to node</summary>
        /// <param name="queryButton">QueryButton that event is registered to</param>
        public void RegisterReplaceButtonPress(QueryButton queryButton)
        {
            queryButton.Clicked += childNode_ReplaceTextEntered;
        }

        /// <summary>
        /// Register 'replace text changed' event to node</summary>
        /// <param name="queryTextInput">QueryTextInput that event is registered to</param>
        public void RegisterReplaceQueryTextInput(QueryTextInput queryTextInput)
        {
            queryTextInput.TextEntered += childNode_ReplaceTextEntered;
        }

        /// <summary>
        /// Register 'option changed' event to node</summary>
        /// <param name="queryOption">QueryOption that event is registered to</param>
        public void RegisterQueryOption(QueryOption queryOption)
        {
            queryOption.OptionChanged += childNode_OptionChanged;
        }

        private void childNode_SearchTextEntered(object sender, EventArgs args)
        {
            SearchTextEntered.Raise(sender, EventArgs.Empty);
            QueryDirty = false;
        }

        void childNode_SearchTextChanged(object sender, EventArgs e)
        {
            QueryDirty = true;
        }

        private void childNode_ReplaceTextEntered(object sender, EventArgs args)
        {
            if (QueryDirty)
            {
                SearchTextEntered.Raise(sender, EventArgs.Empty);
                QueryDirty = false;
            }
            ReplaceTextEntered.Raise(sender, EventArgs.Empty);
        }

        private void childNode_OptionChanged(object sender, EventArgs args)
        {
            OptionChanged.Raise(sender, EventArgs.Empty);
        }

        /// <summary>
        /// Adds instances of classes that implement IQueryPredicate to define what will be searched</summary>
        public virtual IQueryPredicate GetPredicate()
        {
            return null;
        }

        internal bool QueryDirty { get; set; }
    }
}
