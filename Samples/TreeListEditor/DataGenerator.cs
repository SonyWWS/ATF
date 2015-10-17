//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;

namespace TreeListEditor
{
    /// <summary>
    /// Enum for generated data types</summary>
    enum DataType
    {
        Integer = 0,
        String = 1,
    }

    /// <summary>
    /// Class to generate data items to be displayed in tree editors</summary>
    class DataItem : CustomTypeDescriptor
    {
        /// <summary>
        /// Constructor with parameters</summary>
        /// <param name="parent">Item parent</param>
        /// <param name="name">Item name</param>
        /// <param name="type">Item data type</param>
        /// <param name="value">Item value</param>
        public DataItem(DataItem parent, string name, DataType type, object value)
        {
            m_parent = parent;

            Name = name;
            Type = type;
            Value = value;
        }

        /// <summary>
        /// Gets item parent</summary>
        public DataItem Parent
        {
            get { return m_parent; }
        }

        /// <summary>
        /// Gets item name</summary>
        [PropertyEditingAttribute]
        public string Name { get; set; }

        /// <summary>
        /// Gets item data type</summary>
        [PropertyEditingAttribute]
        public DataType Type { get; set; }

        /// <summary>
        /// Gets item value</summary>
        [PropertyEditingAttribute]
        public object Value { get; set; }

        /// <summary>
        /// Gets whether item has children</summary>
        [PropertyEditingAttribute]
        public bool HasChildren
        {
            get { return m_children.Count > 0; }
        }

        /// <summary>
        /// Gets item's children</summary>
        public ICollection<DataItem> Children
        {
            get { return m_children; }
        }

        #region Property Editing

        /// <summary>
        /// Attribute of DataItem</summary>
        [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
        public class PropertyEditingAttribute : Attribute
        {
        }

        /// <summary>
        /// PropertyDescriptor with additional information for a property</summary>
        public class PropertyPropertyDescriptor : PropertyDescriptor
        {
            /// <summary>
            /// Constructor with parameters</summary>
            /// <param name="property">PropertyInfo for property</param>
            /// <param name="ownerType">Owning type</param>
            public PropertyPropertyDescriptor(PropertyInfo property, Type ownerType)
                : base(property.Name, (Attribute[])property.GetCustomAttributes(typeof(Attribute), true))
            {
                m_property = property;
                m_ownerType = ownerType;
            }

            /// <summary>
            /// Gets owning type</summary>
            public Type OwnerType
            {
                get { return m_ownerType; }
            }

            /// <summary>
            /// Gets PropertyInfo for property</summary>
            public PropertyInfo Property
            {
                get { return m_property; }
            }

            /// <summary>
            /// Gets whether this property is read-only</summary>
            public override bool IsReadOnly
            {
                get { return GetChildProperties().Count <= 0; }
            }

            /// <summary>
            /// Returns whether resetting an object changes its value</summary>
            /// <param name="component">Component to test for reset capability</param>
            /// <returns>Whether resetting a component changes its value</returns>
            public override bool CanResetValue(object component)
            {
                return false;
            }

            /// <summary>
            /// Resets the value for this property of the component to the default value</summary>
            /// <param name="component"></param>
            public override void ResetValue(object component)
            {
            }

            /// <summary>
            /// Determines whether the value of this property needs to be persisted</summary>
            /// <param name="component">Component with the property to be examined for persistence</param>
            /// <returns><c>True</c> if the property should be persisted</returns>
            public override bool ShouldSerializeValue(object component)
            {
                return true;
            }

            /// <summary>
            /// Gets the type of the component this property is bound to</summary>
            public override Type ComponentType
            {
                get { return m_property.DeclaringType; }
            }

            /// <summary>
            /// Gets the type of the property</summary>
            public override Type PropertyType
            {
                get { return m_property.PropertyType; }
            }

            /// <summary>
            /// Gets the current value of property on component</summary>
            /// <param name="component">Component with the property value that is to be set</param>
            /// <returns>New value</returns>
            public override object GetValue(object component)
            {
                return m_property.GetValue(component, null);
            }

            /// <summary>
            /// Sets the value of the component to a different value</summary>
            /// <param name="component">Component with the property value that is to be set</param>
            /// <param name="value">New value</param>
            public override void SetValue(object component, object value)
            {
                m_property.SetValue(component, value, null);

                ItemChanged.Raise(component, EventArgs.Empty);
            }

            /// <summary>
            /// Gets an editor of the specified type</summary>
            /// <param name="editorBaseType">The base type of editor, 
            /// which is used to differentiate between multiple editors that a property supports</param>
            /// <returns>Instance of the requested editor type, or null if an editor cannot be found</returns>
            public override object GetEditor(Type editorBaseType)
            {
                if (m_property.PropertyType.Equals(typeof(DataItem)))
                    return m_nestedCollectionEditor ?? (m_nestedCollectionEditor = new NestedCollectionEditor());

                return base.GetEditor(editorBaseType);
            }

            /// <summary>
            /// Event that is raised after an item changed</summary>
            public event EventHandler ItemChanged;

            private readonly Type m_ownerType;
            private readonly PropertyInfo m_property;

            private NestedCollectionEditor m_nestedCollectionEditor;
        }

        /// <summary>
        /// Returns a collection of property descriptors for the object represented by this type descriptor</summary>
        /// <returns>System.ComponentModel.PropertyDescriptorCollection containing the property descriptions for the object 
        /// represented by this type descriptor. The default is System.ComponentModel.PropertyDescriptorCollection.Empty.</returns>
        public override PropertyDescriptorCollection GetProperties()
        {
            var props = new PropertyDescriptorCollection(null);

            foreach (var property in GetType().GetProperties())
            {
                var propertyDesc =
                    new PropertyPropertyDescriptor(property, GetType());

                propertyDesc.ItemChanged += PropertyDescItemChanged;

                foreach (Attribute attr in propertyDesc.Attributes)
                {
                    if (attr.GetType().Equals(typeof(PropertyEditingAttribute)))
                        props.Add(propertyDesc);
                }
            }

            return props;
        }

        /// <summary>
        /// Returns an object that contains the property described by the specified property descriptor</summary>
        /// <param name="pd">Property descriptor for which to retrieve the owning object</param>
        /// <returns>System.Object that owns the given property specified by the type descriptor. The default is null.</returns>
        public override object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        private void PropertyDescItemChanged(object sender, EventArgs e)
        {
            ItemChanged.Raise(this, EventArgs.Empty);
        }

        #endregion

        /// <summary>
        /// Event that is raised after an item changed</summary>
        public event EventHandler ItemChanged;

        private readonly DataItem m_parent;

        private readonly List<DataItem> m_children =
            new List<DataItem>();
    }

