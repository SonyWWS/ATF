//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Perforce
{
    /// <summary>
    /// Class of static methods for URI paths</summary>
    public static class UriPaths
    {
        /// <summary>
        /// Returns canonical path for URI</summary>
        /// <param name="uri">URI</param>
        /// <returns>Canonical path for URI</returns>
        public static string Path(this Uri uri) { return PathUtil.GetCanonicalPath(uri); }
    }
}
