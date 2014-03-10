//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;

using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models
{
    internal class MenuUtil
    {
        public static void BuildSubMenus(ICommandItem command, Menu menu)
        {
            var subMenus = menu.ChildCollection;

            foreach (var segment in command.MenuPath)
            {
                // Try and find an existing submenu
                var subMenu = (Menu)subMenus.FirstOrDefault<IMenuItem>(x => (x is Menu) && (((Menu)x).Text == segment));
                if (subMenu == null)
                {
                    // No existing submenu found - add a new one
                    subMenu = new Menu(menu, command.MenuTag, null, segment, segment);
                    subMenus.Add(subMenu);
                }
                subMenus = subMenu.ChildCollection;
                menu = subMenu;
            }

            subMenus.Add(command);
        }

        public static void InsertGroupSeparators(Menu menu)
        {
            // Create depth first list of commands
            var commands = new List<Tuple<ICommandItem, Menu>>();
            GetCommands(menu, commands);

            for (int i = 1; i < commands.Count; i++)
            {
                var previous = commands[i - 1];
                var current = commands[i];

                if (!CommandComparer.TagsEqual(previous.Item1.GroupTag, current.Item1.GroupTag))
                {
                    InsertSeparator(previous, current);
                }
            }
        }

        private static void InsertSeparator(Tuple<ICommandItem, Menu> previous, Tuple<ICommandItem, Menu> current)
        {
            // Get lineage of each
            IMenu[] previousLineage = previous.Item2.Lineage().Reverse<IMenu>().ToArray<IMenu>();
            IMenu[] currentLineage = current.Item2.Lineage().Reverse<IMenu>().ToArray<IMenu>();

            // Find lowest common ancestor
            // (Assumes common ancestor exists)
            int minLength = Math.Min(previousLineage.Length, currentLineage.Length);

            IMenuItem insertBefore = null;
            IList<IMenuItem> collection = null;
        
            for (int i = 0; i < minLength; i++)
            {
                if (previousLineage[i] != currentLineage[i])
                {
                    insertBefore = currentLineage[i];
                    collection = ((Menu)currentLineage[i - 1]).ChildCollection;
                }
            }

            if (insertBefore == null)
            {
                insertBefore = minLength < currentLineage.Length ? currentLineage[minLength] as IMenuItem
                    : current.Item1;
                collection = ((Menu)currentLineage[minLength - 1]).ChildCollection;
            }

            int idx = collection.IndexOf(insertBefore);
            System.Diagnostics.Debug.Assert(idx >= 0);
            collection.Insert(idx, new Separator());
        }

        private static void GetCommands(Menu menu, List<Tuple<ICommandItem, Menu>> commands)
        {
            foreach (var child in menu.ChildCollection)
            {
                if (child is ICommandItem)
                {
                    commands.Add(new Tuple<ICommandItem, Menu>((ICommandItem)child, menu));
                }
                else if (child is Menu)
                {
                    GetCommands((Menu)child, commands);
                }
            }
        }

    }
}