    /// <summary>
    /// Class to generate collection of data items (DataItem) to be displayed in tree editors</summary>
    class DataContainer : ITreeListView, IItemView, IObservableContext, ISelectionContext, IValidationContext
    {
        /// <summary>
        /// Constructor without parameters</summary>
        public DataContainer()
        {
            m_selection.Changing += TheSelectionChanging;
            m_selection.Changed += TheSelectionChanged;

            if (s_dataImageIndex == -1)
            {
                s_dataImageIndex =
                    ResourceUtil.GetImageList16().Images.IndexOfKey(
                        Resources.DataImage);
            }

            if (s_folderImageIndex == -1)
            {
                s_folderImageIndex =
                    ResourceUtil.GetImageList16().Images.IndexOfKey(
                        Resources.FolderImage);
            }

            // Stop compiler warning
            if (Cancelled == null) return;
        }

        /// <summary>
        /// Gets data item at index</summary>
        /// <param name="index">Index of data item</param>
        /// <returns>Data item at index</returns>
        public DataItem this[int index]
        {
            get { return m_data[index]; }
        }

        /// <summary>
        /// Clears all data items</summary>
        public void Clear()
        {
            m_data.Clear();
            Reloaded.Raise(this, EventArgs.Empty);
        }

        /// <summary>
        /// Generates a list of flat (non-hierarchical) data</summary>
        /// <param name="view">Tree list view</param>
        /// <param name="lastHit">Last data item selected</param>
        public static void GenerateFlat(ITreeListView view, object lastHit)
        {
            var container = view.As<DataContainer>();
            if (container == null)
                return;

            container.GenerateFlat(lastHit.As<DataItem>());
        }

        /// <summary>
        /// Generates a list of hierarchical data</summary>
        /// <param name="view">Tree list view</param>
        /// <param name="lastHit">Last data item selected</param>
        public static void GenerateHierarchical(ITreeListView view, object lastHit)
        {
            var container = view.As<DataContainer>();
            if (container == null)
                return;

            container.GenerateHierarchical(lastHit.As<DataItem>());
        }

