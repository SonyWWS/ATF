//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Event argument for when a DomNode's attribute is changing or has changed</summary>
    /// <remarks>This event is raised only if the attribute really changes, as determined
    /// by AttributeType.AreEqual(). Array types are compared element-by-element. The OldValue
    /// field will be the value that would have previously been returned by DomNode.GetAttribute().
    /// Note that if an attribute is at the default value but then is set to be a value that is
    /// equal to the default value, this event is raised and OldValue and NewValue will be equal.</remarks>
    public class AttributeEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="node">DomNode whose attribute has or will change</param>
        /// <param name="attributeInfo">Attribute metadata for the changing or changed attribute</param>
        /// <param name="oldValue">Previous value, as determined by DomNode.GetAttribute(attributeInfo)</param>
        /// <param name="newValue">New value</param>
        public AttributeEventArgs(
            DomNode node,
            AttributeInfo attributeInfo,
            object oldValue,
            object newValue)
        {
            DomNode = node;
            AttributeInfo = attributeInfo;
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// DomNode whose attribute has or will change</summary>
        public readonly DomNode DomNode;

        /// <summary>
        /// Attribute metadata for the changing or changed attribute</summary>
        public readonly AttributeInfo AttributeInfo;

        /// <summary>
        /// Previous value of the attribute as determined by DomNode.GetAttribute(AttributeInfo)</summary>
        public readonly object OldValue;

        /// <summary>
        /// New value of the attribute that will be returned by DomNode.GetAttribute(AttributeInfo)</summary>
        public readonly object NewValue;
    }
}
