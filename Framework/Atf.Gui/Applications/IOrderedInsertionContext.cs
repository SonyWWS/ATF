// See License.txt.

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for contexts that can insert new objects (e.g., via drag and drop) into a
    /// particular order relative to other objects. Works with both hierarchical and non-
    /// hierarchical data views.</summary>
    /// <remarks>Normally implemented along with IInstancingContext. Used by TreeControlEditor,
    /// for example, to allow specific tree editors like the ProjectLister to let the user
    /// reorder items in a list.</remarks>
    public interface IOrderedInsertionContext
    {
        /// <summary>
        /// Returns true if 'item' can be inserted.</summary>
        /// <param name="parent">The object that will become the parent of the inserted object.
        /// Can be null if the list of objects is a flat list or if the root should be replaced.</param>
        /// <param name="before">The object that is immediately before the inserted object.
        /// Can be null to indicate that the inserted item should become the first child.</param>
        /// <param name="item">The item to be inserted. Consider using Util.ConvertData(item, false)
        /// to retrieve the final one or more items to be inserted.</param>
        /// <returns>True iff 'item' can be successfully inserted</returns>
        bool CanInsert(object parent, object before, object item);

        /// <summary>
        /// Inserts 'item' into the set of objects at the desired position. Can only be called
        /// if CanInsert() returns true.</summary>
        /// <param name="parent">The object that will become the parent of the inserted object.
        /// Can be null if the list of objects is a flat list or if the root should be replaced.</param>
        /// <param name="before">The object that is immediately before the inserted object.
        /// Can be null to indicate that the inserted item should become the first child.</param>
        /// <param name="item">The item to be inserted. Consider using Util.ConvertData(item, false)
        /// to retrieve the final one or more items to be inserted.</param>
        void Insert(object parent, object before, object item);
    }
}
