using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Atf.Utilities
{
    public static class PathUtilities
    {
        /// <summary>
        /// Decomposes a path into an array of volume, directory and file name 'elements', without
        /// any of the directory separator characters. Makes searching up and down the hierarchy
        /// easier.</summary>
        /// <param name="path">e.g., @"C:\Program Files\Microsoft SDKs\Windows\v6.0A\bin"</param>
        /// <returns>e.g., {"C:", "Program Files", "Microsoft SDKs", "Windows", "v6.0A", "bin"}</returns>
        public static string[] GetPathElements(string path)
        {
            return path.Split(DirectorySeparatorChars, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Turns an array of volume, directory, and file name elements representing a path
        /// into a path string. A directory separator, '\', is placed at the end of the result
        /// if the last element is truncated, otherwise, a trailing separator is not placed.
        /// </summary>
        /// <param name="elements">e.g., {"C:", "Program Files", "Microsoft SDKs", "Windows", "v6.0A", "bin"}</param>
        /// <param name="first">index of first element</param>
        /// <param name="last">index of last element</param>
        /// <param name="isDirectory"></param>
        /// <returns>e.g., if 'first' is 0 and 'last' is 5, @"C:\Program Files\Microsoft SDKs\Windows\v6.0A\bin"</returns>
        public static string CreatePath(string[] elements, int first, int last, bool isDirectory)
        {
            StringBuilder sb = new StringBuilder();
            
            bool placeTrailingSeparator = isDirectory || (last < elements.Length - 1);

            for(int i = first; i <= last; i++)
            {
                sb.Append(elements[i]);
                if (i < last || placeTrailingSeparator)
                {
                    sb.Append('\\');
                }
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// Does what Directory.Delete() should do -- deletes the directory and everything in it.</summary>
        /// <param name="path">path to the directory to delete</param>
        public static void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                foreach (string filePath in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                {
                    File.Delete(filePath);
                }
                Directory.Delete(path, true);
            }
        }

        public static readonly char[] DirectorySeparatorChars = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
    }
}