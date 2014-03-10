//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Interface for menus</summary>
    public interface IMenu : IMenuItem
    {
        /// <summary>
        /// Gets enumeration of menu's children menu items</summary>
        IEnumerable<IMenuItem> Children { get; }

        /// <summary>
        /// Gets enumeration of commands under the menu</summary>
        IEnumerable<ICommandItem> CommandsInSubtree { get; }

        /// <summary>
        /// Gets menu's parent</summary>
        IMenu Parent { get; }
    }

    /// <summary>
    /// IMenu extension methods</summary>
    public static class MenuExtensions
    {
        /// <summary>
        /// Obtains enumeration of menu's lineage, i.e., its parent, grandparent, etc.</summary>
        /// <param name="menu">Menu</param>
        /// <returns>Enumeration of menu's lineage</returns>
        public static IEnumerable<IMenu> Lineage(this IMenu menu)
        {
            while (menu != null)
            {
                yield return menu;
                menu = menu.Parent;
            }
        }
    }
}
