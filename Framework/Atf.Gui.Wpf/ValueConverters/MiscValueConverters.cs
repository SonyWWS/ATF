//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Sce.Atf.Input;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters
{
    /// <summary>
    /// Converts a Color to a SolidColorBrush</summary>
    public class ColorToBrushConverter : ConverterMarkupExtension<ColorToBrushConverter>
    {
        /// <summary>
        /// Converts a Color to a SolidColorBrush</summary>
        /// <param name="value">Color</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>SolidColorBrush of desired color</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var type = value.GetType();
                
                if (type == typeof(Color))
                    return new SolidColorBrush((Color)value);
                
                if (type == typeof(string))
                    return new SolidColorBrush(ColorUtil.ConvertFromString((string)value));
            }

            return DependencyProperty.UnsetValue;
        }
    }

    /// <summary>
    /// Converts a Brush to a Color</summary>
    public class BrushToColorConverter : ConverterMarkupExtension<BrushToColorConverter>
    {
        /// <summary>
        /// Convert a brush to a SolidColorBrush</summary>
        /// <param name="value">Brush to convert</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Converted brush</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var solidColorBrush = value as SolidColorBrush;
            if (solidColorBrush == null)
                return Color.FromArgb(0, 0, 0, 0);

            return solidColorBrush.Color;
        }
    }

    /// <summary>
    /// Converts a Color to a SolidColorBrush</summary>
    public class ColorAndOpacityToBrushConverter : MultiConverterMarkupExtension<ColorAndOpacityToBrushConverter>
    {
        /// <summary>
        /// Converts Color values to a SolidColorBrush</summary>
        /// <param name="values">Color values</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>SolidColorBrush of desired color</returns>
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is Color && values[1] is double)
            {
                var c = (Color)values[0];
                var d = (double)values[1];
                return new SolidColorBrush(Color.FromArgb((byte)(d * 255), c.R, c.G, c.B));
            }
            
            return DependencyProperty.UnsetValue;
        }
    }

    /// <summary>
    /// Converter for inverse of a Boolean</summary>
    public class InvertBoolConverter : ConverterMarkupExtension<InvertBoolConverter>
    {
        /// <summary>
        /// Returns inverse of a bool</summary>
        /// <param name="value">Bool value</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Inverse of given bool</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        /// <summary>
        /// Converts back inverse of a bool</summary>
        /// <param name="value">Bool value</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Inverse of given bool</returns>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }

    /// <summary>
    /// Converter for whether or not an object is null</summary>
    public class IsNullConverter : ConverterMarkupExtension<IsNullConverter>
    {
        /// <summary>
        /// Returns whether or not an object is null</summary>
        /// <param name="value">Object to test</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns><c>True</c> if object is null</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null;
        }
    }

    /// <summary>
    /// Converter for whether or not an object is non-null</summary>
    public class IsNotNullConverter : ConverterMarkupExtension<IsNotNullConverter>
    {
        /// <summary>
        /// Returns whether or not an object is non-null</summary>
        /// <param name="value">Object to test</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns><c>True</c> if object is non-null</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }
    }

    /// <summary>
    /// Converter for int to Boolean</summary>
    public class IntToBoolConverter : ConverterMarkupExtension<IntToBoolConverter>
    {
        /// <summary>
        /// Convert int to a bool</summary>
        /// <param name="value">Object to convert</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Converted value</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value > 0;
        }
    }

    /// <summary>
    /// Converter for whether type passed as parameter is assignable from type of value</summary>
    public class IsAssignableFromConverter : ConverterMarkupExtension<IsAssignableFromConverter>
    {
        /// <summary>
        /// Returns true if type passed as parameter is assignable from type of value</summary>
        /// <param name="value">Object to test</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Type to test if assignable from type of value</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns><c>True</c> if type passed as parameter is assignable from type of value</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = parameter as Type;
            if (value != null && type != null)
                return type.IsAssignableFrom(value.GetType());
            return false;
        }
    }

    /// <summary>
    /// Converter for a UI resource using the input value as a resource key</summary>
    public class ResourceLookupConverter : ConverterMarkupExtension<ResourceLookupConverter>
    {
        /// <summary>
        /// Default constructor</summary>
        public ResourceLookupConverter()
        {
        }

        /// <summary>
        /// Constructor with FrameworkElement</summary>
        /// <param name="element">FrameworkElement key</param>
        public ResourceLookupConverter(FrameworkElement element)
        {
            Source = element;
        }

        /// <summary>
        /// Get or set FrameworkElement key</summary>
        public FrameworkElement Source { get; set; }

        /// <summary>
        /// Attempts to return a UI resource using the input value as a resource key</summary>
        /// <param name="value">Resource key</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>UI resource for given key, or null if input value is null</returns>
        /// <remarks>If the requested resource is not found, a System.Windows.ResourceReferenceKeyNotFoundException is thrown.</remarks>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object result = value == null 
                ? null 
                : Source != null
                    ? Source.TryFindResource(value)
                    : Application.Current.TryFindResource(value);

            return result;
        }
    }

    /// <summary>
    /// Converter for an image resource from a resource key.
    /// This class is a workaround for a WPF bug (Fixed in .NET 4).
    /// that prevents binding a MenuItem.Icon to an ImageSource correctly! For details, see
    /// https://connect.microsoft.com/VisualStudio/feedback/details/497408/wpf-menuitem-icon-cannot-be-set-via-setter?wa=wsignin1.0 </summary>
    public class ImageResourceLookupConverter : ConverterMarkupExtension<ImageResourceLookupConverter>
    {
        /// <summary>
        /// Returns an image resource from a resource key.
        /// This method is a workaround for a WPF bug (Fixed in .NET 4).
        /// that prevents binding a MenuItem.Icon to an ImageSource correctly! For details, see
        /// https://connect.microsoft.com/VisualStudio/feedback/details/497408/wpf-menuitem-icon-cannot-be-set-via-setter?wa=wsignin1.0 </summary>
        /// <param name="value">Image resource ID</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Image resource</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value);
        }

        /// <summary>
        /// Obtains an image resource from a resource key</summary>
        /// <param name="value">Image resource ID</param>
        /// <returns>Image resource</returns>
        /// <remarks>This method is a workaround for a WPF bug (Fixed in .NET 4).
        /// that prevents binding a MenuItem.Icon to an ImageSource correctly! For details, see
        /// https://connect.microsoft.com/VisualStudio/feedback/details/497408/wpf-menuitem-icon-cannot-be-set-via-setter?wa=wsignin1.0 </remarks>
        public static object Convert(object value)
        {
            if (value == null)
                return null;

            var resourceKey = new ComponentResourceKey(typeof(ImageResourceLookupConverter), value);
            object image = Application.Current.TryFindResource(resourceKey);
            if (image == null)
            {
                var imageSource = Application.Current.TryFindResource(value) as ImageSource;
                if (imageSource != null)
                {
                    var newImage = new Image();
                    newImage.Source = imageSource;
                    newImage.Style = Application.Current.FindResource(Resources.MenuItemImageStyleKey) as Style;
                    Application.Current.Resources.Add(resourceKey, newImage);
                    image = newImage;
                }
            }

            return image;
        }
    }

    /// <summary>
    /// Converter for a cursor from a resource key</summary>
    public class CursorResourceLookupConverter : ConverterMarkupExtension<CursorResourceLookupConverter>
    {
        /// <summary>
        /// Attempt to convert a resource key to a cursor</summary>
        /// <param name="value">Object to convert</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Converted value</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value);
        }

        /// <summary>
        /// Attempt to convert a resource key to a cursor</summary>
        /// <param name="value">Object to convert</param>
        /// <returns>Converted value or null if no conversion done</returns>
        public static object Convert(object value)
        {
            if (value == null)
                return null;

            var freezable = Application.Current.TryFindResource(value) as FreezableCursor;
            if (freezable != null)
            {
                return freezable.Cursor;
            }

            return null;
        }
    }

    /// <summary>
    /// Converts an image source URI string to an image source.
    /// NOTE: This is normally done automatically by WPF, but this class provides some additional safety checks.</summary>
    public class ImageSourceConverter : ConverterMarkupExtension<ImageSourceConverter>
    {
        /// <summary>
        /// Converts an image source URI string to an image source.
        /// NOTE: This is normally done automatically by WPF, but this class provides some additional safety checks.</summary>
        /// <param name="value">Image URI string</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Image bitmap</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = value as string;
            if (!String.IsNullOrEmpty(str))
            {
                value = new Uri(str, UriKind.RelativeOrAbsolute);
            }

            if (value is Uri)
            {
                return BitmapFrame.Create(value as Uri);
            }

            return null;
        }
    }
 

    /// <summary>
    /// Converts InputGestureCollection to a single KeyGesture string by taking first available KeyGesture</summary>
    public class InputGestureTextConverter : ConverterMarkupExtension<InputGestureTextConverter>
    {
        /// <summary>
        /// Converts InputGestureCollection to a single KeyGesture string by taking first available KeyGesture</summary>
        /// <param name="value">Input gesture collection</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use for string</param>
        /// <returns>KeyGesture string or null if none found</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var inputGesture = value as KeyGesture;
            if(inputGesture != null)
                return inputGesture.GetDisplayStringForCulture(culture);

            if (value is Keys)
                return GetDisplayStringForKeys((Keys)value, culture);

            var list = value as IList;
            if (list == null || list.Count < 1)
                return null;

            for (int i = 0; i < list.Count; i++)
            {
                var gesture = list[i] as KeyGesture;
                if (gesture != null)
                {
                    return gesture.GetDisplayStringForCulture(culture);
                }
                if (list[i] is Keys)
                {
                    return GetDisplayStringForKeys((Keys)list[i], culture);
                }
            }

            return null;
        }

        private static string GetDisplayStringForKeys(Keys value, CultureInfo culture)
        {
            // Convert Keys to a KeyGesture
            return ToWpfKeyGesture(value).GetDisplayStringForCulture(culture);
        }

        private static KeyGesture ToWpfKeyGesture(Keys atfKeys)
        {
            ModifierKeys modifiers = Sce.Atf.Wpf.Interop.KeysInterop.ToWpfModifiers(atfKeys);
            Key key = Sce.Atf.Wpf.Interop.KeysInterop.ToWpf(atfKeys);
            return new KeyGesture(key, modifiers);
        }
    }

    /// <summary>
    /// Converter for list to string</summary>
    public class ListToStringConverter : ConverterMarkupExtension<ListToStringConverter>
    {
        /// <summary>
        /// Attempt to convert a list to a string</summary>
        /// <param name="value">Object to convert</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Converted value or null if no conversion done</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var list = value as IEnumerable;
            if (list != null)
            {
                var sb = new StringBuilder();
                foreach (var item in list)
                    sb.AppendLine(item.ToString());
                return sb.ToString();
            }
            return null;
        }
    }

    /// <summary>
    /// Converts a single value to a collection</summary>
    public class ItemToCollectionConverter : ConverterMarkupExtension<ItemToCollectionConverter>
    {
        /// <summary>
        /// Convert a single value to a collection</summary>
        /// <param name="value">Object to convert</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Converted value</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = new ObservableCollection<object>();
            collection.Add(value);
            return collection;
        }
    }

    /// <summary>
    /// GridLength to double value converter</summary>
    [ValueConversion(typeof(GridLength), typeof(double))]
    public class DoubleToGridLengthConverter : ConverterMarkupExtension<DoubleToGridLengthConverter>
    {
        /// <summary>
        /// Converts GridLength to double value</summary>
        /// <param name="value">GridLength</param>
        /// <param name="targetType">Type of target</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Double value</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertBack(value, targetType, parameter, culture);
        }

        /// <summary>
        /// Converts back to GridLength from double value</summary>
        /// <param name="value">Double value</param>
        /// <param name="targetType">Type of target</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>GridLength</returns>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(GridLength))
            {
                if (value == null)
                    return GridLength.Auto;
                if (value is double)
                    return new GridLength((double)value);
                return GridLength.Auto;
            }
            
            if (targetType == typeof(double))
            {
                if (value is GridLength)
                    return ((GridLength)value).Value;
                return double.NaN;
            }
            
            return null;
        }
    }

    /// <summary>
    /// Allows conversion from int, long, byte, double, and float, to double</summary>
    [ValueConversion(typeof(object), typeof(double))]
    public class ToDoubleConverter : ConverterMarkupExtension<ToDoubleConverter>
    {
        /// <summary>
        /// Converts to int, long, byte, double, or float</summary>
        /// <param name="value">Value to convert</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Double value, 0 if value == null, or Binding.DoNothing if value's type is not int, long, byte, double, or float</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return 0;

            // Explicit unboxing
            var t = value.GetType();
            if (t == typeof(int))
                return (int)value;
            if (t == typeof(long))
                return (long)value;
            if (t == typeof(byte))
                return (byte)value;
            if (t == typeof(double))
                return (double)value;
            if (t == typeof(float))
                return (float)value;

            return Binding.DoNothing;
        }

        /// <summary>
        /// Convert back value</summary>
        /// <param name="value">Value to convert back</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Value converted back</returns>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    /// <summary>
    /// Enum to Boolean converter.
    /// Usage 'Converter={StaticResource EnumToBooleanConverter}, ConverterParameter={x:Static value...}'</summary>
    [ValueConversion(typeof(Enum), typeof(bool))]
    public class EnumToBooleanConverter : ConverterMarkupExtension<EnumToBooleanConverter>
    {
        /// <summary>
        /// Get or set type of enumeration</summary>
        public Type EnumType { get; set; }

        /// <summary>
        /// Attempt to convert an enumeration value to a Boolean</summary>
        /// <param name="value">Object to convert</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Parameter to test against value</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Binding.DoNothing if value or parameter is null. Otherwise, <c>True</c> if value or parameter convert to the same string.</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Binding.DoNothing;
            
            string checkValue = value.ToString();
            string targetValue = parameter.ToString();
            return checkValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Convert back value from Boolean to enumeration value</summary>
        /// <param name="value">Value to convert back</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">String representation of name or numeric value of enumeration value</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Value converted back or Binding.DoNothing if no conversion done</returns>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Binding.DoNothing;
            
            try
            {
                bool boolValue = System.Convert.ToBoolean(value, culture);
                if (boolValue)
                    return Enum.Parse(EnumType, parameter.ToString());
            }
            catch (ArgumentException)
            {
            }
            catch (FormatException)
            {
            }
            
            return Binding.DoNothing;
        }
    }

    /// <summary>
    /// Converter for enumeration value to array of values of constants</summary>
    [ValueConversion(typeof(Enum), typeof(string[]))]
    public class EnumValuesConverter : ConverterMarkupExtension<EnumValuesConverter>
    {
        /// <summary>
        /// Attempt to convert an enumeration value to array of values of constants in enumeration</summary>
        /// <param name="value">Object to convert</param>
        /// <param name="targetType">Type of value</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Array of values of constants in enumeration or original value if no conversion done</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                return Enum.GetValues(value.GetType());
            else if (targetType == typeof(Enum))
                return Enum.GetValues(targetType);

            return value;
        }
    }

    /// <summary>
    /// Converter for null to Boolean</summary>
    [ValueConversion(typeof(bool), typeof(object))]
    public class NullToBoolConverter : ConverterMarkupExtension<NullToBoolConverter>
    {
        /// <summary>
        /// Test whether object null</summary>
        /// <param name="value">Object to test</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns><c>True</c> if value is null</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null);
        }

        /// <summary>
        /// Convert back value from Boolean to null</summary>
        /// <param name="value">Value to convert back (unused)</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>null</returns>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// Converter for not null to Boolean</summary>
    [ValueConversion(typeof(bool), typeof(object))]
    public class NullToFalseConverter : ConverterMarkupExtension<NullToFalseConverter>
    {
        /// <summary>
        /// Test whether object is not null</summary>
        /// <param name="value">Object to convert</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns><c>True</c> if value is not null</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        /// <summary>
        /// Convert back value to null</summary>
        /// <param name="value">Value to convert back (unused)</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>null</returns>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// Converter for TimeSpan to another format</summary>
    [ValueConversion(typeof(TimeSpan), typeof(string))]
    public class TimeSpanFormatConverter : ConverterMarkupExtension<TimeSpanFormatConverter>
    {
        /// <summary>
        /// Attempt to convert a TimeSpan to time string</summary>
        /// <param name="value">Object to convert</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Converted time string</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan timespan = (TimeSpan)value;
            return string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}", timespan.Hours, timespan.Minutes, timespan.Seconds, timespan.Milliseconds);
        }
    }

    /// <summary>
    /// Converter used on any type, overriding the standard behavior of ToString for enumeration values</summary>
    public class DisplayStringConverter : ConverterMarkupExtension<DisplayStringConverter>
    {
        /// <summary>
        /// Retrieves the display string for enumeration values marked with a DisplayStringAttribute</summary>
        /// <param name="value">Value that may be an enumeration</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Display string for enumeration value, or original value if no conversion found</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var type = value.GetType();
                if (type.IsEnum)
                    return EnumDisplayUtil.GetDisplayString(type, value);

                return value.ToString();
            }
            
            return value;
        }
    }

    /// <summary>
    /// Converter for an enumeration to a description</summary>
    [ValueConversion(typeof(object), typeof(string))]
    public class EnumDescriptionConverter : ConverterMarkupExtension<EnumDescriptionConverter>
    {
        /// <summary>
        /// Attempt to convert an enumeration to a description</summary>
        /// <param name="value">Object to convert</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Binding.DoNothing if value is null. Otherwise, enumeration description</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Default, non-converted result.
            if (value == null)
                return Binding.DoNothing;

            string result = value.ToString();

            var field = value.GetType().GetFields(BindingFlags.Static | BindingFlags.GetField | BindingFlags.Public).FirstOrDefault(f => f.GetValue(null).Equals(value));

            if (field != null)
            {
                var descriptionAttribute = field.GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault() as DescriptionAttribute;
                if (descriptionAttribute != null)
                {
                    // Found the attribute, assign description
                    result = descriptionAttribute.Description;
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Converter for scaling a double value</summary>
    public class ScaleDoubleConverter : ConverterMarkupExtension<ScaleDoubleConverter>
    {
        /// <summary>
        /// Scale a double value</summary>
        /// <param name="value">Object to be scaled</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Scale factor</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Scaled double value</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double dVal = (double)value;
            double dScale = 0.75;
            
            if (parameter is string)
            {
                dScale = double.Parse((string)parameter);
            }
            else if (parameter != null)
            {
                dScale = (double)parameter;
            }

            return (dVal * dScale);
        }
    }

    /// <summary>
    /// Converter for rounding a double value</summary>
    public class RoundDoubleConverter : ConverterMarkupExtension<RoundDoubleConverter>
    {
        /// <summary>
        /// Round a double to a given number of decimal places</summary>
        /// <param name="value">Object to round</param>
        /// <param name="targetType">Type of target</param>
        /// <param name="parameter">Number decimal places to round</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Converted value</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double val = 0.0;
            int decimanlPlaces = 1;

            if (value is string)
            {
                val = double.Parse((string)value);
            }
            else
            {
                if (value is double)
                {
                    val = (double)value;
                }
                else if (value is float)
                {
                    val = (float)value;
                }
            }

            if (parameter is string)
            {
                decimanlPlaces = int.Parse((string)parameter);
            }
            else if (parameter != null)
            {
                decimanlPlaces = (int)parameter;
            }

            Requires.Require<ArgumentException>(decimanlPlaces >= 0, "parameter");

            if (targetType == typeof(string))
            {
                var zeroFormat = new string(Enumerable.Repeat('0', decimanlPlaces).ToArray());
                return val.ToString("0." + zeroFormat);
            }

            val = Math.Round(val, decimanlPlaces);
            return val;
        }

        /// <summary>
        /// Convert back rounded value to double</summary>
        /// <param name="value">Value to convert back</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Value converted back</returns>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }
    }

    /// <summary>
    /// Converter for indexed properties</summary>
    public class IndexerBindingConverter : MultiConverterMarkupExtension<IndexerBindingConverter>
    {
        /// <summary>
        /// Attempt to convert values to property value</summary>
        /// <param name="values">Array of objects to convert</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Converted value or DependencyProperty.UnsetValue if no conversion done</returns>
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return DependencyProperty.UnsetValue;
            
            object dataItem = values[1];
            object key = values[0];

            if (dataItem == null || key == null)
                return DependencyProperty.UnsetValue;

            foreach (var propertyInfo in dataItem.GetType().GetProperties())
            {
                if (propertyInfo.GetIndexParameters().Length > 0)
                {
                    object result = propertyInfo.GetValue(dataItem, new[] { key });
                    return result;
                }
            }

            return DependencyProperty.UnsetValue;
        }
    }

    /// <summary>
    /// Converter for enum to Boolean</summary>
    public class EnumToBoolConverter : ConverterMarkupExtension<EnumToBoolConverter>
    {
        /// <summary>
        /// Test if value equals a parameter</summary>
        /// <param name="value">Object to convert</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Parameter to test against value</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns><c>True</c> if value equals parameter</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        /// <summary>
        /// Convert back Boolean to Binding.DoNothing or parameter</summary>
        /// <param name="value">Value to convert back</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Possible return value</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>If value == false Binding.DoNothing, otherwise parameter</returns>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(false) ? Binding.DoNothing : parameter;
        }
    }
}
