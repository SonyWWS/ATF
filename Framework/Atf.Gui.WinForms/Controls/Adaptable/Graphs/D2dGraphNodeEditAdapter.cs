//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;

using System.Linq;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;


namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Adapter that adds graph node dragging capabilities to an adapted control
    /// with a graph adapter. The shift key can be pressed to constrain dragging
    /// to be parallel to either the x-axis or y-axis.</summary>
    /// <typeparam name="TNode">Node type, must implement IGraphNode</typeparam>
    /// <typeparam name="TEdge">Edge type, must implement IGraphEdge</typeparam>
    /// <typeparam name="TEdgeRoute">Edge route type, must implement IEdgeRoute</typeparam>
    public class D2dGraphNodeEditAdapter<TNode, TEdge, TEdgeRoute> : DraggingControlAdapter, IItemDragAdapter
        where TNode : class, IGraphNode
        where TEdge : class, IGraphEdge<TNode, TEdgeRoute>
        where TEdgeRoute : class, IEdgeRoute
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="renderer">Graph renderer</param>
        /// <param name="graphAdapter">Graph adapter</param>
        /// <param name="transformAdapter">Transform adapter</param>
        public D2dGraphNodeEditAdapter(
            D2dGraphRenderer<TNode, TEdge, TEdgeRoute> renderer,
            D2dGraphAdapter<TNode, TEdge, TEdgeRoute> graphAdapter,
            ITransformAdapter transformAdapter)
        {            
            m_renderer = renderer;
            m_graphAdapter = graphAdapter;
            m_transformAdapter = transformAdapter;
        }

        /// <summary>
        /// Gets or sets whether dragging subnodes</summary>
        public bool DraggingSubNodes
        {
            get { return m_draggingSubNodes;  }
            set { m_draggingSubNodes = value; }
        }
             
        /// <summary>
        /// Position node dragged to</summary>
        /// <param name="node">Dragged node</param>
        /// <returns>Point node dragged to or null if not dragged</returns>
        public Point? NodeDraggingPosition(TNode node)
        {
            if (m_draggingNodes != null)
            {
                for (int i = 0; i < m_draggingNodes.Length; ++i)
                {
                    var dragged = m_draggingNodes[i];
                    if (dragged == node)
                    {
                        return m_newPositions[i];
                    }
                }
            }
            return null;
        }
             
        /// <summary>
        /// Binds the adapter to the adaptable control; called in the order that the adapters
        /// were defined on the control.</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Bind(AdaptableControl control)
        {
            m_autoTranslateAdapter = control.As<IAutoTranslateAdapter>();            
            m_selectionPathProvider = control.As<ISelectionPathProvider>();  
            control.ContextChanged += control_ContextChanged;
            control.MouseMove += control_MouseMove;
            control.MouseUp += control_MouseUp;
            base.Bind(control);
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            base.Unbind(control);
            m_autoTranslateAdapter = null;
            m_selectionPathProvider = null;
            control.ContextChanged -= control_ContextChanged;
            control.MouseMove -= control_MouseMove;
            control.MouseUp -= control_MouseUp;
            m_graph = null;
            m_layoutContext = null;
            m_selectionContext = null;
        }

        private void control_ContextChanged(object sender, EventArgs e)
        {
            m_graph = AdaptedControl.ContextAs<IGraph<TNode, TEdge, TEdgeRoute>>();
            m_layoutContext = AdaptedControl.ContextAs<ILayoutContext>();
            m_editableGraphContainer = AdaptedControl.ContextAs<IEditableGraphContainer<TNode, TEdge, TEdgeRoute>>();
            if (m_layoutContext != null)
            {
                m_selectionContext = AdaptedControl.ContextCast<ISelectionContext>();
            }
        }

        private void control_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.None && AdaptedControl.Focused)
            {
                var hitRecord = m_graphAdapter.Pick(e.Location);
            
                if (hitRecord.Part.Is<DiagramBorder>())
                {
                    var borderPart = hitRecord.Part.Cast<DiagramBorder>();
                    if (m_editableGraphContainer != null &&
                        m_editableGraphContainer.CanResize(hitRecord.Item, borderPart))
                    {
                        AdaptedControl.AutoResetCursor = false;
                        if (borderPart.Border == DiagramBorder.BorderType.Right)
                            AdaptedControl.Cursor = Cursors.SizeWE;
                        else if (borderPart.Border == DiagramBorder.BorderType.Bottom)
                            AdaptedControl.Cursor = Cursors.SizeNS;
                     }                      
                }
                else if (hitRecord.SubPart.Is<DiagramBorder>())
                {
                    var borderPart = hitRecord.SubPart.Cast<DiagramBorder>();
                    if (m_editableGraphContainer != null &&
                        m_editableGraphContainer.CanResize(hitRecord.SubItem, borderPart))
                    {
                        AdaptedControl.AutoResetCursor = false;
                        if (borderPart.Border == DiagramBorder.BorderType.Right)
                            AdaptedControl.Cursor = Cursors.SizeWE;
                        else if (borderPart.Border == DiagramBorder.BorderType.Bottom)
                            AdaptedControl.Cursor = Cursors.SizeNS;
                    }
                }
                else
                    AdaptedControl.AutoResetCursor = true;
            }
        }

        private void control_MouseUp(object sender, MouseEventArgs e)
        {
            m_constrainXDetermined = false;
        }

        private bool AllowDragging(TNode node)
        {
            // do not permit in-line dragging template sub-nodes(open group editor for moving nodes)
            return !m_selectionPathProvider.Ancestry(node).Any( x=>x.Is<IReference<Group>>());
        }

        /// <summary>
        /// Determine whether node-dragging or border-dragging(only for expanded group) will happen</summary>
        /// <returns>True when click over:  1) a node that is not a group node; 2) or group node that is not expanded;
        /// 3) a sub-node in a expanded group node</returns>
        private bool CanDragging()
        {
            if (m_mousePick.Node != null || m_mousePick.Item != null)
            {
                if (m_mousePick.Node.Is<ICircuitGroupType<TNode, TEdge, TEdgeRoute>>())
                {
                    var group = m_mousePick.Node.Cast<ICircuitGroupType<TNode, TEdge, TEdgeRoute>>();
                    if (group.Expanded)
                    {
                        if (m_mousePick.Part.Is<DiagramBorder>() || m_mousePick.Part.Is<DiagramTitleBar>())
                            return true;
                        if (m_mousePick.SubItem == null)
                            return false;
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Raises the BeginDrag event and performs custom processing</summary>
        /// <param name="e">A MouseEventArgs that contains the event data</param>
        protected override void OnBeginDrag(MouseEventArgs e)
        {
            base.OnBeginDrag(e);
            if (m_layoutContext != null && e.Button == MouseButtons.Left &&
                   ((Control.ModifierKeys & Keys.Alt) == 0) &&
                   !AdaptedControl.Capture)
            {
                m_firstPoint = GdiUtil.InverseTransform(m_transformAdapter.Transform, FirstPoint);
                m_mousePick = m_graphAdapter.Pick(FirstPoint);
                if (CanDragging())
                {                    
                    foreach (var itemDragAdapter in AdaptedControl.AsAll<IItemDragAdapter>())
                        if (itemDragAdapter != this)
                            itemDragAdapter.BeginDrag(this);
               

                    // drag all selected nodes, and any edges impinging on them
                    ActiveCollection<TNode> draggingNodes = new ActiveCollection<TNode>();
                    HashSet<TNode> nodes = new HashSet<TNode>();

                    foreach (var node in m_selectionContext.GetSelection<TNode>())
                    {
                         if (AllowDragging(node))                 
                            AddDragNode(node, draggingNodes, nodes);
                    }

                    m_draggingNodes = draggingNodes.GetSnapshot<TNode>();

                    if ((m_draggingNodes.Length == 1) && m_draggingNodes[0] == m_mousePick.Item)
                    {
                        if (m_mousePick.SubItem != null)
                        {
                            if (m_mousePick.SubItem.Is<TNode>()) // favor dragging sub item
                                m_draggingNodes[0] = m_mousePick.SubItem.Cast<TNode>();

                            if (m_mousePick.SubPart.Is<DiagramBorder>()) // sub-container resizing?
                            {
                                var borderPart = m_mousePick.SubPart.Cast<DiagramBorder>();
                                if (m_editableGraphContainer != null && m_editableGraphContainer.CanResize(m_mousePick.SubItem, borderPart))
                                {
                                    m_firstBound = m_graphAdapter.GetLocalBound(m_mousePick.SubItem.As<TNode>());
                                    m_resizing = true;
                                    m_targetItem = m_mousePick.SubItem;
                                }
                            }
                        }
                        else if (m_mousePick.Part.Is<DiagramBorder>()) // then favor container resizing
                        {
                            var borderPart = m_mousePick.Part.Cast<DiagramBorder>();
                            if (m_editableGraphContainer != null && m_editableGraphContainer.CanResize(m_mousePick.Item, borderPart))
                            {
                                m_firstBound = m_graphAdapter.GetLocalBound(m_mousePick.Item.As<TNode>());
                                m_resizing = true;
                                m_targetItem = m_mousePick.Item;
                            }
                        }
                       
                    }

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

                    AdaptedControl.Capture = true;
                    if (m_autoTranslateAdapter != null)
                        m_autoTranslateAdapter.Enabled = true;
                }
            }         
   
        }

        private void ResizingNode()
        {
            Point currentPoint = GdiUtil.InverseTransform(m_transformAdapter.Transform, CurrentPoint);
            Point delta = new Point(currentPoint.X - m_firstPoint.X, currentPoint.Y - m_firstPoint.Y);
            m_editableGraphContainer.Resize(m_targetItem, (int)m_firstBound.Width + delta.X, (int)m_firstBound.Height + delta.Y);
        }

        /// <summary>
        /// Performs custom actions when performing a mouse dragging operation</summary>
        /// <param name="e">Mouse event args</param>
        protected override void OnDragging(MouseEventArgs e)
        {
            if (m_resizing)
            {
                ResizingNode();
                return;            
            }
             
            m_movingNodesCrossContainer = false;
            if (m_draggingNodes != null)
            {
                foreach (var draggingNode in m_draggingNodes)
                    m_renderer.SetCustomStyle(draggingNode, DiagramDrawingStyle.DragSource);

                // look for drop target
                if (m_editableGraphContainer != null)
                {
                    var nodes = m_graph.Nodes.Except(m_draggingNodes);
                    var hitRecord = m_graphAdapter.Pick(nodes, EmptyEnumerable<TEdge>.Instance, e.Location);
                    if (hitRecord.Item != null)
                    {
                        var newItemTarget = ChooseActiveTarget(hitRecord);
                   
                        if (m_targetItem != newItemTarget)
                        {
                            ResetCustomStyle(m_targetItem);
                            m_targetItem = newItemTarget;
                        }
                    }
                    else
                    {
                        ResetCustomStyle(m_targetItem);
                        m_targetItem = null;
                    }
                }

                var d2dControl = this.AdaptedControl as D2dAdaptableControl;
                Point currentPoint = GdiUtil.InverseTransform(m_transformAdapter.Transform, CurrentPoint);
                Point delta = new Point(currentPoint.X - m_firstPoint.X, currentPoint.Y - m_firstPoint.Y);
                                
                // Constrain the movement to be parallel to the x-axis or y-axis?
                if (Control.ModifierKeys == Keys.Shift)
                {
                    if (!m_constrainXDetermined)
                    {
                        int dx = Math.Abs(delta.X);
                        int dy = Math.Abs(delta.Y);
                        Size dragSize = SystemInformation.DragSize;
                        if (dx > dragSize.Width ||
                            dy > dragSize.Height)
                        {
                            m_constrainX = (dx < dy);
                            m_constrainXDetermined = true;
                        }
                    }
                    if (m_constrainXDetermined)
                    {
                        if (m_constrainX)
                            delta.X = 0;
                        else
                            delta.Y = 0;
                    }
                }
                else
                {
                    m_constrainXDetermined = false;
                }

                // set dragged nodes' positions, offsetting by drag delta and applying layout constraints
                for (int i = 0; i < m_draggingNodes.Length; i++)
                {                    
                    TNode node = m_draggingNodes[i];
                    Rectangle bounds;
                    m_layoutContext.GetBounds(node, out bounds);
                    bounds.X = m_oldPositions[i].X + delta.X;
                    bounds.Y = m_oldPositions[i].Y + delta.Y;
                    m_newPositions[i] = bounds.Location;
                    m_layoutContext.SetBounds(node, bounds, BoundsSpecified.Location);
                }

                if (m_editableGraphContainer != null)
                {
                    if (m_editableGraphContainer.CanMove(m_targetItem, m_draggingNodes))
                    {
                        m_renderer.SetCustomStyle(m_targetItem, DiagramDrawingStyle.DropTarget);
                        m_movingNodesCrossContainer = true;
                    }
                }
                // directly drawing is faster than invalidating and waiting for OnPaint.
                d2dControl.DrawD2d();
            }
        }

        /// <summary>
        /// Performs custom actions when ending a mouse dragging operation</summary>
        /// <param name="e">Mouse event args</param>
        protected override void OnEndDrag(MouseEventArgs e)
        {
            base.OnEndDrag(e);
            if (m_draggingNodes != null && m_draggingNodes.Length > 0)
            {
                foreach (var draggingNode in m_draggingNodes)
                    ResetCustomStyle(draggingNode);

                foreach (IItemDragAdapter itemDragAdapter in AdaptedControl.AsAll<IItemDragAdapter>())
                    itemDragAdapter.EndingDrag(); //call ourselves, too

                    var transactionContext = AdaptedControl.ContextAs<ITransactionContext>();
                    transactionContext.DoTransaction(delegate
                        {
                            foreach (IItemDragAdapter itemDragAdapter in AdaptedControl.AsAll<IItemDragAdapter>())
                                itemDragAdapter.EndDrag(); //call ourselves, too
                        }, "Drag Items".Localize());

                if (m_autoTranslateAdapter != null)
                    m_autoTranslateAdapter.Enabled = false;
                AdaptedControl.Invalidate();
            }
            m_draggingNodes = null;
            m_newPositions = null;
            m_oldPositions = null;
            ResetCustomStyle(m_targetItem);
            m_targetItem = null;
            m_resizing = false;
            AdaptedControl.Cursor = Cursors.Default;
        }

        #region IItemDragAdapter Members

        void IItemDragAdapter.BeginDrag(ControlAdapter initiator)
        {
            // drag all selected nodes, and any edges impinging on them
            ActiveCollection<TNode> draggingNodes = new ActiveCollection<TNode>();
            HashSet<TNode> nodes = new HashSet<TNode>();

            foreach (TNode node in m_selectionContext.GetSelection<TNode>())
            {
                AddDragNode(node, draggingNodes, nodes);
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
        }

        void IItemDragAdapter.EndingDrag()
        {
            if (m_resizing)
            {
                m_editableGraphContainer.Resize(m_targetItem, (int) m_firstBound.Width, (int) m_firstBound.Height);
                return;
            }
            else if (m_draggingNodes != null) // restore dragged nodes' positions, before the transaction begins.
            {
                for (int k = 0; k < m_draggingNodes.Length; k++)
                {
                    TNode node = m_draggingNodes[k];
                    MoveNode(node, m_oldPositions[k]);
                }
            }
        }

        void IItemDragAdapter.EndDrag()
        {
            if (m_draggingNodes == null)
                return;

            // OnDragging will update the positions and OnEndDrag() will create the transaction.
            // move the nodes to the new position
            int i = 0;
            foreach (TNode node in m_draggingNodes)
            {
                MoveNode(node, m_newPositions[i]);
                i++;
            }

            if (m_movingNodesCrossContainer)
            {
                m_editableGraphContainer.Move(m_targetItem, m_draggingNodes);
                m_movingNodesCrossContainer = false;
            }
            else if (m_resizing)
            {
                Point currentPoint = GdiUtil.InverseTransform(m_transformAdapter.Transform, CurrentPoint);
                Point delta = new Point(currentPoint.X - m_firstPoint.X, currentPoint.Y - m_firstPoint.Y);
                m_editableGraphContainer.Resize(m_targetItem, (int)m_firstBound.Width + delta.X, (int)m_firstBound.Height + delta.Y);
            }
            else if (m_selectionPathProvider != null)
            {
                Point currentPoint = GdiUtil.InverseTransform(m_transformAdapter.Transform, CurrentPoint);
                Point delta = new Point(currentPoint.X - m_firstPoint.X, currentPoint.Y - m_firstPoint.Y);
                if (delta.X < 0 || delta.Y < 0)  // negative offset, check whether we need to relocate group container
                {
                    foreach (var node in m_draggingNodes)
                    {
                        var selectionPath = m_selectionPathProvider.GetSelectionPath(node);
                        if (selectionPath == null)
                            continue;
                        int length = selectionPath.Count();
                        if (length > 1)
                        {
                            
                            var parent = selectionPath[length - 2];
                            if (parent.Is<ICircuitGroupType<TNode, TEdge, TEdgeRoute>>())
                            {
                                var group = parent.Cast<ICircuitGroupType<TNode, TEdge, TEdgeRoute>>();
                                if (node.Bounds.Location.X < -group.Info.Offset.X ||
                                    node.Bounds.Location.Y < -group.Info.Offset.Y)
                                {
                                    Rectangle bounds;
                                    m_layoutContext.GetBounds(group, out bounds);
                                    if (delta.X < 0)
                                        bounds.X += delta.X;
                                    if (delta.Y < 0)
                                        bounds.Y += delta.Y;
                                    m_layoutContext.SetBounds(group, bounds, BoundsSpecified.Location);
                                }
                            }
                        }
                    }
                }
            }
            m_draggingNodes = null;
        }

        #endregion

        private void AddDragNode(TNode node, ActiveCollection<TNode> draggingNodes, HashSet<TNode> nodes)
        {
            draggingNodes.Add(node);
            if (!nodes.Contains(node))
            {
                nodes.Add(node);                
            }

            if (DraggingSubNodes)
            {
                var hierarchicalNode = node.As<IHierarchicalGraphNode<TNode, TEdge, TEdgeRoute>>();
                if (hierarchicalNode != null)
                {
                    foreach (TNode subNode in hierarchicalNode.SubNodes)
                    {
                        AddDragNode(subNode, draggingNodes, nodes);
                    }
                }
            }
        }
       
        private void MoveNode(TNode node, Point location)
        {
            Rectangle bounds;
            m_layoutContext.GetBounds(node, out bounds);
            bounds.Location = location;
            m_layoutContext.SetBounds(node, bounds, BoundsSpecified.Location);
        }


        private object ChooseActiveTarget(GraphHitRecord<TNode, TEdge, TEdgeRoute> hitRecord)
        {
            if (hitRecord.SubItem == null)
                return hitRecord.Item;
            
            if (m_editableGraphContainer != null)
            {
                foreach (var itemInPath in hitRecord.HitPathInversed)
                {
                    if (m_editableGraphContainer.CanMove(itemInPath, m_draggingNodes))
                        // favor subitem when it is expanded
                        return itemInPath;
                }
            }
            return hitRecord.Item;
        }
        
        // custom style is mainly used in expanded group node for in-place editing selection highlight
        private void ResetCustomStyle(object item)
        {
            m_renderer.ClearCustomStyle(item);
        }
        
        private readonly D2dGraphRenderer<TNode, TEdge, TEdgeRoute> m_renderer;
        private readonly D2dGraphAdapter<TNode, TEdge, TEdgeRoute> m_graphAdapter;
        private readonly ITransformAdapter m_transformAdapter;
        private IAutoTranslateAdapter m_autoTranslateAdapter;
        private ISelectionPathProvider m_selectionPathProvider;

        private IGraph<TNode, TEdge, TEdgeRoute> m_graph;
        private IEditableGraphContainer<TNode, TEdge, TEdgeRoute> m_editableGraphContainer;
        private ILayoutContext m_layoutContext;
        private ISelectionContext m_selectionContext;

        private GraphHitRecord<TNode, TEdge, TEdgeRoute> m_mousePick = new GraphHitRecord<TNode, TEdge, TEdgeRoute>();
        private TNode[] m_draggingNodes;
        private Point[] m_newPositions;
        private Point[] m_oldPositions;        
        private Point m_firstPoint;
        private bool m_draggingSubNodes = true;

        private object m_targetItem;
        private bool m_movingNodesCrossContainer;
        private bool m_resizing;
        private RectangleF m_firstBound;
        private bool m_constrainXDetermined;
        private bool m_constrainX;
    }
}
