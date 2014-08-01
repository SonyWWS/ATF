//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// Static helper functions for working with TreeViews</summary>
    public static class TreeViewExtensions
    {
        /// <summary>
        /// Apply an action to all treeview items. Does not work for virtualized treeviews</summary>
        /// <param name="itemAction">The action to perform</param>
        /// <param name="itemsControl">The tree of items</param>
        public static void ApplyActionToAllTreeViewItems(Action<TreeViewItem> itemAction, ItemsControl itemsControl)
        {
            Stack<ItemsControl> itemsControlStack = new Stack<ItemsControl>();
            itemsControlStack.Push(itemsControl);

            while (itemsControlStack.Count != 0)
            {
                ItemsControl currentItem = itemsControlStack.Pop() as ItemsControl;
                TreeViewItem currentTreeViewItem = currentItem as TreeViewItem;
                if (currentTreeViewItem != null)
                {
                    itemAction(currentTreeViewItem);
                }
                if (currentItem != null) // this handles the scenario where some TreeViewItems are already collapsed
                {
                    foreach (object dataItem in currentItem.Items)
                    {
                        ItemsControl childElement = (ItemsControl)currentItem.ItemContainerGenerator.ContainerFromItem(dataItem);
                        itemsControlStack.Push(childElement);
                    }
                }
            }
        }

        /// <summary>
        /// Get all treeview items - does not work for virtualized treeviews!</summary>
        /// <param name="itemsControl">The tree of items</param>
        /// <returns>The list of tree view items</returns>
        public static IEnumerable<TreeViewItem> AllTreeViewItems(ItemsControl itemsControl)
        {
            Stack<ItemsControl> itemsControlStack = new Stack<ItemsControl>();
            itemsControlStack.Push(itemsControl);

            while (itemsControlStack.Count != 0)
            {
                ItemsControl currentItem = itemsControlStack.Pop() as ItemsControl;
                TreeViewItem currentTreeViewItem = currentItem as TreeViewItem;
                if (currentTreeViewItem != null)
                {
                    yield return currentTreeViewItem;
                }
                if (currentItem != null) // this handles the scenario where some TreeViewItems are already collapsed
                {
                    foreach (object dataItem in currentItem.Items)
                    {
                        ItemsControl childElement = (ItemsControl)currentItem.ItemContainerGenerator.ContainerFromItem(dataItem);
                        itemsControlStack.Push(childElement);
                    }
                }
            }
        }

        /// <summary>
        /// Recursively search for a visible treeview item from its data item
        /// Will return null if the treeview item has not been generated </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static TreeViewItem GetTreeViewItem(ItemsControl container, object item)
        {
            return AllTreeViewItems(container).FirstOrDefault<TreeViewItem>(x=>x.DataContext == item);
        }

    }
}
