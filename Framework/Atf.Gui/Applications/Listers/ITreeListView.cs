//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface defining a view on hierarchical data</summary>
    public interface ITreeListView
    {
        /// <summary>
        /// Gets the root level objects of the tree view</summary>
        IEnumerable<object> Roots
        {
            get;
        }

        /// <summary>
        /// Gets enumeration of the children of the given parent object</summary>
        /// <param name="parent">Parent object</param>
        /// <returns>Enumeration of the children of the parent object</returns>
        IEnumerable<object> GetChildren(object parent);

        /// <summary>
        /// Gets names for columns</summary>
        string[] ColumnNames
        {
            get;
        }
    }
}
