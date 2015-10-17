//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Windows.Input;

using Sce.Atf.Applications;
using Sce.Atf.Input;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Interface for command items, which represent a command</summary>
    public interface ICommandItem : IMenuItem, ICommand
    {
        /// <summary>
        /// Gets the unique ID for the command</summary>
        object CommandTag { get; }

        /// <summary>
        /// Gets or sets whether the command is checked</summary>
        bool IsChecked { get; set; }

        /// <summary>
        /// Gets or sets image resource for command</summary>
        object ImageSourceKey { get; set; }

        /// <summary>
        /// Get command keyboard shortcuts</summary>
        IEnumerable<Keys> Shortcuts { get; set; }

        /// <summary>
        /// Gets where command is visible, as on menus, toolbars, etc.</summary>
        CommandVisibility Visibility { get; }

        /// <summary>
        /// Get command index</summary>
        int Index { get; }
    }

    /// <summary>
    /// ICommandItem extension methods</summary>
    public static class CommandItemExtensions
    {
        /// <summary>
        /// Tests whether command is visible or not</summary>
        /// <param name="cmd">ICommandItem for command</param>
        /// <param name="visibility">Where command is visible, as on menus, toolbars, etc.</param>
        /// <returns><c>True</c> if command is visible</returns>
        public static bool IsVisible(this ICommandItem cmd, CommandVisibility visibility)
        {
            return (cmd.Visibility & visibility) > 0;
        }
    }
}
