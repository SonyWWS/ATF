//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// TypeConverter for use with enum editors, like EnumUITypeEditor. Converts integers to strings.
    /// In ATF property editors, the user will be able to enter any string, even if it doesn't match our
    /// list of names. See also, ExclusiveEnumTypeConverter.</summary>
    public class EnumTypeConverter : TypeConverter, IAnnotatedParams
    {
        /// <summary>
        /// Constructor</summary>
        public EnumTypeConverter()
        {
        }

        /// <summary>
        /// Constructor using enum names</summary>
        /// <param name="names">Enum names</param>
        /// <remarks>Enum values default to successive integers, starting with 0.</remarks>
        public EnumTypeConverter(string[] names)
        {
            DefineEnum(names);
        }

        /// <summary>
        /// Constructor using enum names and values</summary>
        /// <param name="names">Enum names</param>
        /// <param name="values">Enum values</param>
        public EnumTypeConverter(string[] names, int[] values)
        {
            DefineEnum(names, values);
        }

        /// <summary>
        /// Defines the enum names and values</summary>
        /// <param name="names">Enum names</param>
        /// <remarks>Enum values default to successive integers, starting with 0. Enum names
        /// with the format "EnumName=X" are parsed so that EnumName gets the value X, where X is an int.</remarks>
        public void DefineEnum(string[] names)
        {
            EnumUtil.ParseEnumDefinitions(names, out m_names, out m_values);
        }

        /// <summary>
        /// Defines the enum names and values</summary>
        /// <param name="names">Enum names</param>
        /// <param name="values">Enum values</param>
        public void DefineEnum(string[] names, int[] values)
        {
            if (names == null || values == null || names.Length != values.Length)
                throw new ArgumentException("names and/or values null, or of unequal length");

            m_names = names;
            m_values = values;
        }

        /// <summary>
        /// Determines whether this instance can convert from the specified context</summary>
        /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that provides a format context</param>
        /// <param name="srcType">A System.Type that represents the type you want to convert from</param>
        /// <returns>True iff this instance can convert from the specified context</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
        {
            return false;
        }

        /// <summary>
        /// Determines whether this instance can convert to the specified context</summary>
        /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that provides a format context</param>
        /// <param name="destType">Type of the destination</param>
        /// <returns>True iff this instance can convert to the specified context</returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
        {
            return destType == typeof(string);
        }

        /// <summary>Converts the given value object to the specified type, using the specified context and culture information</summary>
        /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that provides a format context</param>
        /// <param name="culture">A System.Globalization.CultureInfo. If null is passed, the current culture is assumed</param>
        /// <param name="value">The System.Object to convert</param>
        /// <param name="destType">The System.Type to convert the value parameter to</param>
        /// <returns>The converted object</returns>
        public override object ConvertTo(ITypeDescriptorContext context,
                CultureInfo culture,
                object value,
                Type destType)
        {
            if (value is int && destType == typeof(string))
            {
                // int to string
                int enumValue = (int)value;
                for (int i = 0; i < m_values.Length; i++)
                {
                    if (enumValue == m_values[i])
                        return m_names[i];
                }

                return string.Empty;
            }

            return base.ConvertTo(context, culture, value, destType);
        }

        #region IAnnotatedParams Members

        /// <summary>
        /// Initializes the control</summary>
        /// <param name="parameters">Editor parameters</param>
        public void Initialize(string[] parameters)
        {
            DefineEnum(parameters);
        }

        #endregion

        /// <summary>
        /// Gets the enumerated names. Treat as read-only.</summary>
        protected IList<string> Names
        {
            get { return m_names; }
        }

        /// <summary>
        /// Gets the integers associated with the enumerated names. Treat as read-only.</summary>
        protected IList<int> Values
        {
            get { return m_values; }
        }

        private string[] m_names = EmptyArray<string>.Instance;
        private int[] m_values = EmptyArray<int>.Instance;
    }
}
