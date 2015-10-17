//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf
{
    /// <summary>
    /// Interface for contexts where items may be locked so they can't be modified</summary>
    /// <remarks>Is designed for locking individual objects within a document. See StandardLockCommands.
    /// For locking documents, see ISourceControlContext and SourceControlCommands.</remarks>
    public interface ILockingContext
    {
        /// <summary>
        /// Returns whether the item is locked</summary>
        /// <param name="item">Item</param>
        /// <returns><c>True</c> if the item is locked</returns>
        bool IsLocked(object item);

        /// <summary>
        /// Returns whether the item can be locked and unlocked</summary>
        /// <param name="item">Item</param>
        /// <returns><c>True</c> if the item item can be locked and unlocked</returns>
        bool CanSetLocked(object item);

        /// <summary>
        /// Sets the item's locked state to the value</summary>
        /// <param name="item">Item to lock or unlock</param>
        /// <param name="value">True to lock, false to unlock</param>
        void SetLocked(object item, bool value);
    }
}
