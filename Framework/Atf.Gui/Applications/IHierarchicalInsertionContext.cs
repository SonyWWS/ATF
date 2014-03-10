//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for contexts that can insert new objects (e.g., via drag and drop) under
    /// a specific parent object</summary>
    /// <remarks>Normally implemented along with IInstancingContext. Used by the TreeControlEditor sample,
    /// for example, to allow specific tree editors like the ProjectLister to tell the context
    /// which node a user has dragged an object to.
    /// This context is used by ApplicationUtil's CanInsert and Insert methods and preferred 
    /// over IInstancingContext if both are implemented.</remarks>
    public interface IHierarchicalInsertionContext
    {
        /// <summary>
        /// Returns true if context can insert the child object</summary>
        /// <param name="parent">The proposed parent of the object to insert</param>
        /// <param name="child">Child to insert</param>
        /// <returns>True iff the context can insert the child</returns>
        bool CanInsert(object parent, object child);

        /// <summary>
        /// Inserts the child object into the context</summary>
        /// <param name="parent">The parent of the object to insert</param>
        /// <param name="child">Data to insert</param>
        void Insert(object parent, object child);
    }
}
