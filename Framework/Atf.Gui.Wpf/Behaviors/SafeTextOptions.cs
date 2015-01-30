//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Wrapper for .NET 4 TextOptions which allows for backward compatibility 
    /// with older .NET versions by ignoring these options.
    /// This class is required as there is no way of doing conditional compilation in XAML</summary>
    public class CS4Options
    {
        #region DisplayMode Attached Property

        /// <summary>
        /// Display mode dependency property</summary>
        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.RegisterAttached("DisplayMode", typeof(bool), typeof(CS4Options), new PropertyMetadata(default(bool), DisplayModePropertyChanged));

        /// <summary>
        /// Set display mode dependency property</summary>
        /// <param name="element">Element to set</param>
        /// <param name="value">Value to set</param>
        public static void SetDisplayMode(UIElement element, bool value)
        {
            element.SetValue(DisplayModeProperty, value);
        }

        /// <summary>
        /// Get display mode dependency property</summary>
        /// <param name="element">Element to get property from</param>
        /// <returns>Value of display mode dependency property</returns>
        public static bool GetDisplayMode(UIElement element)
        {
            return (bool)element.GetValue(DisplayModeProperty);
        }

        private static void DisplayModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextOptions.SetTextFormattingMode(d, TextFormattingMode.Display);
        }

        #endregion

        #region UseLayoutRounding Attached Property

        /// <summary>
        /// Whether to use layout rounding dependency property</summary>
        public static readonly DependencyProperty UseLayoutRoundingProperty =
            DependencyProperty.RegisterAttached("UseLayoutRounding", typeof(bool), typeof(CS4Options), new PropertyMetadata(default(bool), UseLayoutRoundingPropertyChanged));

        /// <summary>
        /// Set layout rounding dependency property</summary>
        /// <param name="element">Element to set</param>
        /// <param name="value">Value to set</param>
        public static void SetUseLayoutRounding(UIElement element, bool value)
        {
            element.SetValue(UseLayoutRoundingProperty, value);
        }

        /// <summary>
        /// Get layout rounding dependency property</summary>
        /// <param name="element">Element to get property from</param>
        /// <returns>Value of layout rounding dependency property</returns>
        public static bool GetUseLayoutRounding(UIElement element)
        {
            return (bool)element.GetValue(UseLayoutRoundingProperty);
        }

        private static void UseLayoutRoundingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(FrameworkElement.UseLayoutRoundingProperty, e.NewValue);
        }

        #endregion
    }
}
