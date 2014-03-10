//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf
{
    /// <summary>
    /// Interface for a context that can provide an enumeration of all of its items.
    /// Complements ISelectionContext. See StandardSelectionCommands or StandardViewCommands.</summary>
    public interface IEnumerableContext
    {
        /// <summary>
        /// Gets an enumeration of all of the items of this context</summary>
        IEnumerable<object> Items
        {
            get;
        }
    }
}
