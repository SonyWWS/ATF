//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// TypeConverter for use with enum editors where enum is stored as int.
    /// It converts int to string and string to int.</summary>
    public class IntEnumTypeConverter : TypeConverter, IAnnotatedParams
    {
        /// <summary>
        /// Default construct, required for IAnnotatedParams</summary>
        public IntEnumTypeConverter()
        { }

        /// <summary>
        /// Construct using an enum type</summary>
        /// <param name="enumType">Enum type</param>
        public IntEnumTypeConverter(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException("enumType must Enum");
            m_names = Enum.GetNames(enumType);
            //Note: don't use Enum.GetValues(...) to populate m_values

            m_values = new int[m_names.Length];
            for (int i = 0; i < m_values.Length; i++)
            {
                m_values[i] = (int)Enum.Parse(enumType, m_names[i]);
            }
        }

        /// <summary>
        /// Construct using names and values
        /// </summary>
        /// <param name="names">Enum names</param>
        /// <param name="values">Enum values</param>
        public IntEnumTypeConverter(string[] names, int[] values)
        {
            DefineEnums(names, values);
        }

        #region IAnnotatedParams Members

        void IAnnotatedParams.Initialize(string[] parameters)
        {
            string[] displayNames;
            string[] names;
            int[] values;
            EnumUtil.ParseEnumDefinitions(parameters, out names, out displayNames, out values);
            DefineEnums(names, values);
        }

        #endregion

        #region base override

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            string strVal = value as string;
            if (strVal != null)
            {
                for (int i = 0; i < m_names.Length; i++)
                {
                    if (strVal == m_names[i])
                        return m_values[i];
                }
                // no match.
                return -1;
            }
            throw new ArgumentException("value must be string");
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (value is int && destinationType == typeof(string))
            {
                int intVal = (int)value;
                for (int i = 0; i < m_values.Length; i++)
                {
                    if (intVal == m_values[i])
                        return m_names[i];
                }
                return string.Empty;
            }
            throw new ArgumentException("value must be an int and destinationType must be a string");
        }

        #endregion

        private void DefineEnums(string[] names, int[] values)
        {
            if (names == null
                || names.Length == 0
                || values == null
                || names.Length != values.Length)
                throw new ArgumentException();

            m_names = names;
            m_values = values;
        }

        private string[] m_names;
        private int[] m_values;
    }
}
