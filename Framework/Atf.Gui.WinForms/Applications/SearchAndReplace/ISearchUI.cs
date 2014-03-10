//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for client-defined UI that specifically provides user controls for specifying 
    /// search criteria, and for triggering a search on that criteria</summary>
    public interface ISearchUI : ISearchableContextUI
    {
        /// <summary>
        /// Assigns the queryable content on which the user-defined search is made</summary>
        /// <param name="queryableContext">Interface to data set on which queries can be made</param>
        void Bind(IQueryableContext queryableContext);
    }
}