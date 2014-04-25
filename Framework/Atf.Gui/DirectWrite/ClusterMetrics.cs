//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.DirectWrite
{
    /// <summary>
    /// Glyph cluster (group of glyphs) metrics</summary>
    /// <remarks>For more information, see http://sharpdx.org/documentation/api/t-sharpdx-directwrite-clustermetrics. </remarks>
    public struct ClusterMetrics
    {
        /// <summary>
        /// Number of text positions in cluster</summary>
        public short Length;
        /// <summary>
        /// Total advance width of all glyphs in cluster</summary>
        public float Width;

        /// <summary>
        /// Whether line can be broken right after cluster</summary>
        public bool CanWrapLineAfter { get; set; }
        /// <summary>
        /// Whether cluster corresponds to newline character</summary>
        public bool IsNewline { get; set; }
        /// <summary>
        /// Whether cluster is read from right to left</summary>
        public bool IsRightToLeft { get; set; }
        /// <summary>
        /// Whether cluster corresponds to soft hyphen character</summary>
        public bool IsSoftHyphen { get; set; }
        /// <summary>
        /// Whether cluster corresponds to whitespace character</summary>
        public bool IsWhitespace { get; set; }
    }
}
