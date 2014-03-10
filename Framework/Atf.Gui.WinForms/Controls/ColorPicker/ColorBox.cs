//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/******************************************************************/
/*****                                                        *****/
/*****     Project:           Adobe Color Picker Clone 1      *****/
/*****     Filename:          ctrl2DColorBox.cs               *****/
/*****     Original Author:   Danny Blanchard                 *****/
/*****                        - scrabcakes@gmail.com          *****/
/*****     Updates:                                           *****/
/*****      3/28/2005 - Initial Version : Danny Blanchard     *****/
/*****                                                        *****/
/******************************************************************/

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sce.Atf.Controls.ColorEditing
{
    /// <summary>
    /// Color box displaying colors for a color picker</summary>
    internal class ColorBox : System.Windows.Forms.UserControl
    {
        #region Class Variables

        /// <summary>
        /// Color picker drawing style</summary>
        public enum eDrawStyle
        {
            Hue,
            Saturation,
            Brightness,
            Red,
            Green,
            Blue
        }

        private int        m_iMarker_X = 0;
        private int        m_iMarker_Y = 0;
        private bool       m_bDragging = false;

        //    These variables keep track of how to fill in the content inside the box;
        private eDrawStyle        m_eDrawStyle = eDrawStyle.Hue;
        private AdobeColors.HSL   m_hsl;
        private Color             m_rgb;

        /// <summary> 
        /// Required designer variable</summary>
        private readonly System.ComponentModel.Container components = null;

        #endregion

        #region Constructors / Destructors

        /// <summary>
        /// Constructor</summary>
        public ColorBox()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            //    Initialize Colors
            m_hsl = new AdobeColors.HSL();
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
        protected override void Dispose( bool disposing )
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ColorBox));
            this.SuspendLayout();
            // 
            // ColorBox
            // 
            this.Name = "ColorBox";
            resources.ApplyResources(this, "$this");
            this.Load += new System.EventHandler(this.ctrl2DColorBox_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ctrl2DColorBox_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ctrl2DColorBox_MouseMove);
            this.Resize += new System.EventHandler(this.ctrl2DColorBox_Resize);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ctrl2DColorBox_Paint);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ctrl2DColorBox_MouseUp);
            this.ResumeLayout(false);

        }
        #endregion

        #region Control Events

        private void ctrl2DColorBox_Load(object sender, System.EventArgs e)
        {
            Redraw_Control();
        }


        private void ctrl2DColorBox_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ( e.Button != MouseButtons.Left )    //    Only respond to left mouse button events
                return;

            m_bDragging = true;        //    Begin dragging which notifies MouseMove function that it needs to update the marker

            int x = e.X - 2, y = e.Y - 2;
            if ( x < 0 ) x = 0;
            if ( x > Width - 4 ) x = Width - 4;    //    Calculate marker position
            if ( y < 0 ) y = 0;
            if ( y > Height - 4 ) y = Height - 4;

            if ( x == m_iMarker_X && y == m_iMarker_Y )        //    If the marker hasn't moved, no need to redraw it.
                return;                                        //    or send a scroll notification

            DrawMarker(x, y, true);   //    Redraw the marker
            ResetHSLRGB();            //    Reset the color

            if ( ColorChanged != null )    //    Notify anyone who cares that the controls marker (selected color) has changed
                ColorChanged(this, e);
        }


        private void ctrl2DColorBox_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ( !m_bDragging )        //    Only respond when the mouse is dragging the marker.
                return;

            int x = e.X - 2, y = e.Y - 2;
            if ( x < 0 ) x = 0;
            if ( x > Width - 4 ) x = Width - 4;    //    Calculate marker position
            if ( y < 0 ) y = 0;
            if ( y > Height - 4 ) y = Height - 4;

            if ( x == m_iMarker_X && y == m_iMarker_Y )        //    If the marker hasn't moved, no need to redraw it.
                return;                                        //    or send a scroll notification

            DrawMarker(x, y, true);    //    Redraw the marker
            ResetHSLRGB();             //    Reset the color

            if ( ColorChanged != null )    //    Notify anyone who cares that the controls marker (selected color) has changed
                ColorChanged(this, e);
        }


        private void ctrl2DColorBox_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ( e.Button != MouseButtons.Left )    //    Only respond to left mouse button events
                return;

            if ( !m_bDragging )
                return;

            m_bDragging = false;

            int x = e.X - 2, y = e.Y - 2;
            if ( x < 0 ) x = 0;
            if ( x > Width - 4 ) x = Width - 4;    //    Calculate marker position
            if ( y < 0 ) y = 0;
            if ( y > Height - 4 ) y = Height - 4;

            if ( x == m_iMarker_X && y == m_iMarker_Y )        //    If the marker hasn't moved, no need to redraw it.
                return;                                        //    or send a scroll notification

            DrawMarker(x, y, true);    //    Redraw the marker
            ResetHSLRGB();             //    Reset the color

            if ( ColorChanged != null )    //    Notify anyone who cares that the controls marker (selected color) has changed
                ColorChanged(this, e);
        }


        private void ctrl2DColorBox_Resize(object sender, System.EventArgs e)
        {
            Redraw_Control();
        }


        private void ctrl2DColorBox_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
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
        /// Gets or sets the drawing style of the contol (Hue, Saturation, Brightness, Red, Green or Blue)</summary>
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
                Reset_Marker(true);
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
                Reset_Marker(true);
                Redraw_Control();
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
                Reset_Marker(true);
                Redraw_Control();
            }
        }


        #endregion

        #region Private Methods

        /// <summary>
        /// Redraws only the content over the marker</summary>
        private void ClearMarker()
        {
            Graphics g = CreateGraphics();
            
            //    Determine the area that needs to be redrawn
            int start_x, start_y, end_x, end_y;
            int red = 0; int green = 0; int blue = 0;
            AdobeColors.HSL hsl_start = new AdobeColors.HSL();
            AdobeColors.HSL hsl_end = new AdobeColors.HSL();

            //    Find the markers corners
            start_x = m_iMarker_X - 5;
            start_y = m_iMarker_Y - 5;
            end_x = m_iMarker_X + 5;
            end_y = m_iMarker_Y + 5;
            //    Adjust the area if part of it hangs outside the content area
            if ( start_x < 0 ) start_x = 0;
            if ( start_y < 0 ) start_y = 0;
            if ( end_x > Width - 4 ) end_x = Width - 4;
            if ( end_y > Height - 4 ) end_y = Height - 4;

            //    Redraw the content based on the current draw style:
            //    The code get's a little messy from here
            switch (m_eDrawStyle)
            {
                    //          S=0,S=1,S=2,S=3.....S=100
                    //    L=100
                    //    L=99
                    //    L=98        Drawstyle
                    //    L=97           Hue
                    //    ...
                    //    L=0
                case eDrawStyle.Hue :    

                    hsl_start.H = m_hsl.H;    hsl_end.H = m_hsl.H;    //    Hue is constant
                    hsl_start.S = (double)start_x/(Width - 4);        //    Because we're drawing horizontal lines, s will not change
                    hsl_end.S = (double)end_x/(Width - 4);            //    from line to line

                    for ( int i = start_y; i <= end_y; i++ )          //    For each horizontal line:
                    {
                        hsl_start.L = 1.0 - (double)i/(Height - 4);   //    Brightness (L) WILL change for each horizontal
                        hsl_end.L = hsl_start.L;                      //    line drawn
                
                        LinearGradientBrush br = new LinearGradientBrush(new Rectangle(start_x + 1,i + 2, end_x - start_x + 1, 1), AdobeColors.HSL_to_RGB(hsl_start), AdobeColors.HSL_to_RGB(hsl_end), 0, false); 
                        g.FillRectangle(br,new Rectangle(start_x + 2,i + 2, end_x - start_x + 1 , 1)); 
                    }
                    
                    break;
                    //          H=0,H=1,H=2,H=3.....H=360
                    //    L=100
                    //    L=99
                    //    L=98        Drawstyle
                    //    L=97        Saturation
                    //    ...
                    //    L=0
                case eDrawStyle.Saturation :

                    hsl_start.S = m_hsl.S;    hsl_end.S = m_hsl.S;       //    Saturation is constant
                    hsl_start.L = 1.0 - (double)start_y/(Height - 4);    //    Because we're drawing vertical lines, L will 
                    hsl_end.L = 1.0 - (double)end_y/(Height - 4);        //    not change from line to line

                    for ( int i = start_x; i <= end_x; i++ )             //    For each vertical line:
                    {
                        hsl_start.H = (double)i/(Width - 4);             //    Hue (H) WILL change for each vertical
                        hsl_end.H = hsl_start.H;                         //    line drawn
                
                        LinearGradientBrush br = new LinearGradientBrush(new Rectangle(i + 2,start_y + 1, 1, end_y - start_y + 2), AdobeColors.HSL_to_RGB(hsl_start), AdobeColors.HSL_to_RGB(hsl_end), 90, false); 
                        g.FillRectangle(br,new Rectangle(i + 2, start_y + 2, 1, end_y - start_y + 1)); 
                    }
                    break;
                    //          H=0,H=1,H=2,H=3.....H=360
                    //    S=100
                    //    S=99
                    //    S=98        Drawstyle
                    //    S=97        Brightness
                    //    ...
                    //    S=0
                case eDrawStyle.Brightness :
                    
                    hsl_start.L = m_hsl.L;    hsl_end.L = m_hsl.L;       //    Luminance is constant
                    hsl_start.S = 1.0 - (double)start_y/(Height - 4);    //    Because we're drawing vertical lines, S will 
                    hsl_end.S = 1.0 - (double)end_y/(Height - 4);        //    not change from line to line

                    for ( int i = start_x; i <= end_x; i++ )             //    For each vertical line:
                    {
                        hsl_start.H = (double)i/(Width - 4);             //    Hue (H) WILL change for each vertical
                        hsl_end.H = hsl_start.H;                         //    line drawn
                
                        LinearGradientBrush br = new LinearGradientBrush(new Rectangle(i + 2,start_y + 1, 1, end_y - start_y + 2), AdobeColors.HSL_to_RGB(hsl_start), AdobeColors.HSL_to_RGB(hsl_end), 90, false); 
                        g.FillRectangle(br,new Rectangle(i + 2, start_y + 2, 1, end_y - start_y + 1)); 
                    }

                    break;
                    //          B=0,B=1,B=2,B=3.....B=100
                    //    G=100
                    //    G=99
                    //    G=98        Drawstyle
                    //    G=97           Red
                    //    ...
                    //    G=0
                case eDrawStyle.Red :
                    
                    red = m_rgb.R;                                             //    Red is constant
                    int start_b = Round(255 * (double)start_x/(Width - 4));    //    Because we're drawing horizontal lines, B
                    int end_b = Round(255 * (double)end_x/(Width - 4));        //    will not change from line to line

                    for ( int i = start_y; i <= end_y; i++ )                   //    For each horizontal line:
                    {
                        green = Round(255 - (255 * (double)i/(Height - 4)));   //    green WILL change for each horizontal line drawn

                        LinearGradientBrush br = new LinearGradientBrush(new Rectangle(start_x + 1,i + 2, end_x - start_x + 1, 1), Color.FromArgb(red, green, start_b), Color.FromArgb(red, green, end_b), 0, false); 
                        g.FillRectangle(br,new Rectangle(start_x + 2,i + 2, end_x - start_x + 1 , 1));  
                    }

                    break;
                    //          B=0,B=1,B=2,B=3.....B=100
                    //    R=100
                    //    R=99
                    //    R=98        Drawstyle
                    //    R=97          Green
                    //    ...
                    //    R=0
                case eDrawStyle.Green :
                    
                    green = m_rgb.G;;                                           //    Green is constant
                    int start_b2 = Round(255 * (double)start_x/(Width - 4));    //    Because we're drawing horizontal lines, B
                    int end_b2 = Round(255 * (double)end_x/(Width - 4));        //    will not change from line to line

                    for ( int i = start_y; i <= end_y; i++ )                    //    For each horizontal line:
                    {
                        red = Round(255 - (255 * (double)i/(Height - 4)));      //    red WILL change for each horizontal line drawn

                        LinearGradientBrush br = new LinearGradientBrush(new Rectangle(start_x + 1,i + 2, end_x - start_x + 1, 1), Color.FromArgb(red, green, start_b2), Color.FromArgb(red, green, end_b2), 0, false); 
                        g.FillRectangle(br,new Rectangle(start_x + 2,i + 2, end_x - start_x + 1 , 1)); 
                    }

                    break;
                    //          R=0,R=1,R=2,R=3.....R=100
                    //    G=100
                    //    G=99
                    //    G=98        Drawstyle
                    //    G=97           Blue
                    //    ...
                    //    G=0
                case eDrawStyle.Blue :
                    
                    blue = m_rgb.B;;                                           //    Blue is constant
                    int start_r = Round(255 * (double)start_x/(Width - 4));    //    Because we're drawing horizontal lines, R
                    int end_r = Round(255 * (double)end_x/(Width - 4));        //    will not change from line to line

                    for ( int i = start_y; i <= end_y; i++ )                   //    For each horizontal line:
                    {
                        green = Round(255 - (255 * (double)i/(Height - 4)));   //    green WILL change for each horizontal line drawn

                        LinearGradientBrush br = new LinearGradientBrush(new Rectangle(start_x + 1,i + 2, end_x - start_x + 1, 1), Color.FromArgb(start_r, green, blue), Color.FromArgb(end_r, green, blue), 0, false); 
                        g.FillRectangle(br,new Rectangle(start_x + 2,i + 2, end_x - start_x + 1 , 1)); 
                    }

                    break;
            }
        }


        /// <summary>
        /// Draws the marker (circle) inside the color box</summary>
        /// <param name="x">X-coordinate</param>
        /// <param name="y">Y-coordinate</param>
        /// <param name="Unconditional">Whether to draw the marker, even if its position has not changed</param>
        private void DrawMarker(int x, int y, bool Unconditional)            //       *****
        {                                                                    //      *  |  *
            if ( x < 0 ) x = 0;                                              //     *   |   *
            if ( x > Width - 4 ) x = Width - 4;                              //    *    |    *
            if ( y < 0 ) y = 0;                                              //    *    |    *
            if ( y > Height - 4 ) y = Height - 4;                            //    *----X----*
                                                                             //    *    |    *
            if ( m_iMarker_Y == y && m_iMarker_X == x && !Unconditional )    //    *    |    *
                return;                                                      //     *   |   *
                                                                             //      *  |  *
            ClearMarker();                                                   //       *****

            m_iMarker_X = x;
            m_iMarker_Y = y;

            Graphics g = CreateGraphics();

            Pen pen;
            AdobeColors.HSL _hsl = GetColor(x,y);    //    The selected color determines the color of the marker drawn over
            //    it (black or white)
            if ( _hsl.L < (double)200/255 )
                pen = new Pen(Color.White);                                    //    White marker if selected color is dark
            else if ( _hsl.H < (double)26/360 || _hsl.H > (double)200/360 )
                if ( _hsl.S > (double)70/255 )
                    pen = new Pen(Color.White);
                else
                    pen = new Pen(Color.Black);                                //    Else use a black marker for lighter colors
            else
                pen = new Pen(Color.Black);

            g.DrawEllipse(pen, x - 3, y - 3, 10, 10);                        //    Draw the marker : 11 x 11 circle

            DrawBorder();        //    Force the border to be redrawn, just in case the marker has been drawn over it.
        }


        /// <summary>
        /// Draws the border around the control</summary>
        private void DrawBorder()
        {
            Graphics g = CreateGraphics();

            Pen pencil;
            
            //    To make the control look like Adobe Photoshop's the border around the control will be a gray line
            //    on the top and left side, a white line on the bottom and right side, and a black rectangle (line) 
            //    inside the gray/white rectangle

            pencil = new Pen(Color.FromArgb(172,168,153));    //    The same gray color used by Photoshop
            g.DrawLine(pencil, Width - 2, 0, 0, 0);     //    Draw top line
            g.DrawLine(pencil, 0, 0, 0, Height - 2);    //    Draw left hand line

            pencil = new Pen(Color.White);
            g.DrawLine(pencil, Width - 1, 0, Width - 1,Height - 1);    //    Draw right hand line
            g.DrawLine(pencil, Width - 1,Height - 1, 0,Height - 1);    //    Draw bottome line

            pencil = new Pen(Color.Black);
            g.DrawRectangle(pencil, 1, 1, Width - 3, Height - 3);    //    Draw inner black rectangle
        }


        /// <summary>
        /// Evaluates the drawing style of the control and calls the appropriate
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


        /// <summary>
        /// Draws the content of the control, filling in all color values with the provided hue value</summary>
        private void Draw_Style_Hue()
        {
            Graphics g = CreateGraphics();

            AdobeColors.HSL hsl_start = new AdobeColors.HSL();
            AdobeColors.HSL hsl_end = new AdobeColors.HSL();
            hsl_start.H = m_hsl.H;
            hsl_end.H = m_hsl.H;
            hsl_start.S = 0.0;
            hsl_end.S = 1.0;

            for ( int i = 0; i < Height - 4; i++ )                //    For each horizontal line in the control:
            {
                hsl_start.L = 1.0 - (double)i/(Height - 4);    //    Calculate luminance at this line (Hue and Saturation are constant)
                hsl_end.L = hsl_start.L;
                
                LinearGradientBrush br = new LinearGradientBrush(new Rectangle(2,2, Width - 4, 1), AdobeColors.HSL_to_RGB(hsl_start), AdobeColors.HSL_to_RGB(hsl_end), 0, false); 
                g.FillRectangle(br,new Rectangle(2,i + 2, Width - 4, 1)); 
            }
        }


        /// <summary>
        /// Draws the content of the control, filling in all color values with the provided saturation value</summary>
        private void Draw_Style_Saturation()
        {
            Graphics g = CreateGraphics();

            AdobeColors.HSL hsl_start = new AdobeColors.HSL();
            AdobeColors.HSL hsl_end = new AdobeColors.HSL();
            hsl_start.S = m_hsl.S;
            hsl_end.S = m_hsl.S;
            hsl_start.L = 1.0;
            hsl_end.L = 0.0;

            for ( int i = 0; i < Width - 4; i++ )        //    For each vertical line in the control:
            {
                hsl_start.H = (double)i/(Width - 4);    //    Calculate Hue at this line (Saturation and Luminance are constant)
                hsl_end.H = hsl_start.H;
                
                LinearGradientBrush br = new LinearGradientBrush(new Rectangle(2,2, 1, Height - 4), AdobeColors.HSL_to_RGB(hsl_start), AdobeColors.HSL_to_RGB(hsl_end), 90, false); 
                g.FillRectangle(br,new Rectangle(i + 2, 2, 1, Height - 4)); 
            }
        }


        /// <summary>
        /// Draws the content of the control, filling in all color values with the provided luminance or brightness value</summary>
        private void Draw_Style_Luminance()
        {
            Graphics g = CreateGraphics();

            AdobeColors.HSL hsl_start = new AdobeColors.HSL();
            AdobeColors.HSL hsl_end = new AdobeColors.HSL();
            hsl_start.L = m_hsl.L;
            hsl_end.L = m_hsl.L;
            hsl_start.S = 1.0;
            hsl_end.S = 0.0;

            for ( int i = 0; i < Width - 4; i++ )        //    For each vertical line in the control:
            {
                hsl_start.H = (double)i/(Width - 4);    //    Calculate Hue at this line (Saturation and Luminance are constant)
                hsl_end.H = hsl_start.H;
                
                LinearGradientBrush br = new LinearGradientBrush(new Rectangle(2,2, 1, Height - 4), AdobeColors.HSL_to_RGB(hsl_start), AdobeColors.HSL_to_RGB(hsl_end), 90, false); 
                g.FillRectangle(br,new Rectangle(i + 2, 2, 1, Height - 4)); 
            }
        }


        /// <summary>
        /// Draws the content of the control, filling in all color values with the provided red value</summary>
        private void Draw_Style_Red()
        {
            Graphics g = CreateGraphics();

            int red = m_rgb.R;;

            for ( int i = 0; i < Height - 4; i++ )                //    For each horizontal line in the control:
            {
                //    Calculate Green at this line (Red and Blue are constant)
                int green = Round(255 - (255 * (double)i/(Height - 4)));

                LinearGradientBrush br = new LinearGradientBrush(new Rectangle(2,2, Width - 4, 1), Color.FromArgb(red, green, 0), Color.FromArgb(red, green, 255), 0, false); 
                g.FillRectangle(br,new Rectangle(2,i + 2, Width - 4, 1)); 
            }
        }


        /// <summary>
        /// Draws the content of the control, filling in all color values with the provided green value</summary>
        private void Draw_Style_Green()
        {
            Graphics g = CreateGraphics();

            int green = m_rgb.G;;

            for ( int i = 0; i < Height - 4; i++ )    //    For each horizontal line in the control:
            {
                //    Calculate Red at this line (Green and Blue are constant)
                int red = Round(255 - (255 * (double)i/(Height - 4)));

                LinearGradientBrush br = new LinearGradientBrush(new Rectangle(2,2, Width - 4, 1), Color.FromArgb(red, green, 0), Color.FromArgb(red, green, 255), 0, false); 
                g.FillRectangle(br,new Rectangle(2,i + 2, Width - 4, 1)); 
            }
        }


        /// <summary>
        /// Draws the content of the control, filling in all color values with the provided blue value</summary>
        private void Draw_Style_Blue()
        {
            Graphics g = CreateGraphics();

            int blue = m_rgb.B;;

            for ( int i = 0; i < Height - 4; i++ )    //    For each horizontal line in the control:
            {
                //    Calculate Green at this line (Red and Blue are constant)
                int green = Round(255 - (255 * (double)i/(Height - 4)));

                LinearGradientBrush br = new LinearGradientBrush(new Rectangle(2,2, Width - 4, 1), Color.FromArgb(0, green, blue), Color.FromArgb(255, green, blue), 0, false); 
                g.FillRectangle(br,new Rectangle(2,i + 2, Width - 4, 1)); 
            }
        }


        /// <summary>
        /// Redraws the entire control, calling all the functions neccessary</summary>
        private void Redraw_Control()
        {
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

            DrawMarker(m_iMarker_X, m_iMarker_Y, true);
        }


        /// <summary>
        /// Resets the marker position of the slider to match the controls color. Gives the option of redrawing the slider.</summary>
        /// <param name="Redraw">Set to true if you want the function to redraw the slider after determining the best position</param>
        private void Reset_Marker(bool Redraw)
        {
            switch (m_eDrawStyle)
            {
                case eDrawStyle.Hue :
                    m_iMarker_X = Round((Width - 4) * m_hsl.S);
                    m_iMarker_Y = Round((Height - 4) * (1.0 - m_hsl.L));
                    break;
                case eDrawStyle.Saturation :
                    m_iMarker_X = Round((Width - 4) * m_hsl.H);
                    m_iMarker_Y = Round((Height - 4) * (1.0 - m_hsl.L));
                    break;
                case eDrawStyle.Brightness :
                    m_iMarker_X = Round((Width - 4) * m_hsl.H);
                    m_iMarker_Y = Round((Height - 4) * (1.0 - m_hsl.S));
                    break;
                case eDrawStyle.Red :
                    m_iMarker_X = Round((Width - 4) * (double)m_rgb.B/255);
                    m_iMarker_Y = Round((Height - 4) * (1.0 - (double)m_rgb.G/255));
                    break;
                case eDrawStyle.Green :
                    m_iMarker_X = Round((Width - 4) * (double)m_rgb.B/255);
                    m_iMarker_Y = Round((Height - 4) * (1.0 - (double)m_rgb.R/255));
                    break;
                case eDrawStyle.Blue :
                    m_iMarker_X = Round((Width - 4) * (double)m_rgb.R/255);
                    m_iMarker_Y = Round((Height - 4) * (1.0 - (double)m_rgb.G/255));
                    break;
            }

            if ( Redraw )
                DrawMarker(m_iMarker_X, m_iMarker_Y, true);
        }


        /// <summary>
        /// Resets the controls color (both HSL and RGB variables) based on the current marker position</summary>
        private void ResetHSLRGB()
        {
            int red, green, blue;

            switch (m_eDrawStyle)
            {
                case eDrawStyle.Hue :
                    m_hsl.S = (double)m_iMarker_X/(Width - 4);
                    m_hsl.L = 1.0 - (double)m_iMarker_Y/(Height - 4);
                    m_rgb = AdobeColors.HSL_to_RGB(m_hsl);
                    break;
                case eDrawStyle.Saturation :
                    m_hsl.H = (double)m_iMarker_X/(Width - 4);
                    m_hsl.L = 1.0 - (double)m_iMarker_Y/(Height - 4);
                    m_rgb = AdobeColors.HSL_to_RGB(m_hsl);
                    break;
                case eDrawStyle.Brightness :
                    m_hsl.H = (double)m_iMarker_X/(Width - 4);
                    m_hsl.S = 1.0 - (double)m_iMarker_Y/(Height - 4);
                    m_rgb = AdobeColors.HSL_to_RGB(m_hsl);
                    break;
                case eDrawStyle.Red :
                    blue = Round(255 * (double)m_iMarker_X/(Width - 4));
                    green = Round(255 * (1.0 - (double)m_iMarker_Y/(Height - 4)));
                    m_rgb = Color.FromArgb(m_rgb.A, m_rgb.R, green, blue);
                    m_hsl = AdobeColors.RGB_to_HSL(m_rgb);
                    break;
                case eDrawStyle.Green :
                    blue = Round(255 * (double)m_iMarker_X/(Width - 4));
                    red = Round(255 * (1.0 - (double)m_iMarker_Y/(Height - 4)));
                    m_rgb = Color.FromArgb(m_rgb.A, red, m_rgb.G, blue);
                    m_hsl = AdobeColors.RGB_to_HSL(m_rgb);
                    break;
                case eDrawStyle.Blue :
                    red = Round(255 * (double)m_iMarker_X/(Width - 4));
                    green = Round(255 * (1.0 - (double)m_iMarker_Y/(Height - 4)));
                    m_rgb = Color.FromArgb(m_rgb.A, red, green, m_rgb.B);
                    m_hsl = AdobeColors.RGB_to_HSL(m_rgb);
                    break;
            }
        }


        /// <summary>
        /// Rounds a double</summary>
        /// <param name="val">double value to be rounded to an integer</param>
        /// <returns>Rounded double</returns>
        /// <remarks>Kind of self explanitory. I really need to look up the .NET function that does this.</remarks>
        private int Round(double val)
        {
            int ret_val = (int)val;
            
            int temp = (int)(val * 100);

            if ( (temp % 100) >= 50 )
                ret_val += 1;

            return ret_val;
            
        }


        /// <summary>
        /// Returns the graphed color at the x, y position on the control</summary>
        /// <param name="x">X-coordinate</param>
        /// <param name="y">Y-coordinate</param>
        /// <returns>Color at position</returns>
        private AdobeColors.HSL GetColor(int x, int y)
        {

            AdobeColors.HSL _hsl = new AdobeColors.HSL();

            switch (m_eDrawStyle)
            {
                case eDrawStyle.Hue :
                    _hsl.H = m_hsl.H;
                    _hsl.S = (double)x/(Width - 4);
                    _hsl.L = 1.0 - (double)y/(Height - 4);
                    break;
                case eDrawStyle.Saturation :
                    _hsl.S = m_hsl.S;
                    _hsl.H = (double)x/(Width - 4);
                    _hsl.L = 1.0 - (double)y/(Height - 4);
                    break;
                case eDrawStyle.Brightness :
                    _hsl.L = m_hsl.L;
                    _hsl.H = (double)x/(Width - 4);
                    _hsl.S = 1.0 - (double)y/(Height - 4);
                    break;
                case eDrawStyle.Red :
                    _hsl = AdobeColors.RGB_to_HSL(Color.FromArgb(m_rgb.R, Round(255 * (1.0 - (double)y/(Height - 4))), Round(255 * (double)x/(Width - 4))));
                    break;
                case eDrawStyle.Green :
                    _hsl = AdobeColors.RGB_to_HSL(Color.FromArgb(Round(255 * (1.0 - (double)y/(Height - 4))), m_rgb.G, Round(255 * (double)x/(Width - 4))));
                    break;
                case eDrawStyle.Blue :
                    _hsl = AdobeColors.RGB_to_HSL(Color.FromArgb(Round(255 * (double)x/(Width - 4)), Round(255 * (1.0 - (double)y/(Height - 4))), m_rgb.B));
                    break;
            }

            return _hsl;
        }


        #endregion
    }
}
