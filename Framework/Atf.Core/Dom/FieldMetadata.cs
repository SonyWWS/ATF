//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Metadata for a field (e.g., child element or attribute or extension) that is defined
    /// by a DomNodeType and owned by a possibly separate DomNodeType (if a DOM type exists
    /// that derives from the base DOM type that defines this field).</summary>
    public abstract class FieldMetadata : NamedMetadata
    {
        /// <summary>
        /// Constructor for derived classes.</summary>
        /// <param name="name">Metadata name</param>
        protected FieldMetadata(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the DOM node type that owns this field. This can be different than DefiningType if the
        /// owning type inherits from the defining type.</summary>
        public DomNodeType OwningType
        {
            get { return m_owningType; }
            internal set { m_owningType = value; }
        }

        /// <summary>
        /// Gets the DOM node type in a DOM type hierarchy that first defines this field. Derived DomNodeType
        /// instances have the same DefiningType property value as their BaseType.</summary>
        public DomNodeType DefiningType
        {
            get { return m_definingType; }
            internal set { m_definingType = value; }
        }

        /// <summary>
        /// Indicates if other field metadata refers to the same data as this.</summary>
        /// <param name="other">Other field metadata</param>
        /// <returns>True iff other field metadata is equivalent to this</returns>
        public bool Equivalent(FieldMetadata other)
        {
            return
                other != null &&
                Index == other.Index &&
                DefiningType == other.DefiningType;
        }

        /// <summary>
        /// Gets a stable hash code that correlates with Equivalent()</summary>
        /// <returns>Hash code</returns>
        /// <remarks>If two hash codes are the same, the FieldMetadata objects might be equivalent, and
        /// if two FieldMetadata objects are equivalent, the hash codes are guaranteed to be the same.</remarks>
        public int GetEquivalentHashCode()
        {
            // We can't use DefiningType or Index, because those are set after this object is created,
            //  and we don't want to change the hash code after this object is in a dictionary.
            // It should be impossible for Name to not be identical if Equivalent() returns true.
            return Name.GetHashCode();
        }

        /// <summary>
        /// Gets and sets the index in the type for fast retrieval.</summary>
        internal int Index
        {
            get { return m_index; }
            set { m_index = value; }
        }

        private DomNodeType m_owningType;
        private DomNodeType m_definingType;
        private int m_index;
    }
}
