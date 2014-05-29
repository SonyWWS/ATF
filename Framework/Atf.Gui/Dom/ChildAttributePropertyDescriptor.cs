//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.ComponentModel;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// PropertyDescriptor for an attribute of a child or descendant of a node</summary>
    public class ChildAttributePropertyDescriptor : AttributePropertyDescriptor
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Property's display name</param>
        /// <param name="attributeInfo">Attribute metadata</param>
        /// <param name="childInfo">ChildInfo identifying child</param>
        /// <param name="category">Category of property</param>
        /// <param name="description">Description of property</param>
        /// <param name="isReadOnly">Whether or not property is read-only</param>
        public ChildAttributePropertyDescriptor(
            string name,
            AttributeInfo attributeInfo,
            ChildInfo childInfo,
            string category,
            string description,
            bool isReadOnly)

            : this(name, attributeInfo, childInfo, category, description, isReadOnly, null, null)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Value's display name</param>
        /// <param name="attributeInfo">Attribute metadata</param>
        /// <param name="childInfo">ChildInfo identifying child</param>
        /// <param name="category">Category of property</param>
        /// <param name="description">Description of property</param>
        /// <param name="isReadOnly">Whether or not property is read-only</param>
        /// <param name="editor">The editor used to edit the property</param>
        public ChildAttributePropertyDescriptor(
            string name,
            AttributeInfo attributeInfo,
            ChildInfo childInfo,
            string category,
            string description,
            bool isReadOnly,
            object editor)

            : this(name, attributeInfo, childInfo, category, description, isReadOnly, editor, null)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Value's display name</param>
        /// <param name="attributeInfo">Attribute metadata</param>
        /// <param name="childInfo">ChildInfo identifying child</param>
        /// <param name="category">Category of property</param>
        /// <param name="description">Description of property</param>
        /// <param name="isReadOnly">Whether or not property is read-only</param>
        /// <param name="editor">The editor used to edit the property</param>
        /// <param name="typeConverter">The type converter used for this property</param>
        public ChildAttributePropertyDescriptor(
            string name,
            AttributeInfo attributeInfo,
            ChildInfo childInfo,
            string category,
            string description,
            bool isReadOnly,
            object editor,
            TypeConverter typeConverter)

            : base(name, attributeInfo, category, description, isReadOnly, editor, typeConverter)
        {
            m_childPath = new[] { childInfo };
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Value's display name</param>
        /// <param name="attributeInfo">Attribute metadata</param>
        /// <param name="childInfo">ChildInfo identifying child</param>
        /// <param name="childIndex">Index into ChildInfo, if ChildInfo is a list</param>
        /// <param name="category">Category of property</param>
        /// <param name="description">Description of property</param>
        /// <param name="isReadOnly">Whether or not property is read-only</param>
        /// <param name="editor">The editor used to edit the property</param>
        /// <param name="typeConverter">The type converter used for this property</param>
        public ChildAttributePropertyDescriptor(
            string name,
            AttributeInfo attributeInfo,
            ChildInfo childInfo,
            int childIndex,
            string category,
            string description,
            bool isReadOnly,
            object editor,
            TypeConverter typeConverter)

            : base(name, attributeInfo, category, description, isReadOnly, editor, typeConverter)
        {
            m_childPath = new[] { childInfo };
            m_childIndices = new[] { childIndex };
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Value's display name</param>
        /// <param name="attributeInfo">Attribute metadata</param>
        /// <param name="childPath">ChildInfo array describing path to descendant child</param>
        /// <param name="category">Category of property</param>
        /// <param name="description">Description of property</param>
        /// <param name="isReadOnly">Whether or not property is read-only</param>
        /// <param name="editor">The editor used to edit the property</param>
        /// <param name="typeConverter">The type converter used for this property</param>
        public ChildAttributePropertyDescriptor(
            string name,
            AttributeInfo attributeInfo,
            IEnumerable<ChildInfo> childPath,
            string category,
            string description,
            bool isReadOnly,
            object editor,
            TypeConverter typeConverter)

            : base(name, attributeInfo, category, description, isReadOnly, editor, typeConverter)
        {
            m_childPath = childPath;
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Value's display name</param>
        /// <param name="attributeInfo">Attribute metadata</param>
        /// <param name="childPath">ChildInfo array describing path to descendant child</param>
        /// <param name="childIndices">The index to use in each ChildInfo if that ChildInfo is a list</param>
        /// <param name="category">Category of property</param>
        /// <param name="description">Description of property</param>
        /// <param name="isReadOnly">Whether or not property is read-only</param>
        /// <param name="editor">The editor used to edit the property</param>
        /// <param name="typeConverter">The type converter used for this property</param>
        public ChildAttributePropertyDescriptor(
            string name,
            AttributeInfo attributeInfo,
            IEnumerable<ChildInfo> childPath,
            IEnumerable<int> childIndices,
            string category,
            string description,
            bool isReadOnly,
            object editor,
            TypeConverter typeConverter)

            : base(name, attributeInfo, category, description, isReadOnly, editor, typeConverter)
        {
            m_childPath = childPath;
            m_childIndices = childIndices;
        }

        /// <summary>
        /// Gets the path from the root node to the node that actually holds the property</summary>
        public override IEnumerable<ChildInfo> Path
        {
            get { return m_childPath; }
        }

        /// <summary>
        /// Gets node from component</summary>
        /// <param name="component">Component being edited</param>
        /// <returns>DomNode for component</returns>
        public override DomNode GetNode(object component)
        {
            var node = component.As<DomNode>();
            if (node != null)
            {
                // Check that the child path exists on the Node
                bool haveIndex = false;
                IEnumerator<int> indexEnumerator =
                    m_childIndices == null ? null : m_childIndices.GetEnumerator();
                if (indexEnumerator != null)
                    haveIndex = indexEnumerator.MoveNext();
                foreach (ChildInfo childInfo in m_childPath)
                {
                    if (!node.Type.IsValid(childInfo))
                        return null;

                    if (!haveIndex)
                        node = node.GetChild(childInfo);
                    else
                    {
                        IList<DomNode> nodeList = node.GetChildList(childInfo);
                        if (nodeList.Count > indexEnumerator.Current)
                            node = nodeList[indexEnumerator.Current];
                        else
                            return null;
                        haveIndex = indexEnumerator.MoveNext();
                    }

                    if (node == null)
                        return null;
                }

                if (!node.Type.IsValid(AttributeInfo))
                    return null;
            }

            return node;
        }

        /// <summary>
        /// Tests equality of property descriptor with object</summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>True iff property descriptors are identical</returns>
        /// <remarks>Implements Equals() for organizing descriptors in grid controls</remarks>
        public override bool Equals(object obj)
        {
            var other = obj as ChildAttributePropertyDescriptor;
            if (other == null || !base.Equals(other))
                return false;

            IEnumerator<ChildInfo> myEnumerator = m_childPath.GetEnumerator();
            IEnumerator<ChildInfo> otherEnumerator = other.m_childPath.GetEnumerator();
            do
            {
                bool myNextExists = myEnumerator.MoveNext();
                bool otherNextExists = otherEnumerator.MoveNext();
                if (myNextExists != otherNextExists)
                    return false;
                if (!myNextExists)
                    return true;
                if (!myEnumerator.Current.Equivalent(otherEnumerator.Current))
                    return false;
            } while (true);
        }

        /// <summary>
        /// Gets hash code for property descriptor</summary>
        /// <returns>Hash code</returns>
        /// <remarks>Implements GetHashCode() for organizing descriptors in grid controls</remarks>
        public override int GetHashCode()
        {
            int result = base.GetHashCode();
            foreach (ChildInfo childInfo in m_childPath)
                result ^= childInfo.GetEquivalentHashCode();
            return result;
        }

        private readonly IEnumerable<ChildInfo> m_childPath;
        private readonly IEnumerable<int> m_childIndices;
    }
}
