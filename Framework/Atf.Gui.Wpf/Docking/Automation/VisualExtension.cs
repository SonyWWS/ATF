using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Docking.Automation
{
    public static class VisualExtension
    {
        /// <summary>
        /// Search for an element of a certain type in the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of element to find.</typeparam>
        /// <param name="visual">The parent element.</param>
        /// <returns></returns>
        public static T FindVisualChild<T>(this DependencyObject visual) where T : DependencyObject
        {
            // Pre-requisite
            if (visual == null)
            {
                return null;
            }

            // Find VisuallChild recursively
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++)
            {
                var child = VisualTreeHelper.GetChild(visual, i);
                if (child != null)
                {
                    T correctlyTyped = child as T;
                    if (correctlyTyped != null)
                    {
                        return correctlyTyped;
                    }

                    T descendent = child.FindVisualChild<T>();
                    if (descendent != null)
                    {
                        return descendent;
                    }
                }
            }

            return null;
        }
        
        /// <summary>
        /// Search for an element of a certain type in the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of element to find.</typeparam>
        /// <param name="visual">The parent element.</param>
        /// <returns></returns>
        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject visual) where T : DependencyObject
        {
            // Find VisuallChild recursively
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++)
            {
                var child = VisualTreeHelper.GetChild(visual, i);   
                             
                T correctlyTyped = child as T;
                if (correctlyTyped != null)
                {
                    yield return correctlyTyped;
                }

                foreach (var descendant in child.FindVisualChildren<T>())
                {
                    yield return descendant.FindVisualChild<T>();
                }
            }
        }
    }
}
