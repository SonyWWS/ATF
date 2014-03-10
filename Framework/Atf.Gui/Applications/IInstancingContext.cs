//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for contexts that can instance objects; instancing requires the ability to
    /// copy, insert, and delete items</summary>
    /// <remarks>Consider implementing IHierarchicalInsertionContext, too, if this context
    /// has data that is exposed to any kind of tree control, like the ProjectLister.
    /// In a WinForms app, the 'object' type is probably System.Windows.Forms.IDataObject,
    /// and in a WPF app, this object may be a System.Windows.IDataObject.</remarks>
    public interface IInstancingContext
    {
        /// <summary>
        /// Returns whether the context can copy the selection</summary>
        /// <returns>True iff the context can copy</returns>
        bool CanCopy();

        /// <summary>
        /// Copies the selection. Returns a data object representing the copied items.</summary>
        /// <returns>Data object representing the copied items; e.g., a
        /// System.Windows.Forms.IDataObject object</returns>
        object Copy();

        /// <summary>
        /// Returns whether the context can insert the data object</summary>
        /// <param name="dataObject">Data to insert; e.g., System.Windows.Forms.IDataObject</param>
        /// <returns>True iff the context can insert the data object</returns>
        /// <remarks>ApplicationUtil calls this method in its CanInsert method, BUT
        /// if the context also implements IHierarchicalInsertionContext,
        /// IHierarchicalInsertionContext is preferred and the IInstancingContext
        /// implementation is ignored for insertion.</remarks>
        bool CanInsert(object dataObject);

        /// <summary>
        /// Inserts the data object into the context</summary>
        /// <param name="dataObject">Data to insert; e.g., System.Windows.Forms.IDataObject</param>
        /// <remarks>ApplicationUtil calls this method in its Insert method, BUT
        /// if the context also implements IHierarchicalInsertionContext,
        /// IHierarchicalInsertionContext is preferred and the IInstancingContext
        /// implementation is ignored for insertion.</remarks>
        void Insert(object dataObject);

        /// <summary>
        /// Returns whether the context can delete the selection</summary>
        /// <returns>True iff the context can delete</returns>
        bool CanDelete();

        /// <summary>
        /// Deletes the selection</summary>
        void Delete();
    }
}
