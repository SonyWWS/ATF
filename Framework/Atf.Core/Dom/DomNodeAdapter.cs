//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Base class for DomNode adapters, which allow DomNodes to be dynamically
    /// cast to more convenient forms</summary>
    public abstract class DomNodeAdapter : IAdapter, IAdaptable, IDecoratable
    {
        /// <summary>
        /// Gets the adapted DomNode</summary>
        public DomNode DomNode
        {
            get { return m_domNode; }
            internal set
            {
                m_domNode = value;
                OnNodeSet();
            }
        }

        #region IAdapter Members

        /// <summary>
        /// Gets or sets the object that is adapted. Note that the setter may only be called
        /// once. Any subsequent call should generate an InvalidOperationException.</summary>
        object IAdapter.Adaptee
        {
            get { return m_domNode; }
            set
            {
                if (m_domNode != null)
                    throw new InvalidOperationException("DomNode already set");
                DomNode node = value as DomNode;
                if (node == null)
                    throw new InvalidOperationException("value must be DomNode");
                DomNode = node;
            }
        }

        /// <summary>
        /// Gets all decorators of the specified type</summary>
        /// <param name="type">Decorator type</param>
        /// <returns>Enumeration of non-null decorators that are of the specified type. The enumeration may be empty.</returns>
        public virtual IEnumerable<object> GetDecorators(Type type)
        {
            return m_domNode.GetDecorators(type);
        }


        #endregion

        #region IAdaptable, IDecoratable, and Related Methods

        /// <summary>
        /// Gets an adapter of the specified type, or null</summary>
        /// <param name="type">Adapter type</param>
        /// <returns>Adapter of the specified type, or null</returns>
        public virtual object GetAdapter(Type type)
        {
            return m_domNode.GetAdapter(type);
        }

        #endregion

        /// <summary>
        /// Performs one-time initialization when this adapter's DomNode property is set.
        /// The DomNode property is only ever set once for the lifetime of this adapter.</summary>
        protected virtual void OnNodeSet()
        {
        }

        /// <summary>
        /// Gets the underlying DOM node's parent adapted the given type, or null if there is
        /// no parent or it can't be adapted to the type</summary>
        /// <typeparam name="T">Desired type</typeparam>
        /// <returns>The underlying DOM node's parent adapted the given type, or null</returns>
        protected T GetParentAs<T>()
            where T : class
        {
            DomNode parent = DomNode.Parent;
            return parent != null ? parent.As<T>() : null;
        }

        /// <summary>
        /// Gets the value of this attribute, cast to type T</summary>
        /// <typeparam name="T">Attribute type</typeparam>
        /// <param name="attributeInfo">The attribute metadata of the adapted DomNode to look for</param>
        /// <returns>The value of the attribute or null (if T is a reference type) or
        /// the default value of type T (if T is a value type)</returns>
        protected T GetAttribute<T>(AttributeInfo attributeInfo)
        {
            object value = DomNode.GetAttribute(attributeInfo);

            // if value is not null, attempt the cast; an invalid type will then cause
            //  an IllegalCastException; all value type attributes have a default
            //  value, so will return here
            if (value != null)
                return (T)value;

            // value is null, so it must be a reference type attribute (Uri or DomNode value)
            //  return null
            return default(T);
        }

        /// <summary>
        /// Sets the value of the adapted DomNode's attribute</summary>
        /// <param name="attributeInfo">The attribute metadata of the adapted DomNode to look for</param>
        /// <param name="value">New value</param>
        protected void SetAttribute(AttributeInfo attributeInfo, object value)
        {
            DomNode.SetAttribute(attributeInfo, value);
        }

        /// <summary>
        /// Gets the DomNode that is referenced by this attribute, adapted to type T</summary>
        /// <typeparam name="T">The class to adapt the referenced DomNode to</typeparam>
        /// <param name="attributeInfo">The attribute metadata of the adapted DomNode to look for</param>
        /// <returns>If the given attribute metadata defines a reference to a DomNode, that DomNode
        /// is adapted to type T and the result is returned. Otherwise, null is returned.</returns>
        protected T GetReference<T>(AttributeInfo attributeInfo)
            where T : class
        {
            DomNode refNode = DomNode.GetAttribute(attributeInfo) as DomNode;
            if (refNode != null)
                return refNode.As<T>();

            return null;
        }

        /// <summary>
        /// Sets the attribute of the adapted DomNode to reference a new DomNode</summary>
        /// <param name="attributeInfo">Metadata to indicate which attribute to set</param>
        /// <param name="value">Any IAdaptable, such as DomNodeAdapter, DomNode, or IAdapter</param>
        protected void SetReference(AttributeInfo attributeInfo, IAdaptable value)
        {
            DomNode refNode = value.As<DomNode>();
            DomNode.SetAttribute(attributeInfo, refNode);
        }

        /// <summary>
        /// Gets the child described by the given child metadata</summary>
        /// <typeparam name="T">The type to adapt the child DomNode to</typeparam>
        /// <param name="childInfo">Metadata to indicate which child type to look for</param>
        /// <returns>The child DomNode, adapted to the given type, or null</returns>
        protected T GetChild<T>(ChildInfo childInfo)
            where T : class
        {
            DomNode child = DomNode.GetChild(childInfo);
            if (child != null)
                return child.As<T>();

            return null;
        }

        /// <summary>
        /// Sets the child of our adapted DomNode to a new DomNode</summary>
        /// <param name="childInfo">Metadata to indicate the child</param>
        /// <param name="value">Any IAdaptable, such as DomNodeAdapter, DomNode, or IAdapter</param>
        protected void SetChild(ChildInfo childInfo, IAdaptable value)
        {
            DomNode child = value.As<DomNode>();
            DomNode.SetChild(childInfo, child);
        }

        /// <summary>
        /// Gets the children of our adapted DomNode</summary>
        /// <typeparam name="T">The type to adapt each child to</typeparam>
        /// <param name="childInfo">Metadata to indicate the child list</param>
        /// <returns>Wrapper that adapts a node child list to a list of T items</returns>
        protected IList<T> GetChildList<T>(ChildInfo childInfo)
            where T : class
        {
            return new DomNodeListAdapter<T>(DomNode, childInfo);
        }

        /// <summary>
        /// Gets the extension object of the adapted DomNode that corresponds to the given metadata</summary>
        /// <typeparam name="T">Reference type to adapt the extension to</typeparam>
        /// <param name="extensionInfo">Metadata to indicate the extension to find on the adapted DomNode</param>
        /// <returns>The extension object, adapted to type T, or null</returns>
        protected T GetExtension<T>(ExtensionInfo extensionInfo)
            where T : class
        {
            return DomNode.GetExtension(extensionInfo).As<T>();
        }

        /// <summary>
        /// Tests for equality with another object</summary>
        /// <param name="obj">Other object</param>
        /// <returns>True iff the DomNodeAdapter equals the other object; equality means that
        /// the other object is equal to the underlying DomNode</returns>
        public override bool Equals(object obj)
        {
            if (m_domNode == null)
                throw new InvalidOperationException("node not set");

            return m_domNode.Equals(obj);
        }

        /// <summary>
        /// Generates a hash code for the instance</summary>
        /// <returns>Hash code for the instance</returns>
        public override int GetHashCode()
        {
            if (m_domNode == null)
                throw new InvalidOperationException("node not set");

            return m_domNode.GetHashCode();
        }

        /// <summary>
        /// Gets string representation of DomNodeAdapter</summary>
        /// <returns>String representation of DomNodeAdapter</returns>
        public override string ToString()
        {
            if (m_domNode != null)
                return base.ToString() + ", on " + m_domNode;
            return base.ToString() + ", DomNode is not set";
        }

        private DomNode m_domNode;
    }
}