        /// <summary>
        /// Generates data for a virtual list</summary>
        /// <param name="view">Tree list view</param>
        public static void GenerateVirtual(ITreeListView view)
        {
            var container = view.As<DataContainer>();
            if (container == null)
                return;

            container.GenerateVirtual();
        }

        /// <summary>
        /// Updates generated data items</summary>
        /// <param name="view">Tree list view</param>
        public static void Reload(ITreeListView view)
        {
            var container = view.As<DataContainer>();
            if (container == null)
                return;

            container.Reload();
        }

        /// <summary>
        /// Removes last data item selected</summary>
        /// <param name="view">Tree list view</param>
        /// <param name="lastHit">Last data item selected</param>
        public static void RemoveItem(ITreeListView view, object lastHit)
        {
            var container = view.As<DataContainer>();
            if (container == null)
                return;

            container.RemoveItem(lastHit.As<DataItem>());
        }

        /// <summary>
        /// Regenerates selected data items</summary>
        /// <param name="view">Tree list view</param>
        /// <param name="selection">Selected data items</param>
        public static void ModifySelected(ITreeListView view, IEnumerable<object> selection)
        {
            var container = view.As<DataContainer>();
            if (container == null)
                return;

            container.ModifySelected(selection.ToList());
        }

        #region ITreeListView Interface

        /// <summary>
        /// Gets the root level objects of the tree view</summary>
        public IEnumerable<object> Roots
        {
            get { return m_data.AsIEnumerable<object>(); }
        }

        /// <summary>
        /// Gets enumeration of the children of the given parent object</summary>
        /// <param name="parent">Parent object</param>
        /// <returns>Enumeration of the children of the parent object</returns>
        public IEnumerable<object> GetChildren(object parent)
        {
            var dataParent = parent.As<DataItem>();
            if (dataParent == null)
                yield break;

            if (!dataParent.HasChildren)
                yield break;

            foreach (var data in dataParent.Children)
                yield return data;
        }

        /// <summary>
        /// Gets names for columns</summary>
        public string[] ColumnNames
        {
            get { return s_columnNames; }
        }

        #endregion

        #region IItemView Interface

        /// <summary>
        /// Fills in or modifies the given display info for the item</summary>
        /// <param name="obj">Item</param>
        /// <param name="info">Display info to update</param>
        public void GetInfo(object obj, ItemInfo info)
        {
            var data = obj.As<DataItem>();
            if (data == null)
                return;

            info.Label = data.Name;
            info.Properties =
                new object[]
                {
                    data.Type.ToString(),
                    data.Value
                };

            info.IsLeaf = !data.HasChildren;
            info.ImageIndex =
                data.HasChildren
                    ? s_folderImageIndex
                    : s_dataImageIndex;
        }

        #endregion

        #region IObservableContext Interface

        /// <summary>
        /// Event that is raised when an item is inserted</summary>
        public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

        /// <summary>
        /// Event that is raised when an item is removed</summary>
        public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

        /// <summary>
        /// Event that is raised when an item is changed</summary>
        public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

        /// <summary>
        /// Event that is raised when the collection has been reloaded</summary>
        public event EventHandler Reloaded;

        #endregion

        #region ISelectionContext Interface

        /// <summary>
        /// Gets or sets the enumeration of selected items</summary>
        public IEnumerable<object> Selection
        {
            get { return m_selection; }
            set { m_selection.SetRange(value); }
        }

        /// <summary>
        /// Returns all selected items of the given type</summary>
        /// <typeparam name="T">Desired item type</typeparam>
        /// <returns>All selected items of the given type</returns>
        public IEnumerable<T> GetSelection<T>() where T : class
        {
            return m_selection.AsIEnumerable<T>();
        }

        /// <summary>
        /// Gets the last selected item</summary>
        public object LastSelected
        {
            get { return m_selection.LastSelected; }
        }

        /// <summary>
        /// Gets the last selected item of the given type; this may not be the same
        /// as the LastSelected item</summary>
        /// <typeparam name="T">Desired item type</typeparam>
        /// <returns>Last selected item of the given type</returns>
        public T GetLastSelected<T>() where T : class
        {
            return m_selection.GetLastSelected<T>();
        }

