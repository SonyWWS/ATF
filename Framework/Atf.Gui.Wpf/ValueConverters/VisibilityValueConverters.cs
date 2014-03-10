//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Globalization;
using System.Windows;

using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters
{
    /// <summary>
    /// Converts an int to a Visibility value. value > 0 returns Visibility.Visible, else Visibility.Collapsed.</summary>
    public class IntToVisibilityConverter : ConverterMarkupExtension<IntToVisibilityConverter>
    {
        /// <summary>
        /// Converts an int to a Visibility value. value > 0 returns Visibility.Visible, else Visibility.Collapsed.</summary>
        /// <param name="value">int value</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Visibility value</returns>
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)value > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Converts an int to a Visibility value. value > 0 returns Visibility.Collapsed, else Visibility.Visible.</summary>
    public class InverseIntToVisibilityConverter : ConverterMarkupExtension<InverseIntToVisibilityConverter>
    {
        /// <summary>
        /// Converts an int to a Visibility value. value > 0 returns Visibility.Collapsed, else Visibility.Visible.</summary>
        /// <param name="value">int value</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Visibility value</returns>
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)value > 0 ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    /// <summary>
    /// Converts a bool to a Visibility value. value == true returns Visibility.Visible, else Visibility.Collapsed.</summary>
    public class BoolToVisibilityConverter : ConverterMarkupExtension<BoolToVisibilityConverter>
    {
        /// <summary>
        /// Converts a bool to a Visibility value. value == true returns Visibility.Visible, else Visibility.Collapsed.</summary>
        /// <param name="value">bool value</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Visibility value</returns>
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Converts back from Visibility value to bool value</summary>
        /// <param name="value">Visibility value</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>bool value</returns>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Visibility)value == Visibility.Visible;
        }
    }

    /// <summary>
    /// Converts a bool to a Visibility value. value == true returns Visibility.Collapsed, else Visibility.Visible.</summary>
    public class InverseBoolToVisibilityConverter : ConverterMarkupExtension<InverseBoolToVisibilityConverter>
    {
        /// <summary>
        /// Converts a bool to a Visibility value. value == true returns Visibility.Collapsed, else Visibility.Visible.</summary>
        /// <param name="value">bool value</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Visibility value</returns>
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    /// <summary>
    /// Converts a Visibility value to a bool</summary>
    public class VisibilityToBoolConverter : ConverterMarkupExtension<VisibilityToBoolConverter>
    {
        /// <summary>
        /// Converts a Visibility value to a bool</summary>
        /// <param name="value">Visibility value</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>bool value</returns>
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((Visibility)value) == Visibility.Visible;
        }
    }

    /// <summary>
    /// Converts a Visibility value to a bool</summary>
    public class TwoWayVisibilityToBoolConverter : ConverterMarkupExtension<TwoWayVisibilityToBoolConverter>
    {
        /// <summary>
        /// Converts a Visibility value to a bool</summary>
        /// <param name="value">Visibility value</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>bool value</returns>
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((Visibility)value) == Visibility.Visible;
        }

        /// <summary>
        /// Converts back from bool value to Visibility value, including Visibility.Hidden</summary>
        /// <param name="value">bool value</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">If parameter equals "Hidden", returns Visibility.Hidden if value is false</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Visibility value</returns>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if((string)parameter == "Hidden")
                return ((bool)value) ? Visibility.Visible : Visibility.Hidden;

            return ((bool)value) ? Visibility.Visible : Visibility.Collapsed;

        }
    }

    /// <summary>
    /// Converts from IEnumerable to Visibility value. Returns Visibility.Visible if it contains any items,
    /// otherwise Visibility.Collapsed.</summary>
    public class EnumerableCountToVisibilityConverter : ConverterMarkupExtension<EnumerableCountToVisibilityConverter>
    {
        /// <summary>
        /// Converts from IEnumerable to Visibility value. Returns Visibility.Visible if it contains any items,
        /// otherwise Visibility.Collapsed.</summary>
        /// <param name="value">IEnumerable value</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Visibility value</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var enumerable = value as IEnumerable;
            if (enumerable != null)
            {
                foreach (var item in enumerable)
                    return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Converts from IEnumerable to Visibility value. 
    /// Value converter takes an IEnumerable and returns Visibility.Visible if it contains any items, otherwise Visibility.Collapsed.
    /// Also allows a multi binding to the Count property of the IEnumerable, which triggers re-evaluation
    /// if it changes.</summary>
    public class EnumerableCountToVisibilityMultiConverter : MultiConverterMarkupExtension<EnumerableCountToVisibilityMultiConverter>
    {
        /// <summary>
        /// Converts from IEnumerable to Visibility value. 
        /// Value converter takes an IEnumerable and returns Visibility.Visible if it contains any items, otherwise Visibility.Collapsed.
        /// Also allows a multi binding to the Count property of the IEnumerable, which triggers re-evaluation
        /// if it changes.</summary>
        /// <param name="values">IEnumerable value</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Visibility value</returns>
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var enumerable = values[0] as IEnumerable;
            if (enumerable != null)
            {
                foreach (var item in enumerable)
                    return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Value converter returns Visibility.Visible if the value is non-null, Visibility.Collapsed otherwise</summary>
    public class ItemNullToVisibilityConverter : ConverterMarkupExtension<ItemNullToVisibilityConverter>
    {
        /// <summary>
        /// Value converter returns Visibility.Visible if the value is non-null, Visibility.Collapsed otherwise</summary>
        /// <param name="value">Value to test</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Visibility value</returns>
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Value converter returns Visibility.Visible if the value is non-null, Visibility.Hidden otherwise</summary>
    public class ItemNullToHiddenVisibilityConverter : ConverterMarkupExtension<ItemNullToHiddenVisibilityConverter>
    {
        /// <summary>
        /// Value converter returns Visibility.Visible if the value is non-null, Visibility.Hidden otherwise</summary>
        /// <param name="value">Value to test</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Visibility value</returns>
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null ? Visibility.Visible : Visibility.Hidden;
        }
    }

    /// <summary>
    /// Value converter returns Visibility.Visible if TimeSpan's Ticks > 0, Visibility.Collapsed otherwise</summary>
    public class TimeSpanToVisibilityConverter : ConverterMarkupExtension<TimeSpanToVisibilityConverter>
    {
        /// <summary>
        /// Value converter returns Visibility.Visible if TimeSpan's Ticks > 0, Visibility.Collapsed otherwise</summary>
        /// <param name="value">TimeSpan</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Visibility value</returns>
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TimeSpan ts = (TimeSpan)value;
            return (ts != null && ts.Ticks == 0) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
