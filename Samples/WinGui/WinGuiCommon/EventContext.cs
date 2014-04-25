//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace WinGuiCommon
{
    /// <summary>
    /// DomNode adapter that provides an event context for the DOM editor for updating the views.
    /// It implements IObservableContext, IInstancingContext and IEnumerableContext to provide
    /// services for the context.</summary>
    public class EventContext : EditingContext,
        IListView,
        IItemView,
        IObservableContext,
        IInstancingContext,
        IEnumerableContext
    {
        /// <summary>
        /// Constructor</summary>
        public EventContext()
        {
            if (Reloaded == null) return; // suppress compiler warning
        }

        /// <summary>
        /// Performs initialization when the adapter is connected to the event context's DomNode.
        /// Raises the HistoryContext NodeSet event and performs custom processing:
        /// subscribing to DomNode tree changes.</summary>
        protected override void OnNodeSet()
        {
            DomNode.AttributeChanged += new EventHandler<AttributeEventArgs>(DomNode_AttributeChanged);
            DomNode.ChildInserted += new EventHandler<ChildEventArgs>(DomNode_ChildInserted);
            DomNode.ChildRemoved += new EventHandler<ChildEventArgs>(DomNode_ChildRemoved);

            base.OnNodeSet();
        }

        #region IListView Members

        /// <summary>
        /// Gets the items in the list</summary>
        public IEnumerable<object> Items
        {
            get { return GetChildList<object>(Schema.eventType.resourceChild); }
        }

        /// <summary>
        /// Gets names for table columns</summary>
        public string[] ColumnNames
        {
            get { return new string[] { Localizer.Localize("Name"), Localizer.Localize("Size") }; }
        }

        #endregion

        #region IItemView Members

        /// <summary>
        /// Fills in or modifies the given display info for the item</summary>
        /// <param name="item">Item</param>
        /// <param name="info">Display info to update</param>
        public void GetInfo(object item, ItemInfo info)
        {
            Resource resource = Adapters.As<Resource>(item);
            info.Label = resource.Name;
            string type = null;
            if (resource.DomNode.Type == Schema.animationResourceType.Type)
                type = Resources.AnimationImage;
            else if (resource.DomNode.Type == Schema.geometryResourceType.Type)
                type = Resources.GeometryImage;

            info.ImageIndex = info.GetImageList().Images.IndexOfKey(type);
            info.Properties = new object[] { resource.Size };
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
            IEnumerable<DomNode> resources = Selection.AsIEnumerable<DomNode>();
            List<object> copies = new List<object>(DomNode.Copy(resources));
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
                if (!Adapters.Is<Resource>(item))
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

            DomNode[] itemCopies = DomNode.Copy(Adapters.AsIEnumerable<DomNode>(items));
            IList<Resource> resources = this.Cast<Event>().Resources;
            foreach (Resource resource in Adapters.AsIEnumerable<Resource>(itemCopies))
                resources.Add(resource);

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

        private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
        {
            Resource resource = e.DomNode.As<Resource>();
            if (resource != null)
            {
                ItemChanged.Raise(this, new ItemChangedEventArgs<object>(resource));
            }
        }

        private void DomNode_ChildInserted(object sender, ChildEventArgs e)
        {
            Resource resource = e.Child.As<Resource>();
            if (resource != null)
            {
                ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(e.Index, resource));
            }
        }

        private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
        {
            Resource resource = e.Child.As<Resource>();
            if (resource != null)
            {
                ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(e.Index, resource));
            }
        }
    }
}
