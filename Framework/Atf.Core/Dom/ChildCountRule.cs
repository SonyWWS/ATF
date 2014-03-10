//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// A rule that constrains the number of children (associated with a particular ChildInfo)
    /// of a DomNode</summary>
    public class ChildCountRule : ChildRule
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="min">Minimum allowed # of children</param>
        /// <param name="max">Maximum allowed # of children</param>
        public ChildCountRule(int min, int max)
        {
            m_min = min;
            m_max = max;
        }

        /// <summary>
        /// Gets the minimum # of children</summary>
        public int Min
        {
            get { return m_min; }
        }

        /// <summary>
        /// Gets the maximum # of children</summary>
        public int Max
        {
            get { return m_max; }
        }

        /// <summary>
        /// Checks that the parent DomNode has the correct # of children of the given type</summary>
        /// <param name="parent">Parent DOM node</param>
        /// <param name="child">Child DOM node; ignored</param>
        /// <param name="childInfo">Child relationship info</param>
        /// <returns>True, iff 'parent' has a valid number of children of the type associated
        /// with 'childInfo'</returns>
        public override bool Validate(DomNode parent, DomNode child, ChildInfo childInfo)
        {
            if (childInfo.IsList)
            {
                IList<DomNode> childList = parent.GetChildList(childInfo);
                int count = childList.Count;
                return
                    count >= m_min &&
                    count <= m_max;
            }
            // singleton child references can always be set
            return true;
        }

        private readonly int m_min;
        private readonly int m_max;
    }
}
