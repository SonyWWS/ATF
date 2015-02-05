//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// Interface for tool bar models.
    /// Objects which export this interface in the MEF composition container
    /// will be picked up by the application toolBar system</summary>
    public interface IToolBar
    {
        /// <summary>
        /// Get tool bar tag object</summary>
        object Tag { get; }
    }

    /// <summary>
    /// Interface for tool bar item models.
    /// Objects which export this interface in the MEF composition container
    /// will be picked up by the application toolBar system</summary>
    public interface IToolBarItem
    {
        /// <summary>
        /// Get tool bar item tag object</summary>
        object Tag { get; }

        /// <summary>
        /// Get tool bar item's tool bar tag object</summary>
        object ToolBarTag { get; }

        /// <summary>
        /// Get whether tool bar item visible</summary>
        bool IsVisible { get; }
    }
}
