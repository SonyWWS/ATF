//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.IO;

namespace Sce.Atf
{
    /// <summary>
    /// Directory utilities to enumerate files</summary>
    public static class DirectoryUtil
    {
        /// <summary>
        /// Gets all file paths in given directory and its sub-directories</summary>
        /// <param name="rootPath">Root directory path</param>
        /// <returns>Array of file paths from the given directories</returns>
        /// <remarks>Searches directories and all subdirectories</remarks>
        public static string[] GetFiles(string rootPath)
        {
            return GetFiles(rootPath, "*", SearchOption.AllDirectories);
        }

        /// <summary>
        /// Gets all file paths matching the search pattern in a given directory and its sub-directories</summary>
        /// <param name="rootPath">Root directory path</param>
        /// <param name="searchPattern">Search pattern for matching file paths.
        /// The pattern can contain 2 wildcard characters: "*" matches zero or more characters; 
        /// "?" matches exactly zero or one character.</param>
        /// <returns>Array of file paths matching the search pattern from the given directories</returns>
        /// <remarks>Searches directories and all subdirectories.
        /// </remarks>
        public static string[] GetFiles(string rootPath, string searchPattern)
        {
            return GetFiles(rootPath, searchPattern, SearchOption.AllDirectories);
        }

        /// <summary>
        /// Gets all file paths matching the search pattern in given directory and optional sub-directories</summary>
        /// <param name="rootPath">Root directory path</param>
        /// <param name="searchPattern">Search pattern for matching file paths.
        /// The pattern can contain 2 wildcard characters: "*" matches zero or more characters; 
        /// "?" matches exactly zero or one character.</param>
        /// <param name="option">Whether or not to search recursively, i.e., search sub-directories</param>
        /// <returns>Array of file paths matching the search pattern from the given directory and optional sub-directories</returns>
        /// <remarks>Searches directories and optionally subdirectories</remarks>
        public static string[] GetFiles(string rootPath, string searchPattern, SearchOption option)
        {
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(rootPath, searchPattern, option));
            return files.ToArray();
        }

        /// <summary>
        /// Gets all file paths, one at a time by using 'yield', in the given directory and sub-directories.
        /// This is an efficient way of looking for the first match or first N matches,
        /// because not all of the sub-directories necessarily need to be looked through.</summary>
        /// <param name="rootPath">The directory to begin looking in</param>
        /// <returns>Array of file paths from the given directory and all its sub-directories</returns>
        public static IEnumerable<string> GetFilesIteratively(string rootPath)
        {
            return GetFilesIteratively(rootPath, "*.*");
        }

        /// <summary>
        /// Gets all file paths that match the given search pattern, one at a time by using 'yield', 
        /// in the given directory and sub-directories. Wildcards can be used. This is an
        /// efficient way of looking for the first match or first N matches, because the searching
        /// can be stopped when a match is found.</summary>
        /// <param name="rootPath">The directory to begin looking in</param>
        /// <param name="searchPattern">Search pattern for matching file paths.
        /// The pattern can contain 2 wildcard characters: "*" matches zero or more characters; 
        /// "?" matches exactly zero or one character.</param>
        /// <returns>Array of file paths matching the search pattern in the given directory and all
        /// its sub-directories</returns>
        public static IEnumerable<string> GetFilesIteratively(string rootPath, string searchPattern)
        {
            // search the current directory first
            foreach (string f in Directory.GetFiles(rootPath, searchPattern))
                yield return f;

            // search each sub-directory (depth-first)
            foreach (string d in Directory.GetDirectories(rootPath))
                foreach (string f in GetFilesIteratively(d, searchPattern))
                    yield return f;
        }
    }
}
