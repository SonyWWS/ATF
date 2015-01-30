//Sony Computer Entertainment Confidential

using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// Debugging utility functions</summary>
    public static class DebugUtils
    {
        /// <summary>
        /// Resource key used in XAML files to reference a DebugResource instance</summary>
        public static readonly ResourceKey DebugConverterKey
            = new ComponentResourceKey(typeof(DebugUtils), "DebugConverter");

        /// <summary>
        /// Method provided for when it helps to traverse the logical tree</summary>
        /// <param name="initial">Initial DependencyObject</param>
        /// <returns>DependencyObject tree root</returns>
        public static DependencyObject FindLogicalTreeRoot(DependencyObject initial)
        {
            var current = initial;
            var result = initial;

            while (current != null)
            {
                result = current;
                current = (current is Visual || current is Visual3D) ? VisualTreeHelper.GetParent(current)
                                                                     : LogicalTreeHelper.GetParent(current);
            }

            return result;
        }

        /// <summary>
        /// Get item's data context</summary>
        /// <param name="item">Item</param>
        /// <returns>Data context</returns>
        public static object GetDataContext(object item)
        {
            if (item == null)
                return null;

            object result = null;

            if (item is FrameworkElement)
            {
                var fe = item as FrameworkElement;
                result = fe.DataContext;

                if (result == null)
                    result = GetDataContext(fe.Parent);

                if (result == null)
                    result = GetDataContext(fe.TemplatedParent);
            }

            if (result == null && item is FrameworkContentElement)
            {
                var fce = item as FrameworkContentElement;
                result = fce.DataContext;

                if (result == null)
                    result = GetDataContext(fce.Parent);

                if (result == null)
                    result = GetDataContext(fce.TemplatedParent);
            }

            return result;
        }
    }

    /// <summary>
    /// Debugging class. By referencing this class for the 'Converter' property of any Binding markup,
    /// your debugging session will break in Convert() and ConvertBack(), when that Binding is processed.</summary>
    /// <remarks>
    /// DO NOT CHECK IN XAML THAT USES THIS CONVERTER. It is intended for local debugging only.</remarks>
    public class DebugConverter : ConverterMarkupExtension<DebugConverter>
    {
        /// <summary>
        /// Attempt to convert a value to a target type</summary>
        /// <param name="value">Object to convert</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Converted value or null if no conversion done</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // breakpoint allows inspection of value obtained for a target control in a binding
            TryBreaking();
            var ctx = DebugUtils.GetDataContext(value);
            if (ctx != null) {}

            return value;
        }

        /// <summary>
        /// Convert back value</summary>
        /// <param name="value">Value to convert back</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Value converted back or null if no conversion done</returns>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // breakpoint allows inspection of value obtained for a target control in a binding
            TryBreaking();

            return value;
        }

        [Conditional("DEBUG")]
        private static void TryBreaking()
        {
            if (Debugger.IsAttached == false)
                return;

            Debugger.Break();
        }
    }

}
