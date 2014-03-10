//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Drawing;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Abstract base class for graph renderers, which render and hit-test a graph</summary>
    /// <typeparam name="TNode">Node type, must implement IGraphNode</typeparam>
    /// <typeparam name="TEdge">Edge type, must implement IGraphEdge</typeparam>
    /// <typeparam name="TEdgeRoute">Edge route type, must implement IEdgeRoute</typeparam>
    public abstract class GraphRenderer<TNode, TEdge, TEdgeRoute> : DiagramRenderer
        where TNode : class, IGraphNode
        where TEdge : class, IGraphEdge<TNode, TEdgeRoute>
        where TEdgeRoute : class, IEdgeRoute
    {
        /// <summary>
        /// Draws a graph node</summary>
        /// <param name="node">Node to draw</param>
        /// <param name="style">Diagram drawing style</param>
        /// <param name="g">Graphics object</param>
        public abstract void Draw(TNode node, DiagramDrawingStyle style, Graphics g);

        /// <summary>
        /// Draws a graph edge</summary>
        /// <param name="edge">Edge to draw</param>
        /// <param name="style">Diagram drawing style</param>
        /// <param name="g">Graphics object</param>
        public abstract void Draw(TEdge edge, DiagramDrawingStyle style, Graphics g);

        /// <summary>
        /// Draws a partially defined graph edge</summary>
        /// <param name="fromNode">Source node, or null</param>
        /// <param name="fromRoute">Source route, or null</param>
        /// <param name="toNode">Destination node, or null</param>
        /// <param name="toRoute">Destination route, or null</param>
        /// <param name="label">Edge label</param>
        /// <param name="endPoint">Endpoint to substitute for source or destination, if either is null</param>
        /// <param name="g">Graphics object</param>
        public abstract void Draw(
            TNode fromNode,
            TEdgeRoute fromRoute,
            TNode toNode,
            TEdgeRoute toRoute,
            string label,
            Point endPoint,
            Graphics g);

        /// <summary>
        /// Draws a graph, using an optional selection to determine which nodes should be
        /// highlighted</summary>
        /// <param name="graph">Graph to render</param>
        /// <param name="selection">Selection containing graph nodes and edges, or null</param>
        /// <param name="g">Graphics device</param>
        public void Draw(IGraph<TNode, TEdge, TEdgeRoute> graph, Selection<object> selection, Graphics g)
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
        /// <param name="g">Graphics device</param>
        public void Print(IGraph<TNode, TEdge, TEdgeRoute> graph, Graphics g)
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
        /// Gets the bounding rectangle of a node</summary>
        /// <param name="node">Graph node</param>
        /// <param name="g">Graphics object</param>
        /// <returns>Rectangle completely bounding the node</returns>
        public abstract Rectangle GetBounds(TNode node, Graphics g);

        /// <summary>
        /// Get the bounding rectangle of a collection of nodes</summary>
        /// <param name="nodes">Graph nodes whose bounding rectangle is to be calculated</param>
        /// <param name="g">Graphics object</param>
        /// <returns>Rectangle completely bounding the graph nodes</returns>
        public Rectangle GetBounds(IEnumerable<TNode> nodes, Graphics g)
        {
            Rectangle bounds = Rectangle.Empty;
            bool firstTime = true;
            foreach (TNode node in nodes)
            {
                if (firstTime)
                {
                    bounds = GetBounds(node, g);
                    firstTime = false;
                }
                else
                {
                    bounds = Rectangle.Union(bounds, GetBounds(node, g));
                }
            }
            return bounds;
        }

        /// <summary>
        /// Finds node and/or edge hit by the given point</summary>
        /// <param name="graph">Graph to test</param>
        /// <param name="priorityEdge">Graph edge to test before others</param>
        /// <param name="p">Point to test</param>
        /// <param name="g">Graphics object</param>
        /// <returns>Hit record containing node and/or edge hit by the given point</returns>
        public abstract GraphHitRecord<TNode, TEdge, TEdgeRoute> Pick(
            IGraph<TNode, TEdge, TEdgeRoute> graph, TEdge priorityEdge, Point p, Graphics g);
    }
}
