//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf
{
    /// <summary>
    /// Interface for user-pinnable items in a list. For example, PinnableActiveCollection
    /// looks for this interface and RecentDocumentInfo implements it.</summary>
    public interface IPinnable
    {
        /// <summary>
        /// Gets or sets whether this item is pinned.</summary>
        bool Pinned { get; set; }
    }
}
