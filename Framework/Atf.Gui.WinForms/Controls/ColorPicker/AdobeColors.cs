//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/******************************************************************/
/*****                                                        *****/
/*****     Project:           Adobe Color Picker Clone 1      *****/
/*****     Filename:          AdobeColors.cs                  *****/
/*****     Original Author:   Danny Blanchard                 *****/
/*****                        - scrabcakes@gmail.com          *****/
/*****     Updates:                                           *****/
/*****      3/28/2005 - Initial Version : Danny Blanchard     *****/
/*****                                                        *****/
/******************************************************************/

using System.Drawing;

namespace Sce.Atf.Controls.ColorEditing
{
    /// <summary>
    /// AdobeColors handling class</summary>
    internal class AdobeColors
    {
        #region Constructors / Destructors

        /// <summary>
        /// Constructor</summary>
        public AdobeColors() 
        { 
        } 


        #endregion

        #region Public Methods

        /// <summary> 
        /// Sets the absolute brightness of a color</summary> 
        /// <param name="c">Original color</param> 
        /// <param name="brightness">The luminance level to impose</param> 
        /// <returns>Adjusted color</returns> 
        public static  Color SetBrightness(Color c, double brightness) 
        { 
            HSL hsl = RGB_to_HSL(c); 
            hsl.L=brightness; 
            return HSL_to_RGB(hsl); 
        } 


        /// <summary> 
        /// Modifies an existing brightness level</summary> 
        /// <remarks> 
        /// To reduce brightness, use a number smaller than 1. To increase brightness, use a number larger tnan 1.</remarks> 
        /// <param name="c">The original color</param> 
        /// <param name="brightness">The luminance delta</param> 
        /// <returns>Adjusted color</returns> 
        public static  Color ModifyBrightness(Color c, double brightness) 
        { 
            HSL hsl = RGB_to_HSL(c); 
            hsl.L*=brightness; 
            return HSL_to_RGB(hsl); 
        } 


        /// <summary> 
        /// Sets the absolute saturation level</summary> 
        /// <remarks>Accepted values in range [0..1]</remarks> 
        /// <param name="c">An original color</param> 
        /// <param name="Saturation">The saturation value to impose</param> 
        /// <returns>Adjusted color</returns> 
        public static  Color SetSaturation(Color c, double Saturation) 
        { 
            HSL hsl = RGB_to_HSL(c); 
            hsl.S=Saturation; 
            return HSL_to_RGB(hsl); 
        } 


        /// <summary> 
        /// Modifies an existing saturation level</summary> 
        /// <remarks> 
        /// To reduce saturation, use a number smaller than 1. To increase saturation, use a number larger tnan 1.</remarks> 
        /// <param name="c">The original color</param> 
        /// <param name="Saturation">The saturation delta</param> 
        /// <returns>Adjusted color</returns> 
        public static  Color ModifySaturation(Color c, double Saturation) 
        { 
            HSL hsl = RGB_to_HSL(c); 
            hsl.S*=Saturation; 
            return HSL_to_RGB(hsl); 
        } 


        /// <summary> 
        /// Sets the absolute hue level</summary> 
        /// <remarks>Accepted values in range [0..1]</remarks> 
        /// <param name="c">An original color</param> 
        /// <param name="Hue">The hue value to impose</param> 
        /// <returns>Adjusted color</returns> 
        public static  Color SetHue(Color c, double Hue) 
        { 
            HSL hsl = RGB_to_HSL(c); 
            hsl.H=Hue; 
            return HSL_to_RGB(hsl); 
        } 


        /// <summary> 
        /// Modifies an existing hue level</summary> 
        /// <remarks> 
        /// To reduce hue, use a number smaller than 1. To increase hue, use a number larger tnan 1 
        /// </remarks> 
        /// <param name="c">The original color</param> 
        /// <param name="Hue">The hue delta</param> 
        /// <returns>Adjusted color</returns> 
        public static  Color ModifyHue(Color c, double Hue) 
        { 
            HSL hsl = RGB_to_HSL(c); 
            hsl.H*=Hue; 
            return HSL_to_RGB(hsl); 
        } 


