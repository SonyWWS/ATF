//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Metadata for XML attributes, which add information from XML schemas. This metadata
    /// is used by the XmlPersister class to read and write according to an XML schema.</summary>
    public class XmlAttributeInfo : AttributeInfo
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Attribute name</param>
        /// <param name="type">Attribute type</param>
        /// <param name="forceSerialize">Force serialize this attribute even when it has default value</param>
        public XmlAttributeInfo(string name, AttributeType type,
            bool forceSerialize = false)
            : base(name, type, forceSerialize)
        {            
        }

        /// <summary>
        /// Gets or sets a value indicating if this attribute was originally defined as
        /// an XML element</summary>
        public bool IsElement
        {
            get { return m_isElement; }
            set { m_isElement = value; }
        }
        private bool m_isElement;
    }
}
