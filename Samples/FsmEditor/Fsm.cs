//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Adaptable;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace FsmEditorSample
{
    /// <summary>
    /// Adapts the DOM root node to a finite state machine and a routed graph, for display in AdaptableControl</summary>
    public class Fsm : DomNodeAdapter, IGraph<State, Transition, NumberedRoute>, IAnnotatedDiagram
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the finite state machine's DomNode</summary>
        protected override void OnNodeSet()
        {
            m_states = new DomNodeListAdapter<State>(DomNode, Schema.fsmType.stateChild);
            m_transitions = new DomNodeListAdapter<Transition>(DomNode, Schema.fsmType.transitionChild);
            m_annotations = new DomNodeListAdapter<Annotation>(DomNode, Schema.fsmType.annotationChild);
        }

        /// <summary>
        /// Gets a list of all States in the FSM</summary>
        public IList<State> States
        {
            get { return m_states; }
        }

        /// <summary>
        /// Gets a list of all Transitions in the FSM</summary>
        public IList<Transition> Transitions
        {
            get { return m_transitions; }
        }

        /// <summary>
        /// Gets a list of all Annotations in the FSM</summary>
        public IList<Annotation> Annotations
        {
            get { return m_annotations; }
        }

        #region IGraph<State, Transition, NumberedRoute> Members

        /// <summary>
        /// Gets the States in the FSM</summary>
        IEnumerable<State> IGraph<State, Transition, NumberedRoute>.Nodes
        {
            get { return States; }
        }

        /// <summary>
        /// Gets the Transitions in the graph</summary>
        IEnumerable<Transition> IGraph<State, Transition, NumberedRoute>.Edges
        {
            get { return Transitions; }
        }

        #endregion

        #region IAnnotatedDiagram Members

        /// <summary>
        /// Gets the sequence of annotations in the context</summary>
        IEnumerable<IAnnotation> IAnnotatedDiagram.Annotations
        {
            get { return Adapters.AsIEnumerable<IAnnotation>(Annotations); }
        }

        #endregion

        private DomNodeListAdapter<State> m_states;
        private DomNodeListAdapter<Transition> m_transitions;
        private DomNodeListAdapter<Annotation> m_annotations;
    }
}
