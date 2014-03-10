//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Perforce
{
    public static class UriPaths
    {
        public static string Path(this Uri uri) { return PathUtil.GetCanonicalPath(uri); }
    }
}
