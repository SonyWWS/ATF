//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/******************************************************************/
/*****                                                        *****/
/*****     Project:           Adobe Color Picker Clone 1      *****/
/*****     Filename:          ctrlVerticalColorSlider.cs      *****/
/*****     Original Author:   Danny Blanchard                 *****/
/*****                        - scrabcakes@gmail.com          *****/
/*****     Updates:                                           *****/
/*****      3/28/2005 - Initial Version : Danny Blanchard     *****/
/*****                                                        *****/
/******************************************************************/

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.ColorEditing
{
    /// <summary>
    /// A vertical slider control that shows a range for a color property (a.k.a. Hue, Saturation, Brightness,
    /// Red, Green, Blue) and sends an event when the slider is changed</summary>
    internal class VerticalColorSlider : System.Windows.Forms.UserControl
    {
        #region Class Variables

        /// <summary>
        /// Drawing style of the control</summary>
        public enum eDrawStyle
        {
            Hue,
            Saturation,
            Brightness,
            Red,
            Green,
            Blue
        }


        //    Slider properties
        private int            m_iMarker_Start_Y = 0;
        private bool           m_bDragging = false;

        //    These variables keep track of how to fill in the content inside the box;
        private eDrawStyle        m_eDrawStyle = eDrawStyle.Hue;
        private AdobeColors.HSL   m_hsl;
        private Color             m_rgb;

        private readonly System.ComponentModel.Container components = null;

        #endregion

        #region Constructors / Destructors

        /// <summary>
        /// Constructor</summary>
        public VerticalColorSlider()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            //    Initialize Colors
            m_hsl = new AdobeColors.HSL();
            m_hsl.A = 1.0;
            m_hsl.H = 1.0;
            m_hsl.S = 1.0;
            m_hsl.L = 1.0;
            m_rgb = AdobeColors.HSL_to_RGB(m_hsl);
            m_eDrawStyle = eDrawStyle.Hue;
        }


        /// <summary> 
        /// Cleans up any resources being used</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }


        #endregion

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor</summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VerticalColorSlider));
            this.SuspendLayout();
            // 
            // VerticalColorSlider
            // 
            this.Name = "VerticalColorSlider";
            resources.ApplyResources(this, "$this");
            this.Load += new System.EventHandler(this.ctrl1DColorBar_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ctrl1DColorBar_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ctrl1DColorBar_MouseMove);
            this.Resize += new System.EventHandler(this.ctrl1DColorBar_Resize);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ctrl1DColorBar_Paint);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ctrl1DColorBar_MouseUp);
            this.ResumeLayout(false);

        }
        #endregion

        #region Control Events

        private void ctrl1DColorBar_Load(object sender, System.EventArgs e)
        {
            Redraw_Control();
        }


        private void ctrl1DColorBar_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ( e.Button != MouseButtons.Left )    //    Only respond to left mouse button events
                return;

            m_bDragging = true;        //    Begin dragging which notifies MouseMove function that it needs to update the marker

            int y;
            y = e.Y;
            y -= 4;                                          //    Calculate slider position
            if ( y < 0 ) y = 0;
            if ( y > Height - 9 ) y = Height - 9;

            if ( y == m_iMarker_Start_Y )                    //    If the slider hasn't moved, no need to redraw it.
                return;                                      //    or send a scroll notification

            DrawSlider(y, false);    //    Redraw the slider
            ResetHSLRGB();           //    Reset the color

            if ( ColorChanged != null )    //    Notify anyone who cares that the controls slider(color) has changed
                ColorChanged(this, e);
        }


        private void ctrl1DColorBar_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ( !m_bDragging )        //    Only respond when the mouse is dragging the marker.
                return;

            int y;
            y = e.Y;
            y -= 4;                                         //    Calculate slider position
            if ( y < 0 ) y = 0;
            if ( y > Height - 9 ) y = Height - 9;

            if ( y == m_iMarker_Start_Y )                    //    If the slider hasn't moved, no need to redraw it.
                return;                                      //    or send a scroll notification

            DrawSlider(y, false);    //    Redraw the slider
            ResetHSLRGB();           //    Reset the color

            if ( ColorChanged != null )    //    Notify anyone who cares that the controls slider(color) has changed
                ColorChanged(this, e);
        }


        private void ctrl1DColorBar_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ( e.Button != MouseButtons.Left )    //    Only respond to left mouse button events
                return;

            m_bDragging = false;

            int y;
            y = e.Y;
            y -= 4;                                         //    Calculate slider position
            if ( y < 0 ) y = 0;
            if ( y > Height - 9 ) y = Height - 9;

            if ( y == m_iMarker_Start_Y )                    //    If the slider hasn't moved, no need to redraw it.
                return;                                      //    or send a scroll notification

            DrawSlider(y, false);    //    Redraw the slider
            ResetHSLRGB();           //    Reset the color

            if ( ColorChanged != null )    //    Notify anyone who cares that the controls slider(color) has changed
                ColorChanged(this, e);
        }


        private void ctrl1DColorBar_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            Redraw_Control();
        }


        private void ctrl1DColorBar_Resize(object sender, System.EventArgs e)
        {
            Redraw_Control();
        }


        #endregion

        #region Events

        /// <summary>
        /// Event that is raised after the color changed</summary>
        public event EventHandler ColorChanged;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets or sets the draw style of the control (Hue, Saturation, Brightness, Red, Green or Blue)</summary>
        public eDrawStyle DrawStyle
        {
            get
            {
                return m_eDrawStyle;
            }
            set
            {
                m_eDrawStyle = value;

                //    Redraw the control based on the new eDrawStyle
                Reset_Slider(true);
                Redraw_Control();
            }
        }


        /// <summary>
        /// Gets or sets the HSL color of the control. Changing the HSL automatically changes the RGB color for the control.</summary>
        public AdobeColors.HSL HSL
        {
            get
            {
                return m_hsl;
            }
            set
            {
                m_hsl = value;
                m_rgb = AdobeColors.HSL_to_RGB(m_hsl);

                //    Redraw the control based on the new color.
                Reset_Slider(true);
                DrawContent();
            }
        }


        /// <summary>
        /// Gets or sets the RGB color of the control. Changing the RGB automatically changes the HSL color for the control.</summary>
        public Color RGB
        {
            get
            {
                return m_rgb;
            }
            set
            {
                m_rgb = value;
                m_hsl = AdobeColors.RGB_to_HSL(m_rgb);

                //    Redraw the control based on the new color.
                Reset_Slider(true);
                DrawContent();
            }
        }


        #endregion

        #region Private Methods

        /// <summary>
        /// Redraws the background over the slider area on both sides of the control</summary>
        private void ClearSlider()
        {
            Graphics g = CreateGraphics();
            Brush brush = System.Drawing.SystemBrushes.Control;
            g.FillRectangle(brush, 0, 0, 8, Height);            //    clear left hand slider
            g.FillRectangle(brush, Width - 8, 0, 8, Height);    //    clear right hand slider
        }


        /// <summary>
        /// Draws the slider arrows on both sides of the control</summary>
        /// <param name="position">Position value of the slider, lowest being at the bottom. The range
        /// is between 0 and the control's height-9. The values are adjusted if too large/small.</param>
        /// <param name="Unconditional">If Unconditional is true, the slider is drawn, otherwise some logic 
        /// is performed to determine is drawing is really neccessary</param>
        private void DrawSlider(int position, bool Unconditional)
        {
            if ( position < 0 ) position = 0;
            if ( position > Height - 9 ) position = Height - 9;

            if ( m_iMarker_Start_Y == position && !Unconditional )    //    If the marker position hasn't changed
                return;                                               //    since the last time it was drawn and we don't HAVE to redraw
            //    then exit procedure

            m_iMarker_Start_Y = position;    //    Update the controls marker position

            ClearSlider();        //    Remove old slider

            Graphics g = CreateGraphics();

            Pen pencil = new Pen(Color.FromArgb(116,114,106));    //    Same gray color Photoshop uses
            Brush brush = Brushes.White;
            
            Point[] arrow = new Point[7];                //     GGG
            arrow[0] = new Point(1,position);            //    G   G
            arrow[1] = new Point(3,position);            //    G    G
            arrow[2] = new Point(7,position + 4);        //    G     G
            arrow[3] = new Point(3,position + 8);        //    G      G
            arrow[4] = new Point(1,position + 8);        //    G     G
            arrow[5] = new Point(0,position + 7);        //    G    G
            arrow[6] = new Point(0,position + 1);        //    G   G
            //     GGG

            g.FillPolygon(brush, arrow);    //    Fill left arrow with white
            g.DrawPolygon(pencil, arrow);    //    Draw left arrow border with gray

            //        GGG
            arrow[0] = new Point(Width - 2,position);        //       G   G
            arrow[1] = new Point(Width - 4,position);        //      G    G
            arrow[2] = new Point(Width - 8,position + 4);    //     G     G
            arrow[3] = new Point(Width - 4,position + 8);    //    G      G
            arrow[4] = new Point(Width - 2,position + 8);    //     G     G
            arrow[5] = new Point(Width - 1,position + 7);    //      G    G
            arrow[6] = new Point(Width - 1,position + 1);    //       G   G
            //        GGG

            g.FillPolygon(brush, arrow);    //    Fill right arrow with white
            g.DrawPolygon(pencil, arrow);   //    Draw right arrow border with gray

        }


        /// <summary>
        /// Draws the border around the control, in this case, the border around the content area between
        /// the slider arrows</summary>
        private void DrawBorder()
        {
            Graphics g = CreateGraphics();

            Pen pencil;
            
            //    To make the control look like Adobe Photoshop's the border around the control will be a gray line
            //    on the top and left side, a white line on the bottom and right side, and a black rectangle (line) 
            //    inside the gray/white rectangle

            pencil = new Pen(Color.FromArgb(172,168,153));    //    The same gray color used by Photoshop
            g.DrawLine(pencil, Width - 10, 2, 9, 2);    //    Draw top line
            g.DrawLine(pencil, 9, 2, 9, Height - 4);    //    Draw left hand line

            pencil = new Pen(Color.White);
            g.DrawLine(pencil, Width - 9, 2, Width - 9,Height - 3);    //    Draw right hand line
            g.DrawLine(pencil, Width - 9,Height - 3, 9,Height - 3);    //    Draw bottome line

            pencil = new Pen(Color.Black);
            g.DrawRectangle(pencil, 10, 3, Width - 20, Height - 7);    //    Draw inner black rectangle
        }


        /// <summary>
        /// Evaluates the DrawStyle property of the control and calls the appropriate
        /// drawing function for content</summary>
        private void DrawContent()
        {
            switch (m_eDrawStyle)
            {
                case eDrawStyle.Hue :
                    Draw_Style_Hue();
                    break;
                case eDrawStyle.Saturation :
                    Draw_Style_Saturation();
                    break;
                case eDrawStyle.Brightness :
                    Draw_Style_Luminance();
                    break;
                case eDrawStyle.Red :
                    Draw_Style_Red();
                    break;
                case eDrawStyle.Green :
                    Draw_Style_Green();
                    break;
                case eDrawStyle.Blue :
                    Draw_Style_Blue();
                    break;
            }
        }


        #region Draw_Style_X - Content drawing functions

        //    The following functions do the real work of the control, drawing the primary content (the area between the slider)
        //    

        /// <summary>
        /// Fills in the content of the control showing all values of Hue (from 0 to 360)</summary>
        private void Draw_Style_Hue()
        {
            Graphics g = CreateGraphics();

            AdobeColors.HSL _hsl = new AdobeColors.HSL();
            _hsl.S = 1.0;    //    S and L will both be at 100% for this DrawStyle
            _hsl.L = 1.0;

            for ( int i = 0; i < Height - 8; i++ )    //    i represents the current line of pixels we want to draw horizontally
            {
                _hsl.H = 1.0 - (double)i/(Height - 8);            //    H (hue) is based on the current vertical position
                Pen pen = new Pen(AdobeColors.HSL_to_RGB(_hsl));  //    Get the Color for this line

                g.DrawLine(pen, 11, i + 4, Width - 11, i + 4);    //    Draw the line and loop back for next line
            }
        }


        /// <summary>
        /// Fills in the content of the control showing all values of Saturation (0 to 100%) for the given
        /// Hue and Luminance</summary>
        private void Draw_Style_Saturation()
        {
            Graphics g = CreateGraphics();

            AdobeColors.HSL _hsl = new AdobeColors.HSL();
            _hsl.H = m_hsl.H;    //    Use the H and L values of the current color (m_hsl)
            _hsl.L = m_hsl.L;

            for ( int i = 0; i < Height - 8; i++ ) //    i represents the current line of pixels we want to draw horizontally
            {
                _hsl.S = 1.0 - (double)i/(Height - 8);            //    S (Saturation) is based on the current vertical position
                Pen pen = new Pen(AdobeColors.HSL_to_RGB(_hsl));  //    Get the Color for this line

                g.DrawLine(pen, 11, i + 4, Width - 11, i + 4);    //    Draw the line and loop back for next line
            }
        }


        /// <summary>
        /// Fills in the content of the control showing all values of Luminance (0 to 100%) for the given
        /// Hue and Saturation</summary>
        private void Draw_Style_Luminance()
        {
            Graphics g = CreateGraphics();

            AdobeColors.HSL _hsl = new AdobeColors.HSL();
            _hsl.H = m_hsl.H;    //    Use the H and S values of the current color (m_hsl)
            _hsl.S = m_hsl.S;

            for ( int i = 0; i < Height - 8; i++ ) //    i represents the current line of pixels we want to draw horizontally
            {
                _hsl.L = 1.0 - (double)i/(Height - 8);            //    L (Luminance) is based on the current vertical position
                Pen pen = new Pen(AdobeColors.HSL_to_RGB(_hsl));  //    Get the Color for this line

                g.DrawLine(pen, 11, i + 4, Width - 11, i + 4);    //    Draw the line and loop back for next line
            }
        }


        /// <summary>
        /// Fills in the content of the control showing all values of Red (0 to 255) for the given
        /// Green and Blue</summary>
        private void Draw_Style_Red()
        {
            Graphics g = CreateGraphics();

            for ( int i = 0; i < Height - 8; i++ ) //    i represents the current line of pixels we want to draw horizontally
            {
                int red = 255 - Round(255 * (double)i/(Height - 8));       //    red is based on the current vertical position
                Pen pen = new Pen(Color.FromArgb(red, m_rgb.G, m_rgb.B));  //    Get the Color for this line

                g.DrawLine(pen, 11, i + 4, Width - 11, i + 4);             //    Draw the line and loop back for next line
            }
        }


        /// <summary>
        /// Fills in the content of the control showing all values of Green (0 to 255) for the given
        /// Red and Blue</summary>
        private void Draw_Style_Green()
        {
            Graphics g = CreateGraphics();

            for ( int i = 0; i < Height - 8; i++ ) //    i represents the current line of pixels we want to draw horizontally
            {
                int green = 255 - Round(255 * (double)i/(Height - 8));       //    green is based on the current vertical position
                Pen pen = new Pen(Color.FromArgb(m_rgb.R, green, m_rgb.B));  //    Get the Color for this line

                g.DrawLine(pen, 11, i + 4, Width - 11, i + 4);            //    Draw the line and loop back for next line
            }
        }


        /// <summary>
        /// Fills in the content of the control showing all values of Blue (0 to 255) for the given
        /// Red and Green.</summary>
        private void Draw_Style_Blue()
        {
            Graphics g = CreateGraphics();

            for ( int i = 0; i < Height - 8; i++ ) //    i represents the current line of pixels we want to draw horizontally
            {
                int blue = 255 - Round(255 * (double)i/(Height - 8));       //    green is based on the current vertical position
                Pen pen = new Pen(Color.FromArgb(m_rgb.R, m_rgb.G, blue));  //    Get the Color for this line

                g.DrawLine(pen, 11, i + 4, Width - 11, i + 4);            //    Draw the line and loop back for next line
            }
        }


        #endregion

        /// <summary>
        /// Calls all the functions neccessary to redraw the entire control</summary>
        private void Redraw_Control()
        {
            DrawSlider(m_iMarker_Start_Y, true);
            DrawBorder();
            switch (m_eDrawStyle)
            {
                case eDrawStyle.Hue :
                    Draw_Style_Hue();
                    break;
                case eDrawStyle.Saturation :
                    Draw_Style_Saturation();
                    break;
                case eDrawStyle.Brightness :
                    Draw_Style_Luminance();
                    break;
                case eDrawStyle.Red :
                    Draw_Style_Red();
                    break;
                case eDrawStyle.Green :
                    Draw_Style_Green();
                    break;
                case eDrawStyle.Blue :
                    Draw_Style_Blue();
                    break;
            }
        }


        /// <summary>
        /// Resets the vertical position of the slider to match the controls color. Gives the option of redrawing the slider.</summary>
        /// <param name="Redraw">Set to true if you want the function to redraw the slider after determining the best position</param>
        private void Reset_Slider(bool Redraw)
        {
            //    The position of the marker (slider) changes based on the current drawstyle:
            switch (m_eDrawStyle)
            {
                case eDrawStyle.Hue :
                    m_iMarker_Start_Y = (Height - 8) - Round( (Height - 8) * m_hsl.H );
                    break;
                case eDrawStyle.Saturation :
                    m_iMarker_Start_Y = (Height - 8) - Round( (Height - 8) * m_hsl.S );
                    break;
                case eDrawStyle.Brightness :
                    m_iMarker_Start_Y = (Height - 8) - Round( (Height - 8) * m_hsl.L );
                    break;
                case eDrawStyle.Red :
                    m_iMarker_Start_Y = (Height - 8) - Round( (Height - 8) * (double)m_rgb.R/255 );
                    break;
                case eDrawStyle.Green :
                    m_iMarker_Start_Y = (Height - 8) - Round( (Height - 8) * (double)m_rgb.G/255 );
                    break;
                case eDrawStyle.Blue :
                    m_iMarker_Start_Y = (Height - 8) - Round( (Height - 8) * (double)m_rgb.B/255 );
                    break;
            }

            if ( Redraw )
                DrawSlider(m_iMarker_Start_Y, true);
        }


        /// <summary>
        /// Resets the controls color (both HSL and RGB variables) based on the current slider position</summary>
        private void ResetHSLRGB()
        {
            switch (m_eDrawStyle)
            {
                case eDrawStyle.Hue :
                    m_hsl.H = 1.0 - (double)m_iMarker_Start_Y/(Height - 9);
                    m_rgb = AdobeColors.HSL_to_RGB(m_hsl);
                    break;
                case eDrawStyle.Saturation :
                    m_hsl.S = 1.0 - (double)m_iMarker_Start_Y/(Height - 9);
                    m_rgb = AdobeColors.HSL_to_RGB(m_hsl);
                    break;
                case eDrawStyle.Brightness :
                    m_hsl.L = 1.0 - (double)m_iMarker_Start_Y/(Height - 9);
                    m_rgb = AdobeColors.HSL_to_RGB(m_hsl);
                    break;
                case eDrawStyle.Red :
                    m_rgb = Color.FromArgb(m_rgb.A, 255 - Round( 255 * (double)m_iMarker_Start_Y/(Height - 9) ), m_rgb.G, m_rgb.B);
                    m_hsl = AdobeColors.RGB_to_HSL(m_rgb);
                    break;
                case eDrawStyle.Green :
                    m_rgb = Color.FromArgb(m_rgb.A, m_rgb.R, 255 - Round( 255 * (double)m_iMarker_Start_Y/(Height - 9) ), m_rgb.B);
                    m_hsl = AdobeColors.RGB_to_HSL(m_rgb);
                    break;
                case eDrawStyle.Blue :
                    m_rgb = Color.FromArgb(m_rgb.A, m_rgb.R, m_rgb.G, 255 - Round( 255 * (double)m_iMarker_Start_Y/(Height - 9) ));
                    m_hsl = AdobeColors.RGB_to_HSL(m_rgb);
                    break;
            }
        }


        /// <summary>
        /// Rounds value. Kind of self explanatory, I really need to look up the .NET function that does this.</summary>
        /// <param name="val">Double value to be rounded to an integer</param>
        /// <returns>Rounded value</returns>
        private int Round(double val)
        {
            int ret_val = (int)val;
            
            int temp = (int)(val * 100);

            if ( (temp % 100) >= 50 )
                ret_val += 1;

            return ret_val;
            
        }


        #endregion
    }
}
