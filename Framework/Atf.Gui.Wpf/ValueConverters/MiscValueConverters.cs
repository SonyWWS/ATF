//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
            if (value == null || value.GetType() != typeof(Color))
                return value;
            
            return new SolidColorBrush((Color)value);
        }
    }

    /// <summary>
    /// Returns inverse of a bool</summary>
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
    /// Returns whether or not an object is null</summary>
    public class IsNullConverter : ConverterMarkupExtension<IsNullConverter>
    {
        /// <summary>
        /// Returns whether or not an object is null</summary>
        /// <param name="value">Object to test</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>True iff object is null</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null;
        }
    }

    /// <summary>
    /// Returns whether or not an object is non-null</summary>
    public class IsNotNullConverter : ConverterMarkupExtension<IsNotNullConverter>
    {
        /// <summary>
        /// Returns whether or not an object is non-null</summary>
        /// <param name="value">Object to test</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>True iff object is non-null</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }
    }
    
    /// <summary>
    /// Returns true if type passed as parameter is assignable from type of value</summary>
    public class IsAssignableFromConverter : ConverterMarkupExtension<IsAssignableFromConverter>
    {
        /// <summary>
        /// Returns true if type passed as parameter is assignable from type of value</summary>
        /// <param name="value">Object to test</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Type to test if assignable from type of value</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>True iff type passed as parameter is assignable from type of value</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = parameter as Type;
            if (value != null && type != null)
                return type.IsAssignableFrom(value.GetType());
            return false;
        }
    }

    /// <summary>
    /// Attempts to return a UI resource using the input value as a resource key</summary>
    public class ResourceLookupConverter : ConverterMarkupExtension<ResourceLookupConverter>
    {
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
            return value == null ? null : Application.Current.FindResource(value);
        }
    }

    /// <summary>
    /// Returns an image resource from a resource key.
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
                var imageSource = Application.Current.FindResource(value) as ImageSource;
                var newImage = new Image();
                newImage.Source = imageSource;
                newImage.Style = Application.Current.FindResource(Resources.MenuItemImageStyleKey) as Style;
                Application.Current.Resources.Add(resourceKey, newImage);
                image = newImage;
            }

            return image;
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

            var inputGestures = value as IList;
            if (inputGestures == null || inputGestures.Count < 1)
                return null;

            for (int i = 0; i < inputGestures.Count; i++)
            {
                var gesture = inputGestures[i] as KeyGesture;
                if (gesture != null)
                {
                    return gesture.GetDisplayStringForCulture(culture);
                }
            }

            return null;
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
        /// Converts from int, long, byte, double, and float, to double</summary>
        /// <param name="value">Value to convert</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Double value</returns>
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
        /// Converts back to double from int, long, byte, double, and float</summary>
        /// <param name="value">Value to convert</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Original value</returns>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
    
    /// <summary>
    /// Retrieves the display string for enum values marked with a DisplayStringAttribute.
    /// Used on an enum, will override the standard behavior of ToString for enum values.</summary>
    public class DisplayStringConverter : ConverterMarkupExtension<DisplayStringConverter>
    {
        /// <summary>
        /// Retrieves the display string for enum values marked with a DisplayStringAttribute</summary>
        /// <param name="value">Value that may be an enum</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Display string for enum value, or original value if no conversion found</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                Type type = value.GetType();
                if (type.IsEnum)
                    return EnumUtil.GetDisplayString(type, value);
            }
            return value;
        }
    }
}
