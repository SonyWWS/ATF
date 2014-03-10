//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf
{
    /// <summary>
    /// Static class that provides easy access to global localizers. For now, only string
    /// localization is supported.</summary>
    public static class Localizer
    {
        /// <summary>
        /// Sets the global string localizer. This method should be called before starting
        /// up the GUI.</summary>
        /// <param name="stringLocalizer">String localizer</param>
        public static void SetStringLocalizer(StringLocalizer stringLocalizer)
        {
            if (s_stringLocalizer != null)
                throw new InvalidOperationException("Some strings have already been localized by another string localizer");
            s_stringLocalizer = stringLocalizer;
        }

        private static StringLocalizer GetStringLocalizer()
        {
            return s_stringLocalizer ?? (s_stringLocalizer = new StringLocalizer());
        }

        private static StringLocalizer s_stringLocalizer;

        /// <summary>
        /// Returns a localized version of this string</summary>
        /// <param name="s">This string to be localized</param>
        /// <returns>The localized string or this string if no localized string could be found</returns>
        /// <example>Console.WriteLine("Hi, Mom".Localize());</example>
        public static string Localize(this string s)
        {
            return GetStringLocalizer().Localize(s, string.Empty);
        }

        /// <summary>
        /// Returns a localized version of this string</summary>
        /// <param name="s">This string to be localized</param>
        /// <param name="context">The context that this string is used in. This helps guide a human translator
        /// or disambiguates a word that might be translated differently in different contexts.</param>
        /// <returns>The localized string or this string if no localized string could be found</returns>
        /// <example>Console.WriteLine("Hi, Mom".Localize("the salutation of an email"));</example>
        public static string Localize(this string s, string context)
        {
            return GetStringLocalizer().Localize(s, context);
        }
    }
}
