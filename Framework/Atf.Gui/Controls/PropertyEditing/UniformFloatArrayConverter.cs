//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Globalization;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Type converter that can convert a uniform float[] to and from a string of the form "c1,c2,...,cN"</summary>
    /// <remarks>This is useful for uniform scale vectors, for example.</remarks>
    public class UniformFloatArrayConverter : TypeConverter
    {
        /// <summary>
        /// Gets and sets the formatting string for the components of the float[]</summary>
        public string Format
        {
            get { return m_format; }
            set { m_format = value; }
        }

        /// <summary>
        /// Gets and sets the array length of float[] values</summary>
        public int ArrayLength
        {
            get { return m_arrayLength; }
            set { m_arrayLength = value; }
        }

        /// <summary>
        /// Determines whether converter can convert type to float[]</summary>
        /// <param name="context">The context</param>
        /// <param name="t">The type</param>
        /// <returns>True iff converter can convert type to float[]</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type t)
        {
            if (t == typeof(string))
                return true;
            else
                return base.CanConvertFrom(context, t);
        }

        /// <summary>
        /// Converts string to float[]</summary>
        /// <param name="context">The context</param>
        /// <param name="info">Culture info</param>
        /// <param name="value">Value to convert</param>
        /// <returns>Array of floats</returns>
        /// <exception cref="ArgumentException">If conversion fails</exception>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo info, object value)
        {
            string s = value as string;
            if (s != null)
            {
                float component;
                if (!float.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out component))
                    throw new ArgumentException("Error converting " + s + " to uniform float[]");
                float[] array = new float[m_arrayLength];
                for (int i = 0; i < m_arrayLength; i++)
                    array[i] = component;

                return array;
            }

            return base.ConvertFrom(context, info, value);
        }

        /// <summary>
        /// Determines whether converter can convert a float[] to type</summary>
        /// <param name="context">The context</param>
        /// <param name="t">The type</param>
        /// <returns>True iff converter can convert a float[] to type</returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type t)
        {
            if (t == typeof(string))
                return true;
            else
                return base.CanConvertTo(context, t);
        }

        /// <summary>
        /// Converts float[] to type</summary>
        /// <param name="context">The context</param>
        /// <param name="culture">Culture info</param>
        /// <param name="value">The value to convert</param>
        /// <param name="destType">Type to convert to</param>
        /// <returns>Converted object as type</returns>
        public override object ConvertTo(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value,
            Type destType)
        {
            if (destType == typeof(string) && value is float[])
            {
                float[] array = (float[])value;
                if (array.Length > 0)
                    return array[0].ToString(m_format, CultureInfo.CurrentCulture);
            }
            return base.ConvertTo(context, culture, value, destType);
        }

        private string m_format = "0.####";
        private int m_arrayLength = 3;
    }
}