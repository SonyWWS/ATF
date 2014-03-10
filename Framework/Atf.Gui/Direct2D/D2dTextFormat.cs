//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using SharpDX.DirectWrite;

namespace Sce.Atf.Direct2D
{
    /// <summary>
    /// The D2dTextFormat describes the font and paragraph properties used to format text, 
    /// and it describes locale information</summary>
    public class D2dTextFormat : D2dResource
    {       
        /// <summary>
        /// Gets the font family name</summary>
        public string FontFamilyName 
        {
            get { return m_nativeTextFormat.FontFamilyName; }
        }

        /// <summary>
        /// Gets the font size in DIP (Device Independent Pixels) units</summary>
        public float FontSize 
        {
            get { return m_nativeTextFormat.FontSize; }
        }
        
        /// <summary>
        /// Gets the height of the font</summary>
        public float FontHeight
        {
            get { return m_fontHeight; }
        }
             
        /// <summary>
        /// Gets or sets the alignment option of a paragraph that is relative to the top and
        /// bottom edges of a layout box</summary>
        public D2dParagraphAlignment ParagraphAlignment 
        {
            get { return (D2dParagraphAlignment)m_nativeTextFormat.ParagraphAlignment; }
            set 
            { 
                m_nativeTextFormat.ParagraphAlignment = (ParagraphAlignment)value;
            }
        }

        /// <summary>
        /// Gets or sets the current reading direction for text in a paragraph</summary>
        public D2dReadingDirection ReadingDirection 
        {
            get { return (D2dReadingDirection)m_nativeTextFormat.ReadingDirection; }
            set
            {
                m_nativeTextFormat.ReadingDirection = (ReadingDirection)value;
            }                
        }
               
        /// <summary>
        /// Gets or sets the alignment option of text relative to the layout box's leading and
        /// trailing edge</summary>
        public D2dTextAlignment TextAlignment 
        {
            get { return (D2dTextAlignment)m_nativeTextFormat.TextAlignment; }
            set 
            {
                m_nativeTextFormat.TextAlignment = (TextAlignment)value;
            }
        }

        /// <summary>
        /// Gets or sets the word wrapping option</summary>
        public D2dWordWrapping WordWrapping 
        {
            get { return (D2dWordWrapping)m_nativeTextFormat.WordWrapping; }
            set 
            {
                m_nativeTextFormat.WordWrapping = (WordWrapping)value;                
            }
        }


        /// <summary>
        /// Gets or sets the trimming options for text that overflows the layout box</summary>
        public D2dTrimming Trimming
        {
            get
            {
                D2dTrimming trimmingOptions;
                InlineObject trimmingSign;
                Trimming tmpTrimming;

                m_nativeTextFormat.GetTrimming(out tmpTrimming, out trimmingSign);
                trimmingOptions.Delimiter = tmpTrimming.Delimiter;
                trimmingOptions.DelimiterCount = tmpTrimming.DelimiterCount;
                trimmingOptions.Granularity =(D2dTrimmingGranularity) tmpTrimming.Granularity;
                return trimmingOptions;
            }

            set
            {
                Trimming trimming = new Trimming();
                trimming.Delimiter = value.Delimiter;
                trimming.DelimiterCount = value.DelimiterCount;
                trimming.Granularity = (TrimmingGranularity)value.Granularity;

                IntPtr inlineObj = (value.Granularity != D2dTrimmingGranularity.None) ?
                m_ellipsisTrimming.NativePointer : IntPtr.Zero;

                object[] args = { trimming, inlineObj };
                SetTrimmingInfo.Invoke(m_nativeTextFormat, args);
            }
        }

        /// <summary>
        /// Gets and sets text snapping and clipping options.
        /// Default value:
        ///   Text is vertically snapped to pixel boundaries
        ///   and it is clipped to the layout rectangle.</summary>
        public D2dDrawTextOptions DrawTextOptions
        {
            get {return m_drawTextOptions; }
            set { m_drawTextOptions = value; }
        }


