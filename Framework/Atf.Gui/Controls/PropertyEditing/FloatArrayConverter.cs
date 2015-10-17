//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Type converter that can convert float[] to and from a string of the form "c1,c2,...,cN"</summary>
    public class FloatArrayConverter : TypeConverter
    {
        /// <summary>
        /// Gets or sets the formatting string for the components of the float[]</summary>
        public string Format
        {
            get { return m_format; }
            set { m_format = value; }
        }

        /// <summary>
        /// Gets or sets the scale factor, which is used to scale the value when it is converted
        /// into a string. The inverse factor is used for the opposite conversion.</summary>
        public double ScaleFactor
        {
            get { return m_scaleFactor; }
            set
            {
                if (value == 0)
                    throw new ArgumentException("value must be non-zero");

                m_scaleFactor = (float)value;
            }
        }

        /// <summary>
        /// Determines whether this instance can convert from the specified context</summary>
        /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that provides a format context</param>
        /// <param name="sourceType">A System.Type that represents the type you want to convert from</param>
        /// <returns><c>True</c> if this instance can convert from the specified context</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
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
            string s = value as string;
            if (s != null)
            {
                string[] components = s.Split(',');
                float[] array = new float[components.Length];
                for (int i = 0; i < components.Length; i++)
                {
                    float f;
                    if (!float.TryParse(components[i], NumberStyles.Float, CultureInfo.CurrentCulture, out f))
                        throw new ArgumentException("Error converting " + s + " to float[]");
                    array[i] = f / m_scaleFactor;
                }

                return array;
            }

            return base.ConvertFrom(context, culture, value);
        }

        /// <summary>Converts the given value object to the specified type, using the specified
        /// context and culture information</summary>
        /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that provides a format context</param>
        /// <param name="culture">A System.Globalization.CultureInfo. If null is passed, the current culture is assumed</param>
        /// <param name="value">The System.Object to convert</param>
        /// <param name="destinationType">The System.Type to convert the value parameter to</param>
        /// <returns>An object that represents the converted value</returns>
        public override object ConvertTo(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value,
            Type destinationType)
        {
            if (culture == null)
                culture = CultureInfo.CurrentCulture;
            string listSeparator = culture.TextInfo.ListSeparator;

            var array = value as float[];
            if (array != null)
            {
                var sb = new StringBuilder();
                foreach (float f in array)
                {
                    float scaled = f * m_scaleFactor;
                    sb.Append(scaled.ToString(m_format, culture));
                    sb.Append(listSeparator);
                }
                // removing trailing ','
                if (sb.Length > 0)
                    sb.Length -= listSeparator.Length;

                string result = sb.ToString();

                return result;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        private string m_format = "0.####";
        private float m_scaleFactor = 1.0f;
    }
}