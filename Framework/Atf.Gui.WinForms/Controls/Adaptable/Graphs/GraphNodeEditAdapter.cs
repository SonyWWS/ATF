//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// OBSOLETE. Please use D2dGraphNodeEditAdapter instead.
    /// Adapter that adds graph node dragging capabilities to an adapted control
    /// with a graph adapter.</summary>
    /// <typeparam name="TNode">Node type, must implement IGraphNode</typeparam>
    /// <typeparam name="TEdge">Edge type, must implement IGraphEdge</typeparam>
    /// <typeparam name="TEdgeRoute">Edge route type, must implement IEdgeRoute</typeparam>
    public class GraphNodeEditAdapter<TNode, TEdge, TEdgeRoute> : DraggingControlAdapter, IItemDragAdapter
        where TNode : class, IGraphNode
        where TEdge : class, IGraphEdge<TNode, TEdgeRoute>
        where TEdgeRoute : class, IEdgeRoute
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="renderer">Graph renderer</param>
        /// <param name="graphAdapter">Graph adapter</param>
        /// <param name="transformAdapter">Transform adapter</param>
        public GraphNodeEditAdapter(
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
                ResetHotNode();
            }
        }

        #region IItemDragAdapter Members

        void IItemDragAdapter.BeginDrag(ControlAdapter initiator)
        {
            m_isDragging = true;

            // drag all selected nodes, and any edges impinging on them
            ActiveCollection<TNode> draggingNodes = new ActiveCollection<TNode>();
            List<TEdge> draggingEdges = new List<TEdge>();
            HashSet<TNode> nodes = new HashSet<TNode>();

            foreach (TNode node in m_selectionContext.GetSelection<TNode>())
            {
                AddDragNode(node, draggingNodes, nodes);
            }

            // render all edges connected to the dragging nodes
            foreach (TEdge edge in m_graph.Edges)
            {
                if (nodes.Contains(edge.FromNode) ||
                    nodes.Contains(edge.ToNode))
                {
                    draggingEdges.Add(edge);

                    m_graphAdapter.SetStyle(edge, DiagramDrawingStyle.Ghosted);
                }
            }

            m_draggingNodes = draggingNodes.GetSnapshot<TNode>();
            m_newPositions = new Point[m_draggingNodes.Length];
            m_oldPositions = new Point[m_draggingNodes.Length];
            for (int i = 0; i < m_draggingNodes.Length; i++)
            {
                // Initialize m_newPositions in case the mouse up event occurs before
                //  a paint event, which can happen during rapid clicking.
                Point currentLocation = m_draggingNodes[i].Bounds.Location;
                m_newPositions[i] = currentLocation;
                m_oldPositions[i] = currentLocation;
            }

            m_draggingEdges = draggingEdges.ToArray();
        }

        void IItemDragAdapter.EndingDrag()
        {
        }

        void IItemDragAdapter.EndDrag()
        {
            foreach (TNode node in m_draggingNodes)
                m_graphAdapter.ResetStyle(node);

            foreach (TEdge edge in m_draggingEdges)
                m_graphAdapter.ResetStyle(edge);

            int i = 0;
            foreach (TNode node in m_draggingNodes)
            {
                MoveNode(node, m_newPositions[i]);
                i++;
            }

            m_draggingNodes = null;
            m_newPositions = null;
            m_oldPositions = null;
            m_draggingEdges = null;
            m_isDragging = false;
        }

        #endregion

        /// <summary>
        /// Binds the adapter to the adaptable control; called in the order that the adapters
        /// were defined on the control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Bind(AdaptableControl control)
        {
            m_autoTranslateAdapter = control.As<IAutoTranslateAdapter>();

            control.ContextChanged += new EventHandler(control_ContextChanged);
            control.Paint += new PaintEventHandler(control_Paint);

            base.Bind(control);
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            control.ContextChanged -= new EventHandler(control_ContextChanged);
            control.Paint -= new PaintEventHandler(control_Paint);

            base.Unbind(control);
        }

        private void control_ContextChanged(object sender, EventArgs e)
        {
            m_graph = AdaptedControl.ContextAs<IGraph<TNode, TEdge, TEdgeRoute>>();
            m_layoutContext = AdaptedControl.ContextAs<ILayoutContext>();

            if (m_layoutContext != null)
            {
                m_selectionContext = AdaptedControl.ContextCast<ISelectionContext>();
            }
        }

        private void control_Paint(object sender, PaintEventArgs e)
        {
            if (m_draggingNodes != null)
            {
                Graphics g = e.Graphics;
                Matrix transform = m_transformAdapter.Transform;
                Matrix oldTransform = g.Transform;
                Region oldClip = g.Clip;

                g.Transform = transform;
                Rectangle clip = GdiUtil.InverseTransform(g.Transform, e.ClipRectangle);
                g.SetClip(clip);

                Point currentPoint = GdiUtil.InverseTransform(m_transformAdapter.Transform, CurrentPoint);
                Point delta = new Point(currentPoint.X - m_firstPoint.X, currentPoint.Y - m_firstPoint.Y);

                // set dragged nodes' positions, offsetting by drag delta and applying layout constraints
                for (int i = 0; i < m_draggingNodes.Length; i++)
                {
                    TNode node = m_draggingNodes[i];
                    Rectangle bounds;
                    m_layoutContext.GetBounds(node, out bounds);
                    bounds.X += delta.X;
                    bounds.Y += delta.Y;
                    m_newPositions[i] = bounds.Location;
                    m_layoutContext.SetBounds(node, bounds, BoundsSpecified.Location);
                }

                // draw dragging nodes and edges
                foreach (TNode node in m_draggingNodes)
                    m_renderer.Draw(node, DiagramDrawingStyle.Normal, g);
                foreach (TEdge edge in m_draggingEdges)
                    m_renderer.Draw(edge, DiagramDrawingStyle.Normal, g);

                // restore dragged nodes' positions
                for (int i = 0; i < m_draggingNodes.Length; i++)
                {
                    TNode node = m_draggingNodes[i];
                    MoveNode(node, m_oldPositions[i]);
                }

                g.Transform = oldTransform;
                g.Clip = oldClip;
            }
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
                if (m_hotTrack)
                {
                    m_mousePick = m_graphAdapter.Pick(CurrentPoint);

                    // if over a node, but not on an edge or route
                    TNode hotNode = null;
                    if (m_mousePick.Node != null &&
                        m_mousePick.Edge == null &&
                        m_mousePick.FromRoute == null &&
                        m_mousePick.ToRoute == null)
                    {
                        hotNode = m_mousePick.Node;
                    }

                    if (hotNode != m_hotNode)
                    {
                        ResetHotNode();

                        SetHotNode(hotNode);
                    }
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

            ResetHotNode();
        }

        private void SetHotNode(TNode hotNode)
        {
            m_hotNode = hotNode;
            if (hotNode != null)
                m_graphAdapter.SetStyle(m_hotNode, DiagramDrawingStyle.Hot);

            if (AdaptedControl.Focused)
                AdaptedControl.Invalidate();
        }

        private void ResetHotNode()
        {
            if (m_hotNode != null)
            {
                m_graphAdapter.ResetStyle(m_hotNode);
                m_hotNode = null;

                if (AdaptedControl.Focused)
                    AdaptedControl.Invalidate();
            }
        }

        /// <summary>
        /// Performs custom actions when performing a mouse dragging operation</summary>
        /// <param name="e">Mouse move event args</param>
        protected override void OnDragging(MouseEventArgs e)
        {
            if (m_layoutContext != null &&
                !m_isDragging)
            {
                ResetHotNode();

                if (e.Button == MouseButtons.Left &&
                    ((Control.ModifierKeys & Keys.Alt) == 0) &&
                    !AdaptedControl.Capture)
                {
                    {
                        m_firstPoint = GdiUtil.InverseTransform(m_transformAdapter.Transform, FirstPoint);
                        m_mousePick = m_graphAdapter.Pick(FirstPoint);
                        if (m_mousePick.Node != null)
                        {
                            m_initiated = true;

                            foreach (IItemDragAdapter itemDragAdapter in AdaptedControl.AsAll<IItemDragAdapter>())
                                itemDragAdapter.BeginDrag(this);

                            AdaptedControl.Capture = true;

                            if (m_autoTranslateAdapter != null)
                                m_autoTranslateAdapter.Enabled = true;
                        }
                    }
                }
            }

            if (m_draggingNodes != null)
            {
                AdaptedControl.Invalidate();
            }
        }

        private void AddDragNode(TNode node, ActiveCollection<TNode> draggingNodes, HashSet<TNode> nodes)
        {
            draggingNodes.Add(node);
            if (!nodes.Contains(node))
            {
                nodes.Add(node);
                m_graphAdapter.SetStyle(node, DiagramDrawingStyle.Ghosted);
            }

            IHierarchicalGraphNode<TNode, TEdge, TEdgeRoute> hierarchicalNode =
                Adapters.As<IHierarchicalGraphNode<TNode, TEdge, TEdgeRoute>>(node);
            if (hierarchicalNode != null)
            {
                foreach (TNode subNode in hierarchicalNode.SubNodes)
                {
                    AddDragNode(subNode, draggingNodes, nodes);
                }
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
                if (m_draggingNodes != null)
                {
                    if (m_initiated)
                    {
                        ITransactionContext transactionContext = AdaptedControl.ContextAs<ITransactionContext>();
                        TransactionContexts.DoTransaction(transactionContext,
                            delegate
                            {
                                foreach (IItemDragAdapter itemDragAdapter in AdaptedControl.AsAll<IItemDragAdapter>())
                                    itemDragAdapter.EndDrag();
                            },
                            "Drag Items".Localize());

                        m_initiated = false;
                    }

                    AdaptedControl.Invalidate();
                }

                if (m_autoTranslateAdapter != null)
                    m_autoTranslateAdapter.Enabled = false;
            }
        }

        private void MoveNode(TNode node, Point location)
        {
            Rectangle bounds;
            m_layoutContext.GetBounds(node, out bounds);
            bounds.Location = location;
            m_layoutContext.SetBounds(node, bounds, BoundsSpecified.Location);
        }

        private readonly GraphRenderer<TNode, TEdge, TEdgeRoute> m_renderer;
        private readonly IGraphAdapter<TNode, TEdge, TEdgeRoute> m_graphAdapter;
        private readonly ITransformAdapter m_transformAdapter;
        private IAutoTranslateAdapter m_autoTranslateAdapter;

        private IGraph<TNode, TEdge, TEdgeRoute> m_graph;
        private ILayoutContext m_layoutContext;
        private ISelectionContext m_selectionContext;

        private GraphHitRecord<TNode, TEdge, TEdgeRoute> m_mousePick = new GraphHitRecord<TNode, TEdge, TEdgeRoute>();
        private TNode m_hotNode;

        private TNode[] m_draggingNodes;
        private Point[] m_newPositions;
        private Point[] m_oldPositions;
        private TEdge[] m_draggingEdges;
        private Point m_firstPoint;

        private bool m_hotTrack = true;
        private bool m_isDragging;
        private bool m_initiated;
    }
}