        /// <summary>
        /// Gets and sets whether the text is underlined</summary>
        public bool Underlined
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets whether the text has the strike-out effect</summary>
        public bool Strikeout
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the line spacing adjustment set for a multiline text paragraph</summary>
        /// <param name="lineSpacingMethod">A value that indicates how line height is determined</param>
        /// <param name="lineSpacing">When this method returns, contains the line height, 
        /// or distance between one baseline to another</param>
        /// <param name="baseline">When this method returns, contains the distance from top of line to baseline.
        /// A reasonable ratio to lineSpacing is 80 percent.</param>
        /// <returns>If the method succeeds, it returns D2dResult.Ok. 
        /// Otherwise, it throws an exception.</returns>
        public D2dResult GetLineSpacing(
            out D2dLineSpacingMethod lineSpacingMethod, 
            out float lineSpacing, 
            out float baseline)
        {
            LineSpacingMethod tmpLineSpacingMethod;
            m_nativeTextFormat.GetLineSpacing(
                out tmpLineSpacingMethod,
                out lineSpacing,
                out baseline);

            lineSpacingMethod = (D2dLineSpacingMethod)tmpLineSpacingMethod;
            return D2dResult.Ok;
        }

        /// <summary>
        /// Sets the line spacing</summary>
        /// <param name="lineSpacingMethod">Specifies how line height is being determined; see D2dLineSpacingMethod
        /// for more information</param>
        /// <param name="lineSpacing">The line height, or distance between one baseline to another</param>
        /// <param name="baseline">The distance from top of line to baseline. A reasonable ratio to lineSpacing
        /// is 80 percent.</param>
        /// <returns>If the method succeeds, it returns D2dResult.OK. Otherwise, it throws an exception.</returns>
        /// <remarks>
        /// For the default method, spacing depends solely on the content. For uniform
        /// spacing, the specified line height overrides the content.</remarks>
        public D2dResult SetLineSpacing(D2dLineSpacingMethod lineSpacingMethod, float lineSpacing, float baseline)
        {
            m_nativeTextFormat.SetLineSpacing((LineSpacingMethod)lineSpacingMethod, lineSpacing, baseline);
            return D2dResult.Ok;
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            m_ellipsisTrimming.Dispose();
            m_nativeTextFormat.Dispose();
            base.Dispose(disposing);
        }

        internal TextFormat NativeTextFormat
        {
            get { return m_nativeTextFormat; }
        }

        internal D2dTextFormat(TextFormat textFormat)
        {
            if (textFormat == null)
                throw new ArgumentNullException("textFormat");
            m_nativeTextFormat = textFormat;

            m_ellipsisTrimming = new EllipsisTrimming(D2dFactory.NativeDwFactory
                , m_nativeTextFormat);

            // it seems SharpDX guys forgot to expose SetTrimming(..) method.
            // for now use reflection to access it. 
            if(SetTrimmingInfo == null)
            {
                SetTrimmingInfo = textFormat.GetType().GetMethod("SetTrimming_"
                    , System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            }
            
            m_fontHeight =(float)Math.Ceiling(m_nativeTextFormat.FontSize * 1.2);
        }
       
        private static System.Reflection.MethodInfo SetTrimmingInfo;
        private TextFormat m_nativeTextFormat;
        private EllipsisTrimming m_ellipsisTrimming;
        private D2dDrawTextOptions m_drawTextOptions = D2dDrawTextOptions.None;
        private float m_fontHeight;
    }
     
    /// <summary>
    /// Enum that specifies the alignment of paragraph text along the flow direction axis,
    /// relative to the top and bottom of the flow's layout box</summary>
    public enum D2dParagraphAlignment
    {        
        /// <summary>
        /// The top of the text flow is aligned to the top edge of the layout box</summary>
        Near = 0,

        /// <summary>
        /// The bottom of the text flow is aligned to the bottom edge of the layout box</summary>
        Far = 1,

        /// <summary>
        /// The center of the flow is aligned to the center of the layout box</summary>
        Center = 2,
    }
    
    /// <summary>
    /// Enum that specifies the direction in which reading progresses</summary>
    public enum D2dReadingDirection
    {        
        /// <summary>
        /// Indicates that reading progresses from left to right</summary>
        LeftToRight = 0,

        /// <summary>
        /// Indicates that reading progresses from right to left</summary>
        RightToLeft = 1,
    }

    /// <summary>
    /// Enum that specifies the alignment of paragraph text along the reading direction axis,
    /// relative to the leading and trailing edge of the layout box</summary>
    public enum D2dTextAlignment
    {     
        /// <summary>
        /// The leading edge of the paragraph text is aligned to the leading edge of
        /// the layout box</summary>
        Leading = 0,
        
