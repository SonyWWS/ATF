//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;

namespace DomTreeEditorSample
{
    /// <summary>
    /// Class to represent an empty (null) reference, or "slot"; used by UITreeView
    /// and UIEditingContext</summary>
    public class EmptyRef
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="parent">Parent, with no reference child</param>
        /// <param name="childInfo">Information about reference child</param>
        public EmptyRef(DomNode parent, ChildInfo childInfo)
        {
            Parent = parent;
            ChildInfo = childInfo;
        }
        /// <summary>
        /// Parent, with no reference child</summary>
        public readonly DomNode Parent;

        /// <summary>
        /// Information about reference child</summary>
        public readonly ChildInfo ChildInfo;
    }
}