        /// <summary> 
        /// Converts a color from HSL to RGB</summary> 
        /// <remarks>Adapted from the algorithm in Foley and Van-Dam</remarks> 
        /// <param name="hsl">The HSL value</param> 
        /// <returns>A Color structure containing the equivalent RGB values</returns> 
        public static Color HSL_to_RGB(HSL hsl) 
        { 
            int Max, Mid, Min;
            double q;
            int alpha =    Round(255 * hsl.A);

            Max = Round(hsl.L * 255);
            Min = Round((1.0 - hsl.S)*(hsl.L/1.0)*255);
            q   = (double)(Max - Min)/255;

            if ( hsl.H >= 0 && hsl.H <= (double)1/6 )
            {
                Mid = Round(((hsl.H - 0) * q) * 1530 + Min);
                return Color.FromArgb(alpha,Max,Mid,Min);
            }
            else if ( hsl.H <= (double)1/3 )
            {
                Mid = Round(-((hsl.H - (double)1/6) * q) * 1530 + Max);
                return Color.FromArgb(alpha,Mid,Max,Min);
            }
            else if ( hsl.H <= 0.5 )
            {
                Mid = Round(((hsl.H - (double)1/3) * q) * 1530 + Min);
                return Color.FromArgb(alpha,Min,Max,Mid);
            }
            else if ( hsl.H <= (double)2/3 )
            {
                Mid = Round(-((hsl.H - 0.5) * q) * 1530 + Max);
                return Color.FromArgb(alpha,Min,Mid,Max);
            }
            else if ( hsl.H <= (double)5/6 )
            {
                Mid = Round(((hsl.H - (double)2/3) * q) * 1530 + Min);
                return Color.FromArgb(alpha,Mid,Min,Max);
            }
            else if ( hsl.H <= 1.0 )
            {
                Mid = Round(-((hsl.H - (double)5/6) * q) * 1530 + Max);
                return Color.FromArgb(alpha,Max,Min,Mid);
            }
            else    return Color.FromArgb(alpha,0,0,0);
        } 
  

        /// <summary> 
        /// Converts RGB to HSL</summary> 
        /// <remarks>Takes advantage of what is already built in to .NET by using the Color.GetHue, Color.GetSaturation and Color.GetBrightness methods</remarks> 
        /// <param name="c">Color to convert</param> 
        /// <returns>An HSL value</returns> 
        public static HSL RGB_to_HSL (Color c) 
        { 
            HSL hsl =  new HSL(); 
          
            hsl.A = (double)c.A/255;

            int Max, Min, Diff;

            //  Of our RGB values, assign the highest value to Max, and the Smallest to Min
            if ( c.R > c.G )    { Max = c.R; Min = c.G; }
            else                { Max = c.G; Min = c.R; }
            if ( c.B > Max )      Max = c.B;
            else if ( c.B < Min ) Min = c.B;

            Diff = Max - Min;

            //  Luminance - a.k.a. Brightness - Adobe photoshop uses the logic that the
            //  site VBspeed regards (regarded) as too primitive = superior decides the 
            //  level of brightness.
            hsl.L = (double)Max/255;

            //    Saturation
            if ( Max == 0 ) hsl.S = 0;        // Protecting from the impossible operation of division by zero.
            else hsl.S = (double)Diff/Max;    // The logic of Adobe Photoshops is this simple.

            //  Hue   R is situated at the angel of 360 eller noll degrees; 
            //        G vid 120 degrees
            //        B vid 240 degrees
            double q;
            if ( Diff == 0 ) q = 0; // Protecting from the impossible operation of division by zero.
            else q = (double)60/Diff;
            
            if ( Max == c.R )
            {
                if ( c.G < c.B )    hsl.H = (double)(360 + q * (c.G - c.B))/360;
                else                hsl.H = (double)(q * (c.G - c.B))/360;
            }
            else if ( Max == c.G )    hsl.H = (double)(120 + q * (c.B - c.R))/360;
            else if ( Max == c.B )    hsl.H = (double)(240 + q * (c.R - c.G))/360;
            else                      hsl.H = 0.0;

            return hsl; 
        } 


        /// <summary>
        /// Converts RGB to CMYK</summary>
        /// <param name="c">Color to convert</param>
        /// <returns>CMYK object</returns>
        public static CMYK RGB_to_CMYK(Color c)
        {
            CMYK _cmyk = new CMYK();
            double low = 1.0;

            _cmyk.A = (double)c.A/255;

            _cmyk.C = (double)(255 - c.R)/255;
            if ( low > _cmyk.C )
                low = _cmyk.C;

            _cmyk.M = (double)(255 - c.G)/255;
            if ( low > _cmyk.M )
                low = _cmyk.M;

            _cmyk.Y = (double)(255 - c.B)/255;
            if ( low > _cmyk.Y )
                low = _cmyk.Y;

            if ( low > 0.0 )
            {
                _cmyk.K = low;
            }

            return _cmyk;
        }


