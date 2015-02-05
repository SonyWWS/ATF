//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Direct2D;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Adapter that adds graph edge dragging capabilities to an adapted control
    /// with a graph adapter</summary>
    /// <typeparam name="TNode">Node type, must implement IGraphNode</typeparam>
    /// <typeparam name="TEdge">Edge type, must implement IGraphEdge</typeparam>
    /// <typeparam name="TEdgeRoute">Edge route type, must implement IEdgeRoute</typeparam>
    public class D2dGraphEdgeEditAdapter<TNode, TEdge, TEdgeRoute> : DraggingControlAdapter
        where TNode : class, IGraphNode
        where TEdge : class, IGraphEdge<TNode, TEdgeRoute>
        where TEdgeRoute : class, IEdgeRoute
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="renderer">Graph renderer</param>
        /// <param name="graphAdapter">Graph adapter</param>
        /// <param name="transformAdapter">Transform adapter</param>
        public D2dGraphEdgeEditAdapter(
            D2dGraphRenderer<TNode, TEdge, TEdgeRoute> renderer,
            D2dGraphAdapter<TNode, TEdge, TEdgeRoute> graphAdapter,
            ITransformAdapter transformAdapter)
        {
            m_renderer = renderer;
            m_graphAdapter = graphAdapter;
            m_draggingContext = new EdgeDraggingContext(this);

            OverRouteCursor = Cursors.Cross;
            FromPlaceCursor = Cursors.PanWest;
            ToPlaceCursor = Cursors.PanEast;
            InadmissibleCursor = Cursors.Cross;
        }

        /// <summary>
        /// Gets whether the user is currently dragging a graph edge</summary>
        public bool IsDraggingEdge
        {
            get { return m_isConnecting; }
        }

        /// <summary>
        /// Gets or sets the cursor that is displayed when the mouse pointer is over a route</summary>
        public Cursor OverRouteCursor { get; set; }

        /// <summary>
        /// Gets or sets the cursor that is displayed when the mouse pointer is over a route that is admissible as a from route </summary>
        public Cursor FromPlaceCursor { get; set; }

        /// <summary>
        /// Gets or sets the cursor that is displayed when the mouse pointer is over a route that is admissible as a to route</summary>
        public Cursor ToPlaceCursor { get; set; }

        /// <summary>
        /// Gets or sets the cursor that is displayed when the mouse pointer is over a route that is inadmissible to form a new edge</summary>
        public Cursor InadmissibleCursor { get; set; }

        /// <summary>
        /// Traverses a given path of groups, starting at the last (picked) item through a given destination group, for the pin
        /// that corresponds to the given group pin in the last group in the path. The last group of "hitPath" is the picked item.</summary>
        public Func<AdaptablePath<object>, object, TEdgeRoute, TEdgeRoute> EdgeRouteTraverser;

        /// <summary>
        /// Context providing facilities for dragging an edge for D2dGraphEdgeEditAdapter</summary>
        public class EdgeDraggingContext
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="edgeEditAdapter">D2dGraphEdgeEditAdapter object</param>
            public EdgeDraggingContext(D2dGraphEdgeEditAdapter<TNode, TEdge, TEdgeRoute> edgeEditAdapter)
            {
                m_edgeEditAdapter = edgeEditAdapter;
            }

            /// <summary>
            /// Gets or sets IGraphNode dragged from</summary>
            public TNode DragFromNode { get; set; }

            /// <summary>
            /// Gets or sets IEdgeRoute drag from route</summary>
            public TEdgeRoute DragFromRoute { get; set; }

            /// <summary>
            /// Gets or sets IGraphNode dragged to</summary>
            public TNode DragToNode { get; set; }
            /// <summary>
            /// Gets or sets IEdgeRoute drag to route</summary>
            public TEdgeRoute DragToRoute { get; set; }
            /// <summary>
            /// Gets or sets whether drag is from source to destination</summary>
            public bool FromSourceToDestination { get; set; }
            /// <summary>
            /// Gets or sets the existing edge to be dragged</summary>
            public TEdge ExistingEdge { get; set; }
            /// <summary>
            /// Gets or sets the existing edge to disconnect</summary>
            public TEdge DisconnectEdge { get; set; }

            /// <summary>
            /// Gets or sets GraphHitRecord</summary>
            public GraphHitRecord<TNode, TEdge, TEdgeRoute> MousePick { get; set; }

            internal IEditableGraph<TNode, TEdge, TEdgeRoute> EditableGraph
            {
                get
                {
                    var lca = HitPathsGetLowestCommonAncestor();
                    var editableGraph = lca.As<IEditableGraph<TNode, TEdge, TEdgeRoute>>();
                    return editableGraph ?? m_edgeEditAdapter.m_mainEditableGraph;
                }

            }

            internal AdaptablePath<object> DragFromNodeHitPath { get; set; }
            internal AdaptablePath<object> DragToNodeHitPath { get; set; }

            /// <summary>
            /// From route position in client space</summary>
            public PointF FromRoutePos { get; set; }

            /// <summary>
            /// To route position in client space</summary>
            public PointF ToRoutePos
            {
                get { return m_toRoutePos; }
                set
                {
                    if (value == PointF.Empty)
                    {

                    }
                    m_toRoutePos = value;
                }
            }

            private PointF m_toRoutePos;



            internal TNode ActualFromNode()
            {
                if (DragFromNode == null)
                    return null;

                if (DragFromNode == DragToNode || DragToNode == null) // self-wiring or no to-route
                    return DragFromNode;

                if (DragFromNodeHitPath == null) // fromNode top level 
                    return DragFromNode;
                if (DragToNodeHitPath == null) // counterpart top level 
                    return DragFromNodeHitPath[0].As<TNode>();


                var lca = HitPathsGetLowestCommonAncestor();
                if (lca == null)
                    return DragFromNodeHitPath[0].As<TNode>(); // counterpart  in another container
                int index = DragFromNodeHitPath.IndexOf(lca);
                return DragFromNodeHitPath[index + 1].As<TNode>(); // return the child node of the lca

            }


            internal TNode ActualToNode()
            {
                if (DragToNode == null)
                    return null;

                if (DragFromNode == DragToNode || DragFromNode == null)
                    return DragToNode;

                if (DragToNodeHitPath == null) // toNode top level 
                    return DragToNode;
                if (DragFromNodeHitPath == null) // counterpart top level 
                    return DragToNodeHitPath[0].As<TNode>();
                var lca = HitPathsGetLowestCommonAncestor();
                if (lca == null)
                    return DragToNodeHitPath[0].As<TNode>(); // counterpart  in another container
                int index = DragToNodeHitPath.IndexOf(lca);
                return DragToNodeHitPath[index + 1].As<TNode>();

            }

            internal TEdgeRoute ActualFromRoute(TNode actualFromNode)
            {
                if (DragFromNodeHitPath == null || DragFromRoute == null) // fromNode top level 
                    return DragFromRoute;
                if (actualFromNode == DragFromNode)
                    return DragFromRoute;
                // we hit a sub-pin, and actualFromNode must be an ancestor of the hit sub-node 
                // find the corresponding pin in actualFromNode, follow up the hitpath
                if (m_edgeEditAdapter.EdgeRouteTraverser != null)
                    return m_edgeEditAdapter.EdgeRouteTraverser(DragFromNodeHitPath, actualFromNode, DragFromRoute);
                return null;
            }

            internal TEdgeRoute ActualToRoute(TNode actualToNode)
            {
                if (DragToNodeHitPath == null || DragToRoute == null) // fromNode top level 
                    return DragToRoute;
                if (actualToNode == DragToNode)
                    return DragToRoute;
                // we hit a sub-pin, and actualToNode must be an ancestor of the hit sub-node 
                // find the corresponding pin in actualToNode, follow up the hitpath
                if (m_edgeEditAdapter.EdgeRouteTraverser != null)
                    return m_edgeEditAdapter.EdgeRouteTraverser(DragToNodeHitPath, actualToNode, DragToRoute);
                return null;
            }

            // Gets the lowest common ancestor (LCA)  for the 2 hit path
            private TNode HitPathsGetLowestCommonAncestor()
            {
                if (DragToNodeHitPath == null || DragFromNodeHitPath == null)
                    return null;
                for (int i = DragToNodeHitPath.Count - 1; i >= 0; --i)
                    if (DragFromNodeHitPath.Contains(DragToNodeHitPath[i]))
                        return DragToNodeHitPath[i].As<TNode>();
                return null;
            }

            private D2dGraphEdgeEditAdapter<TNode, TEdge, TEdgeRoute> m_edgeEditAdapter;

        };

        private readonly EdgeDraggingContext m_draggingContext;

        /// <summary>
        /// Gets edge dragging context</summary>
        protected EdgeDraggingContext DraggingContext
        {
            get
            {
                m_draggingContext.MousePick = m_mousePick;
                return m_draggingContext;
            }
        }

        /// <summary>
        /// Binds the adapter to the adaptable control; called in the order that the adapters
        /// were defined on the control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Bind(AdaptableControl control)
        {
            m_autoTranslateAdapter = control.As<IAutoTranslateAdapter>();

            var d2dControl = control as D2dAdaptableControl;
            d2dControl.ContextChanged += control_ContextChanged;
            d2dControl.DrawingD2d += control_Paint;
            base.Bind(control);
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            var d2dControl = control as D2dAdaptableControl;
            d2dControl.ContextChanged -= control_ContextChanged;
            d2dControl.DrawingD2d -= control_Paint;

            base.Unbind(control);
        }

        /// <summary>
        /// Context changed event handler</summary>
        /// <param name="sender">Adaptable control</param>
        /// <param name="e">Event args</param>
        protected virtual void control_ContextChanged(object sender, EventArgs e)
        {
            m_mainGraph = AdaptedControl.ContextAs<IGraph<TNode, TEdge, TEdgeRoute>>();
            m_mainEditableGraph = AdaptedControl.ContextAs<IEditableGraph<TNode, TEdge, TEdgeRoute>>();
        }

        /// <summary>
        /// DrawingD2d event handler</summary>
        /// <param name="sender">Adaptable control</param>
        /// <param name="e">Event args</param>
        protected virtual void control_Paint(object sender, EventArgs e)
        {
            if (!m_isConnecting)
                return;

            var d2dControl = this.AdaptedControl as D2dAdaptableControl;
            D2dGraphics gfx = d2dControl.D2dGraphics;

            string label = m_draggingContext.ExistingEdge != null ? m_draggingContext.ExistingEdge.Label : null;


            TNode dragFromNode = m_draggingContext.ActualFromNode();
            TEdgeRoute dragFromRoute = m_draggingContext.ActualFromRoute(dragFromNode);
            TNode dragToNode = m_draggingContext.ActualToNode();
            TEdgeRoute dragToRoute = m_draggingContext.ActualToRoute(dragToNode);

            Debug.Assert(dragFromRoute != null || dragToRoute != null);

            PointF start = dragFromRoute == null ? m_edgeDragPoint : m_draggingContext.FromRoutePos;
            PointF end = dragToRoute == null ? m_edgeDragPoint : m_draggingContext.ToRoutePos;

            m_renderer.DrawPartialEdge(dragFromNode, dragFromRoute, dragToNode, dragToRoute, label,
                start, end, gfx);
        }

        /// <summary>
        /// Performs custom actions on adaptable control MouseMove events; base method should
        /// be called first</summary>
        /// <param name="sender">Adaptable control</param>
        /// <param name="e">Event args</param>
        protected override void OnMouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(sender, e);

            if (m_isConnecting)
                ConnectWires(e);

            if (!m_isConnecting && e.Button == MouseButtons.None && AdaptedControl.Focused)
            {
                m_mousePick = m_graphAdapter.Pick(CurrentPoint);

                // wires can be edited if we're over a route
                bool wiring =
                    m_mousePick.FromRoute != null ||
                    m_mousePick.ToRoute != null;

                if (wiring && AdaptedControl.Cursor == Cursors.Default)
                {
                    AdaptedControl.Cursor = OverRouteCursor;
                }
            }
        }

        /// <summary>
        /// Performs custom actions when performing a mouse dragging operation</summary>
        /// <param name="e">Mouse event args</param>
        protected override void OnDragging(MouseEventArgs e)
        {
            ConnectWires(e);
        }

        /// <summary>
        /// Performs custom actions on adaptable control MouseUp events. On a double-click, this
        /// method gets called twice.</summary>
        /// <param name="sender">Adaptable control</param>
        /// <param name="e">Event args</param>
        /// <remarks>If a derived class needs to do this work on the MouseClick event, then
        /// override this method and do *not* call the base class's OnMouseUp().</remarks>
        protected override void OnMouseUp(object sender, MouseEventArgs e)
        {
            // This method should not do any other logic, so that clients can choose whether
            //  to call DoMouseClick() from here or from OnMouseClick().
            DoMouseClick(e);
        }

        /// <summary>
        /// Performs custom actions on adaptable control MouseClick events. On a double-click, this
        /// method is only called once.</summary>
        /// <param name="sender">Adaptable control</param>
        /// <param name="e">Event args</param>
        /// <remarks>If a derived class needs to do the edge connecting and disconnecting
        /// in this method, then 1) override this method and call DoMouseClick() and 2)
        /// also override OnMouseUp() and make OnMouseUp() do nothing.</remarks>
        protected override void OnMouseClick(object sender, MouseEventArgs e)
        {
            // This method should do nothing. Clients can override and call DoMouseClick().
        }

        /// <summary>
        /// Creates an edge between two nodes, and deleting any existing edges if necessary</summary>
        protected virtual void MakeConnection()
        {
            // disconnect any existing edge on the node route
            if (m_draggingContext.DisconnectEdge != null)
            {
                DraggingContext.EditableGraph.Disconnect(m_draggingContext.DisconnectEdge);
            }

            if (m_draggingContext.ExistingEdge != null)
            {
                DraggingContext.EditableGraph.Disconnect(m_draggingContext.ExistingEdge);
            }


            TNode dragFromNode = m_draggingContext.ActualFromNode();
            TEdgeRoute dragFromRoute = m_draggingContext.ActualFromRoute(dragFromNode);
            TNode dragToNode = m_draggingContext.ActualToNode();
            TEdgeRoute dragToRoute = m_draggingContext.ActualToRoute(dragToNode);

            if (dragToNode != null && dragToRoute != null &&
                dragFromNode != null && dragFromRoute != null)
            {
                DraggingContext.EditableGraph.Connect(dragFromNode, dragFromRoute, dragToNode, dragToRoute, m_draggingContext.ExistingEdge);
            }
        }

        /// <summary>
        /// Does the work of connecting and disconnecting wires. Is called by OnMouseUp(),
        /// but clients may want to call it from OnMouseClick().</summary>
        /// <param name="e">Mouse event arguments</param>
        protected void DoMouseClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left &&
                ((Control.ModifierKeys & Keys.Alt) == 0))
            {
                if (!m_isConnecting)
                {
                    ConnectWires(e);
                }
                else
                {
                    // Attempt to complete the connection
                    if (m_dragEdgeReversed)
                    {
                        if (CanConnectTo())
                        {
                            m_draggingContext.DisconnectEdge = GetDisconnectEdgeTo();
                        }
                    }
                    else
                    {
                        if (CanConnectFrom())
                        {
                            m_draggingContext.DisconnectEdge = GetDisconnectEdgeFrom();
                        }
                    }

                    // only if dragging reaches both start/end pins, make a new wire
                    if (m_draggingContext.DragToNode != null && m_draggingContext.DragToRoute != null &&
                        m_draggingContext.DragFromNode != null && m_draggingContext.DragFromRoute != null)
                    {
                        // make sure drag changed the edge
                        if (m_draggingContext.ExistingEdge == null || // this is a new edge
                            m_draggingContext.ExistingEdge.ToNode != m_draggingContext.DragToNode ||
                            m_draggingContext.ExistingEdge.ToRoute != m_draggingContext.DragToRoute ||
                            m_draggingContext.ExistingEdge.FromNode != m_draggingContext.DragFromNode ||
                            m_draggingContext.ExistingEdge.FromRoute != m_draggingContext.DragFromRoute)
                        {
                            ITransactionContext transactionContext = AdaptedControl.ContextAs<ITransactionContext>();
                            transactionContext.DoTransaction(MakeConnection, "Drag Edge".Localize());
                        }
                    }

                    if (m_autoTranslateAdapter != null)
                        m_autoTranslateAdapter.Enabled = false;

                    m_isConnecting = false;
                    m_draggingContext.DragFromNode = null;
                    m_draggingContext.DragFromRoute = null;
                    m_draggingContext.DragToNode = null;
                    m_draggingContext.DragToRoute = null;
                    m_draggingContext.ExistingEdge = null;
                    m_draggingContext.DisconnectEdge = null;
                    m_graphAdapter.HideEdge(null);

                    AdaptedControl.AutoResetCursor = true;
                    AdaptedControl.Cursor = m_oldCursor;
                    m_renderer.RouteConnecting = null;

                    AdaptedControl.Invalidate();
                }
            }
        }

        private void ConnectWires(MouseEventArgs e)
        {
            if (m_mainEditableGraph != null &&
                !m_isConnecting)
            {
                if (e.Button == MouseButtons.Left &&
                    ((Control.ModifierKeys & Keys.Alt) == 0) &&
                    !AdaptedControl.Capture)
                {
                    // reset dragging context
                    m_draggingContext.DisconnectEdge = null;
                    m_draggingContext.DragFromNode = null;
                    m_draggingContext.DragToNode = null;
                    m_draggingContext.DragFromRoute = null;
                    m_draggingContext.DragToRoute = null;

                    m_mousePick = m_graphAdapter.Pick(FirstPoint);
                    if (m_mousePick.Node != null)
                    {
                        Cursor cursor = AdaptedControl.Cursor;
                        m_edgeDragPoint = FirstPoint;

                        //m_existingEdge = m_mousePick.Edge; // Don't pick wires under nodes

                        // reversed if dragging edge from its destination node's ToRoute(input pin) 
                        // towards source node's FromRoute(output pin)                  
                        m_draggingContext.FromSourceToDestination = false;

                        // if no edge is picked but there are fan-in/out restrictions, try to drag an existing edge
                        if (m_draggingContext.ExistingEdge == null)
                        {
                            if (m_mousePick.FromRoute != null && !m_mousePick.FromRoute.AllowFanOut)
                            {
                                m_draggingContext.ExistingEdge = GetFirstEdgeFrom(m_draggingContext.ActualFromNode(), m_mousePick.FromRoute);
                            }
                            else if (m_mousePick.ToRoute != null && !m_mousePick.ToRoute.AllowFanIn)
                            {
                                m_draggingContext.ExistingEdge = GetFirstEdgeTo(m_draggingContext.ActualToNode(), m_mousePick.ToRoute);
                            }
                        }

                        TNode startNode = null;
                        TEdgeRoute startRoute = null;

                        if (m_mousePick.FromRoute != null) // favor dragging from source to destination
                        {
                            startNode = m_draggingContext.DragFromNode = m_mousePick.SubNode ?? m_mousePick.Node;
                            m_draggingContext.DragFromNodeHitPath = m_mousePick.HitPath;
                            startRoute = m_draggingContext.DragFromRoute = m_mousePick.FromRoute;
                            m_draggingContext.FromRoutePos = m_mousePick.FromRoutePos;

                            m_dragEdgeReversed = true;
                            m_draggingContext.FromSourceToDestination = true;
                            m_isConnecting = true;
                            cursor = OverRouteCursor;
                        }
                        else if (m_mousePick.ToRoute != null)
                        {
                            startNode = m_draggingContext.DragToNode = m_mousePick.SubNode ?? m_mousePick.Node;
                            m_draggingContext.DragToNodeHitPath = m_mousePick.HitPath;
                            startRoute = m_draggingContext.DragToRoute = m_mousePick.ToRoute;
                            m_draggingContext.ToRoutePos = m_mousePick.ToRoutePos;
                            m_dragEdgeReversed = false;
                            m_isConnecting = true;
                            cursor = OverRouteCursor;
                        }

                        if (m_isConnecting)
                        {
                            m_oldCursor = AdaptedControl.Cursor;
                            AdaptedControl.AutoResetCursor = false;
                            AdaptedControl.Cursor = cursor;
                            AdaptedControl.Capture = true;
                            if (startNode != null)
                            {
                                var info = new D2dGraphRenderer<TNode, TEdge, TEdgeRoute>.RouteConnectingInfo()
                                {
                                    EditableGraph = DraggingContext.EditableGraph,
                                    StartNode = startNode,
                                    StartRoute = startRoute
                                };
                                m_renderer.RouteConnecting = info;
                            }
                            else
                            {
                                m_renderer.RouteConnecting = null;
                            }

                            if (m_autoTranslateAdapter != null)
                                m_autoTranslateAdapter.Enabled = true;
                        }
                    }
                    m_graphAdapter.HideEdge(m_draggingContext.ExistingEdge);
                }
            }

            if (m_isConnecting)
            {
                m_edgeDragPoint = CurrentPoint;
                m_mousePick = m_graphAdapter.Pick(CurrentPoint);
                Cursor cursor;

                if (m_dragEdgeReversed)
                {
                    if (CanConnectTo())
                    {
                        m_draggingContext.DragToNode = m_mousePick.SubNode ?? m_mousePick.Node;
                        m_draggingContext.DragToNodeHitPath = m_mousePick.HitPath;
                        m_draggingContext.DragToRoute = m_mousePick.ToRoute;
                        m_draggingContext.ToRoutePos = m_mousePick.ToRoutePos;
                        cursor = ToPlaceCursor;
                    }
                    else
                    {
                        m_draggingContext.DragToNode = null;
                        m_draggingContext.DragToRoute = null;
                        cursor = InadmissibleCursor;
                    }
                }
                else
                {
                    if (CanConnectFrom())
                    {
                        m_draggingContext.DragFromNode = m_mousePick.SubNode ?? m_mousePick.Node;
                        m_draggingContext.DragFromNodeHitPath = m_mousePick.HitPath;
                        m_draggingContext.DragFromRoute = m_mousePick.FromRoute;
                        m_draggingContext.FromRoutePos = m_mousePick.FromRoutePos;
                        cursor = FromPlaceCursor;
                    }
                    else
                    {
                        m_draggingContext.DragFromNode = null;
                        m_draggingContext.DragFromRoute = null;
                        cursor = InadmissibleCursor;
                    }
                }

                AdaptedControl.AutoResetCursor = false;
                AdaptedControl.Cursor = cursor;
                var d2dControl = this.AdaptedControl as D2dAdaptableControl;
                d2dControl.DrawD2d();
            }
        }

        /// <summary>
        /// Can the user create a connection by dragging, starting from the IGraphEdge's FromNode
        /// (e.g., the Output pin of one node) to the IGraphEdge's ToNode (e.g., the Input pin
        /// of another node)?</summary>
        /// <returns>True iff user can create connection</returns>
        protected virtual bool CanConnectTo()
        {
            // m_dragFromNode contains the starting node of the drag operation -- the IGraphEdge's FromNode.
            // m_mousePick.Node contains the ending node of the drag operation -- the IGraphEdige's ToNode.

            if (m_mainEditableGraph == null || m_mousePick.Node == null || m_mousePick.ToRoute == null)
                return false;

            m_draggingContext.DragToNode = m_mousePick.SubNode ?? m_mousePick.Node;
            m_draggingContext.DragToRoute = m_mousePick.ToRoute;
            m_draggingContext.ToRoutePos = m_mousePick.ToRoutePos;
            m_draggingContext.DragToNodeHitPath = m_mousePick.HitPath; // update DragToNodeHitPath for DraggingContext.EditableGraph

            TNode dragFromNode = m_draggingContext.ActualFromNode();
            TEdgeRoute dragFromRoute = m_draggingContext.ActualFromRoute(dragFromNode);
            TNode dragToNode = m_draggingContext.ActualToNode();
            TEdgeRoute dragToRoute = m_draggingContext.ActualToRoute(dragToNode);

            //// --> debug
            //if (m_draggingContext.DragFromNode != null && m_draggingContext.DragFromRoute != null &&
            //    m_draggingContext.DragToNode != null && m_draggingContext.DragToRoute != null)
            //{
            //    if (m_draggingContext.DragFromNode != m_draggingContext.DragToNode)
            //    {
            //        dragToNode = m_draggingContext.ActualToNode();
            //        dragToRoute = m_draggingContext.ActualToRoute(dragToNode);
            //        bool result = DraggingContext.EditableGraph.CanConnect(dragFromNode, dragFromRoute, dragToNode, dragToRoute);
            //    }
            //}// <-- debug

            return DraggingContext.EditableGraph.CanConnect(dragFromNode, dragFromRoute, dragToNode, dragToRoute);
        }

        /// <summary>
        /// Can the user create a connection by dragging, starting from the IGraphEdge's ToNode
        ///  (e.g., Input pin of one node) to the IGraphEdge's FromNode (e.g., the Output pin of
        ///  another node)?</summary>
        /// <returns>True iff user can create connection</returns>
        protected virtual bool CanConnectFrom()
        {
            // m_mousePick.Node contains the ending node of the drag operation -- the IGraphEdge's FromNode.
            // m_dragToNode contains the starting node of the drag operation -- the IGraphEdge's ToNode.

            if (m_mainEditableGraph == null || m_mousePick.Node == null || m_mousePick.FromRoute == null)
                return false;

            m_draggingContext.DragFromNode = m_mousePick.SubNode ?? m_mousePick.Node;
            m_draggingContext.DragFromRoute = m_mousePick.FromRoute;
            m_draggingContext.FromRoutePos = m_mousePick.FromRoutePos;
            m_draggingContext.DragFromNodeHitPath = m_mousePick.HitPath;// update DragFromNodeHitPath for DraggingContext.EditableGraph

            TNode dragFromNode = m_draggingContext.ActualFromNode();
            TEdgeRoute dragFromRoute = m_draggingContext.ActualFromRoute(dragFromNode);
            TNode dragToNode = m_draggingContext.ActualToNode();
            TEdgeRoute dragToRoute = m_draggingContext.ActualToRoute(dragToNode);

            return DraggingContext.EditableGraph.CanConnect(dragFromNode, dragFromRoute, dragToNode, dragToRoute);
        }

        private TEdge GetDisconnectEdgeTo()
        {
            TEdge result = null;
            if (!m_mousePick.ToRoute.AllowFanIn)
            {
                TNode dragToNode = m_draggingContext.ActualToNode();
                result = GetFirstEdgeTo(dragToNode, m_mousePick.ToRoute);
                if (!CanDisconnect(result))
                    result = null;
            }
            return result;
        }

        private TEdge GetDisconnectEdgeFrom()
        {
            TEdge result = null;
            if (!m_mousePick.FromRoute.AllowFanOut)
            {
                result = GetFirstEdgeFrom(m_draggingContext.ActualFromNode(), m_mousePick.FromRoute);
                if (!CanDisconnect(result))
                    result = null;
            }

            return result;
        }

        /// <summary>
        /// Gets whether the edge can be disconnected</summary>
        /// <param name="edge">Edge to disconnect</param>
        /// <returns>Whether the edge can be disconnected</returns>
        protected bool CanDisconnect(TEdge edge)
        {
            return
                edge != m_draggingContext.ExistingEdge &&
                DraggingContext.EditableGraph.CanDisconnect(edge);
        }

        /// <summary>
        /// Gets the IGraphEdge to a given IGraphNode with given "to" IEdgeRoute</summary>
        /// <param name="node">"To" IGraphNode</param>
        /// <param name="toRoute">"To" IEdgeRoute</param>
        /// <returns>IGraphEdge with given "to" IGraphNode and "to" IEdgeRoute</returns>
        protected TEdge GetFirstEdgeTo(TNode node, object toRoute)
        {
            if (m_mousePick.SubNode == null) // top level
                return FindFirstEdgeInGraph(m_mainGraph, node, toRoute as TEdgeRoute, false);


            IGraph<TNode, TEdge, TEdgeRoute> parenGraph = null;
            object parent = null;
            for (int i = m_mousePick.HitPath.Count - 1; i >= 0; --i)
            {
                var current  = m_mousePick.HitPath[i];
                if (current == node)
                {
                    if (i > 0)
                        parent = m_mousePick.HitPath[i - 1];
                    break;
                }
            }
       

            parenGraph = parent.As<IGraph<TNode, TEdge, TEdgeRoute>>();
            if (parenGraph == null)
                return null;


            if (EdgeRouteTraverser != null) // ask for external helper, because client know more about the graph hierarchy
            {
                TEdgeRoute edgeRoute = EdgeRouteTraverser(m_mousePick.HitPath, node, toRoute.Cast<TEdgeRoute>());
                if (parenGraph != null)
                    return FindFirstEdgeInGraph(parenGraph, node, edgeRoute, false);

            }

            return null;
        }



        /// <summary>
        /// Gets the IGraphEdge from a given IGraphNode with given "from" IEdgeRoute</summary>
        /// <param name="node">"From" IGraphNode</param>
        /// <param name="fromRoute">"From" IEdgeRoute</param>
        /// <returns>IGraphEdge with given "from" IGraphNode and "from" IEdgeRoute</returns>
        protected TEdge GetFirstEdgeFrom(TNode node, object fromRoute)
        {
            if (m_mousePick.SubNode == null) // top level
                return FindFirstEdgeInGraph(m_mainGraph, node, fromRoute as TEdgeRoute, true);

            IGraph<TNode, TEdge, TEdgeRoute> parenGraph = null;
            object parent = null;
            for (int i = m_mousePick.HitPath.Count - 1; i >= 0; --i)
            {
                var current = m_mousePick.HitPath[i];
                if (current == node)
                {
                    if (i > 0)
                        parent = m_mousePick.HitPath[i - 1];
                    break;
                }
            }


            parenGraph = parent.As<IGraph<TNode, TEdge, TEdgeRoute>>();
            if (parenGraph == null)
                return null;
            if (EdgeRouteTraverser != null)
            {
                TEdgeRoute edgeRoute = EdgeRouteTraverser(m_mousePick.HitPath, parent, fromRoute.Cast<TEdgeRoute>());

                if (parenGraph != null)
                    return FindFirstEdgeInGraph(parenGraph, node, edgeRoute, true);

            }

            return null;
        }

        private TEdge FindFirstEdgeInGraph(IGraph<TNode, TEdge, TEdgeRoute> graph, TNode node, TEdgeRoute route, bool fromEdge)
        {
            if (fromEdge)
            {
                foreach (TEdge edge in graph.Edges)
                    if (edge.FromNode == node && edge.FromRoute.Equals(route))
                        return edge;
            }
            else
            {
                foreach (TEdge edge in graph.Edges)
                    if (edge.ToNode == node && edge.ToRoute.Equals(route))
                        return edge;
            }
            return null;
        }

        private readonly D2dGraphRenderer<TNode, TEdge, TEdgeRoute> m_renderer;
        private readonly D2dGraphAdapter<TNode, TEdge, TEdgeRoute> m_graphAdapter;
        private IAutoTranslateAdapter m_autoTranslateAdapter;

        private IGraph<TNode, TEdge, TEdgeRoute> m_mainGraph;
        private IEditableGraph<TNode, TEdge, TEdgeRoute> m_mainEditableGraph;

        private GraphHitRecord<TNode, TEdge, TEdgeRoute> m_mousePick = new GraphHitRecord<TNode, TEdge, TEdgeRoute>();

        private Point m_edgeDragPoint;
        private Cursor m_oldCursor;

        private bool m_isConnecting; //is the user connecting wires, either by dragging or clicking pins
        private bool m_dragEdgeReversed;

    }
}
