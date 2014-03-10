//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// TypeConverter for use with FlagsUITypeEditor; converts int flags to/from string</summary>
    public class FlagsTypeConverter : TypeConverter, IAnnotatedParams
    {
        /// <summary>
        /// Default constructor</summary>
        public FlagsTypeConverter()
        {
        }

        /// <summary>
        /// Constructor using flag definitions</summary>
        /// <param name="definitions">Flag definitions</param>
        /// <remarks>Flag values default to successive powers of 2, starting with 1</remarks>
        public FlagsTypeConverter(string[] definitions)
        {
            DefineFlags(definitions);
        }

        /// <summary>
        /// Constructor using flag definitions and values</summary>
        /// <param name="names">Flag names</param>
        /// <param name="values">Flag values</param>
        public FlagsTypeConverter(string[] names, int[] values)
        {
            DefineFlags(names, values);
        }

        /// <summary>
        /// Defines the flag names and display names and values</summary>
        /// <param name="definitions">Flag definitions</param>
        /// <remarks>Flag values default to successive powers of 2, starting with 1. Flag names
        /// with the format "FlagName=X" are parsed so that FlagName gets the value X, where X is an int.</remarks>
        public void DefineFlags(string[] definitions)
        {
            EnumUtil.ParseFlagDefinitions(definitions, out m_names, out m_displayNames, out m_values);
        }

        /// <summary>
        /// Defines the flag names and values</summary>
        /// <param name="names">Flag names</param>
        /// <param name="values">Flag values</param>
        public void DefineFlags(string[] names, int[] values)
        {
            if (names == null || values == null || names.Length != values.Length)
                throw new ArgumentException("names and/or values null, or of unequal length");

            m_names = names;
            m_displayNames = names;
            m_values = values;
        }

        /// <summary>
        /// Determines whether this instance can convert from the specified context</summary>
        /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that provides a format context</param>
        /// <param name="srcType">A System.Type that represents the type you want to convert from</param>
        /// <returns>True iff this instance can convert from the specified context</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
        {
            return srcType == typeof(string);
        }

        /// <summary>
        /// Converts the given object to the type of this converter, using the specified context and culture information</summary>
        /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that provides a format context</param>
        /// <param name="culture">The <see cref="T:System.Globalization.CultureInfo"></see> to use as the current culture.</param>
        /// <param name="value">The <see cref="T:System.Object"></see> to convert</param>
        /// <returns>An <see cref="T:System.Object"></see> that represents the converted value</returns>
        /// <exception cref="T:System.NotSupportedException">The conversion cannot be performed</exception>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string flagString = value as string;
            if (flagString != null)
            {
                string internalNames = string.Empty;
                string[] displayNames = flagString.Split('|');
                foreach (string displayName in displayNames)
                {
                    string name = GetInternalName(displayName);
                    if (name != null)
                    {
                        if (internalNames != string.Empty)
                            internalNames += '|' + name;
                        else
                            internalNames = name;
                    }
                }
                // Returning the given value allows for editing the enum as long as the DomValueValidator also
                //  permits it. Not returning 'value' but instead calling base.ConvertFrom will throw an
                //  exception and thus prevent the user from creating new enums.
                return internalNames;
            }
            return base.ConvertFrom(context, culture, value);
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

        /// <summary>Converts the given value object to the specified type, using the specified
        /// context and culture information</summary>
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
                int flagsValue = (int)value;
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < m_values.Length; i++)
                {
                    if ((flagsValue & m_values[i]) != 0)
                    {
                        sb.Append(m_names[i]);
                        sb.Append("|");
                    }
                }

                if (sb.Length > 0)
                    sb.Length--; // trim last "|"
                else
                    sb.Append(NoFlags);

                return sb.ToString();
            }
            else if (value is string && destType == typeof(string))
            {
                string displayNames = string.Empty;
                string[] internalNames = ((string)value).Split('|');
                foreach (string internalName in internalNames)
                {
                    string displayName = GetDisplayName(internalName);
                    if (displayName != null)
                    {
                        if (displayNames != string.Empty)
                            displayNames += '|' + displayName;
                        else
                            displayNames = displayName;
                    }
                }
                return displayNames;
            }

            return base.ConvertTo(context, culture, value, destType);
        }

        private string GetInternalName(string displayName)
        {
            for (int i = 0; i < m_displayNames.Length; i++)
            {
                if (m_displayNames[i] == displayName)
                    return m_names[i];
            }
            return null;
        }

        private string GetDisplayName(string internalName)
        {
            for (int i = 0; i < m_names.Length; i++)
            {
                if (m_names[i] == internalName)
                    return m_displayNames[i];
            }
            return null;
        }

        #region IAnnotatedParams Members

        /// <summary>
        /// Initializes the control</summary>
        /// <param name="parameters">Editor parameters</param>
        public void Initialize(string[] parameters)
        {
            DefineFlags(parameters);
        }

        #endregion

        private string[] m_names = EmptyArray<string>.Instance;
        private int[] m_values = EmptyArray<int>.Instance;
        private string[] m_displayNames = EmptyArray<string>.Instance;
        
        private static readonly string NoFlags = "(none)".Localize("No flags");
    }
}

