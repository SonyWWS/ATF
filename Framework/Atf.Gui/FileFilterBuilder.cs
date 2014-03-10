//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Text;

namespace Sce.Atf
{
    /// <summary>
    /// Class to simplify building file filter strings</summary>
    public class FileFilterBuilder
    {
        /// <summary>
        /// Gets the number of file types in the filter</summary>
        public int Count
        {
            get { return m_count; }
        }

        /// <summary>
        /// Adds a human-readable file type name along with associated extensions to this database</summary>
        /// <param name="fileType">Human-readable file type, such as "Text" or "Image"</param>
        /// <param name="extensions">One or more extensions, such as "*.txt" or "*.BMP"</param>
        public void AddFileType(string fileType, params string[] extensions)
        {
            AddFileType(fileType, (IEnumerable<string>)extensions);
        }

        /// <summary>
        /// Adds a human-readable file type name along with associated extensions to this database</summary>
        /// <param name="fileType">Human-readable file type, such as "Text" or "Image"</param>
        /// <param name="extensions">One or more extensions, such as "*.txt" or "*.BMP"</param>
        public void AddFileType(string fileType, IEnumerable<string> extensions)
        {
            // Example filter strings:
            // "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            // "Image files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*"

            if (m_sb.Length > 0)
                m_sb.Append('|');

            m_sb.AppendFormat("{0} files".Localize("For example, 'Circuit files' or 'All files'"), fileType);
            m_sb.Append(" (");
            foreach (string extension in extensions)
            {
                m_extensions.Add(extension);

                AppendExtension(extension);
                m_sb.Append(";");
            }
            m_sb.Length--; // remove trailing ";"
            m_sb.Append(")|");
            foreach (string extension in extensions)
            {
                AppendExtension(extension);
                m_sb.Append(";");
            }
            m_sb.Length--; // remove trailing ";"

            m_count++;
        }

        /// <summary>
        /// Adds a filter for "All files" with an associated file extension of "*.*"</summary>
        public void AddAllFiles()
        {
            if (m_count > 0)
                m_sb.Append("|");
            m_sb.Append("All files".Localize() + " (*.*)|*.*|");
        }

        /// <summary>
        /// Adds a filter named "All" with all of the previously added extensions</summary>
        public void AddAllFilesWithExtensions()
        {
            // Don't append '|' here - AddFileType handles it

            string[] extensions = m_extensions.ToArray();
            m_extensions.Clear();
            AddFileType("All".Localize("As in 'All files'"), extensions);
        }

        /// <summary>
        /// Returns the combined filter string of all the file filters entered so far. Does not
        /// modify any state. Can be used as the filter string for open and save file dialog boxes.</summary>
        /// <returns>File extension filter, e.g., "Setting file (*.xml;*.txt)|*.xml;*.txt|Any (*.*)|*.*"</returns>
        public override string ToString()
        {
            return m_sb.ToString();
        }

        /// <summary>
        /// Creates a filter string for a single file type and its associated file extensions</summary>
        /// <param name="fileType">Human-readable file type, such as "Text" or "Image"</param>
        /// <param name="extensions">One or more extensions, such as "*.txt" or "*.BMP"</param>
        /// <returns>File extension filter, e.g., "Setting file (*.xml;*.txt)|*.xml;*.txt"</returns>
        public static string GetFilterString(string fileType, params string[] extensions)
        {
            return GetFilterString(fileType, (IEnumerable<string>)extensions);
        }

        /// <summary>
        /// Creates a filter string for a single file type and its associated file extensions</summary>
        /// <param name="fileType">Human-readable file type, such as "Text" or "Image"</param>
        /// <param name="extensions">One or more extensions, such as "*.txt" or "*.BMP"</param>
        /// <returns>File extension filter, e.g., "Setting file (*.xml;*.txt)|*.xml;*.txt"</returns>
        public static string GetFilterString(string fileType, IEnumerable<string> extensions)
        {
            var fb = new FileFilterBuilder();
            fb.AddFileType(fileType, extensions);
            return fb.ToString();
        }

        private void AppendExtension(string extension)
        {
            m_sb.Append("*");            
            m_sb.Append(extension);
        }

        private readonly StringBuilder m_sb = new StringBuilder();
        private readonly List<string> m_extensions = new List<string>();
        private int m_count;
    }
}
