//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;


namespace UsingDirect2D
{
    /// <summary>
    /// Simple canvas for testing GDI performance</summary>
    public class GdiCanvas : Control
    {
        /// <summary>
        /// Constructor that creates various graphics objects</summary>
        public GdiCanvas()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint
               | ControlStyles.UserPaint, true);
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;

            this.BackColor = m_bkgColor;

            // create few solid color bruhes.
            m_brush1 = new SolidBrush(Color.White);
            m_brush2 = new SolidBrush(Color.White);
            m_brush3 = new SolidBrush(Color.White);


            // create linear gradient brush for painting complex state (state samples).
            // copied from state machine editor.
            Color stateColor1 = System.Drawing.Color.FromArgb(142, 182, 243);
            float retain = 0.75f;
            int stred = (int)(stateColor1.R * retain);
            int stgreen = (int)(stateColor1.G * retain);
            int stblue = (int)(stateColor1.B * retain);
            Color stateColor2 = Color.FromArgb(stred, stgreen, stblue);
            
            //m_titlebrush = m_d2dGraphics.CreateLinearGradientBrush(gradstops);


            // create bitmap resources
            m_bmp = new Bitmap("Resources\\Level1.png");


            // create texformat for info font.
            m_info = new System.Drawing.Font("Calibri", 16, FontStyle.Bold);
            m_strFormat = new StringFormat();

