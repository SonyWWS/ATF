using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using Sce.Atf.Direct2D;
using Sce.Atf.VectorMath;


namespace UsingDirect2D
{
    /// <summary>
    /// Main form</summary>
    public class Form1 : Form
    {
        private Canvas2d m_canvas;
        /// <summary>
        /// Constructor</summary>
        public Form1()
        {
            bool useD2d = true;

            if (useD2d)
            {
                D2dFactory.EnableResourceSharing(this.Handle);
                m_canvas = new Canvas2d();
                m_canvas.Dock = DockStyle.Fill;
                Controls.Add(m_canvas);
            }
            else
            {
                GdiCanvas can = new GdiCanvas();
                can.Dock = DockStyle.Fill;
                Controls.Add(can);
            }

            this.ClientSize = new Size(1280, 900);


            //// create test forms
            //for (int i = 0; i < 4; i++)
            //{
            //    Form f = new TestRT();
            //    f.ClientSize = new System.Drawing.Size(400, 300);
            //    f.Owner = this;
            //    f.Show(this);
            //}
        }
    }
//#region Test Render target form

    public class TestRT : Form
    {
        public TestRT()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint
               | ControlStyles.Opaque | ControlStyles.UserPaint, true);
            this.ResizeRedraw = true;

            m_brush = D2dFactory.CreateSolidBrush(Color.SkyBlue);
            m_graphics = D2dFactory.CreateD2dHwndGraphics(this.Handle);


            if (s_lnBrush == null)
            {
                Color stateColor1 = System.Drawing.Color.Red;
                Color stateColor2 = Color.Violet;
                D2dGradientStop[] gradstops = 
                    { 
                        new D2dGradientStop(stateColor1, 0),
                        new D2dGradientStop(stateColor2, 1.0f),
                        new D2dGradientStop(Color.WhiteSmoke, 1),

                    };
                s_lnBrush = D2dFactory.CreateLinearGradientBrush(gradstops);
            }

