//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications
{

    /// <summary>
    /// Interface for component that provides selection path on a selected item</summary>
    public interface ISelectionPathProvider
    {
        /// <summary>
        /// Get the selection path for the given item</summary>
        /// <param name="item">Item to get selection path for</param>
        /// <returns>Selection path for item</returns>
        AdaptablePath<object> GetSelectionPath(object item);

        /// <summary>
        /// Remove the selection path for the item</summary>
        /// <param name="item">Item to remove selection path on</param>
        /// <returns>True iff selection path removed</returns>
        bool RemoveSelectionPath(object item);

        /// <summary>
        /// Update the selection path for the item</summary>
        /// <param name="item">Item to update for</param>
        /// <param name="path">Path used to update for the item</param>
        void UpdateSelectionPath(object item, AdaptablePath<object> path);

        /// <summary>
        /// Obtain the selection path where the given item is part of the path but not the last</summary>
        /// <param name="item">Part of path, not last part</param>
        /// <returns>Selection path containing given item</returns>
        AdaptablePath<object> IncludedPath(object item);
 
        /// <summary>
        /// Get the SelectionPathProviderInfo object</summary>
        SelectionPathProviderInfo Info
        {
            get;
        }
    }

    /// <summary>
    /// Useful static methods on transaction contexts</summary>
    public static class SelectionPathProviders
    {
        /// <summary>
        /// Obtains the parent in the selection path for given item</summary>
        /// <param name="selectionPathProvider">Selection path provider</param>
        /// <param name="item">Item whose parent path is obtained</param>
        /// <returns>Parent path of given item</returns>
        public static object Parent(this ISelectionPathProvider selectionPathProvider, object item)
        {
            if (selectionPathProvider != null)
            {
                var selectionPath = selectionPathProvider.GetSelectionPath(item);
                if ((selectionPath != null) && selectionPath.Count > 1)
                    return selectionPath[selectionPath.Count - 2];
            }
            return null;
        }

        /// <summary>
        /// Gets the ancestry of the item in selection path, 
        /// starting with the item's parent and ending with the top level</summary>
        /// <param name="selectionPathProvider">Selection path provider</param>
        /// <param name="item">Item whose ancestry is obtained</param>
        /// <returns>Ancestry of the item in selection path</returns>
        public static IEnumerable<object> Ancestry(this ISelectionPathProvider selectionPathProvider, object item)
        {
            if (selectionPathProvider != null)
            {
                var selectionPath = selectionPathProvider.GetSelectionPath(item);
                if ((selectionPath != null) && selectionPath.Count > 1)
                {
                    for(int i= selectionPath.Count - 2; i>=0; --i)
                     yield return selectionPath[i];
                }                 
            }
        }
    }

    /// <summary>
    /// Selection path provider information</summary>
    public class SelectionPathProviderInfo
    {
        /// <summary>
        /// Gets or sets selection context</summary>
        public ISelectionContext SelectionContext
        {
            get; set;
        }
    }

}
