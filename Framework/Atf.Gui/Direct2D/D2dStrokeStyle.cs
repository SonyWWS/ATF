//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using SharpDX.Direct2D1;

namespace Sce.Atf.Direct2D
{
    /// <summary>
    /// Describes the caps, miter limit, line join, and dash information 
    /// for a stroke</summary>
    public class D2dStrokeStyle : D2dResource
    {
        internal D2dStrokeStyle(StrokeStyle strokeStyle)
        {
            if (strokeStyle == null)
                throw new ArgumentNullException("strokeStyle");

            m_strokeStyle = strokeStyle;
        }

        internal StrokeStyle NativeStrokeStyle
        {
            get { return m_strokeStyle; }
        }
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            m_strokeStyle.Dispose();
            base.Dispose(disposing);
        }

        private StrokeStyle m_strokeStyle;
    }


    /// <summary>
    /// Describes the cap style, i.e., the shape at the end of a line or segment</summary>
    public enum D2dCapStyle
    {
        /// <summary>
        /// A cap that does not extend past the last point of the line.
        /// Comparable to cap used for objects other than lines.</summary>
        Flat = 0,
        
        /// <summary>
        /// Half of a square that has a length equal to the line thickness</summary>
        Square = 1,

        /// <summary>
        /// A semicircle that has a diameter equal to the line thickness</summary>        
        Round = 2,

        /// <summary>
        /// An isosceles right triangle whose hypotenuse is equal in 
        /// length to the thickness of the line</summary>
        Triangle = 3,
    }

    /// <summary>
    /// Describes the sequence of dashes and gaps in a stroke</summary>
    public enum D2dDashStyle
    {
        /// <summary>
        /// A solid line with no breaks</summary>
        Solid = 0,

        /// <summary>
        /// A dash followed by a gap of equal length. The dash and the gap are each twice
        /// as long as the stroke thickness. The equivalent dash array for 
        /// D2D1_DASH_STYLE_DASH is {2, 2}.</summary>
        Dash = 1,
       
        /// <summary>
        /// A dot followed by a longer gap. The equivalent dash array for 
        /// D2D1_DASH_STYLE_DOT is {0, 2}.</summary>
        Dot = 2,
               
        /// <summary>
        /// A dash, followed by a gap, followed by a dot, followed by another gap. The
        /// equivalent dash array for D2D1_DASH_STYLE_DASH_DOT is {2, 2, 0, 2}.</summary>
        DashDot = 3,

        /// <summary>
        /// A dash, followed by a gap, followed by a dot, followed by another gap, followed
        /// by another dot, followed by another gap. The equivalent dash array for D2D1_DASH_STYLE_DASH_DOT_DOT
        /// is {2, 2, 0, 2, 0, 2}.</summary>       
        DashDotDot = 4,

        /// <summary>
        /// The dash pattern is specified by an array of floating-point values</summary>
        Custom = 5,
    }
   
    /// <summary>
    /// Describes the join, i.e., the shape that joins two lines or segments</summary>
    public enum D2dLineJoin
    {
        /// <summary>
        /// Regular angular vertices</summary>
        Miter = 0,
        
        /// <summary>
        /// Beveled vertices</summary>
        Bevel = 1,

        /// <summary>
        /// Rounded vertices</summary>
        Round = 2,
        
        /// <summary>
        /// Regular angular vertices unless the join would extend beyond the miter limit;
        /// otherwise, beveled vertices</summary>
        MiterOrBevel = 3,
    }

    /// <summary>
    /// Describes the stroke that outlines a shape</summary>
    public struct D2dStrokeStyleProperties
    {
        /// <summary>
        /// The shape at either end of each dash segment</summary>
        public D2dCapStyle DashCap;
       
        /// <summary>
        /// A value that specifies an offset in the dash sequence. A positive dash offset
        /// value shifts the dash pattern, in units of stroke width, toward the start
        /// of the stroked geometry. A negative dash offset value shifts the dash pattern,
        /// in units of stroke width, toward the end of the stroked geometry.</summary>
        public float DashOffset;
       
        /// <summary>
        /// A value that specifies whether the stroke has a dash pattern and, if so,
        /// the dash style</summary>
        public D2dDashStyle DashStyle;
        
        /// <summary>
        /// The cap applied to the end of all the open figures in a stroked geometry</summary>
        public D2dCapStyle EndCap;
        
        /// <summary>
        /// A value that describes how segments are joined. This value is ignored for
        /// a vertex if the segment flags specify that the segment should have a smooth join.</summary>
        public D2dLineJoin LineJoin;

        /// <summary>
        /// The limit of the thickness of the join on a mitered corner. This value is
        /// always treated as though it is greater than or equal to 1.0f.</summary>
        public float MiterLimit;

        /// <summary>
        /// The cap applied to the start of all the open figures in a stroked geometry</summary>
        public D2dCapStyle StartCap;
    }
}
