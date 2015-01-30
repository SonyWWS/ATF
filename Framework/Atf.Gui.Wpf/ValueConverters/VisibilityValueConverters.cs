//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
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
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Convert back Visibility value to Boolean</summary>
        /// <param name="value">Value to convert back</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>true iff value == Visibility.Visible</returns>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Visibility)value == Visibility.Visible;
        }
    }

    /// <summary>
    /// Converter for Boolean to Visibility</summary>
    public class BoolToHiddenVisibilityConverter : ConverterMarkupExtension<BoolToHiddenVisibilityConverter>
    {
        /// <summary>
        /// Convert a Boolean to Visibility, where value == true => Visibility.Visible otherwise Visibility.Hidden</summary>
        /// <param name="value">Object to convert</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Visibility value</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// Convert back Visibility value to Boolean</summary>
        /// <param name="value">Value to convert back</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>true iff value == Visibility.Visible</returns>
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
        /// Converts a bool to a Visibility value, where value == true returns Visibility.Collapsed, else Visibility.Visible.</summary>
        /// <param name="value">bool value</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Visibility value</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    /// <summary>
    /// Class that converts a bool to a visibility value.
    /// value == true returns Hidden, else visible.</summary>
    public class InverseBoolToHiddenVisibilityConverter : ConverterMarkupExtension<InverseBoolToHiddenVisibilityConverter>
    {
        /// <summary>
        /// Convert a Boolean to Visibility, where value == true returns Visibility.Hidden otherwise Visibility.Visible</summary>
        /// <param name="value">Object to convert</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Visibility value</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Hidden : Visibility.Visible;
        }
    }

    /// <summary>
    /// Converts an visibility value to a bool</summary>
    public class VisibilityToBoolConverter : ConverterMarkupExtension<VisibilityToBoolConverter>
    {
        /// <summary>
        /// Converts a Visibility value to a bool</summary>
        /// <param name="value">Visibility value</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>bool value</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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
            if ((string)parameter == "Hidden")
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
            return (enumerable != null && enumerable.Cast<object>().Any()) ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Converts from IEnumerable to Visibility value. Returns Visibility.Visible if it contains any items,
    /// otherwise Visibility.Hidden.</summary>
    public class EnumerableCountToVisibilityHiddenConverter : ConverterMarkupExtension<EnumerableCountToVisibilityHiddenConverter>
    {
        /// <summary>
        /// Converts from IEnumerable to Visibility value. Returns Visibility.Visible if it contains any items,
        /// otherwise Visibility.Hidden.</summary>
        /// <param name="value">IEnumerable value</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Visibility value</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var enumerable = value as IEnumerable;
            return (enumerable != null && enumerable.Cast<object>().Any()) ? Visibility.Visible : Visibility.Hidden;
        }
    }

    /// <summary>
    /// Converts from IEnumerable to Visibility value. 
    /// Value converter takes an IEnumerable and returns Visibility.Visible if it contains any items, otherwise Visibility.Collapsed.
    /// Also allows a multi binding to the Count property of the IEnumerable, which triggers re-evaluation
    /// if it changes.</summary>
    public class EnumerableAnyToVisibilityCollapsedConverter : MultiConverterMarkupExtension<EnumerableAnyToVisibilityCollapsedConverter>
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
            return (enumerable != null && enumerable.Cast<object>().Any()) ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Converter for enumerable values to Visibility values</summary>
    public class EnumerableNoneToVisibilityCollapsedConverter : MultiConverterMarkupExtension<EnumerableNoneToVisibilityCollapsedConverter>
    {
        /// <summary>
        /// Attempt to convert enumerable values to Visibility values</summary>
        /// <param name="values">Object to convert</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Converted value or null if no conversion done</returns>
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var enumerable = values[0] as IEnumerable;
            return (enumerable != null && enumerable.Cast<object>().Any()) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    /// <summary>
    /// Value converter returns Visibility.Visible if the value is non-null, Visibility.Collapsed otherwise</summary>
    public class ItemNullToVisibilityConverter : ConverterMarkupExtension<ItemNullToVisibilityConverter>
    {
        /// <summary>
        /// Value converter returns Visibility.Visible if the value is non-null, otherwise Visibility.Collapsed</summary>
        /// <param name="value">Value to test</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Visibility value</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Converter for null to Visibility</summary>
    public class ItemNotNullToVisibilityConverter : ConverterMarkupExtension<ItemNotNullToVisibilityConverter>
    {
        /// <summary>
        /// Value converter that returns Visibility.Visible if the value is null, otherwise Visibility.Collapsed</summary>
        /// <param name="value">Value to test</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Visibility value</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Converter for not null to Visibility</summary>
    public class ItemNullToHiddenVisibilityConverter : ConverterMarkupExtension<ItemNullToHiddenVisibilityConverter>
    {
        /// <summary>
        /// Value converter returns Visibility.Visible if the value is non-null, otherwise Visibility.Hidden</summary>
        /// <param name="value">Value to test</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Visibility value</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Visibility.Visible : Visibility.Hidden;
        }
    }

    /// <summary>
    /// Converter for Visibility values</summary>
    public class InvertVisbilityConverter : ConverterMarkupExtension<InvertVisbilityConverter>
    {
        /// <summary>
        /// Value converter that returns Visibility.Collapsed if the value is Visibility.Visible, otherwise Visibility.Visible</summary>
        /// <param name="value">Value to test</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Visibility value</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value is Visibility) && (Visibility)value == Visibility.Visible)
                return Visibility.Collapsed;
            return Visibility.Visible;
        }
    }

    /// <summary>
    /// Converter for TimeSpan to Visibility</summary>
    public class TimeSpanToVisibilityConverter : ConverterMarkupExtension<TimeSpanToVisibilityConverter>
    {
        /// <summary>
        /// Value converter returns Visibility.Visible if TimeSpan's Ticks > 0, Visibility.Collapsed otherwise</summary>
        /// <param name="value">TimeSpan</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Visibility value</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan)
            {
                var ts = (TimeSpan)value;
                return ts.Ticks == 0 ? Visibility.Collapsed : Visibility.Visible;
            }

            if (value is double)
            {
                var ts = (double)value;
                return Math.Abs(ts - 0.0) < 0.00001 ? Visibility.Collapsed : Visibility.Visible;
            }

            return DependencyProperty.UnsetValue;
        }
    }

    /// <summary>
    /// Converter for Visibility values to a Visibility value</summary>
    public class VisibilityAndConverter : MultiConverterMarkupExtension<VisibilityAndConverter>
    {
        /// <summary>
        /// Attempt to convert Visibility values to a Visibility value</summary>
        /// <param name="values">Visibility values to convert</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Converted Visibility value</returns>
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Any(obj2 => !(obj2 is Visibility) || (((Visibility)obj2) != Visibility.Visible)) 
                ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
