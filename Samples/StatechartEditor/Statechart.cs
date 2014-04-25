//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Drawing;

using Sce.Atf.Dom;
using Sce.Atf.Adaptation;

namespace StatechartEditorSample
{
    /// <summary>
    /// Adapts DomNode to a Statechart</summary>
    public class Statechart : DomNodeAdapter
    {
        /// <summary>
        /// Gets or sets the bounding rectangle for the statechart. Not backed by DOM.</summary>
        public Rectangle Bounds
        {
            get { return m_bounds; }
            set { m_bounds = value; }
        }

        /// <summary>
        /// Gets the statechart's parent state, null if this is the root statechart</summary>
        public State Parent
        {
            get
            {
                DomNode parent = DomNode.Parent;
                return (parent != null) ? parent.As<State>() : null;
            }
        }

        /// <summary>
        /// Gets the states in the statechart</summary>
        public IList<StateBase> States
        {
            get { return GetChildList<StateBase>(Schema.statechartType.stateChild); }
        }

        /// <summary>
        /// Gets the states in the statechart, and all sub-statecharts</summary>
        public IEnumerable<StateBase> AllStates
        {
            get
            {
                Queue<Statechart> statecharts = new Queue<Statechart>();
                statecharts.Enqueue(this);
                while (statecharts.Count > 0)
                {
                    Statechart statechart = statecharts.Dequeue();
                    foreach (StateBase state in statechart.States)
                    {
                        yield return state;

                        State complexState = state as State;
                        if (complexState != null)
                        {
                            foreach (Statechart subStatechart in complexState.Statecharts)
                                statecharts.Enqueue(subStatechart);
                        }
                    }
                }
            }
        }

        private Rectangle m_bounds;
    }
}
