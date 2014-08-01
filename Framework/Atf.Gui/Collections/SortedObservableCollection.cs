//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sce.Atf.Collections
{
    /// <summary>
    /// Utility class for a sorted observable collection</summary>
    /// <typeparam name="T">Type of the objects in the collection</typeparam>
    public class SortedObservableCollection<T> : ObservableCollection<T>
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="sorter">Comparison method to use in sorting the collection</param>
        public SortedObservableCollection(IComparer<T> sorter) 
            : base()
        {
            Sorter = sorter;
        }

        /// <summary>
        /// Gets and sets the comparer</summary>
        public IComparer<T> Sorter
        {
            get { return m_sorter; }
            set
            {
                m_sorter = value;
                ApplySort();
            }
        }

        /// <summary>
        /// Insert an item into the collection and re-sort if needed.</summary>
        /// <param name="index">Requested index to place the item at</param>
        /// <param name="item">The item to add to the collection</param>
        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            ApplySort();
        }

        /// <summary>
        /// Add an item to the collection, replacing the item at the specified index.
        /// Re-sort the collection if needed.</summary>
        /// <param name="index">Index to place the item at</param>
        /// <param name="item">The item to add to the collection</param>
        protected override void SetItem(int index, T item)
        {
            base.SetItem(index, item);
            ApplySort();
        }

        private void ApplySort()
        {
            if (m_sorter != null && Count > 1)
            {
                var sorted = new List<T>(this);
                sorted.Sort(m_sorter);
                foreach (var item in sorted)
                {
                    Move(IndexOf(item), sorted.IndexOf(item));
                }
            }

            // TODO: May be more efficient to supress noifications during Move operations and instead fire
            // a single Reset notification
            //OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private IComparer<T> m_sorter;
    }
}
