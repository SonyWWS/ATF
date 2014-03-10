//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;
using SharpDX;
using SharpDX.DirectWrite;
using ClusterMetrics = Sce.Atf.DirectWrite.ClusterMetrics;
using HitTestMetrics = Sce.Atf.DirectWrite.HitTestMetrics;
using LineMetrics = Sce.Atf.DirectWrite.LineMetrics;

namespace Sce.Atf.Direct2D
{
    /// <summary>
    /// The D2dTextLayout object represents a block of text after it has been
    /// fully analyzed and formatted</summary>
    public class D2dTextLayout : D2dTextFormat
    {
        /// <summary>
        /// Gets text that is formatted using this object</summary>
        /// <remarks>This property cannot be set. A new instance of 
        /// D2dTextFormat must be created per unique string.</remarks>
        public string Text
        {
            get { return m_text; }
        }

        /// <summary>    
        /// Sets strikethrough for text within a specified text range</summary>    
        /// <param name="hasStrikethrough">A Boolean flag that indicates whether 
        /// strikethrough takes place in the range specified by textRange</param>
        /// <param name="startPosition">The start position of the text range</param>
        /// <param name="length">The number of positions in the text range</param>                
        public void SetStrikethrough(bool hasStrikethrough, int startPosition, int length)
        {
            m_nativeTextLayout.SetStrikethrough(hasStrikethrough, new TextRange(startPosition, length));
        }

        /// <summary>    
        /// Sets underlining for text within a specified text range</summary>    
        /// <param name="hasUnderline">A Boolean flag that indicates whether 
        /// underlining takes place within a specified text range</param>        
        /// <param name="startPosition">The start position of the text range</param>
        /// <param name="length">The number of positions in the text range</param>                
        public void SetUnderline(bool hasUnderline, int startPosition, int length)
        {
            m_nativeTextLayout.SetUnderline(hasUnderline, new TextRange(startPosition, length));
        }

        /// <summary>
        /// Gets and sets layout width</summary>
        public float LayoutWidth
        {
            get { return m_nativeTextLayout.MaxWidth; }
            set { m_nativeTextLayout.MaxWidth = value; }
        }

        /// <summary>
        /// Gets and sets layout height</summary>
        public float LayoutHeight
        {
            get { return m_nativeTextLayout.MaxHeight; }
            set { m_nativeTextLayout.MaxHeight = value; }
        }

        /// <summary>
        /// Get a value that indicates the width of the formatted text, while ignoring trailing whitespace at the end of each line.</summary>
        public float Width
        {
            get { return m_nativeTextLayout.Metrics.Width; }
        }

        /// <summary>
        /// Get the height of the formatted text.</summary>
        /// <remarks> The height of an empty string is set to the same value as that of the default font.</remarks>
        public float Height
        {
            get { return m_nativeTextLayout.Metrics.Height; }
        }

        /// <summary>
        /// Get the total number of lines.</summary>
        public int LineCount
        {
            get { return m_nativeTextLayout.Metrics.LineCount; }
        }

        /// <summary>
        /// The application calls this function passing in a specific pixel location relative to the top-left location 
        /// of the layout box and obtains the information about the correspondent hit-test metrics of the text string 
        /// where the hit-test has occurred. When the specified pixel location is outside the text string, 
        /// the function sets the output value IsInside to False.</summary>
        /// <param name="x">The pixel location X to hit-test, relative to the top-left location of the layout box.</param>
        /// <param name="y">The pixel location Y to hit-test, relative to the top-left location of the layout box.</param>
        public HitTestMetrics HitTestPoint(float x, float y)
        {
            Bool isTrailingHit;
            Bool isInside;
            var hitTestMetrics = NativeTextLayout.HitTestPoint(x, y, out isTrailingHit, out isInside);
            var result = new HitTestMetrics
            {
                IsInside = isInside,
                IsTrailingHit = isTrailingHit,
                TextPosition =  hitTestMetrics.TextPosition,
                Length = hitTestMetrics.Length
            };
         
            return result;
        }

