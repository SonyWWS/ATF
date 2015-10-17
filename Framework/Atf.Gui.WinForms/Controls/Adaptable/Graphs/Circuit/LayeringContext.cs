//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Context for the LayerLister</summary>
    /// <remarks>This context has its own independent selection, 
    /// but uses the main GameContext's HistoryContext for undo/redo operations.
    /// IInstancingContext and IHierarchicalInsertionContext implementations control drag/drop and 
    /// copy/paste operations within the LayerLister (internal), pastes/drops to the 
    /// LayerLister and drag/copies from the LayerLister (external).
    /// The IObservableContext implementation notifies the LayerLister's TreeControl
    /// when a change occurs that requires an update of one or more tree nodes.
    /// The ITreeView implementation controls the hierarchy in the LayerLister's TreeControl.
    /// The IItemView implementation controls icons and labels in the LayerLister's TreeControl.</remarks>
    public abstract class LayeringContext : SelectionContext,
        IInstancingContext,
        IHierarchicalInsertionContext,
        ILayeringContext, // : IVisibilityContext, ITreeView, IItemView
        IObservableContext,
        INamingContext
    {
        // required  attribute info
        /// <summary>
        /// Gets visible attribute for layer</summary>
        protected abstract AttributeInfo VisibleAttribute { get; }

        // required  child info
        /// <summary>
        /// Gets ChildInfo for layers in circuit</summary>
        protected abstract ChildInfo LayerFolderChildInfo { get; }

        // required  DomNodeType info
        /// <summary>
        /// Gets type of layer folder</summary>
        protected abstract DomNodeType LayerFolderType { get; }
        /// <summary>
        /// Gets type of module reference</summary>
        protected abstract DomNodeType ElementRefType { get; }

        /// <summary>
        /// Performs initialization when the adapter is connected to the layer's DomNode.
        /// Raises the SelectionContext NodeSet event and performs custom processing.</summary>
        protected override void OnNodeSet()
        {
 
            base.OnNodeSet();

            DomNode.AttributeChanged += DomNode_AttributeChanged;
            DomNode.ChildInserted += DomNode_ChildInserted;
            DomNode.ChildRemoved += DomNode_ChildRemoved;

            m_activeItem = this;
        }

        /// <summary>
        /// Gets a list of all layers</summary>
        public IList<LayerFolder> Layers
        {
            get { return GetChildList<LayerFolder>(LayerFolderChildInfo); }
        }

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
            return this.Cast<CircuitEditingContext>().Copy();
        }

        /// <summary>
        /// Returns whether the context can insert the data object in the layer</summary>
        /// <param name="insertingObject">Data to insert in the layer</param>
        /// <returns><c>True</c> if the context can insert the data object in the layer</returns>
        public bool CanInsert(object insertingObject)
        {
            return CanInsert(m_activeItem, insertingObject);
        }

        /// <summary>
        /// Adds the given objects to a layer</summary>
        /// <param name="insertingObject">Object(s) to insert in layer</param>
        public void Insert(object insertingObject)
        {
            Insert(m_activeItem, insertingObject);
        }

        /// <summary>
        /// Returns whether the context can delete the selection from the layer</summary>
        /// <returns><c>True</c> if the context can delete</returns>
        public bool CanDelete()
        {
            foreach (DomNode domNode in GetSelection<DomNode>())
            {
                if (domNode.Parent != null)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Deletes the selection from the layer</summary>
        public void Delete()
        {            
            IEnumerable<DomNode> selectedDomNodes = GetSelection<DomNode>();
            foreach(DomNode domNode in DomNode.GetRoots(selectedDomNodes))
            {
                SetVisible(domNode, true);
                domNode.RemoveFromParent();
            }
        }

        #endregion

        #region IHierarchicalInsertionContext Members

        /// <summary>
        /// Returns true if context can insert the child object</summary>
        /// <param name="parent">The proposed parent of the object to insert</param>
        /// <param name="insertingObject">Child to insert</param>
        /// <returns><c>True</c> if the context can insert the child</returns>
        public bool CanInsert(object parent, object insertingObject)
        {
            if (parent == null)
                parent = m_activeItem ?? this;
            else if (!(parent is LayeringContext) && !(parent is LayerFolder))
                return false;

            IEnumerable<DomNode> nodes = GetCompatibleNodes((IDataObject)insertingObject);
            if (nodes == null)
                return false;

            DomNode insertionPoint = parent.As<DomNode>();
            if (insertionPoint == null)
                return false;

            foreach (DomNode node in nodes)
            {
                // Avoid inserting objects that are not part of the current document
                if (!IsLayerItem(node) || DomNode == null || node.GetRoot() != DomNode.GetRoot())
                    return false;

                // Don't allow parenting cycles
                foreach (DomNode ancestor in insertionPoint.Lineage)
                    if (node.Equals(ancestor))
                        return false;

                // Don't reparent to same parent
                if (insertionPoint != DomNode && node.Parent == insertionPoint)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Adds the given objects to a layer using a transaction; entry point for automation. If a new layer
        /// is created, returns that new layer. Otherwise returns null.</summary>
        /// <param name="parent">Layer's parent</param>
        /// <param name="objectToInsert">Object(s) to insert in layer</param>
        /// <returns>New layer if created, null otherwise</returns>
        public LayerFolder InsertAuto(object parent, object objectToInsert)
        {
            DataObject dataObject = null;
            if (objectToInsert is IEnumerable)
            {
                //if the input is a native C# IEnumerable, such as passing in circuit.Connections,
                //manaully convert the IEnumerable into an object array
                List<object> tempArray = new List<object>();
                foreach (object obj in (IEnumerable)objectToInsert)
                {
                    tempArray.Add(obj);
                }
                dataObject = new DataObject(tempArray.ToArray());
            }
            else
            {
                //if the input is a single object, put the single object into an array
                dataObject = new DataObject(new object[] {objectToInsert});
            }

            int cntBefore = Layers.Count;
            ITransactionContext transactionContext = this.As<ITransactionContext>();
            transactionContext.DoTransaction(
                delegate
                {
                    Insert(parent, dataObject);
                }, "Scripted Insert Layer");
            int cntAfter = Layers.Count;

            LayerFolder newLayer = null;
            if (cntAfter > cntBefore)
                newLayer = Layers[cntAfter - 1];

            return newLayer;
        }

        /// <summary>
        /// Adds the given objects to a layer</summary>
        /// <param name="parent">Layer's parent</param>
        /// <param name="insertingObject">Object(s) to insert in layer</param>
        public void Insert(object parent, object insertingObject)
        {
            if (parent == null)
                parent = m_activeItem ?? this;
            else if (!(parent is LayeringContext) && !(parent is LayerFolder))
                return;

            IEnumerable<DomNode> nodes = GetCompatibleNodes((IDataObject)insertingObject);
            if (nodes == null)
                return;

            // If the parent is the LayeringContext itself:
            // create a new layer for the objects about to be inserted
            LayerFolder layer = parent.As<LayerFolder>();
            if (layer == null && nodes.Any())
            {
                DomNode node = new DomNode(LayerFolderType);
                LayerFolder subLayer = node.As<LayerFolder>();
                subLayer.Name = "New Layer".Localize();
                Layers.Add(subLayer);
                layer = subLayer;
            }
            if (layer == null)
                return;

            foreach (DomNode node in nodes)
            {
                // Insert GameObjects
                Element gameObject = node.As<Element>();
                if (gameObject != null && !layer.Contains(gameObject))
                {
                    DomNode refNode = new DomNode(ElementRefType);
                    ElementRef newRef = refNode.As<ElementRef>();
                    newRef.Element = gameObject;
                    layer.ElementRefs.Add(newRef);
                }

                // Insert References
                ElementRef reference = node.As<ElementRef>();
                if (reference != null)
                {
                    if (reference.Element != null && !layer.Contains(reference.Element))
                        layer.ElementRefs.Add(reference);
                }

                // Insert Sub-Layers
                LayerFolder otherLayer = node.As<LayerFolder>();
                if (otherLayer != null)
                    layer.Folders.Add(otherLayer);
            }
        }

        private static IEnumerable<DomNode> GetCompatibleNodes(IDataObject dataObject)
        {
            IList<DomNode> nodes = new List<DomNode>();
            IEnumerable<object> items = dataObject.GetData(typeof(object[])) as object[];
            if (items != null)
            {
                foreach (object item in items)
                    if (item.Is<Element>() || item.Is<ElementRef>() || item.Is<LayerFolder>())
                        nodes.Add(item.Cast<DomNode>());
            }
            return nodes;
        }

        #endregion

        #region ILayeringContext Members

        /// <summary>
        /// Sets the active item in the layering context; used by UI components to
        /// set insertion point as the user selects and edits</summary>
        /// <param name="item">Active layer or item</param>
        public void SetActiveItem(object item)
        {
            m_activeItem = item;
        }

        #endregion

        #region ITreeView Members

        /// <summary>
        /// Gets the root object of the tree view</summary>
        object ITreeView.Root
        {
            get { return this; }
        }

        /// <summary>
        /// Obtains enumeration of the children of the given parent object</summary>
        /// <param name="parent">Parent object</param>
        /// <returns>Enumeration of children of the parent object</returns>
        IEnumerable<object> ITreeView.GetChildren(object parent)
        {
            LayerFolder layer = parent.As<LayerFolder>();
            if (layer != null)
            {
                foreach (LayerFolder subLayer in layer.Folders)
                    yield return subLayer;
                foreach (ElementRef reference in layer.ElementRefs)
                    yield return reference;
            }
            else if (parent.Is<LayeringContext>())
            {
                foreach (LayerFolder childLayer in Layers)
                    yield return childLayer;
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
            LayerFolder layer = item.As<LayerFolder>();
            if (layer != null)
            {
                info.Label = layer.Name;
                info.HasCheck = true;
                info.SetCheckState(GetCheckState(layer));
                return;
            }

            ElementRef reference = item.As<ElementRef>();
            if (reference != null)
            {
                Element element = reference.Element;
                if (element != null)
                {
                    info.Label = element.Id;
                    info.IsLeaf = true;
                }

                IVisible iVisible = GetIVisible(item);
                if (iVisible != null)
                {
                    info.HasCheck = true;
                    info.Checked = iVisible.Visible;
                }
                return;
            }
        }

        private static CheckState GetCheckState(LayerFolder layer)
        {
            bool hasUncheckedChild = false;
            bool hasCheckedChild = false;

            foreach (LayerFolder subLayer in layer.Folders)
            {
                CheckState subCheckState = GetCheckState(subLayer);
                switch (subCheckState)
                {
                    case CheckState.Checked:
                        hasCheckedChild = true;
                        break;
                    case CheckState.Unchecked:
                        hasUncheckedChild = true;
                        break;
                    case CheckState.Indeterminate:
                        hasCheckedChild = true;
                        hasUncheckedChild = true;
                        break;
                }
            }

            foreach (Element gameObject in layer.GetElements())
            {
                IVisible iVisible = gameObject.As<IVisible>();
                if (iVisible != null)
                {
                    if (iVisible.Visible)
                        hasCheckedChild = true;
                    else
                        hasUncheckedChild = true;
                }
            }

            if (hasCheckedChild && !hasUncheckedChild)
                return CheckState.Checked;
            if (hasUncheckedChild && !hasCheckedChild)
                return CheckState.Unchecked;
            return CheckState.Indeterminate;
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
        /// Event that is raised when collection has been reloaded. Never raised.</summary>
        public event EventHandler Reloaded // Never raised
        {
            add { }
            remove { }
        }

        /// <summary>
        /// Performs custom actions on AttributeChanged events</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">AttributeEventArgs containing event data</param>
        void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
        {
            // Always use the Equivalent method when comparing two AttributeInfos
            // because using simple equality (==) doesn't respect inheritance
            // i.e. (Schema.baseType.someAttribute == Schema.derivedType.someAttribute) returns false
            // whereas (Schema.baseType.someAttribue.Equivalent(Schema.derivedType.someAttribute)) returns true
            if (e.AttributeInfo.Equivalent(VisibleAttribute))
            {
                // Update the checkbox for the current object
                ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));

                // Update checkboxes for all layers that may contain the current object
                foreach (DomNode domNode in DomNode.Subtree)
                    if (IsLayerItem(domNode))
                        ItemChanged.Raise(this, new ItemChangedEventArgs<object>(domNode));
            }
        }

        /// <summary>
        /// Performs custom actions on ChildInserted events</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">ChildEventArgs containing event data</param>
        void DomNode_ChildInserted(object sender, ChildEventArgs e)
        {
            if (IsLayerItem(e.Child))
                ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(e.Index, e.Child, e.Parent));
        }

        /// <summary>
        /// Performs custom actions on ChildRemoved events</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">ChildEventArgs containing event data</param>
        void DomNode_ChildRemoved(object sender, ChildEventArgs e)
        {
            if (IsLayerItem(e.Child))
                ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(e.Index, e.Child, e.Parent));
        }

        #endregion

        #region IVisibilityContext Members

        /// <summary>
        /// Returns whether the item is visible</summary>
        /// <param name="item">Item</param>
        /// <returns><c>True</c> if the item is visible</returns>
        public bool IsVisible(object item)
        {
            LayerFolder layer = item.As<LayerFolder>();
            return (layer == null || GetCheckState(layer) == CheckState.Checked);
        }

        /// <summary>
        /// Returns whether the item can be made visible and invisible</summary>
        /// <param name="item">Item</param>
        /// <returns><c>True</c> if the item can be made visible and invisible</returns>
        public bool CanSetVisible(object item)
        {
            return item.Is<LayerFolder>() || item.Is<IVisible>();
        }

        /// <summary>
        /// Sets the visibility state of the item to the value</summary>
        /// <param name="item">Item to show or hide</param>
        /// <param name="value">True to show, false to hide</param>
        public void SetVisible(object item, bool value)
        {
            LayerFolder layer = item.As<LayerFolder>();
            if (layer != null)
                PropagateVisible(layer, value);
            else
            {
                IVisible iVisible = GetIVisible(item);
                if (iVisible != null)
                    iVisible.Visible = value;
            }
        }

        private static void PropagateVisible(LayerFolder layer, bool visible)
        {
            // Recursive call to update all sub-layers
            foreach (LayerFolder subLayer in layer.Folders)
                PropagateVisible(subLayer, visible);

            // Set visibility for all GameObjects
            foreach (Element gameObject in layer.GetElements())
            {
                IVisible iVisible = gameObject.As<IVisible>();
                if (iVisible != null)
                    iVisible.Visible = visible;
            }
        }

        private static IVisible GetIVisible(object item)
        {
            ElementRef reference = item.As<ElementRef>();
            if (reference != null)
                return reference.Element.As<IVisible>();
            return item.As<IVisible>();
        }

        #endregion

        #region INamingContext Members

        /// <summary>
        /// Gets the item's name in the context, or null if none</summary>
        /// <param name="item">Item</param>
        /// <returns>Item's name in the context, or null if none</returns>
        public string GetName(object item)
        {
            LayerFolder layerFolder = item.As<LayerFolder>();
            if (layerFolder != null)
                return layerFolder.Name;

            return null;
        }

        /// <summary>
        /// Returns whether the item can be named</summary>
        /// <param name="item">Item to name</param>
        /// <returns><c>True</c> if the item can be named</returns>
        public bool CanSetName(object item)
        {
            return item.Is<LayerFolder>();
        }

        /// <summary>
        /// Sets the item's name</summary>
        /// <param name="item">Item to name</param>
        /// <param name="name">New item name</param>
        public void SetName(object item, string name)
        {
            LayerFolder layerFolder = item.As<LayerFolder>();
            if (layerFolder != null)
                layerFolder.Name = name;
        }

        #endregion

        private static bool IsLayerItem(DomNode node)
        {
            return node.Is<LayerFolder>() || node.Is<ElementRef>() || node.Is<Element>();
        }

        private object m_activeItem;
    }
}
