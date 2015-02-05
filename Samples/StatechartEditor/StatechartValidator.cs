//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace StatechartEditorSample
{
    /// <summary>
    /// DomNode adapter to track changes to the statechart and enforce constraints on
    /// states and transitions</summary>
    public class StatechartValidator : DomNodeAdapter
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the diagram annotation's DomNode.
        /// Raises the DomNodeAdapter NodeSet event and performs custom processing.</summary>
        protected override void OnNodeSet()
        {
            DomNode.ChildInserting += DomNode_ChildInserting;

            base.OnNodeSet();
        }

        private void DomNode_ChildInserting(object sender, ChildEventArgs e)
        {
            // check pseudo-state constraints
            StateBase state = e.Child.As<StateBase>();
            if (state != null && state.IsPseudoState)
            {
                Statechart statechart = e.Parent.As<Statechart>();
                CheckUniqueness(statechart, state.Type);
            }
            else
            {
                // check state transition constraints
                Transition transition = e.Child.As<Transition>();
                if (transition != null)
                {
                    if (transition.FromState.IsPseudoState)
                    {
                        if (transition.FromState.Type == StateType.Final)
                        {
                            throw new InvalidTransactionException(
                                "Can't have a transition from the final state".Localize());
                        }
                    }
                    if (transition.ToState.IsPseudoState)
                    {
                        if (transition.ToState.Type == StateType.Start)
                        {
                            throw new InvalidTransactionException(
                                "Can't have a transition to the start state".Localize());
                        }
                    }
                }
            }
        }

        private void CheckUniqueness(Statechart statechart, StateType type)
        {
            foreach (StateBase state in statechart.States)
            {
                if (state.Type == type)
                {
                    string typeName = Enum.GetName(typeof(StateType), type);
                    throw new InvalidTransactionException(
                        "Each statechart cannot have more than one psuedo-state of type: ".Localize() +
                        typeName);
                }
            }
        }
    }
}
