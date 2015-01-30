//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// Interface for root menu models.
    /// Objects which export this interface in the MEF composition container
    /// will be picked up by the application menu system</summary>
    public interface IMenu
    {
        /// <summary>
        /// Get menu tag object</summary>
        object MenuTag { get; }

        /// <summary>
        /// Get menu text string</summary>
        string Text { get; }

        /// <summary>
        /// Get menu description text string</summary>
        string Description { get; }
    }
}
