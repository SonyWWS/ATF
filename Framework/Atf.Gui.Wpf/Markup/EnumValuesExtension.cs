//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;

namespace Sce.Atf.Wpf.Markup
{
    /// <summary>
    /// Extension for working with enum values</summary>
    [ContentProperty("EnumType")]
    [MarkupExtensionReturnType(typeof(object[]))]
    public class EnumValuesExtension : MarkupExtension
    {
        /// <summary>
        /// Constructor</summary>
        public EnumValuesExtension()
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="enumType">Type of the enum</param>
        public EnumValuesExtension(Type enumType)
        {
            Requires.NotNull(enumType, "The enum type is not set");

            EnumType = enumType;
        }

        /// <summary>
        /// Type of the enum</summary>
        [ConstructorArgument("enumType")]
        public Type EnumType
        {
            get { return m_enumType; }
            private set
            {
                if (m_enumType != value)
                {
                    var enumType = Nullable.GetUnderlyingType(value) ?? value;

                    if (enumType.IsEnum == false)
                        throw new ArgumentException("Type must be an Enum.");

                    m_enumType = value;
                }
            }
        }

        /// <summary>
        /// Returns an array of EnumerationMembers representing the enum values of EnumType</summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var enumValues = Enum.GetValues(EnumType);
            var result =
                from object enumValue in enumValues
                select new EnumerationMember
                           {
                               Value = enumValue,
                               Description = GetDescription(enumValue),
                               DisplayString = GetDisplayString(enumValue)
                           };
            
            return result.ToArray();
        }

        /// <summary>
        /// Class representing an enumeration value</summary>
        public class EnumerationMember
        {
            /// <summary>
            /// Gets and sets the enum value</summary>
            public object Value { get; set; }

            /// <summary>
            /// Gets and sets the enum value display string</summary>
            public string DisplayString { get; set; }

            /// <summary>
            /// Gets and sets the enum value description</summary>
            public string Description { get; set; }

            /// <summary>
            /// Converts the EnumerationMember to a string</summary>
            /// <returns>The DisplayString if it is not null or whitespace-only, otherwise returns Value.ToString()</returns>
            public override string ToString()
            {
                return string.IsNullOrWhiteSpace(DisplayString) ? Value.ToString() : DisplayString;
            }
        }

        private string GetDescription(object enumValue)
        {
            var descriptionAttribute =
                EnumType
                    .GetField(enumValue.ToString())
                    .GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .FirstOrDefault() as DescriptionAttribute;

            return descriptionAttribute != null
                       ? descriptionAttribute.Description
                       : enumValue.ToString();
        }

        private string GetDisplayString(object enumValue)
        {
            var displayStringAttribute =
                EnumType
                    .GetField(enumValue.ToString())
                    .GetCustomAttributes(typeof(DisplayStringAttribute), false)
                    .FirstOrDefault() as DisplayStringAttribute;

            return displayStringAttribute != null
                       ? displayStringAttribute.Value
                       : enumValue.ToString();
        }

        private Type m_enumType;
    }
}
