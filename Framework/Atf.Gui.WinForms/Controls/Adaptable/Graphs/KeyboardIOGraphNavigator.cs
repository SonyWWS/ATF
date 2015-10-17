//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Allows for navigating an "input-output" graph using the arrow keys. This kind of
    /// graph has inputs on only one side of a node and outputs on another side.</summary>
    /// <typeparam name="TNode">Node type, must implement IGraphNode</typeparam>
    /// <typeparam name="TEdge">Edge type, must implement IGraphEdge</typeparam>
    /// <typeparam name="TEdgeRoute">Edge route type, must implement IEdgeRoute</typeparam>
    /// <remarks>Requires that the adaptable control's context be adaptable to ISelectionContext
    /// and IGraph. Optionally, this context should be adaptable to IViewingContext.
    /// Requires that the adaptable control can be adapted to IPickingAdapter2.
    /// See also KeyboardGraphNavigator.</remarks>
    public class KeyboardIOGraphNavigator<TNode, TEdge, TEdgeRoute> : ControlAdapter
        where TNode : class, IGraphNode
        where TEdge : class, IGraphEdge<TNode, TEdgeRoute>
        where TEdgeRoute : class, IEdgeRoute
    {
        /// <summary>
        /// Returns whether the given key should navigate to input nodes. By default, the input nodes
        /// are assumed to connect on the left side of the current node, and outputs are assumed to
        /// connect on the right side.</summary>
        /// <param name="key">Key press, without any modifiers</param>
        /// <returns>Whether the given key should navigate to input nodes</returns>
        protected virtual bool IsInputNavigationKey(Keys key)
        {
            return key == Keys.Left;
        }

        /// <summary>
        /// Returns whether the given key should navigate to output nodes. By default, the input nodes
        /// are assumed to connect on the left side of the current node, and outputs are assumed to
        /// connect on the right side.</summary>
        /// <param name="key">Key press, without any modifiers</param>
        /// <returns>Whether the given key should navigate to output nodes</returns>
        protected virtual bool IsOutputNavigationKey(Keys key)
        {
            return key == Keys.Right;
        }

        /// <summary>
        /// Returns whether the given key should navigate to other nodes</summary>
        /// <param name="key">Key press, without any modifiers</param>
        /// <returns>Whether the given key should navigate to other nodes</returns>
        protected virtual bool IsNavigationKey(Keys key)
        {
            return
                key == Keys.Up ||
                key == Keys.Right ||
                key == Keys.Down ||
                key == Keys.Left;
        }

        /// <summary>
        /// Returns whether or not these two keys represent opposite directions, for purposes of
        /// knowing if Shift+selection should be undone. The keys do not have modifiers (Shift,
        /// Control, or Alt).</summary>
        /// <param name="a">First key</param>
        /// <param name="b">Second key</param>
        /// <returns><c>True</c> if two keys represent opposite directions</returns>
        protected virtual bool OppositeNavigationKeys(Keys a, Keys b)
        {
            return
                (IsOutputNavigationKey(a) && IsInputNavigationKey(b)) ||
                (IsInputNavigationKey(a) && IsOutputNavigationKey(b)) ||
                (a == Keys.Up && b == Keys.Down) ||
                (a == Keys.Down && b == Keys.Up);
        }

        /// <summary>
        /// Binds the adapter to the adaptable control; called in the order that the adapters
        /// were defined on the control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Bind(AdaptableControl control)
        {
            base.Bind(control);

            control.ContextChanged += control_ContextChanged;
            control.PreviewKeyDown += PreviewKeyDown;
            m_pickingAdapter = control.Cast<IPickingAdapter2>();
            m_transformAdapter = control.As<ITransformAdapter>(); //optional
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            control.ContextChanged -= control_ContextChanged;
            control.PreviewKeyDown -= PreviewKeyDown;
            m_pickingAdapter = null;
            m_transformAdapter = null;
            m_shiftKeySelectionStack.Clear();
            m_lastKeyWithShift = Keys.None;
            
            base.Unbind(control);
        }

        /// <summary>
        /// Finds the nearest node to a starting node, in the desired direction</summary>
        /// <param name="startNode">Node to measure distance from</param>
        /// <param name="key">Key press (e.g., Keys.Up, Keys.Right, Keys.Down, or Keys.Left),
        /// without modifier keys</param>
        /// <param name="modifiers">Modifier keys that were pressed</param>
        /// <returns>The nearest node or null if there is none in that direction</returns>
        protected virtual TNode FindNearestNode(TNode startNode, Keys key, Keys modifiers)
        {
            TNode nearest = null;
            int bestDist = int.MaxValue;

            Rectangle startRect = m_pickingAdapter.GetBounds(new[] { startNode });
            foreach (TNode node in m_graph.Nodes)
            {
                if (node != startNode)
                {
                    Rectangle targetRect = m_pickingAdapter.GetBounds(new[] { node });
                    int dist = WinFormsUtil.CalculateDistance(startRect, key, targetRect);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        nearest = node;
                    }
                }
            }

            return nearest;
        }

        /// <summary>
        /// Finds the connected nodes to a starting node, given the key press. Only searches for
        /// nodes connected to starting node via an edge.</summary>
        /// <param name="startNode">Starting node</param>
        /// <param name="key">Key press (e.g., Keys.Up, Keys.Right, Keys.Down, or Keys.Left),
        /// without modifier keys</param>
        /// <param name="modifiers">Modifier keys that were pressed</param>
        /// <returns>Connected nodes to starting node</returns>
        protected virtual IEnumerable<TNode> FindConnectedNodes(TNode startNode, Keys key, Keys modifiers)
        {
            if (IsOutputNavigationKey(key))
            {
                foreach (TNode outNode in m_graph.GetOutputNodes(startNode))
                    yield return outNode;
            }
            else if (IsInputNavigationKey(key))
            {
                foreach (TNode inNode in m_graph.GetInputNodes(startNode))
                    yield return inNode;
            }
        }

        /// <summary>
        /// Handles the PreviewKeyDown event on AdaptedControl, and changes the selection
        /// if the user navigates using the keyboard</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        protected virtual void PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            Keys keyData = e.KeyData;
            Keys modifiers = keyData & Keys.Modifiers;
            keyData &= ~Keys.Modifiers;

            if (IsNavigationKey(keyData))
            {
                List<TNode> oldSelection;

                // If the user is reversing direction of a past shift+selection, and they didn't
                //  overshoot the beginning (i.e., the stack isn't empty), then don't search for
                //  nodes and instead restore a past selection.
                if (modifiers == Keys.Shift &&
                    OppositeNavigationKeys(m_lastKeyWithShift, keyData) &&
                    m_shiftKeySelectionStack.Count > 0)
                {
                    oldSelection = m_shiftKeySelectionStack.Pop();
                    ChangeSelection(oldSelection);
                    return;
                }

                // We need to know the current selection now.
                oldSelection = new List<TNode>(m_selectionContext.SelectionCount);
                oldSelection.AddRange(m_selectionContext.GetSelection<TNode>());

                // If the user is doing a shift+selection, then push the current selection on to the
                //  stack. But if the user chose a new direction, clear the stack first.
                bool pushOldSelectionIfSelectionChanges = false;
                if (modifiers == Keys.Shift)
                {
                    if (m_lastKeyWithShift != Keys.None && m_lastKeyWithShift != keyData)
                    {
                        m_lastKeyWithShift = Keys.None;
                        m_shiftKeySelectionStack.Clear();
                    }
                    pushOldSelectionIfSelectionChanges = true;
                }
                else
                {
                    m_lastKeyWithShift = Keys.None;
                    m_shiftKeySelectionStack.Clear();
                }

                // Do a search from the existing selected nodes, looking only along wires.
                var connectedNodes = new HashSet<TNode>();
                foreach (TNode startElement in m_selectionContext.GetSelection<TNode>())
                {
                    foreach (TNode connectedNode in FindConnectedNodes(startElement, keyData, modifiers))
                        connectedNodes.Add(connectedNode);
                }

                // If no connected nodes were found, then look for the nearest nodes.
                if (connectedNodes.Count == 0)
                {
                    foreach (TNode startElement in m_selectionContext.GetSelection<TNode>())
                    {
                        TNode nearestNode = FindNearestNode(startElement, keyData, modifiers);
                        if (nearestNode != null)
                            connectedNodes.Add(nearestNode);
                    }
                }

                // If we have new connected nodes then change the selection.
                if (connectedNodes.Count > 0)
                {
                    // With this keyboard navigation, the Control key doesn't seem to make sense
                    //  for toggling nodes in the selection.
                    modifiers &= ~Keys.Control;

                    var newSelection = new HashSet<TNode>(oldSelection);
                    KeysUtil.Select(newSelection, connectedNodes, modifiers);

                    if (!newSelection.SetEquals(oldSelection))
                    {
                        ChangeSelection(newSelection);

                        if (pushOldSelectionIfSelectionChanges)
                        {
                            m_lastKeyWithShift = keyData;
                            m_shiftKeySelectionStack.Push(new List<TNode>(oldSelection));
                        }

                        // Attempt to pan, to make sure the newly selected nodes are visible
                        if (m_transformAdapter != null)
                        {
                            Rectangle boundingRect = m_pickingAdapter.GetBounds(newSelection.OfType<object>());
                            m_transformAdapter.PanToRect(boundingRect);
                        }
                    }
                }
            }
        }

        private void ChangeSelection(IEnumerable<TNode> selection)
        {
            try
            {
                m_changingSelection = true;
                m_selectionContext.Selection = selection.Cast<object>();
            }
            finally
            {
                m_changingSelection = false;
            }
        }

        private void control_ContextChanged(object sender, EventArgs e)
        {
            if (m_selectionContext != null)
                m_selectionContext.SelectionChanged -= m_selectionContext_SelectionChanged;

            // mandatory
            m_selectionContext = AdaptedControl.ContextCast<ISelectionContext>();
            m_selectionContext.SelectionChanged += m_selectionContext_SelectionChanged;
            m_graph = AdaptedControl.ContextCast<IGraph<TNode, TEdge, TEdgeRoute>>();
        }

        private void m_selectionContext_SelectionChanged(object sender, EventArgs args)
        {
            if (!m_changingSelection)
            {
                m_shiftKeySelectionStack.Clear();
                m_lastKeyWithShift = Keys.None;
            }
        }

        private ISelectionContext m_selectionContext; //mandatory
        private IGraph<TNode, TEdge, TEdgeRoute> m_graph; //mandatory
        private IPickingAdapter2 m_pickingAdapter; //mandatory
        private ITransformAdapter m_transformAdapter; //optional

        // To remember how to unwind the selection as the user is holding the shift key
        //  and using the input and output navigation keys (e.g., left and right arrow keys).
        private Stack<List<TNode>> m_shiftKeySelectionStack = new Stack<List<TNode>>();
        private Keys m_lastKeyWithShift = Keys.None;
        private bool m_changingSelection;
    }
}
