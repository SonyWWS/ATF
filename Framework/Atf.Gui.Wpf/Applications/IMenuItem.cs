//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Interface for menu items, which may also be menus</summary>
    public interface IMenuItem : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets menu item's text displayed to user</summary>
        string Text { get; set; }

        /// <summary>
        /// Gets or sets menu item's description</summary>
        string Description { get; set; }

        /// <summary>
        /// Gets or sets menu item's unique ID</summary>
        object MenuTag { get; }

        /// <summary>
        /// Gets or sets menu item's unique group ID</summary>
        object GroupTag { get; }
    }
}
