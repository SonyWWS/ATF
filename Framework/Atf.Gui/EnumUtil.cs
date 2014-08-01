//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Sce.Atf
{
    /// <summary>
    /// Static utility methods for working with enums</summary>
    public static class EnumDisplayUtil
    {
        /// <summary>
        /// Gets the display string for an enum value, with type and null checks</summary>
        /// <param name="enumType">Type of the enum</param>
        /// <param name="value">Enum value whose display string is requested</param>
        /// <returns>The display string for the specified enum value</returns>
        public static string GetDisplayString(Type enumType, object value)
        {
            Requires.NotNull(enumType, "enumType");
            Requires.NotNull(value, "value");
            Requires.Require<ArgumentException>(enumType.IsEnum, "enumType must by an enum type");

            return GetEnumData(enumType).DisplayStrings[value];
        }

        /// <summary>
        /// Gets the display string for an enum value, with type and null checks</summary>
        /// <typeparam name="TEnum">Type of the enum</typeparam>
        /// <param name="value">Enum value whose display string is requested</param>
        /// <returns>The display string for the specified enum value</returns>
        public static string GetDisplayString<TEnum>(object value)
        {
            Requires.NotNull(value, "value");
            Requires.Require<ArgumentException>(typeof(TEnum).IsEnum, "enumType must by an enum type");

            return GetEnumData(typeof(TEnum)).DisplayStrings[value];
        }

        private static EnumData GetEnumData(Type enumType)
        {
            EnumData result;

            if (!s_cache.TryGetValue(enumType, out result))
            {
                // Generate data
                result = new EnumData(enumType);

                var fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
                foreach (var field in fields)
                {
                    // TODO: localization support
                    string displayString = null;
                    object enumValue = field.GetValue(null);

                    var a = field.GetCustomAttributes(typeof(DisplayStringAttribute), false);

                    if (a != null && a.Length > 0)
                    {
                        displayString = ((DisplayStringAttribute)a[0]).Value;
                    }

                    if (displayString == null)
                    {
                        displayString = field.Name;
                        // displayString = GetBackupDisplayStringValue(enumValue);
                    }

                    result.DisplayStrings.Add(enumValue, displayString);
                    result.EnumValues.Add(displayString, enumValue);
                }

                // Add data to cache
                lock (s_cache)
                {
                    if (!s_cache.ContainsKey(enumType))
                    {
                        s_cache.Add(enumType, result);
                    }
                }
            }

            return result;
        }

        private class EnumData
        {
            public EnumData(Type enumType)
            {
                EnumType = enumType;
            }

            public readonly Type EnumType;

            public readonly Dictionary<object, string> DisplayStrings = new Dictionary<object, string>();

            public readonly Dictionary<string, object> EnumValues = new Dictionary<string, object>();
        }

        private static Dictionary<Type, EnumData> s_cache = new Dictionary<Type, EnumData>();
    }
}
