//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Event argument for child node events</summary>
    public class ChildEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="parent">Node's parent</param>
        /// <param name="childInfo">Metadata for child</param>
        /// <param name="child">Child node</param>
        /// <param name="index">Node's index in parent</param>
        public ChildEventArgs(
            DomNode parent,
            ChildInfo childInfo,
            DomNode child,
            int index)
        {
            Parent = parent;
            ChildInfo = childInfo;
            Child = child;
            Index = index;
        }

        /// <summary>
        /// Node's parent</summary>
        public readonly DomNode Parent;
        /// <summary>
        /// Metadata for child</summary>
        public readonly ChildInfo ChildInfo;
        /// <summary>
        /// Child node</summary>
        public readonly DomNode Child;
        /// <summary>
        /// Node's index in parent</summary>
        public readonly int Index;
    }
}
