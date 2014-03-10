//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf
{
    /// <summary>
    /// The base class for translating string literals that are embedded in the source code.
    /// The translation occurs at run-time. This default implementation does nothing and so
    /// would be appropriate for English-only applications (because the ATF uses English
    /// literal strings in its source code).</summary>
    public class StringLocalizer
    {
        /// <summary>
        /// Returns a localized version of the user-readable string 's'</summary>
        /// <param name="s">The string to be localized</param>
        /// <param name="context">The context that the string is used in, to help guide a human translator
        /// or to disambiguate a word that might be translated differently in different contexts</param>
        /// <returns>The localized string or the input string 's' if no localized string could be found</returns>
        public virtual string Localize(string s, string context)
        {
            return s;
        }
    }
}
