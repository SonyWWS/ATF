//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// GUI for specifying a search on any data set wrapped within a class that implements IQueryableContext</summary>
    public abstract class SearchToolStrip : ToolStrip, ISearchUI
    {
        #region ISearchUI members
        
        /// <summary>
        /// Binds this search Control to a data set (that is wrapped in a class implementing IQueryableContext)</summary>
        /// <param name="queryableContext">The queryable context object, or null.</param>
        public void Bind(IQueryableContext queryableContext)
        {
            QueryableContext = queryableContext;
            Enabled = (queryableContext != null);
        }
        
        /// <summary>
        /// Gets actual client-defined GUI Control</summary>
        public Control Control { get { return this; } }

        /// <summary>
        /// Event raised by client when UI has graphically changed</summary>
        public abstract event EventHandler UIChanged;

        #endregion

        /// <summary>
        /// Adds instances of classes that implement IQueryPredicate to define what will be searched</summary>
        public abstract IQueryPredicate GetPredicate();

        /// <summary>
        /// Triggers the bound data IQueryableContext to perform a search, using the parameters received from search ToolStrip UI user input</summary>
        /// <returns>The results of the search</returns>
        public IEnumerable<object> DoSearch()
        {
            if (QueryableContext == null)
                return null;

            return QueryableContext.Query(GetPredicate());
        }

        /// <summary>
        /// Gets the queryable context. Can only be called after Bind().</summary>
        protected IQueryableContext QueryableContext { get; private set; }
    }
}

