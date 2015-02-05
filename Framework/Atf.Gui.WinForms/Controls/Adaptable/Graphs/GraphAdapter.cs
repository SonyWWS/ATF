//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// OBSOLETE. Please use D2dGraphAdapter instead.
    /// Control adapter to reference and render a graph diagram; also provides hit testing
    /// with the Pick method, and viewing support with the Frame and EnsureVisible methods.</summary>
    /// <typeparam name="TNode">Node</typeparam>
    /// <typeparam name="TEdge">Edge</typeparam>
    /// <typeparam name="TEdgeRoute">Edge route</typeparam>
    public class GraphAdapter<TNode, TEdge, TEdgeRoute> : ControlAdapter,
        IGraphAdapter<TNode, TEdge, TEdgeRoute>,
        IPickingAdapter,
        IPrintingAdapter,
        IDisposable

        where TNode : class, IGraphNode
        where TEdge : class, IGraphEdge<TNode, TEdgeRoute>
        where TEdgeRoute : class, IEdgeRoute
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="renderer">Graph renderer, to draw and hit-test graph</param>
        /// <param name="transformAdapter">Transform adapter</param>
        public GraphAdapter(
            GraphRenderer<TNode, TEdge, TEdgeRoute> renderer,
            ITransformAdapter transformAdapter)
        {
            m_renderer = renderer;
            m_renderer.Redraw += renderer_Redraw;
            m_transformAdapter = transformAdapter;
        }

        #region IDisposable Members

        /// <summary>
        /// Releases non-memory resources</summary>
        public void Dispose()
        {
            m_renderer.Redraw -= renderer_Redraw;
        }

        #endregion

        /// <summary>
        /// Gets the renderer for the adapter</summary>
        public GraphRenderer<TNode, TEdge, TEdgeRoute> Renderer
        {
            get { return m_renderer; }
        }

        /// <summary>
        /// Gets the bounding rectangle of the given node in client coordinates</summary>
        /// <param name="node">Graph node</param>
        /// <returns>Bounding rectangle of node</returns>
        public Rectangle GetBounds(TNode node)
        {
            Rectangle bounds;
            using (Graphics g = AdaptedControl.CreateGraphics())
            {
                g.Transform = m_transformAdapter.Transform;
                bounds = m_renderer.GetBounds(node, g);
            }
            return bounds;
        }

        /// <summary>
        /// Gets the bounding rectangle of the nodes in client coordinates</summary>
        /// <param name="nodes">Graph nodes</param>
        /// <returns>Bounding rectangle of nodes</returns>
        public Rectangle GetBounds(IEnumerable<TNode> nodes)
        {
            Rectangle bounds;
            using (Graphics g = AdaptedControl.CreateGraphics())
            {
                g.Transform = m_transformAdapter.Transform;
                bounds = m_renderer.GetBounds(nodes, g);
            }
            return bounds;
        }

        /// <summary>
        /// Frames a node in the adapted control</summary>
        /// <param name="node">GraphNode</param>
        public void Frame(TNode node)
        {
            Rectangle bounds = GetBounds(node);
            m_transformAdapter.Frame(bounds);
        }

        /// <summary>
        /// Frames the given nodes in the adapted control</summary>
        /// <param name="nodes">Collection of nodes</param>
        public void Frame(IEnumerable<TNode> nodes)
        {
            Rectangle bounds = GetBounds(nodes);
            m_transformAdapter.Frame(bounds);
        }

        /// <summary>
        /// Ensures the nodes are visible in the adapted control</summary>
        /// <param name="nodes">Nodes</param>
        public void EnsureVisible(IEnumerable<TNode> nodes)
        {
            Rectangle bounds = GetBounds(nodes);
            m_transformAdapter.EnsureVisible(bounds);
        }

        /// <summary>
        /// Performs a picking operation on the graph with the point</summary>
        /// <param name="p">Hit test point</param>
        /// <returns>Hit record resulting from picking operation</returns>
        public GraphHitRecord<TNode, TEdge, TEdgeRoute> Pick(Point p)
        {
            // use cache to speed up multiple pick requests at the same point
            if (m_cachedHitRecord == null || p != m_cachedHitPoint)
            {
                m_cachedHitPoint = p;

                TEdge priorityEdge = null;
                if (m_selectionContext != null)
                    priorityEdge = m_selectionContext.GetLastSelected<TEdge>();

                p = GdiUtil.InverseTransform(m_transformAdapter.Transform, p);
                using (Graphics g = AdaptedControl.CreateGraphics())
                {
                    m_cachedHitRecord = m_renderer.Pick(m_graph, priorityEdge, p, g);
                }
            }

            return m_cachedHitRecord;
        }

        /// <summary>
        /// Performs a picking operation and returns enumeration of Nodes intersecting the region</summary>
        /// <param name="pickRegion">Hit test region</param>
        /// <returns>Nodes intersecting the pick region</returns>
        public IEnumerable<TNode> Pick(Region pickRegion)
        {
            return Pick<TNode>(pickRegion);
        }

        /// <summary>
        /// Performs a picking operation and returns enumeration of Nodes intersecting the region</summary>
        /// <param name="pickRegion">Hit test region</param>
        /// <typeparam name="T">Type of objects intersecting pick region</typeparam>
        /// <returns>Objects of type T intersecting the pick region</returns>
        /// <remarks>The default implementation only returns intersecting Nodes, but derived
        /// classes can override this method to return Edges or EdgeRoutes as well.</remarks>
        public virtual IEnumerable<T> Pick<T>(Region pickRegion) where T : class
        {
            List<TNode> pickedGraphNodes = new List<TNode>();
            using (Graphics g = AdaptedControl.CreateGraphics())
            {
                RectangleF pickRect = pickRegion.GetBounds(g);
                pickRect = GdiUtil.InverseTransform(m_transformAdapter.Transform, pickRect);
                foreach (TNode node in m_graph.Nodes)
                {
                    RectangleF nodeBounds = m_renderer.GetBounds(node, g);
                    if (nodeBounds.IntersectsWith(pickRect))
                        pickedGraphNodes.Add(node);
                }
            }

            return pickedGraphNodes.AsIEnumerable<T>();
        }

        #region IGraphAdapter Members

        /// <summary>
        /// Sets the rendering style for a diagram item. This style will override the
        /// normal style used by the graph adapter.</summary>
        /// <param name="item">Rendered item</param>
        /// <param name="style">Diagram style to use for item</param>
        public void SetStyle(object item, DiagramDrawingStyle style)
        {
            m_styles[item] = style;
        }

        /// <summary>
        /// Resets the rendering style for a diagram item</summary>
        /// <param name="item">Rendered item</param>
        public void ResetStyle(object item)
        {
            m_styles.Remove(item);
        }

        /// <summary>
        /// Gets the current rendering style for an item</summary>
        /// <param name="item">Rendered item</param>
        /// <returns>Rendering style set by SetStyle, Normal if no override is set.</returns>
        public DiagramDrawingStyle GetStyle(object item)
        {
            DiagramDrawingStyle result; // default enum value is DiagramDrawingStyle.Normal
            if (!m_styles.TryGetValue(item, out result))
            {
                // no override
                if (m_visibilityContext != null)
                {
                    if (!m_visibilityContext.IsVisible(item))
                        result = DiagramDrawingStyle.Hidden;
                }

                if (m_selectionContext != null &&
                    m_selectionContext.SelectionContains(item))
                {
                    if (m_selectionContext.LastSelected.Equals(item))
                        result = DiagramDrawingStyle.LastSelected;
                    else
                        result = DiagramDrawingStyle.Selected;
                }
            }
            return result;
        }

        #endregion

        #region IPickingAdapter Members

        DiagramHitRecord IPickingAdapter.Pick(Point pickPoint)
        {
            return Pick(pickPoint);
        }

        IEnumerable<object> IPickingAdapter.Pick(Region pickRegion)
        {
            return Pick<object>(pickRegion);
        }

        Rectangle IPickingAdapter.GetBounds(IEnumerable<object> items)
        {
            return GetBounds(items.AsIEnumerable<TNode>());
        }

        #endregion

        #region IPrintingAdapter Members

        void IPrintingAdapter.Print(PrintDocument printDocument, Graphics g)
        {
            switch (printDocument.PrinterSettings.PrintRange)
            {
                case PrintRange.Selection:
                    PrintSelection(g);
                    break;

                default:
                    PrintAll(g);
                    break;
            }
        }

        private void PrintSelection(Graphics g)
        {
            if (m_selectionContext != null)
            {
                HashSet<TNode> selectedNodes = new HashSet<TNode>();
                foreach (TNode node in m_graph.Nodes)
                {
                    m_renderer.Draw(node, DiagramDrawingStyle.Normal, g);
                    selectedNodes.Add(node);
                }
                foreach (TEdge edge in m_graph.Edges)
                {
                    if (selectedNodes.Contains(edge.FromNode) &&
                        selectedNodes.Contains(edge.ToNode))
                    {
                        m_renderer.Draw(edge, DiagramDrawingStyle.Normal, g);
                    }
                }
            }
        }

        private void PrintAll(Graphics g)
        {
            foreach (TNode node in m_graph.Nodes)
                m_renderer.Draw(node, DiagramDrawingStyle.Normal, g);
            foreach (TEdge edge in m_graph.Edges)
                m_renderer.Draw(edge, DiagramDrawingStyle.Normal, g);
        }

        #endregion

        /// <summary>
        /// Binds the adapter to the adaptable control; called in the order that the adapters
        /// were defined on the control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Bind(AdaptableControl control)
        {
            control.ContextChanged += control_ContextChanged;
            control.Paint += control_Paint;
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            control.ContextChanged -= control_ContextChanged;
            control.Paint -= control_Paint;
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

        private void control_Paint(object sender, PaintEventArgs e)
        {
            Matrix transform = m_transformAdapter.Transform;
            Matrix oldTransform = e.Graphics.Transform;
            Region oldClip = e.Graphics.Clip;

            e.Graphics.Transform = transform;
            Rectangle clip = GdiUtil.InverseTransform(e.Graphics.Transform, e.ClipRectangle);
            e.Graphics.SetClip(clip);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            foreach (TNode node in m_graph.Nodes)
            {
                DiagramDrawingStyle style = GetStyle(node);
                m_renderer.Draw(node, style, e.Graphics);
            }

            foreach (TEdge edge in m_graph.Edges)
            {
                DiagramDrawingStyle style = GetStyle(edge);
                m_renderer.Draw(edge, style, e.Graphics);
            }

            e.Graphics.Transform = oldTransform;
            e.Graphics.Clip = oldClip;
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
            m_cachedHitRecord = null;
            AdaptedControl.Invalidate();
        }

        // Empty graph to simplify code when there is no graph
        private class EmptyGraph : IGraph<TNode, TEdge, TEdgeRoute>
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

        private readonly GraphRenderer<TNode, TEdge, TEdgeRoute> m_renderer;
        private readonly ITransformAdapter m_transformAdapter;

        private IGraph<TNode, TEdge, TEdgeRoute> m_graph = s_emptyGraph;
        private IObservableContext m_observableContext;
        private ISelectionContext m_selectionContext;
        private IVisibilityContext m_visibilityContext;

        private Point m_cachedHitPoint;
        private GraphHitRecord<TNode, TEdge, TEdgeRoute> m_cachedHitRecord;

        private readonly Dictionary<object, DiagramDrawingStyle> m_styles = new Dictionary<object, DiagramDrawingStyle>();

        private static readonly EmptyGraph s_emptyGraph = new EmptyGraph();
    }
}