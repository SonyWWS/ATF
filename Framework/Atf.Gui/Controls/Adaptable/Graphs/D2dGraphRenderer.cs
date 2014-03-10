//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using Sce.Atf.Adaptation;
using Sce.Atf.Direct2D;
using Sce.Atf.Rendering;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Abstract base class for graph renderers, which render and hit-test a graph</summary>
    public abstract class D2dGraphRenderer<TNode, TEdge, TEdgeRoute> : DiagramRenderer
        where TNode : class, IGraphNode
        where TEdge : class, IGraphEdge<TNode, TEdgeRoute>
        where TEdgeRoute : class, IEdgeRoute
    {
        public float EdgeThickness { get; set; }
        public float MinimumEdgeThickness { get; set; }
        public float MaxiumEdgeThickness { get; set; }


        /// <summary>
        /// Draws a graph node</summary>
        /// <param name="node">Node to draw</param>
        /// <param name="style">Diagram drawing style</param>
        /// <param name="g">D2dGraphics object</param>
        public abstract void Draw(TNode node, DiagramDrawingStyle style, D2dGraphics g);

        /// <summary>
        /// Draws a graph edge</summary>
        /// <param name="edge">Edge to draw</param>
        /// <param name="style">Diagram drawing style</param>
        /// <param name="g">D2dGraphics object</param>
        public abstract void Draw(TEdge edge, DiagramDrawingStyle style, D2dGraphics g);

        /// <summary>
        /// Draws a partially defined graph edge</summary>
        /// <param name="fromNode">Source node or null</param>
        /// <param name="fromRoute">Source route or null</param>
        /// <param name="toNode">Destination node or null</param>
        /// <param name="toRoute">Destination route or null</param>
        /// <param name="label">Edge label</param>
        /// <param name="endPoint">Endpoint to substitute for source or destination, if null</param>
        /// <param name="g">D2dGraphics object</param>
        public abstract void Draw(
            TNode fromNode,
            TEdgeRoute fromRoute,
            TNode toNode,
            TEdgeRoute toRoute,
            string label,
            Point endPoint,
            D2dGraphics g);

        /// <summary>
        /// Draws a partially defined graph edge</summary>
        /// <param name="fromNode">Source node or null</param>
        /// <param name="fromRoute">Source route or null</param>
        /// <param name="toNode">Destination node or null</param>
        /// <param name="toRoute">Destination route or null</param>
        /// <param name="label">Edge label</param>
        /// <param name="startPoint">Startpoint for source</param>
        /// <param name="endPoint">Endpoint for source</param>
        /// <param name="g">D2dGraphics object</param>
        public virtual void DrawPartialEdge(
            TNode fromNode,
            TEdgeRoute fromRoute,
            TNode toNode,
            TEdgeRoute toRoute,
            string label,
            PointF startPoint,
            PointF endPoint,
            D2dGraphics g)
        {}

        /// <summary>
        /// Draws a graph, using an optional selection to determine which nodes should be
        /// highlighted</summary>
        /// <param name="graph">Graph to render</param>
        /// <param name="selection">Selection containing graph nodes and edges, or null</param>
        /// <param name="g">D2dGraphics device</param>
        public void Draw(IGraph<TNode, TEdge, TEdgeRoute> graph, Selection<object> selection, D2dGraphics g)
        {
            foreach (TNode node in graph.Nodes)
            {
                DiagramDrawingStyle style = DiagramDrawingStyle.Normal;
                if (selection != null && selection.Contains(node))
                {
                    if (selection.LastSelected.Equals(node))
                        style = DiagramDrawingStyle.LastSelected;
                    else
                        style = DiagramDrawingStyle.Selected;
                }

                Draw(node, style, g);
            }

            foreach (TEdge edge in graph.Edges)
            {
                DiagramDrawingStyle style = DiagramDrawingStyle.Normal;
                if (selection != null && selection.Contains(edge))
                {
                    if (selection.LastSelected.Equals(edge))
                        style = DiagramDrawingStyle.LastSelected;
                    else
                        style = DiagramDrawingStyle.Selected;
                }

                Draw(edge, style, g);
            }
        }

        /// <summary>
        /// Draws a graph in a print friendly way</summary>
        /// <param name="graph">Graph to render</param>
        /// <param name="g">D2dGraphics device</param>
        public void Print(IGraph<TNode, TEdge, TEdgeRoute> graph, D2dGraphics g)
        {
            try
            {
                IsPrinting = true;
                Draw(graph, null, g);
            }
            finally
            {
                IsPrinting = false;
            }
        }

        /// <summary>
        /// Gets the bounding rect of an edge, in graph space (world space)</summary>
        /// <param name="edge">Edge</param>
        /// <param name="g">D2dGraphics object</param>
        /// <returns>Rectangle completely bounding the node in graph space</returns>
        public virtual RectangleF GetBounds(TEdge edge, D2dGraphics g)
        {
            RectangleF bounds = GetBounds(edge.FromNode, g);
            bounds = RectangleF.Union(bounds, GetBounds(edge.ToNode, g));
            return bounds;
        }

        /// <summary>
        /// Gets the bounding rect of a node</summary>
        /// <param name="node">Graph node</param>
        /// <param name="g">D2dGraphics object</param>
        /// <returns>Rectangle completely bounding the node in graph space</returns>
        public virtual RectangleF GetBounds(TNode node, D2dGraphics g)
        {
            return node.Bounds;
        }

        /// <summary>
        /// Gets the bounding rect of a collection of nodes</summary>
        /// <param name="nodes">Graph nodes whose bounding rect is to be calculated</param>
        /// <param name="g">D2dGraphics object</param>
        /// <returns>Rectangle completely bounding the graph nodes in graph space</returns>
        public RectangleF GetBounds(IEnumerable<TNode> nodes, D2dGraphics g)
        {
            RectangleF bounds = RectangleF.Empty;
            bool firstTime = true;
            foreach (var node in nodes)
            {
                if (firstTime)
                {
                    bounds = GetBounds(node, g);
                    firstTime = false;
                }
                else
                {
                    bounds = RectangleF.Union(bounds, GetBounds(node, g));
                }
            }
            return bounds;
        }

        /// <summary>
        /// Finds node and/or edge hit by the given point</summary>
        /// <param name="graph">Graph to test</param>
        /// <param name="priorityEdge">Graph edge to test before others</param>
        /// <param name="p">Point to test in graph space</param>
        /// <param name="g">D2dGraphics object</param>
        /// <returns>Hit record containing node and/or edge hit by the given point</returns>
        public abstract GraphHitRecord<TNode, TEdge, TEdgeRoute> Pick(
            IGraph<TNode, TEdge, TEdgeRoute> graph, TEdge priorityEdge, PointF p, D2dGraphics g);

        /// <summary>
        /// Finds node and/or edge hit by the given point</summary>
        /// <param name="nodes">nodes to test</param>
        /// <param name="edges">edges to test</param>
        /// <param name="priorityEdge">Graph edge to test before others</param>
        /// <param name="p">Point to test in graph space</param>
        /// <param name="g">D2dGraphics object</param>
        /// <returns>Hit record containing node and/or edge hit by the given point</returns>
        public virtual GraphHitRecord<TNode, TEdge, TEdgeRoute> Pick(
            IEnumerable<TNode> nodes, IEnumerable<TEdge> edges, TEdge priorityEdge, PointF p, D2dGraphics g)
        {
            throw new NotImplementedException("Override in derived class");
        }

        /// <summary>
        /// Finds node and/or edge hit by the given rect</summary>
        /// <param name="graph">Graph to test</param>
        /// <param name="rect">given rect in graph space</param>
        /// <param name="g">D2dGraphics object</param>
        /// <returns>Hit record containing node and/or edge hit by the given rect</returns>
        public virtual IEnumerable<object> Pick(
            IGraph<TNode, TEdge, TEdgeRoute> graph, RectangleF rect, D2dGraphics g)
          {
              List<object> pickedGraphNodes = new List<object>();
              foreach (TNode node in graph.Nodes)
              {
                  var visible = node.As<IVisible>();
                  if (visible != null && !visible.Visible)
                      continue;
                  RectangleF nodeBounds = GetBounds(node, g);
                  if (nodeBounds.IntersectsWith(rect))
                      pickedGraphNodes.Add(node);
              }
              return pickedGraphNodes;    
          }

        /// <summary>
        /// An enumeration of drawing modes of edge routes (such as pins in the circuit editor)</summary>
        public enum EdgeRouteDrawMode
        {
            /// <summary>
            /// RouteConnecting is null; no connecting is going on by the user</summary>
            Normal, 
            /// <summary>
            /// User is actively connecting wires and this edge route can be connected</summary>
            CanConnect,
            /// <summary>
            /// User is actively connecting wires and this edge route can't be connected</summary>
            CannotConnect
        }

        /// <summary>
        /// Gets the edge route drawing mode for given node and its edge route (e.g., pin)</summary>
        /// <param name="destinationNode">Destination node</param>
        /// <param name="destination">Destination edge route</param>
        /// <returns>Drawing mode</returns>
        public virtual EdgeRouteDrawMode GetEdgeRouteDrawMode(TNode destinationNode, TEdgeRoute destination)
        {
            return EdgeRouteDrawMode.Normal;
        }

        /// <summary>
        /// Is called when the content of the graph object changes.
        /// </summary>
        public virtual void OnGraphObjectChanged(object sender, ItemChangedEventArgs<object> e)
        {
        }


        /// <summary>
        /// Set a custom style that overrides default one during graph rendering </summary>
        /// <param name="node">Graph node to use the custom style</param>
        /// <param name="style">Custom style</param>
        public void SetCustomStyle(object node, DiagramDrawingStyle style)
        {
            if (node != null)
            {              
                m_customStyles[node] = style;
            }
        }

        /// <summary>
        /// Clear a custom style that was registered for the given object
        /// </summary>
        /// <param name="node"></param>
        public void ClearCustomStyle(object node)
        {
            if (node != null)
            {
                m_customStyles.Remove(node);
            }
        }

        public DiagramDrawingStyle GetCustomStyle(object node)
        {
            DiagramDrawingStyle style;
            if (node != null && m_customStyles.TryGetValue(node, out style))
                return style;
            return DiagramDrawingStyle.None;
        }

        /// <summary>
        /// Describes the start of a connection being made. All the fields should be non-null.</summary>
        public class RouteConnectingInfo
        {
            /// <summary>
            /// Graph</summary>
            public IEditableGraph<TNode, TEdge, TEdgeRoute> EditableGraph;

            /// <summary>
            /// Starting node</summary>
            public TNode StartNode;

            /// <summary>
            /// Starting edge route</summary>
            public TEdgeRoute StartRoute;
        }

        /// <summary>
        /// Gets or sets the connecting info. If null, then the user is not connecting anything.</summary>
        public RouteConnectingInfo RouteConnecting
        {
            get;
            set;
        }

        private readonly Dictionary<object, DiagramDrawingStyle> m_customStyles = new Dictionary<object, DiagramDrawingStyle>();
 
    }
}
