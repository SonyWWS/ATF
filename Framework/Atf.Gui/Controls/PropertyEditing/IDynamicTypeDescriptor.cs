//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// An interface that specifies whether or not the property descriptors that are
    /// provided by this object are the same for all instances of this type of object</summary>
    /// <remarks>This is useful for PropertyUtils and MultiPropertyDescriptor, because it greatly
    /// improves performance while allowing some types of objects to opt out of a cache.</remarks>
    public interface IDynamicTypeDescriptor : ICustomTypeDescriptor
    {
        /// <summary>
        /// Returns <c>True</c> if this custom type descriptor can provide a PropertyDescriptorCollection
        /// (via GetProperties) that is the same for all instances of this type of object
        /// and that can be permanently cached</summary>
        /// <remarks>Returning 'true' greatly improves performance.</remarks>
        bool CacheableProperties
        {
            get;
        }
    }
}
