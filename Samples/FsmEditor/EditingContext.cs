//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Adaptable;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace FsmEditorSample
{
    /// <summary>
    /// Adapts FSM for editing. Implements ISelectionContext, IValidationContext,
    /// ITransactionContext, IHistoryContext, IInstancingContext, and IEditableGraph,
    /// for editing by AdaptableControl.</summary>
    public class EditingContext : Sce.Atf.Dom.EditingContext,
        IEnumerableContext,
        IObservableContext,
        INamingContext,
        IInstancingContext,
        IEditableGraph<State, Transition, NumberedRoute>
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the editing context's DomNode.
        /// Raises the EditingContext NodeSet event and performs custom processing.</summary>
        protected override void OnNodeSet()
        {
            m_fsm = DomNode.Cast<Fsm>();

            // use the viewing context to access the viewing control, and for bounds calculations
            m_viewingContext = DomNode.Cast<ViewingContext>();

            DomNode.AttributeChanged += new EventHandler<AttributeEventArgs>(DomNode_AttributeChanged);
            DomNode.ChildInserted += new EventHandler<ChildEventArgs>(DomNode_ChildInserted);
            DomNode.ChildRemoved += new EventHandler<ChildEventArgs>(DomNode_ChildRemoved);

            base.OnNodeSet();
        }

        /// <summary>
        /// Gets the Fsm (finite state machine) instance</summary>
        public Fsm Fsm
        {
            get { return m_fsm; }
        }

        #region IEnumerableContext Members

        /// <summary>
        /// Gets an enumeration of all of the items of this context</summary>
        IEnumerable<object> IEnumerableContext.Items
        {
            get
            {
                foreach (State state in Fsm.States)
                    yield return state;
                foreach (Transition transition in Fsm.Transitions)
                    yield return transition;
                foreach (Annotation annotation in Fsm.Annotations)
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
        /// Event that is raised when the collection has been reloaded</summary>
        public event EventHandler Reloaded
        {
            add { }
            remove { }
        }

        private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
        {
            if (IsFsmItem(e.DomNode, e.DomNode.Parent))
                ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
        }

        private void DomNode_ChildInserted(object sender, ChildEventArgs e)
        {
            if (IsFsmItem(e.Child, e.Parent))
                ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(e.Index, e.Child, e.Parent));
        }

        private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
        {
            if (IsFsmItem(e.Child, e.Parent))
                ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(e.Index, e.Child, e.Parent));
        }

        private static bool IsFsmItem(DomNode child, DomNode parent)
        {
            return
                (parent != null && parent.Is<Fsm>());
        }

        #endregion

        #region INamingContext Members

        /// <summary>
        /// Gets the item's name in the context, or null if none</summary>
        /// <param name="item">Item</param>
        /// <returns>Item's name in the context, or null if none</returns>
        string INamingContext.GetName(object item)
        {
            State state = Adapters.As<State>(item);
            if (state != null)
                return state.Name;

            Transition transition = Adapters.As<Transition>(item);
            if (transition != null)
                return transition.Label;

            Annotation annotation = Adapters.As<Annotation>(item);
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
                Adapters.Is<State>(item) ||
                Adapters.Is<Transition>(item) ||
                Adapters.Is<Annotation>(item);
        }

        /// <summary>
        /// Sets the item's name</summary>
        /// <param name="item">Item to name</param>
        /// <param name="name">New item name</param>
        void INamingContext.SetName(object item, string name)
        {
            State state = Adapters.As<State>(item);
            if (state != null)
            {
                state.Name = name;
            }
            else
            {
                Transition transition = Adapters.As<Transition>(item);
                if (transition != null)
                {
                    transition.Label = name;
                }
                else
                {
                    Annotation annotation = Adapters.As<Annotation>(item);
                    if (annotation != null)
                        annotation.Text = name;
                }
            }
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
            // get the set of states and copy them
            HashSet<State> states = new HashSet<State>(Selection.AsIEnumerable<State>());
            HashSet<DomNode> itemsToCopy = new HashSet<DomNode>(Adapters.AsIEnumerable<DomNode>(states));

            // look for selected transitions between copied states and copy them
            foreach (Transition transition in Selection.AsIEnumerable<Transition>())
                if (IsTransitionCopyable(transition, states))
                    itemsToCopy.Add(Adapters.As<DomNode>(transition));

            // if no transitions were added from items, look for copyable transitions between the copied states
            if (itemsToCopy.Count == states.Count)
            {
                foreach (State state in states)
                    foreach (Transition transition in m_fsm.Transitions)
                        if (IsTransitionCopyable(transition, states))
                            itemsToCopy.Add(Adapters.As<DomNode>(transition));
            }

            // add annotations
            foreach (Annotation annotation in Selection.AsIEnumerable<Annotation>())
                itemsToCopy.Add(Adapters.As<DomNode>(annotation));

            List<object> copies = new List<object>(DomNode.Copy(itemsToCopy));
            return new DataObject(copies.ToArray());
        }

        private bool IsTransitionCopyable(Transition transition, HashSet<State> states)
        {
            return
                states.Contains(transition.ToState) &&
                states.Contains(transition.FromState);
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
                if (!Adapters.Is<State>(item) &&
                    !Adapters.Is<Transition>(item) &&
                    !Adapters.Is<Annotation>(item))
                {
                    return false;
                }
            }

            return true;
        }
        
        /// <summary>
        /// Inserts a new state with the specified location, label/name, and size.
        /// The x, y coordinates specify the upper left coordinates of the new state.</summary>
        /// <param name="xUpperLeft">Upper left x-coordinate of the new state</param>
        /// <param name="yUpperLeft">Upper left y-coordinate of the new state</param>
        /// <param name="label">State's label</param>
        /// <param name="size">State's size</param>
        /// <returns>New State</returns>
        public State InsertState(int xUpperLeft, int yUpperLeft, string label, int size)
        {
            xUpperLeft = xUpperLeft < 0 ? 0 : xUpperLeft;
            yUpperLeft = yUpperLeft < 0 ? 0 : yUpperLeft;
            size = size < 64 ? 64 : size;
            DomNode domNode = new DomNode(Schema.stateType.Type);
            domNode.SetAttribute(Schema.stateType.nameAttribute, "State");
            domNode.SetAttribute(Schema.stateType.xAttribute, xUpperLeft);
            domNode.SetAttribute(Schema.stateType.yAttribute, yUpperLeft);
            domNode.SetAttribute(Schema.stateType.sizeAttribute, size);
            domNode.SetAttribute(Schema.stateType.labelAttribute, label);            
            DataObject dataObject = new DataObject(new object[] { domNode });

            //The Point object passed to the Insert() function represents the center
            //of the new state (to match the drag and drop functionality), so convert
            //the upperLeft coordinates to the center coordinates:
            int xCenter = xUpperLeft + size / 2;
            int yCenter = yUpperLeft + size / 2;

            ITransactionContext transactionContext = this.As<ITransactionContext>();
            transactionContext.DoTransaction(
                delegate
                {
                    Insert(dataObject, new Point(xCenter, yCenter));
                }, "Scripted InsertState");
            
            return Selection.GetLastSelected<State>();
        }

        /// <summary>
        /// Inserts a new annotation (comment) with the specified location and text.
        /// The x, y coordinates specify the upper left coordinates of the new annotation.</summary>
        /// <param name="xUpperLeft">Upper left x-coordinate of the new annotation</param>
        /// <param name="yUpperLeft">Upper left y-coordinate of the new annotation</param>
        /// <param name="text">Annotation's text</param>
        /// <returns>New Annotation</returns>
        public Annotation InsertComment(int xUpperLeft, int yUpperLeft, string text)
        {
            xUpperLeft = xUpperLeft < 0 ? 0 : xUpperLeft;
            yUpperLeft = yUpperLeft < 0 ? 0 : yUpperLeft;
            DomNode domNode = new DomNode(Schema.annotationType.Type);
            domNode.SetAttribute(Schema.annotationType.xAttribute, xUpperLeft);
            domNode.SetAttribute(Schema.annotationType.yAttribute, yUpperLeft);
            domNode.SetAttribute(Schema.annotationType.textAttribute, text);
            DataObject dataObject = new DataObject(new object[] { domNode });

            ITransactionContext transactionContext = this.As<ITransactionContext>();
            transactionContext.DoTransaction(
                delegate
                {
                    Insert(dataObject, new Point(xUpperLeft, yUpperLeft));
                }, "Scripted InsertAnnotation");

            return Selection.GetLastSelected<Annotation>();
        }

        /// <summary>
        /// Inserts the data object into the context</summary>
        /// <param name="insertingObject">Data to insert; e.g., System.Windows.Forms.IDataObject</param>
        public void Insert(object insertingObject)
        {
            Point center = new Point(m_viewingContext.Control.Width / 2, m_viewingContext.Control.Height / 2);
            DragDropAdapter dragDropAdapter = m_viewingContext.Control.As<DragDropAdapter>();
            if (dragDropAdapter != null && dragDropAdapter.IsDropping)
                center = dragDropAdapter.MousePosition;
            Insert(insertingObject, center);
        }
        private void Insert(object insertingObject, Point centerLocation)
        {
            IDataObject dataObject = (IDataObject)insertingObject;
            object[] items = dataObject.GetData(typeof(object[])) as object[];
            if (items == null)
                return;

            object[] itemCopies = DomNode.Copy(Adapters.AsIEnumerable<DomNode>(items));

            List<State> states = new List<State>(Adapters.AsIEnumerable<State>(itemCopies));
            foreach (State state in states)
                m_fsm.States.Add(state);

            foreach (Transition transition in Adapters.AsIEnumerable<Transition>(itemCopies))
                m_fsm.Transitions.Add(transition);

            foreach (Annotation annotation in Adapters.AsIEnumerable<Annotation>(itemCopies))
                m_fsm.Annotations.Add(annotation);

            Center(itemCopies, centerLocation);

            Selection.SetRange(itemCopies);
        }

        /// <summary>
        /// Centers items in canvas at point</summary>
        /// <param name="items">Items to center</param>
        /// <param name="p">Point at which to center items</param>
        public void Center(IEnumerable<object> items, Point p)
        {
            ILayoutContext layoutContext = this.As<ILayoutContext>();
            if (layoutContext != null)
            {
                // get bounds, convert to world coords                
                Matrix transform = m_viewingContext.Control.As<ITransformAdapter>().Transform;
                p = GdiUtil.InverseTransform(transform, p);
                LayoutContexts.Center(layoutContext, items, p);
            }
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

        #region IEditableGraph<State, Transition, NumberedRoute> Members

        /// <summary>
        /// Returns whether these two States can be connected with a Transition. "from" and "to" refer to the corresponding
        /// properties in IGraphEdge, not to a dragging operation, for example.</summary>
        /// <param name="fromNode">From State</param>
        /// <param name="fromRoute">From NumberedRoute</param>
        /// <param name="toNode">To State</param>
        /// <param name="toRoute">To NumberedRoute</param>
        /// <returns>Whether the "from" State/NumberedRoute can be connected to the "to" State/NumberedRoute</returns>
        bool IEditableGraph<State, Transition, NumberedRoute>.CanConnect(State fromNode, NumberedRoute fromRoute, State toNode, NumberedRoute toRoute)
        {
            return true;
        }

        /// <summary>
        /// Connects a new transition between the provided states using a transaction.
        /// Interface for automation.</summary>
        /// <param name="fromNode">From State</param>
        /// <param name="toNode">To State</param>
        /// <returns>New Transition</returns>
        public Transition InsertTransition(
            State fromNode, State toNode)
        {
            ITransactionContext transactionContext = this.As<ITransactionContext>();
            
            transactionContext.DoTransaction(
                delegate
                {
                    //Not sure why I have to caste this, but calling Connect() directly does not compile
                    //Connect(fromNode, new NumberedRoute(), toNode, new NumberedRoute(), null);
                    ((IEditableGraph<State, Transition, NumberedRoute>)this).Connect(fromNode, new NumberedRoute(), toNode, new NumberedRoute(), null);
                }, "Scripted InsertTransition");

            return m_fsm.Transitions[m_fsm.Transitions.Count - 1];
        }

        /// <summary>
        /// Connects the "from" State/NumberedRoute to the "to" State/NumberedRoute by creating a Transition whose
        /// FromNode is 'fromNode', ToNode is 'toNode', etc.</summary>
        /// <param name="fromNode">From State</param>
        /// <param name="fromRoute">From NumberedRoute</param>
        /// <param name="toNode">To State</param>
        /// <param name="toRoute">To NumberedRoute</param>
        /// <param name="existingEdge">Existing Transition that is being reconnected, or null if new Transition</param>
        /// <returns>New Transition connecting the "from" State/NumberedRoute to the "to" State/NumberedRoute</returns>
        Transition IEditableGraph<State, Transition, NumberedRoute>.Connect(
            State fromNode, NumberedRoute fromRoute, State toNode, NumberedRoute toRoute, Transition existingEdge)
        {
            DomNode domNode = new DomNode(Schema.transitionType.Type);
            Transition transition = domNode.As<Transition>();

            transition.FromState = fromNode as State;
            transition.ToState = toNode as State;
            // we set the route after the logical operation completes

            if (existingEdge != null)
                transition.Label = existingEdge.Label;

            m_fsm.Transitions.Add(transition);
            return transition;
        }

        /// <summary>
        /// Gets whether the Transition can be disconnected</summary>
        /// <param name="edge">Transition to disconnect</param>
        /// <returns>Whether the Transition can be disconnected</returns>
        bool IEditableGraph<State, Transition, NumberedRoute>.CanDisconnect(Transition edge)
        {
            return true;
        }

        /// <summary>
        /// Disconnects the Transition</summary>
        /// <param name="edge">Transition to disconnect</param>
        void IEditableGraph<State, Transition, NumberedRoute>.Disconnect(Transition edge)
        {
            m_fsm.Transitions.Remove(edge as Transition);
        }

        #endregion

        private Fsm m_fsm;
        private ViewingContext m_viewingContext;
    }
}
