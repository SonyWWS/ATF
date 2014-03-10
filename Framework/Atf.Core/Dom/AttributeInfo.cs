//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Metadata for attributes, which are DOM fields holding a primitive type or
    /// an array-of-primitive type. The primitive type is usually a .NET value type
    /// (such as int or float), but could also be a reference type in the case of URIs
    /// or references to DomNodes.</summary>
    public class AttributeInfo : FieldMetadata
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Attribute name</param>
        /// <param name="type">Attribute type</param>
        public AttributeInfo(string name, AttributeType type)
            : base(name)
        {
            m_type = type;
        }

        /// <summary>
        /// Gets whether this is Id attribute of owning type</summary>
        public bool IsIdAttribute
        {
            get
            {
                if (OwningType != null)
                    return Equivalent(OwningType.IdAttribute);
                return false;
            }
        }

        /// <summary>
        /// Gets the attribute type</summary>
        public AttributeType Type
        {
            get { return m_type; }
        }

        /// <summary>
        /// Gets or sets the attribute's default value</summary>
        public object DefaultValue
        {
            get
            {
                if (m_defaultValue != null)
                    return m_defaultValue;

                return m_type.GetDefault();
            }
            set
            {
                if (value != null &&
                    !m_type.ClrType.IsAssignableFrom(value.GetType()))
                {
                    throw new InvalidOperationException("Incompatible value type");
                }

                m_defaultValue = value;
            }
        }

        /// <summary>
        /// Gets the rules for this attribute, which constrain the allowable values</summary>
        public IEnumerable<AttributeRule> Rules
        {
            get
            {
                if (m_rules != null)
                    return m_rules;

                return EmptyEnumerable<AttributeRule>.Instance;
            }
        }

        /// <summary>
        /// Adds a rule to the attribute</summary>
        /// <param name="rule">Rule, constraining the attribute value</param>
        public void AddRule(AttributeRule rule)
        {
            if (rule == null)
                throw new ArgumentNullException("rule");

            if (m_rules == null)
                m_rules = new List<AttributeRule>();
            m_rules.Add(rule);
        }

        /// <summary>
        /// Validates a value for assignment to this attribute</summary>
        /// <param name="value">Value, to be assigned</param>
        /// <returns>True, iff value can be assigned to attribute</returns>
        public virtual bool Validate(object value)
        {
            if (!m_type.Validate(value, this))
                return false;

            if (m_rules != null)
            {
                foreach (AttributeRule rule in m_rules)
                    if (!rule.Validate(value, this))
                        return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the parent metadata for this attribute</summary>
        /// <returns>Parent metadata</returns>
        protected override NamedMetadata GetParent()
        {
            if (OwningType != null)
                return OwningType.BaseType.GetAttributeInfo(Index);

            return null;
        }

        private readonly AttributeType m_type;
        private object m_defaultValue;
        private List<AttributeRule> m_rules;
    }
}
