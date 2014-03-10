//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;

namespace StatechartEditorSample
{
    /// <summary>
    /// Adapts DomNodes to a reactions, which are contained in states. Also serves as the
    /// base class for Transition.</summary>
    public class Reaction : DomNodeAdapter
    {
        /// <summary>
        /// Gets and sets the event text</summary>
        public string Event
        {
            get { return (string)DomNode.GetAttribute(Schema.reactionType.eventAttribute); }
            set { DomNode.SetAttribute(Schema.reactionType.eventAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the guard text</summary>
        public string Guard
        {
            get { return (string)DomNode.GetAttribute(Schema.reactionType.guardAttribute); }
            set { DomNode.SetAttribute(Schema.reactionType.guardAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the action text</summary>
        public string Action
        {
            get { return (string)DomNode.GetAttribute(Schema.reactionType.actionAttribute); }
            set { DomNode.SetAttribute(Schema.reactionType.actionAttribute, value); }
        }

        /// <summary>
        /// Converts reaction to standard string form</summary>
        /// <returns>Standard string form of reaction</returns>
        public override string ToString()
        {
            string _event = Event;
            string guard = Guard;
            string action = Action;

            string result = string.Empty;
            if (!string.IsNullOrEmpty(_event))
                result = _event;
            if (!string.IsNullOrEmpty(guard))
                result += '[' + guard + ']';
            if (!string.IsNullOrEmpty(action))
                result += '/' + action;

            return result;
        }
    }
}
