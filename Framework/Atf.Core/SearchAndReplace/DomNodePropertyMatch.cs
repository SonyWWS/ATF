//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;

namespace Sce.Atf.Dom
{

    /// <summary>
    /// Class that encapsulates a matching DomNode property, found during a DomNode property query</summary>
    public class DomNodePropertyMatch : IQueryMatch
    {
        /// <summary>
        /// Constructor</summary>
        private DomNodePropertyMatch() { }

        /// <summary>
        /// Constructor with property and DomNode</summary>
        /// <param name="property">The matching property</param>
        /// <param name="domNode">The DomNode with the matching property</param>
        public DomNodePropertyMatch(PropertyDescriptor property, DomNode domNode)
        {
            if (property == null)
                throw new ArgumentNullException("property", "Cannot create a DomNodePropertyMatchCandiate with a null property.");

            if (domNode == null)
                throw new ArgumentNullException("domNode", "Cannot create a DomNodePropertyMatchCandiate with a null domNode.");

            m_propertyDescriptor = property;
            m_domNode = domNode;
        }

        #region IQueryMatch
        /// <summary>
        /// Gets value of the DomNode's matching property</summary>
        /// <returns>Value of DomNode's matching property</returns>
        public object GetValue() { return m_propertyDescriptor.GetValue(m_domNode); }

        /// <summary>
        /// Sets value of the DomNode's matching property</summary>
        /// <param name="value">Value to set</param>
        public void SetValue(object value)
        {
            object correctlyTypedValue = Convert.ChangeType(value, GetValue().GetType());
            if (correctlyTypedValue == null)
                throw new InvalidOperationException("Attempted to replace the value of a search result with an incompatible type");
            m_propertyDescriptor.SetValue(m_domNode, correctlyTypedValue);
        }
        #endregion

        /// <summary>
        /// Gets or sets name of the DomNode's matching property</summary>
        public string Name
        {
            get { return m_propertyDescriptor.Name; }
            set { /* cannot change the name of a property descriptor */}
        }

        /// <summary>
        /// Gets DomNode's matching property descriptor</summary>
        public PropertyDescriptor PropertyDescriptor
        {
            get { return m_propertyDescriptor; }
        }

        /// <summary>
        /// DomNode's matching property descriptor</summary>
        protected PropertyDescriptor m_propertyDescriptor;
        /// <summary>
        /// DomNode with the matching property</summary>
        protected DomNode m_domNode;
    }
}
