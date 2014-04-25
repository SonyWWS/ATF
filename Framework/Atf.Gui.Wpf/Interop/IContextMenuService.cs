//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Sce.Atf.Wpf.Interop
{
    /// <summary>
    /// Service to create context menus</summary>
    public interface IContextMenuService
    {
        /// <summary>
        /// Returns a context menu containing commands with commandTags</summary>
        /// <param name="commandTags">Command tags for commands to include on menu</param>
        /// <returns>ContextMenu</returns>
        ContextMenu GetContextMenu(IEnumerable<object> commandTags);

        /// <summary>
        /// Gets or sets auto menu compacting in which only commands that can execute are displayed</summary>
        bool AutoCompact { get; set; }
    }

    /// <summary>
    /// Class with useful static methods for IContextMenuService</summary>
    public static class ContextMenuServiceExtensions
    {
        /// <summary>
        /// Runs context menu at current mouse position</summary>
        /// <param name="service">Context menu service</param>
        /// <param name="commandTags">Command tags for commands to include on menu</param>
        public static void RunContextMenu(this IContextMenuService service, IEnumerable<object> commandTags)
        {
            var menu = service.GetContextMenu(commandTags);
            menu.Placement = PlacementMode.MousePoint;
            OpenContextMenuIfNotEmpty(menu);
        }

        /// <summary>
        /// Runs context menu at screen offset (device independant pixels)</summary>
        /// <param name="service">Context menu service</param>
        /// <param name="commandTags">Command tags for commands to include on menu</param>
        /// <param name="screenOffset">Screen offset</param>
        public static void RunContextMenu(this IContextMenuService service, IEnumerable<object> commandTags, Point screenOffset)
        {
            var menu = service.GetContextMenu(commandTags);
            menu.Placement = PlacementMode.AbsolutePoint;
            menu.HorizontalOffset = screenOffset.X;
            menu.VerticalOffset = screenOffset.Y;
            OpenContextMenuIfNotEmpty(menu);
        }

        /// <summary>
        /// Runs context menu at offset to provided UIElement (device independant pixels)</summary>
        /// <param name="service">Context menu service</param>
        /// <param name="commandTags">Command tags for commands to include on menu</param>
        /// <param name="element">UIElement for positioning</param>
        /// <param name="offset">Screen offset</param>
        public static void RunContextMenu(this IContextMenuService service, IEnumerable<object> commandTags, UIElement element, Point offset)
        {
            var menu = service.GetContextMenu(commandTags);
            menu.Placement = PlacementMode.Relative;
            menu.PlacementTarget = element;
            menu.HorizontalOffset = offset.X;
            menu.VerticalOffset = offset.Y;
            OpenContextMenuIfNotEmpty(menu);
        }

        private static void OpenContextMenuIfNotEmpty(ContextMenu menu)
        {
            foreach (var item in menu.ItemsSource)
            {
                menu.IsOpen = true;
                return;
            }
        }
    }
}
