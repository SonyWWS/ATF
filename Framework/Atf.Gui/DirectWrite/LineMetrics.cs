//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.DirectWrite
{
    public struct LineMetrics
    {
        public float Baseline; // The distance from the top of the text line to its baseline.
        public float Height;   // The height of the text line.
        public bool IsTrimmed; // The line is trimmed.
        public int Length; // The number of text positions in the text line. This includes any trailing whitespace and newline characters.
        public int NewlineLength; // The number of characters in the newline sequence at the end of the text line. 
                                 //  If the count is zero, then the text line was either wrapped or it is the end of the text.
        public int TrailingWhitespaceLength; // The number of whitespace positions at the end of the text line. Newline sequences are considered whitespace.
    }
}
