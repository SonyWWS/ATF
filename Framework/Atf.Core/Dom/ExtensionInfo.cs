//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Metadata for extensions, which can be any type. Use extensions to extend DOM
    /// data when it is in memory. Extensions that implement IAdapter can be retrieved
    /// through the IAdaptable interface of DomNode.</summary>
    public abstract class ExtensionInfo : FieldMetadata
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Unique name of extension; the name should be unique in
        /// the context of the type and its lineage</param>
        public ExtensionInfo(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the extension's type</summary>
        public abstract Type Type
        {
            get;
        }

        /// <summary>
        /// Creates an instance of the extension</summary>
        /// <param name="node">DOM node being extended</param>
        /// <returns>Instance of the extension</returns>
        public abstract object Create(DomNode node);

        /// <summary>
        /// Returns a value indicating if other extension metadata is equivalent to this</summary>
        /// <param name="other">Other extension metadata</param>
        /// <returns><c>True</c> if other extension metadata is equivalent to this</returns>
        public bool IsEquivalent(ExtensionInfo other)
        {
            return
                other != null &&
                Index == other.Index &&
                DefiningType == other.DefiningType;
        }

        /// <summary>
        /// Gets the parent metadata for this</summary>
        /// <returns>Parent metadata</returns>
        protected override NamedMetadata GetParent()
        {
            if (OwningType != null)
                return OwningType.BaseType.GetExtensionInfo(Index);

            return null;
        }
    }

    /// <summary>
    /// Metadata for extensions, which can be any type. Use extensions to extend DOM
    /// data when it is in memory. Extensions that implement IAdapter can be retrieved
    /// through the IAdaptable interface of DomNode.</summary>
    /// <typeparam name="T">Type of extension; type must have default constructor</typeparam>
    public class ExtensionInfo<T> : ExtensionInfo
        where T : new()
    {
        /// <summary>
        /// Constructor</summary>
        /// <remarks>The extension's Name will be the type's FullName (e.g., "MyCompany.MyApp.MyContext")</remarks>
        public ExtensionInfo()
            : base(typeof(T).FullName)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Extension name</param>
        public ExtensionInfo(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the extension's type</summary>
        public override Type Type
        {
            get { return typeof(T); }
        }

        /// <summary>
        /// Creates an instance of the extension</summary>
        /// <param name="node">DOM node being extended</param>
        /// <returns>Instance of the extension</returns>
        public override object Create(DomNode node)
        {
            return new T();
        }
    }
}