        /// <summary>
        /// The application calls this function to get the pixel location relative to the top-left of the layout box 
        /// given the text position and the logical side of the position. 
        /// This function is normally used as part of caret positioning of text where the caret is drawn 
        /// at the location corresponding to the current text editing position. 
        /// It may also be used as a way to programmatically obtain the geometry of a particular text position in UI automation. 
        /// </summary>
        /// <param name="textPosition">The text position used to get the pixel location.</param>
        /// <param name="isTrailingHit">A Boolean flag that whether the pixel location is of the leading or 
        /// the trailing side of the specified text position.</param>
        /// <returns></returns>
        public HitTestMetrics HitTestTextPosition(int textPosition, bool isTrailingHit)
        {
        
            float x, y;
            var hitTestMetrics = NativeTextLayout.HitTestTextPosition(textPosition,  isTrailingHit, out  x, out y );
            var result = new HitTestMetrics
            {             
                TextPosition = hitTestMetrics.TextPosition,
                Length = hitTestMetrics.Length,
                Point = new PointF(x,y),
                Width =  hitTestMetrics.Width,
                Height = hitTestMetrics.Height
            };

            return result;
        }

        /// <summary>
        /// The application calls this function to get a set of hit-test metrics corresponding to a range of text positions. 
        /// One of the main usages is to implement highlight selection of the text string. 
        /// </summary>
        /// <param name="textPosition">The first text position of the specified range.</param>
        /// <param name="textLength">The number of positions of the specified range.</param>
        /// <param name="originX">The origin pixel location X at the left of the layout box. 
        /// This offset is added to the hit-test metrics returned. </param>
        /// <param name="originY">The origin pixel location Y at the top of the layout box. 
        /// This offset is added to the hit-test metrics returned. </param>
        /// <returns>An array of D2dTextHitTestMetrics fully enclosing the specified position range.</returns>
        public HitTestMetrics[] HitTestTextRange(int textPosition, int textLength, float originX, float originY)
        {
            var hitTestMetrics = NativeTextLayout.HitTestTextRange(textPosition, textLength, originX, originY);
            var result = new HitTestMetrics[hitTestMetrics.Length];
            for (int i = 0; i < hitTestMetrics.Length; ++i)
            {
                result[i].TextPosition = hitTestMetrics[i].TextPosition;
                result[i].Length = hitTestMetrics[i].Length;
                result[i].Point = new PointF(hitTestMetrics[i].Left, hitTestMetrics[i].Top);
                result[i].Height = hitTestMetrics[i].Height;
                result[i].Width = hitTestMetrics[i].Width;
                result[i].Top = hitTestMetrics[i].Top;
            }
            return result;
        }

        /// <summary>
        /// Retrieves the information about each individual text line of the text string.</summary>
        public LineMetrics[] GetLineMetrics()
        {
            var lineMetrics = NativeTextLayout.GetLineMetrics();
            var result = new LineMetrics[lineMetrics.Length];
            for (int i = 0; i < lineMetrics.Length; ++i)
            {
                result[i].Baseline = lineMetrics[i].Baseline;
                result[i].Height = lineMetrics[i].Height;
                result[i].IsTrimmed = lineMetrics[i].IsTrimmed;
                result[i].Length = lineMetrics[i].Length;
                result[i].TrailingWhitespaceLength = lineMetrics[i].TrailingWhitespaceLength;
                result[i].NewlineLength = lineMetrics[i].NewlineLength;          
            }
            return result;
        }
        /// <summary>
        /// Retrieves logical properties and measurements of each glyph cluster.
        /// </summary>
        /// <returns>Returns metrics, such as line-break or total advance width, for a glyph cluster. </returns>
        public ClusterMetrics[] GetClusterMetrics()
        {
            var clusterMetrics = NativeTextLayout.GetClusterMetrics();
            var result = new ClusterMetrics[clusterMetrics.Length];
            for (int i = 0; i < clusterMetrics.Length; ++i)
            {
                result[i].Length = clusterMetrics[i].Length;
                result[i].Width = clusterMetrics[i].Width;
                result[i].CanWrapLineAfter = clusterMetrics[i].CanWrapLineAfter;
                result[i].IsNewline = clusterMetrics[i].IsNewline;
                result[i].IsRightToLeft = clusterMetrics[i].IsRightToLeft;
                result[i].IsSoftHyphen = clusterMetrics[i].IsSoftHyphen;
                result[i].IsWhitespace = clusterMetrics[i].IsWhitespace;
            }
            return result;
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            m_nativeTextLayout.Dispose();
        }

        internal D2dTextLayout(string text, TextLayout textLayout)
            : base(textLayout)
        {
            m_nativeTextLayout = textLayout;
            m_text = text;
        }

        internal TextLayout NativeTextLayout
        {
            get { return m_nativeTextLayout; }
        }

        private string m_text;
        private TextLayout m_nativeTextLayout;
    }
}