        /// <summary>
        /// The trailing edge of the paragraph text is aligned to the trailing edge of
        /// the layout box</summary>
        Trailing = 1,

        /// <summary>
        /// The center of the paragraph text is aligned to the center of the layout box</summary>
        Center = 2,
    }
    
    /// <summary>
    /// Enum that specifies the word wrapping to be used in a particular multiline paragraph</summary>
    public enum D2dWordWrapping
    {
        /// <summary>
        /// Indicates that words are broken across lines to avoid text 
        /// overflowing the layout box</summary>
        Wrap = 0,

        /// <summary>
        /// Indicates that words are kept within the same line even when it overflows
        /// the layout box. This option is often used with scrolling to reveal overflow text.</summary>
        NoWrap = 1,
    }

    /// <summary>
    /// Enum that specifies the method used for line spacing in a text layout</summary>
    /// <remarks>
    /// The line spacing method is set by using the SetLineSpacing method of
    /// the D2dTextFormat or D2dTextLayout.
    /// To get the current line spacing method of a text format or textlayout, use
    /// the GetLineSpacing method.</remarks>    
    public enum D2dLineSpacingMethod
    {        
        /// <summary>
        /// Line spacing depends solely on the content, adjusting to accommodate the
        /// size of fonts and inline objects</summary>
        Default = 0,
               
        /// <summary>
        /// Lines are explicitly set to uniform spacing, regardless of the size of fonts
        /// and inline objects. This can be useful to avoid the uneven appearance that
        /// can occur from font fallback.</summary>
        Uniform = 1,
    }
    
    /// <summary>
    /// Enum that specifies the text granularity used to trim text overflowing the layout box</summary>
    public enum D2dTrimmingGranularity
    {        
        /// <summary>
        /// No trimming occurs. Text flows beyond the layout width.</summary>
        None = 0,

        /// <summary>
        /// Trimming occurs at a character cluster boundary</summary>
        Character = 1,

        /// <summary>
        /// Trimming occurs at a word boundary</summary>
        Word = 2,
    }


    /// <summary>
    /// Struct that specifies the trimming option for text overflowing the layout box</summary>
    public struct D2dTrimming
    {               
        /// <summary>
        /// A character code used as the delimiter that signals the beginning of the
        /// portion of text to be preserved. Most useful for path ellipsis, where the
        /// delimiter would be a slash.</summary>
        public int Delimiter;

        /// <summary>
        /// A value that indicates how many occurrences of the delimiter to step back</summary>
        public int DelimiterCount;
        
        /// <summary>
        /// A value that specifies the text granularity used to trim text overflowing
        /// the layout box</summary>
        public D2dTrimmingGranularity Granularity;
    }

    /// <summary>
    /// Enum that specifies whether text snapping is suppressed-or-clipping to the layout rectangle
    /// is enabled. This enumeration allows a bitwise combination of its member values.</summary>
    [Flags]
    public enum D2dDrawTextOptions
    {
        /// <summary>
        /// Text is vertically snapped to pixel boundaries and is not clipped to the
        /// layout rectangle</summary>
        None = 0,

        /// <summary>
        /// Text is not vertically snapped to pixel boundaries. This setting is recommended
        /// for text that is being animated.</summary>
        NoSnap = 1,

        /// <summary>
        /// Text is clipped to the layout rectangle</summary>
        Clip = 2,
    }

    /// <summary>
    /// Enum that represents the density of a typeface, in terms of the lightness or heaviness
    /// of the strokes. The enumerated values correspond to the usWeightClass definition
    /// in the OpenType specification. The usWeightClass represents an integer value
    /// between 1 and 999. Lower values indicate lighter weights; higher values indicate
    /// heavier weights.</summary>
    /// <remarks>
    /// Weight differences are generally differentiated by an increased stroke or
    /// thickness that is associated with a given character in a typeface, as compared
    /// to a "normal" character from that same typeface.</remarks>
    public enum D2dFontWeight
    {        
        /// <summary>
        /// Predefined font weight : Thin (100)</summary>
        Thin = 100,

        /// <summary>
        /// Predefined font weight : Extra-light (200)</summary>
        ExtraLight = 200,

        /// <summary>
        /// Predefined font weight : Ultra-light (200)</summary>
        UltraLight = 200,

