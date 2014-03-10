//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;

using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;

namespace StatechartEditorSample
{
    /// <summary>
    /// DomNode adapter that tracks changes to states and updates their bounds to be big
    /// enough to hold any child states and statecharts during validation.</summary>
    public class BoundsValidator : Validator
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the BoundsValidator's DomNode.
        /// Raises the Observer NodeSet event and performs custom processing.</summary>
        protected override void OnNodeSet()
        {
            m_statechart = DomNode.Cast<Statechart>();

            base.OnNodeSet();
        }

        // set by editor
        /// <summary>
        /// Gets or sets the statechart renderer</summary>
        public D2dStatechartRenderer<StateBase, Transition> StatechartRenderer
        {
            get { return m_statechartRenderer; }
            set
            {
                m_statechartRenderer = value;
                Layout(m_statechart);
            }
        }

        /// <summary>
        /// Raises the AttributeChanged event and performs custom processing</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">AttributeEventArgs containing event data</param>
        protected override void OnAttributeChanged(object sender, AttributeEventArgs e)
        {
            if (Validating)
                m_layoutInvalid = true;

            base.OnAttributeChanged(sender, e);
        }

        /// <summary>
        /// Raises the ChildInserted event and performs custom processing</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">ChildEventArgs containing event data</param>
        protected override void OnChildInserted(object sender, ChildEventArgs e)
        {
            if (Validating)
                m_layoutInvalid = true;

            base.OnChildInserted(sender, e);
        }

        /// <summary>
        /// Raises the ChildRemoved event and performs custom processing</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">ChildEventArgs containing event data</param>
        protected override void OnChildRemoved(object sender, ChildEventArgs e)
        {
            if (Validating)
                m_layoutInvalid = true;

            base.OnChildRemoved(sender, e);
        }

        /// <summary>
        /// Raises the Ending event and performs custom processing</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">EventArgs containing event data</param>
        protected override void OnEnding(object sender, EventArgs e)
        {
            // if any states changed, verify that parent states bounds
            //  contain their child states.
            if (m_layoutInvalid)
            {
                m_layoutInvalid = false;
                Layout(m_statechart);
            }

            base.OnEnding(sender, e);
        }

        /// <summary>
        /// Raises the Cancelled event and performs custom processing</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">EventArgs containing event data</param>
        protected override void OnCancelled(object sender, EventArgs e)
        {
            m_layoutInvalid = false;

            base.OnCancelled(sender, e);
        }

        private Rectangle Layout(Statechart statechart)
        {
            Rectangle bounds = new Rectangle();
            foreach (StateBase state in statechart.States)
            {
                Rectangle stateBounds = Layout(state);
                if (bounds.IsEmpty)
                {
                    bounds = stateBounds;
                }
                else
                {
                    bounds = Rectangle.Union(bounds, stateBounds);
                }
            }

            statechart.Bounds = bounds;

            return bounds;
        }

        private Rectangle Layout(StateBase stateBase)
        {
            Rectangle currentBounds = stateBase.Bounds;

            // return bounds if not a complex state (ie. if pseudo-state)
            State state = stateBase as State;
            if (state == null)
                return currentBounds;

            // if complex state, layout sub-statecharts and get the union of their bounds
            Rectangle newBounds = new Rectangle();
            foreach (Statechart statechart in state.Statecharts)
            {
                Rectangle statechartBounds = Layout(statechart);
                if (!statechartBounds.IsEmpty)
                {
                    if (newBounds.IsEmpty)
                        newBounds = statechartBounds;
                    else
                        newBounds = Rectangle.Union(newBounds, statechartBounds);
                }
            }

            // add margins for complex states (containing 1 or more statecharts)
            if (newBounds.IsEmpty)
            {
                newBounds = currentBounds;
            }
            //else
            //{
            //    int margin = 16;
            //    int topMargin = 26;
            //    newBounds.X -= margin;
            //    newBounds.Width += 2 * margin;

            //    int topMargin = margin + m_statechartRenderer.Theme.Font.Height;
            //    newBounds.Y -= topMargin;
            //    newBounds.Height += topMargin + margin;
            //}

            // minimum size constraint
            newBounds.Width = Math.Max(newBounds.Width, 64);
            newBounds.Height = Math.Max(newBounds.Height, 64);

            newBounds = Rectangle.Union(currentBounds, newBounds);
            state.Bounds = newBounds;

            return newBounds;
        }

        private Statechart m_statechart;
        private D2dStatechartRenderer<StateBase, Transition> m_statechartRenderer;
        private bool m_layoutInvalid;
    }
}