        /// <summary>
        /// Returns whether the selection contains the given item</summary>
        /// <param name="item">Item</param>
        /// <returns><c>True</c> if the selection contains the given item</returns>
        /// <remarks>Override to customize how items are compared for equality, e.g., for
        /// tree views, the selection might be adaptable paths, in which case the override
        /// could compare the item to the last elements of the selected paths.</remarks>
        public bool SelectionContains(object item)
        {
            return m_selection.Contains(item);
        }

        /// <summary>
        /// Gets the number of items in the current selection</summary>
        public int SelectionCount
        {
            get { return m_selection.Count; }
        }

        /// <summary>
        /// Event that is raised before the selection changes</summary>
        public event EventHandler SelectionChanging;

        /// <summary>
        /// Event that is raised after the selection changes</summary>
        public event EventHandler SelectionChanged;

        #endregion

        #region IValidationContext Interface

        /// <summary>
        /// Event that is raised before validation begins</summary>
        public event EventHandler Beginning;

        /// <summary>
        /// Event that is raised after validation is cancelled</summary>
        public event EventHandler Cancelled;

        /// <summary>
        /// Event that is raised before validation ends</summary>
        public event EventHandler Ending;

        /// <summary>
        /// Event that is raised after validation ends</summary>
        public event EventHandler Ended;

        #endregion

        private void GenerateFlat(DataItem parent)
        {
            Beginning.Raise(this, EventArgs.Empty);

            var items = s_random.Next(5, 16);
            for (var i = 0; i < items; i++)
            {
                var data = CreateItem(parent);
                data.ItemChanged += DataItemChanged;

                if (parent != null)
                    parent.Children.Add(data);
                else
                    m_data.Add(data);

                ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(-1, data, parent));
            }

            if (parent != null)
            {
                ItemChanged.Raise(this, new ItemChangedEventArgs<object>(parent));
            }

            Ending.Raise(this, EventArgs.Empty);
            Ended.Raise(this, EventArgs.Empty);
        }

        private void GenerateHierarchical(DataItem parent)
        {
            DataItem root = null;
            var tempParent = parent;

            var items = s_random.Next(3, 10);
            for (var i = 0; i < items; i++)
            {
                var data = CreateItem(tempParent);
                data.ItemChanged += DataItemChanged;

                if (root == null)
                    root = data;

                if (tempParent == null)
                    m_data.Add(data);
                else
                    tempParent.Children.Add(data);

                tempParent = data;
            }

            ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(-1, root, parent));

            if (parent != null)
            {
                ItemChanged.Raise(this, new ItemChangedEventArgs<object>(parent));
            }
        }

