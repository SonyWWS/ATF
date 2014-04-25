//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.DirectWrite
{
    /// <summary>
    /// Text line metrics</summary>
    public struct LineMetrics
    {
        /// <summary>Distance from the top of text line to its baseline</summary>
        public float Baseline;
        /// <summary>Height of text line</summary>
        public float Height;
        /// <summary>Line is trimmed</summary>
        public bool IsTrimmed;
        /// <summary>Number of text positions in text line. This includes any trailing whitespace and newline characters.</summary>
        public int Length;
        /// <summary>Number of characters in newline sequence at the end of text line. 
        /// If count is zero, text line was either wrapped or it is the end of text.</summary>
        public int NewlineLength;
        /// <summary>Number of whitespace positions at the end of text line. Newline sequences are considered whitespace.</summary>
        public int TrailingWhitespaceLength;
    }
}
