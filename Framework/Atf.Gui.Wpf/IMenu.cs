//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// Interface for root menu models.
    /// Objects which export this interface in the MEF composition container
    /// will be picked up by the application menu system
    /// </summary>
    public interface IMenu
    {
        object MenuTag { get; }

        string Text { get; }

        string Description { get; }
    }
}