        /// <summary>
        /// Predefined font weight : Light (300)</summary>
        Light = 300,

        /// <summary>
        /// Predefined font weight : Normal (400)</summary>
        Normal = 400,

        /// <summary>
        /// Predefined font weight : Regular (400)</summary>
        Regular = 400,

        /// <summary>
        /// Predefined font weight : Medium (500)</summary>
        Medium = 500,
        
        /// <summary>
        /// Predefined font weight : Demi-bold (600)</summary>
        DemiBold = 600,

        /// <summary>
        /// Predefined font weight : Semi-bold (600)</summary>
        SemiBold = 600,

        /// <summary>
        /// Predefined font weight : Bold (700)</summary>
        Bold = 700,

        /// <summary>
        /// Predefined font weight : Ultra-bold (800)</summary>
        UltraBold = 800,

        /// <summary>
        /// Predefined font weight : Extra-bold (800)</summary>
        ExtraBold = 800,

        /// <summary>
        /// Predefined font weight : Heavy (900)</summary>
        Heavy = 900,

        /// <summary>
        /// Predefined font weight : Black (900)</summary>
        Black = 900,

        /// <summary>
        /// Predefined font weight : Extra-black (950)</summary>
        ExtraBlack = 950,

        /// <summary>
        /// Predefined font weight : Ultra-black (950)</summary>
        UltraBlack = 950,
    }

    /// <summary>
    /// Enum that represents the style of a font face as normal, italic, or oblique</summary>
    /// <remarks>
    /// Three terms categorize the slant of a font:
    /// Normal: The characters in a normal, or roman, font are upright.  
    /// Italic: The characters in an italic font are truly slanted and appear as they were designed.
    /// Oblique: The characters in an oblique font are artificially slanted. 
    /// For Oblique, the slant is achieved by performing a shear transformation on the characters 
    /// from a normal font. When a true italic font is not available on a computer or printer, 
    /// an oblique style can be generated from the normal font and used to simulate an italic font.
    /// The following illustration shows the normal, italic, and oblique font styles for the Palatino 
    /// Linotype font. Notice how the italic font style has a more flowing and visually appealing 
    /// appearance than the oblique font style, which is simply created by skewing the normal font 
    /// style version of the text.</remarks>
    public enum D2dFontStyle
    {
        /// <summary>
        /// Font style : Normal</summary>
        Normal = 0,

        /// <summary>
        /// Font style : Oblique</summary>
        Oblique = 1,

        /// <summary>
        /// Font style : Italic</summary>
        Italic = 2,
    }

    /// <summary>
    /// Enum that represents the degree to which a font has been stretched compared to a font's
    /// normal aspect ratio. The enumerated values correspond to the usWidthClass
    /// definition in the OpenType specification. The usWidthClass represents an
    /// integer value between 1 and 9. Lower values indicate narrower widths; higher
    /// values indicate wider widths.</summary>
    /// <remarks>
    /// A font stretch describes the degree to which a font form is stretched from
    /// its normal aspect ratio, which is the original width to height ratio specified
    /// for the glyphs in the font.</remarks>
    public enum D2dFontStretch
    {       
        /// <summary>
        /// Predefined font stretch : Not known (0)</summary>
        Undefined = 0,

        /// <summary>
        /// Predefined font stretch : Ultra-condensed (1)</summary>
        UltraCondensed = 1,

        /// <summary>
        /// Predefined font stretch : Extra-condensed (2)</summary>
        ExtraCondensed = 2,

        /// <summary>
        /// Predefined font stretch : Condensed (3)</summary>
        Condensed = 3,

        /// <summary>
        /// Predefined font stretch : Semi-condensed (4)</summary>
        SemiCondensed = 4,

        /// <summary>
        /// Predefined font stretch : Normal (5)</summary>
        Normal = 5,

        /// <summary>
        /// Predefined font stretch : Medium (5)</summary>
        Medium = 5,


        /// <summary>
        /// Predefined font stretch : Semi-expanded (6)</summary>
        SemiExpanded = 6,

        /// <summary>
        /// Predefined font stretch : Expanded (7)</summary>
        Expanded = 7,

        /// <summary>
        /// Predefined font stretch : Extra-expanded (8)</summary>
        ExtraExpanded = 8,

        /// <summary>
        /// Predefined font stretch : Ultra-expanded (9)</summary>
        UltraExpanded = 9,
    }
}
