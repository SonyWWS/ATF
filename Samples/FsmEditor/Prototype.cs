//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Dom;

namespace FsmEditorSample
{
    /// <summary>
    /// Adapts DomNode to a FSM prototype, which contains states and transitions
    /// that can be instanced (cloned) in an FSM document</summary>
    public class Prototype : DomNodeAdapter
    {
        /// <summary>
        /// Gets and sets the prototype name</summary>
        public string Name
        {
            get { return (string)DomNode.GetAttribute(Schema.prototypeType.nameAttribute); }
            set { DomNode.SetAttribute(Schema.prototypeType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets the States in the prototype</summary>
        public IList<State> States
        {
            get { return GetChildList<State>(Schema.prototypeType.stateChild); }
        }

        /// <summary>
        /// Gets the Transitions in the prototype</summary>
        public IList<Transition> Transitions
        {
            get { return GetChildList<Transition>(Schema.prototypeType.transitionChild); }
        }
    }
}
