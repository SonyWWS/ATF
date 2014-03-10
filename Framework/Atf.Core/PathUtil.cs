//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Sce.Atf
{
    /// <summary>
    /// Path utilities</summary>
    public static class PathUtil
    {
        /// <summary>
        /// Gets relative path from absolute path</summary>
        /// <param name="absolutePath">Absolute path</param>
        /// <param name="basePath">Base path</param>
        /// <returns>Relative path, or null if it could not be determined</returns>
        /// <remarks>Consider using Uri.MakeRelativeUri() instead</remarks>
        public static string GetRelativePath(string absolutePath, string basePath)
        {
            // trim trailing slashes
            basePath = basePath.TrimEnd(Slashes);
            absolutePath = absolutePath.TrimEnd(Slashes);

            StringBuilder path = new StringBuilder(MAX_PATH);
            if (PathRelativePathTo(path, basePath, FILE_ATTRIBUTE_DIRECTORY, absolutePath, FILE_ATTRIBUTE_DIRECTORY))
                return path.ToString();
            else
                return null;
        }

        /// <summary>
        /// Gets absolute path from a base path and a relative path</summary>
        /// <param name="path">Relative path</param>
        /// <param name="basePath">Base path</param>
        /// <returns>Absolute path, or null if it could not be determined</returns>
        public static string GetAbsolutePath(string path, string basePath)
        {
            string combinedPath = Path.Combine(basePath, path);
            StringBuilder absPath = new StringBuilder(MAX_PATH);
            if (PathCanonicalize(absPath, combinedPath))
                return absPath.ToString();
            else
                return null;
        }

        /// <summary>
        /// Given the current thread's language and region setting, the final cultural-specific
        /// filename is returned. The satellite assembly model is followed, and so the
        /// search is done in the following order, for example:
        /// \bin\schema\ja-JP\file.xsd   -- holds language and region specific version. If missing, then...
        /// \bin\schema\ja\file.xsd      -- holds language-specific and region-neutral version. If missing, then...
        /// \bin\schema\file.xsd         -- holds default (English) version.</summary>
        /// <param name="defaultPath">Path and filename of the default (English) version of the file</param>
        /// <returns>Filename that best matches the current culture setting for this thread</returns>
        public static string GetCulturePath(string defaultPath)
        {
            string language = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName; //"en" or "ja"
            string culture = Thread.CurrentThread.CurrentUICulture.Name; //"en-US" or "ja-JP"

            // In the satellite assembly model, the English version is in the root directory.
            // Let's have a quick return in this case to avoid uselessly searching for \en-US and \en
            // sub-directories. Yes, this doesn't work if we port ATF to English-Australia. Unlikely!
            if (language == "en")
                return defaultPath;

            string file = Path.GetFileName(defaultPath);
            string baseDir = Path.GetDirectoryName(defaultPath);

            // First try language and region-specific directory.
            string dirAttempt = Path.Combine(baseDir, culture);
            string attempt = Path.Combine(dirAttempt, file);
            if (File.Exists(attempt))
                return attempt;

            // Next try language-specific, but region-neutral version. It's possible that language==culture.
            if (language != culture)
            {
                dirAttempt = Path.Combine(baseDir, language);
                attempt = Path.Combine(dirAttempt, file);
                if (File.Exists(attempt))
                    return attempt;
            }

            // No culture-specific version exists. Use the default (English).
            return defaultPath;
        }

        /// <summary>
        /// Returns a value indicating if the given path is valid in Windows</summary>
        /// <param name="filePath">File path</param>
        /// <returns>True if the given path is valid in Windows</returns>
        public static bool IsValidPath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            // no leading or trailing whitespace, although technically, leading spaces are allowed in Windows
            if (filePath.Length != filePath.Trim().Length)
                return false;

            if (filePath.Length > MAX_PATH)
                return false;

            if (filePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                return false;

            // no trailing period
            if (filePath.EndsWith("."))
                return false;

            return true;
        }

        /// <summary>
        /// Tests if path is relative</summary>
        /// <param name="path">Path tested</param>
        /// <returns>True iff path is relative</returns>
        public static bool IsRelative(string path)
        {
            return PathIsRelative(path);
        }

        /// <summary>
        /// Tests whether path is absolute</summary>
        /// <param name="path">Path tested</param>
        /// <returns>True iff path is absolute</returns>
        public static bool IsAbsolute(string path)
        {
            return !PathIsRelative(path);
        }

        /// <summary>
        /// Gets the canonical path for the URI</summary>
        /// <param name="uri">URI of item</param>
        /// <returns>Canonical path for the URI</returns>
        /// <remarks>This method correctly handles "subst" drive prefixes</remarks>
        public static string GetCanonicalPath(Uri uri)
        {
            string result = uri.AbsolutePath;
            result = Uri.UnescapeDataString(result);
            return GetCanonicalPath(result);
        }

        /// <summary>
        /// Gets the canonical version of the given path</summary>
        /// <param name="path">Path</param>
        /// <returns>Canonical version of the given path</returns>
        /// <remarks>This method correctly handles "subst" drive prefixes</remarks>
        public static string GetCanonicalPath(string path)
        {
            path = Path.GetFullPath(path);
            string drive = path.Substring(0, 2);
            StringBuilder sb = new StringBuilder(256);
            Kernel32.QueryDosDeviceW(drive, sb, 256);
            string device = sb.ToString();

            const string cSubstDrivePrefix = @"\??\";
            if (device.StartsWith(cSubstDrivePrefix))
            {
                path = device.Substring(cSubstDrivePrefix.Length) + path.Substring(2);
            }

            return path;
        }

        /// <summary>
        /// Shortens the given path to fit within the given length, putting in ellipses to
        /// indicate the missing directories</summary>
        /// <param name="path">Input path to be shortened</param>
        /// <param name="length">Maximum number of characters to place in the result</param>
        /// <returns>The possibly shortened path, with ellipses if necessary</returns>
        public static string GetCompactedPath(string path, int length)
        {
            StringBuilder result = new StringBuilder();
            PathCompactPathEx(result, path, length, 0);
            return result.ToString();
        }

        /// <summary>
        /// Gets the last element of a path</summary>
        /// <param name="path">Path, e.g., @"C:\Program Files\Microsoft SDKs\Windows\v6.0A\bin"</param>
        /// <returns>Last element, e.g., "bin"</returns>
        public static string GetLastElement(string path)
        {
            int last;
            for (last = path.Length - 1; last >= 0; last--)
            {
                if (path[last] != Path.DirectorySeparatorChar &&
                    path[last] != Path.AltDirectorySeparatorChar)
                {
                    break;
                }
            }

            int first;
            for (first = last - 1; first >= 0; first--)
            {
                if (path[first] == Path.DirectorySeparatorChar ||
                    path[first] == Path.AltDirectorySeparatorChar)
                {
                    first++;
                    break;
                }
            }

            if (first < 0)
                first = 0;
            if (last <= first)
                return string.Empty;

            return path.Substring(first, last - first + 1);
        }

        /// <summary>
        /// Decomposes a path into an array of volume, directory and file name 'elements', without
        /// any of the directory separator characters. Makes searching up and down the hierarchy
        /// easier.</summary>
        /// <param name="path">Path, e.g., @"C:\Program Files\Microsoft SDKs\Windows\v6.0A\bin"</param>
        /// <returns>Decomposed path, e.g., {"C:", "Program Files", "Microsoft SDKs", "Windows", "v6.0A", "bin"}</returns>
        public static string[] GetPathElements(string path)
        {
            return path.Split(DirectorySeparatorChars, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// An array of .NET directory separator characters ('\' and '/' in Windows)</summary>
        public static readonly char[] DirectorySeparatorChars = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern bool PathRelativePathTo(StringBuilder path, string from, int attrFrom, string to, int attrTo);

        // Replaces the directory navigation text like "\\..\\" and "\\.\\"
        //  http://msdn.microsoft.com/en-us/library/bb773569(VS.85).aspx
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern bool PathCanonicalize(StringBuilder path, string src);

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern bool PathIsRelative(string path);

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern bool PathCompactPathEx([Out] StringBuilder resultPath, string originalPath, int maxResultChars, int dwFlags);

        // from MAPIWIN.h :
        private const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
        private const int MAX_PATH = 260;

        private static readonly char[] Slashes = new[] { '/', '\\' };
    }
}

