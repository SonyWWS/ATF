//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Controls.Adaptable.Graphs;

namespace StatechartEditorSample
{
    /// <summary>
    /// Adapts DomNode to a transition between states</summary>
    public class Transition : Reaction, IGraphEdge<StateBase, BoundaryRoute>
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the diagram annotation's DomNode.
        /// Raises the DomNodeAdapter NodeSet event and performs custom processing.</summary>
        protected override void OnNodeSet()
        {
            m_fromRoute.Position = FromPosition;
            m_toRoute.Position = ToPosition;

            base.OnNodeSet();
        }

        /// <summary>
        /// Gets or sets the state at which this transition begins</summary>
        public StateBase FromState
        {
            get { return GetReference<StateBase>(Schema.transitionType.fromStateAttribute); }
            set { SetReference(Schema.transitionType.fromStateAttribute, value); }
        }

        /// <summary>
        /// Gets or sets route position on perimeter of state at which this transition begins</summary>
        public float FromPosition
        {
            get { return (float)DomNode.GetAttribute(Schema.transitionType.fromPositionAttribute); }
            set
            {
                m_fromRoute.Position = value;
                DomNode.SetAttribute(Schema.transitionType.fromPositionAttribute, value);
            }
        }

        /// <summary>
        /// Gets or sets the state at which this transition ends</summary>
        public StateBase ToState
        {
            get { return GetReference<StateBase>(Schema.transitionType.toStateAttribute); }
            set { SetReference(Schema.transitionType.toStateAttribute, value); }
        }

        /// <summary>
        /// Gets or sets route position on perimeter of state at which this transition ends</summary>
        public float ToPosition
        {
            get { return (float)DomNode.GetAttribute(Schema.transitionType.toPositionAttribute); }
            set
            {
                m_toRoute.Position = value;
                DomNode.SetAttribute(Schema.transitionType.toPositionAttribute, value);
            }
        }

        #region IGraphEdge<StateBase, BoundaryRoute> Members

        /// <summary>
        /// Gets transition's source state</summary>
        StateBase IGraphEdge<StateBase>.FromNode
        {
            get { return FromState; }
        }

        /// <summary>
        /// Gets the BoundaryRoute taken from the source state</summary>
        BoundaryRoute IGraphEdge<StateBase, BoundaryRoute>.FromRoute
        {
            get { return m_fromRoute; }
        }
        private BoundaryRoute m_fromRoute = new BoundaryRoute();

        /// <summary>
        /// Gets transition's destination state</summary>
        StateBase IGraphEdge<StateBase>.ToNode
        {
            get { return ToState; }
        }

        /// <summary>
        /// Gets the BoundaryRoute taken to the destination state</summary>
        BoundaryRoute IGraphEdge<StateBase, BoundaryRoute>.ToRoute
        {
            get { return m_toRoute; }
        }
        private BoundaryRoute m_toRoute = new BoundaryRoute();

        /// <summary>
        /// Gets transition's label</summary>
        string IGraphEdge<StateBase>.Label
        {
            get { return ToString(); }
        }

        #endregion
    }
}
