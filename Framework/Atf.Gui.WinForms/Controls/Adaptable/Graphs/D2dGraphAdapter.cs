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
            m_renderer.Redraw += new EventHandler(renderer_Redraw);
            m_transformAdapter = transformAdapter;
            m_renderer.GetStyle = GetStyle;
        }

        public enum DrawEdgePolicy
        {
            Associated, // default,  draw an edge after its start and end nodes are drawn
            AllFirst    // draw all edges first, followed by nodes
        }

        public DrawEdgePolicy EdgeRenderPolicy { get; set; }

        #region IDisposable Members

        /// <summary>
        /// Releases non-memory resources</summary>
        public void Dispose()
        {
             m_renderer.Redraw -= new EventHandler(renderer_Redraw);
             Dispose(true);
        }

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
        public DiagramDrawingStyle GetStyle(object item)
        {
            DiagramDrawingStyle result = DiagramDrawingStyle.Normal;
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

                if (CircuitUtil.IsGroupTemplateInstance(item))
                    result = DiagramDrawingStyle.TemplatedInstance;
                else if (item.Is<Group>())
                    result = DiagramDrawingStyle.CopyInstance;
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
        /// This method is used for hidding an edge while dragging its replacement.</summary>
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
            Matrix3x2F invXform = Matrix3x2F.Invert(m_d2dGraphics.Transform);
            PointF pt = Matrix3x2F.TransformPoint(invXform, pickPoint);

            TEdge priorityEdge = null;
            if (m_selectionContext != null)
                priorityEdge = m_selectionContext.GetLastSelected<TEdge>();
            return m_renderer.Pick(m_graph, priorityEdge, pt, m_d2dGraphics);                                    
        }

        /// <summary>
        /// Performs hit test for a point, in client coordinates</summary>   
        /// <returns>Hit record for a point, in client coordinates</returns>
        public GraphHitRecord<TNode, TEdge, TEdgeRoute> Pick(IEnumerable<TNode> nodes, IEnumerable<TEdge> edges, Point pickPoint)
        {
            Matrix3x2F invXform = Matrix3x2F.Invert(m_d2dGraphics.Transform);
            PointF pt = Matrix3x2F.TransformPoint(invXform, pickPoint);

            TEdge priorityEdge = null;
            if (m_selectionContext != null)
                priorityEdge = m_selectionContext.GetLastSelected<TEdge>();
            return m_renderer.Pick(nodes, edges, priorityEdge, pt, m_d2dGraphics);
        }

        /// <summary>
        /// Performs hit testing for rectangle bounds, in client coordinates</summary>
        /// <param name="pickRect">Pick rectangle, in client coordinates</param>
        /// <returns>Items that overlap with the rectangle, in client coordinates</returns>
        public virtual IEnumerable<object> Pick(Rectangle pickRect)
        {
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
            bounds = D2dUtil.Transform(m_d2dGraphics.Transform, bounds);
            return Rectangle.Truncate(bounds);
        }

        /// <summary>
        /// Gets a bounding rectangle for the item in graph space</summary>
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
            m_d2dControl = (D2dAdaptableControl)control;
            m_d2dGraphics = m_d2dControl.D2dGraphics;
            m_d2dControl.ContextChanged += control_ContextChanged;
            m_d2dControl.DrawingD2d += control_Paint;
            m_d2dControl.MouseDown += d2dControl_MouseDown;
            m_d2dControl.MouseMove += d2dControl_MouseMove;
            m_d2dControl.MouseLeave += d2dControl_MouseLeave;
            m_selectionPathProvider = control.As<ISelectionPathProvider>();  
        }
        
        
        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            if (m_d2dControl != control)
                throw new InvalidOperationException("We can only unbind from a D2dAdaptableControl that we previously were bound to");
            m_d2dControl.ContextChanged -= control_ContextChanged;
            m_d2dControl.DrawingD2d -= control_Paint;
            m_d2dControl.MouseDown -= d2dControl_MouseDown;
            m_d2dControl.MouseMove -= d2dControl_MouseMove;
            m_d2dControl.MouseLeave -= d2dControl_MouseLeave;
            m_d2dGraphics = null;
            m_d2dControl = null;
            m_selectionPathProvider = null;
        }

        /// <summary>
        /// Renders entire graph</summary>
        protected virtual void OnRender()
        {
            m_d2dGraphics.AntialiasMode = D2dAntialiasMode.PerPrimitive;
            Matrix3x2F invMtrx = m_d2dGraphics.Transform;
            invMtrx.Invert();
            RectangleF boundsGr = Matrix3x2F.Transform(invMtrx, this.AdaptedControl.ClientRectangle);

            if (EdgeRenderPolicy == DrawEdgePolicy.AllFirst)
            {
                foreach (var edge in m_graph.Edges)
                    DrawEdge(edge, boundsGr);
            }
            else
            {
                // build node to edge maps
                m_fromNodeEdges.Clear();
                m_toNodeEdges.Clear();
                m_edgeNodeEncounter.Clear();
                foreach (TEdge edge in m_graph.Edges)
                {
                    m_fromNodeEdges.Add(edge.FromNode, edge);
                    m_toNodeEdges.Add(edge.ToNode, edge);
                    m_edgeNodeEncounter.Add(edge, 0);
                }
            }


            // Draw normal nodes first
            TNode selectedGroup = null;
            var draggingNodes = new List<TNode>();
            var expandedGroupNodes = new List<TNode>();
            foreach (var node in m_graph.Nodes)
            {
                RectangleF nodeBounds = m_renderer.GetBounds(node, m_d2dGraphics);
                if (boundsGr.IntersectsWith(nodeBounds))
                {
                    bool expandedGroup = false;
                    if (node.Is<ICircuitGroupType<TNode, TEdge, TEdgeRoute>>())
                    {
                        var group = node.Cast<ICircuitGroupType<TNode, TEdge, TEdgeRoute>>();
                        group.Info.PickingPriority = 0;
                        if (group.Expanded)
                        {

                            if (node == ActicveContainer())
                                selectedGroup = node;
                            else
                                expandedGroupNodes.Add(node);
                            expandedGroup = true;
                        }
                    }

                    if (!expandedGroup)
                    {
                        if (m_renderer.GetCustomStyle(node) == DiagramDrawingStyle.DragSource)
                            draggingNodes.Add(node);
                        else
                        {
                            m_renderer.Draw(node, GetStyle(node), m_d2dGraphics);
                            if (EdgeRenderPolicy == DrawEdgePolicy.Associated)
                                TryDrawAssociatedEdges(node, boundsGr);
                        }
                    }
                }
                else if (EdgeRenderPolicy == DrawEdgePolicy.Associated)
                    TryDrawAssociatedEdges(node, boundsGr);
            }

            int pickPriority = 0;
            foreach (var node in expandedGroupNodes)
            {
                var group = node.Cast<ICircuitGroupType<TNode, TEdge, TEdgeRoute>>();
                group.Info.PickingPriority = pickPriority++;
                m_renderer.Draw(node, GetStyle(node), m_d2dGraphics);
                if (EdgeRenderPolicy == DrawEdgePolicy.Associated)
                    TryDrawAssociatedEdges(node, boundsGr);
            }

            if (selectedGroup != null)
            {
                var group = selectedGroup.Cast<ICircuitGroupType<TNode, TEdge, TEdgeRoute>>();
                group.Info.PickingPriority = pickPriority++;
                m_renderer.Draw(selectedGroup, GetStyle(selectedGroup), m_d2dGraphics);
                if (EdgeRenderPolicy == DrawEdgePolicy.Associated)
                    TryDrawAssociatedEdges(selectedGroup, boundsGr);
            }

            // Draw dragging nodes last to ensure they are visible (necessary for container-crossing move operation)
            foreach (var node in draggingNodes)
            {
                m_renderer.Draw(node, DiagramDrawingStyle.DragSource, m_d2dGraphics);
                if (EdgeRenderPolicy == DrawEdgePolicy.Associated)
                    TryDrawAssociatedEdges(node, boundsGr);
            }
        }


        private object ActicveContainer()
        {
            if (m_selectionContext != null )
            {
                return m_selectionPathProvider.Parent(m_selectionContext.LastSelected);
            }
            return null;
        }


        // draw an edge as soon as both of its start and end nodes are drawn
        private void TryDrawAssociatedEdges(TNode nodeDrawn, RectangleF clipBounds)
        {
            if (m_fromNodeEdges.ContainsKey(nodeDrawn))
            {
                var edges = m_fromNodeEdges[nodeDrawn];
                foreach (var edge in edges)
                {
                    m_edgeNodeEncounter[edge] = m_edgeNodeEncounter[edge] + 1;
                    if (m_edgeNodeEncounter[edge] == 2) // draw edge
                    {
                        DrawEdge(edge, clipBounds);
                    }
                }
            }

            if (m_toNodeEdges.ContainsKey(nodeDrawn))
            {
                var edges = m_toNodeEdges[nodeDrawn];
                foreach (var edge in edges)
                {
                    m_edgeNodeEncounter[edge] = m_edgeNodeEncounter[edge] + 1;
                    if (m_edgeNodeEncounter[edge] == 2) //draw edge
                    {
                        DrawEdge(edge, clipBounds);
                    }
                }
            }
        }

        private void DrawEdge(TEdge edge, RectangleF clipBounds)
        {
            if (edge == m_hiddenEdge) return;

            RectangleF edgeBounds = m_renderer.GetBounds(edge, m_d2dGraphics);
            if (!clipBounds.IntersectsWith(edgeBounds)) return;

            DiagramDrawingStyle style = GetStyle(edge);
            m_renderer.Draw(edge, style, m_d2dGraphics);
        }

        private void control_Paint(object sender, EventArgs e)
        {
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
                var hoverHitRecord= pickAdapter.Pick(e.Location);
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
                        m_observableContext.ItemInserted -= new EventHandler<ItemInsertedEventArgs<object>>(graph_ObjectInserted);
                        m_observableContext.ItemRemoved -= new EventHandler<ItemRemovedEventArgs<object>>(graph_ObjectRemoved);
                        m_observableContext.ItemChanged -= new EventHandler<ItemChangedEventArgs<object>>(graph_ObjectChanged);
                        m_observableContext.Reloaded -= new EventHandler(graph_Reloaded);
                        m_observableContext = null;
                    }
                    if (m_selectionContext != null)
                    {
                        m_selectionContext.SelectionChanged -= new EventHandler(selection_Changed);
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
                        m_observableContext.ItemInserted += new EventHandler<ItemInsertedEventArgs<object>>(graph_ObjectInserted);
                        m_observableContext.ItemRemoved += new EventHandler<ItemRemovedEventArgs<object>>(graph_ObjectRemoved);
                        m_observableContext.ItemChanged += new EventHandler<ItemChangedEventArgs<object>>(graph_ObjectChanged);
                        m_observableContext.Reloaded += new EventHandler(graph_Reloaded);
                    }

                    m_selectionContext = AdaptedControl.ContextAs<ISelectionContext>();
                    if (m_selectionContext != null)
                    {
                        m_selectionContext.SelectionChanged += new EventHandler(selection_Changed);
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
            Invalidate();
        }

        private void graph_ObjectRemoved(object sender, ItemRemovedEventArgs<object> e)
        {
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

        private void Invalidate()
        {
            var d2dControl = this.AdaptedControl as D2dAdaptableControl;
            if (d2dControl != null)
                d2dControl.Invalidate();
        }

        // Empty graph to simplify code when there is no graph
        protected class EmptyGraph : IGraph<TNode, TEdge, TEdgeRoute>
        {
            public IEnumerable<TNode> Nodes
            {
                get { return EmptyEnumerable<TNode>.Instance; }
            }

            public IEnumerable<TEdge> Edges
            {
                get { return EmptyEnumerable<TEdge>.Instance; }
            }
        }

        // custom style is mainly used in expanded group node for in-place editing selection highlight
        private void ResetCustomStyle(object item)
        {
            if (m_selectionContext != null && m_selectionContext.SelectionContains(item))
                m_renderer.SetCustomStyle(item, DiagramDrawingStyle.Selected);
            else
                m_renderer.ClearCustomStyle(item);
        }

        private D2dAdaptableControl m_d2dControl;
        private D2dGraphics m_d2dGraphics;
        private readonly D2dGraphRenderer<TNode, TEdge, TEdgeRoute> m_renderer;
        private readonly ITransformAdapter m_transformAdapter;
        private object m_hoverObject;
        private object m_hoverSubObject;
        private ISelectionPathProvider m_selectionPathProvider;
      
        private TEdge m_hiddenEdge; // hide edge when dragging its replacement.
        private IGraph<TNode, TEdge, TEdgeRoute> m_graph = s_emptyGraph;
        private IObservableContext m_observableContext;
        private ISelectionContext m_selectionContext;
        private IVisibilityContext m_visibilityContext;
        private Multimap<TNode, TEdge> m_fromNodeEdges = new Multimap<TNode, TEdge>();
        private Multimap<TNode, TEdge> m_toNodeEdges = new Multimap<TNode, TEdge>();
        private Dictionary<TEdge, int> m_edgeNodeEncounter = new Dictionary<TEdge, int>();



        protected static readonly EmptyGraph s_emptyGraph = new EmptyGraph();
    }
}
