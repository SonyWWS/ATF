//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface defining a view on hierarchical data. Note that, depending on the
    /// implementation, the tree may be a directed acyclic graph or a directed
    /// graph. When used with TreeControlAdapter, consider also implementing IItemView,
    /// IObservableContext, IValidationContext, ISelectionContext, IInstancingContext,
    /// and IHierarchicalInsertionContext.</summary>
    public interface ITreeView
    {
        /// <summary>
        /// Gets the root object of the tree view</summary>
        object Root
        {
            get;
        }

        /// <summary>
        /// Obtains enumeration of the children of the given parent object</summary>
        /// <param name="parent">Parent object</param>
        /// <returns>Enumeration of children of the parent object</returns>
        IEnumerable<object> GetChildren(object parent);
    }
}
