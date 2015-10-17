//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace SimpleDomEditorWpfSample
{
    /// <summary>
    /// DomNode adapter that provides an editing context for the ResourceListView</summary>
    public class ResourceListContext : EditingContext,
        IObservableContext,
        IInstancingContext,
        IEnumerableContext,
        INotifyPropertyChanged
    {
        /// <summary>
        /// Constructor</summary>
        public ResourceListContext()
        {
            if (Reloaded == null) return; // suppress compiler warning
        }

        /// <summary>
        /// Gets and sets the view displaying our content</summary>
        public ResourceListView View { get; set; }

        /// <summary>
        /// Gets the items in the document</summary>
        public IEnumerable<object> Items
        {
            get { return GetChildList<object>(Schema.eventType.resourceChild); }
        }

        /// <summary>
        /// Converts Items to Resource objects for easy data binding</summary>
        public IEnumerable<Resource> Resources
        {
            get
            {
                foreach (object item in Items)
                {
                    yield return item.As<Resource>();
                }
            }
        }

        /// <summary>
        /// Exposes the Selection for two way data binding. Needed because the ISelectionContext.Selection
        /// property is read-only.</summary>
        public IEnumerable<object> BindableSelection
        {
            get { return this.As<ISelectionContext>().Selection; }
            set { this.As<ISelectionContext>().SetRange(value); }
        }

        /// <summary>
        /// Performs initialization when the adapter is connected to the event context's DomNode.
        /// Raises the HistoryContext NodeSet event and performs custom processing.</summary>
        protected override void OnNodeSet()
        {
            DomNode.AttributeChanged += DomNode_AttributeChanged;
            DomNode.ChildInserted += DomNode_ChildInserted;
            DomNode.ChildRemoved += DomNode_ChildRemoved;

            base.OnNodeSet();
        }

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
        /// <returns><c>True</c> if the context can copy</returns>
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
        /// <returns><c>True</c> if the context can insert the data object</returns>
        public bool CanInsert(object insertingObject)
        {
            IDataObject dataObject = (IDataObject)insertingObject;
            object[] items = dataObject.GetData(typeof(object[])) as object[];
            if (items == null)
                return false;

            foreach (object item in items)
                if (!item.Is<Resource>())
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
            IList<Resource> resources = this.Cast<Event>().Resources;
            foreach (Resource resource in itemCopies.AsIEnumerable<Resource>())
                resources.Add(resource);

            Selection.SetRange(itemCopies);
        }

        /// <summary>
        /// Returns whether the context can delete the selection</summary>
        /// <returns><c>True</c> if the context can delete</returns>
        public bool CanDelete()
        {
            return Selection.Count > 0;
        }

        /// <summary>
        /// Deletes the selection</summary>
        public void Delete()
        {
            List<DomNode> nodesToRemove = new List<DomNode>();
            foreach (DomNode node in Selection.AsIEnumerable<DomNode>())
                nodesToRemove.Add(node);

            foreach (DomNode node in nodesToRemove)
                node.RemoveFromParent();

            Selection.Clear();
            OnPropertyChanged();
        }

        #endregion

        /// <summary>
        /// Occurs when a property value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
        {
            Resource resource = e.DomNode.As<Resource>();
            if (resource != null)
            {
                ItemChanged.Raise(this, new ItemChangedEventArgs<object>(resource));
            }

            OnPropertyChanged();
        }

        private void DomNode_ChildInserted(object sender, ChildEventArgs e)
        {
            Resource resource = e.Child.As<Resource>();
            if (resource != null)
            {
                ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(e.Index, resource));
            }

            OnPropertyChanged();
        }

        private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
        {
            Resource resource = e.Child.As<Resource>();
            if (resource != null)
            {
                ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(e.Index, resource));
            }

            OnPropertyChanged();
        }

        private void OnPropertyChanged()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Resources"));
            }
        }
    }
}
