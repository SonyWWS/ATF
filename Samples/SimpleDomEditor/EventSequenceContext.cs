//Copyright � 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace SimpleDomEditorSample
{
    /// <summary>
    /// DomNode adapter that provides an event sequence context for the DOM editor.
    /// It implements IObservableContext, IInstancingContext, IEnumerableContext and ISubSelectionContext
    /// to provide editing services.</summary>
    public class EventSequenceContext : EditingContext,
        IListView,
        IItemView,
        IObservableContext,
        IInstancingContext,
        IEnumerableContext,
        ISubSelectionContext
    {
        /// <summary>
        /// Constructor</summary>
        public EventSequenceContext()
        {
            m_listView = new ListView();
            m_listView.SmallImageList = ResourceUtil.GetImageList16();
            m_listView.AllowDrop = true;
            m_listView.MultiSelect = true;
            m_listView.AllowColumnReorder = true;
            m_listView.LabelEdit = true;
            m_listView.Dock = DockStyle.Fill;

            m_subSelectionContext = new GenericSelectionContext();

            if (Reloaded == null) return; // suppress compiler warning
        }

        /// <summary>
        /// Performs initialization when the adapter is connected to the event context's DomNode.
        /// Raises the HistoryContext NodeSet event and performs custom processing.</summary>
        protected override void OnNodeSet()
        {
            DomNode.AttributeChanged += DomNode_AttributeChanged;
            DomNode.ChildInserted += DomNode_ChildInserted;
            DomNode.ChildRemoved += DomNode_ChildRemoved;

            m_listViewAdapter = new ListViewAdapter(m_listView);
            m_listViewAdapter.AllowSorting = true;
            m_listViewAdapter.ListView = this;

            base.OnNodeSet();
        }

        /// <summary>
        /// Gets the context's ListView</summary>
        public ListView ListView
        {
            get { return m_listView; }
        }

        /// <summary>
        /// Gets the context's ListViewAdapter</summary>
        public ListViewAdapter ListViewAdapter
        {
            get { return m_listViewAdapter; }
        }

        /// <summary>
        /// Gets or sets the context's ControlInfo</summary>
        public ControlInfo ControlInfo
        {
            get { return m_controlInfo; }
            set { m_controlInfo = value; }
        }

        #region IListView Members

        /// <summary>
        /// Gets the items in the list</summary>
        public IEnumerable<object> Items
        {
            get { return GetChildList<object>(Schema.eventSequenceType.eventChild); }
        }

        /// <summary>
        /// Gets names for table columns</summary>
        public string[] ColumnNames
        {
            get { return new string[] { "Name".Localize(), "Duration".Localize() }; }
        }

        #endregion

        #region IItemView Members

        /// <summary>
        /// Fills in or modifies the given display info for the item</summary>
        /// <param name="item">Item</param>
        /// <param name="info">Display info to update</param>
        public void GetInfo(object item, ItemInfo info)
        {
            Event _event = item.As<Event>();
            info.Label = _event.Name;
            info.ImageIndex = info.GetImageList().Images.IndexOfKey(Resources.EventImage);
            info.Properties = new object[] { _event.Duration };
        }

        #endregion

        #region IObservableContext Members

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

        #region IEnumerableContext Members

        /// <summary>
        /// Gets an enumeration of all of the items of this context</summary>
        IEnumerable<object> IEnumerableContext.Items
        {
            get { return Items; }
        }

        #endregion

        #region IInstancingContext Members

        /// <summary>
        /// Returns whether the context can copy the selection</summary>
        /// <returns>True iff the context can copy</returns>
        public bool CanCopy()
        {
            return Selection.Count > 0;
        }

        /// <summary>
        /// Copies the selection. Returns a data object representing the copied items.</summary>
        /// <returns>Data object representing the copied items; e.g., a
        /// System.Windows.Forms.IDataObject object</returns>
        public object Copy()
        {
            IEnumerable<DomNode> events = Selection.AsIEnumerable<DomNode>();
            List<object> copies = new List<object>(DomNode.Copy(events));
            return new DataObject(copies.ToArray());
        }

        /// <summary>
        /// Returns whether the context can insert the data object</summary>
        /// <param name="insertingObject">Data to insert; e.g., System.Windows.Forms.IDataObject</param>
        /// <returns>True iff the context can insert the data object</returns>
        public bool CanInsert(object insertingObject)
        {
            IDataObject dataObject = (IDataObject)insertingObject;
            object[] items = dataObject.GetData(typeof(object[])) as object[];
            if (items == null)
                return false;

            foreach (object item in items)
                if (!item.Is<Event>())
                    return false;

            return true;
        }

        /// <summary>
        /// Inserts the data object into the context</summary>
        /// <param name="insertingObject">Data to insert; e.g., System.Windows.Forms.IDataObject</param>
        public void Insert(object insertingObject)
        {
            IDataObject dataObject = (IDataObject)insertingObject;
            object[] items = dataObject.GetData(typeof(object[])) as object[];
            if (items == null)
                return;

            DomNode[] itemCopies = DomNode.Copy(items.AsIEnumerable<DomNode>());
            IList<Event> events = this.Cast<EventSequence>().Events;
            foreach (Event _event in itemCopies.AsIEnumerable<Event>())
                events.Add(_event);

            Selection.SetRange(itemCopies);
        }

        /// <summary>
        /// Returns whether the context can delete the selection</summary>
        /// <returns>True iff the context can delete</returns>
        public bool CanDelete()
        {
            return Selection.Count > 0;
        }

        /// <summary>
        /// Deletes the selection</summary>
        public void Delete()
        {
            foreach (DomNode node in Selection.AsIEnumerable<DomNode>())
                node.RemoveFromParent();

            Selection.Clear();
        }

        #endregion

        #region ISubSelectionContext
        /// <summary>
        /// Gets the sub-selection context--the selection context relative to the selection of this selection context</summary>
        public ISelectionContext SubSelectionContext
        {
            get { return m_subSelectionContext; }
        }
        #endregion

        private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
        {
            Event _event = e.DomNode.As<Event>();
            if (_event != null)
            {
                ItemChanged.Raise(this, new ItemChangedEventArgs<object>(_event));
            }
        }

        private void DomNode_ChildInserted(object sender, ChildEventArgs e)
        {
            Event _event = e.Child.As<Event>();
            if (_event != null)
            {
                ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(e.Index, _event));
            }
        }

        private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
        {
            Event _event = e.Child.As<Event>();
            if (_event != null)
            {
                ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(e.Index, _event));
            }
        }

        private readonly ListView m_listView;
        private ListViewAdapter m_listViewAdapter;
        private ControlInfo m_controlInfo;
        private readonly GenericSelectionContext m_subSelectionContext;
    }
}
