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
    /// This class is required as there is no way of doing conditional compilation in xaml
    /// </summary>
    public class CS4Options
    {
        #region DisplayMode Attached Property

        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.RegisterAttached("DisplayMode", typeof(bool), typeof(CS4Options), new PropertyMetadata(default(bool), DisplayModePropertyChanged));

        public static void SetDisplayMode(UIElement element, bool value)
        {
            element.SetValue(DisplayModeProperty, value);
        }

        public static bool GetDisplayMode(UIElement element)
        {
            return (bool)element.GetValue(DisplayModeProperty);
        }

        private static void DisplayModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
#if CS_4
            TextOptions.SetTextFormattingMode(d, TextFormattingMode.Display);
#endif
        }

        #endregion

        #region UseLayoutRounding Attached Property

        public static readonly DependencyProperty UseLayoutRoundingProperty =
            DependencyProperty.RegisterAttached("UseLayoutRounding", typeof(bool), typeof(CS4Options), new PropertyMetadata(default(bool), UseLayoutRoundingPropertyChanged));



        public static void SetUseLayoutRounding(UIElement element, bool value)
        {
            element.SetValue(UseLayoutRoundingProperty, value);
        }

        public static bool GetUseLayoutRounding(UIElement element)
        {
            return (bool)element.GetValue(UseLayoutRoundingProperty);
        }

        private static void UseLayoutRoundingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
#if CS_4
            d.SetValue(FrameworkElement.UseLayoutRoundingProperty, e.NewValue);
#endif
        }

        #endregion
    }
}
