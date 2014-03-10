//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;

namespace FsmEditorSample
{
    /// <summary>
    /// Adapter that tracks changes to transitions and updates their routing during validation</summary>
    public class TransitionRouter : Validator
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the TransitionRouter's DomNode.
        /// Raises the Observer NodeSet event and performs custom processing.</summary>
        protected override void OnNodeSet()
        {
            m_fsm = DomNode.Cast<Fsm>();

            // subscribe to DOM change events to invalidate transition routing on any
            //  change to transitions.
            DomNode.AttributeChanged += new EventHandler<AttributeEventArgs>(DomNode_AttributeChanged);
            DomNode.ChildInserted += new EventHandler<ChildEventArgs>(DomNode_ChildInserted);
            DomNode.ChildRemoved += new EventHandler<ChildEventArgs>(DomNode_ChildRemoved);

            // get initial routing
            RouteTransitions();

            base.OnNodeSet();
        }

        private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
        {
            if (e.DomNode.Type == Schema.transitionType.Type)
            {
                Transition transition = e.DomNode.As<Transition>();
                if (e.AttributeInfo.Equivalent(Schema.transitionType.sourceAttribute))
                {
                    m_routingInvalid = true;
                }
                else if (e.AttributeInfo.Equivalent(Schema.transitionType.destinationAttribute))
                {
                    m_routingInvalid = true;
                }
            }
        }

        private void DomNode_ChildInserted(object sender, ChildEventArgs e)
        {
            if (e.Child.Type == Schema.transitionType.Type)
            {
                m_routingInvalid = true;
            }
        }

        private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
        {
            if (e.Child.Type == Schema.transitionType.Type)
            {
                m_routingInvalid = true;
            }
        }

        /// <summary>
        /// Routes transitions by grouping them by state pairs, then assigning an index
        /// in the order they're encountered</summary>
        private void RouteTransitions()
        {
            Dictionary<Pair<State, State>, int> routesByPair = new Dictionary<Pair<State, State>, int>();
            foreach (Transition transition in m_fsm.Transitions)
            {
                Pair<State, State> states = new Pair<State, State>(transition.FromState, transition.ToState);
                int routes;
                if (!routesByPair.TryGetValue(states, out routes))
                {
                    // start routes at 0 if there are no connections in the opposite direction,
                    //  otherwise, start at 1
                    if (routesByPair.ContainsKey(new Pair<State, State>(transition.ToState, transition.FromState)))
                        routes = 1;
                    routesByPair.Add(states, routes);
                }

                transition.Route = routes++;
                routesByPair[states] = routes;
            }
        }

        /// <summary>
        /// Raises the OnEnded event and performs custom processing</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">EventArgs containing event data</param>
        protected override void OnEnded(object sender, EventArgs e)
        {
            if (m_routingInvalid)
            {
                m_routingInvalid = false;
                RouteTransitions();
            }

            base.OnEnded(sender, e);
        }

        /// <summary>
        /// Raises the Cancelled event and performs custom processing</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">EventArgs containing event data</param>
        protected override void OnCancelled(object sender, EventArgs e)
        {
            m_routingInvalid = false;

            base.OnCancelled(sender, e);
        }

        private Fsm m_fsm;
        private bool m_routingInvalid;
    }
}