        /// <summary>
        /// Converts CMYK to RGB</summary>
        /// <param name="_cmyk">Color to convert</param>
        /// <returns>Color object</returns>
        public static Color CMYK_to_RGB(CMYK _cmyk)
        {
            int alpha, red, green, blue;

            alpha =    Round(255 * _cmyk.A);
            red =      Round(255 - (255 * _cmyk.C));
            green =    Round(255 - (255 * _cmyk.M));
            blue =     Round(255 - (255 * _cmyk.Y));

            return Color.FromArgb(alpha, red, green, blue);
        }


        /// <summary>
        /// Custom rounding function</summary>
        /// <param name="val">Value to round</param>
        /// <returns>Rounded value</returns>
        private static int Round(double val)
        {
            int ret_val = (int)val;
            
            int temp = (int)(val * 100);

            if ( (temp % 100) >= 50 )
                ret_val += 1;

            return ret_val;
        }


        #endregion

        #region Public Classes

        public class HSL 
        { 
            #region Class Variables

            /// <summary>
            /// Constructor</summary>
            public HSL() 
            { 
                _a=1.0;
                _h=0; 
                _s=0; 
                _l=0; 
            } 

            double _a;
            double _h; 
            double _s; 
            double _l; 

            #endregion

            #region Public Methods

            /// <summary>
            /// Gets or sets alpha component</summary>
            public double A 
            {
                get { return _a; }
                set
                {
                    _a = value;
                    _a = _a > 1 ? 1 : _a < 0 ? 0 : _a;
                }
            }

            /// <summary>
            /// Gets or sets hue component</summary>
            public double H 
            { 
                get{return _h;} 
                set 
                { 
                    _h=value; 
                    _h=_h>1 ? 1 : _h<0 ? 0 : _h; 
                } 
            }

            /// <summary>
            /// Gets or sets saturation component</summary>
            public double S 
            { 
                get{return _s;} 
                set 
                { 
                    _s=value; 
                    _s=_s>1 ? 1 : _s<0 ? 0 : _s; 
                } 
            }

            /// <summary>
            /// Gets or sets luminance component</summary>
            public double L 
            { 
                get{return _l;} 
                set 
                { 
                    _l=value; 
                    _l=_l>1 ? 1 : _l<0 ? 0 : _l; 
                } 
            } 


            #endregion
        } 


        public class CMYK 
        { 
            #region Class Variables

            /// <summary>
            /// Constructor</summary>
            public CMYK() 
            { 
                _a=1.0; 
                _c=0; 
                _m=0; 
                _y=0; 
                _k=0; 
            } 


            double _a;  // alpha
            double _c; 
            double _m; 
            double _y; 
            double _k;

            #endregion

            #region Public Methods

            /// <summary>
            /// Gets or sets alpha component</summary>
            public double A 
            {
                get { return _a; }
                set
                {
                    _a = value;
                    _a = _a > 1 ? 1 : _a < 0 ? 0 : _a;
                }
            }

            /// <summary>
            /// Gets or sets cyan component</summary>
            public double C 
            { 
                get{return _c;} 
                set 
                { 
                    _c=value; 
                    _c=_c>1 ? 1 : _c<0 ? 0 : _c; 
                } 
            }

            /// <summary>
            /// Gets or sets magenta component</summary>
            public double M 
            { 
                get{return _m;} 
                set 
                { 
                    _m=value; 
                    _m=_m>1 ? 1 : _m<0 ? 0 : _m; 
                } 
            }

            /// <summary>
            /// Gets or sets yellow component</summary>
            public double Y 
            { 
                get{return _y;} 
                set 
                { 
                    _y=value; 
                    _y=_y>1 ? 1 : _y<0 ? 0 : _y; 
                } 
            }

            /// <summary>
            /// Gets or sets black component</summary>
            public double K 
            { 
                get{return _k;} 
                set 
                { 
                    _k=value; 
                    _k=_k>1 ? 1 : _k<0 ? 0 : _k; 
                } 
            } 


            #endregion
        } 


        #endregion
    }
}
