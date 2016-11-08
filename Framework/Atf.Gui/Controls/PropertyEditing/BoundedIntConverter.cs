//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Globalization;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// TypeConverter for use with BoundedIntEditor; restricts parsed int values to specified range</summary>
    /// <remarks>First argument, if not empty, is the minimum; second, if not empty, is the maximum</remarks>
    public class BoundedIntConverter : Int32Converter, IAnnotatedParams
    {
        #region IAnnotatedParams Members

        /// <summary>
        /// Initializes the control</summary>
        /// <param name="parameters">Editor parameters</param>
        public void Initialize( string[] parameters )
        {
            if (parameters.Length < 2)
                throw new ArgumentException("Can't parse bounds");

            try
            {
                if (parameters[0].Length > 0)
                    m_min = Int32.Parse(parameters[0]);
                if (parameters[1].Length > 0)
                    m_max = Int32.Parse(parameters[1]);
            }
            catch
            {
                throw new ArgumentException("Can't parse bounds");
            }

            if (m_min.HasValue && m_max.HasValue && m_min.Value >= m_max.Value)
                throw new ArgumentException("Max must be > min");
        }

        #endregion

        /// <summary>
        /// Sets min and max
        /// </summary>
        /// <param name="min">min value</param>
        /// <param name="max">max value</param>
        public void SetMinMax(int min, int max)
        {
            if (min >= max)
                throw new ArgumentOutOfRangeException("min must be less than max");
            if ((m_min.HasValue && m_min.Value != min) || (m_max.HasValue && m_max.Value != max))
            {
                m_min = min;
                m_max = max;
            }
        }

        /// <summary>
        /// Converts the given object to the type of this converter, using the specified context and culture information</summary>
        /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that provides a format context</param>
        /// <param name="culture">The <see cref="T:System.Globalization.CultureInfo"></see> to use as the current culture</param>
        /// <param name="value">The object to convert</param>
        /// <returns>An object that represents the converted value</returns>
        /// <exception cref="T:System.NotSupportedException">The conversion cannot be performed</exception>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            int intValue = (int)base.ConvertFrom(context, culture, value);
            if (m_min.HasValue)
                intValue = Math.Max(intValue, m_min.Value);
            if (m_max.HasValue)
                intValue = Math.Min(intValue, m_max.Value);
            return intValue;
        }

        private int? m_min;
        private int? m_max;
    }
}


