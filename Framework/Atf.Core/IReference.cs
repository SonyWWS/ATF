//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf
{
    /// <summary>
    /// Interface for references</summary>
    /// <typeparam name="T">Type of the reference target</typeparam>
    public interface IReference<T>
    {
        /// <summary>
        /// Returns true iff the reference can reference the specified target item</summary>
        /// <param name="item">Item to be referenced</param>
        /// <returns>True iff the reference can reference the specified target item</returns>
        /// <remarks>This method should never throw any exceptions</remarks>
        bool CanReference(T item);

        /// <summary>
        /// Gets or sets the target item</summary>
        /// <remarks>Callers should always check CanReference before setting this property.
        /// It is up to the implementer to decide whether null is an acceptable value and whether to
        /// throw an exception if the specified value cannot be targeted.</remarks>
        T Target { get; set; }
    }
}
