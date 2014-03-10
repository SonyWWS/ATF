//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Base class for metadata. DomNodeType, AttributeInfo, ChildInfo, and ExtensionInfo
    /// all derive from this type.</summary>
    public abstract class NamedMetadata
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Metadata name</param>
        protected NamedMetadata(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            m_name = name;
        }

        /// <summary>
        /// Gets the metadata name</summary>
        /// <remarks>If this is a DomNodeType, and if it was defined by a schema file, then the name is
        /// the qualified name (System.Xml.Schema.XmlSchemaType.QualifiedName) like "timeline:markerType".
        /// If this is an Extension of generic type T, then the name is the FullName of the type, like
        /// "TimelineEditorSample.TimelineContext".</remarks>
        public string Name
        {
            get { return m_name; }
        }

        /// <summary>
        /// Sets a tag object on the metadata, under the key</summary>
        /// <param name="key">Tag key</param>
        /// <param name="value">Tag value</param>
        public void SetTag(object key, object value)
        {
            if (m_tags == null)
                m_tags = new Dictionary<object, object>();

            m_tags[key] = value;
        }

        /// <summary>
        /// Sets a tag object on the metadata, under the type</summary>
        /// <typeparam name="T">Tag type</typeparam>
        /// <param name="value">Tag value</param>
        public void SetTag<T>(T value)
        {
            SetTag(typeof(T), value);
        }

        /// <summary>
        /// Gets a tag object on the metadata corresponding to the key</summary>
        /// <param name="key">Tag key</param>
        /// <returns>Tag value</returns>
        public object GetTagLocal(object key)
        {
            object tag = null;
            if (m_tags != null)
                m_tags.TryGetValue(key, out tag);
            return tag;
        }

        /// <summary>
        /// Gets a tag object on the metadata corresponding to the type</summary>
        /// <typeparam name="T">Tag type</typeparam>
        /// <returns>Tag value</returns>
        public T GetTagLocal<T>()
        {
            object tag = GetTagLocal(typeof(T));
            if (tag is T)
                return (T)tag;

            return default(T);
        }

        /// <summary>
        /// Gets a tag's associated object ("value") on the metadata corresponding to the key;
        /// searches up the lineage to the root node type, or until a tag is found</summary>
        /// <param name="key">Tag key</param>
        /// <returns>Tag value, or null if it can't be found</returns>
        public object GetTag(object key)
        {
            object tag = null;
            NamedMetadata metadata = this;
            while (metadata != null)
            {
                tag = metadata.GetTagLocal(key);
                if (tag != null)
                    break;

                metadata = metadata.GetParent();
            }

            return tag;
        }

        /// <summary>
        /// Gets a tag object on the metadata corresponding to the type; searches up the lineage
        /// to the root node type, or until a tag is found. Returns the default value of T (e.g.,
        /// null) if the tag wasn't found.</summary>
        /// <typeparam name="T">Tag type</typeparam>
        /// <returns>Tag value if found or the default value of T (e.g., null) otherwise</returns>
        public T GetTag<T>()
        {
            object tag = GetTag(typeof(T));
            if (tag is T)
                return (T)tag;

            return default(T);
        }

        /// <summary>
        /// Gets the parent of this metadata. For DomNodeType, this would be the base node type,
        /// while for AttributeInfo, this would be the corresponding info on the base type.</summary>
        /// <returns>Parent of this metadata</returns>
        protected virtual NamedMetadata GetParent()
        {
            return null;
        }

        /// <summary>
        /// Converts metadata to string</summary>
        /// <returns>String representation of metadata</returns>
        public override string ToString()
        {
            if (m_name != null)
                return m_name;

            return base.ToString();
        }

        private readonly string m_name;
        private Dictionary<object, object> m_tags;
    }
}