            // generate some random color.
            Random r = new Random(737);
            for (int i = 0; i < 3000; i++)
            {
                int red = r.Next(255);
                int green = r.Next(255);
                int blue = r.Next(255);
                m_colors.Add(Color.FromArgb(red, green, blue));
            }
                        
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
            this.Invalidate();
        }
        /// <summary>
        /// Raises the SizeChanged event and performs custom processing</summary>
        /// <param name="e">EventArgs containing event data</param>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            // recompute lists used for the demo.
            GenPrimitives();
        }

        /// <summary>
        /// Handles the Paint event and performs custom processing</summary>
        /// <param name="e">PaintEventArgs containing event data</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode
                = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.Bilinear;
           
            Render(e.Graphics);
        }


        private void GenPrimitives()
        {
            
            SizeF csize = this.ClientSize;
            // since backbuffer size is the same as this.Client size.
            
            Random r = new Random(7737);
            int w = (int)csize.Width;
            int h = (int)csize.Height;

           if (m_sampleDrawing == SampleDrawings.FillSolidRects
                
                || m_sampleDrawing == SampleDrawings.Draw_Rects)
            {


                int itemCount = 300;
                m_rects.Clear();
                for (int i = 0; i < itemCount; i++)
                {
                    int cx = r.Next(-40, w);
                    int cy = r.Next(-40, h);
                    int width = r.Next(20, 140);
                    int height = r.Next(20, 140);
                    Rectangle rect =
                        new Rectangle(cx, cy, width, height);
                    m_rects.Add(rect);
                }

                if (m_sampleDrawing == SampleDrawings.FillSolidRects)
                    m_drawInfo = string.Format("Fill {0} solid rectangles", itemCount);                
                else
                    m_drawInfo = string.Format("Draw {0} rectangles", itemCount);
            }

            else if (m_sampleDrawing == SampleDrawings.DrawRandomLines1
                || m_sampleDrawing == SampleDrawings.DrawRandomLines2)
            {
                int itemCount = 2000;
                m_lines.Clear();
                for (int i = 0; i < itemCount; i++)
                {
                    PointF pt1 = new PointF(r.Next(w), r.Next(h));
                    PointF pt2 = new PointF(r.Next(w), r.Next(h));
                    m_lines.Add(new Line(pt1, pt2));
                }

                if (m_sampleDrawing == SampleDrawings.DrawRandomLines1)
                    m_drawInfo = string.Format("Draw {0} lines width = 1", itemCount);
                else
                    m_drawInfo = string.Format("Draw {0} lines width = 2", itemCount);
            }
           
           
           
            else if (m_sampleDrawing == SampleDrawings.DrawBeziers)
            {
                r = new Random(7737);
                int itemCount = 200;
                Bezier bz = new Bezier();
                bz.P1 = new PointF(0, 0);
                bz.P2 = new PointF(20, 25);
                bz.P3 = new PointF(40, -25);
                bz.P4 = new PointF(60, 25);

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
            else if (m_sampleDrawing == SampleDrawings.DrawBitmaps)
            {
                m_drawInfo = "Draw overlapping transparent bitmaps";
            }
           
            m_fps = 0.0f;
            UpdateInfo();
        }



        private void Render(Graphics g)
        {            
            Size csize = this.ClientSize;
            int w = (int)csize.Width;
            int h = (int)csize.Height;

            frmclk.Start(); // start frame timer.

            switch (m_sampleDrawing)
            {

                case SampleDrawings.FillSolidRects:
                    {
                        for (int i = 0; i < m_rects.Count; i++)
                        {
                            m_brush1.Color = m_colors[i];
                            g.FillRectangle(m_brush1, m_rects[i]);
                        }
                    }
                    break;

                case SampleDrawings.Draw_Rects:
                    {
                        Pen p = new Pen(Color.Red);
                        for (int i = 0; i < m_rects.Count; i++)
                        {
                            p.Color = m_colors[i];
                            g.DrawRectangle(p, m_rects[i]);
                        }
                        p.Dispose();
                    }
                    break;

                case SampleDrawings.DrawRandomLines1:
                    {

                        Pen p = new Pen(Color.Red);
                        for (int i = 0; i < m_lines.Count; i++)
                        {
                            p.Color = m_colors[i];
                            Line line = m_lines[i];
                            g.DrawLine(p, line.P1, line.P2);
                        }
                        p.Dispose();
                    }
                    break;
                case SampleDrawings.DrawRandomLines2:
                    {
                        Pen p = new Pen(Color.Red, 2.0f);
                        for (int i = 0; i < m_lines.Count; i++)
                        {
                            p.Color = m_colors[i];
                            Line line = m_lines[i];
                            g.DrawLine(p, line.P1, line.P2);
                        }
                        p.Dispose();

                    }
                    break;

                case SampleDrawings.DrawBeziers:
                    {
                        int c = 0;
                        Pen p = new Pen(Color.White, 2.0f);
                        //Pen p = new Pen(Color.White);                        
                        foreach (Bezier bz in m_beziers)
                        {
                            p.Color = m_colors[c++];
                            g.DrawBezier(p, bz.P1, bz.P2, bz.P3, bz.P4);
                        }
                        p.Dispose();
                    }
                    break;
                case SampleDrawings.DrawText:
                    {
                        RectangleF layoutrect = new RectangleF();
                        layoutrect.Size = g.MeasureString(m_drawInfo, m_info);

                        for (int i = 0; i < m_texts.Count; i++)
                        {
                            m_brush1.Color = m_colors[i];
                            layoutrect.Location = m_texts[i];
                            g.DrawString(m_drawInfo
                                , m_info, m_brush1, layoutrect, m_strFormat);
                        }
                    }

                    break;
                case SampleDrawings.DrawBitmaps:
                    {
                        Random rnd = new Random(7533);

                        RectangleF bmpRect = new RectangleF();
                        bmpRect.Size = m_bmp.Size;

                        for (int i = 0; i < 20; i++)
                        {
                            bmpRect.Location = new PointF(rnd.Next(w), rnd.Next(h));
                            g.DrawImage(m_bmp, bmpRect);
                        }
                    }
                    break;


                default:
                    break;
            }



            m_frameCount++;
            m_cumulativeTime += (float)frmclk.Elapsed;

            // compute fps and tpf every 10 frames.
            if (m_frameCount > 10)
            {
                m_fps = m_frameCount / m_cumulativeTime;
                m_cumulativeTime = 0;
                m_frameCount = 0;

                UpdateInfo();
            }
        }



        /// <summary>
        /// Disposes of resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
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

            if (e.KeyCode == Keys.Up)
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
                this.Invalidate();
            }
            Console.WriteLine(m_sampleDrawing);
        }

        private const float TitleBarHeight = 26;

        private void UpdateInfo()
        {
            if (m_fps == 0)
            {
                this.Parent.Text = "Mouse move to compute Fps  " + m_drawInfo;
            }
            else
            {
                float tps = (1000.0f / m_fps);
                string info
                    = string.Format("Fps:{0}  Tpf:{1} ms    {2}",
                    m_fps.ToString("F"),
                    tps.ToString("F2"),
                    m_drawInfo
                    );
                this.Parent.Text = info;
            }
        }

       

        private Clock frmclk = new Clock();
        private float m_cumulativeTime = 0;
        private int m_frameCount = 0;
        private float m_fps = 0;

        private Bitmap m_bmp;
        private SolidBrush m_brush1;
        private SolidBrush m_brush2;
        private SolidBrush m_brush3;

        private Font m_info;

        private SampleDrawings m_sampleDrawing = (SampleDrawings)0;
        private string m_drawInfo;
        private Color m_bkgColor = Color.FromArgb(144, 159, 175);


        // used for painting state-body.
        //private D2dLinearGradientBrush m_titlebrush;

        //private D2dBitmap m_bmprt;

        //private List<D2dEllipse> m_ellipses
      //    = new List<D2dEllipse>();

        private List<Bezier> m_beziers
            = new List<Bezier>();

        private List<Rectangle> m_rects
            = new List<Rectangle>();



        private List<PointF> m_connectedLines
            = new List<PointF>();

        private List<Line> m_lines
            = new List<Line>();

        private List<Color> m_colors
             = new List<Color>();

        private List<PointF> m_texts
            = new List<PointF>();

        private List<State> m_states
            = new List<State>();

        private StringFormat m_strFormat;
        private RectangleF m_infoRect = new RectangleF(0, 0, 500, 32);

        private enum SampleDrawings
        {
            FillSolidRects,
            //FillGradientRects,
            Draw_Rects,
           // FillSoliedRoundedRects,
           // DrawRoundedRects,
          //  FillSolidEllipse,
         //   DrawEllipse,
            DrawRandomLines1,
            DrawRandomLines2,
         //   DrawConnectedLines,
            DrawBeziers,
         //   DrawcachedBitmap,
            DrawText,
            DrawBitmaps,

         
            LastValue,
        }

    }
}
