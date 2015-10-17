//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// GridView column header behavior</summary>
    public static class GridViewColumnHeaderBehavior
    {
         /// <summary>
        /// Attached property to indicate IsResizable</summary>
        public static DependencyProperty IsResizableProperty = DependencyProperty.RegisterAttached(
            "IsResizable", typeof(bool), typeof(GridViewColumnHeaderBehavior), new PropertyMetadata(true, OnIsResizableChanged));

        /// <summary>
        /// Sets whether element is resizable</summary>
        /// <param name="element">Dependency object to set property for</param>
        /// <param name="value">Whether element is resizable</param>
        public static void SetIsResizable(DependencyObject element, bool value)
        {
            element.SetValue(IsResizableProperty, value);
        }

        /// <summary>
        /// Gets whether element is resizable</summary>
        /// <param name="element">Dependency object to obtain property for</param>
        /// <returns><c>True</c> if element is resizable</returns>
        public static bool GetIsResizable(DependencyObject element)
        {
            return (bool)element.GetValue(IsResizableProperty);
        }

        private static void OnIsResizableChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            // Find header gripper and set hidden
        }

        /// <summary>
        /// Attached property to indicate IsClickable.
        /// When this is set to false, the GridViewColumnHeader control template disables
        /// mouse interactivity effects, such as clicking and mouse over effects.</summary>
        public static DependencyProperty IsClickableProperty = DependencyProperty.RegisterAttached(
            "IsClickable", typeof(bool), typeof(GridViewColumnHeaderBehavior), new PropertyMetadata(true));

        /// <summary>
        /// Sets whether element is clickable</summary>
        /// <param name="element">Dependency object to set property for</param>
        /// <param name="value">Whether element is clickable</param>
        public static void SetIsClickable(DependencyObject element, bool value)
        {
            element.SetValue(IsClickableProperty, value);
        }

        /// <summary>
        /// Gets whether element is clickable</summary>
        /// <param name="element">Dependency object to obtain property for</param>
        /// <returns><c>True</c> if element is clickable</returns>
        public static bool GetIsClickable(DependencyObject element)
        {
            return (bool)element.GetValue(IsClickableProperty);
        }
    }
}
