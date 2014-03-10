//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Xml;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// A collection of metadata (DomNodeType, ChildInfo, and AttributeInfo objects) that is
    /// associated with a target namespace from an XML schema file</summary>
    public class XmlSchemaTypeCollection
    {
        internal XmlSchemaTypeCollection(
            XmlQualifiedName[] namespaces,
            string targetNamespace,
            XmlSchemaTypeLoader loader)
        {
            m_namespaces = namespaces;
            m_targetNamespace = targetNamespace;
            m_loader = loader;

            foreach (XmlQualifiedName ns in m_namespaces)
            {
                if (ns.Name == string.Empty)
                {
                    m_defaultNamespace = ns.Namespace;
                    break;
                }
            }
        }

        /// <summary>
        /// Gets all namespaces in the schema</summary>
        public XmlQualifiedName[] Namespaces
        {
            get { return m_namespaces; }
        }

        /// <summary>
        /// Gets the target namespace of the schema</summary>
        public string TargetNamespace
        {
            get { return m_targetNamespace; }
        }

        /// <summary>
        /// Gets the default namespace of the schema</summary>
        public string DefaultNamespace
        {
            get { return m_defaultNamespace; }
        }

        /// <summary>
        /// Gets the namespace prefix of the given namespace</summary>
        /// <param name="ns">Namespace to search for</param>
        /// <returns>Namespace prefix of the given namespace</returns>
        public string GetPrefix(string ns)
        {
            if (ns == m_defaultNamespace)
                return string.Empty;

            foreach (XmlQualifiedName name in m_namespaces)
                if (name.Namespace == ns)
                    return name.Name;

            return null;
        }

        /// <summary>
        /// Gets the attribute metadata defined in the target namespace</summary>
        /// <returns>Attribute metadata</returns>
        public IEnumerable<AttributeType> GetAttributeTypes()
        {
            return m_loader.GetAttributeTypes(m_targetNamespace);
        }

        /// <summary>
        /// Gets an attribute type</summary>
        /// <param name="name">Unqualified name of attribute type</param>
        /// <returns>Attribute type or null if unknown name</returns>
        public AttributeType GetAttributeType(string name)
        {
            return m_loader.GetAttributeType(m_targetNamespace + ":" + name);
        }

        /// <summary>
        /// Gets the DomNodeType metadata that is defined in the target namespace</summary>
        /// <returns>DomNodeType metadata</returns>
        public IEnumerable<DomNodeType> GetNodeTypes()
        {
            return m_loader.GetNodeTypes(m_targetNamespace);
        }

        /// <summary>
        /// Gets the DomNodeType metadata that is defined in the given namespace</summary>
        /// <param name="ns">Namespace to retrieve DomNodeTypes from</param>
        /// <returns>DomNodeType metadata</returns>
        public IEnumerable<DomNodeType> GetNodeTypes(string ns)
        {
            return m_loader.GetNodeTypes(ns);
        }

        /// <summary>
        /// Gets a node type</summary>
        /// <param name="name">Unqualified name of node type</param>
        /// <returns>Node type or null if unknown name</returns>
        public DomNodeType GetNodeType(string name)
        {
            return m_loader.GetNodeType(m_targetNamespace + ":" + name);
        }

        /// <summary>
        /// Gets a node type</summary>
        /// <param name="targetNamespace">Namespace path of node type to get</param>
        /// <param name="name">Unqualified name of node type</param>
        /// <returns>Node type or null if unknown name</returns>
        public DomNodeType GetNodeType(string targetNamespace, string name)
        {
            return m_loader.GetNodeType(targetNamespace + ":" + name);
        }

        /// <summary>
        /// Gets the root element with the given name</summary>
        /// <param name="name">Unqualified name of element</param>
        /// <returns>Child info for root element or null if unknown name</returns>
        public ChildInfo GetRootElement(string name)
        {
            return m_loader.GetRootElement(m_targetNamespace + ":" + name);
        }

        /// <summary>
        /// Gets the root element with the given name</summary>
        /// <param name="targetNamespace">Namespace path of root element to get</param>
        /// <param name="name">Unqualified name of element</param>
        /// <returns>Child info for root element or null if unknown name</returns>
        public ChildInfo GetRootElement(string targetNamespace, string name)
        {
            return m_loader.GetRootElement(targetNamespace + ":" + name);
        }

        /// <summary>
        /// Gets all root elements</summary>
        /// <returns>Enumeration of child info for all root elements</returns>
        public IEnumerable<ChildInfo> GetRootElements()
        {
            return m_loader.GetRootElements();
        }

        ///// <summary>
        ///// Gets all root elements of the target namespace</summary>
        ///// <returns>enumeration of child info for all root elements in the target namespace</returns>
        //public IEnumerable<ChildInfo> GetRootElements()
        //{
        //    return m_loader.GetRootElements(m_targetNamespace);
        //}

        ///// <summary>
        ///// Gets all root elements of all namespaces</summary>
        ///// <returns>enumeration of child info for all root elements of all namespaces</returns>
        //public IEnumerable<ChildInfo> GetAllRootElements()
        //{
        //    return m_loader.GetRootElements();
        //}

        private readonly string m_targetNamespace;
        private readonly XmlQualifiedName[] m_namespaces;
        private readonly XmlSchemaTypeLoader m_loader;
        private readonly string m_defaultNamespace = string.Empty;
    }
}
