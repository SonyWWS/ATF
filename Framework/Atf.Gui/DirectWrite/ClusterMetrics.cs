//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.DirectWrite
{
    public struct ClusterMetrics
    {
        public short Length;
        public float Width;

        public bool CanWrapLineAfter { get; set; }
        public bool IsNewline { get; set; }
        public bool IsRightToLeft { get; set; }
        public bool IsSoftHyphen { get; set; }
        public bool IsWhitespace { get; set; }
    }
}
