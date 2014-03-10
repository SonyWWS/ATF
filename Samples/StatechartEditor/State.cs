//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;

using Sce.Atf.Controls.Adaptable.Graphs;

namespace StatechartEditorSample
{
    /// <summary>
    /// Adapts DomNode to a true state</summary>
    public class State : StateBase, IComplexState<StateBase, Transition>
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the diagram annotation's DomNode.
        /// Raises the StateBase NodeSet event and performs custom processing.</summary>
        protected override void OnNodeSet()
        {
            Size sz = this.Size;
            if (sz.Width == 0 || sz.Height == 0)
            {
                this.Size = new Size(64, 64);
            }
            base.OnNodeSet();
        }
        /// <summary>
        /// Gets the state type</summary>
        public override StateType Type
        {
            get { return StateType.Normal; }
        }

        /// <summary>
        /// Gets whether this is a pseudo-state</summary>
        public override bool IsPseudoState
        {
            get { return false; }
        }

        /// <summary>
        /// Gets or sets the state name</summary>
        public string Name
        {
            get { return (string)DomNode.GetAttribute(Schema.stateType.labelAttribute); }
            set { DomNode.SetAttribute(Schema.stateType.labelAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the state size</summary>
        public override Size Size
        {
            get
            {
                return new Size(
                    (int)DomNode.GetAttribute(Schema.stateType.widthAttribute),
                    (int)DomNode.GetAttribute(Schema.stateType.heightAttribute));
            }
            set
            {
                DomNode.SetAttribute(Schema.stateType.widthAttribute, value.Width);
                DomNode.SetAttribute(Schema.stateType.heightAttribute, value.Height);
            }
        }

        /// <summary>
        /// Gets or sets the state's entry action</summary>
        public string EntryAction
        {
            get { return (string)DomNode.GetAttribute(Schema.stateType.entryActionAttribute); }
            set { DomNode.SetAttribute(Schema.stateType.entryActionAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the state's exit action</summary>
        public string ExitAction
        {
            get { return (string)DomNode.GetAttribute(Schema.stateType.exitActionAttribute); }
            set { DomNode.SetAttribute(Schema.stateType.exitActionAttribute, value); }
        }

        /// <summary>
        /// Gets whether the state is simple</summary>
        public bool IsSimple
        {
            get { return DomNode.GetChildList(Schema.stateType.statechartChild).Count == 0; }
        }

        /// <summary>
        /// Gets the list of static reactions in the state</summary>
        public IList<Reaction> Reactions
        {
            get { return GetChildList<Reaction>(Schema.stateType.reactionChild); }
        }

        /// <summary>
        /// Gets a list of sub-statecharts in the state (AND and OR states only)</summary>
        public IList<Statechart> Statecharts
        {
            get { return GetChildList<Statechart>(Schema.stateType.statechartChild); }
        }

        /// <summary>
        /// Gets an enumeration of the substates in the state</summary>
        public IEnumerable<StateBase> SubStates
        {
            get
            {
                foreach (Statechart statechart in Statecharts)
                    foreach (StateBase subState in statechart.States)
                        yield return subState;
            }
        }

        #region IComplexState<StateBase, Transition> Members

        /// <summary>
        /// Gets the state's interior text</summary>
        public string Text
        {
            get { return GetStateText(); }
        }

        #endregion

        #region IHierarchicalGraphNode<StateBase, Transition, BoundaryRoute> Members

        /// <summary>
        /// Gets the sequence of nodes that are children of this hierarchical graph node</summary>
        IEnumerable<StateBase> IHierarchicalGraphNode<StateBase, Transition, BoundaryRoute>.SubNodes
        {
            get { return SubStates; }
        }

        #endregion

        private string GetStateText()
        {
            string result = string.Empty;
            string entryAction = EntryAction;
            if (!string.IsNullOrEmpty(entryAction))
                result = @"entry/" + entryAction + Environment.NewLine;
            string exitAction = ExitAction;
            if (!string.IsNullOrEmpty(exitAction))
                result += @"exit/" + exitAction + Environment.NewLine;

            foreach (Reaction reaction in Reactions)
                result += reaction.ToString() + Environment.NewLine;

            if (result.Length > 0)
                result = result.Substring(0, result.Length - 1);

            return result;
        }
    }
}
