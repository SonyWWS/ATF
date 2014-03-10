//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// PropertyDescriptor for a child of a node</summary>
    public class ChildPropertyDescriptor : PropertyDescriptor
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Property's display name</param>
        /// <param name="childInfo">ChildInfo identifying child</param>
        /// <param name="category">Category of property</param>
        /// <param name="description">Description of property</param>
        /// <param name="isReadOnly">Whether or not property is read-only</param>
        public ChildPropertyDescriptor(
            string name,
            ChildInfo childInfo,
            string category,
            string description,
            bool isReadOnly)

            : this(name, childInfo, category, description, isReadOnly, null, null)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Value's display name</param>
        /// <param name="childInfo">ChildInfo identifying child</param>
        /// <param name="category">Category of property</param>
        /// <param name="description">Description of property</param>
        /// <param name="isReadOnly">Whether or not property is read-only</param>
        /// <param name="editor">The editor used to edit the property</param>
        public ChildPropertyDescriptor(
            string name,
            ChildInfo childInfo,
            string category,
            string description,
            bool isReadOnly,
            object editor)

            : this(name, childInfo, category, description, isReadOnly, editor, null)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Value's display name</param>
        /// <param name="childInfo">ChildInfo identifying child</param>
        /// <param name="category">Category of property</param>
        /// <param name="description">Description of property</param>
        /// <param name="isReadOnly">Whether or not property is read-only</param>
        /// <param name="editor">The editor used to edit the property</param>
        /// <param name="typeConverter">The type converter used for this property</param>
        public ChildPropertyDescriptor(
            string name,
            ChildInfo childInfo,
            string category,
            string description,
            bool isReadOnly,
            object editor,
            TypeConverter typeConverter)

            : base(name, typeof(DomNode), category, description, isReadOnly, editor, typeConverter)
        {
            m_childInfo = childInfo;
        }

        #region Overrides

        /// <summary>
        /// Tests equality of property descriptor with object</summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>True iff property descriptors are identical</returns>
        /// <remarks>Implements Equals() for organizing descriptors in grid controls</remarks>
        public override bool Equals(object obj)
        {
            ChildPropertyDescriptor other = obj as ChildPropertyDescriptor;
            if (other == null)
                return false;

            return m_childInfo == other.m_childInfo;
        }

        /// <summary>
        /// Gets hash code for property descriptor</summary>
        /// <returns>Hash code</returns>
        /// <remarks>Implements GetHashCode() for organizing descriptors in grid controls</remarks>
        public override int GetHashCode()
        {
            int result = base.GetHashCode();
            result ^= m_childInfo.GetHashCode();
            return result;
        }

        /// <summary>
        /// When overridden in a derived class, returns whether resetting an object changes its value</summary>
        /// <param name="component">The component to test for reset capability</param>
        /// <returns>True iff resetting the component changes its value</returns>
        public override bool CanResetValue(object component)
        {
            return false;
        }

        /// <summary>
        /// When overridden in a derived class, resets the value for this property of the component to the default value</summary>
        /// <param name="component">The component with the property value that is to be reset to the default value</param>
        /// <exception cref="InvalidOperationException">Can't reset value</exception>
        public override void ResetValue(object component)
        {
            throw new InvalidOperationException("Can't reset value");
        }

        /// <summary>
        /// When overridden in a derived class, gets the result value of the property on a component</summary>
        /// <param name="component">The component with the property for which to retrieve the value</param>
        /// <returns>The value of a property for a given component.</returns>
        public override object GetValue(object component)
        {
            DomNode node = GetNode(component);
            if (node != null &&
                node.Type.IsValid(m_childInfo))
            {
                if (m_childInfo.IsList)
                    return node.GetChildList(m_childInfo);
                return node.GetChild(m_childInfo);
            }

            return null;
        }

        /// <summary>
        /// When overridden in a derived class, sets the value of the component to a different value</summary>
        /// <param name="component">The component with the property value that is to be set</param>
        /// <param name="value">The new value</param>
        public override void SetValue(object component, object value)
        {
            DomNode node = GetNode(component);

            if (node != null)
            {
                if (m_childInfo.IsList)
                {
                    IList<DomNode> targetList = node.GetChildList(m_childInfo);
                    IList<DomNode> valueList = value as IList<DomNode>;
                    if (targetList != null && valueList != null)
                    {
                        // Diff targetList and valueList
                        int i = 0;
                        while (i < Math.Min(valueList.Count, targetList.Count) && targetList[i] == valueList[i])
                            ++i;
                        // => Now they are identical up to index i

                        // Remove excess items from target list
                        while (targetList.Count > i)
                            targetList.RemoveAt(i);

                        // Add additional items from value list
                        for (; i < valueList.Count; i++)
                            targetList.Add(valueList[i]);
                    }
                }
                else
                {
                    DomNode valueNode = GetNode(value);
                    if (valueNode != null)
                        node.SetChild(m_childInfo, valueNode);
                }
            }
        }

        #endregion

        /// <summary>
        /// Gets the DOM ChildInfo</summary>
        public ChildInfo ChildInfo { get { return m_childInfo; } }

        private readonly ChildInfo m_childInfo;
    }
}