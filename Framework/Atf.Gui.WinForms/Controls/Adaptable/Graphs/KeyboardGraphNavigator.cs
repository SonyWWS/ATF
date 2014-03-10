//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Allows for navigating a graph using the arrow keys</summary>
    /// <typeparam name="TNode">Node type, must implement IGraphNode</typeparam>
    /// <typeparam name="TEdge">Edge type, must implement IGraphEdge</typeparam>
    /// <typeparam name="TEdgeRoute">Edge route type, must implement IEdgeRoute</typeparam>
    /// <remarks>Requires that the adaptable control's context be adaptable to ISelectionContext
    /// and IGraph. Optionally, this context should be adaptable to IViewingContext.
    /// Requires that the adaptable control can be adapted to IPickingAdapter2.
    /// Consider using KeyboardIOGraphNavigator instead, if the graph has inputs and outputs on specific
    /// sides of the nodes.</remarks>
    public class KeyboardGraphNavigator<TNode, TEdge, TEdgeRoute> : ControlAdapter
        where TNode : class, IGraphNode
        where TEdge : class, IGraphEdge<TNode, TEdgeRoute>
        where TEdgeRoute : class, IEdgeRoute
    {
        /// <summary>
        /// Binds the adapter to the adaptable control; called in the order that the adapters
        /// were defined on the control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Bind(AdaptableControl control)
        {
            control.ContextChanged += control_ContextChanged;
            control.PreviewKeyDown += control_PreviewKeyDown;

            base.Bind(control);
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            control.ContextChanged -= control_ContextChanged;
            control.PreviewKeyDown -= control_PreviewKeyDown;

            base.Unbind(control);
        }

        /// <summary>
        /// Finds the nearest node to a starting node, in the desired direction</summary>
        /// <param name="startNode">Node to measure distance from</param>
        /// <param name="arrow">Direction (Keys.Up, Keys.Right, Keys.Down, or Keys.Left)</param>
        /// <param name="nearestRect">Nearest node's rectangle, or an empty rectangle if no
        /// nearest node is found</param>
        /// <returns>The nearest node or null if there is none in that direction</returns>
        protected virtual TNode FindNearestElement(TNode startNode, Keys arrow, out Rectangle nearestRect)
        {
            TNode nearest = null;
            int bestDist = int.MaxValue;
            var pickingAdapter = AdaptedControl.Cast<IPickingAdapter2>();
            nearestRect = new Rectangle();

            Rectangle startRect = pickingAdapter.GetBounds(new[] { startNode });
            foreach (TNode node in m_graph.Nodes)
            {
                if (node != startNode)
                {
                    Rectangle targetRect = pickingAdapter.GetBounds(new[] { node });
                    int dist = WinFormsUtil.CalculateDistance(startRect, arrow, targetRect);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        nearest = node;
                        nearestRect = targetRect;
                    }
                }
            }

            return nearest;
        }

        private void control_ContextChanged(object sender, EventArgs e)
        {
            // mandatory
            m_selectionContext = AdaptedControl.ContextCast<ISelectionContext>();
            m_graph = AdaptedControl.ContextCast<IGraph<TNode, TEdge, TEdgeRoute>>();
        }

        private void control_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            Keys keyData = e.KeyData;
            Keys modifiers = keyData & Keys.Modifiers;
            keyData &= ~Keys.Modifiers;

            if (keyData == Keys.Up ||
                keyData == Keys.Right ||
                keyData == Keys.Down ||
                keyData == Keys.Left)
            {
                TNode startElement = Adapters.As<TNode>(m_selectionContext.LastSelected);
                if (startElement != null)
                {
                    Rectangle nearestRect;
                    TNode nearest = FindNearestElement(startElement, keyData, out nearestRect);
                    if (nearest != null)
                    {
                        var selection = new List<TNode>(m_selectionContext.SelectionCount);
                        selection.AddRange(m_selectionContext.GetSelection<TNode>());
                        KeysUtil.Select<TNode>(selection, nearest, modifiers);
                        m_selectionContext.Selection = selection.Cast<object>();
                        var transformAdapter = AdaptedControl.As<ITransformAdapter>();
                        if (transformAdapter != null)
                            transformAdapter.PanToRect(nearestRect);
                    }
                }
            }
        }

        private ISelectionContext m_selectionContext;
        private IGraph<TNode, TEdge, TEdgeRoute> m_graph;
    }
}
