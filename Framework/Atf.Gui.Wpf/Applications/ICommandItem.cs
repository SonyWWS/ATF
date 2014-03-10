//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Windows.Input;

using Sce.Atf.Applications;

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
        /// Gets or sets a sequence of input device gestures to execute the command</summary>
        InputGesture[] InputGestures { get; set; }

        /// <summary>
        /// Gets where command is visible, as on menus, toolbars, etc.</summary>
        CommandVisibility Visibility { get; }

        /// <summary>
        /// Gets strings describing menu path</summary>
        IEnumerable<string> MenuPath { get; }
    }

    /// <summary>
    /// ICommandItem extension methods</summary>
    public static class CommandItemExtensions
    {
        /// <summary>
        /// Tests whether command is visible or not</summary>
        /// <param name="cmd">ICommandItem for command</param>
        /// <param name="visibility">Where command is visible, as on menus, toolbars, etc.</param>
        /// <returns>True iff command is visible</returns>
        public static bool IsVisible(this ICommandItem cmd, CommandVisibility visibility)
        {
            return (cmd.Visibility & visibility) > 0;
        }
    }
}
