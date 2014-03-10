//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Event data for when a reference to a DomNode changes to a different DomNode</summary>
    public class ReferenceEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="owner">DomNode that has an attribute that is a reference to a target DomNode</param>
        /// <param name="attributeInfo">Metadata indicating the owner's attribute that is a reference</param>
        /// <param name="target">DomNode referred to by the reference. Also known as a referent.</param>
        public ReferenceEventArgs(DomNode owner, AttributeInfo attributeInfo, DomNode target)
        {
            Owner = owner;
            AttributeInfo = attributeInfo;
            Target = target;
        }

        /// <summary>
        /// DomNode that has an attribute that is a reference to a target DomNode</summary>
        public readonly DomNode Owner;

        /// <summary>
        /// Metadata indicating the owner's attribute that is a reference</summary>
        public readonly AttributeInfo AttributeInfo;

        /// <summary>
        /// DomNode referred to by the reference. Also known as a referent.</summary>
        public readonly DomNode Target;
    }
}
