//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf
{
    /// <summary>
    /// Class to assign unique names</summary>
    public class UniqueNamer
    {
        /// <summary>
        /// Constructor</summary>
        /// <remarks>Default suffix separator is '_'</remarks>
        public UniqueNamer()
            : this('_', 1)
        {
        }

        /// <summary>
        /// Constructor specifying separator</summary>
        /// <param name="suffixSeparator">Suffix separator; allowable values are
        /// ' ', '-', '_', '/', '\' and '('</param>
        public UniqueNamer(char suffixSeparator)
            : this(suffixSeparator, 1)
        {
        }

        /// <summary>
        /// Constructor specifying separator and min # digits</summary>
        /// <param name="suffixSeparator">Suffix separator; allowable values are
        /// ' ', '-', '_', '/', '\' and '('</param>
        /// <param name="minNumDigits">The minimum number of digits that the suffix
        /// should be. This helps keeps names in order when sorting.</param>
        public UniqueNamer(char suffixSeparator, int minNumDigits)
        {
            if (suffixSeparator != ' ' &&
                suffixSeparator != '-' &&
                suffixSeparator != '_' &&
                suffixSeparator != '/' &&
                suffixSeparator != '\\' &&
                suffixSeparator != '(')
                throw new ArgumentException("Invalid suffix separator");

            m_separator = suffixSeparator;

            if (minNumDigits > MaxMinNumDigits)
                throw new NotSupportedException("Maximum 10 digits supported");

            m_minNumDigits = minNumDigits;
        }

        private const int MaxMinNumDigits = 10;
        private const string Zeros = "000000000";

        /// <summary>
        /// Checks if the desired name is already taken</summary>
        /// <param name="desired">Desired name</param>
        /// <returns><c>True</c> if the name is taken</returns>
        public bool IsTaken(string desired)
        {
            return m_names.Contains(desired);
        }

        /// <summary>
        /// Gets a unique name based on a desired name. If the desired name is a new, it is left unchanged; otherwise
        /// a suffix of the form "_n" or "(n)" is appended, where n is an integer that makes the name unique.</summary>
        /// <param name="desired">Desired name</param>
        /// <returns>Unique name, based on desired name</returns>
        public string Name(string desired)
        {
            string result = desired;

            if (m_names.Contains(desired))
            {
                // parse this unnamed object into prefix and suffix number
                string root;
                int suffixNumber;
                Parse(desired, out root, out suffixNumber);

                // desired name is taken, generate a unique modification by adding a suffix
                for (int i = 1; ; ++i)
                {
                    result = root + m_separator;

                    string iAsString = i.ToString();
                    if (m_minNumDigits > 1)
                    {
                        int suffixLength = iAsString.Length;
                        if (suffixLength < m_minNumDigits)
                            result += Zeros.Substring(0, m_minNumDigits - suffixLength);
                    }

                    result += iAsString;

                    if (m_separator == '(')
                        result += ')';

                    if (!m_names.Contains(result))
                        break;
                }
            }

            m_names.Add(result);
            return result;
        }

        /// <summary>
        /// Retires a name</summary>
        /// <param name="name">Name to retire</param>
        public void Retire(string name)
        {
            if (m_names.Count != 0) // optimization, if Clear() has been called
                m_names.Remove(name);
        }

        /// <summary>
        /// Changes a name</summary>
        /// <param name="oldName">Old name, to be retired</param>
        /// <param name="newName">New desired name, to be made unique</param>
        /// <returns>Unique name, based on given new name</returns>
        public string Change(string oldName, string newName)
        {
            Retire(oldName);
            return Name(newName);
        }

        /// <summary>
        /// Retires all names</summary>
        public void Clear()
        {
            m_names.Clear();
        }

        /// <summary>
        /// Parses a name previously produced by this unique namer into its component parts</summary>
        /// <param name="name">The previously produced unique name</param>
        /// <param name="root">The root or base part of the name</param>
        /// <param name="suffixNumber">The suffix number, or zero if there was none.</param>
        public void Parse(string name, out string root, out int suffixNumber)
        {
            root = name;
            suffixNumber = 0;

            int separatorIndex = name.LastIndexOf(m_separator);
            if (separatorIndex >= 0)
            {
                int suffixIndex = separatorIndex + 1;
                int suffixLength = name.Length - suffixIndex;
                if (m_separator == '(')
                    suffixLength--;
                string suffix = name.Substring(suffixIndex, suffixLength);
                if (Int32.TryParse(suffix, out suffixNumber))
                    root = name.Substring(0, separatorIndex);
            }
        }

        private readonly HashSet<string> m_names = new HashSet<string>();
        private readonly char m_separator;
        private readonly int m_minNumDigits;
    }
}