            if (s_bmp == null)
            {
                string level = Application.StartupPath + "\\Resources\\Level1.png";
                s_bmp = D2dFactory.CreateBitmap(level);
            }

        }


        /// <summary>
        /// Handles the Paint event and performs custom processing</summary>
        /// <param name="e">PaintEventArgs containing event data</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            Render();
        }
        /// <summary>
        /// Raises the SizeChanged event and performs custom processing</summary>
        /// <param name="e">EventArgs containing event data</param>
        protected override void OnSizeChanged(EventArgs e)
        {
            m_graphics.Resize(ClientSize);

            base.OnSizeChanged(e);

        }

        private void Render()
        {
            m_graphics.BeginDraw();
            m_graphics.Clear(Color.Gray);
            m_graphics.FillRectangle(new RectangleF(1, 1, 80, 40), s_lnBrush);
            m_graphics.DrawLine(10, 10, 100, 100, m_brush, 10);
            m_graphics.DrawBitmap(s_bmp, new RectangleF(2, 50, 60, 60), 1.0f, D2dBitmapInterpolationMode.Linear);
            m_graphics.EndDraw();

        }

        private void CreateRes()
        {

        }


        /// <summary>
        /// Disposes of resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_brush.Dispose();
                m_graphics.Dispose();
            }
            base.Dispose(disposing);
        }

        private D2dHwndGraphics m_graphics;
        private D2dSolidColorBrush m_brush;
        public static D2dLinearGradientBrush s_lnBrush;
        public static D2dBitmap s_bmp;
    }

    /// <summary>
    /// Canvas for displaying graphics</summary>
    public class Canvas2d : Control
    {
        /// <summary>
        /// Constructor</summary>
        public Canvas2d()
        {

            this.SetStyle(ControlStyles.AllPaintingInWmPaint
                | ControlStyles.Opaque | ControlStyles.UserPaint, true);
            this.ResizeRedraw = true;


            // - Allocate all the resources you need in advance not per call (res: brushes, Stroke, bitmaps, etc).
            // - Units dip vs pixels. (use pixel just like GDI+).         
            // - Bitmaps (try to create few large bitmaps instead of lots of small ones.
            //    use texture atlas (http://en.wikipedia.org/wiki/Texture_atlas).
            // - turn off Anti-Aliase unless needed.
            //   Relative rendering speeds
            // - Drawing outline of shapes are more expensive than filling them.
            // - Drawing rounded rectangles are 24 times more expensive than drawing regular rectangles.
            // - Filling rounded rectangle 3 times more expesive than filling regular rectangle.
            // - drawing lines width = 1.0f  is 20 times more expensive that drawing line width =[2 to 10];
            //   when Anti-aliase is ON.  
            //    try to draw line with width 2.

            // D2dGraphics is equivalent to System.Drawing.Graphics
            // D2dGraphics is hardware resource.
            // create D2dGraphics once and keep it for the life time of the control.
            // should not create and dispose D2dGraphics within a method call.
            //
            // D2dHwndGraphics need to be resized on size changed. look at on sizechanged.            
            m_d2dGraphics = D2dFactory.CreateD2dHwndGraphics(this.Handle);
            // for d2d graphics you do not need doublebuffering, because d2d is already 
            // double buffered.


            // note about resources,  brushes, and bitmaps, ect.
            //1- there is no Pen in direct2d. everything is drawn using brushes. 
            //2- resource should be created once and reused during life-time of the control.
            //3- for example you can create few brushes then use color property to draw 
            //   with different color.
            //4- there are two way to create resource 
            //    a- using D2dGraphics.CreateXXX(..) method
            //    b- using D2dFactory.CreateXXXX(..) method.
            //    resources created with D2dGraphics is device dependent resources.
            //    resources created with D2dFactory are device independent resource
            //    that can be shared between D2dGraphics.
            //    look at direct2d docs at msdn for more info.
            // use D2dFactory to create resource because you can share them between 
            //   just like GDI brushes and pens.
            //
            // you can create static resource.Example  private static D2dBrush s_brush // is ok.
            // static resource will be auto disposed when the app exits.

            CreateRTResources();
            m_d2dGraphics.RecreateResources += delegate
            {
                ReleaseRTResources();
                CreateRTResources();
            };

            CreateSharedResources();
        }


        /// <summary>
        /// Release resources created by D2dGraphics.</summary>
        private void ReleaseRTResources()
        {
            if (m_bitmapgraphics != null)
                m_bitmapgraphics.Dispose();
        }

        /// <summary>
        /// Create resources using d2dgraphics.
        /// </summary>
        private void CreateRTResources()
        {
            // How to render to backbuffer.
            //create off screen buffer and render to it.
            // 1- create compatible bitmap graphics.
            //    in GDI+ is create a bitmap then create graphics from it, 
            // 2- use D2dBitmapGraphics to render something.
            // 3- Get backbuffer as  bitmap and draw it using D2dHwndGraphics.           
            m_bitmapgraphics = m_d2dGraphics.CreateCompatibleGraphics(
                new SizeF(100, 100), D2dCompatibleGraphicsOptions.None);
        }

        /// <summary>
        /// Create shared resources using D2dFactory</summary>
        private void CreateSharedResources()
        {
            // create few solid color bruhes.
            m_brush1 = D2dFactory.CreateSolidBrush(Color.FromArgb(128, Color.Blue));
            m_brush2 = D2dFactory.CreateSolidBrush(Color.Black);
            m_brush3 = D2dFactory.CreateSolidBrush(Color.Black);

            // create linear gradient brush for painting complex state (state samples).
            // copied from state machine editor.

            Color stateColor1 = System.Drawing.Color.FromArgb(142, 182, 243);
            float retain = 0.75f;
            int stred = (int)(stateColor1.R * retain);
            int stgreen = (int)(stateColor1.G * retain);
            int stblue = (int)(stateColor1.B * retain);
            Color stateColor2 = Color.FromArgb(stred, stgreen, stblue);
            D2dGradientStop[] gradstops = 
            { 
                new D2dGradientStop(stateColor1, 0),
                new D2dGradientStop(stateColor2, 1.0f),
                new D2dGradientStop(Color.WhiteSmoke, 1),

            };
            m_titlebrush = D2dFactory.CreateLinearGradientBrush(gradstops);


            m_darkenBrush = D2dFactory.CreateLinearGradientBrush(
                new D2dGradientStop(Color.FromArgb(0, 0, 0, 0), 0.0f),
                new D2dGradientStop(Color.FromArgb(100, 0, 0, 0), 1.0f));

            // create text format object for rendering the State name in the State Machine mode
            // note that D2dTextFormat is like System.Drawing.StringFormat but it has font too.                        
            float fontsize = 12;
            m_stateTextFormat = D2dFactory.CreateTextFormat("Trebuchet MS", D2dFontWeight.Bold
                , D2dFontStyle.Normal, D2dFontStretch.Normal, fontsize, "");

            m_stateTextFormat.WordWrapping = D2dWordWrapping.NoWrap;
            m_stateTextFormat.TextAlignment = D2dTextAlignment.Leading;
            m_stateTextFormat.ParagraphAlignment = D2dParagraphAlignment.Center;
            D2dTrimming trimming = new D2dTrimming();
            trimming.Delimiter = 0;
            trimming.DelimiterCount = 0;
            trimming.Granularity = D2dTrimmingGranularity.Character;
            m_stateTextFormat.Trimming = trimming;

            // create few states
            SizeF stSize = new SizeF(96, 96);
            for (int i = 0; i < 5; i++)
            {
                State st = new State();
                st.Bound.Location = new PointF(i * stSize.Width + (i + 1) * 50, (i + 1) * stSize.Height / 3);
                st.Bound.Size = stSize;
                st.Name = "State_" + i.ToString();
                m_states.Add(st);
            }
            m_states[2].Name = "long state name";
            m_states[2].Bound.Width = stSize.Width + 30;

            // rounded rectangle for drawing state body.
            stRect.RadiusX = 14;
            stRect.RadiusY = 14;


            // create bitmap resources

            string level = Application.StartupPath + "\\Resources\\Level1.png";
            m_bmp = D2dFactory.CreateBitmap(level);

            m_emptyBmp = D2dFactory.CreateBitmap(400, 400);
            Bitmap bmp = m_emptyBmp.GdiBitmap;
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Blue);
            g.DrawLine(Pens.Yellow, new Point(0, 0), new Point(bmp.Width, bmp.Height));
            m_emptyBmp.Update();


            

            
            // create bitmap brush
            m_bmpBrush = D2dFactory.CreateBitmapBrush(m_bmp);
            m_bmpBrush.ExtendModeX = D2dExtendMode.Wrap;
            m_bmpBrush.ExtendModeY = D2dExtendMode.Wrap;
            m_bmpBrush.InterpolationMode = D2dBitmapInterpolationMode.NearestNeighbor;


            // create text format object for various test modes
            m_generalTextFormat = D2dFactory.CreateTextFormat("Calibri"
                , D2dFontWeight.Bold, D2dFontStyle.Normal, D2dFactory.FontSizeToPixel(16));

            m_generalTextFormat.WordWrapping = D2dWordWrapping.NoWrap;


            // generate some random color.
            Random r = new Random(7373);
            for (int i = 0; i < 3000; i++)
            {
                int red = r.Next(255);
                int green = r.Next(255);
                int blue = r.Next(255);
                m_colors.Add(Color.FromArgb(red, green, blue));
            }



            D2dGradientStop[] radGradstops = new D2dGradientStop[3];
            D2dGradientStop[] linearGradstops = new D2dGradientStop[4];
            D2dGradientStop[] linearGradstops2 = new D2dGradientStop[2];

            for (int i = 0; i < 60; i++)
            {
                Color c1 = m_colors[i];
                Color c2 = Color.FromArgb(c1.R / 2, c1.G / 2, c1.B / 2);

                radGradstops[0] = new D2dGradientStop(Color.FromArgb(255, c1), 0);
                radGradstops[1] = new D2dGradientStop(Color.FromArgb(240, c2), 0.94f);
                radGradstops[2] = new D2dGradientStop(Color.FromArgb(255, c1), 1.0f);
                D2dRadialGradientBrush radbrush
                      = D2dFactory.CreateRadialGradientBrush(radGradstops);
                m_radialBrushes.Add(radbrush);

                linearGradstops[0] = new D2dGradientStop(Color.FromArgb(0, c1), 0);
                linearGradstops[1] = new D2dGradientStop(Color.FromArgb(200, c2), 0.7f);
                linearGradstops[2] = new D2dGradientStop(Color.FromArgb(255, c2), 0.90f);
                linearGradstops[3] = new D2dGradientStop(Color.FromArgb(255, c1), 1.0f);
                D2dLinearGradientBrush linearBrush
                    = D2dFactory.CreateLinearGradientBrush(linearGradstops);

                m_linearBrushes.Add(linearBrush);

                linearGradstops2[0] = new D2dGradientStop(Color.FromArgb(255, Color.White), 0);
                linearGradstops2[1] = new D2dGradientStop(Color.FromArgb(0, c1), 1.0f);
                D2dLinearGradientBrush linearBrush2
                    = D2dFactory.CreateLinearGradientBrush(linearGradstops2);
                m_linearBrushes2.Add(linearBrush2);

            }
        }

        /// <summary>
        /// Release resources created by D2dFactory.
        /// </summary>
        private void ReleaseSharedResources()
        {
            m_bmp.Dispose();
            m_emptyBmp.Dispose();
            m_brush1.Dispose();
            m_stateTextFormat.Dispose();            
            m_brush2.Dispose();
            m_titlebrush.Dispose();
            m_brush3.Dispose();
            m_generalTextFormat.Dispose();
            m_bmpBrush.Dispose();
            m_darkenBrush.Dispose();

            foreach (D2dRadialGradientBrush brush in m_radialBrushes)
            {
                brush.Dispose();
            }
            m_radialBrushes.Clear();

            foreach (D2dLinearGradientBrush brush in m_linearBrushes)
            {
                brush.Dispose();
            }
            m_linearBrushes.Clear();


            foreach (D2dLinearGradientBrush brush in m_linearBrushes2)
            {
                brush.Dispose();
            }
            m_linearBrushes2.Clear();
        }

        /// <summary>
        /// Raises the MouseDown event and performs custom processing</summary>
        /// <param name="e">MouseEventArgs containing event data</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.Focus();
        }
        /// <summary>
        /// Raises the MouseMove event and performs custom processing</summary>
        /// <param name="e">MouseEventArgs containing event data</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Render();
        }

        int scr = 2;
        /// <summary>
        /// Raises the SizeChanged event and performs custom processing</summary>
        /// <param name="e">EventArgs containing event data</param>
        protected override void OnSizeChanged(EventArgs e)
        {                        
            m_d2dGraphics.Resize(this.Size);

            base.OnSizeChanged(e);
            // recompute lists used for the demo.
            GenPrimitives();
        }
        /// <summary>
        /// Raises the PaintBackground event and performs custom processing</summary>
        /// <param name="pevent">PaintEventArgs containing event data</param>
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }
        /// <summary>
        /// Handles the Paint event and performs custom processing</summary>
        /// <param name="e">PaintEventArgs containing event data</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            Render();
           // this.Invalidate();
        }
      
        /// <summary>
        /// Tests if this is input key for changing display</summary>
        /// <param name="keyData">Key code</param>
        /// <returns>True iff input key</returns>
        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Down || keyData == Keys.Up)
                return true;
            return base.IsInputKey(keyData);
        }


        /// <summary>
        /// Raises the KeyDown event and performs custom processing</summary>
        /// <param name="e">KeyEventArgs containing event data</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            SampleDrawings sampleDrawing = m_sampleDrawing;
            int maxval = (int)SampleDrawings.LastValue - 1;
            int curVal = (int)sampleDrawing;

            if (e.KeyCode == Keys.T)
            {
                StressTest();
            }
            else if (e.KeyCode == Keys.Up)
            {
                curVal--;
                if (curVal < 0) curVal = 0;
            }
            else if (e.KeyCode == Keys.Down)
            {
                curVal++;
                if (curVal > maxval) curVal = maxval;
            }
            sampleDrawing = (SampleDrawings)curVal;
            
            if (sampleDrawing != m_sampleDrawing)
            {
                m_sampleDrawing = sampleDrawing;
                GenPrimitives();
                Render();
            }
            if (e.KeyCode == Keys.C)
            {

                flip = !flip;
                if (flip)
                {
                    m_generalTextFormat.TextAlignment = D2dTextAlignment.Center;
                    m_generalTextFormat.ParagraphAlignment = D2dParagraphAlignment.Center;
                }
                else
                {
                    m_generalTextFormat.TextAlignment = D2dTextAlignment.Leading;
                    m_generalTextFormat.ParagraphAlignment = D2dParagraphAlignment.Near;
                }
                Render();
            }
            Console.WriteLine(m_sampleDrawing);
        }

        bool flip;

        private void Render()
        {            
            SizeF csize = m_d2dGraphics.Size;
            int w = (int)csize.Width;
            int h = (int)csize.Height;
            Size clsize = this.ClientSize;
            
            PointF pt = this.PointToClient(Control.MousePosition);


            frmclk.Start(); // start frame timer.

            m_d2dGraphics.BeginDraw();
            m_d2dGraphics.Transform = Matrix3x2F.Identity;
            m_d2dGraphics.Clear(m_bkgColor);
                       

            switch (m_sampleDrawing)
            {
                case SampleDrawings.DrawFewStates:
                    {
                        m_d2dGraphics.AntialiasMode = D2dAntialiasMode.PerPrimitive;
                        m_d2dGraphics.TextAntialiasMode = D2dTextAntialiasMode.Default;

                        float scale = 2.0f;
                        Matrix3x2F xform = Matrix3x2F.CreateScale(scale, scale);
                        m_d2dGraphics.Transform = xform;
                        m_brush1.Color = Color.Black;
                        m_brush2.Color = Color.White;
                        foreach (State state in m_states)
                        {
                            DrawState(state, scale);
                        }
                        m_d2dGraphics.Transform = Matrix3x2F.Identity;
                    }
                    break;
                case SampleDrawings.FillSolidRects:
                    {
                        
                        // turn off AA
                        m_d2dGraphics.AntialiasMode = D2dAntialiasMode.Aliased;
                        for (int i = 0; i < m_rects.Count; i++)
                            m_d2dGraphics.FillRectangle(m_rects[i], m_colors[i]);
                    }
                    break;

                case SampleDrawings.FillBmpRects:
                    {
                         //turn off AA
                        m_d2dGraphics.AntialiasMode = D2dAntialiasMode.PerPrimitive;
                        for (int i = 1; i < 5; i++)
                        {
                            RectangleF rect = new RectangleF(20 + i * 50, 10, 50, 50);
                            m_bmpBrush.Location = rect.Location;
                            m_d2dGraphics.FillRectangle(rect, m_bmpBrush);
                        }

                    }
                    break;
                case SampleDrawings.FillGradientRects1:
                    {                        
                        // below is a way to use one linear gradient brush 
                        // to fill rectangle with the given color.
                        // this method works best for  non-overlapping rectangles.
                        //
                        // The first loop renders all the rectangles using FillRectangle(RectangleF rect, Color color)
                        // The second loop fill rectangles using linear gradient brush.
                        // The reason for using two loops is to minimize render state changes.
                        // (changing render state is expensive for hardware d2d graphics).
                        
                       
                        // turn off AA
                        m_d2dGraphics.AntialiasMode = D2dAntialiasMode.Aliased;

                        // two loops methods does not work for this example.
                        // because rectangles are overlapping.
                        for (int i = 0; i < m_rects.Count; i++)
                        {
                            RectangleF rect = m_rects[i];                            

                            m_d2dGraphics.FillRectangle(rect, m_colors[i]);

                            m_darkenBrush.StartPoint = rect.Location;
                            m_darkenBrush.EndPoint = new PointF(rect.X, rect.Bottom);
                            m_d2dGraphics.FillRectangle(rect, m_darkenBrush);
                        }
                    }
                    break;
                case SampleDrawings.FillGradientRects2:
                    {
                        // turn off AA
                        m_d2dGraphics.AntialiasMode = D2dAntialiasMode.Aliased;
                        for (int i = 0; i < m_rects.Count; i++)
                        {
                            RectangleF rect = m_rects[i];
                            Color start = m_colors[i];
                            Color end = Color.FromArgb((int)(start.R / 1.90f), (int)(start.G / 1.90f), (int)(start.B / 1.90f));
                            PointF pt1 = rect.Location;
                            PointF pt2 = new PointF(rect.X, rect.Bottom);

                            // note for each unique color a new linear gradient brush 
                            // will be created and cached for reuse.
                            m_d2dGraphics.FillRectangle(rect, pt1, pt2, start, end);
                        }                        
                    }
                    break;

                case SampleDrawings.FillGradientRects3:
                    {
                        // turn off AA
                        m_d2dGraphics.AntialiasMode = D2dAntialiasMode.Aliased;
                        for (int i = 0; i < m_rects.Count; i++)
                        {
                            RectangleF rect = m_rects[i];
                            m_titlebrush.StartPoint = rect.Location;
                            m_titlebrush.EndPoint = new PointF(rect.Right, rect.Bottom);
                            m_d2dGraphics.FillRectangle(rect, m_titlebrush);
                        }
                    }
                    break;

                case SampleDrawings.DrawTriangle:
                    {                     
                        m_d2dGraphics.AntialiasMode = D2dAntialiasMode.PerPrimitive;
                        PointF[] polys
                            = {
                                  new PointF(200,200),
                                  new PointF(250,300),
                                  new PointF(150,300),
                              };
                        m_d2dGraphics.FillPolygon(polys, Color.DarkBlue);
                        m_d2dGraphics.DrawPolygon(polys, Color.Yellow, 3.0f);                          
                    }
                    break;
                case SampleDrawings.DrawRects:
                    {
                        // turn off AA
                        m_d2dGraphics.AntialiasMode = D2dAntialiasMode.Aliased;
                        for (int i = 0; i < m_rects.Count; i++)
                        {
                            m_brush1.Color = m_colors[i];
                            m_d2dGraphics.DrawRectangle(m_rects[i], m_brush1, 2.0f);
                        }
                    }
                    break;
                case SampleDrawings.DrawRectsWithBitmapMasks:
                    {
                       //  turn off AA
                        m_d2dGraphics.AntialiasMode = D2dAntialiasMode.Aliased;
                        for (int i = 0; i < m_rects.Count; i++)
                        {
                            m_brush1.Color = m_colors[i];
                            m_d2dGraphics.FillOpacityMask(m_bmp, m_brush1, m_rects[i]);
                        }
                    }
                    break;
                case SampleDrawings.FillSolidRoundedRects:
                    {
                        // turn On AA
                        m_d2dGraphics.AntialiasMode = D2dAntialiasMode.PerPrimitive;

                        // fill rounded rectangles                        
                        for (int i = 0; i < m_roundedRects.Count; i++)
                        {
                            m_brush1.Color = m_colors[i];
                            m_d2dGraphics.FillRoundedRectangle(m_roundedRects[i], m_brush1);
                        }
                    }
                    break;
                case SampleDrawings.DrawRoundedRects:
                    {
                        // turn On AA
                        m_d2dGraphics.AntialiasMode = D2dAntialiasMode.PerPrimitive;

                        // fill rounded rectangles
                        for (int i = 0; i < m_roundedRects.Count; i++)
                        {
                            m_brush1.Color = m_colors[i];
                            m_d2dGraphics.DrawRoundedRectangle(m_roundedRects[i], m_brush1, 1.0f);
                        }
                    }
                    break;
                case SampleDrawings.FillSolidEllipse:
                    {
                        // turn On AA
                        m_d2dGraphics.AntialiasMode = D2dAntialiasMode.PerPrimitive;

                        for (int i = 0; i < m_ellipses.Count; i++)
                        {
                            m_brush1.Color = m_colors[i];
                            m_d2dGraphics.FillEllipse(m_ellipses[i], m_brush1);
                        }
                    }
                    break;
                case SampleDrawings.DrawEllipse:
                    {
                        // turn On AA
                        m_d2dGraphics.AntialiasMode = D2dAntialiasMode.PerPrimitive;

                        for (int i = 0; i < m_ellipses.Count; i++)
                        {
                            m_brush1.Color = m_colors[i];
                            m_d2dGraphics.DrawEllipse(m_ellipses[i], m_brush1, 2.0f);
                        }
                    }
                    break;
                case SampleDrawings.DrawOrbs:
                    {                                                
                        m_d2dGraphics.AntialiasMode = D2dAntialiasMode.PerPrimitive;
                        for (int i = 0; i < m_ellipses.Count; i++)
                        {
                            D2dRadialGradientBrush
                            radialBrush = m_radialBrushes[i];
                            D2dEllipse ellipse = m_ellipses[i];
                            PointF center = ellipse.Center;
                            
                            radialBrush.Center = center;                            
                            radialBrush.RadiusX = ellipse.RadiusX;
                            radialBrush.RadiusY = ellipse.RadiusY;

                            D2dLinearGradientBrush linearBrush = m_linearBrushes[i];
                            linearBrush.StartPoint = new PointF(0, center.Y - ellipse.RadiusY);
                            linearBrush.EndPoint = new PointF(0, center.Y + ellipse.RadiusY);
                                                                               
                            m_d2dGraphics.FillEllipse(m_ellipses[i], radialBrush);
                            m_d2dGraphics.FillEllipse(ellipse, linearBrush);
                           
                            D2dEllipse glassy = new D2dEllipse();
                            glassy.Center =
                                new PointF(center.X, center.Y - ellipse.RadiusY * 0.5f);
                            glassy.RadiusX = ellipse.RadiusX * 0.75f;
                            glassy.RadiusY = ellipse.RadiusY * 0.5f;


                            D2dLinearGradientBrush linearBrush2 = m_linearBrushes2[i];
                            linearBrush2.StartPoint
                                = new PointF(0, glassy.Center.Y - glassy.RadiusY);
                            linearBrush2.EndPoint
                                = new PointF(0, glassy.Center.Y + glassy.RadiusY);

                            m_d2dGraphics.FillEllipse(glassy, linearBrush2);
                        }

                    }
                    break;
                case SampleDrawings.DrawRandomLines1:
                    {
                        // turn AA off for thin lines.
                        m_d2dGraphics.AntialiasMode = D2dAntialiasMode.PerPrimitive;
                        //m_d2dGraphics.AntialiasMode = D2dAntialiasMode.Aliased;
                        float width = scr;
                        for (int i = 0; i < m_lines.Count; i++)
                        {
                            m_brush1.Color = m_colors[i];
                            Line line = m_lines[i];
                            m_d2dGraphics.DrawLine(line.P1, line.P2, m_brush1, width);
                        }                        
                    }
                    break;
                case SampleDrawings.DrawRandomLines2:
                    {
                        // OK turn AA ON for thick lines.
                        m_d2dGraphics.AntialiasMode = D2dAntialiasMode.PerPrimitive;

                        for (int i = 0; i < m_lines.Count; i++)
                        {
                            m_brush1.Color = m_colors[i];
                            Line line = m_lines[i];
                            m_d2dGraphics.DrawLine(line.P1, line.P2, m_brush1, 2.0f);
                        }

                    }
                    break;
                case SampleDrawings.UseClipRectangle:
                    {

                        m_brush1.Color = Color.Yellow;

                        m_d2dGraphics.DrawText
                            ("Use Clip Rectangle", m_generalTextFormat,
                            new RectangleF(10, 10, 200, 50), m_brush1);
                        // OK turn AA ON for thick lines.
                        m_d2dGraphics.AntialiasMode = D2dAntialiasMode.PerPrimitive;

                        RectangleF clipRect
                            = new RectangleF(120, 120, 400,400);

                        m_d2dGraphics.PushAxisAlignedClip(clipRect);

                        for (int i = 0; i < m_lines.Count; i++)
                        {
                            m_brush1.Color = m_colors[i];
                            Line line = m_lines[i];
                            m_d2dGraphics.DrawLine(line.P1, line.P2, m_brush1, 2.0f);
                        }

                        m_d2dGraphics.PopAxisAlignedClip();

                    }
                    break;
                case SampleDrawings.DrawConnectedLines:
                    {
                        // OK turn AA ON for thick lines.
                        m_d2dGraphics.AntialiasMode = D2dAntialiasMode.PerPrimitive;

                        m_brush1.Color = Color.White;
                        m_d2dGraphics.DrawLines(m_connectedLines, m_brush1, 2.0f);
                    }
                    break;
                case SampleDrawings.DrawBeziers:
                    {
                        // turn AA on.
                        m_d2dGraphics.AntialiasMode = D2dAntialiasMode.PerPrimitive;
                        float width = 2.0f;                        
                        int c = 0;
                        foreach (Bezier bz in m_beziers)
                        {
                            m_brush1.Color = m_colors[c++];
                            m_d2dGraphics.DrawBezier(
                                bz.P1, bz.P2, bz.P3, bz.P4, m_brush1, width);
                        }
                    }
                    break;
                case SampleDrawings.DrawCachedBitmap:
                    {

                        // render to offscreen buffer.
                        m_bitmapgraphics.BeginDraw();
                        m_bitmapgraphics.Clear(Color.Green);
                        m_bitmapgraphics.FillRectangle(new RectangleF(10, 10, 80, 80), Color.Yellow);
                        m_bitmapgraphics.EndDraw();

                        using (D2dBitmap bmp = m_bitmapgraphics.GetBitmap())
                        {
                            m_d2dGraphics.DrawBitmap(bmp, new PointF(10, 10), 1.0f);
                        }
                    }
                    break;
                case SampleDrawings.DrawText:
                    {
                        m_d2dGraphics.TextAntialiasMode = D2dTextAntialiasMode.Default;
                        m_d2dGraphics.AntialiasMode = D2dAntialiasMode.PerPrimitive;

                        for (int i = 0; i < m_texts.Count; i++)
                        {                           
                            m_brush1.Color = m_colors[i];                            
                             m_d2dGraphics.DrawText(
                                  m_drawInfo,
                                  m_generalTextFormat,
                                  m_texts[i],
                                  m_brush1);
                        }
                    }

                    break;
                case SampleDrawings.DrawTextLayout:
                    {
                        m_d2dGraphics.TextAntialiasMode = D2dTextAntialiasMode.Default;
                        m_d2dGraphics.AntialiasMode = D2dAntialiasMode.PerPrimitive;
                        for (int i = 0; i < m_texts.Count; i++)
                        {                            
                            m_brush1.Color = m_colors[i];
                            m_d2dGraphics.DrawTextLayout(m_texts[i], m_textLayouts[i], m_brush1);                            
                        }
                    }
                    break;
                case SampleDrawings.DrawBitmaps:
                    {
                        Random rnd = new Random(7533);

                        for (int i = 0; i < 20; i++)
                        {
                            PointF bmpPt
                                = new PointF(rnd.Next(w), rnd.Next(h));
                            m_d2dGraphics.DrawBitmap(m_bmp, bmpPt, 1.0f - i / 20.0f);
                        }
                        m_d2dGraphics.DrawBitmap(m_emptyBmp, new PointF(5, 5), 1.0f);
                    }
                    break;
                case SampleDrawings.GdiInterOp:
                    {
                        m_d2dGraphics.BeginGdiSection();
                        m_d2dGraphics.Graphics.SmoothingMode
                            = System.Drawing.Drawing2D.SmoothingMode.None;
                        
                        Pen p = new Pen(Color.Gold);
                        for (int i = 10; i < 200; i++)
                        {
                            Rectangle rect
                                = new Rectangle(i, i, i, i);
                            p.Color = m_colors[i];
                            m_d2dGraphics.Graphics.DrawRectangle(p, rect);
                        }
                        p.Dispose();                                              
                        m_d2dGraphics.EndGdiSection();                        
                    }
                    break;
                case SampleDrawings.LastValue:
                    break;
                default:
                    break;
            }

         
            m_brush1.Color = Color.Blue;
            RectangleF msRect
                = new RectangleF(pt, new SizeF(4, 4));
            msRect.Offset(-2, -2);

            m_d2dGraphics.FillRectangle(msRect, m_brush1);

            m_d2dGraphics.EndDraw();
                        
            m_frameCount++;
            m_cumulativeTime += (float)frmclk.Elapsed;

            // compute frames-per-second and time-per-frame regularly
            if (m_frameCount > 30)
            {
                m_fps = m_frameCount / m_cumulativeTime;
                m_cumulativeTime = 0;
                m_frameCount = 0;

                UpdateInfo();
            }
        }

        private void GenPrimitives()
        {
            
            SizeF csize = m_d2dGraphics.Size;            

            Random r = new Random(7737);
            int w = (int)csize.Width;
            int h = (int)csize.Height;

            if (m_sampleDrawing == SampleDrawings.DrawFewStates)
            {
                m_drawInfo = "draw few states";
            }
            else if (m_sampleDrawing == SampleDrawings.FillSolidRects
                || m_sampleDrawing == SampleDrawings.FillGradientRects1
                || m_sampleDrawing == SampleDrawings.FillGradientRects2
                || m_sampleDrawing == SampleDrawings.FillGradientRects3
                || m_sampleDrawing == SampleDrawings.DrawRects
                || m_sampleDrawing == SampleDrawings.DrawRectsWithBitmapMasks
                || m_sampleDrawing == SampleDrawings.FillBmpRects)

            {
                int itemCount = 500;
                m_rects.Clear();
                for (int i = 0; i < itemCount; i++)
                {
                    float cx = r.Next(-40, w);
                    float cy = r.Next(-40, h);
                    float width = r.Next(20, 140);
                    float height = r.Next(20, 140);
                    RectangleF rect =
                        new RectangleF(cx, cy, width, height);
                    m_rects.Add(rect);
                }

                if (m_sampleDrawing == SampleDrawings.FillSolidRects)
                    m_drawInfo = string.Format("Fill {0} solid rectangles", itemCount);
                else if (m_sampleDrawing == SampleDrawings.FillGradientRects1
                    || m_sampleDrawing == SampleDrawings.FillGradientRects2
                    || m_sampleDrawing == SampleDrawings.FillGradientRects3)
                    m_drawInfo = string.Format("Fill {0} gradient rectangles", itemCount);
                else if (m_sampleDrawing == SampleDrawings.FillBmpRects)
                    m_drawInfo = string.Format("Fill {0} rectangles using bitmap brush", itemCount);
                else if (m_sampleDrawing == SampleDrawings.DrawRectsWithBitmapMasks)
                    m_drawInfo = string.Format("Fill {0} rectangles using bitmap's alpha blending", itemCount);
                else
                    m_drawInfo = string.Format("Draw {0} rectangles", itemCount);
            }
            else if (m_sampleDrawing == SampleDrawings.FillSolidRoundedRects
                || m_sampleDrawing == SampleDrawings.DrawRoundedRects)
            {
                int itemCount = 300;
                m_roundedRects.Clear();
                for (int i = 0; i < itemCount; i++)
                {
                    float cx = r.Next(-40, w);
                    float cy = r.Next(-40, h);
                    float width = r.Next(20, 140);
                    float height = r.Next(20, 140);
                    D2dRoundedRect roundRect = new D2dRoundedRect();
                    roundRect.Rect = new RectangleF(cx, cy, width, height);
                    roundRect.RadiusX = 12;
                    roundRect.RadiusY = 12;
                    m_roundedRects.Add(roundRect);
                }

                if (m_sampleDrawing == SampleDrawings.FillSolidRoundedRects)
                    m_drawInfo = string.Format("Fill {0} solid rounded rectangles", itemCount);
                else
                    m_drawInfo = string.Format("Draw {0} rounded rectangles", itemCount);
            }
            else if (m_sampleDrawing == SampleDrawings.DrawRandomLines1
                || m_sampleDrawing == SampleDrawings.DrawRandomLines2)
            {
                int itemCount = 2000;
                m_lines.Clear();
                for (int i = 0; i < itemCount; i++)
                {

                    PointF pt1 = new PointF(r.Next(w), r.Next(h));
                    //PointF pt2 = new PointF(pt1.X + r.Next(-20, 20), pt1.Y+r.Next(-20, 20));
                    PointF pt2 = new PointF(r.Next(w), r.Next(h));
                    m_lines.Add(new Line(pt1, pt2));
                }

                if (m_sampleDrawing == SampleDrawings.DrawRandomLines1)
                    m_drawInfo = string.Format("Draw {0} lines width = 1", itemCount);
                else
                    m_drawInfo = string.Format("Draw {0} lines width = 2", itemCount);
            }
            else if (m_sampleDrawing == SampleDrawings.DrawConnectedLines)
            {
                int itemCount = 200;
                m_connectedLines.Clear();
                for (int i = 0; i < itemCount; i++)
                {
                    PointF pt = new PointF(r.Next(w), r.Next(h));
                    m_connectedLines.Add(pt);
                }
                m_drawInfo = string.Format("Draw {0} connected lines width = 2", itemCount - 1);
            }
            else if (m_sampleDrawing == SampleDrawings.FillSolidEllipse)
            {
                int itemCount = 300;
                m_ellipses.Clear();
                for (int i = 0; i < itemCount; i++)
                {
                    D2dEllipse elp = new D2dEllipse();
                    elp.Center = new PointF(r.Next(w), r.Next(h));
                    elp.RadiusX = r.Next(20, 120);
                    elp.RadiusY = r.Next(20, 120);
                    m_ellipses.Add(elp);
                }

                m_drawInfo = string.Format("Fill {0} solid ellipses", itemCount);
            }
            else if (m_sampleDrawing == SampleDrawings.DrawOrbs)
            {
                int itemCount = 60;
                m_ellipses.Clear();
                for (int i = 0; i < itemCount; i++)
                {
                    D2dEllipse elp = new D2dEllipse();
                    elp.Center = new PointF(r.Next(w), r.Next(h));
                    float rad = r.Next(60,120);
                    elp.RadiusX = rad;
                    elp.RadiusY = rad;
                    m_ellipses.Add(elp);
                }
                m_drawInfo = string.Format("Render {0} Glass balls", itemCount);

            }
            else if (m_sampleDrawing == SampleDrawings.DrawEllipse)
            {
                int itemCount = 200;
                m_ellipses.Clear();
                for (int i = 0; i < itemCount; i++)
                {
                    int k = i + 10;
                    D2dEllipse elp = new D2dEllipse();
                    elp.Center = new PointF(k + k / 2, k + k / 2);
                    elp.RadiusX = k / 2;
                    elp.RadiusY = k / 2;
                    m_ellipses.Add(elp);
                }
                m_drawInfo = string.Format("Draw {0} ellipses", itemCount);
            }
            else if (m_sampleDrawing == SampleDrawings.DrawBeziers)
            {
                r = new Random(7737);
                int itemCount = 200;
                Bezier bz = new Bezier();
                
                bz.P1 = new PointF(0, 0);
                bz.P2 = new PointF(20f, 25f);
                bz.P3 = new PointF(40f, -25f);
                bz.P4 = new PointF(60f, 25f);

                m_beziers.Clear();
                for (int i = 0; i < itemCount; i++)
                {
                    Bezier b = new Bezier();
                    SizeF sz = new SizeF();
                    sz.Width = r.Next(w);
                    sz.Height = r.Next(h);
                    b.P1 = bz.P1 + sz;
                    b.P2 = bz.P2 + sz;
                    b.P3 = bz.P3 + sz;
                    b.P4 = bz.P4 + sz;
                    m_beziers.Add(b);
                }
                m_drawInfo = string.Format("Draw {0} beziers", itemCount);
            }
            else if (m_sampleDrawing == SampleDrawings.DrawCachedBitmap)
            {
                m_drawInfo = "Draw of screen buffer (bitmap)";
            }
            else if (m_sampleDrawing == SampleDrawings.DrawText)
            {
                r = new Random(7737);
                int itemCount = 200;
                m_texts.Clear();                
                for (int i = 0; i < itemCount; i++)
                {
                    PointF pt = new PointF(r.Next(w), r.Next(h));
                    m_texts.Add(pt);                    
                }
                m_drawInfo = string.Format("Draw {0} text", itemCount);
            }
            else if (m_sampleDrawing == SampleDrawings.DrawTextLayout)
            {
                foreach (D2dTextLayout txtlayout in m_textLayouts)
                    txtlayout.Dispose();

                r = new Random(7737);
                int itemCount = 200;
                m_texts.Clear();                
                m_textLayouts.Clear();
                for (int i = 0; i < itemCount; i++)
                {
                    PointF pt = new PointF(r.Next(w), r.Next(h));
                    m_texts.Add(pt);
                    D2dTextLayout txtLayout =
                        D2dFactory.CreateTextLayout("Draw Text " + i, m_generalTextFormat);
                    m_textLayouts.Add(txtLayout);                    
                }
                m_drawInfo = string.Format("Draw {0} text layout", itemCount);

            }
            else if (m_sampleDrawing == SampleDrawings.DrawBitmaps)
            {
                m_drawInfo = "Draw overlapping transparent bitmaps";
            }
            else if (m_sampleDrawing == SampleDrawings.GdiInterOp)
            {
                m_drawInfo = "Gdi interop";
            }
            else if (m_sampleDrawing == SampleDrawings.DrawTriangle)
            {
                m_drawInfo = "Fill and draw a triangle";
            }

            m_fps = 0.0f;
            UpdateInfo();
        }

        private D2dRoundedRect stRect = new D2dRoundedRect();
        private const float TitleBarHeight = 26;
        private void DrawState(State state, float scale)
        {
            RectangleF stbound = state.Bound;
            stRect.Rect = stbound;

            // paint state body.
            m_titlebrush.StartPoint = new PointF(0, stbound.Y);
            m_titlebrush.EndPoint = new PointF(0, stbound.Y + TitleBarHeight);
            m_d2dGraphics.FillRoundedRectangle(stRect, m_titlebrush);

            float outlinewidth = 2.0f / scale;

            m_d2dGraphics.DrawRoundedRectangle(stRect, m_brush1, outlinewidth);
            m_d2dGraphics.DrawLine(
                new PointF(stbound.X, stbound.Y + TitleBarHeight),
                new PointF(stbound.Right, stbound.Y + TitleBarHeight),
                m_brush1, outlinewidth);

            // draw state name.
            RectangleF nameRect =
                new RectangleF(stbound.X + 10, stbound.Y, stbound.Width - 20, TitleBarHeight);
            m_d2dGraphics.DrawText(state.Name, m_stateTextFormat, nameRect, m_brush2);
        }

        private void UpdateInfo()
        {
            string msg = "Press T to run performance test and  up/down arrow keys to change drawings";
            if (m_fps == 0)
            {
                Parent.Text = "Mouse move to compute Fps  " + m_drawInfo + msg;                    
            }
            else
            {
                float tps = (1000.0f / m_fps);
                string info = string.Format("Fps:{0}  Tpf:{1} ms    {2} {3}",
                    m_fps.ToString("F"),
                    tps.ToString("F2"),
                    m_drawInfo,
                    msg);
                Parent.Text = info;
            }
        }

        // Runs a stress test to get consistent accurate frame rates. The statistical mean
        //  frame rendering time is probably the most useful piece of data here.
        private void StressTest()
        {
            const int NumFrames = 1000;
            var allFrames = new long[NumFrames];
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < NumFrames; i++)
            {
                long startTick = stopwatch.ElapsedTicks;
                Render();
                long endTick = stopwatch.ElapsedTicks;
                allFrames[i] = endTick - startTick;
            }
            stopwatch.Stop();

            Array.Sort(allFrames); //sorts from lowest to highest
            long meanTicks = allFrames[NumFrames / 2];

            var report = new StringBuilder();
            report.AppendLine("Rendered " + NumFrames + " frames");
            report.AppendLine();
            report.AppendLine("Mean frame rendering time: " + meanTicks + " ticks");
            report.AppendLine("Fastest frame rendering time: " + allFrames[0] + " ticks");
            report.AppendLine("Slowest frame rendering time: " + allFrames[NumFrames - 1] + " ticks");
            report.AppendLine("Total rendering time: " + stopwatch.ElapsedMilliseconds + "ms");
            MessageBox.Show(report.ToString(), "Performance Report");
        }

        
        /// <summary>
        /// Disposes of resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_d2dGraphics.Dispose();
                ReleaseRTResources();
                ReleaseSharedResources();
            }
            base.Dispose(disposing);
        }



        // used for painting state-body.
        private D2dLinearGradientBrush m_titlebrush;

        private D2dLinearGradientBrush m_darkenBrush;

        private List<D2dEllipse> m_ellipses
          = new List<D2dEllipse>();

        private List<Bezier> m_beziers
            = new List<Bezier>();

        private List<RectangleF> m_rects
            = new List<RectangleF>();

        private List<D2dRoundedRect> m_roundedRects
            = new List<D2dRoundedRect>();

        private List<PointF> m_connectedLines
            = new List<PointF>();

        private List<Line> m_lines
            = new List<Line>();

        private List<Color> m_colors
             = new List<Color>();

        private List<PointF> m_texts
            = new List<PointF>();
        private List<D2dTextLayout> m_textLayouts
            = new List<D2dTextLayout>();

        private List<State> m_states
            = new List<State>();

        private List<D2dRadialGradientBrush>
            m_radialBrushes = new List<D2dRadialGradientBrush>();

        private List<D2dLinearGradientBrush>
            m_linearBrushes = new List<D2dLinearGradientBrush>();

        private List<D2dLinearGradientBrush>
            m_linearBrushes2 = new List<D2dLinearGradientBrush>();


        private RectangleF m_infoRect = new RectangleF(0, 0, 500, 32);
        private Clock frmclk = new Clock();
        private float m_cumulativeTime = 0;
        private int m_frameCount = 0;
        private float m_fps = 0;        
        private D2dBitmap m_bmp;
        private D2dBitmap m_emptyBmp;
        private D2dHwndGraphics m_d2dGraphics;
        private D2dBitmapGraphics m_bitmapgraphics;
        private D2dSolidColorBrush m_brush1;
        private D2dSolidColorBrush m_brush2;
        private D2dSolidColorBrush m_brush3;
        private D2dBitmapBrush m_bmpBrush;
        private D2dTextFormat m_stateTextFormat;
        private D2dTextFormat m_generalTextFormat;

        private SampleDrawings m_sampleDrawing = (SampleDrawings)0;
        private string m_drawInfo;
        private Color m_bkgColor = Color.FromArgb(144, 159, 175);


        private enum SampleDrawings
        {
            FillSolidRects,            
            FillGradientRects1,
            FillGradientRects2,
            FillGradientRects3,
            DrawTriangle,
            DrawOrbs,            
            FillBmpRects,
            FillSolidRoundedRects,
            DrawRects,
            DrawRectsWithBitmapMasks,
            DrawRoundedRects,
            FillSolidEllipse,
            DrawEllipse,
            DrawRandomLines1,
            DrawRandomLines2,            
            UseClipRectangle,
            DrawConnectedLines,
            DrawBeziers,
            DrawCachedBitmap,
            DrawText,
            DrawTextLayout,
            DrawBitmaps,
            GdiInterOp,
            DrawFewStates,
            LastValue,
        }
    }

    /// <summary>
    /// Clock for performance timing</summary>
    public class Clock
    {
        /// <summary>
        /// Constructor</summary>
        public Clock()
        {
            Frq = (double)System.Diagnostics.Stopwatch.Frequency;
            m_start = System.Diagnostics.Stopwatch.GetTimestamp();
        }

        /// <summary>
        /// Starting timing an operation</summary>
        public void Start()
        {
            m_start = System.Diagnostics.Stopwatch.GetTimestamp();
        }

        /// <summary>
        /// Gets time elapsed since Start</summary>
        public double Elapsed
        {
            get
            {
                long delta = System.Diagnostics.Stopwatch.GetTimestamp() - m_start;
                return (double)delta / Frq;
            }
        }

        private double Frq;
        private long m_start;
    }

    /// <summary>
    /// State information</summary>
    public class State
    {
        /// <summary>
        /// State rectangle</summary>
        public RectangleF Bound;
        /// <summary>
        /// State name</summary>
        public string Name = "State";
    }
    
    /// <summary>
    /// Bezier points</summary>
    public struct Bezier
    {
        public PointF P1;
        public PointF P2;
        public PointF P3;
        public PointF P4;
    }

    /// <summary>
    /// Struct used for drawing random line segments</summary>
    public struct Line
    {
        /// <summary>
        /// Line data</summary>
        /// <param name="pt1">Point 1 in line</param>
        /// <param name="pt2">Point 2 in line</param>
        public Line(PointF pt1, PointF pt2)
        {
            P1 = pt1;
            P2 = pt2;            
        }        
        /// <summary>
        /// Point 1 in line</summary>
        public PointF P1;
        /// <summary>
        /// Point 2 in line</summary>
        public PointF P2;
    }
}
