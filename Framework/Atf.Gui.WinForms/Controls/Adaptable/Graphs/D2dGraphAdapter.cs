//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.VectorMath;
using Sce.Atf.Direct2D;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Control adapter to reference and render a graph diagram. Also provides hit testing
    /// with the Pick method, and viewing support with the Frame and EnsureVisible methods.</summary>
    /// <typeparam name="TNode">Node type, must implement IGraphNode</typeparam>
    /// <typeparam name="TEdge">Edge type, must implement IGraphEdge</typeparam>
    /// <typeparam name="TEdgeRoute">Edge route type, must implement IEdgeRoute</typeparam>
    /// <remark>ControlAdapter accesses AdaptableControl, which is a Form's control, hence any class derived from ControlAdapter
    /// belongs to Atf.Gui.WinForms project</remark>    
    public class D2dGraphAdapter<TNode, TEdge, TEdgeRoute> : ControlAdapter, IPickingAdapter2, IDisposable        
        where TNode : class, IGraphNode
        where TEdge : class, IGraphEdge<TNode, TEdgeRoute>
        where TEdgeRoute : class, IEdgeRoute
    {
        /// <summary>
        /// Constructor</summary>        
        /// <param name="renderer">Graph renderer to draw and hit-test graph</param>
        /// <param name="transformAdapter">Transform adapter</param>
        public D2dGraphAdapter(D2dGraphRenderer<TNode, TEdge, TEdgeRoute> renderer,
            ITransformAdapter transformAdapter)
        {            
            m_renderer = renderer;
            m_renderer.Redraw += renderer_Redraw;
            EdgeRenderPolicy = DrawEdgePolicy.AllFirst;
        }

        /// <summary>
        /// Enumeration for draw edge policy that determines when edges are drawn relative to nodes</summary>
        public enum DrawEdgePolicy
        {
            /// <summary>
            /// Draw an edge after its start and end nodes are drawn</summary>
            Associated,
            /// <summary>
            /// Draw all edges first, followed by nodes</summary>            
            AllFirst
        }

        /// <summary>
        /// Gets or sets draw edge policy that determines when edges are drawn relative to nodes</summary>
        public DrawEdgePolicy EdgeRenderPolicy { get; set; }

        #region IDisposable Members

        /// <summary>
        /// Releases non-memory resources</summary>
        public void Dispose()
        {
             m_renderer.Redraw -= renderer_Redraw;
             Dispose(true);
        }

        /// <summary>
        /// Sets dispose flag</summary>
        /// <param name="disposing">Value to set dispose flag to</param>
        protected virtual void Dispose(bool disposing)
        {         
        }
        #endregion

        /// <summary>
        /// Gets the renderer for the adapter</summary>
        public D2dGraphRenderer<TNode, TEdge, TEdgeRoute> Renderer
        {
            get { return m_renderer; }
        }
                                
        /// <summary>
        /// Gets the current rendering style for an item</summary>
        /// <param name="item">Rendered item</param>
        /// <returns>Rendering style set by SetStyle, Normal if no override is set</returns>
        public virtual DiagramDrawingStyle GetStyle(object item)
        {
            // Give the renderer an opportunity to override our style selection.
            DiagramDrawingStyle result = m_renderer.GetCustomStyle(item);
            if (result != DiagramDrawingStyle.None)
                return result;

            result = DiagramDrawingStyle.Normal;
            // no override
            if (m_visibilityContext != null && !m_visibilityContext.IsVisible(item))
            {
                result = DiagramDrawingStyle.Hidden;
            }
            else if (item == m_hoverObject || item == m_hoverSubObject)
            {
                if (CircuitUtil.IsGroupTemplateInstance(item))
                    result = DiagramDrawingStyle.TemplatedInstance;
                else if (item.Is<Group>())
                    result = DiagramDrawingStyle.CopyInstance;
                else
                    result = DiagramDrawingStyle.Hot;
            }           
            else if (m_selectionContext != null && m_selectionContext.SelectionContains(item))
            {
                if (m_selectionContext.LastSelected.Equals(item))
                    result = DiagramDrawingStyle.LastSelected;
                else
                    result = DiagramDrawingStyle.Selected;
            }
            else if (m_selectionPathProvider != null && m_selectionPathProvider.IncludedPath(item) != null)
            {
                if (CircuitUtil.IsGroupTemplateInstance(item))
                    result = DiagramDrawingStyle.TemplatedInstance;
                else if (item.Is<Group>())
                    result = DiagramDrawingStyle.CopyInstance;
                else
                    result = DiagramDrawingStyle.Hot;
            }
            else if (m_renderer.RouteConnecting != null)
            {
                // connection context cue: highlight edges that connect to the starting node
                if (item.Is<TEdge>())
                {
                    var edge = item.Cast<TEdge>();
                    if (m_renderer.RouteConnecting.StartNode.Equals(edge.FromNode) ||
                        m_renderer.RouteConnecting.StartNode.Equals(edge.ToNode))
                        result = DiagramDrawingStyle.Hot;
                }
            }
            return result;
        }

        /// <summary>
        /// Hides the given edge.
        /// This method is used for hiding an edge while dragging its replacement.</summary>
        /// <param name="edge">Edge to be hidden or null</param>
        public void HideEdge(TEdge edge)
        {
            m_hiddenEdge = edge;
        }

        /// <summary>
        /// Performs a picking operation on the graph with the point</summary>
        /// <param name="p">Hit test point in screen space</param>
        /// <returns>Hit record resulting from picking operation</returns>
        public GraphHitRecord<TNode, TEdge, TEdgeRoute> Pick(Point p)
        {
            IPickingAdapter2 pickAdapter = (IPickingAdapter2)this;
            return (GraphHitRecord<TNode, TEdge, TEdgeRoute>)pickAdapter.Pick(p);
        }

        #region IPickingAdapters Members

        /// <summary>
        /// Performs hit test for a point, in client coordinates</summary>
        /// <param name="pickPoint">Pick point, in client coordinates</param>
        /// <returns>Hit record for a point, in client coordinates</returns>
        DiagramHitRecord IPickingAdapter2.Pick(Point pickPoint)
        {
            m_renderer.GetStyle = GetStyle;
            if (AdaptedControl.Context != null)
            {
                Matrix3x2F invXform = Matrix3x2F.Invert(m_d2dGraphics.Transform);
                PointF pt = Matrix3x2F.TransformPoint(invXform, pickPoint);

                TEdge priorityEdge = null;
                if (m_selectionContext != null)
                    priorityEdge = m_selectionContext.GetLastSelected<TEdge>();
                return m_renderer.Pick(m_graph, priorityEdge, pt, m_d2dGraphics);
            }
            return new GraphHitRecord<TNode, TEdge, TEdgeRoute>();
        }

        /// <summary>
        /// Performs hit test for a point, in client coordinates</summary>
        /// <param name="nodes">Nodes to test if hit</param>
        /// <param name="edges">Edges to test if hit</param>
        /// <param name="pickPoint">Pick point, in client coordinates</param>
        /// <returns>Hit record for a point, in client coordinates</returns>
        public GraphHitRecord<TNode, TEdge, TEdgeRoute> Pick(IEnumerable<TNode> nodes, IEnumerable<TEdge> edges, Point pickPoint)
        {
            m_renderer.GetStyle = GetStyle;

            if (AdaptedControl.Context != null)
            {
                Matrix3x2F invXform = Matrix3x2F.Invert(m_d2dGraphics.Transform);
                PointF pt = Matrix3x2F.TransformPoint(invXform, pickPoint);

                TEdge priorityEdge = null;
                if (m_selectionContext != null)
                    priorityEdge = m_selectionContext.GetLastSelected<TEdge>();
                return m_renderer.Pick(nodes, edges, priorityEdge, pt, m_d2dGraphics);
            }
            return new GraphHitRecord<TNode, TEdge, TEdgeRoute>();
        }

        /// <summary>
        /// Performs hit testing for rectangle bounds, in client coordinates</summary>
        /// <param name="pickRect">Pick rectangle, in client coordinates</param>
        /// <returns>Items that overlap with the rectangle, in client coordinates</returns>
        public virtual IEnumerable<object> Pick(Rectangle pickRect)
        {
            m_renderer.GetStyle = GetStyle;
            Matrix3x2F invXform = Matrix3x2F.Invert(m_d2dGraphics.Transform);
            RectangleF rect = D2dUtil.Transform(invXform,pickRect);
            return m_renderer.Pick(m_graph, rect, m_d2dGraphics);           
        }

        /// <summary>
        /// Gets a bounding rectangle for the items, in client coordinates</summary>
        /// <param name="items">Items to bound</param>
        /// <returns>Bounding rectangle for the items, in client coordinates</returns>
        public virtual Rectangle GetBounds(IEnumerable<object> items)
        {
            RectangleF bounds = m_renderer.GetBounds(items.AsIEnumerable<TNode>(), m_d2dGraphics);
            if (bounds.IsEmpty) return Rectangle.Empty;
            bounds = D2dUtil.Transform(m_d2dGraphics.Transform, bounds); 
            return Rectangle.Truncate(bounds);
        }

        /// <summary>
        /// Gets a bounding rectangle for the item in graph/world space</summary>
        /// <param name="elem">Item to bound</param>
        /// <returns>Bounding rectangle for the items, in world coordinates</returns>
        public virtual RectangleF GetLocalBound(TNode elem)
        {
            RectangleF bounds = m_renderer.GetBounds(elem, m_d2dGraphics);
            return bounds;
        }

        #endregion

        #region IPrintingAdapter Members

        //void IPrintingAdapter.Print(PrintDocument printDocument, Graphics g)
        //{
        //    throw new NotImplementedException();
        //    //switch (printDocument.PrinterSettings.PrintRange)
        //    //{
        //    //    case PrintRange.Selection:
        //    //        PrintSelection(g);
        //    //        break;

        //    //    default:
        //    //        PrintAll(g);
        //    //        break;
        //    //}
        //}

        //private void PrintSelection(Graphics g)
        //{
        //    if (m_selectionContext != null)
        //    {
        //        HashSet<TNode> selectedNodes = new HashSet<TNode>();
        //        foreach (TNode node in m_graph.Nodes)
        //        {
        //            m_renderer.Draw(node, DiagramDrawingStyle.Normal, m_d2dGraphics);
        //            selectedNodes.Add(node);
        //        }
        //        foreach (TEdge edge in m_graph.Edges)
        //        {
        //            if (selectedNodes.Contains(edge.FromNode) &&
        //                selectedNodes.Contains(edge.ToNode))
        //            {
        //                m_renderer.Draw(edge, DiagramDrawingStyle.Normal, m_d2dGraphics);
        //            }
        //        }
        //    }
        //}

        //private void PrintAll(Graphics g)
        //{
        //    foreach (TNode node in m_graph.Nodes)
        //        m_renderer.Draw(node, DiagramDrawingStyle.Normal, m_d2dGraphics);
        //    foreach (TEdge edge in m_graph.Edges)
        //        m_renderer.Draw(edge, DiagramDrawingStyle.Normal, m_d2dGraphics);
        //}

        #endregion

        /// <summary>
        /// Gets the D2D adaptable control that we are currently bound to, or null</summary>
        protected D2dAdaptableControl D2DAdaptableControl
        {
            get { return m_d2dControl; }
        }

        /// <summary>
        /// Gets the current D2dGraphics object of the D2dAdaptableControl that we are bound to, or null</summary>
        protected D2dGraphics D2dGraphics
        {
            get { return m_d2dGraphics; }
        }

        /// <summary>
        /// Gets the current IGraph that we are adapted to</summary>
        protected IGraph<TNode, TEdge, TEdgeRoute> Graph
        {
            get { return m_graph; }
        }

        /// <summary>
        /// Gets the current hidden edge, or null</summary>
        protected TEdge HiddenEdge
        {
            get { return m_hiddenEdge; }
        }

        /// <summary>
        /// Binds the adapter to the adaptable control; called in the order that the adapters
        /// were defined on the control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Bind(AdaptableControl control)
        {
            if (m_d2dControl != null)
                throw new InvalidOperationException("We can only bind to one D2dAdaptableControl at a time");

            m_draggingAdapters = control.AsAll<DraggingControlAdapter>().ToArray();

            m_d2dControl = (D2dAdaptableControl)control;
            m_d2dGraphics = m_d2dControl.D2dGraphics;
            m_d2dControl.ContextChanged += control_ContextChanged;
            m_d2dControl.DrawingD2d += control_Paint;
            m_d2dControl.MouseDown += d2dControl_MouseDown;
            m_d2dControl.MouseMove += d2dControl_MouseMove;
            m_d2dControl.MouseLeave += d2dControl_MouseLeave;
            m_selectionPathProvider = control.As<ISelectionPathProvider>();  
        }

        private DraggingControlAdapter[] m_draggingAdapters;
        
        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            if (m_d2dControl != control)
                throw new InvalidOperationException("We can only unbind from a D2dAdaptableControl that we previously were bound to");


            m_draggingAdapters = null;

            m_d2dControl.ContextChanged -= control_ContextChanged;
            m_d2dControl.DrawingD2d -= control_Paint;
            m_d2dControl.MouseDown -= d2dControl_MouseDown;
            m_d2dControl.MouseMove -= d2dControl_MouseMove;
            m_d2dControl.MouseLeave -= d2dControl_MouseLeave;
            m_d2dGraphics = null;
            m_d2dControl = null;
            m_selectionPathProvider = null;
        }


        private Multimap<TNode, TEdge> m_nodeEdges = new Multimap<TNode, TEdge>();
        private Dictionary<TEdge, int> m_edgeNodeEncounter = new Dictionary<TEdge, int>();
        private List<TNode> m_draggingNodes = new List<TNode>();
        private List<TNode> m_selectedNodes = new List<TNode>();
        private List<TNode> m_expandedGroups = new List<TNode>();
        private List<TEdge> m_edgesOnGroups = new List<TEdge>();

        /// <summary>
        /// Renders entire graph</summary>
        protected virtual void OnRender()
        {
            try
            {
                m_renderer.GetStyle = GetStyle;
                m_d2dGraphics.AntialiasMode = D2dAntialiasMode.PerPrimitive;
                Matrix3x2F invMtrx = m_d2dGraphics.Transform;
                invMtrx.Invert();
                RectangleF boundsGr = Matrix3x2F.Transform(invMtrx, this.AdaptedControl.ClientRectangle);

                // Either draw (most) edges first or prepare multimaps for draw-as-we-go edges.                
                if (EdgeRenderPolicy == DrawEdgePolicy.AllFirst)
                {
                    foreach (var edge in m_graph.Edges)
                    {
                        if (edge == m_hiddenEdge) continue;
                        RectangleF bounds = m_renderer.GetBounds(edge, m_d2dGraphics);
                        if (!boundsGr.IntersectsWith(bounds)) continue;

                        var group = edge.FromNode.As<ICircuitGroupType<TNode, TEdge, TEdgeRoute>>();
                        if (group == null || !group.Expanded)
                            group = edge.ToNode.As<ICircuitGroupType<TNode, TEdge, TEdgeRoute>>();
                        if (group != null && group.Expanded)
                            m_edgesOnGroups.Add(edge);
                        else
                        {
                            DiagramDrawingStyle style = GetStyle(edge);
                            m_renderer.Draw(edge, style, m_d2dGraphics);
                        }
                    }
                }
                else
                {
                    // build node to edge maps                    
                    foreach (TEdge edge in m_graph.Edges)
                    {
                        m_nodeEdges.Add(edge.FromNode, edge);
                        m_nodeEdges.Add(edge.ToNode, edge);
                        m_edgeNodeEncounter.Add(edge, 0);
                    }
                }

                // Draw normal nodes first
                TNode containerOfSelectedNode = null;
                
                foreach (var node in m_graph.Nodes)
                {
                    RectangleF nodeBounds = m_renderer.GetBounds(node, m_d2dGraphics);
                    if (boundsGr.IntersectsWith(nodeBounds))
                    {
                        DiagramDrawingStyle drawStyle = GetStyle(node);

                        // Draw all dragged nodes (even expanded groups) last.
                        if (drawStyle == DiagramDrawingStyle.DragSource)
                        {
                            m_draggingNodes.Add(node);
                        }
                        else
                        {
                            // Draw selected nodes after normal nodes. If the node
                            //  is hot, check if it's selected.
                            if (drawStyle == DiagramDrawingStyle.Selected ||
                                drawStyle == DiagramDrawingStyle.LastSelected ||
                                (drawStyle == DiagramDrawingStyle.Hot && m_selectionContext != null &&
                                    m_selectionContext.SelectionContains(node)))
                            {
                                m_selectedNodes.Add(node);
                            }
                            else
                            {
                                // Expanded groups are drawn after normal nodes.
                                bool expandedGroup = false;
                                var group = node.As<ICircuitGroupType<TNode, TEdge, TEdgeRoute>>();
                                if (group != null)
                                {
                                    group.Info.PickingPriority = 0;
                                    if (group.Expanded)
                                    {
                                        if (node == ActiveContainer())
                                            containerOfSelectedNode = node;
                                        else
                                            m_expandedGroups.Add(node);
                                        expandedGroup = true;
                                    }
                                }

                                // Draw normal nodes and collapsed groups.
                                if (!expandedGroup)
                                {
                                    m_renderer.Draw(node, drawStyle, m_d2dGraphics);
                                    if (EdgeRenderPolicy == DrawEdgePolicy.Associated)
                                        DrawAssociatedEdges(node, boundsGr);
                                }
                            }
                        }
                    }
                    else if (EdgeRenderPolicy == DrawEdgePolicy.Associated)
                        DrawAssociatedEdges(node, boundsGr);
                }

                // Draw expanded groups on top of normal sibling nodes, so that normal nodes that overlap
                //  these groups don't appear as if they are in the groups.
                int pickPriority = 0;
                foreach (var node in m_expandedGroups)
                {
                    var group = node.Cast<ICircuitGroupType<TNode, TEdge, TEdgeRoute>>();
                    group.Info.PickingPriority = pickPriority++;
                    m_renderer.Draw(node, GetStyle(node), m_d2dGraphics);
                    if (EdgeRenderPolicy == DrawEdgePolicy.Associated)
                        DrawAssociatedEdges(node, boundsGr);
                }

                // Draw the expanded group that contains a selected or dragged child node, so that
                //  if multiple expanded groups overlap, that the user can see the owning group.
                if (containerOfSelectedNode != null)
                {
                    var group = containerOfSelectedNode.Cast<ICircuitGroupType<TNode, TEdge, TEdgeRoute>>();
                    group.Info.PickingPriority = pickPriority++;
                    m_renderer.Draw(containerOfSelectedNode, GetStyle(containerOfSelectedNode), m_d2dGraphics);
                    if (EdgeRenderPolicy == DrawEdgePolicy.Associated)
                        DrawAssociatedEdges(containerOfSelectedNode, boundsGr);
                }

                // Draw selected nodes.
                foreach (var node in m_selectedNodes)
                {
                    var group = node.As<ICircuitGroupType<TNode, TEdge, TEdgeRoute>>();
                    if (group != null)
                        group.Info.PickingPriority = pickPriority++;
                    m_renderer.Draw(node, GetStyle(node), m_d2dGraphics);
                    if (EdgeRenderPolicy == DrawEdgePolicy.Associated)
                        DrawAssociatedEdges(node, boundsGr);
                }

                             
                // Draw dragging nodes last to ensure they are visible (necessary for container-crossing move operation)
                foreach (var node in m_draggingNodes)
                {
                    m_renderer.Draw(node, DiagramDrawingStyle.DragSource, m_d2dGraphics);
                    if (EdgeRenderPolicy == DrawEdgePolicy.Associated)
                        DrawAssociatedEdges(node, boundsGr);
                }

                // Draw "all first" edges that connect to expanded groups.
                foreach (var edge in m_edgesOnGroups)
                {
                    DiagramDrawingStyle style = GetStyle(edge);
                    m_renderer.Draw(edge, style, m_d2dGraphics);
                }                        
            }
            finally
            {
                m_nodeEdges.Clear();
                m_edgeNodeEncounter.Clear();
                m_draggingNodes.Clear();
                m_selectedNodes.Clear();
                m_expandedGroups.Clear();
                m_edgesOnGroups.Clear();
            }
        }

        private void DrawAssociatedEdges(TNode node, RectangleF clipBounds)
        {
            
            var edges = m_nodeEdges.Find(node);
            foreach (var edge in edges)
            {
                int edgeVisit = m_edgeNodeEncounter[edge] + 1;
                m_edgeNodeEncounter[edge] = edgeVisit;
                if (edgeVisit == 2)
                {
                    RectangleF bounds = m_renderer.GetBounds(edge, m_d2dGraphics);
                    DiagramDrawingStyle style = GetStyle(edge);
                    if (clipBounds.IntersectsWith(bounds) && style != DiagramDrawingStyle.Hidden)
                        m_renderer.Draw(edge, style, m_d2dGraphics);
                }
            }
        }

        private object ActiveContainer()
        {
            if (m_selectionContext != null )
            {
                return m_selectionPathProvider.Parent(m_selectionContext.LastSelected);
            }
            return null;
        }

        private void control_Paint(object sender, EventArgs e)
        {
            m_renderer.GetStyle = GetStyle;
            OnRender();
        }

        private void d2dControl_MouseLeave(object sender, EventArgs e)
        {
            m_hoverObject = null;
            ResetCustomStyle(m_hoverSubObject);
            m_hoverSubObject = null;

         }

        private void d2dControl_MouseDown(object sender, MouseEventArgs e)
        {
            m_hoverObject = null;
            ResetCustomStyle(m_hoverSubObject);
            m_hoverSubObject = null;

         }

        private void d2dControl_MouseMove(object sender, MouseEventArgs e)
        {
            if ( e.Button == MouseButtons.None && AdaptedControl.Focused)
            {
                IPickingAdapter2 pickAdapter = this as IPickingAdapter2;
                DiagramHitRecord hoverHitRecord= pickAdapter.Pick(e.Location);
                object item = hoverHitRecord.Item;

                bool redraw = false;
                if (m_hoverObject != item)
                {
                    m_hoverObject = item;
                    redraw = true;
                }
                if (m_hoverSubObject != hoverHitRecord.SubItem)
                {
                    ResetCustomStyle(m_hoverSubObject);
                    m_hoverSubObject = hoverHitRecord.SubItem;
                    m_renderer.SetCustomStyle(m_hoverSubObject, DiagramDrawingStyle.DragSource);
 
                    redraw = true;
                }
               
                if (redraw)
                    Invalidate();
            }
            
        }

        private void control_ContextChanged(object sender, EventArgs e)
        {
            IGraph<TNode, TEdge, TEdgeRoute> graph = AdaptedControl.ContextAs<IGraph<TNode, TEdge, TEdgeRoute>>();
            if (graph == null)
                graph = s_emptyGraph;

            if (m_graph != graph)
            {
                if (m_graph != null)
                {
                    if (m_observableContext != null)
                    {
                        m_observableContext.ItemInserted -= graph_ObjectInserted;
                        m_observableContext.ItemRemoved -= graph_ObjectRemoved;
                        m_observableContext.ItemChanged -= graph_ObjectChanged;
                        m_observableContext.Reloaded -= graph_Reloaded;
                        m_observableContext = null;
                    }
                    if (m_selectionContext != null)
                    {
                        m_selectionContext.SelectionChanged -= selection_Changed;
                        m_selectionContext = null;
                    }

                    m_visibilityContext = null;
                }

                m_graph = graph;

                if (m_graph != null)
                {
                    m_observableContext = AdaptedControl.ContextAs<IObservableContext>();
                    if (m_observableContext != null)
                    {
                        m_observableContext.ItemInserted += graph_ObjectInserted;
                        m_observableContext.ItemRemoved += graph_ObjectRemoved;
                        m_observableContext.ItemChanged += graph_ObjectChanged;
                        m_observableContext.Reloaded += graph_Reloaded;
                    }

                    m_selectionContext = AdaptedControl.ContextAs<ISelectionContext>();
                    if (m_selectionContext != null)
                    {
                        m_selectionContext.SelectionChanged += selection_Changed;
                    }

                    m_visibilityContext = AdaptedControl.ContextAs<IVisibilityContext>();
                }
            }
        }

        private void selection_Changed(object sender, EventArgs e)
        {
            
            Invalidate();
        }

        private void graph_ObjectInserted(object sender, ItemInsertedEventArgs<object> e)
        {
            m_renderer.OnGraphObjectInserted(sender, e);
            Invalidate();
        }

        private void graph_ObjectRemoved(object sender, ItemRemovedEventArgs<object> e)
        {
            m_renderer.OnGraphObjectRemoved(sender, e);
            Invalidate();
        }

        private void graph_ObjectChanged(object sender, ItemChangedEventArgs<object> e)
        {
            m_renderer.OnGraphObjectChanged(sender, e);
            Invalidate();
        }

        private void graph_Reloaded(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void renderer_Redraw(object sender, EventArgs e)
        {
            Invalidate();
        }

        private bool IsDragging()
        {
            if (m_draggingAdapters != null)
            {
                foreach (var adpater in m_draggingAdapters)
                    if (adpater.IsDragging) return true;
            }
            return false;
        }
        private void Invalidate()
        {
            if(!IsDragging())// no need to invalidate when dragging.
                AdaptedControl.Invalidate();            
        }

        /// <summary>
        /// Empty graph to simplify code when there is no graph</summary>
        protected class EmptyGraph : IGraph<TNode, TEdge, TEdgeRoute>
        {
            /// <summary>
            /// Gets node enumeration as EmptyEnumerable</summary>
            public IEnumerable<TNode> Nodes
            {
                get { return EmptyEnumerable<TNode>.Instance; }
            }

            /// <summary>
            /// Gets edges enumeration as EmptyEnumerable</summary>
            public IEnumerable<TEdge> Edges
            {
                get { return EmptyEnumerable<TEdge>.Instance; }
            }
        }

        // custom style is mainly used in expanded group node for in-place editing (dragging source/target highlight)
        private void ResetCustomStyle(object item)
        {
            m_renderer.ClearCustomStyle(item);
        }

        private D2dAdaptableControl m_d2dControl;
        private D2dGraphics m_d2dGraphics;
        private readonly D2dGraphRenderer<TNode, TEdge, TEdgeRoute> m_renderer;
        private object m_hoverObject;
        private object m_hoverSubObject;
        private ISelectionPathProvider m_selectionPathProvider;
      
        private TEdge m_hiddenEdge; // hide edge when dragging its replacement.
        private IGraph<TNode, TEdge, TEdgeRoute> m_graph = s_emptyGraph;
        private IObservableContext m_observableContext;
        private ISelectionContext m_selectionContext;
        private IVisibilityContext m_visibilityContext;
        
        /// <summary>
        /// EmptyGraph object</summary>
        protected static readonly EmptyGraph s_emptyGraph = new EmptyGraph();
    }
}
