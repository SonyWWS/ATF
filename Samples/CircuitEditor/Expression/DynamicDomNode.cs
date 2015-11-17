//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// A wrapper class that exposes attributes and children of a 
    /// DomNode as a dynamic properties.    
    /// Note: expand/edit this class as needed.
    ///  
    /// </summary>
    class DynamicDomNode : DynamicObject
    {
        private DomNode m_node;
        private ExpressionManager m_mgr;
        public DynamicDomNode(DomNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            m_mgr = node.GetRoot().As<ExpressionManager>();
            if (m_mgr == null)
                throw new ArgumentException("node must be part of circuit");
            m_node = node;

        }
      
        public string Id
        {
            get { return m_node.GetId(); }
        }

        public IEnumerable<AttributePropertyDescriptor> Descriptors
        {
            get
            {
                var props = PropertyUtils.GetDefaultProperties(m_node);
                foreach (PropertyDescriptor propDescr in props)
                {
                    AttributePropertyDescriptor attDescr = propDescr as AttributePropertyDescriptor;
                    if (attDescr != null) yield return attDescr;
                }
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {            
            // linearly search property by name.
            foreach (var attDescr in Descriptors)
            {
                if (attDescr.Name == binder.Name)
                {
                    if (!attDescr.IsReadOnly)
                        attDescr.SetValue(m_node, value);
                    return true;
                }
            }

            return base.TrySetMember(binder, value);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {            
            // linearly search property by name.
            foreach (var attDescr in Descriptors)
            {
                if (attDescr.Name == binder.Name)
                {
                    result = attDescr.GetValue(m_node);
                    m_mgr.ObserveAttribute(m_node, attDescr.AttributeInfo);
                    return true;
                }
            }

            return base.TryGetMember(binder, out result);
        }
    }
}
