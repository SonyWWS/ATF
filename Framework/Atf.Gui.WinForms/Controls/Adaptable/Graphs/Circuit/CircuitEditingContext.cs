//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;


using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Class that defines a circuit editing context. Each context represents a circuit,
    /// with a history, selection, and editing capabilities. There may be multiple
    /// contexts within a single circuit document, because each sub-circuit has its own
    /// editing context.</summary>
    /// <remarks>The class has a default implementation of IInstancingContext if no other 
    /// IInstancingContext adaptors are associated with the DomNode. </remarks>
    public abstract class CircuitEditingContext : EditingContext,
        IEnumerableContext,
        INamingContext,
        IInstancingContext,
        IObservableContext,
        IColoringContext,
        IEditableGraphContainer<Element, Wire, ICircuitPin>
    {
        // required  DomNodeType info
        /// <summary>
        /// Gets type for Wire</summary>
        protected abstract DomNodeType WireType { get; }

        // callbacks needed for container-crossing elements moving (IEditableGraphContainer)
        /// <summary>
        /// Callback to get bounding rectangle for item in graph space</summary>
        public Func<AdaptableControl, Element, RectangleF> GetLocalBound;
        /// <summary>
        /// Callback to get drawing offset for graph path in graph space</summary>
        public Func<AdaptableControl, IEnumerable<Element>, Point> GetWorldOffset;
        /// <summary>
        /// Callback to get title height at top of element</summary>
        public Func<AdaptableControl, int> GetTitleHeight;
        /// <summary>
        /// Callback to get label height at bottom of element</summary>
        public Func<AdaptableControl, int> GetLabelHeight;
        /// <summary>
        /// Callback to get offset to be added to draw all sub-elements when group is expanded inline</summary>
        public Func<AdaptableControl, Point> GetSubContentOffset;

        private enum MoveElementBehavior
        {
            MoveConstrainToCursorContainment, // an element is eligible to move into the new container if current cursor position is contained by the new container
            MoveConstrainToContainerBounds, // an element is eligible to move into the new container only its bound are completely contained by the new container
        }

        /// <summary>
        /// Performs initialization when the adapter is connected to the editing context's DomNode.
        /// Raises the EditingContext NodeSet event and performs custom processing.</summary>
        protected override void OnNodeSet()
        {
            // The HistoryContext should subscribe to DOM events as soon as possible,
            //  because it is read-only in response to the DOM events whereas we may modify
            //  the DOM, which can cause change events to be re-ordered for subsequent listeners.
            base.OnNodeSet();

            // fall back to the default implementation if no other IInstancingContext defined
            m_instancingContext = DomNode.Type.AsAll<IInstancingContext>().FirstOrDefault(x => x != this);
            m_templatingContext = DomNode.As<ITemplatingContext>();
            m_circuitContainer = DomNode.Cast<ICircuitContainer>();
            m_viewingContext = DomNode.Cast<IViewingContext>();

            DomNode.AttributeChanged += DomNode_AttributeChanged;
            DomNode.ChildInserted += DomNode_ChildInserted;
            DomNode.ChildRemoved += DomNode_ChildRemoved;

            foreach (DomNode node in DomNode.Subtree)
                AddNode(node);
        }


        private void AddNode(DomNode node)
        {
            if (node.Is<Group>())
            {
                node.Cast<Group>().Changed += GroupChanged;
            }
        }

        private void RemoveNode(DomNode node)
        {
            if (node.Is<Group>())
            {
                node.Cast<Group>().Changed -= GroupChanged;
            }
        }

        /// <summary>
        /// Gets the Circuit</summary>
        public ICircuitContainer CircuitContainer
        {
            get { return m_circuitContainer; }
        }

        /// <summary>
        /// Gets or sets the schema loader</summary>
        public XmlSchemaTypeLoader SchemaLoader
        {
            get { return m_schemaLoader; }
            set { m_schemaLoader = value; }
        }

        /// <summary>
        /// Gets or sets whether the editing context supports nested group</summary>
        public bool SupportsNestedGroup
        {
            get { return m_supportsNestedGroup; }
            set { m_supportsNestedGroup = value; }
        }

        /// <summary>
        /// Circuit clipboard data type</summary>
        public static string CircuitFormat { get; set; }

        #region IEnumerableContext Members

        /// <summary>
        /// Gets an enumeration of all of the items of this context</summary>
        IEnumerable<object> IEnumerableContext.Items
        {
            get
            {
                foreach (Element module in CircuitContainer.Elements)
                    yield return module;
                foreach (Wire connection in CircuitContainer.Wires)
                    yield return connection;
                if (CircuitContainer.Annotations != null)
                {
                    foreach (Annotation annotation in CircuitContainer.Annotations)
                        yield return annotation;
                }
            }
        }

        #endregion

        #region INamingContext Members

        /// <summary>
        /// Gets the item's name in the context, or null if none</summary>
        /// <param name="item">Item</param>
        /// <returns>Item's name in the context, or null if none</returns>
        string INamingContext.GetName(object item)
        {
            Element element = item.As<Element>();
            if (element != null)
                return element.Name;

            Wire wire = item.As<Wire>();
            if (wire != null)
                return wire.Label;

            var groupPin = item.As<GroupPin>();
            if (groupPin != null)
                return groupPin.Name;
            return null;
        }

        /// <summary>
        /// Returns whether the item can be named</summary>
        /// <param name="item">Item to name</param>
        /// <returns>True iff the item can be named</returns>
        bool INamingContext.CanSetName(object item)
        {
            return
                item.Is<Element>() ||
                item.Is<Wire>() ||
                item.Is<GroupPin>();
        }

        /// <summary>
        /// Sets the item's name</summary>
        /// <param name="item">Item to name</param>
        /// <param name="name">New item name</param>
        void INamingContext.SetName(object item, string name)
        {
            Element element = item.As<Element>();
            if (element != null)
            {
                element.Name = name;
                return;
            }

            Wire wire = item.As<Wire>();
            if (wire != null)
            {
                wire.Label = name;
                return;
            }


            var groupPin = item.As<GroupPin>();
            if (groupPin != null)
            {
                groupPin.Name = name;
                return;
            }
        }

        #endregion

        #region IInstancingContext Members

        /// <summary>
        /// Tests if can copy selection from the circuit</summary>
        /// <returns>True iff there are items to copy</returns>
        public virtual bool CanCopy()
        {
            if (m_instancingContext != null)
                return m_instancingContext.CanCopy();
            return Selection.Count > 0;
        }

        /// <summary>
        /// Copies selected items from the circuit</summary>
        /// <returns>DataObject containing an enumeration of selected items</returns>
        public virtual object Copy()
        {
            // copy selected text in the selected annotation?
            var annotationAdapter = m_viewingContext.Cast<AdaptableControl>().As<D2dAnnotationAdapter>();
            if (annotationAdapter != null)
            {
                foreach (Annotation annotation in Selection.AsIEnumerable<Annotation>())
                {
                    if (annotationAdapter.CanCopyText(annotation))
                    {

                        DataObject dataObjectText = new DataObject();
                        dataObjectText.SetText(annotationAdapter.TextSelected(annotation));
                        Clipboard.SetText(annotationAdapter.TextSelected(annotation));
                        return dataObjectText;
                    }

                }
            }

            if (m_instancingContext != null)
                return m_instancingContext.Copy();

            // get the selected modules
            HashSet<Element> modules = new HashSet<Element>(Selection.AsIEnumerable<Element>());
            HashSet<object> itemsToCopy = new HashSet<object>(modules.AsIEnumerable<object>());

            // get selected connections between selected modules
            foreach (Wire connection in Selection.AsIEnumerable<Wire>())
                if (IsConnectionCopyable(connection, modules))
                    itemsToCopy.Add(connection);

            // if no connections were added, look for copyable connections between the selected elements
            if (itemsToCopy.Count == modules.Count)
            {
                foreach (Wire connection in m_circuitContainer.Wires)
                    if (IsConnectionCopyable(connection, modules))
                        itemsToCopy.Add(connection);
            }

            // add annotations
            foreach (Annotation annotation in Selection.AsIEnumerable<Annotation>())
                itemsToCopy.Add(annotation.As<DomNode>());

            // create format for local use
            DataObject dataObject = new DataObject(itemsToCopy.ToArray());

            // add a serializable format for the system clipboard
            DomNodeSerializer serializer = new DomNodeSerializer();
            byte[] data = serializer.Serialize(itemsToCopy.AsIEnumerable<DomNode>());
            dataObject.SetData(CircuitFormat, data);

            return dataObject;
        }

        /// <summary>
        /// Tests if can insert a given object into the circuit</summary>
        /// <param name="insertingObject">Object to insert</param>
        /// <returns>True iff can insert object into the circuit</returns>
        public virtual bool CanInsert(object insertingObject)
        {
            // pasted selected text in the selected annotation?
            var annotationAdapter = m_viewingContext.Cast<AdaptableControl>().As<D2dAnnotationAdapter>();
            if (annotationAdapter != null)
            {
                foreach (Annotation annotation in Selection.AsIEnumerable<Annotation>())
                {
                    if (annotationAdapter.CanInsertText(annotation))
                    {
                        return true;
                    }

                }
            }

            if (m_instancingContext != null)
                return m_instancingContext.CanInsert(insertingObject);

            IDataObject dataObject = (IDataObject)insertingObject;
            IEnumerable<object> items = GetCompatibleData(dataObject);
            return items != null;
        }

        /// <summary>
        /// Inserts object into circuit at the center of the canvas</summary>
        /// <param name="insertingObject">Object to insert</param>
        public virtual void Insert(object insertingObject)
        {
            // paste selected text in the selected annotation?
            var annotationAdapter = m_viewingContext.Cast<AdaptableControl>().As<D2dAnnotationAdapter>();
            if (annotationAdapter != null)
            {
                foreach (Annotation annotation in Selection.AsIEnumerable<Annotation>())
                {
                    if (annotationAdapter.CanInsertText(annotation))
                    {
                        annotationAdapter.PasteFromClipboard(annotation);
                        return;
                    }
                }
            }

            if (m_instancingContext != null)
            {
                m_instancingContext.Insert(insertingObject);
                return;
            }

            var control = m_viewingContext.As<AdaptableControl>();
            Point center = new Point(control.Width / 2, control.Height / 2);
            var dragDropAdapter = control.As<DragDropAdapter>();
            Group hitGroup = null;
            if (dragDropAdapter != null && dragDropAdapter.IsDropping)
            {
                center = dragDropAdapter.MousePosition;
                var hitRecord = Pick(dragDropAdapter.MousePosition);
                if (hitRecord != null) 
                {
                    if (hitRecord.SubItem.Is<Group>())
                        hitGroup = hitRecord.SubItem.Cast<Group>();
                    else if (hitRecord.Item.Is<Group>())
                        hitGroup = hitRecord.Item.Cast<Group>();
                }
            }

            var insertedItems = Insert(insertingObject, center);

            if (hitGroup != null && insertedItems != null) //drop onto a group
            {
                var self = (IEditableGraphContainer<Element, Wire, ICircuitPin>)this;
                self.Move(hitGroup, insertedItems);
            }
        }

        /// <summary>
        /// Tests if can delete selected items from the circuit</summary>
        /// <returns>True iff can delete selected items from the circuit</returns>
        public virtual bool CanDelete()
        {
            if (m_instancingContext != null)
                return m_instancingContext.CanDelete();
            return Selection.Count > 0;
        }

        /// <summary>
        /// Deletes selected items from the circuit</summary>
        public virtual void Delete()
        {
            if (m_instancingContext != null)
            {
                m_instancingContext.Delete();
                return;
            }
            foreach (DomNode node in Selection.AsIEnumerable<DomNode>())
            {
                if (node.Is<IAnnotation>())
                {
                    // annotation editing takes priority over standard node editing 
                    var annotationAdapter = m_viewingContext.As<AdaptableControl>().As<D2dAnnotationAdapter>();
                    if (annotationAdapter != null && annotationAdapter.CanDeleteTextSelection(node.Cast<IAnnotation>()))
                    {
                        annotationAdapter.DeleteTextSelection(node.Cast<IAnnotation>());
                        return;
                    }

                }
                node.RemoveFromParent();
            }

            Selection.Clear();
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
        public event EventHandler Reloaded
        {
            add { }
            remove { }
        }

        /// <summary>
        /// Raises the ItemInserted event and performs custom processing</summary>
        /// <param name="e">ItemInsertedEventArgs containing event data</param>
        protected virtual void OnObjectInserted(ItemInsertedEventArgs<object> e)
        {
            ItemInserted.Raise(this, e);
        }

        /// <summary>
        /// Raises the ItemRemoved event and performs custom processing</summary>
        /// <param name="e">ItemRemovedEventArgs containing event data</param>
        protected virtual void OnObjectRemoved(ItemRemovedEventArgs<object> e)
        {
            ItemRemoved.Raise(this, e);
        }

        /// <summary>
        /// Raises the ItemChanged event and performs custom processing</summary>
        /// <param name="e">ItemChangedEventArgs containing event data</param>
        protected virtual void OnObjectChanged(ItemChangedEventArgs<object> e)
        {
            ItemChanged.Raise(this, e);
        }

        /// <summary>
        /// Explicitly notify the graph object has been changed. This is useful when some changes, 
        /// such as group pin connectivity, are computed at runtime, outside DOM attribute mechanism.</summary>
        /// <param name="element">Changed graph object</param>
        public void NotifyObjectChanged(object element)
        {
            OnObjectChanged(new ItemChangedEventArgs<object>(element));
        }

        private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
        {
            if (IsCircuitItem(e.DomNode, e.DomNode.Parent))
            {
                NotifyObjectChanged(e.DomNode); //required for Layers. http://tracker.ship.scea.com/jira/browse/WWSATF-1389

                // Editing the subgraph may cause changes in the parent graph, such as reordering group pins in a group needs 
                // to change the pin indexes of the external edges in the parent graph. 
                // Each circuit or group has its own local editing context, and the default TransactionContext implementation 
                // only responds to these Dom changes from the adapted DomNode and below. 
                // We need to catch changes up in the hierarchy too for proper undo/redo.
                var circuitValidator = DomNode.GetRoot().As<CircuitValidator>();
                if (circuitValidator != null)
                {

                    var transactionContext = circuitValidator.ActiveHistoryContext;
                    if (transactionContext != null && transactionContext != this)
                    {
                        if (transactionContext.DomNode.Ancestry.FirstOrDefault(x => x == e.DomNode.Parent) != null)
                        {
                            if (transactionContext.InTransaction)
                            {
                                transactionContext.AddOperation(new AttributeChangedOperation(e));
#if DEBUG_VERBOSE
                            var parent = transactionContext.DomNode.Ancestry.FirstOrDefault(x => x == e.DomNode.Parent);
                            Trace.TraceInformation("PARENT GRAPH {0} element  {1} -- Attribute {2} changed from  {3} to {4}",
                            CircuitUtil.GetDomNodeName(parent), CircuitUtil.GetDomNodeName(e.DomNode), e.AttributeInfo.Name, e.OldValue, e.NewValue);
#endif
                            }
                        }
                    }
                }
            }
        }

        private void DomNode_ChildInserted(object sender, ChildEventArgs e)
        {
            AddNode(e.Child);
            if (IsCircuitItem(e.Child, e.Parent))
            {
                if (e.Child.Is<Wire>())
                {
                    var connection = e.Child.Cast<Wire>();
                    if (connection.InputElement.Is<Group>())  // set dirty to force update group pin connectivity                                     
                        connection.InputElement.Cast<Group>().Dirty = true;

                    if (connection.OutputElement.Is<Group>())
                        connection.OutputElement.Cast<Group>().Dirty = true;
                }

                OnObjectInserted(new ItemInsertedEventArgs<object>(e.Index, e.Child, e.Parent));

                var circuitValidator = DomNode.GetRoot().As<CircuitValidator>();
                if (circuitValidator != null)
                {
                    var transactionContext = circuitValidator.ActiveHistoryContext;
                    if (transactionContext != null && transactionContext != this)
                    {
                        if (transactionContext.DomNode.Ancestry.FirstOrDefault(x => x == e.Parent) != null)
                        {
                            if (transactionContext.InTransaction)
                            {
                                transactionContext.AddOperation(new ChildInsertedOperation(e));
#if DEBUG_VERBOSE
                            var parent = transactionContext.DomNode.Ancestry.FirstOrDefault(x => x == e.Parent);
                            Trace.TraceInformation("PARENT GRAPH {0} --  Added {1} to parent {2}",
                                  CircuitUtil.GetDomNodeName(parent), CircuitUtil.GetDomNodeName(e.Child), CircuitUtil.GetDomNodeName(e.Parent));

#endif
                            }
                        }
                    }
                }
            }

        }

        private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
        {
            RemoveNode(e.Child);
            if (IsCircuitItem(e.Child, e.Parent))
            {
                if (e.Child.Is<Wire>())
                {
                    var connection = e.Child.Cast<Wire>();
                    if (connection.InputElement.Is<Group>())
                        connection.InputElement.Cast<Group>().Dirty = true;
                    if (connection.OutputElement.Is<Group>())
                        connection.OutputElement.Cast<Group>().Dirty = true;
                }

                OnObjectRemoved(new ItemRemovedEventArgs<object>(e.Index, e.Child, e.Parent));

                var circuitValidator = DomNode.GetRoot().As<CircuitValidator>();
                if (circuitValidator != null)
                {
                    var transactionContext = circuitValidator.ActiveHistoryContext;
                    if (transactionContext != null && transactionContext != this)
                    {
                        if (transactionContext.DomNode.Ancestry.FirstOrDefault(x => x == e.Parent) != null)
                        {
                            if (transactionContext.InTransaction)
                            {
                                transactionContext.AddOperation(new ChildRemovedOperation(e));
#if DEBUG_VERBOSE
                            var parent = transactionContext.DomNode.Ancestry.FirstOrDefault(x => x == e.Parent);
                            Trace.TraceInformation("PARENT GRAPH {0} --  Removed {1}  from parent {2}", CircuitUtil.GetDomNodeName(parent),
                                                         CircuitUtil.GetDomNodeName(e.Child), CircuitUtil.GetDomNodeName(e.Parent));

#endif
                            }
                        }
                    }
                }
            }

        }

        private static bool IsCircuitItem(DomNode child, DomNode parent)
        {
            if (parent == null)
                return false;

            while (parent != null &&
                parent.Is<LayerFolder>())
            {
                parent = parent.Parent;
            }

            // Dynamic properties, a feature in the circuit editor sample app, have a parent that is an Element.
            return
               child.Is<Group>() || parent.Is<Circuit>() || parent.Is<Group>() || parent.Is<Element>();
        }


        private void GroupChanged(object sender, EventArgs eventArgs)
        {

            var group = sender.Cast<Group>();
            // Some group properties, such as its display bound, are computed at runtime but not stored as Dom Attributes.  
            // Since group elements could be nested and child changes could affect the appearance of the parent group,  
            // need to bubble up the ItemChanged event by traversing up the editing context chain 
            foreach (var node in group.DomNode.Lineage)
            {
                var editingContext = node.As<CircuitEditingContext>();
                if (editingContext != null)
                    editingContext.ItemChanged.Raise(this, new ItemChangedEventArgs<object>(group.DomNode));
            }

        }

        #endregion

        #region IColoringContext Members

        /// <summary>
        /// Gets the item's specified color in the context</summary>
        /// <param name="kind">Coloring type</param>
        /// <param name="item">Item</param>
        Color IColoringContext.GetColor(ColoringTypes kind, object item)
        {
            if (item.Is<Annotation>())
            {
                if (kind == ColoringTypes.BackColor)
                    return item.Cast<Annotation>().BackColor;
                if (kind == ColoringTypes.ForeColor)
                    return item.Cast<Annotation>().ForeColor;
            }

            return s_zeroColor;
        }

        /// <summary>
        /// Returns whether the item can be colored</summary>
        /// <param name="kind">Coloring type</param>
        /// <param name="item">Item to color</param>
        /// <returns>True iff the item can be colored</returns>
        bool IColoringContext.CanSetColor(ColoringTypes kind, object item)
        {
            if (item.Is<Annotation>())
            {
                if (kind == ColoringTypes.BackColor)
                    return true;
                if (kind == ColoringTypes.ForeColor)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the item's color</summary>
        /// <param name="kind">Coloring type</param>
        /// <param name="item">Item to name</param>
        /// <param name="newValue">Item new color</param>
        void IColoringContext.SetColor(ColoringTypes kind, object item, Color newValue)
        {
            if (item.Is<Annotation>())
            {
                if (kind == ColoringTypes.BackColor)
                    item.Cast<Annotation>().BackColor = newValue;
                else if (kind == ColoringTypes.ForeColor)
                    item.Cast<Annotation>().ForeColor = newValue;
            }
        }

        #endregion


        #region IEditableGraph<Module, Connection, ICircuitPin> Members

        /// <summary>
        /// Returns whether the given module's two pins can be connected. "from" and "to" refer to the corresponding
        /// properties in IGraphEdge, not to a dragging operation, for example.</summary>
        /// <param name="fromNode">"From" node</param>
        /// <param name="outputPin">Output pin</param>
        /// <param name="toNode">"To" node</param>
        /// <param name="inputPin">Input pin</param>
        /// <returns>Whether the "from" node pin can be connected to the "to" node pin</returns>
        public virtual bool CanConnect(
            Element fromNode, ICircuitPin outputPin, Element toNode, ICircuitPin inputPin)
        {
            if (fromNode == null ||
                outputPin == null ||
                toNode == null ||
                inputPin == null ||
                outputPin.TypeName != inputPin.TypeName)
                return false;

            if (fromNode.HasOutputPin(outputPin) && toNode.HasInputPin(inputPin))
                return true;

            if (fromNode.HasInputPin(outputPin) && toNode.HasOutputPin(inputPin))
                return true;

            return false;
        }

        /// <summary>
        /// Connects the "from" module's pin to the "to" module's pin by creating an IGraphEdge whose
        /// "from" node is "fromNode", "to" node is "toNode", etc.</summary>
        /// <param name="fromNode">"From" module</param>
        /// <param name="fromRoute">"From" pin</param>
        /// <param name="toNode">"To" module</param>
        /// <param name="toRoute">"To" pin</param>
        /// <param name="existingEdge">Existing connection that is being reconnected, or null if new connection</param>
        /// <returns>New connection from the "from" module's pin to the "to" module's pin</returns>
        public virtual Wire Connect(
            Element fromNode, ICircuitPin fromRoute, Element toNode, ICircuitPin toRoute, Wire existingEdge)
        {
            var domNode = new DomNode(WireType);

            var wire = domNode.As<Wire>();
            wire.OutputElement = fromNode;
            wire.OutputPin = fromRoute;
            wire.InputElement = toNode;
            wire.InputPin = toRoute;

            wire.SetPinTarget();

            if (existingEdge != null)
            {
                wire.Label = existingEdge.Label;
            }

            m_circuitContainer.Wires.Add(wire);

            // observe fan in/out constraints
            if (!fromRoute.AllowFanOut || !toRoute.AllowFanIn)
            {
                var wires = m_circuitContainer.Wires.ToArray();
                if (!fromRoute.AllowFanOut)
                {
                    // remove other edges from the route
                    foreach (var edge in wires)
                    {
                        if (edge == wire)
                            continue;
                        if (edge.OutputPin == fromRoute && edge.OutputElement == wire.OutputElement)
                            m_circuitContainer.Wires.Remove(edge);
                    }
                }
                if (!toRoute.AllowFanIn)
                {
                    // remove other edges to the route
                    foreach (var edge in wires)
                    {
                        if (edge == wire)
                            continue;
                         if (edge.InputPin == toRoute && edge.InputElement == wire.InputElement)
                            m_circuitContainer.Wires.Remove(edge);
                    }
                }
            }
            return wire;
        }

        /// <summary>
        /// Gets whether the Connection can be disconnected</summary>
        /// <param name="edge">Connection to disconnect</param>
        /// <returns>Whether the Connection can be disconnected</returns>
        public virtual bool CanDisconnect(Wire edge)
        {
            return true;
        }

        /// <summary>
        /// Disconnects the Connection</summary>
        /// <param name="edge">Connection to disconnect</param>
        public virtual void Disconnect(Wire edge)
        {
            m_circuitContainer.Wires.Remove(edge);
        }

        #endregion

        #region IEditableGraphContainer<Module, Connection, ICircuitPin> Members

        /// <summary>
        /// Can given modules be moved into a new container</summary>
        /// <param name="newParent">New module parent</param>
        /// <param name="movingObjects">Objects being moved</param>
        /// <returns>True iff objects can be moved to new parent</returns>
        bool IEditableGraphContainer<Element, Wire, ICircuitPin>.CanMove(object newParent, IEnumerable<object> movingObjects)
        {
            if (newParent == null)
                newParent = this;

            if (!newParent.Is<ICircuitContainer>())
                return false;

            var newContainer = newParent.Cast<ICircuitContainer>();

            // do not allow moving into a collapsed container that is a child of the current editing context
            if (newContainer != m_circuitContainer && !newContainer.Expanded)
                return false;

            var modules = movingObjects.AsIEnumerable<Element>();
            if (!modules.Any())
                return false;

            bool moveIntoGroup = newParent.Is<Group>();
            foreach (var module in modules)
            {
                // avoid move in objects that are not part of the current document
                if (module.DomNode.GetRoot() != DomNode.GetRoot())
                    return false;

                // don't allow parenting cycles( the module to be moved is not a parent of the new owner) 
                foreach (DomNode ancestor in newContainer.Cast<DomNode>().Lineage)
                    if (module.DomNode.Equals(ancestor))
                        return false;

                // don't re-parent to same parent
                if (module.DomNode.Parent == newContainer.Cast<DomNode>())
                    return false;

                if (IsSelfContainedOrIntersected(module.DomNode, newContainer.Cast<DomNode>()))
                    return false;

                if (!IsContained(module.DomNode, newContainer.Cast<DomNode>()))
                    return false;

                if (!SupportsNestedGroup && moveIntoGroup) // don't allow moving groups into a group
                    if (module.Is<Group>())
                        return false;
            }

            // probably should limit all moving nodes belong to the same parent(container)
            return modules.Skip(1).All(x => x.DomNode.Parent == modules.First().DomNode.Parent);

        }

        /// <summary>
        /// Moves the given nodes into a container</summary>
        /// <param name="newParent">New container</param>
        /// <param name="movingObjects">Nodes to move</param>
        void IEditableGraphContainer<Element, Wire, ICircuitPin>.Move(object newParent, IEnumerable<object> movingObjects)
        {
            if (newParent == null)
                newParent = this;

            
            var movingItems = movingObjects.ToArray();


            var movingAnnotations = movingItems.AsIEnumerable<Annotation>().ToArray();            
            var moduleSet = new HashSet<Element>();
            var movingNodes = movingItems.AsIEnumerable<Element>().ToArray();
            var newContainer = newParent.Cast<ICircuitContainer>();

            var oldContainer = movingNodes.Length > 0 ?
                movingNodes.First().DomNode.Parent.Cast<ICircuitContainer>() :
                movingAnnotations.First().DomNode.Parent.Cast<ICircuitContainer>();
            Debug.Assert(oldContainer != newContainer);


            // all relevant (internal to the old container) edges before the moving
            var internalConnections = new List<Wire>();
            var incomingConnections = new List<Wire>();
            var outgoingConnections = new List<Wire>();

            CircuitUtil.GetSubGraph(oldContainer, movingItems, moduleSet, internalConnections, incomingConnections, outgoingConnections);
            var graphValidator = DomNode.GetRoot().Cast<CircuitValidator>();
            graphValidator.Suspended = true;

            // transfer modules
            foreach (var module in movingNodes)
            {
                oldContainer.Elements.Remove(module);
                newContainer.Elements.Add(module);
            }

            // transfer internal connections (those between grouped modules)
            foreach (Wire connection in internalConnections)
            {
                oldContainer.Wires.Remove(connection);
                newContainer.Wires.Add(connection);
            }

            // location transformation of moved modules         
            var offset = GetRelativeOffset(oldContainer, newContainer);
            foreach (var module in movingNodes)
            {
                var relLoc = module.Bounds.Location;
                relLoc.Offset(offset);
                module.Bounds = new Rectangle(relLoc, module.Bounds.Size);
            }

            foreach (var annot in movingAnnotations)
            {
                oldContainer.Annotations.Remove(annot);
                newContainer.Annotations.Add(annot);

                var relLoc = annot.Bounds.Location;
                relLoc.Offset(offset);
                annot.Bounds = new Rectangle(relLoc, annot.Bounds.Size);
            }

            graphValidator.Suspended = false;
            graphValidator.MovingCrossContainer = true;
        }

        /// <summary>
        /// Can a container be resized</summary>
        /// <param name="container">Container to resize</param>
        /// <param name="borderPart">Part of border to resize</param>
        /// <returns>True iff the container border can be resized</returns>
        bool IEditableGraphContainer<Element, Wire, ICircuitPin>.CanResize(object container, DiagramBorder borderPart)
        {
            if (container.Is<Group>())
            {
                if (container.Is<IReference<Group>>())
                    return false; // disallow resizing group references

                var group = container.Cast<Group>();
                if (group.Expanded)// && !group.AutoSize)
                {
                    if (borderPart.Border == DiagramBorder.BorderType.Right ||
                        borderPart.Border == DiagramBorder.BorderType.Bottom)
                    {
                        var layoutContext = m_viewingContext.Cast<ILayoutContext>();
                        BoundsSpecified specified = layoutContext.CanSetBounds(group);
                        if ((specified & BoundsSpecified.Width) != 0 || (specified & BoundsSpecified.Height) != 0)
                            return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Resizes a container</summary>
        /// <param name="container">Container to resize</param>
        /// <param name="newWidth">New container width</param>
        /// <param name="newHeight">New container height</param>
        void IEditableGraphContainer<Element, Wire, ICircuitPin>.Resize(object container, int newWidth, int newHeight)
        {
            var control = m_viewingContext.Cast<AdaptableControl>();

            var group = container.Cast<Group>();
            if (!string.IsNullOrEmpty(group.Name))
                // Subtract the label height because this isn't a part of the CircuitGroupInfo.MinimumSize or group.Bounds.
                // The label height is added back in by D2dCircuitRenderer.GetBounds(). http://tracker.ship.scea.com/jira/browse/WWSATF-1504
                newHeight -= GetLabelHeight(control);

            if (group.AutoSize)
                group.Info.MinimumSize = new Size(newWidth, newHeight);
            else
                group.Bounds = new Rectangle(group.Bounds.Location.X, group.Bounds.Location.Y, newWidth, newHeight);

            group.OnChanged(EventArgs.Empty);
        }

        #endregion

        /// <summary>
        /// Centers items in canvas at point</summary>
        /// <param name="items">Items to center</param>
        /// <param name="p">Point at which to center items, in client space</param>
        public void Center(IEnumerable<object> items, Point p)
        {
            // get bounds, convert to world coords
            Rectangle bounds;
            LayoutContexts.GetBounds(m_viewingContext.Cast<ILayoutContext>(), items, out bounds);

            Matrix transform = m_viewingContext.Cast<AdaptableControl>().Cast<ITransformAdapter>().Transform;
            p = GdiUtil.InverseTransform(transform, p); // convert center to world coords
            m_viewingContext.Cast<ILayoutContext>().Center(items, p);
        }

        /// <summary>
        /// Finds element, edge or pin hit by the given point</summary>
        /// <param name="point">Point in client space</param>
        /// <returns>GraphHitRecord describing hit point</returns>
        /// <remarks>Currently you need to override this method only if you want to support
        /// directly dragging and dropping a palette item onto a group.</remarks>
        protected virtual GraphHitRecord<Element, Wire, ICircuitPin> Pick(Point point)
        {
            // The default behavior is to return null. So when dragging and dropping a palette item onto a group in the canvas,   
            // the palette item does not automatically get added to the group. Instead, it gets dropped onto the main canvas 
            // and then becomes hidden from view (because it is behind the group).	This then requires moving the group to
            // the side and performing a drag and drop from the canvas onto the group. 

            // Override this method so ATF can detect the case for drag & drop over a group
            return null;
        }

        /// <summary>
        /// Adds new object of given type to circuit using a transaction. Called by automated scripts during testing.</summary>
        /// <typeparam name="T">Type of object to add</typeparam>
        /// <param name="domNode">DomNode that contains added object</param>
        /// <param name="xPos">X-coordinate at center of insertion position</param>
        /// <param name="yPos">Y-coordinate at center of insertion position</param>
        /// <returns>Last selected item</returns>
        public T Insert<T>(DomNode domNode, int xPos, int yPos) where T : class
        {
            DataObject dataObject = new DataObject(new object[] { domNode });

            ITransactionContext transactionContext = this.As<ITransactionContext>();
            transactionContext.DoTransaction(
                delegate
                {
                    Insert(dataObject, new Point(xPos, yPos));
                }, "Scripted Insert Object");

            return Selection.GetLastSelected<T>();
        }

        /// <summary>
        /// Edits objects using a transaction, so the history context can be verified.
        /// Called by automated scripts during testing.</summary>
        /// <param name="node">Object to modify</param>
        /// <param name="attr">Attribute to modify</param>
        /// <param name="newValue">New value</param>
        public void SetProperty(DomNode node, AttributeInfo attr, object newValue)
        {
            ITransactionContext transactionContext = this.As<ITransactionContext>();
            transactionContext.DoTransaction(
                delegate
                {
                    node.SetAttribute(attr, newValue);
                }, "Scripted Edit Property");
        }

        private DomNode[] Insert(object insertingObject, Point center)
        {
            var dataObject = (IDataObject)insertingObject;
            IEnumerable<object> items = GetCompatibleData(dataObject);
            if (items == null)
                return null;

            if (items.All(x => x.Is<Template>()))
            {
                var refs = new List<object>();
                foreach (var item in items.AsIEnumerable<Template>())
                    refs.Add(InsertReference(item));
                Center(refs, center);
                Selection.SetRange(refs);
                return null;
            }

            var itemCopies = DomNode.Copy(items.AsIEnumerable<DomNode>());

            var modules = new List<Element>(itemCopies.AsIEnumerable<Element>());
            foreach (var module in modules)
                m_circuitContainer.Elements.Add(module);

            foreach (var connection in itemCopies.AsIEnumerable<Wire>())
                m_circuitContainer.Wires.Add(connection);

            foreach (var annotation in itemCopies.AsIEnumerable<Annotation>())
                m_circuitContainer.Annotations.Add(annotation);

            Center(itemCopies, center);

            Selection.SetRange(itemCopies);

            return itemCopies;
        }

        private DomNode InsertReference(Template template)
        {
            var elementReference = m_templatingContext.CreateReference(template).Cast<Element>();
            m_circuitContainer.Elements.Add(elementReference);
            return elementReference.DomNode;
        }

        private IEnumerable<object> GetCompatibleData(IDataObject dataObject)
        {
            // try the local format first
            IEnumerable<object> items = dataObject.GetData(typeof(object[])) as object[];
            if (items == null)
                return null;

            if (!ValidTemplateReferences(items))
                return null;

            if (AreCircuitItems(items) || AreTemplateItems(items))
            {
                return items;
            }

            // try serialized format
            byte[] data = dataObject.GetData(CircuitFormat) as byte[];
            if (data != null)
            {
                try
                {
                    DomNodeSerializer serializer = new DomNodeSerializer();
                    IEnumerable<DomNode> deserialized = serializer.Deserialize(data, m_schemaLoader.GetNodeType);
                    items = deserialized.AsIEnumerable<object>();
                    if (AreCircuitItems(items))
                        return items;
                }
                catch /*(Exception ex)*/
                {
                    // the app cannot recover when using output service                   
                    //Outputs.WriteLine(OutputMessageType.Warning, ex.Message);
                }
            }

            return null;
        }

        private bool AreCircuitItems(IEnumerable<object> items)
        {
            return items.All(IsCircuitItem);
        }

        private bool AreTemplateItems(IEnumerable<object> items)
        {
            return items.All(IsTemplateItem);
        }

        private static bool IsCircuitItem(object item)
        {

            return item.Is<Element>() || item.Is<Wire>() || item.Is<Annotation>();
        }

        private bool IsTemplateItem(object item)
        {
            return m_templatingContext != null && m_templatingContext.CanReference(item);
        }

        /// <summary>
        /// Template references are valid only when the template is listed in the template library</summary>
        private bool ValidTemplateReferences(IEnumerable<object> items)
        {
            var templatingContext = m_templatingContext.As<TemplatingContext>();
            if (templatingContext == null)
                return true; // skip validating references 
            foreach (var item in items)
            {
                var reference = item.As<IReference<DomNode>>();
                if (reference != null)
                {
                    Guid targetGuid = Guid.Empty;
                    var groupReference = reference.As<GroupReference>();
                    if (groupReference != null)
                        targetGuid = groupReference.Template.Guid;
                    else
                    {
                        var elementReference = reference.As<ElementReference>();
                        if (elementReference != null)
                            targetGuid = elementReference.Template.Guid;
                    }
                    if (targetGuid != Guid.Empty)
                    {
                        if (templatingContext.SearchForTemplateByGuid(templatingContext.RootFolder, targetGuid) == null)
                            return false;
                    }
                }
            }

            return true;
        }

        private bool IsConnectionCopyable(Wire wire, HashSet<Element> modules)
        {
            return
                modules.Contains(wire.OutputElement) &&
                modules.Contains(wire.InputElement);
        }

        /// Gets location offset from oldContainer to newContainer, compensate renderer displacements for element title
        /// and margin sub-nodes location are defined relative to the parent container
        private Point GetRelativeOffset(ICircuitContainer oldContainer, ICircuitContainer newContainer)
        {
            AdaptableControl control = m_viewingContext.Cast<AdaptableControl>();

            var offset = new Point();
            var oldDomContainer = oldContainer.Cast<DomNode>();
            var newDomContainer = newContainer.Cast<DomNode>();

            var commonAncestor = DomNode.GetLowestCommonAncestor(oldDomContainer, newDomContainer);

            var upPath = oldDomContainer.Lineage.TakeWhile(x => x != commonAncestor);
            var upOffset = GetWorldOffset(control, upPath.AsIEnumerable<Element>());
            offset.Offset(upOffset.X, upOffset.Y);

            var downPath = newDomContainer.Lineage.TakeWhile(x => x != commonAncestor).Reverse();
            var downOffset = GetWorldOffset(control, downPath.AsIEnumerable<Element>());
            offset.Offset(-downOffset.X, -downOffset.Y);
            return offset;
        }


        // an element is eligible to move into another container only it first moves out its current container
        private bool IsSelfContainedOrIntersected(DomNode element, DomNode container)
        {
            // the moving element should first move out of the  parent container next to the common ancestor
            var commonAncestor = DomNode.GetLowestCommonAncestor(element, container);
            var currentContainer = element.Lineage.First(x => x.Parent == commonAncestor);
            if (element == currentContainer)
                return false; // no self containing


            var control = m_viewingContext.Cast<AdaptableControl>();
            var elemLocalBound = GetLocalBound(control, element.Cast<Element>());
            var containerLocalBound = GetLocalBound(control, currentContainer.Cast<Element>());

            containerLocalBound.Location = new PointF(0, GetTitleHeight(control));
            containerLocalBound.Height -= GetLabelHeight(control);// exclude bottom label area

            elemLocalBound.Offset(GetSubContentOffset(control));
            bool contained = containerLocalBound.Contains(elemLocalBound);
            containerLocalBound.Height -= GetTitleHeight(control);// no subcontent offset if element is moved out of the current container
            bool intersected = containerLocalBound.IntersectsWith(elemLocalBound);

            return contained || intersected;

        }

        private bool IsContained(DomNode element, DomNode container)
        {
            if (container.Is<Circuit>())
                return true;

            if (m_moveElementBehavior == MoveElementBehavior.MoveConstrainToCursorContainment)
            {
                // since container is the drop target, the cursor must be over the container when CanMove() is called
                return true;
            }
            else if (m_moveElementBehavior == MoveElementBehavior.MoveConstrainToContainerBounds)
            {
                AdaptableControl control = m_viewingContext.Cast<AdaptableControl>();

                var offset = GetRelativeOffset(element.Parent.Cast<ICircuitContainer>(), container.Cast<ICircuitContainer>());

                // get bound in local space
                var elemLocalBound = GetLocalBound(control, element.Cast<Element>());
                var containerLocalBound = GetLocalBound(control, container.Cast<Element>());

                elemLocalBound.Offset(offset);

                containerLocalBound.Location = new PointF(0, GetTitleHeight(control));
                containerLocalBound.Height -= GetLabelHeight(control);// exclude bottom label area

                elemLocalBound.Offset(GetSubContentOffset(control));

                return containerLocalBound.Contains(elemLocalBound);
            }
            return false;
        }

        private static Color s_zeroColor = new Color();
        private IInstancingContext m_instancingContext;
        private ITemplatingContext m_templatingContext;
        private ICircuitContainer m_circuitContainer;
        private IViewingContext m_viewingContext;
        private XmlSchemaTypeLoader m_schemaLoader;
        private MoveElementBehavior m_moveElementBehavior = MoveElementBehavior.MoveConstrainToCursorContainment;
        private bool m_supportsNestedGroup = true;

    }
}
