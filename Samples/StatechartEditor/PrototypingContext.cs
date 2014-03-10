//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace StatechartEditorSample
{
    /// <summary>
    /// Editing context for prototypes in the statechart document; this is the context that is
    /// bound to the PrototypeLister when an statechart document becomes the active context. This
    /// context implements instancing differently than the StatechartContext. It inserts states
    /// and transitions by converting them into prototypes. It copies by converting prototypes
    /// back into their component states and transitions.</summary>
    public class PrototypingContext : Sce.Atf.Dom.EditingContext,
        IInstancingContext,
        IPrototypingContext,
        IObservableContext,
        INamingContext
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the prototype's DomNode.
        /// Raises the SelectionContext NodeSet event and performs custom processing.</summary>
        protected override void OnNodeSet()
        {
            DomNode.AttributeChanged += new EventHandler<AttributeEventArgs>(DomNode_AttributeChanged);
            DomNode.ChildInserted += new EventHandler<ChildEventArgs>(DomNode_ChildInserted);
            DomNode.ChildRemoved += new EventHandler<ChildEventArgs>(DomNode_ChildRemoved);

            base.OnNodeSet();

            if (Reloaded == null) return; // inhibit compiler warning
        }

        /// <summary>
        /// Gets the active PrototypeFolder</summary>
        public PrototypeFolder PrototypeFolder
        {
            get { return GetChild<PrototypeFolder>(Schema.statechartDocumentType.prototypeFolderChild); }
        }

        #region INamingContext Members

        /// <summary>
        /// Gets the item's name in the context, or null if none</summary>
        /// <param name="item">Item</param>
        /// <returns>Item's name in the context, or null if none</returns>
        string INamingContext.GetName(object item)
        {
            Prototype prototype = Adapters.As<Prototype>(item);
            if (prototype != null)
                return prototype.Name;

            PrototypeFolder folder = Adapters.As<PrototypeFolder>(item);
            if (folder != null)
                return folder.Name;

            return null;
        }

        /// <summary>
        /// Returns whether the item can be named</summary>
        /// <param name="item">Item to name</param>
        /// <returns>True iff the item can be named</returns>
        bool INamingContext.CanSetName(object item)
        {
            return
                Adapters.Is<Prototype>(item) ||
                Adapters.Is<PrototypeFolder>(item);
        }

        /// <summary>
        /// Sets the item's name</summary>
        /// <param name="item">Item to name</param>
        /// <param name="name">New item name</param>
        void INamingContext.SetName(object item, string name)
        {
            Prototype prototype = Adapters.As<Prototype>(item);
            if (prototype != null)
            {
                prototype.Name = name;
            }
            else
            {
                PrototypeFolder folder = Adapters.As<PrototypeFolder>(item);
                if (folder != null)
                    folder.Name = name;
            }
        }

        #endregion

        #region IPrototypingContext Members

        /// <summary>
        /// Sets the active item in the prototyping context; used by UI components to
        /// set insertion point as the user selects and edits</summary>
        /// <param name="item">Active layer or item</param>
        public void SetActiveItem(object item)
        {
            m_activeItem = item;
        }

        /// <summary>
        /// Gets the IDataObject for the items being dragged from a prototype lister, for
        /// use in a drag-and-drop operation</summary>
        /// <param name="items">Objects being dragged</param>
        /// <returns>IDataObject representing the dragged items</returns>
        public IDataObject GetInstances(IEnumerable<object> items)
        {
            List<object> instances = new List<object>();
            foreach (object item in items)
            {
                Prototype prototype = Adapters.As<Prototype>(item);
                if (prototype != null)
                {
                    instances.AddRange(Adapters.AsIEnumerable<object>(prototype.States));
                    instances.AddRange(Adapters.AsIEnumerable<object>(prototype.Transitions));
                    continue;
                }

                PrototypeFolder folder = Adapters.As<PrototypeFolder>(item);
                if (folder != null)
                    instances.Add(folder);
            }
            return new DataObject(instances.ToArray());
        }

        #endregion

        #region IInstancingContext Members

        /// <summary>
        /// Returns whether the context can copy the selection</summary>
        /// <returns>True iff the context can copy</returns>
        public bool CanCopy()
        {
            // CanCopy is the same as for statechart
            return this.As<EditingContext>().CanCopy();
        }

        /// <summary>
        /// Copies the selection. Returns a data object representing the copied items.</summary>
        /// <returns>Data object representing the copied items; e.g., a
        /// System.Windows.Forms.IDataObject object</returns>
        public object Copy()
        {
            // Copy is the same as for statechart
            return this.As<EditingContext>().Copy();
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
            {
                if (!Adapters.Is<StateBase>(item) &&
                    !Adapters.Is<Transition>(item) &&
                    !Adapters.Is<Annotation>(item))
                {
                    return false;
                }
            }

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

            object[] itemCopies = DomNode.Copy(Adapters.AsIEnumerable<DomNode>(items));

            // create a new prototype
            DomNode node = new DomNode(Schema.prototypeType.Type);
            Prototype prototype = node.As<Prototype>();
            prototype.Name = Localizer.Localize("Prototype", "Statechart prototype");
            foreach (StateBase state in Adapters.AsIEnumerable<StateBase>(itemCopies))
                prototype.States.Add(state);
            foreach (Transition transition in Adapters.AsIEnumerable<Transition>(itemCopies))
                prototype.Transitions.Add(transition);

            PrototypeFolder folder = Adapters.As<PrototypeFolder>(m_activeItem);
            if (folder == null)
                folder = PrototypeFolder;

            folder.Prototypes.Add(prototype);
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

        #region ITreeView Members

        /// <summary>
        /// Gets the root object of the tree view</summary>
        object ITreeView.Root
        {
            get { return PrototypeFolder; }
        }

        /// <summary>
        /// Obtains enumeration of the children of the given parent object</summary>
        /// <param name="parent">Parent object</param>
        /// <returns>Enumeration of children of the parent object</returns>
        IEnumerable<object> ITreeView.GetChildren(object parent)
        {
            PrototypeFolder folder = Adapters.As<PrototypeFolder>(parent);
            if (folder != null)
            {
                foreach (PrototypeFolder childFolder in folder.Folders)
                    yield return childFolder;
                foreach (Prototype prototype in folder.Prototypes)
                    yield return prototype;
            }
        }

        #endregion

        #region IItemView Members

        /// <summary>
        /// Fills in or modifies the given display info for the item</summary>
        /// <param name="item">Item</param>
        /// <param name="info">Display info to update</param>
        public void GetInfo(object item, ItemInfo info)
        {
            PrototypeFolder category = Adapters.As<PrototypeFolder>(item);
            if (category != null)
            {
                info.Label = category.Name;
            }
            else
            {
                Prototype prototype = Adapters.As<Prototype>(item);
                if (prototype != null)
                {
                    info.Label = prototype.Name;
                    info.ImageIndex = info.GetImageList().Images.IndexOfKey(Resources.StatechartDocImage);
                    info.IsLeaf = true;
                }
            }
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
        /// Event that is raised when collection has been reloaded</summary>
        public event EventHandler Reloaded;

        private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
        {
            if (IsPrototypeItem(e.DomNode, e.DomNode.Parent))
                ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
        }

        private void DomNode_ChildInserted(object sender, ChildEventArgs e)
        {
            if (IsPrototypeItem(e.Child, e.Parent))
                ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(e.Index, e.Child, e.Parent));
        }

        private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
        {
            if (IsPrototypeItem(e.Child, e.Parent))
                ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(e.Index, e.Child, e.Parent));
        }

        private static bool IsPrototypeItem(DomNode child, DomNode parent)
        {
            return
                child.Is<PrototypeFolder>() ||
                (parent != null && parent.Is<PrototypeFolder>());
        }

        #endregion

        private object m_activeItem;
    }
}
