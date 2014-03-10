//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Text;

namespace Sce.Atf
{
    /// <summary>
    /// String utilities</summary>
    public static class StringUtil
    {
        /// <summary>
        /// Tests if a string is null, empty, or whitespace</summary>
        /// <param name="s">String to test</param>
        /// <returns>True iff string is null, empty, or whitespace</returns>
        public static bool IsNullOrEmptyOrWhitespace(string s)
        {
            if (string.IsNullOrEmpty(s))
                return true;
            string str2 = s.Trim();
            return str2.Length == 0;
        }

        /// <summary>
        /// Removes all whitespace in a string</summary>
        /// <param name="s">String</param>
        /// <returns>String without whitespace</returns>
        public static string RemoveAllWhiteSpace(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            StringBuilder result = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (!char.IsWhiteSpace(c))
                    result.Append(c);
            }
            return result.ToString();
        }

        /// <summary>
        /// Gets the Unicode code points, also known as Unicode scalar values, of the string</summary>
        /// <param name="text">Being a .NET string, it consists of Unicode characters (UTF-16 format)</param>
        /// <returns>The code points. May have a Count less than the Length property of 'text' due to
        /// surrogate pairs being combined.</returns>
        public static IList<int> GetUnicodeCodePoints(string text)
        {
            List<int> codePoints = new List<int>(text.Length);

            int highSurrogate = 0;
            foreach (char c in text)
            {
                int codePoint = c;

                // This little formula comes from section 3.7 of this Unicode standard:
                // http://www.unicode.org/unicode/uni2book/ch03.pdf
                if (codePoint >= 0xD800 && codePoint <= 0xDFFF) //Is part of a surrogate pair?
                {
                    if (codePoint <= 0xDBFF) //Is a high surrogate?
                    {
                        highSurrogate = codePoint;
                        continue;
                    }
                    else //Is a low surrogate. Combine with previously found high surrogate.
                    {
                        codePoint = (highSurrogate - 0xD800) * 0x400 + (codePoint - 0xDC00) + 0x10000;
                    }
                }

                codePoints.Add(codePoint);
            }

            return codePoints;
        }

        /// <summary>
        /// Compares the two strings using a "natural" ordering where a numerical suffix takes precedence
        /// over alphabetical sorting, e.g., "test10" comes after "test2", "name(15)" after "name(8)", etc.</summary>
        /// <param name="strA">first string</param>
        /// <param name="strB">second string</param>
        /// <returns>-1 if strA should come before strB, 0 if they are equal, and 1 if strB should come after strA</returns>
        public static int CompareNaturalOrder(string strA, string strB)
        {
            // Parse the strings into these parts: [base][number][suffix]
            // Find the 'base'
            int endBase = 0;
            while (endBase < strA.Length && endBase < strB.Length && strA[endBase] == strB[endBase])
                endBase++;

            // Find the 'number'
            int endNumA = endBase;
            while (endNumA < strA.Length && Char.IsDigit(strA[endNumA]))
                endNumA++;
            int endNumB = endBase;
            while (endNumB < strB.Length && Char.IsDigit(strB[endNumB]))
                endNumB++;

            // If there are #s for both strings, then compare the #s.
            if (endNumA > endBase && endNumB > endBase)
            {
                // Attempt to parse the #s. Use 'TryParse' in case the numbers are too big.
                long numA, numB;
                if (Int64.TryParse(strA.Substring(endBase, endNumA - endBase), out numA) &&
                    Int64.TryParse(strB.Substring(endBase, endNumB - endBase), out numB))
                {
                    if (numA < numB) return -1;
                    if (numA > numB) return 1;
                }
            }

            // If the #s are the same or if one or both strings has no #s, then just do string comparison.
            return string.Compare(strA, strB);
        }

        /// <summary>
        /// Inserts a backslash ('\') before all single and double quotes</summary>
        /// <param name="source"></param>
        /// <returns>String with quotes escaped</returns>
        public static string EscapeQuotes(string source)
        {
            var sb = new StringBuilder(source);

            sb.Replace("\"", "\\\"");
            sb.Replace("'", "\\'");

            return sb.ToString();
        }
    }
}
