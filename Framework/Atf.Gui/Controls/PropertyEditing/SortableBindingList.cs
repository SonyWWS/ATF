using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sce.Atf.Controls.PropertyEditing
{
   /// <summary>
   /// Class that provides a sortable binding list, which provides a mechanism for two way data binding between the list and a source</summary>
   /// <typeparam name="T">List object type</typeparam>
   /// <remarks>Collection items are sorted by a specified property value.</remarks>
    public class SortableBindingList<T> : BindingList<T>, IBindingListView// INotifyCollectionChanged
    {
        PropertyComparerCollection<T> _sorts;
        PropertyDescriptor _lastProperty;
        ListSortDirection _lastDirection;

        /// <summary>
        /// Adds the elements of the specified collection to the end of the list</summary>
        /// <param name="collection">Collection to be added</param>
        public void AddRange(IEnumerable<T> collection)
        {
            bool  oldValue = RaiseListChangedEvents;
            RaiseListChangedEvents = false;
            foreach (var item in collection)
                Add(item);
            RaiseListChangedEvents = oldValue;
            ListChangedEventArgs args = new ListChangedEventArgs(ListChangedType.Reset, -1);
            OnListChanged(args);
        }

        /// <summary>
        /// Gets whether there is a sorted list</summary>
        protected override bool IsSortedCore
        {
            get
            {
                //return base.IsSortedCore;
                return _sorts != null;
            }
        }

        /// <summary>
        /// Removes sorted list</summary>
        protected override void RemoveSortCore()
        {
            //base.RemoveSortCore();
            _sorts = null;
        }

        /// <summary>
        /// Gets whether a list is supported</summary>
        protected override bool SupportsSortingCore
        {
            get
            {
                //return base.SupportsSortingCore;
                return true;
            }
        }

        /// <summary>
        /// Gets primary sort direction of list</summary>
        protected override ListSortDirection SortDirectionCore
        {
            get
            {
                //return base.SortDirectionCore;
                return _sorts == null ? ListSortDirection.Ascending : _sorts.PrimaryDirection;
            }
        }

        /// <summary>
        /// Gets PropertyDescriptor of property whose value is used to sort the list</summary>
        protected override PropertyDescriptor SortPropertyCore
        {
            get
            {
                //return base.SortPropertyCore;
                return _sorts == null ? null : _sorts.PrimaryProperty;
            }
        }

        /// <summary>
        /// Creates sort list from a property whose value is sorted by and a direction</summary>
        /// <param name="prop">Property whose value is sorted by</param>
        /// <param name="direction">Sort direction</param>
        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            if (prop.Equals(_lastProperty))
            {
                direction = _lastDirection == ListSortDirection.Ascending ?
                ListSortDirection.Descending : ListSortDirection.Ascending;
                _lastDirection = direction;
            }
            else
            {
                _lastProperty = prop;
                _lastDirection = direction;
            }
            //base.ApplySortCore(prop, direction);
            ListSortDescription[] arr = { new ListSortDescription(prop, direction) };
            ApplySort(new ListSortDescriptionCollection(arr));
        }

        /// <summary>
        /// Sorts a ListSortDescriptionCollection</summary>
        /// <param name="sortCollection">ListSortDescriptionCollection</param>
        public void ApplySort(ListSortDescriptionCollection sortCollection)
        {
            bool oldRaise = RaiseListChangedEvents;
            RaiseListChangedEvents = false;
            try
            {
                PropertyComparerCollection<T> tmp = new PropertyComparerCollection<T>(sortCollection);
                List<T> items = new List<T>(this);
                items.Sort(tmp);
                int index = 0;
                foreach (T item in items)
                {
                    SetItem(index++, item);
                }
                _sorts = tmp;
            }
            finally
            {
                RaiseListChangedEvents = oldRaise;
                ResetBindings();
            }
        }

        /// <summary>
        /// Gets or sets the filter to be used to exclude items from the collection of items returned by the data source</summary>
        /// <remarks>Not implemented</remarks>
        String IBindingListView.Filter
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Removes the current filter applied to the data source</summary>
        /// <remarks>Not implemented</remarks>
        void IBindingListView.RemoveFilter()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a collection of object property sort operation descriptions</summary>
        ListSortDescriptionCollection IBindingListView.SortDescriptions
        {
            get { return _sorts.Sorts; }
        }

        bool IBindingListView.SupportsAdvancedSorting
        {
            get { return true; }
        }

        bool IBindingListView.SupportsFiltering
        {
            get { return false; }
        }
    }

    /// <summary>
    /// Collection of PropertyComparers (object property comparers)</summary>
    /// <typeparam name="T">Type of object with property to be compared</typeparam>
    public class PropertyComparerCollection<T> : IComparer<T>
    {
        private readonly ListSortDescriptionCollection _sorts;
        private readonly PropertyComparer<T>[] _comparers;

        /// <summary>
        /// Gets a collection of object property sort operation descriptions</summary>
        public ListSortDescriptionCollection Sorts
        {
            get { return _sorts; }
        }

        /// <summary>
        /// Gets primary (first) object property</summary>
        public PropertyDescriptor PrimaryProperty
        {
            get
            {
                return _comparers.Length == 0 ? null : _comparers[0].Property;
            }
        }

        /// <summary>
        /// Gets primary (first) list sort direction</summary>
        public ListSortDirection PrimaryDirection
        {
            get
            {
                return _comparers.Length == 0 ? ListSortDirection.Ascending
                : _comparers[0].Descending ?
                ListSortDirection.Descending : ListSortDirection.Ascending;
            }
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="sorts">Collection of object property sort operation descriptions</param>
        /// <exception cref="ArgumentNullException">If ListSortDescriptionCollection is null</exception>
        public PropertyComparerCollection(ListSortDescriptionCollection sorts)
        {
            if (sorts == null)
                throw new ArgumentNullException("sorts");
            _sorts = sorts;
            List<PropertyComparer<T>> list = new List<PropertyComparer<T>>();
            foreach (ListSortDescription item in _sorts)
            {
                list.Add(new PropertyComparer<T>(item.PropertyDescriptor,
                item.SortDirection == ListSortDirection.Descending));
                _comparers = list.ToArray();
            }
        }

        /// <summary>
        /// Comparer function that gets first non-identical comparison in collection of PropertyComparers, if there is one</summary>
        /// <param name="x">Type 1 of object with property to be compared</param>
        /// <param name="y">Type 2 of object with property to be compared</param>
        /// <returns>A signed integer that indicates the relative values of comparisons in collection.
        /// Less than zero: Type 1 is less than Type 2. 
        /// Zero: Type 1 equals Type 2. 
        /// Greater than zero: Type 1 is greater than Type 2.</returns>
        int IComparer<T>.Compare(T x, T y)
        {
            int result = 0;
            for (int i = 0; i < _comparers.Length; i++)
            {
                result = _comparers[i].Compare(x, y);
                if (result != 0) break;
            }
            return result;
        }
    }

    /// <summary>
    /// Class for comparing object's properties</summary>
    /// <typeparam name="T">Type of object with property to be compared</typeparam>
    public class PropertyComparer<T> : IComparer<T>
    {
        private readonly bool _descending;
        private readonly PropertyDescriptor _property;

        /// <summary>
        /// Gets whether sort is descending</summary>
        public bool Descending
        {
            get { return _descending; }
        }

        /// <summary>
        /// Gets comparison property</summary>
        public PropertyDescriptor Property
        {
            get { return _property; }
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="property">Property to be compared</param>
        /// <param name="descending">Whether sort is descending or not</param>
        /// <exception cref="ArgumentNullException">If PropertyDescriptor is null</exception>
        public PropertyComparer(PropertyDescriptor property, bool descending)
        {
            if (property == null) throw new ArgumentNullException("property");
            this._descending = descending;
            this._property = property;
        }

        /// <summary>
        /// Property value comparer function</summary>
        /// <param name="x">Type 1 of object with property to be compared</param>
        /// <param name="y">Type 2 of object with property to be compared</param>
        /// <returns>Signed integer that indicates the relative values of properties of objects.
        /// Less than zero: Type 1 object property is less than Type 2 object property. 
        /// Zero: Type 1 object property equals Type 2 object property. 
        /// Greater than zero: Type 1 object property is greater than Type 2 object property.</returns>
        public int Compare(T x, T y)
        {
            int value = Comparer.Default.Compare(_property.GetValue(x), _property.GetValue(y));
            return _descending ? -value : value;
        }
    }
}
