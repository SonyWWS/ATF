//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;

namespace Sce.Atf.DirectWrite
{
    /// <summary>
    /// Describes the region obtained by a DirectWrite hit test.
    /// </summary>
    public struct  HitTestMetrics
    {
        /// <summary>
        /// The first text position within the hit region. </summary>
        public int TextPosition;

        /// <summary>
        /// The number of text positions within the hit region. </summary>
        public int Length;

        /// <summary>
        /// An output flag that indicates whether the hit-test location is inside the text string. When False, the position nearest the text's edge is returned.
        /// </summary>
        public bool IsInside;
        
        /// <summary>
        /// An output flag that indicates whether the hit-test location is at the leading or the trailing side of the character. 
        /// When the output IsInside value is set to FALSE, this value is set according to the output TextPosition value to represent the edge closest to the hit-test location.
        /// </summary>
        public bool IsTrailingHit;

        /// <summary>
        /// Pixel location relative to the top-left of the layout box for the text position
        /// </summary>
        public PointF Point;

        /// <summary>
        /// The height of the hit region.</summary>
        public float Height;

        /// <summary>
        /// The width of the hit region.</summary>
        public float Width;

        /// <summary>
        /// Top position of the top-left coordinate of the geometry.</summary>
        public float Top;
    }
}
