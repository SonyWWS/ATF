//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// Interface for tool bar models.
    /// Objects which export this interface in the MEF composition container
    /// will be picked up by the application toolBar system
    /// </summary>
    public interface IToolBar
    {
        object Tag { get; }
    }

    /// <summary>
    /// Interface for tool bar item models.
    /// Objects which export this interface in the MEF composition container
    /// will be picked up by the application toolBar system
    /// </summary>
    public interface IToolBarItem
    {
        object Tag { get; }

        object ToolBarTag { get; }

        bool IsVisible { get; }
    }
}
