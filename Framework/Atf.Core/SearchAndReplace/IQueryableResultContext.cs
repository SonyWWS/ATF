//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf
{
    /// <summary>
    /// Interface for accessing the results of a query and being notified when those results change</summary>
    public interface IQueryableResultContext
    {
        /// <summary>
        /// Event sent whenever the search results have been updated</summary>
        event EventHandler ResultsChanged;

        /// <summary>
        /// Gets the list of found objects on the most recent Query() execution</summary>
        IEnumerable<object> Results
        {
            get;
        }
    }
}
