//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for selection contexts that optionally can specify a 'sub-selection' context,
    /// i.e., the selection context of sub-elements, presumably related to the selection in this context.
    /// This interface serves to abstractly allow consumers of selection contexts to hook into events from sub-selections.</summary>
    public interface ISubSelectionContext : ISelectionContext
    {
        /// <summary>
        /// Gets the sub-selection context--the selection context relative to the selection of this selection context</summary>
        ISelectionContext SubSelectionContext
        {
            get;
        }
    }

    /// <summary>
    /// Useful methods for operating on ISelectionContext</summary>
    public static class SubSelectionContexts
    {
        /// <summary>
        /// Sets the sub-selection to a single item</summary>
        /// <param name="selectionContext">Sub-selection context</param>
        /// <param name="item">Item to select</param>
        public static void Set(this ISubSelectionContext selectionContext, object item)
        {
            if (item != null)
                selectionContext.SubSelectionContext.SetRange(new[] { item });
        }

        /// <summary>
        /// Clears the sub-selection</summary>
        /// <param name="selectionContext">Sub-selection context</param>
        public static void Clear(this ISubSelectionContext selectionContext)
        {
            if (selectionContext == null)
                throw new ArgumentNullException("selectionContext");

            selectionContext.SubSelectionContext.Clear();
        }
    }
}
