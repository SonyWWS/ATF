//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace FsmEditorSample
{
    /// <summary>
    /// Adapts DomNode to a transition in FSMs</summary>
    public class Transition : DomNodeAdapter, IGraphEdge<State, NumberedRoute>
    {
        /// <summary>
        /// Gets the from state of the transition</summary>
        public State FromState
        {
            get { return GetReference<State>(Schema.transitionType.sourceAttribute); }
            set { SetReference(Schema.transitionType.sourceAttribute, value); }
        }

        /// <summary>
        /// Gets the to state of the transition</summary>
        public State ToState
        {
            get { return GetReference<State>(Schema.transitionType.destinationAttribute); }
            set { SetReference(Schema.transitionType.destinationAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the label on the transition</summary>
        public string Label
        {
            get { return GetAttribute<string>(Schema.transitionType.labelAttribute); }
            set { SetAttribute(Schema.transitionType.labelAttribute, value); }
        }

        /// <summary>
        /// Gets route index of transition</summary>
        public int Route
        {
            get { return m_route.Index; }
            set { m_route.Index = value; }
        }
        private NumberedRoute m_route = new NumberedRoute();

        #region IGraphEdge<State, NumberedRoute> and IGraphEdge<State, NumberedRoute> Members

        /// <summary>
        /// Gets the source node</summary>
        State IGraphEdge<State>.FromNode
        {
            get { return FromState; }
        }

        /// <summary>
        /// Gets the route taken from the source node</summary>
        NumberedRoute IGraphEdge<State, NumberedRoute>.FromRoute
        {
            get { return m_route; }
        }

        /// <summary>
        /// Gets the destination node</summary>
        State IGraphEdge<State>.ToNode
        {
            get { return ToState; }
        }

        /// <summary>
        /// Gets the route taken to the destination node</summary>
        NumberedRoute IGraphEdge<State, NumberedRoute>.ToRoute
        {
            get { return m_route; }
        }

        /// <summary>
        /// Gets the label on the transition</summary>
        string IGraphEdge<State>.Label
        {
            get { return Label; }
        }

        #endregion
    }
}
