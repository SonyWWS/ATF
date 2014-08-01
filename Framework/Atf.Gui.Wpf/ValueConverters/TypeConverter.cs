//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Data;
using System.Windows;
using System.Windows.Markup;
using System.Globalization;
using System.ComponentModel;

namespace Sce.Atf.Wpf.ValueConverters
{
    [ValueConversion(typeof(object), typeof(object))]
    public class TypeConverter : FrameworkElement, IValueConverter
    {
        /// <summary>
        /// Identifies the <see cref="SourceType"/> dependency property.</summary>
        public static readonly DependencyProperty SourceTypeProperty = 
            DependencyProperty.Register("SourceType", typeof(Type), typeof(TypeConverter));

        /// <summary>
        /// Identifies the <see cref="TargetType"/> dependency property.</summary>
        public static readonly DependencyProperty TargetTypeProperty = 
            DependencyProperty.Register("TargetType", typeof(Type), typeof(TypeConverter));

        /// <summary>
        /// Gets or sets the source type for the conversion.</summary>
        [ConstructorArgument("sourceType")]
        public Type SourceType
        {
            get { return GetValue(SourceTypeProperty) as Type; }
            set { SetValue(SourceTypeProperty, value); }
        }

        /// <summary>
        /// Gets or sets the target type for the conversion.</summary>
        [ConstructorArgument("targetType")]
        public Type TargetType
        {
            get { return GetValue(TargetTypeProperty) as Type; }
            set { SetValue(TargetTypeProperty, value); }
        }

        /// <summary>
        /// Constructs an instance of <c>TypeConverter</c>.</summary>
        public TypeConverter()
        {
        }

        /// <summary>
        /// Constructs an instance of <c>TypeConverter</c> with the specified source and target types.</summary>
        /// <param name="sourceType">The source type (see <see cref="SourceType"/>).</param>
        /// <param name="targetType">The target type (see <see cref="TargetType"/>).</param>
        public TypeConverter(Type sourceType, Type targetType)
        {
            SourceType = sourceType;
            TargetType = targetType;
        }

        /// <summary>
        /// Attempts to convert the specified value.</summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Requires.NotNull(TargetType, "NoTargetType");
            return DoConversion(value, TargetType, culture);
        }

        /// <summary>
        /// Attempts to convert the specified value back.</summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Requires.NotNull(SourceType, "NoSourceType");
            return DoConversion(value, SourceType, culture);
        }

        private static object DoConversion(object value, Type toType, CultureInfo culture)
        {
            if ((value is IConvertible) || (value == null))
            {
                try
                {
                    return System.Convert.ChangeType(value, toType, culture);
                }
                catch (Exception)
                {
                    return DependencyProperty.UnsetValue;
                }
            }
            else
            {
                System.ComponentModel.TypeConverter typeConverter = TypeDescriptor.GetConverter(value);

                if (typeConverter.CanConvertTo(toType))
                {
                    return typeConverter.ConvertTo(null, culture, value, toType);
                }
            }

            return DependencyProperty.UnsetValue;
        }
    }
}
