//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Xml.Schema;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Metadata for XML attribute types, with extra information from an XML schema; this
    /// metadata is used by the XmlPersister class to read and write according to the schema</summary>
    public class XmlAttributeType : AttributeType
    {
        /// <summary>
        /// Initializes an instance of a scalar or array simple type</summary>
        /// <param name="name">Type name</param>
        /// <param name="type">CLR type</param>
        /// <param name="length">If not 1, length of array</param>
        /// <param name="xmlTypeCode">XML type code</param>
        public XmlAttributeType(string name, Type type, int length, XmlTypeCode xmlTypeCode)
            : base(name, type, length)
        {
            m_xmlTypeCode = xmlTypeCode;
        }

        /// <summary>
        /// Gets a value indicating the XML type of this attribute. This information
        /// is needed by the XmlPersister to correctly read and write the XML. Types that get
        /// special handling are xs:base64Binary, xs:ID, xs:IDREF, xs:anyURI</summary>
        public XmlTypeCode XmlTypeCode
        {
            get { return m_xmlTypeCode; }
        }
        private readonly XmlTypeCode m_xmlTypeCode;

        /// <summary>
        /// Converts an instance of the simple type to a string</summary>
        /// <param name="value">Instance of simple type</param>
        /// <returns>String representation of instance</returns>
        public override string Convert(object value)
        {
            if (value != null &&
                m_xmlTypeCode == XmlTypeCode.Base64Binary)
            {
                return System.Convert.ToBase64String((byte[])value);
            }
            return base.Convert(value);
        }

        /// <summary>
        /// Converts string to instance of simple type</summary>
        /// <param name="s">Input string</param>
        /// <returns>Instance of simple type</returns>
        public override object Convert(string s)
        {
            if (m_xmlTypeCode == XmlTypeCode.Base64Binary)
                return System.Convert.FromBase64String(s);

            return base.Convert(s);
        }
    }
}
