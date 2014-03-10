//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Struct to hold unresolved node references</summary>
    public struct XmlNodeReference
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="node">DomNode holding the reference</param>
        /// <param name="attributeInfo">Reference attribute of node</param>
        /// <param name="value">Persistent string form of the reference</param>
        public XmlNodeReference(DomNode node, AttributeInfo attributeInfo, string value)
        {
            Node = node;
            AttributeInfo = attributeInfo;
            Value = value;
        }

        /// <summary>
        /// Gets the DomNode that has an attribute that is holding the reference</summary>
        public readonly DomNode Node;

        /// <summary>
        /// Gets the reference attribute of the DomNode</summary>
        public readonly AttributeInfo AttributeInfo;

        /// <summary>
        /// Gets the persistent string form of the reference</summary>
        public readonly string Value;
    }
}
