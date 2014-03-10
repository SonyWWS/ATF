//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Abstracts an enumeration of objects that can be used as tags, one per row, in a
    /// list control along with corresponding user-readable column names at the top.
    /// See ListViewAdapter.</summary>
    public interface IListView
    {
        /// <summary>
        /// Gets names for table columns</summary>
        string[] ColumnNames
        {
            get;
        }

        /// <summary>
        /// Gets the items in the list</summary>
        IEnumerable<object> Items
        {
            get;
        }
    }
}
