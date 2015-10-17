//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// TypeConverter for use with ColorEditor; converts color stored in DOM as an ARGB int to/from color or string types</summary>
    public class IntColorConverter : TypeConverter
    {
        /// <summary>
        /// Determines whether this instance can convert from the specified context</summary>
        /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that provides a format context</param>
        /// <param name="sourceType">A System.Type that represents the type you want to convert from</param>
        /// <returns><c>True</c> if this instance can convert from the specified context</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string) ||
                    sourceType == typeof(Color));
        }

        /// <summary>
        /// Determines whether this instance can convert to the specified context</summary>
        /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that provides a format context</param>
        /// <param name="destType">Type of the destination</param>
        /// <returns><c>True</c> if this instance can convert to the specified context</returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
        {
            return (destType == typeof(string) ||
                    destType == typeof(Color));
        }

        /// <summary>
        /// Converts the given object to the type of this converter, using the specified context and culture information</summary>
        /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that provides a format context</param>
        /// <param name="culture">The <see cref="T:System.Globalization.CultureInfo"></see> to use as the current culture.</param>
        /// <param name="value">The object to convert</param>
        /// <returns>An object that represents the converted value</returns>
        /// <exception cref="T:System.NotSupportedException">The conversion cannot be performed</exception>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null)
                return null;

            if (value is string)
            {
                // string -> ARGB int
                TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));
                Color color = (Color) colorConverter.ConvertFrom(context, culture, value);
                return color.ToArgb();
            }
            else if (value is Color)
            {
                // Color -> ARGB int
                Color color = (Color) value;
                return color.ToArgb();
            }

            return base.ConvertFrom(context, culture, value);
        }

        /// <summary>Converts the given value object to the specified type, using the specified
        /// context and culture information</summary>
        /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that provides a format context</param>
        /// <param name="culture">A System.Globalization.CultureInfo. If null is passed, the current culture is assumed</param>
        /// <param name="value">The System.Object to convert</param>
        /// <param name="destType">The System.Type to convert the value parameter to</param>
        /// <returns>An object that represents the converted value</returns>
        public override object ConvertTo(ITypeDescriptorContext context,
                CultureInfo culture,
                object value,
                Type destType)
        {
            if (value == null)
                return null;

            if (value is Color)
                value = ((Color)value).ToArgb();

            if (value is int && destType == typeof(string))
            {
                // ARGB int -> string
                Color color = Color.FromArgb((int) value);
                TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));
                return colorConverter.ConvertTo(context, culture, color, destType);
            }
            else if (value is int && destType == typeof(Color))
            {
                // ARGB int -> Color
                return Color.FromArgb((int) value);
            }

            return base.ConvertTo(context, culture, value, destType);
        }
    }
}

