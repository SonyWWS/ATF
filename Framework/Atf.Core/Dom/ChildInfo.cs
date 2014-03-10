//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Metadata for children, which are DOM fields holding a reference to another
    /// DOM node</summary>
    public class ChildInfo : FieldMetadata
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Field name</param>
        /// <param name="type">Field type</param>
        public ChildInfo(
            string name,
            DomNodeType type)
            : this(name, type, false)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Field name</param>
        /// <param name="type">Field type</param>
        /// <param name="isList">Whether there is 1 child, or a list of children</param>
        public ChildInfo(
            string name,
            DomNodeType type,
            bool isList)
            : base(name)
        {
            m_type = type;
            m_isList = isList;
        }

        /// <summary>
        /// Gets the field type</summary>
        public DomNodeType Type
        {
            get { return m_type; }
        }

        /// <summary>
        /// Whether there is 1 child, or a list of children</summary>
        public bool IsList
        {
            get { return m_isList; }
        }


        /// <summary>
        /// Gets or sets the child rules for this type, which constrain the allowable
        /// values</summary>
        public IEnumerable<ChildRule> Rules
        {
            get
            {
                if (m_rules != null)
                    return m_rules;

                return EmptyEnumerable<ChildRule>.Instance;
            }
        }

        /// <summary>
        /// Adds a rule to the child type. All the rules' Validate methods must return true
        /// in order for this ChildInfo's Validate to return true.</summary>
        /// <param name="rule">Rule, constraining the child</param>
        public void AddRule(ChildRule rule)
        {
            if (rule == null)
                throw new ArgumentNullException("rule");

            if (m_rules == null)
                m_rules = new List<ChildRule>(1);
            m_rules.Add(rule);
        }

        /// <summary>
        /// Checks whether a child node is valid (i.e., passes all the rules) for a given parent</summary>
        /// <param name="parent">Parent DOM node</param>
        /// <param name="child">Child DOM node</param>
        /// <returns>True, if the child meets the validation rules of the parent, or if the parent has no
        /// validation rules</returns>
        public virtual bool Validate(DomNode parent, DomNode child)
        {
            if (m_rules != null)
            {
                foreach (ChildRule rule in m_rules)
                    if (!rule.Validate(parent, child, this))
                        return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a value indicating if other child metadata is equivalent to this</summary>
        /// <param name="other">Other child metadata</param>
        /// <returns>True, iff other child metadata is equivalent to this</returns>
        public bool IsEquivalent(ChildInfo other)
        {
            return
                other != null &&
                Index == other.Index &&
                DefiningType == other.DefiningType;
        }

        /// <summary>
        /// Gets the parent metadata for this</summary>
        /// <returns>Parent metadata</returns>
        protected override NamedMetadata GetParent()
        {
            if (OwningType != null)
                return OwningType.BaseType.GetChildInfo(Index);

            return null;
        }

        private readonly DomNodeType m_type;
        private List<ChildRule> m_rules;
        private readonly bool m_isList;
    }
}
