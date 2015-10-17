//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Abstract base class for a rule that constrains parent-child relationships. A rule
    /// might constrain the number or type of children.</summary>
    public abstract class ChildRule
    {
        /// <summary>
        /// Checks that the parent's children are in a valid state</summary>
        /// <param name="parent">Parent DOM node, containing children</param>
        /// <param name="child">Child DOM node, to add or remove to/from parent</param>
        /// <param name="childInfo">Child relationship info</param>
        /// <returns><c>True</c> if the parent and child are in a valid state with respect to
        /// each other</returns>
        public abstract bool Validate(DomNode parent, DomNode child, ChildInfo childInfo);
    }
}
