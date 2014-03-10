//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for selection contexts</summary>
    public interface ISelectionContext
    {
        /// <summary>
        /// Gets or sets the enumeration of selected items</summary>
        IEnumerable<object> Selection
        {
            get;
            set;
        }

        /// <summary>
        /// Returns all selected items of the given type</summary>
        /// <typeparam name="T">Desired item type</typeparam>
        /// <returns>All selected items of the given type</returns>
        IEnumerable<T> GetSelection<T>()
            where T : class;

        /// <summary>
        /// Gets the last selected item</summary>
        object LastSelected
        {
            get;
        }

        /// <summary>
        /// Gets the last selected item of the given type; this may not be the same
        /// as the LastSelected item</summary>
        /// <typeparam name="T">Desired item type</typeparam>
        /// <returns>Last selected item of the given type</returns>
        T GetLastSelected<T>()
            where T : class;

        /// <summary>
        /// Returns whether the selection contains the given item</summary>
        /// <param name="item">Item</param>
        /// <returns>True iff the selection contains the given item</returns>
        /// <remarks>Override to customize how items are compared for equality, e.g., for
        /// tree views, the selection might be adaptable paths, in which case the override
        /// could compare the item to the last elements of the selected paths.</remarks>
        bool SelectionContains(object item);

        /// <summary>
        /// Gets the number of items in the current selection</summary>
        int SelectionCount
        {
            get;
        }

        /// <summary>
        /// Event that is raised before the selection changes</summary>
        event EventHandler SelectionChanging;

        /// <summary>
        /// Event that is raised after the selection changes</summary>
        event EventHandler SelectionChanged;
    }

    /// <summary>
    /// Useful methods for operating on ISelectionContext</summary>
    public static class SelectionContexts
    {
        /// <summary>
        /// Clears the selection</summary>
        /// <param name="selectionContext">Selection context</param>
        public static void Clear(this ISelectionContext selectionContext)
        {
            if (selectionContext == null)
                throw new ArgumentNullException("selectionContext");

            selectionContext.Selection = EmptyEnumerable<object>.Instance;
        }

        /// <summary>
        /// Sets the selection to a single item</summary>
        /// <param name="selectionContext">Selection context</param>
        /// <param name="item">Item to select</param>
        public static void Set(this ISelectionContext selectionContext, object item)
        {
            SetRange(selectionContext, new[] { item });
        }

        /// <summary>
        /// Sets the selection to multiple objects</summary>
        /// <param name="selectionContext">Selection context</param>
        /// <param name="items">Objects to select</param>
        public static void SetRange(this ISelectionContext selectionContext, IEnumerable<object> items)
        {
            if (selectionContext == null)
                throw new ArgumentNullException("selectionContext");
            if (items == null)
                throw new ArgumentNullException("items");

            selectionContext.Selection = items;
        }

        /// <summary>
        /// Sets the selection to multiple items</summary>
        /// <param name="selectionContext">Selection context</param>
        /// <param name="items">Items to select</param>
        public static void SetRange(this ISelectionContext selectionContext, IEnumerable items)
        {
            if (selectionContext == null)
                throw new ArgumentNullException("selectionContext");
            if (items == null)
                throw new ArgumentNullException("items");

            selectionContext.Selection = items.Cast<object>();
        }

        /// <summary>
        /// Adds an item to the selection</summary>
        /// <param name="selectionContext">Selection context</param>
        /// <param name="item">Item to add</param>
        public static void Add(this ISelectionContext selectionContext, object item)
        {
            AddRange(selectionContext, (IEnumerable)new[] { item });
        }

        /// <summary>
        /// Adds multiple items to the selection</summary>
        /// <param name="selectionContext">Selection context</param>
        /// <param name="items">Items to add</param>
        public static void AddRange(this ISelectionContext selectionContext, IEnumerable<object> items)
        {
            AddRange(selectionContext, (IEnumerable)items);
        }

        /// <summary>
        /// Adds multiple items to the selection</summary>
        /// <param name="selectionContext">Selection context</param>
        /// <param name="items">Items to add</param>
        public static void AddRange(this ISelectionContext selectionContext, IEnumerable items)
        {
            if (selectionContext == null)
                throw new ArgumentNullException("selectionContext");
            if (items == null)
                throw new ArgumentNullException("items");

            var newSelection = new List<object>();

            // get items in selection that aren't being added
            var itemSet = new HashSet<object>(items.Cast<object>());
            foreach (object item in selectionContext.Selection)
            {
                if (!itemSet.Contains(item))
                    newSelection.Add(item);
            }

            // append the added items
            newSelection.AddRange(items.Cast<object>());

            selectionContext.Selection = newSelection;
        }

        /// <summary>
        /// Removes an item from the selection</summary>
        /// <param name="selectionContext">Selection context</param>
        /// <param name="item">Item to remove</param>
        public static void Remove(this ISelectionContext selectionContext, object item)
        {
            RemoveRange(selectionContext, (IEnumerable)new[] { item });
        }

        /// <summary>
        /// Removes multiple objects from the selection</summary>
        /// <param name="selectionContext">Selection context</param>
        /// <param name="items">Objects to remove</param>
        public static void RemoveRange(this ISelectionContext selectionContext, IEnumerable<object> items)
        {
            RemoveRange(selectionContext, (IEnumerable)items);
        }

        /// <summary>
        /// Removes multiple items from the selection</summary>
        /// <param name="selectionContext">Selection context</param>
        /// <param name="items">Items to remove</param>
        public static void RemoveRange(this ISelectionContext selectionContext, IEnumerable items)
        {
            if (selectionContext == null)
                throw new ArgumentNullException("selectionContext");
            if (items == null)
                throw new ArgumentNullException("items");

            var removed = new HashSet<object>();
            foreach (object item in items)
                removed.Add(item);

            var newSelection = new List<object>();
            foreach (object item in selectionContext.Selection)
                if (!removed.Contains(item))
                    newSelection.Add(item);

            selectionContext.Selection = newSelection;
        }

        /// <summary>
        /// Toggles an item in the selection</summary>
        /// <param name="selectionContext">Selection context</param>
        /// <param name="item">Item to toggle</param>
        public static void Toggle(this ISelectionContext selectionContext, object item)
        {
            ToggleRange(selectionContext, (IEnumerable)new[] { item });
        }

        /// <summary>
        /// Toggles multiple objects in the selection</summary>
        /// <param name="selectionContext">Selection context</param>
        /// <param name="items">Objects to toggle</param>
        public static void ToggleRange(this ISelectionContext selectionContext, IEnumerable<object> items)
        {
            ToggleRange(selectionContext, (IEnumerable)items);
        }

        /// <summary>
        /// Toggles multiple items in the selection</summary>
        /// <param name="selectionContext">Selection context</param>
        /// <param name="items">Items to toggle</param>
        public static void ToggleRange(this ISelectionContext selectionContext, IEnumerable items)
        {
            if (selectionContext == null)
                throw new ArgumentNullException("selectionContext");
            if (items == null)
                throw new ArgumentNullException("items");

            var toggled = new HashSet<object>();
            foreach (object item in items)
                toggled.Add(item);

            var newSelection = new List<object>();
            // keep already selected items that aren't to be toggled
            foreach (object item in selectionContext.Selection)
                if (!toggled.Contains(item))
                    newSelection.Add(item);

            // add toggled items that aren't in selection
            foreach (object item in items)
                if (!selectionContext.SelectionContains(item))
                    newSelection.Add(item);

            selectionContext.Selection = newSelection;
        }
    }
}
