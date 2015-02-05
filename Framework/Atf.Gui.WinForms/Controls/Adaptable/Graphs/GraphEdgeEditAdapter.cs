//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// OBSOLETE. Please use D2dGraphEdgeEditAdapter instead.
    /// Adapter that adds graph edge dragging capabilities to an adapted control
    /// with a graph adapter.</summary>
    /// <typeparam name="TNode">Node type, must implement IGraphNode</typeparam>
    /// <typeparam name="TEdge">Edge type, must implement IGraphEdge</typeparam>
    /// <typeparam name="TEdgeRoute">Edge route type, must implement IEdgeRoute</typeparam>
    public class GraphEdgeEditAdapter<TNode, TEdge, TEdgeRoute> : DraggingControlAdapter
        where TNode : class, IGraphNode
        where TEdge : class, IGraphEdge<TNode, TEdgeRoute>
        where TEdgeRoute : class, IEdgeRoute
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="renderer">Graph renderer</param>
        /// <param name="graphAdapter">Graph adapter</param>
        /// <param name="transformAdapter">Transform adapter</param>
        public GraphEdgeEditAdapter(
            GraphRenderer<TNode, TEdge, TEdgeRoute> renderer,
            IGraphAdapter<TNode, TEdge, TEdgeRoute> graphAdapter,
            ITransformAdapter transformAdapter)
        {
            m_renderer = renderer;
            m_graphAdapter = graphAdapter;
            m_transformAdapter = transformAdapter;
        }

        /// <summary>
        /// Gets or sets a value enabling hot-tracking of edges</summary>
        public bool HotTrack
        {
            get { return m_hotTrack; }
            set
            {
                m_hotTrack = value;
                ResetHotEdge();
            }
        }

        /// <summary>
        /// Gets a value indicating if the user is currently dragging a graph edge</summary>
        public bool IsDraggingEdge
        {
            get { return m_isDragging; }
        }

        /// <summary>
        /// Binds the adapter to the adaptable control; called in the order that the adapters
        /// were defined on the control.</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Bind(AdaptableControl control)
        {
            m_autoTranslateAdapter = control.As<IAutoTranslateAdapter>();

            control.ContextChanged += control_ContextChanged;
            control.Paint += control_Paint;

            base.Bind(control);
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            control.ContextChanged -= control_ContextChanged;
            control.Paint -= control_Paint;

            base.Unbind(control);
        }

        private void control_ContextChanged(object sender, EventArgs e)
        {
            m_graph = AdaptedControl.ContextAs<IGraph<TNode, TEdge, TEdgeRoute>>();
            m_editableGraph = AdaptedControl.ContextAs<IEditableGraph<TNode, TEdge, TEdgeRoute>>();
        }

        private void control_Paint(object sender, PaintEventArgs e)
        {
            if (!m_isDragging)
                return;

            Graphics g = e.Graphics;
            Matrix transform = m_transformAdapter.Transform;
            Matrix oldTransform = g.Transform;
            Region oldClip = g.Clip;

            g.Transform = transform;
            Rectangle clip = GdiUtil.InverseTransform(g.Transform, e.ClipRectangle);
            g.SetClip(clip);

            if (m_disconnectEdge != null)
                m_renderer.Draw(m_disconnectEdge, DiagramDrawingStyle.Ghosted, g);

            string label = m_existingEdge != null ? m_existingEdge.Label : null;
            m_renderer.Draw(
                m_dragFromNode, m_dragFromRoute, m_dragToNode, m_dragToRoute, label,
                m_edgeDragPoint,
                g);

            g.Transform = oldTransform;
            g.Clip = oldClip;
        }

        /// <summary>
        /// Performs custom actions on adaptable control MouseMove events; base method should
        /// be called first</summary>
        /// <param name="sender">Adaptable control</param>
        /// <param name="e">Event args</param>
        protected override void OnMouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(sender, e);

            if (e.Button == MouseButtons.None && AdaptedControl.Focused)
            {
                m_mousePick = m_graphAdapter.Pick(CurrentPoint);
                if (m_hotTrack)
                {
                    // if over a node, but not on an edge or route
                    TEdge hotEdge = null;
                    if (m_mousePick.Edge != null)
                    {
                        hotEdge = m_mousePick.Edge;
                    }

                    if (hotEdge != m_hotEdge)
                    {
                        ResetHotEdge();

                        SetHotEdge(hotEdge);
                    }
                }

                // wires can be edited if  we're over a route
                bool wiring =
                    m_mousePick.FromRoute != null ||
                    m_mousePick.ToRoute != null;

                if (wiring &&
                    AdaptedControl.Cursor == Cursors.Default)
                {
                    AdaptedControl.Cursor = Cursors.UpArrow;
                }
            }
        }

        /// <summary>
        /// Performs custom actions on adaptable control MouseLeave events; base method should
        /// be called first</summary>
        /// <param name="sender">Adaptable control</param>
        /// <param name="e">Event args</param>
        protected override void OnMouseLeave(object sender, EventArgs e)
        {
            base.OnMouseLeave(sender, e);

            ResetHotEdge();
        }

        private void SetHotEdge(TEdge hotEdge)
        {
            m_hotEdge = hotEdge;
            if (hotEdge != null)
                m_graphAdapter.SetStyle(m_hotEdge, DiagramDrawingStyle.Hot);

            if (AdaptedControl.Focused)
                AdaptedControl.Invalidate();
        }

        private void ResetHotEdge()
        {
            if (m_hotEdge != null)
            {
                m_graphAdapter.ResetStyle(m_hotEdge);
                m_hotEdge = null;

                if (AdaptedControl.Focused)
                    AdaptedControl.Invalidate();
            }
        }

        /// <summary>
        /// Performs custom actions when performing a mouse dragging operation</summary>
        /// <param name="e">Mouse move event args</param>
        protected override void OnDragging(MouseEventArgs e)
        {
            if (m_editableGraph != null &&
                !m_isDragging)
            {
                ResetHotEdge();

                if (e.Button == MouseButtons.Left &&
                    ((Control.ModifierKeys & Keys.Alt) == 0) &&
                    !AdaptedControl.Capture)
                {
                    {
                        m_mousePick = m_graphAdapter.Pick(FirstPoint);
                        if (m_mousePick.Node != null)
                        {
                            Cursor cursor = AdaptedControl.Cursor;
                            m_edgeDragPoint = FirstPoint;

                            m_existingEdge = m_mousePick.Edge;
                            bool reversed = m_mousePick.FromRoute == null; // TODO explain!

                            // if no edge is picked but there are fan-in/out restrictions, try to drag an existing edge
                            if (m_existingEdge == null)
                            {
                                if (m_mousePick.FromRoute != null && !m_mousePick.FromRoute.AllowFanOut)
                                {
                                    m_existingEdge = GetFirstEdgeFrom(m_mousePick.Node, m_mousePick.FromRoute);
                                    reversed = false; // connecting "from-to"
                                }
                                else if (m_mousePick.ToRoute != null && !m_mousePick.ToRoute.AllowFanIn)
                                {
                                    m_existingEdge = GetFirstEdgeTo(m_mousePick.Node, m_mousePick.ToRoute);
                                    reversed = true; // connecting "to-from"
                                }
                            }

                            if (m_existingEdge != null)
                            {
                                if (m_editableGraph.CanDisconnect(m_existingEdge))
                                {
                                    m_dragFromNode = m_existingEdge.FromNode;
                                    m_dragFromRoute = m_existingEdge.FromRoute;
                                    m_dragToNode = m_existingEdge.ToNode;
                                    m_dragToRoute = m_existingEdge.ToRoute;

                                    m_graphAdapter.SetStyle(m_existingEdge, DiagramDrawingStyle.Ghosted);
                                    m_dragEdgeReversed = reversed;
                                    m_isDragging = true;
                                    cursor = Cursors.UpArrow;
                                }
                            }
                            else if (m_mousePick.FromRoute != null) // favor dragging from source to destination
                            {
                                m_dragFromNode = m_mousePick.Node;
                                m_dragFromRoute = m_mousePick.FromRoute;
                                m_dragEdgeReversed = true;
                                m_isDragging = true;
                                cursor = Cursors.UpArrow;
                            }
                            else if (m_mousePick.ToRoute != null)
                            {
                                m_dragToNode = m_mousePick.Node;
                                m_dragToRoute = m_mousePick.ToRoute;
                                m_dragEdgeReversed = false;
                                m_isDragging = true;
                                cursor = Cursors.UpArrow;
                            }

                            if (m_isDragging)
                            {
                                m_oldCursor = AdaptedControl.Cursor;
                                AdaptedControl.Cursor = cursor;
                                AdaptedControl.Capture = true;

                                if (m_autoTranslateAdapter != null)
                                    m_autoTranslateAdapter.Enabled = true;
                            }
                        }
                    }
                }
            }

            if (m_isDragging)
            {
                m_edgeDragPoint = CurrentPoint;
                m_mousePick = m_graphAdapter.Pick(CurrentPoint);

                if (m_disconnectEdge != null)
                {
                    m_graphAdapter.ResetStyle(m_disconnectEdge);
                    m_disconnectEdge = null;
                }

                if (m_dragEdgeReversed)
                {
                    if (CanConnectTo())
                    {
                        m_dragToNode = m_mousePick.Node;
                        m_dragToRoute = m_mousePick.ToRoute;
                        m_disconnectEdge = GetDisconnectEdgeTo();
                        AdaptedControl.Cursor = Cursors.UpArrow;
                    }
                    else
                    {
                        m_dragToNode = null;
                        m_dragToRoute = null;
                        AdaptedControl.Cursor = Cursors.No;
                    }
                }
                else
                {
                    if (CanConnectFrom())
                    {
                        m_dragFromNode = m_mousePick.Node;
                        m_dragFromRoute = m_mousePick.FromRoute;
                        m_disconnectEdge = GetDisconnectEdgeFrom();
                        AdaptedControl.Cursor = Cursors.UpArrow;
                    }
                    else
                    {
                        m_dragFromNode = null;
                        m_dragFromRoute = null;
                        AdaptedControl.Cursor = Cursors.No;
                    }
                }

                if (m_disconnectEdge != null)
                {
                    m_graphAdapter.SetStyle(m_disconnectEdge, DiagramDrawingStyle.Ghosted);
                }

                AdaptedControl.Invalidate();
            }
        }

        /// <summary>
        /// Performs custom actions on adaptable control MouseUp events; base method should
        /// be called first</summary>
        /// <param name="sender">Adaptable control</param>
        /// <param name="e">Event args</param>
        protected override void OnMouseUp(object sender, MouseEventArgs e)
        {
            base.OnMouseUp(sender, e);

            if (e.Button == MouseButtons.Left)
            {
                if (m_isDragging)
                {
                    if (m_existingEdge != null)
                        m_graphAdapter.ResetStyle(m_existingEdge);

                    if (m_disconnectEdge != null)
                        m_graphAdapter.ResetStyle(m_disconnectEdge);

                    // make sure drag changed the edge
                    if (m_existingEdge == null || // this is a new edge
                        m_existingEdge.ToNode != m_dragToNode ||
                        m_existingEdge.ToRoute != m_dragToRoute ||
                        m_existingEdge.FromNode != m_dragFromNode ||
                        m_existingEdge.FromRoute != m_dragFromRoute)
                    {
                        ITransactionContext transactionContext = AdaptedControl.ContextAs<ITransactionContext>();
                        transactionContext.DoTransaction(delegate
                        {
                            // disconnect any existing edge on the node route
                            if (m_disconnectEdge != null)
                            {
                                m_editableGraph.Disconnect(m_disconnectEdge);
                            }

                            if (m_existingEdge != null)
                            {
                                m_editableGraph.Disconnect(m_existingEdge);
                            }

                            if (m_dragToNode != null &&
                                m_dragToRoute != null &&
                                m_dragFromNode != null &&
                                m_dragFromRoute != null)
                            {
                                m_editableGraph.Connect(
                                    m_dragFromNode,
                                    m_dragFromRoute,
                                    m_dragToNode,
                                    m_dragToRoute,
                                    m_existingEdge);
                            }
                        }, "Drag Edge".Localize());
                    }

                    AdaptedControl.Invalidate();
                }
            }
 
            if (m_autoTranslateAdapter != null)
                m_autoTranslateAdapter.Enabled = false;

            m_isDragging = false;
            m_dragFromNode = null;
            m_dragFromRoute = null;
            m_dragToNode = null;
            m_dragToRoute = null;
            m_existingEdge = null;
            m_disconnectEdge = null;

            AdaptedControl.Cursor = m_oldCursor;
        }

        // Can the user create a connection by dragging, starting from the IGraphEdge's FromNode
        //  (e.g., the Output pin of one node) to the IGraphEdge's ToNode (e.g., the Input pin
        //  of another node)?
        private bool CanConnectTo()
        {
            // m_dragFromNode contains the starting node of the drag operation -- the IGraphEdge's FromNode.
            // m_mousePick.Node contains the ending node of the drag operation -- the IGraphEdige's ToNode.
            return
                m_mousePick.Node != null &&
                m_mousePick.ToRoute != null &&
                m_editableGraph != null &&
                m_editableGraph.CanConnect(m_dragFromNode, m_dragFromRoute, m_mousePick.Node, m_mousePick.ToRoute);
        }

        // Can the user create a connection by dragging, starting from the IGraphEdge's ToNode
        //  (e.g., Input pin of one node) to the IGraphEdge's FromNode (e.g., the Output pin of
        //  another node)?
        private bool CanConnectFrom()
        {
            // m_mousePick.Node contains the ending node of the drag operation -- the IGraphEdge's FromNode.
            // m_dragToNode contains the starting node of the drag operation -- the IGraphEdge's ToNode.
            return
                m_mousePick.Node != null &&
                m_mousePick.FromRoute != null &&
                m_editableGraph != null &&
                m_editableGraph.CanConnect(m_mousePick.Node, m_mousePick.FromRoute, m_dragToNode, m_dragToRoute);
        }

        private TEdge GetDisconnectEdgeTo()
        {
            TEdge result = null;
            if (!m_mousePick.ToRoute.AllowFanIn)
            {
                result = GetFirstEdgeTo(m_mousePick.Node, m_mousePick.ToRoute);
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
                result = GetFirstEdgeFrom(m_mousePick.Node, m_mousePick.FromRoute);
                if (!CanDisconnect(result))
                    result = null;
            }

            return result;
        }

        private bool CanDisconnect(TEdge edge)
        {
            return
                edge != m_existingEdge &&
                m_editableGraph.CanDisconnect(edge);
        }

        private TEdge GetFirstEdgeTo(TNode node, object toRoute)
        {
            foreach (TEdge edge in m_graph.Edges)
                if (edge.ToNode == node && edge.ToRoute.Equals(toRoute))
                    return edge;

            return null;
        }

        private TEdge GetFirstEdgeFrom(TNode node, object fromRoute)
        {
            foreach (TEdge edge in m_graph.Edges)
                if (edge.FromNode == node && edge.FromRoute.Equals(fromRoute))
                    return edge;

            return null;
        }

        private readonly GraphRenderer<TNode, TEdge, TEdgeRoute> m_renderer;
        private readonly IGraphAdapter<TNode, TEdge, TEdgeRoute> m_graphAdapter;
        private readonly ITransformAdapter m_transformAdapter;
        private IAutoTranslateAdapter m_autoTranslateAdapter;

        private IGraph<TNode, TEdge, TEdgeRoute> m_graph;
        private IEditableGraph<TNode, TEdge, TEdgeRoute> m_editableGraph;

        private GraphHitRecord<TNode, TEdge, TEdgeRoute> m_mousePick = new GraphHitRecord<TNode, TEdge, TEdgeRoute>();
        private TEdge m_hotEdge;

        private TNode m_dragFromNode;
        private TEdgeRoute m_dragFromRoute;
        private TNode m_dragToNode;
        private TEdgeRoute m_dragToRoute;

        private TEdge m_existingEdge;
        private TEdge m_disconnectEdge;
        private Point m_edgeDragPoint;
        private Cursor m_oldCursor;

        private bool m_hotTrack = true;
        private bool m_isDragging;
        private bool m_dragEdgeReversed;
    }
}
