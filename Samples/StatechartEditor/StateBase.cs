//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace StatechartEditorSample
{
    /// <summary>
    /// Abstract base DomNode adapter for states and pseudo-states</summary>
    public abstract class StateBase : DomNodeAdapter, IState
    {
        /// <summary>
        /// Gets whether this is a pseudo-state</summary>
        public virtual bool IsPseudoState
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets the state position, which is at the state's upper-left corner</summary>
        public Point Position
        {
            // position is relative to parent state, if any
            get
            {
                return new Point(
                    (int)DomNode.GetAttribute(Schema.stateBaseType.xAttribute),
                    (int)DomNode.GetAttribute(Schema.stateBaseType.yAttribute));
            }
            set
            {
                DomNode.SetAttribute(Schema.stateBaseType.xAttribute, value.X );
                DomNode.SetAttribute(Schema.stateBaseType.yAttribute, value.Y);
            }
        }

        /// <summary>
        /// Gets or sets the state size</summary>
        public virtual Size Size
        {
            get { return D2dStatechartRenderer<StateBase, Transition>.PseudostateSize; }
            set { }
        }

        /// <summary>
        /// Gets or sets the state's parent statechart</summary>
        public Statechart Parent
        {
            get
            {
                DomNode parent = DomNode.Parent;
                return (parent != null) ? parent.As<Statechart>() : null;
            }
        }

        /// <summary>
        /// Gets or sets locked state; not backed by DOM data</summary>
        public bool Locked
        {
            get { return m_locked; }
            set { m_locked = value; }
        }

        /// <summary>
        /// Gets or sets the state's bounding rectangle</summary>
        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(Position, Size);
            }
            set
            {
                Position = value.Location;
                Size = value.Size;
            }
        }

        #region IState Members

        /// <summary>
        /// Gets the state type</summary>
        public abstract StateType Type
        {
            get;
        }

        /// <summary>
        /// Gets or sets the visual indicators on the state</summary>
        public StateIndicators Indicators
        {
            get { return m_indicators; }
            set { m_indicators = value; }
        }

        #endregion

        #region IGraphNode Members

        /// <summary>
        /// Gets the node name</summary>
        string IGraphNode.Name
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the bounding rectangle for the node in world space (or local space if a
        /// hierarchy is involved). The location portion should always
        /// be accurate, but the renderer should be queried for the size of the rectangle.
        /// See D2dGraphRenderer.GetBounds().</summary>
        Rectangle IGraphNode.Bounds
        {
            get { return Bounds; }
        }

        #endregion

        private StateIndicators m_indicators;
        private bool m_locked;
    }
}
