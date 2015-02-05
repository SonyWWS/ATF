//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Sce.Atf.Wpf.Behaviors;

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// Extension methods for visual trees</summary>
    public static class VisualTreeExtensions
    {
        /// <summary>
        /// This is a workaround for a .NET4 bug in which sometimes a sentinel object is
        /// returned from a DataContext of a FrameworkElement.
        /// http://social.msdn.microsoft.com/Forums/nl/wpf/thread/e6643abc-4457-44aa-a3ee-dd389c88bd86 </summary>
        /// <param name="e">FrameworkElement</param>
        /// <returns>DataContext of FrameworkElement</returns>
        public static object SafeGetDataContext(this FrameworkElement e)
        {
            object dataContext = e.DataContext;
            if (s_disconnectedItem != null && dataContext == s_disconnectedItem)
                return null;
            return dataContext;
        }

        private static readonly object s_disconnectedItem = typeof(System.Windows.Data.BindingExpressionBase)
            .GetField("DisconnectedItem", BindingFlags.Static | BindingFlags.NonPublic)
            .GetValue(null);


        /// <summary>
        /// Search through subtree of a FrameworkElement and return the first
        /// FrameworkElement of type T and name found</summary>
        /// <remarks>Recursive function</remarks>
        /// <typeparam name="T">FrameworkElement type to search for</typeparam>
        /// <param name="referenceElement">Subtree to search</param>
        /// <param name="name">Name to search for</param>
        /// <returns>FrameworkElement found or null if not found</returns>
        public static T GetFrameworkElementByName<T>(this FrameworkElement referenceElement, String name)
            where T : FrameworkElement
        {
            // Confirm parent and childName are valid. 
            if (referenceElement == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(referenceElement);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(referenceElement, i) as FrameworkElement;
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = GetFrameworkElementByName<T>(child, name);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(name))
                {
                    // If the child's name is set for search
                    if (child.Name == name)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        /// <summary>
        /// Search through subtree of a DependencyObject and return the first object of given type</summary>
        /// <remarks>Recursive function</remarks>
        /// <typeparam name="T">Type searched for</typeparam>
        /// <param name="parent">DependencyObject to search</param>
        /// <returns>First object of type T or default value for T if not found</returns>
        public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            T child = default(T);
            
            if (parent == null)
                return child;

            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var visualChild = VisualTreeHelper.GetChild(parent, i);
                child = visualChild as T ?? FindVisualChild<T>(visualChild);
                if (child != null)
                    break;
            }

            return child;
        }

        /// <summary>
        /// Search through subtree of a FrameworkElement and return the first object of given type</summary>
        /// <remarks>Recursive function</remarks>
        /// <typeparam name="T">Type searched for</typeparam>
        /// <param name="referenceElement">FrameworkElement to search</param>
        /// <returns>First object of type T or default value for T if not found</returns>
        public static T GetFrameworkElementByType<T>(this FrameworkElement referenceElement)
            where T : FrameworkElement
        {
            T child = default(T);

            if (referenceElement == null)
                return child;

            int childCount = VisualTreeHelper.GetChildrenCount(referenceElement);
            for (int i = 0; i < childCount; i++)
            {
                var visualChild = VisualTreeHelper.GetChild(referenceElement, i) as FrameworkElement;
                child = visualChild as T ?? GetFrameworkElementByType<T>(visualChild);
                if (child != null)
                    break;
            }

            return child;
        }

        /// <summary>
        /// Search through subtree of a FrameworkElement and return enumeration of objects of given type</summary>
        /// <remarks>Recursive function</remarks>
        /// <typeparam name="T">Type searched for</typeparam>
        /// <param name="referenceElement">FrameworkElement to search</param>
        /// <returns>Enumeration of objects of type T</returns>
        public static IEnumerable<T> GetFrameworkElementsByType<T>(this FrameworkElement referenceElement)
           where T : FrameworkElement
        {
            FrameworkElement child = null;

            for (Int32 i = 0; i < VisualTreeHelper.GetChildrenCount(referenceElement) && child == null; i++)
            {
                child = VisualTreeHelper.GetChild(referenceElement, i) as FrameworkElement;

                if (child != null && child.GetType() == typeof(T))
                    yield return child as T;

                foreach (var item in GetFrameworkElementsByType<T>(child))
                    yield return item;
            }
        }

        /// <summary>
        /// Find nearest ancestor of given type of DependencyObject</summary>
        /// <typeparam name="T">Type searched for</typeparam>
        /// <param name="obj">DependencyObject searched</param>
        /// <returns>Nearest ancestor of given type</returns>
        public static T FindParent<T>(this DependencyObject obj) where T : DependencyObject
        {
            return obj == null ? null : obj.GetAncestors().OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// Enumerate all ancestors of a DependencyObject</summary>
        /// <remarks>Enumeration includes element itself</remarks>
        /// <param name="element">DependencyObject whose ancestors are obtained</param>
        /// <returns>Enumeration of all object's ancestors, including itself</returns>
        public static IEnumerable<DependencyObject> GetAncestors(this DependencyObject element)
        {
            Requires.NotNull(element, "element");
            
            do
            {
                yield return element;
                element = VisualTreeHelper.GetParent(element);
            }
            while (element != null);
        }

        /// <summary>
        /// Search up through the visual hierarchy and return the first DependencyObject of type T found</summary>
        /// <typeparam name="T">Type searched for</typeparam>
        /// <param name="dep">DependencyObject to search</param>
        /// <returns>First DependencyObject of type T found</returns>
        public static T FindAncestor<T>(this DependencyObject dep)
            where T : class
        {
            return dep.GetLineage().FirstOrDefault(x => x is T) as T;
        }

        /// <summary>
        /// Enumerate the lineage of a DependencyObject from both visual and logical trees</summary>
        /// <param name="dep">DependencyObject to determine lineage of</param>
        /// <returns>Enumeration of lineage DependencyObjects from both visual and logical trees, including the object itself</returns>
        public static IEnumerable<DependencyObject> GetLineage(this DependencyObject dep)
        {
            DependencyObject current = dep;

            while (current != null)
            {
                yield return current;
                current = current.GetVisualOrLogicalParent();
            }
        }


        /// <summary>
        /// Enumerate the subtree of a DependencyObject in post-order (breadth first)</summary>
        /// <param name="dep">DependencyObject</param>
        /// <returns>Enumeration of DependencyObjects</returns>
        public static IEnumerable<DependencyObject> GetSubtree(this DependencyObject dep)
        {
            var nodes = new Queue<DependencyObject>();
            nodes.Enqueue(dep);

            while (nodes.Count > 0)
            {
                var node = nodes.Dequeue();
                yield return node;

                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(node, i);
                    if (child != null)
                        nodes.Enqueue(child);
                }
            }
        }

        /// <summary>
        /// Get DependencyObject parent in visual or logical tree</summary>
        /// <param name="dep">DependencyObject to get parent for</param>
        /// <returns>DependencyObject parent, which may be in visual or logical tree. If no parent, return null.</returns>
        public static DependencyObject GetVisualOrLogicalParent(this DependencyObject dep)
        {
            if (dep == null)
                return null;

            var ce = dep as ContentElement;
            if (ce != null)
            {
                var parent = ContentOperations.GetParent(ce);
                if (parent != null)
                    return parent;

                var fce = ce as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }

            if (dep is Visual || dep is Visual3D)
                return VisualTreeHelper.GetParent(dep);

            // If we're in Logical Land then we must walk 
            // up the logical tree until we find a 
            // Visual/Visual3D to get us back to Visual Land.
            return LogicalTreeHelper.GetParent(dep);
        }

        /// <summary>
        /// Find the item container from a given item in an ItemsControl ancestor, searching up the tree</summary>
        /// <remarks>Throws <see cref="ArgumentNullException"/> if itemsControl or child is null</remarks>
        /// <param name="itemsControl">Control ancestor container</param>
        /// <param name="child">Item</param>
        /// <returns>Item container from given item, which may be the item. If itemsControl empty, return null.</returns>
        public static UIElement GetItemContainerFromChildElement(ItemsControl itemsControl, UIElement child)
        {
            Requires.NotNull(itemsControl, "itemsControl");
            Requires.NotNull(child, "child");

            if (itemsControl.Items.Count > 0)
            {
                // find the ItemsPanel
                Panel panel = VisualTreeHelper.GetParent(itemsControl.ItemContainerGenerator.ContainerFromIndex(0)) as Panel;

                if (panel != null)
                {
                    // Walk the tree until we get to the ItemsPanel, once we get there we know
                    // that the immediate child of the parent is going to be the ItemContainer

                    UIElement parent;
                    do
                    {
                        parent = VisualTreeHelper.GetParent(child) as UIElement;
                        if (parent == panel)
                        {
                            return child;
                        }
                        child = parent;
                    }
                    while (parent != null);
                }
            }
            
            return null;
        }

        /// <summary>
        /// Get generated DependencyObject at mouse pointer in an ItemsControl, searching up the tree to find a generated object</summary>
        /// <param name="parent">ItemsControl containing object</param>
        /// <returns>DependencyObject at a point or null if no object at point or object not generated</returns>
        public static object GetItemAtMousePoint(this ItemsControl parent)
        {
            // Dan - this is a fix to fix a null reference exception caused when switching
            // tabs in tab control - the parent item here can end up not being in the visual 
            // tree
            PresentationSource inputSource = PresentationSource.FromVisual(parent);
            if (inputSource != null)
            {
                return parent.GetItemAtPoint(MouseUtilities.CorrectGetPosition(parent));
            }
            return null;
        }

        /// <summary>
        /// Get item container at point in an ItemsControl, searching up the tree to find a generated object</summary>
        /// <remarks>The similar function GetItemAtPoint() returns a data item rather than an item container</remarks>
        /// <param name="itemsControl">ItemsControl containing item container</param>
        /// <param name="p">Point</param>
        /// <returns>Item container at point or null if no item at point or item not generated</returns>
        public static DependencyObject GetItemContainerAtPoint(this ItemsControl itemsControl, Point p)
        {
            if (itemsControl is TreeView)
                return ((TreeView)itemsControl).GetItemContainerAtPoint(p);

            object item = itemsControl.GetItemAtPoint(p);
            if (item != null)
                return itemsControl.ItemContainerGenerator.ContainerFromItem(item);

            return null;
        }

        /// <summary>
        /// Get data item at a point in an ItemsControl, searching up the tree to find a generated object</summary>
        /// <remarks>The similar function GetItemContainerAtPoint() returns an item container rather than a data item</remarks>
        /// <param name="itemsControl">ItemsControl containing data item</param>
        /// <param name="p">Point</param>
        /// <returns>Data item at a point or null if no object at point or object not generated</returns>
        public static object GetItemAtPoint(this ItemsControl itemsControl, Point p)
        {
            if (itemsControl is TreeView)
                return ((TreeView)itemsControl).GetItemAtPoint(p);

            var result = VisualTreeHelper.HitTest(itemsControl, p);
            if (result != null)
            {
                var dep = result.VisualHit;
                if (dep != null)
                {
                    while (dep != null && dep != itemsControl)
                    {
                        object data = itemsControl.ItemContainerGenerator.ItemFromContainer(dep);
                        if (data != DependencyProperty.UnsetValue)
                            return data;

                        dep = LogicalTreeHelper.GetParent(dep) ?? VisualTreeHelper.GetParent(dep);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get TreeView data item in TreeView at point, searching up the tree</summary>
        /// <remarks>The similar function GetItemContainerAtPoint() returns an item container rather than a data item</remarks>
        /// <param name="treeView">TreeView</param>
        /// <param name="p">Point</param>
        /// <returns>TreeView data item in TreeView at point or null if no object at point</returns>
        public static object GetItemAtPoint(this TreeView treeView, Point p)
        {
            var tvi = treeView.GetItemContainerAtPoint(p);

            ItemsControl parent = ItemsControl.ItemsControlFromItemContainer(tvi);

            if (parent != null)
            {
                object data = parent.ItemContainerGenerator.ItemFromContainer(tvi);
                if (data != DependencyProperty.UnsetValue)
                {
                    return data;
                }
            }

            return null;
        }

        /// <summary>
        /// Get TreeView item container in TreeView at point, searching up the tree</summary>
        /// <remarks>The similar function GetItemAtPoint() returns a data item rather than an item container</remarks>
        /// <param name="treeView">TreeView</param>
        /// <param name="p">Point</param>
        /// <returns>TreeViewItem in TreeView at point or null if no object at point</returns>
        public static TreeViewItem GetItemContainerAtPoint(this TreeView treeView, Point p)
        {
            var dep = treeView.InputHitTest(p) as DependencyObject;
            if (dep != null)
            {
                return dep.FindAncestor<TreeViewItem>();
            }

            return null;
        }

    }
}
