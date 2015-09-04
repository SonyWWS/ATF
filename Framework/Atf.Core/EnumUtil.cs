//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Sce.Atf
{
    /// <summary>
    /// Utilities for working with 'enum' types</summary>
    public static class EnumUtil
    {
        /// <summary>
        /// Tries to parse a string into an enum of type T</summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="value">String to parse</param>
        /// <param name="result">If successful, result is set to the matching enum. Otherwise, 
        /// result is set to the default value of type T.</param>
        /// <returns>True iff the string was successfully matched to a member of the enum</returns>
        /// <exception cref="InvalidOperationException"> if T is not actually an enum type</exception>
        public static bool TryParse<T>(string value, out T result)
        {
            // there is no generic constraint "where T : enum", so do a runtime check
            if (!typeof(T).IsEnum)
                throw new InvalidOperationException("can only be used on Enums");

            result = default(T);
            int intValue;
            if (Int32.TryParse(value, out intValue))
            {
                if (Enum.IsDefined(typeof(T), intValue))
                {
                    result = (T)(object)intValue;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Parses enum definitions into name and value arrays</summary>
        /// <param name="enumDefinitions">Enum definition strings</param>
        /// <param name="names">Parsed enum names</param>
        /// <param name="displayNames">Parsed enum display names. Same as 'names' if not specified.</param>
        /// <param name="values">Parsed enum values</param>
        /// <remarks>Enum values default to successive integers, starting with 0. Enum names
        /// with the format "EnumName=X" are parsed so that EnumName gets the value X, where X is
        /// an int.</remarks>
        public static void ParseEnumDefinitions(
            string[] enumDefinitions,
            out string[] names,
            out string[] displayNames,
            out int[] values)
        {
            names = new string[enumDefinitions.Length];
            displayNames = new string[enumDefinitions.Length];
            values = new int[enumDefinitions.Length];

            int enumValue = 0;
            for (int i = 0; i < enumDefinitions.Length; i++)
            {
                string displayName;

                names[i] = ParseDefinition(enumDefinitions[i], out displayName, ref enumValue);
                displayNames[i] = displayName;
                values[i] = enumValue;

                enumValue++;
            }
        }

        /// <summary>
        /// Parses enum definitions into name and value arrays. Ignores display names.</summary>
        /// <param name="enumDefinitions">Enum definition strings</param>
        /// <param name="names">Parsed enum names</param>
        /// <param name="values">Parsed enum values</param>
        /// <remarks>Enum values default to successive integers, starting with 0. Enum names
        /// with the format "EnumName=X" are parsed so that EnumName gets the value X, where X is
        /// an int.</remarks>
        public static void ParseEnumDefinitions(
            string[] enumDefinitions,
            out string[] names,
            out int[] values)
        {
            string[] displayNames;
            ParseEnumDefinitions(enumDefinitions, out names, out displayNames, out values);
        }

        /// <summary>
        /// Parses flag definitions into name and value arrays</summary>
        /// <param name="flagDefinitions">Flag definition strings</param>
        /// <param name="names">Parsed flag names</param>
        /// <param name="displayNames">Parsed flag display names</param>
        /// <param name="values">Parsed flag values</param>
        /// <remarks>Flag values default to successive powers of 2, starting with 1. Flag names
        /// with the format "FlagName=X" are parsed so that FlagName gets the value X, where X is
        /// an int.</remarks>
        public static void ParseFlagDefinitions(
            string[] flagDefinitions,
            out string[] names,
            out string[] displayNames,
            out int[] values)
        {
            names = new string[flagDefinitions.Length];
            displayNames = new string[flagDefinitions.Length];
            values = new int[flagDefinitions.Length];

            int flagValue = 1;
            for (int i = 0; i < flagDefinitions.Length; i++)
            {
                string displayName;
                names[i] = ParseDefinition(flagDefinitions[i], out displayName, ref flagValue);
                displayNames[i] = displayName;

                values[i] = flagValue;
                flagValue *= 2;
            }
        }

        /// <summary>
        /// Parses flag definitions into name and value arrays. Ignores display names.</summary>
        /// <param name="flagDefinitions">Flag definition strings</param>
        /// <param name="names">Parsed flag names</param>
        /// <param name="values">Parsed flag values</param>
        /// <remarks>Flag values default to successive powers of 2, starting with 1. Flag names
        /// with the format "FlagName=X" are parsed so that FlagName gets the value X, where X is
        /// an int.</remarks>
        public static void ParseFlagDefinitions(
            string[] flagDefinitions,
            out string[] names,
            out int[] values)
        {
            string[] displayNames;
            ParseFlagDefinitions(flagDefinitions, out names, out displayNames, out values);
        }

        /// <summary>
        /// Retrieves the display string for enum values marked with a DisplayStringAttribute.
        /// If no attribute is provided, it returns the name of the enum value.</summary>
        /// <param name="enumType">The enum type</param>
        /// <param name="value">The enum value</param>
        /// <returns>A display string for the enum value</returns>
        public static string GetDisplayString(Type enumType, object value)
        {
            Requires.NotNull(enumType, "enumType");
            Requires.NotNull(value, "value");
            Requires.Require<ArgumentException>(enumType.IsEnum, "enumType must by an enum type");

            return GetEnumData(enumType).DisplayStrings[value];
        }

        /// <summary>
        /// Retrieves the display string for enum values marked with a DisplayStringAttribute.
        /// If no attribute is provided, it returns the name of the enum value.</summary>
        /// <typeparam name="T">The enum type</typeparam>
        /// <param name="value">The enum value</param>
        /// <returns>A display string for the enum value</returns>
        public static string GetDisplayString<T>(object value)
        {
            Requires.NotNull(value, "value");
            Requires.Require<ArgumentException>(typeof(T).IsEnum, "enumType must by an enum type");

            return GetEnumData(typeof(T)).DisplayStrings[value];
        }

        // Parses a flag or enum definition into its component parts: name, display name, and integer value.
        private static string ParseDefinition(string definition, out string displayName, ref int value)
        {
            string[] sections = definition.Split('=');

            string name = sections[0];

            if (sections.Length == 1)
            {
                displayName = name;
            }
            else if (sections.Length == 2)
            {
                // e.g., "A=2"
                displayName = name;
                value = int.Parse(sections[1]);
            }
            else if (sections.Length == 3)
            {
                // e.g., "A==Big A"
                displayName = sections[2];
            }
            else if (sections.Length == 4)
            {
                // e.g., "A==Big A=2"
                displayName = sections[2];
                value = int.Parse(sections[3]);
            }
            else
                throw new FormatException(string.Format("This enum or flag definition is bad:{0}", definition));

            return name;
        }

        private static EnumData GetEnumData(Type enumType)
        {
            EnumData result;

            lock (s_cache)
            {
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

                        var a = field.GetCustomAttributes(typeof (DisplayStringAttribute), false);

                        if (a != null && a.Length > 0)
                        {
                            displayString = ((DisplayStringAttribute) a[0]).Value;
                        }

                        if (displayString == null)
                        {
                            displayString = field.Name;
                        }

                        // Type Dictionary<> asserts on redundant key adds.
                        // So ignore enum entries having the same value
                        // as a previous entry.
                        //
                        // If this is problematic, remove line below, and
                        // re-type DisplayStrings as a multimap.  However, 
                        // doing so might constitute a breaking change.

                        if (result.DisplayStrings.ContainsKey(enumValue))
                            continue;

                        result.DisplayStrings.Add(enumValue, displayString);
                    }

                    // Add data to cache
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
        }

        private static Dictionary<Type, EnumData> s_cache = new Dictionary<Type, EnumData>();
    }
}
