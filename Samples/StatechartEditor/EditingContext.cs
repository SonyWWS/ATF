//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Adaptable;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace StatechartEditorSample
{
    /// <summary>
    /// Main editing context for the statechart. This DOM adapter is defined on the
    /// statechart document type, and makes the root statechart observable and enumerable.
    /// It also implements naming, locking, and instancing behavior, and adapts the
    /// statechart to a graph, and an editable graph of states and transitions.</summary>
    public class EditingContext : Sce.Atf.Dom.EditingContext,
        IEnumerableContext,
        IObservableContext,
        INamingContext,
        ILockingContext,
        IInstancingContext,
        IGraph<StateBase, Transition, BoundaryRoute>,
        IEditableGraph<StateBase, Transition, BoundaryRoute>
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the editing context's DomNode.
        /// Raises the EditingContext NodeSet event and performs custom processing.</summary>
        protected override void OnNodeSet()
        {
            m_statechart = DomNode.Cast<Statechart>();
            m_document = DomNode.Cast<Document>();
            m_viewingContext = DomNode.Cast<ViewingContext>();

            m_transitions = new DomNodeListAdapter<Transition>(DomNode, Schema.statechartDocumentType.transitionChild);

            DomNode.AttributeChanged += DomNode_AttributeChanged;
            DomNode.ChildInserted += DomNode_ChildInserted;
            DomNode.ChildRemoved += DomNode_ChildRemoved;

            base.OnNodeSet();
        }

        /// <summary>
        /// Gets the Statechart</summary>
        public Statechart Statechart
        {
            get { return m_statechart; }
        }

        /// <summary>
        /// Gets the Document</summary>
        public Document Document
        {
            get { return m_document; }
        }

        /// <summary>
        /// Centers items in canvas at point</summary>
        /// <param name="items">Items to center</param>
        /// <param name="center">Point at which to center items, in world coordinates</param>
        public void Center(IEnumerable<object> items, Point center)
        {
             Rectangle bounds; //world coordinates
             m_viewingContext.GetBounds(items, out bounds);
           
            // calculate offset
            Point offset = new Point(
                center.X - (bounds.Left + bounds.Width / 2),
                center.Y - (bounds.Top + bounds.Height / 2));            

            // we must handle states because we also need to center their child states
            //  otherwise, we could use LayoutContext.Center
            foreach (StateBase stateBase in items.AsIEnumerable<StateBase>())
                Offset(stateBase, offset);

            foreach (Annotation annotation in items.AsIEnumerable<Annotation>())
                Offset(annotation, offset);
        }

        // 'offset' is in world coordinates
        private void Offset(StateBase stateBase, Point offset)
        {
            Rectangle itemBounds = stateBase.Bounds;
            itemBounds.X += offset.X;
            itemBounds.Y += offset.Y;

            ((ILayoutContext)m_viewingContext).SetBounds(stateBase, itemBounds, BoundsSpecified.Location);

            State state = stateBase as State;
            if (state != null)
            {
                foreach (StateBase subState in state.SubStates)
                    Offset(subState, offset);
            }
        }

        // 'offset' is in world coordinates
        private void Offset(Annotation annotation, Point offset)
        {
            Rectangle itemBounds = annotation.Bounds;
            itemBounds.X += offset.X;
            itemBounds.Y += offset.Y;
            ((ILayoutContext)m_viewingContext).SetBounds(annotation, itemBounds, BoundsSpecified.Location);
        }

        #region IEnumerableContext Members

        /// <summary>
        /// Gets an enumeration of all of the items of this context</summary>
        IEnumerable<object> IEnumerableContext.Items
        {
            get
            {
                foreach (StateBase state in m_statechart.States)
                    yield return state;
                foreach (Annotation annotation in m_document.Annotations)
                    yield return annotation;
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
        public event EventHandler Reloaded
        {
            add { }
            remove { }
        }

        private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
        {
            if (IsStatechartItem(e.DomNode, e.DomNode.Parent))
                ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
        }

        private void DomNode_ChildInserted(object sender, ChildEventArgs e)
        {
            if (IsStatechartItem(e.Child, e.Parent))
                ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(e.Index, e.Child, e.Parent));
        }

        private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
        {
            if (IsStatechartItem(e.Child, e.Parent))
                ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(e.Index, e.Child, e.Parent));
        }

        private static bool IsStatechartItem(DomNode child, DomNode parent)
        {
            return
                (parent != null && parent.GetRoot().Is<Statechart>());
        }

        #endregion

        #region INamingContext Members

        /// <summary>
        /// Gets the item's name in the context, or null if none</summary>
        /// <param name="item">Item</param>
        /// <returns>Item's name in the context, or null if none</returns>
        string INamingContext.GetName(object item)
        {
            State state = item.As<State>();
            if (state != null)
                return state.Name;

            Annotation annotation = item.As<Annotation>();
            if (annotation != null)
                return annotation.Text;

            return null;
        }

        /// <summary>
        /// Returns whether the item can be named</summary>
        /// <param name="item">Item to name</param>
        /// <returns>True iff the item can be named</returns>
        bool INamingContext.CanSetName(object item)
        {
            return
                item.Is<State>() ||
                item.Is<Annotation>();
        }

        /// <summary>
        /// Sets the item's name</summary>
        /// <param name="item">Item to name</param>
        /// <param name="name">New item name</param>
        void INamingContext.SetName(object item, string name)
        {
            State state = item.As<State>();
            if (state != null)
            {
                state.Name = name;
            }
            else
            {
                Annotation annotation = item.As<Annotation>();
                if (annotation != null)
                    annotation.Text = name;
            }
        }

        #endregion

        #region ILockingContext Members

        /// <summary>
        /// Returns whether the item is locked</summary>
        /// <param name="item">Item</param>
        /// <returns>True iff the item is locked</returns>
        bool ILockingContext.IsLocked(object item)
        {
            StateBase stateBase = item.As<StateBase>();
            return
                stateBase != null &&
                stateBase.Locked;
        }

        /// <summary>
        /// Returns whether the item can be locked and unlocked</summary>
        /// <param name="item">Item</param>
        /// <returns>True iff the item item can be locked and unlocked</returns>
        bool ILockingContext.CanSetLocked(object item)
        {
            return item.Is<StateBase>();
        }

        /// <summary>
        /// Sets the item's locked state to the given value</summary>
        /// <param name="item">Item to lock or unlock</param>
        /// <param name="value">True to lock, false to unlock</param>
        void ILockingContext.SetLocked(object item, bool value)
        {
            StateBase stateBase = item.As<StateBase>();
            if (stateBase != null)
                stateBase.Locked = value;
        }

        #endregion

        #region IInstancingContext Members

        /// <summary>
        /// Tests if can copy selection from the statechart</summary>
        /// <returns>True iff there are items to copy</returns>
        public bool CanCopy()
        {
            return Selection.Count > 0;
        }

        /// <summary>
        /// Copies selected items from the statechart</summary>
        /// <returns>DataObject containing an enumeration of selected items</returns>
        public object Copy()
        {
            List<DomNode> domNodes = new List<DomNode>();
            // form state "closure" for determining which transitions to copy
            HashSet<StateBase> allStates = new HashSet<StateBase>();

            // for all selected root states and sub-states
            IEnumerable<DomNode> rootNodes = DomNode.GetRoots(Selection.AsIEnumerable<DomNode>());
            IEnumerable<StateBase> rootStates = rootNodes.AsIEnumerable<StateBase>();
            foreach (StateBase stateBase in rootStates)
            {
                domNodes.Add(stateBase.DomNode);
                allStates.Add(stateBase);
                State state = stateBase.As<State>();
                if (state != null)
                {
                    foreach (StateBase subState in state.SubStates)
                        allStates.Add(subState);
                }
            }

            // get selected transitions between selected states
            bool itemsIncludeTransitions = false;
            foreach (Transition transition in Selection.AsIEnumerable<Transition>())
            {
                domNodes.Add(transition.DomNode);
                itemsIncludeTransitions = true;
            }

            // if there were none, then try to add any transitions between selected states or sub-states
            if (!itemsIncludeTransitions)
            {
                foreach (Transition transition in m_transitions)
                {
                    if (allStates.Contains(transition.FromState) &&
                        allStates.Contains(transition.ToState))
                    {
                        domNodes.Add(transition.DomNode);
                    }
                }
            }

            foreach (Annotation annotation in Selection.AsIEnumerable<Annotation>())
                domNodes.Add(annotation.DomNode);

            DomNode[] copies = DomNode.Copy(domNodes);

            return new DataObject(copies.ToArray<object>());
        }

        /// <summary>
        /// Tests if can insert a given object into the statechart</summary>
        /// <param name="insertingObject">Object to insert</param>
        /// <returns>True iff can insert object into the statechart</returns>
        public bool CanInsert(object insertingObject)
        {
            IDataObject dataObject = (IDataObject)insertingObject;
            object[] items = dataObject.GetData(typeof(object[])) as object[];
            if (items == null)
                return false;

            foreach (object item in items)
            {
                if (!item.Is<StateBase>() &&
                    !item.Is<Transition>() &&
                    !item.Is<Annotation>())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Adds new objects of given type to statechart using a transaction. 
        /// Called by automated scripts during testing.</summary>
        /// <typeparam name="T">Type of objects to add</typeparam>
        /// <param name="domNode">DomNode that contains added objects</param>
        /// <param name="xPos">X-coordinate at center of insertion position</param>
        /// <param name="yPos">Y-coordinate at center of insertion position</param>
        /// <param name="parentState">Insertion point for added objects</param>
        /// <returns>Last selected item</returns>
        public T Insert<T>(DomNode domNode, int xPos, int yPos, State parentState) where T : class
        {
            //Use a default name of the type of object.  Annotations don't have the nameAttribute though
            if (domNode.Type != Schema.annotationType.Type)
                domNode.SetAttribute(Schema.stateBaseType.nameAttribute, typeof(T).Name);
            DataObject dataObject = new DataObject(new object[] { domNode });

            Statechart insertionPoint = m_statechart;
            if (parentState != null)
                insertionPoint = GetStatechart(parentState);

            ITransactionContext transactionContext = this.As<ITransactionContext>();
            transactionContext.DoTransaction(
                delegate
                {
                    Insert(dataObject, new Point(xPos, yPos), insertionPoint);
                }, "Scripted Insert Object");

            return Selection.GetLastSelected<T>();
        }

        /// <summary>
        /// Inserts object into statechart at the center of the canvas or last selected state if there was one</summary>
        /// <param name="insertingObject">Object to insert</param>
        public void Insert(object insertingObject)
        {
            Statechart insertionPoint = m_statechart; // default is root statechart
            AdaptableControl control = m_viewingContext.Control;
            DragDropAdapter dragDropAdapter = control.As<DragDropAdapter>();
            Matrix transform = control.As<ITransformAdapter>().Transform;

            Point center; // in world coordinates
            if (dragDropAdapter != null && dragDropAdapter.IsDropping)
            {
                insertionPoint = FindStatechartUnder(dragDropAdapter.MousePosition);
                center = GdiUtil.InverseTransform(transform, dragDropAdapter.MousePosition);
            }
            else // paste into last selected state
            {
                State state = Selection.GetLastSelected<State>();
                if (state != null)
                {
                    insertionPoint = GetStatechart(state);
                    Rectangle stateBounds = m_viewingContext.GetBounds(state);
                    center = new Point(
                            stateBounds.X + stateBounds.Width / 2,
                            stateBounds.Y + stateBounds.Height / 2);
                    center = GdiUtil.InverseTransform(transform, center);
                }
                else
                {
                    center = GdiUtil.InverseTransform(transform,
                         new Point(
                             control.Width / 2,
                             control.Height / 2));
                }
            }

            Insert(insertingObject, center, insertionPoint);
        }

        // 'center' must be in world coordinates
        private void Insert(object insertingObject, Point center, Statechart insertionPoint)
        {
            IDataObject dataObject = (IDataObject)insertingObject;
            object[] items = dataObject.GetData(typeof(object[])) as object[];
            if (items == null)
                return;
            
            object[] itemCopies = DomNode.Copy(items.AsIEnumerable<DomNode>());

            foreach (Annotation annotation in itemCopies.AsIEnumerable<Annotation>())
                this.As<Document>().Annotations.Add(annotation);
            
            IEnumerable<StateBase> states = itemCopies.AsIEnumerable<StateBase>();
            foreach (StateBase state in states)
                insertionPoint.States.Add(state);

            foreach (Transition transition in itemCopies.AsIEnumerable<Transition>())
                m_transitions.Add(transition);

            // centering hierarchical states requires some special code
            Center(itemCopies, center);

            Selection.SetRange(itemCopies);
        }

        /// <summary>
        /// Tests if can delete selected items from the statechart</summary>
        /// <returns>True iff can delete selected items from the statechart</returns>
        public bool CanDelete()
        {
            return Selection.Count > 0;
        }

        /// <summary>
        /// Deletes selected items from the statechart</summary>
        public void Delete()
        {
            foreach (DomNode node in Selection.AsIEnumerable<DomNode>())
                node.RemoveFromParent();

            Selection.Clear();
        }

        private Statechart FindStatechartUnder(Point clientPoint)
        {
            AdaptableControl control = m_viewingContext.Control;
            GraphHitRecord<StateBase, Transition, BoundaryRoute> hitRecord =
                control.As<D2dGraphAdapter<StateBase, Transition, BoundaryRoute>>().Pick(clientPoint);
            if (hitRecord.Node != null)
            {
                State hitState = hitRecord.Node as State;
                if (hitState != null)
                    return GetStatechart(hitState);
            }
            // default to root statechart
            return m_statechart;
        }

        private static Statechart GetStatechart(State state)
        {
            if (state.Statecharts.Count == 0)
                state.Statecharts.Add(new DomNode(Schema.statechartType.Type).As<Statechart>());
            return state.Statecharts[0];
        }

        #endregion

        #region IGraph<StateBase,Transition,StatechartRoute> Members

        /// <summary>
        /// Gets the States in the statechart</summary>
        IEnumerable<StateBase> IGraph<StateBase, Transition, BoundaryRoute>.Nodes
        {
            get { return m_statechart.AllStates; }
        }

        /// <summary>
        /// Gets the Transitions in the statechart</summary>
        IEnumerable<Transition> IGraph<StateBase, Transition, BoundaryRoute>.Edges
        {
            get { return m_transitions; }
        }

        #endregion

        #region IEditableGraph Members

        /// <summary>
        /// Returns whether two states can be connected with a Transition. "from" and "to" refer to the corresponding
        /// properties in IGraphEdge, not to a dragging operation, for example.</summary>
        /// <param name="fromNode">From state</param>
        /// <param name="fromRoute">From BoundaryRoute</param>
        /// <param name="toNode">To state</param>
        /// <param name="toRoute">To BoundaryRoute</param>
        /// <returns>Whether the "from" state/BoundaryRoute can be connected to the "to" state/BoundaryRoute</returns>
        bool IEditableGraph<StateBase, Transition, BoundaryRoute>.CanConnect(
            StateBase fromNode, BoundaryRoute fromRoute, StateBase toNode, BoundaryRoute toRoute)
        {
            return true;
        }

        /// <summary>
        /// Connects the "from" state/BoundaryRoute to the "to" state/BoundaryRoute by creating a Transition whose
        /// FromNode is 'fromNode', ToNode is 'toNode', etc.</summary>
        /// <param name="fromNode">"From" state</param>
        /// <param name="fromRoute">"From" BoundaryRoute</param>
        /// <param name="toNode">"To" state</param>
        /// <param name="toRoute">"To" BoundaryRoute</param>
        /// <param name="existingEdge">Existing Transition that is being reconnected, or null if new Transition</param>
        /// <returns>New Transition connecting the "from" state/BoundaryRoute to the "to" state/BoundaryRoute</returns>
        Transition IEditableGraph<StateBase, Transition, BoundaryRoute>.Connect(
            StateBase fromNode, BoundaryRoute fromRoute, StateBase toNode, BoundaryRoute toRoute, Transition existingEdge)
        {
            DomNode domNode = new DomNode(Schema.transitionType.Type);
            Transition transition = domNode.As<Transition>();

            transition.FromState = fromNode;
            transition.FromPosition = fromRoute.Position;
            transition.ToState = toNode;
            transition.ToPosition = toRoute.Position;

            if (existingEdge != null)
            {
                Transition existingTransition = existingEdge as Transition;
                transition.Event = existingTransition.Event;
                transition.Guard = existingTransition.Guard;
                transition.Action = existingTransition.Action;
            }

            m_transitions.Add(transition);

            return transition;
        }

        /// <summary>
        /// Gets whether the Transition can be disconnected</summary>
        /// <param name="edge">Transition to disconnect</param>
        /// <returns>Whether the Transition can be disconnected</returns>
        bool IEditableGraph<StateBase, Transition, BoundaryRoute>.CanDisconnect(Transition edge)
        {
            return true;
        }

        /// <summary>
        /// Disconnects the Transition</summary>
        /// <param name="transition">Transition to disconnect</param>
        void IEditableGraph<StateBase, Transition, BoundaryRoute>.Disconnect(Transition transition)
        {
            m_transitions.Remove(transition);
        }

        #endregion

        private Statechart m_statechart;
        private Document m_document;
        private ViewingContext m_viewingContext;
        private DomNodeListAdapter<Transition> m_transitions;
    }
}
