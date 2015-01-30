//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Hit record specifying the node, edge, and in/out edge routes from graph
    /// picking operation. Additionally, node and edge labels may be specified
    /// as the item part.</summary>
    /// <typeparam name="TNode">Node type</typeparam>
    /// <typeparam name="TEdge">Edge type</typeparam>
    /// <typeparam name="TEdgeRoute">In/out edge route type</typeparam>
    public class GraphHitRecord<TNode, TEdge, TEdgeRoute> : DiagramHitRecord
        where TNode : class, IGraphNode
        where TEdge : class, IGraphEdge<TNode, TEdgeRoute>
        where TEdgeRoute : class, IEdgeRoute
    {
        /// <summary>
        /// Constructor (nothing hit)</summary>
        public GraphHitRecord()
        {
        }

        /// <summary>
        /// Constructor (node hit)</summary>
        /// <param name="node">Node that was hit</param>
        /// <param name="part">Node part that was hit</param>
        public GraphHitRecord(TNode node, object part)
        {
            Node = node;
            Item = node;
            Part = part;
        }

        /// <summary>
        /// Constructor (edge hit)</summary>
        /// <param name="edge">Edge that was hit</param>
        /// <param name="part">Edge part that was hit</param>
        public GraphHitRecord(TEdge edge, object part)
        {
            Edge = edge;
            Item = edge;
            Part = part;
        }

        /// <summary>
        /// Constructor (EdgeRoute hit)</summary>
        /// <param name="edgeRoute">EdgeRoute that was hit</param>
        /// <param name="part">EdgeRoute part that was hit</param>
        public GraphHitRecord(TEdgeRoute edgeRoute, object part)
        {
            Item = edgeRoute;
            FromRoute = edgeRoute;
            ToRoute = edgeRoute;
            Part = part;
        }

        /// <summary>
        /// Constructor (for complex node-edge hits)</summary>
        /// <param name="node">Node that was hit</param>
        /// <param name="edge">Edge that was hit</param>
        /// <param name="fromRoute">Out route that was hit</param>
        /// <param name="toRoute">In route that was hit</param>
        public GraphHitRecord(TNode node, TEdge edge, TEdgeRoute fromRoute, TEdgeRoute toRoute)
        {
            Node = node;
            Edge = edge;
            FromRoute = fromRoute;
            ToRoute = toRoute;

            // prefer nodes to edges.  We don't want to select wires that are behind nodes
            if (Node != null)
            {
                Item = Node;

                if (FromRoute != null)
                    Part = FromRoute;
                else if (ToRoute != null)
                    Part = ToRoute;
            }
            else if (Edge != null)
            {
                Item = Edge;
            }
        }

        /// <summary>
        /// Gets or sets the full path of the hit circuit element, with the most nested item first</summary>
        /// <remarks>The path is inverted meaning that the lowest level element in the hierarchy is first
        /// and the highest level container (which is the Item property) is last.</remarks>
        public IEnumerable<TNode> HitPathInversed
        {
            get { return m_pathInversed; }
            set
            {
                m_pathInversed = value;
                if (value != null)
                    HitPath = new AdaptablePath<object>(HitPathInversed.Reverse());
                else
                    HitPath = null;

            }
        }

        /// <summary>
        /// Gets or sets the subnode that was hit</summary>
        /// <remarks>Subnodes are used to describe child objects in the hierarchical node.</remarks>
        public TNode SubNode 
        {
            get { return SubItem.As<TNode>(); }
        }

        /// <summary>
        /// The node that was hit or null</summary>
        public readonly TNode Node;

        /// <summary>
        /// The edge that was hit or null</summary>
        public readonly TEdge Edge;

        /// <summary>
        /// The edge route from a source node, or null if none</summary>
        public readonly TEdgeRoute FromRoute;

        /// <summary>
        /// The edge route to a destination node, or null if none</summary>
        public readonly TEdgeRoute ToRoute;

        /// <summary>
        ///  fromRoute position in client space</summary>
        public PointF FromRoutePos { get; set; }

        /// <summary>
        ///  toRoute position in client space</summary>
        public PointF ToRoutePos { get; set; }

        private IEnumerable<TNode> m_pathInversed;
    }
}
