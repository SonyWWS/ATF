//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// Visual tree utilities</summary>
    public static class VisualTreeUtil
    {
        /// <summary>
        /// Gets the first child encountered of given type T</summary>
        /// <typeparam name="T">Type of object to search for</typeparam>
        /// <param name="referenceElement">The element from which to begin the search</param>
        /// <returns>The first child of referenceElement encountered of type T</returns>
        public static T GetFrameworkElementByType<T>(this FrameworkElement referenceElement)
            where T : FrameworkElement
        {
            FrameworkElement child = null;

            for (Int32 i = 0; i < VisualTreeHelper.GetChildrenCount(referenceElement) && child == null; i++)
            {
                child = VisualTreeHelper.GetChild(referenceElement, i) as FrameworkElement;
                if (child != null && child.GetType() == typeof(T))
                    break;

                if (child != null)
                    child = GetFrameworkElementByType<T>(child);
            }

            return child as T;
        }
        
        /// <summary>
        /// Searches up through the visual hierarchy and returns the first DependencyObject
        /// of type T found</summary>
        /// <typeparam name="T">Type of object to search for</typeparam>
        /// <param name="dep">DependencyObject from which to begin the search</param>
        /// <returns>First DependencyObject of type T found</returns>
        public static T FindAncestor<T>(this DependencyObject dep)
            where T : class
        {
            DependencyObject current = dep;

            while (current != null)
            {
                T result = current as T;
                if (result != null)
                {
                    return result;
                }

                if (current is Visual || current is Visual3D)
                {
                    current = VisualTreeHelper.GetParent(current);
                }
                else
                {
                    // If we're in Logical Land then we must walk 
                    // up the logical tree until we find a 
                    // Visual/Visual3D to get us back to Visual Land.
                    current = LogicalTreeHelper.GetParent(current);
                }
            }
            
            return null;
        }
    }
}
