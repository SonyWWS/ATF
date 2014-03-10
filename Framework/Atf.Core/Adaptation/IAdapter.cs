//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Adaptation
{
    /// <summary>
    /// Interface for adapters, which adapt an adaptee. This interface allows adapters
    /// to be created in factories, and linked to their adaptees at some point after
    /// construction.</summary>
    public interface IAdapter : IAdaptable, IDecoratable
    {
        /// <summary>
        /// Gets or sets the object that is adapted. Note that the setter may only be called
        /// once. Any subsequent call should generate an InvalidOperationException.</summary>
        object Adaptee
        {
            get;
            set;
        }
    }
}