        private void GenerateVirtual()
        {
            var items = s_random.Next(10000, 100001);

            Outputs.WriteLine(
                OutputMessageType.Info,
                "Adding {0} items to the virtual list.",
                items);

            var arrayItems = new object[items];
            for (var i = 0; i < items; i++)
            {
                var data = CreateItem(null);
                data.ItemChanged += DataItemChanged;
                arrayItems[i] = data;
                m_data.Add(data);
            }

            // Can accept an array of objects or single object
            ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(-1, arrayItems));
        }

        private void Reload()
        {
            Beginning.Raise(this, EventArgs.Empty);
            Reloaded.Raise(this, EventArgs.Empty);
            Ending.Raise(this, EventArgs.Empty);
            Ended.Raise(this, EventArgs.Empty);
        }

        private void RemoveItem(DataItem item)
        {
            if (item == null)
                return;

            if (item.Parent != null)
                item.Parent.Children.Remove(item);
            else
                m_data.Remove(item);

            ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(-1, item));
        }

        private void ModifySelected(IEnumerable<object> selection)
        {
            Beginning.Raise(this, EventArgs.Empty);

            foreach (var obj in selection)
            {
                var data = obj.As<DataItem>();
                if (data == null)
                    continue;

                switch (data.Type)
                {
                    case DataType.Integer:
                        data.Value = (int)data.Value + s_random.Next(2, 6);
                        break;

                    case DataType.String:
                        data.Value = string.Format("{0}{1}", data.Value, Alphabet[s_random.Next(0, Alphabet.Length)]);
                        break;
                }

                ItemChanged.Raise(this, new ItemChangedEventArgs<object>(data));
            }

            Ending.Raise(this, EventArgs.Empty);
            Ended.Raise(this, EventArgs.Empty);
        }

        private void TheSelectionChanging(object sender, EventArgs e)
        {
            SelectionChanging.Raise(this, EventArgs.Empty);
        }

        private void TheSelectionChanged(object sender, EventArgs e)
        {
            SelectionChanged.Raise(this, EventArgs.Empty);
        }

        private void DataItemChanged(object sender, EventArgs e)
        {
            var data = (DataItem)sender;

            ItemChanged.Raise(this, new ItemChangedEventArgs<object>(data));
        }

        private static DataItem CreateItem(DataItem parent)
        {
            var enumLength = Enum.GetNames(typeof(DataType)).Length;
            var name = CreateString(s_random.Next(2, 11));
            var type = (DataType)s_random.Next(0, enumLength);

            object value;
            switch (type)
            {
                case DataType.Integer:
                    value = s_random.Next(0, 51);
                    break;

                case DataType.String:
                    value = CreateString(s_random.Next(5, 16));
                    break;

                default:
                    value = type.ToString();
                    break;
            }

            var data =
                new DataItem(
                    parent,
                    name,
                    type,
                    value);

            return data;
        }

        private static string CreateString(int characters)
        {
            var sb = new StringBuilder();

            var max = Alphabet.Length;
            for (var i = 0; i < characters; i++)
            {
                var ch = Alphabet[s_random.Next(0, max)];
                sb.Append(ch);
            }

            return sb.ToString();
        }

        private readonly List<DataItem> m_data =
            new List<DataItem>();

        private readonly Selection<object> m_selection =
            new Selection<object>();

        private static int s_dataImageIndex = -1;
        private static int s_folderImageIndex = -1;

        private static readonly Random s_random =
            new Random();

        private static readonly string[] s_columnNames =
            new[]
            {
                "Name",
                "Type",
                "Value",
            };

        private const string Alphabet =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    }

    /// <summary>
    /// Class to compare data items for sorting</summary>
    class DataComparer : IComparer<TreeListView.Node>
    {
        /// <summary>
        /// Constructor with TreeListView</summary>
        /// <param name="control">Tree list view</param>
        public DataComparer(TreeListView control)
        {
            m_control = control;
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether 
        /// one is less than, equal to, or greater than the other</summary>
        /// <param name="x">First object to compare</param>
        /// <param name="y">Second object to compare</param>
        /// <returns>Signed integer that indicates the relative values of x and y. 
        /// Less than zero: x is less than y. Zero: x equals y. Greater than zero: x is greater than y.</returns>
        public int Compare(TreeListView.Node x, TreeListView.Node y)
        {
            if ((x == null) && (y == null))
                return 0;

            if (x == null)
                return 1;

            if (y == null)
                return -1;

            if (ReferenceEquals(x, y))
                return 0;

            var lhs = x.Tag.As<DataItem>();
            var rhs = y.Tag.As<DataItem>();

            if ((lhs == null) && (rhs == null))
                return 0;

            if (lhs == null)
                return 1;

            if (rhs == null)
                return -1;

            CompareFunction[] sortFuncs;
            switch (m_control.SortColumn)
            {
                case 1: sortFuncs = s_column1Sort; break;
                case 2: sortFuncs = s_column2Sort; break;
                default: sortFuncs = s_column0Sort; break;
            }

            var result = 0;

            for (var i = 0; i < sortFuncs.Length; i++)
            {
                result = sortFuncs[i](lhs, rhs);
                if (result != 0)
                    break;
            }

            if (m_control.SortOrder == SortOrder.Descending)
                result *= -1;

            return result;
        }

        private static int CompareNames(DataItem x, DataItem y)
        {
            return string.Compare(x.Name, y.Name);
        }

        private static int CompareTypes(DataItem x, DataItem y)
        {
            if (x.Type == y.Type)
                return 0;

            return (int)x.Type < (int)y.Type ? -1 : 1;
        }

        private static int CompareValues(DataItem x, DataItem y)
        {
            return string.Compare(x.Value.ToString(), y.Value.ToString());
        }

        private delegate int CompareFunction(DataItem x, DataItem y);

        private static readonly CompareFunction[] s_column0Sort =
            new CompareFunction[] { CompareNames, CompareTypes, CompareValues };

        private static readonly CompareFunction[] s_column1Sort =
            new CompareFunction[] { CompareTypes, CompareNames, CompareValues };

        private static readonly CompareFunction[] s_column2Sort =
            new CompareFunction[] { CompareValues, CompareNames, CompareTypes };

        private readonly TreeListView m_control;
    }
}