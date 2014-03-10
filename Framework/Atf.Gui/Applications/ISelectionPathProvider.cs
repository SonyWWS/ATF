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
        /// <param name="item"></param>
        /// <returns></returns>
        AdaptablePath<object> GetSelectionPath(object item);

        /// <summary>
        /// Removes the selection path for the item </summary>
        bool RemoveSelectionPath(object item);

        /// <summary>
        /// Update the selection path for the item </summary>
        /// <param name="item">The item to update for</param>
        /// <param name="path">The path used to update for the item</param>
        void UpdateSelectionPath(object item, AdaptablePath<object> path);

        /// <summary>
        ///  Get the selection path where the given item is part of the  path but not the last</summary>
        AdaptablePath<object> IncludedPath(object item);
 
        /// <summary>
        /// Gets the SelectionPathProviderInfo object</summary>
        SelectionPathProviderInfo Info
        {
            get;
        }
    }

    /// <summary>
    /// Useful static methods on transaction contexts</summary>
    public static class SelectionPathProviders
    {
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
        /// Gets the ancestry of the item in the selection path , 
        /// starting with the item's parent, and ending with the top level</summary>
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

    public class SelectionPathProviderInfo
    {
        public ISelectionContext SelectionContext
        {
            get; set;
        }
    }

}
