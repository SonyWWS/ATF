//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.CurveEditing
{
    /// <summary>
    /// Cartesian 2D canvas</summary>
    public class Cartesian2dCanvas : Control
    {
        /// <summary>
        /// Constructor</summary>
        public Cartesian2dCanvas()
        {
            SuspendLayout();
            Name = "Cartesian2dCanvas";
            Size = new Size(400, 400);
            BackColor =  Color.FromArgb(150, 150, 150);
            ResumeLayout(false);
            m_scaleTextFont = new Font(Font.Name, 8.25f);            
            
            SetStyle(ControlStyles.UserPaint
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw
                , true);
                        
            MinimumSize = new Size(10, 10);
            Zoom_d = new PointD(1, 1);
        }

        
        /// <summary>
        /// Frames to fit a given graph rectangle</summary>
        /// <param name="rect">Rectangle to frame</param>
        public void Frame(RectangleF rect)
        {
            float h = ClientSize.Height;
            float w = ClientSize.Width;
            float hh = h / 2;
            float hw = w / 2;
            if(rect.Width == 0 || rect.Height == 0 )
                return;
            
            // uniform zoom
            //float z = Math.Min(w / rect.Width, h / rect.Height);
            //Zoom = new Vec2F(z, z);
            switch(m_lockorg)
            {
                case OriginLockMode.Free:
                    {
                        Zoom = new Vec2F(w / rect.Width, h / rect.Height);
                        Vec2F center = new Vec2F(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
                        Pan = new Vec2F(hw - center.X * Zoom.X, hh + center.Y * Zoom.Y);
                        break;
                    }
                case OriginLockMode.Center:
                    {
                        float absleft = Math.Abs(rect.Left);
                        float absright = Math.Abs(rect.Right);
                        float fx = Math.Max(absleft, absright);
                        float abstop = Math.Abs(rect.Top);
                        float absbottom = Math.Abs(rect.Bottom);
                        float fy = Math.Max(abstop, absbottom);
                        Zoom = new Vec2F(hw / fx, hh / fy);
                        break;
                    }
                case OriginLockMode.Left:
                    {                        
                        if (rect.Right > 0)
                        {
                            float left = Math.Max(0, rect.Left);
                            float fx = (left > 0) ? rect.Width : rect.Right;
                            Zoom = new Vec2F(w / fx, h / rect.Height);
                            Vec2F center = new Vec2F(left + fx / 2, rect.Y + rect.Height / 2);
                            Pan = new Vec2F(hw - center.X * Zoom.X, hh + center.Y * Zoom.Y);
                        }                        
                        break;
                    }
                case OriginLockMode.LeftTop:

                    if (rect.Right > 0 && rect.Bottom > 0)
                    {
                        float left = Math.Max(0, rect.Left);
                        float fx = (left > 0) ? rect.Width : rect.Right;

                        float top = Math.Max(0, rect.Top);
                        float fy = (top > 0) ? rect.Height : rect.Bottom;
                        Zoom = new Vec2F(w / fx, h / fy);
                        Vec2F center = new Vec2F(left + fx / 2, top + fy / 2);
                        Pan = new Vec2F(hw - center.X * Zoom.X, hh + center.Y * Zoom.Y);
                    }             
                    break;
                case OriginLockMode.LeftBottom:
                    {
                        if (rect.Right > 0 && rect.Bottom > 0)
                        {
                            float left = Math.Max(0, rect.Left);
                            float fx = (left > 0) ? rect.Width : rect.Right;

                            float top = Math.Max(0, rect.Top);
                            float fy = (top > 0) ? rect.Height : rect.Bottom;
                            Zoom = new Vec2F(w / fx, h / fy);
                            Vec2F center = new Vec2F(left + fx / 2, top + fy / 2);
                            Pan = new Vec2F(hw - center.X * Zoom.X, hh + center.Y * Zoom.Y);
                        }                        
                        break;
                    }
                case OriginLockMode.LeftMiddle:
                    {
                        if (rect.Right > 0)
                        {
                            float left = Math.Max(0, rect.Left);
                            float fx = (left > 0) ? rect.Width : rect.Right;
                            float abstop = Math.Abs(rect.Top);
                            float absbottom = Math.Abs(rect.Bottom);
                            float fy = Math.Max(abstop, absbottom);
                            Zoom = new Vec2F(w / fx, hh / fy);
                            Vec2F center = new Vec2F(left + fx / 2, 0);
                            Pan = new Vec2F(hw - center.X * Zoom.X, Pan.Y);
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// Pans so the origin is at the center of the control</summary>
        /// <param name="invalidate">Whether to invalidate entire surface of control, causing it to be redrawn</param>
        public void PanToOrigin(bool invalidate = true)
        {
            switch(m_lockorg)
            {
                case OriginLockMode.Free:
                    Pan_d = new PointD(ClientSize.Width / 2, ClientSize.Height / 2);
                    break;
                case OriginLockMode.LeftTop:
                    Pan_d = new PointD(1, 1);
                    break;
                case OriginLockMode.Left:                    
                case OriginLockMode.LeftMiddle:
                    Pan_d = new PointD(1, ClientSize.Height / 2);
                    break;
                case OriginLockMode.LeftBottom:
                    Pan_d = new PointD(1, ClientSize.Height);
                    break;                    
            }     
            if(invalidate)
                Invalidate();
        }

        /// <summary>
        /// Zooms around a given point using an x scale and y scale</summary>
        /// <param name="at">Zoom about at</param>
        /// <param name="xs">X scale</param>
        /// <param name="ys">Y scale</param>
        public void SetZoom(Point at, float xs, float ys)
        {
            PointD zm = Zoom_d;
            PointD zoomCenterStart = new PointD(
                   (at.X - m_trans.X) / zm.X,
                   (at.Y - m_trans.Y) / zm.Y);
            Zoom_d = new PointD(zm.X * xs, zm.Y * ys);
            zm = Zoom_d;
            Pan_d = new PointD((at.X - zoomCenterStart.X * zm.X),
                (at.Y - zoomCenterStart.Y * zm.Y));
            Invalidate();
        }

        /// <summary>
        /// Sets x and z-coordinates to zoom to 1x. Pans so the origin is at the center of the control </summary>
        public void ResetTransform()
        {
            Zoom_d = new PointD(1, 1);
            PanToOrigin();            
            Invalidate();
        }

        /// <summary>
        /// Transforms point from graph space to client space</summary>
        /// <param name="p">Point to be transformed</param>
        /// <returns>Vec2F representing transformed point in client space</returns>
        public Vec2F GraphToClient(Vec2F p)
        {
            Vec2F result = new Vec2F();
            result.X = (float)(m_trans.X + p.X * m_scale.X);
            result.Y = (float)(m_trans.Y + p.Y * m_scale.Y);
            return result;
        }

        /// <summary>
        /// Transforms x and y-coordinates from graph space to client space</summary>        
        /// <param name="x">X-coordinate to be transformed</param>
        /// <param name="y">Y-coordinate to be transformed</param>
        /// <returns>Vec2F representing transformed x and y-coordinates in client space</returns>
        public Vec2F GraphToClient(double x, double y)
        {
            Vec2F result = new Vec2F();
            result.X = (float)(m_trans.X + x * m_scale.X);
            result.Y = (float)(m_trans.Y + y * m_scale.Y);
            return result;
        }

        /// <summary>
        /// Transforms x and y-coordinates from graph space to client space</summary>        
        /// <param name="x">X-coordinate to be transformed</param>
        /// <param name="y">Y-coordinate to be transformed</param>
        /// <returns>Vec2F representing transformed x and y-coordinates in client space</returns>
        public Vec2F GraphToClient(float x, float y)
        {
            Vec2F result = new Vec2F();
            result.X = (float)(m_trans.X + x * m_scale.X);
            result.Y = (float)(m_trans.Y + y * m_scale.Y);
            return result;
        }

        /// <summary>
        /// Transforms x-coordinate from graph space to client space</summary>        
        /// <param name="x">X-coordinate to be transformed</param>
        /// <returns>Transformed x-coordinate in client space</returns>
        public float GraphToClient(double x)
        {
            return (float)(m_trans.X + x * m_scale.X);
        }

        /// <summary>
        /// Transforms x-coordinate from graph space to client space</summary>        
        /// <param name="x">X-coordinate to be transformed</param>
        /// <returns>Transformed x-coordinate in client space</returns>
        public float GraphToClient(float x)
        {
            return (float)(m_trans.X + x * m_scale.X);
        }

        /// <summary>
        /// Transforms tangent from graph space to client space</summary>  
        /// <param name="tan">Tangent to transform</param>
        /// <returns>Transformed tangent in client space</returns>
        public Vec2F GraphToClientTangent(Vec2F tan)
        {
            return new Vec2F(tan.X * (float)m_scale.X, tan.Y * (float)m_scale.Y);
        }
        /// <summary>
        /// Transforms x and y-coordinates from client space to graph space</summary>      
        /// <param name="px">X-coordinate to convert</param>
        /// <param name="py">Y-coordinate to convert</param>
        /// <returns>Vec2F representing transformed point in graph space</returns>
        public Vec2F ClientToGraph(float px, float py)
        {
            Vec2F result = new Vec2F();
            result.X = (float)((px - m_trans.X) / m_scale.X);
            result.Y = (float)((py - m_trans.Y) / m_scale.Y);
            return result;
        }

        /// <summary>
        /// Transforms x and y-coordinates from client space to graph space</summary>        
        /// <param name="px">X-coordinate in client space</param>
        /// <param name="py">Y-coordinate in client space</param>
        /// <returns>Point in graph space</returns>
        public PointD ClientToGraph_d(double px, double py)
        {
            double gX = (px - m_trans.X) / m_scale.X;
            double gy = (py - m_trans.Y) / m_scale.Y;
            return new PointD(gX, gy);
        }

        /// <summary>
        /// Transforms x-coordinate from client space to graph space</summary>   
        /// <param name="px">X-coordinate in client space</param>
        /// <returns>X-coordinate in graph space</returns>
        public double ClientToGraph_d(double px)
        {
            return (px - m_trans.X) / m_scale.X;            
        }

        /// <summary>
        /// Transforms point from client space to graph space</summary>   
        /// <param name="p">Point to be transformed</param>
        /// <returns>Vec2F representing transformed point in graph space</returns>
        public Vec2F ClientToGraph(Vec2F p)
        {
            Vec2F result = new Vec2F();
            result.X = (float)((p.X - m_trans.X) / m_scale.X);
            result.Y = (float)((p.Y - m_trans.Y) / m_scale.Y);
            return result;
        }

        /// <summary>
        /// Transforms x-coordinate from client space to graph space</summary>   
        /// <param name="px">X-coordinate to be transformed</param>
        /// <returns>Transformed x-coordinate in graph space</returns>
        public float ClientToGraph(float px)
        {
            return (float)((px - m_trans.X) / m_scale.X);
        }

        /// <summary>
        /// Computes and returns transformation matrix.
        /// Used for transforming from graph space to client space</summary>
        /// <returns>Transformation matrix</returns>
        public virtual Matrix GetTransform()
        {
            m_xform.Reset();
            m_xform.Translate((float)m_trans.X, (float)m_trans.Y);
            m_xform.Scale((float)m_scale.X, (float)m_scale.Y);            
            return m_xform;
        }
       
        /// <summary>
        /// Draws horizontal scale and ticks</summary>
        /// <param name="g">Graphics object</param>
        public virtual void DrawHorizontalScale(Graphics g)
        {
            if (g == null)
                throw new ArgumentNullException("g");

            double xleft = ClientToGraph_d(0); // left graph rect.
            double xright = ClientToGraph_d(m_clientSize.X);  // right of graph rect.            
            double x = xleft - xleft % m_majorTickX;
            float height = (float)m_clientSize.Y;
            float heightWithMargin = height - GridTextMargin;            
            float lastTextEnd = -Width;
            for (; x <= xright; x += m_majorTickX)
            {                                
                // xform x from graph to client space.
                float xpos = GraphToClient(x);                
                string strVal = Math.Round(x, 9).ToString();
                SizeF sz = g.MeasureString(strVal, m_scaleTextFont);
                float hw = sz.Width * 0.5f;
                // check for overlap
                if (lastTextEnd < (xpos - hw))
                {
                    g.DrawString(strVal, m_scaleTextFont, m_scaleTextBrush, xpos - hw, heightWithMargin - sz.Height);
                    lastTextEnd = xpos + hw;
                }
                
            }
        }

        /// <summary>
        /// Draws vertical scale and ticks</summary>        
        /// <param name="g">Graphics object</param>
        public virtual void DrawVerticalScale(Graphics g)
        {
            if (g == null)
                throw new ArgumentNullException("g");
            float textHeight = g.MeasureString("+1289E", m_scaleTextFont).Height;

            double ytop = ClientToGraph_d(0, 0).Y;
            double ybottom = ClientToGraph_d(0, m_clientSize.Y).Y;
            if (m_flipY)
            {
                double tmp = ytop;
                ytop = ybottom;
                ybottom = tmp;
            }

            double y = ybottom - ybottom % m_majorTickY;            
            for (; y <= ytop; y += m_majorTickY)
            {
                float ypos = (float)(m_trans.Y + y * m_scale.Y);                
                string strVal = Math.Round(y, 9).ToString();                
                g.DrawString(strVal, m_scaleTextFont, m_scaleTextBrush, GridTextMargin, ypos - textHeight / 2);
            }                   
        }

        /// <summary>
        /// Draws horizontal grid lines with major ticks</summary>        
        /// <param name="g">Graphics object</param>
        public virtual void DrawHorizontalMajorTicks(Graphics g)
        {
            DrawHorizontalTicks(g, m_majorGridlinePen, m_majorTickY);                
        }

        /// <summary>
        /// Draws vertical grid lines with major ticks</summary>        
        /// <param name="g">Graphics object</param>
        public virtual void DrawVerticalMajorTicks(Graphics g)
        {
            DrawVerticalTicks(g, m_majorGridlinePen, m_majorTickX);            
        }

        /// <summary>
        /// Draws horizontal grid lines with minor ticks</summary>        
        /// <param name="g">Graphics object</param>
        public virtual void DrawHorizontalMinorTicks(Graphics g)
        {
            if (DrawMinorTickEnabled) 
                DrawHorizontalTicks(g, m_minorGridlinePen, m_majorTickY / m_numOfMinorTicks);            
        }


        /// <summary>
        /// Draws vertical grid lines with minor ticks</summary>        
        /// <param name="g">Graphics object</param>
        public virtual void DrawVerticalMinorTicks(Graphics g)
        {
            if (DrawMinorTickEnabled)
                DrawVerticalTicks(g, m_minorGridlinePen, m_majorTickX / m_numOfMinorTicks);
        }
        
        /// <summary>
        /// Draws x and y labels</summary>
        /// <param name="g">Graphics object</param>
        /// <param name="xLabel">X-axis label</param>
        /// <param name="yLabel">Y-axis label</param>        
        /// <param name="color">Optional color</param>        
        public virtual void DrawXYLabel(Graphics g, string xLabel, string yLabel, Color? color = null)
        {
            Color c = color != null ? color.Value : Color.Black;
            
            using (SolidBrush brush = new SolidBrush(c))
            {
                float margin = Math.Min( 2.0f * m_axisLabelFont.Height,40);
                float leadingspace = 120;
                g.DrawString(xLabel, m_axisLabelFont, brush, leadingspace, ClientSize.Height - margin);               
                Matrix xform = g.Transform;
                g.RotateTransform(-90);
                g.TranslateTransform(margin, ClientSize.Height - leadingspace, MatrixOrder.Append);
                g.DrawString(yLabel, m_axisLabelFont, brush, 0, 0);
                g.Transform = xform;
            }
        }

        /// <summary>
        /// Draws coordinate axes</summary>        
        /// <param name="g">Graphics object</param>
        public virtual void DrawCoordinateAxes(Graphics g)
        {
            if (g == null) 
                throw new ArgumentNullException("g");

            Vec2F center = GraphToClient(0, 0);
            float w = ClientSize.Width;
            float h = ClientSize.Height;
            if(center.Y > 0 && center.Y < h)
                g.DrawLine(m_axisPen, 0, center.Y, w, center.Y);
            if(center.X > 0 && center.X < w)
            g.DrawLine(m_axisPen, center.X, 0, center.X, h);                                       
        }

        /// <summary>
        /// Drawx cartesian grid</summary>        
        /// <param name="g">Graphics object</param>
        public virtual void DrawCartesianGrid(Graphics g)
        {
            if (g == null)
                throw new ArgumentNullException("g");

            //There is no need to apply expensive anti-aliasing for vertical and horizontal lines.
            //There is no need for transparency.
            g.SmoothingMode = SmoothingMode.None;
            g.CompositingMode = CompositingMode.SourceCopy;

            if (DrawMinorTickEnabled)
            {
                DrawHorizontalMinorTicks(g);
                DrawVerticalMinorTicks(g);
            }
            DrawVerticalMajorTicks(g);
            DrawHorizontalMajorTicks(g);
            DrawCoordinateAxes(g);

            g.CompositingMode = CompositingMode.SourceOver;
            DrawVerticalScale(g);
            DrawHorizontalScale(g);
            
        }

        /// <summary>
        /// Makes selection rectangle from two points</summary>
        /// <param name="p1">Point in first corner</param>
        /// <param name="p2">Point in second corner</param>
        /// <returns>Selection rectangle from two points</returns>
        protected RectangleF MakeRect(PointF p1, PointF p2)
        {
            PointF min = new PointF();
            min.X = Math.Min(p1.X, p2.X);
            min.Y = Math.Min(p1.Y, p2.Y);
            PointF max = new PointF();
            max.X = Math.Max(p1.X, p2.X);
            max.Y = Math.Max(p1.Y, p2.Y);
            return new RectangleF(min.X, min.Y, max.X - min.X, max.Y - min.Y);
        }
       
        /// <summary>
        /// Gets or sets y-axis flip</summary>
        public bool FlipY
        {
            get { return m_flipY; }
            set
            {
                m_flipY = value;
                m_scale.Y = (m_flipY) ? Math.Abs(m_scale.Y) : -Math.Abs(m_scale.Y);                
            }
        }
        private bool m_flipY;

        /// <summary>
        /// Gets or sets origin lock modes</summary>
        public OriginLockMode LockOrigin
        {
            get { return m_lockorg; }
            set
            {
                m_lockorg = value;
                UpdatePan();
                Invalidate();

            }
        }
        /// <summary>
        /// Gets or sets minimum zoom limit</summary>
        public double MinZoom
        {
            get { return m_minZoom; }
            set 
            {
                m_minZoom = Math.Min(value,m_maxZoom);
                Zoom_d = m_scale;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets maximum zoom limit</summary>
        public double MaxZoom
        {
            get { return m_maxZoom; }
            set 
            {
                m_maxZoom = Math.Max(value, m_minZoom);
                Zoom_d = m_scale;
                Invalidate();
            }
        }



        /// <summary>
        /// Gets or sets whether to draw minor ticks</summary>
        public bool DrawMinorTickEnabled
        {
            get { return m_drawMinorTicks; }
            set
            {
                m_drawMinorTicks = value;
                Invalidate();
            }
        }
        private bool m_drawMinorTicks;


        /// <summary>
        /// Gets or sets Brush used for drawing 
        /// number scale along the axes</summary>
        public Brush ScaleTextBrush
        {
            get { return m_scaleTextBrush; }
            set { SetDisposableVar(ref m_scaleTextBrush, value); }
        }
        private Brush m_scaleTextBrush = new SolidBrush(Color.Black);

        /// <summary>
        /// Gets or sets Font used for drawing
        /// number scale along the axes</summary>
        public Font ScaleTextFont
        {
            get { return m_scaleTextFont; }
            set { SetDisposableVar(ref m_scaleTextFont, value); }
        }
        private Font m_scaleTextFont;


        /// <summary>
        /// Gets or sets Pen used for drawing major gridlines</summary>
        public Pen MajorGridlinePen
        {
            get { return m_majorGridlinePen; }
            set { SetDisposableVar(ref m_majorGridlinePen, value); }
        }        
        private Pen m_majorGridlinePen = new Pen(Color.FromArgb(85, 85, 85));

        /// <summary>
        /// Gets or sets Pen used for drawing minor gridlines.</summary>
        public Pen MinorGridlinePen
        {
            get { return m_minorGridlinePen; }
            set { SetDisposableVar(ref m_minorGridlinePen, value); }
        }
        private Pen m_minorGridlinePen = new Pen(Color.FromArgb(90, 90, 90));

        /// <summary>
        /// Gets or sets Pen used for drawing the axes.</summary>
        public Pen AxisPen
        {
            get { return m_axisPen; }
            set { SetDisposableVar(ref m_axisPen, value); }
        }
        private Pen m_axisPen = new Pen(Color.FromArgb(200,200,200));

        /// <summary>
        /// Gets or sets Font used for drawing axis labels</summary>
        public Font AxisLabelFont
        {
            get { return m_axisLabelFont; }
            set { SetDisposableVar(ref m_axisLabelFont, value); }
        }
        private Font m_axisLabelFont = new Font("Trebuchet MS", 13.0f);
     
        /// <summary>
        /// Gets or sets color used to draw coordinate axes</summary>
        public Color AxisColor
        {
            get { return m_axisPen.Color; }
            set { m_axisPen.Color = value; }
        }

        /// <summary>
        /// Gets or sets color used for drawing grid</summary>
        public Color GridColor
        {
            get { return m_majorGridlinePen.Color; }
            set { m_majorGridlinePen.Color = value; }
        }

        /// <summary>
        /// Gets or sets color used for drawing scale numbers, x and y axis labels</summary>
        public Color TextColor
        {
            get 
            {
                SolidBrush sb = m_scaleTextBrush as SolidBrush;
                if (sb == null)
                    throw new InvalidOperationException("ScaleTextBrush is null or not a SolidBruhs");
                return sb.Color; 
            }
            set 
            {
                SolidBrush sb = m_scaleTextBrush as SolidBrush;
                if (sb == null)
                    throw new InvalidOperationException("ScaleTextBrush is null or not a SolidBruhs");
                sb.Color = value; 
            }
        }


        /// <summary>
        /// Gets or sets pan. Same as Pan property, but holds a double instead of a float</summary>
        public PointD Pan_d
        {
            get { return m_trans; }
            set
            {
                double x = Math.Max(m_minOffsetX, Math.Min(m_maxOffsetX, value.X));
                double y = Math.Max(m_minOffsetY, Math.Min(m_maxOffsetY, value.Y));
                m_trans = new PointD(x, y);
            }
        }


        /// <summary>
        /// Gets or sets pan</summary>
        public Vec2F Pan
        {
            get { return (Vec2F)m_trans; }
            set
            {
                double x = Math.Max(m_minOffsetX, Math.Min(m_maxOffsetX, value.X));
                double y = Math.Max(m_minOffsetY, Math.Min(m_maxOffsetY, value.Y));
                m_trans = new PointD(x, y);
            }
        }


        /// <summary>
        /// Gets or sets zoom. Same as Zoom property, but holds a double instead of a float</summary>
        public PointD Zoom_d
        {
            get 
            {
                PointD result = new PointD();
                result.X = m_scale.X;
                result.Y = Math.Abs(m_scale.Y);
                return result; 
            }
            set
            {
                double x = Math.Max(m_minZoom, Math.Min(m_maxZoom, value.X));
                double y = Math.Max(m_minZoom, Math.Min(m_maxZoom, value.Y));
                m_scale = m_flipY ? new PointD(x, y) : new PointD(x, -y);
                // recompute grid span along x and y 
                m_majorTickX = ComputeGridSpan(m_majorTickSpacing / x);
                m_majorTickY = ComputeGridSpan(m_majorTickSpacing / y);
            }
        }

        /// <summary>
        /// Gets or sets zoom or scale</summary>
        public Vec2F Zoom
        {
            get 
            {
                Vec2F result = new Vec2F();
                result.X = (float)m_scale.X;
                result.Y = (float)Math.Abs(m_scale.Y);
                return result;                
            }
            set
            {
                double x = Math.Max(m_minZoom, Math.Min(m_maxZoom, value.X));
                double y = Math.Max(m_minZoom, Math.Min(m_maxZoom, value.Y));
                m_scale = m_flipY ? new PointD(x, y) : new PointD(x, -y);

                // recompute grid span along x and y 
                m_majorTickX = ComputeGridSpan(m_majorTickSpacing / x);
                m_majorTickY = ComputeGridSpan(m_majorTickSpacing / y);
            }
        }


        /// <summary>
        /// Gets major tick along x in graph space</summary>
        public float MajorTickX
        {
            get { return (float)m_majorTickX; }
        }
        
        /// <summary>
        /// Gets major tick along y in graph space</summary>
        public float MajorTickY
        {
            get { return (float)m_majorTickY; }            
        }

        /// <summary>
        /// Gets minor tick along x in graph space</summary>
        public float MinorTickX
        {
            get { return (float)(m_majorTickX / m_numOfMinorTicks); }
        }

        /// <summary>
        /// Gets minor tick along y in graph space</summary>
        public float MinorTickY
        {
            get { return (float)(m_majorTickY / m_numOfMinorTicks); }
        }

        /// <summary>
        /// Gets or sets major tick spacing in pixel</summary>
        public float MajorTickSpacing
        {
            get {  return (float)m_majorTickSpacing; }
            set 
            {
                if (value < 10)
                    throw new ArgumentOutOfRangeException("value");
                m_majorTickSpacing = value; 
            }
        }

        /// <summary>
        /// Gets or sets number of minor ticks</summary>
        public float NumOfMinorTicks
        {
            get { return (float)m_numOfMinorTicks; }
            set 
            {
                if (value >= m_majorTickSpacing)
                    throw new ArgumentOutOfRangeException("value");
                m_numOfMinorTicks = value; 
            }
        }

        /// <summary>
        /// Performs custom actions on MouseLeave event. Resets some internal variable on mouse leave.</summary>
        /// <param name="e">Event args</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            CurrentPoint = new Vec2F(-1, -1);
            PreviousPoint = CurrentPoint;
            CurrentGraphPoint = ClientToGraph(CurrentPoint);
            PreviousGraphPoint = CurrentGraphPoint;
        }
        /// <summary>
        /// Performs custom actions on MouseDown event. Sets data for performing pan and zoom.</summary>        
        /// <param name="e">Event args</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {            
            Focus();
            Capture = true;                       
            ClickPoint = new Vec2F(e.X, e.Y);            
            ClickGraphPoint = ClientToGraph(ClickPoint);
            PreviousPoint = ClickPoint;
            PreviousGraphPoint = ClickGraphPoint;
            CurrentPoint = ClickPoint;
            CurrentGraphPoint = ClickGraphPoint;           
            ClickPan_d = Pan_d;
            ClickZoom_d = Zoom_d;            
            m_startDrag = true;
            SelectionRect.Location = ClickPoint;
            base.OnMouseDown(e);
        }

        /// <summary>
        /// Performs custom actions on MouseMove event. Performs pan and zoom depending on m_editAction field.</summary>        
        /// <param name="e">Event args</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            PreviousPoint = CurrentPoint;
            PreviousGraphPoint = CurrentGraphPoint;

            CurrentPoint = new Vec2F(e.X, e.Y);
            CurrentGraphPoint = ClientToGraph(CurrentPoint);

            float dx = CurrentPoint.X - ClickPoint.X;
            float dy = CurrentPoint.Y - ClickPoint.Y;

            if (m_startDrag && !m_dragOverThreshold
                && (Math.Abs(dx) > m_dragThreshold || Math.Abs(dy) > m_dragThreshold))
            {
                m_dragOverThreshold = true;
            }
         
            base.OnMouseMove(e);
        }

        /// <summary>
        /// Performs custom actions on MouseUp event. Resets a few members.</summary>        
        /// <param name="e">Event args</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {                        
            SelectionRect = RectangleF.Empty;
            m_startDrag = false;
            m_dragOverThreshold = false;
            base.OnMouseUp(e);
            Capture = false;
        }       

        /// <summary>
        /// Performs custom actions on MouseWheel event. Zooms uniformly.</summary>        
        /// <param name="e">Event args</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (Control.MouseButtons == MouseButtons.None)
            {
                float zf = (1.0f + e.Delta / 1200.0f);
                SetZoom(e.Location, zf, zf);
            }
        }

        /// <summary>
        /// Performs custom actions on Resize event. Sets m_clientSize field.</summary>        
        /// <param name="e">Event args</param>
        protected override void OnResize(EventArgs e)
        {           
            base.OnResize(e);
            m_clientSize.X = ClientSize.Width;
            m_clientSize.Y = ClientSize.Height;
            UpdatePan();
           
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.Control" /> and its child controls and 
        /// optionally releases the managed resources.</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // The setters will dispose and set to null
                ScaleTextBrush = null;
                ScaleTextFont = null;
                MajorGridlinePen = null;
                MinorGridlinePen = null;
                AxisPen = null;
                AxisLabelFont = null;
            }
            base.Dispose(disposing);
        }
        private void SetDisposableVar<T>(ref T mvar, T value) where T : class , IDisposable
        {
            if (mvar != null) mvar.Dispose();
            mvar = value;
        }

        private void DrawVerticalTicks(Graphics g, Pen p, double tickLength)
        {
            if (g == null)
                throw new ArgumentNullException("g");
            
            double xleft = ClientToGraph_d(0); // left graph rect.
            double xright = ClientToGraph_d(m_clientSize.X); // right of graph rect.            
            double x = xleft - xleft % tickLength;
            float height = (float)m_clientSize.Y;
            for (; x <= xright; x += tickLength)
            {
                // xform x from graph to client space.
                float xpos = (float)(m_trans.X + x * m_scale.X);
                g.DrawLine(p, xpos, 0, xpos, height);
            }

        }
        /// <summary>
        /// Draw horizontal ticks.</summary>        
        private void DrawHorizontalTicks(Graphics g, Pen p, double tickLength)
        {
             if (g == null)
                throw new ArgumentNullException("g");            
            double ytop = ClientToGraph_d(0, 0).Y;
            double ybottom = ClientToGraph_d(0, m_clientSize.Y).Y;
            if (m_flipY)
            {
                double tmp = ytop;
                ytop = ybottom;
                ybottom = tmp;
            }

            double y = ybottom - ybottom % tickLength;
            float width = (float)m_clientSize.X;
            for (; y <= ytop; y += tickLength)
            {
                float ypos = (float)(m_trans.Y + y * m_scale.Y);
                g.DrawLine(p, 0, ypos, width, ypos);
            }
        }
        /// <summary>
        /// Updates pan against current origin lock mode</summary>
        private void UpdatePan()
        {
            double w = ClientSize.Width;
            double h = ClientSize.Height;
            
            switch (m_lockorg)
            {
                case OriginLockMode.Free:
                    m_minOffsetX = float.MinValue;
                    m_maxOffsetX = float.MaxValue;
                    m_minOffsetY = float.MinValue;                    
                    m_maxOffsetY = float.MaxValue;
                    break;
                case OriginLockMode.Center:
                    m_minOffsetX = w / 2;                    
                    m_maxOffsetX = w / 2;
                    m_minOffsetY = h / 2;
                    m_maxOffsetY = h / 2;
                    break;
                case OriginLockMode.Left:
                    m_minOffsetX = float.MinValue;
                    m_maxOffsetX = 1;
                    m_minOffsetY = float.MinValue;                    
                    m_maxOffsetY = float.MaxValue;
                    break;
                case OriginLockMode.LeftTop:
                    m_minOffsetX = float.MinValue;
                    m_maxOffsetX = 1;
                    m_minOffsetY = -float.MaxValue;
                    m_maxOffsetY = 1;
                    break;
                case OriginLockMode.LeftMiddle:
                    m_minOffsetX = float.MinValue;
                    m_maxOffsetX = 1;
                    m_minOffsetY = h / 2;
                    m_maxOffsetY = h / 2;
                    break;
                case OriginLockMode.LeftBottom:
                    m_minOffsetX = float.MinValue;
                    m_maxOffsetX = 1;
                    m_minOffsetY = h - 1;
                    m_maxOffsetY = float.MaxValue;
                    break;
            }
            if (m_lockorg != OriginLockMode.Free)
                Pan_d = m_trans;
            
        }
        private static readonly double[] s_spanTable = { 0.1, 0.2, 0.5, 1.0, 2.0, 4.0, 5.0};
        /// <summary>
        /// Method used for calculating grid span.
        /// There could be a better or more efficient way.</summary>        
        private static double ComputeGridSpan(double attendedSpan)
        {
            double digit = Math.Truncate(Math.Log10(attendedSpan));
            double baseNum = Math.Pow(10, digit);
            
            double minSpan = 0;
            double min = Double.MaxValue;
            for (int i = 0; i < s_spanTable.Length; ++i)
            {
                double span = baseNum * s_spanTable[i];
                double dis = Math.Abs(span - attendedSpan);

                if (dis < min)
                {
                    min = dis;
                    minSpan = span;
                }
            }            
            return minSpan;
        }

        /// <summary>Client space point on mouse down</summary>
        protected Vec2F ClickPoint;

        /// <summary>Previous point in client space</summary>
        protected Vec2F PreviousPoint;

        /// <summary>Current point in client space</summary>        
        protected Vec2F CurrentPoint;

        /// <summary>Graph space point on mouse down</summary>
        protected Vec2F ClickGraphPoint;

        /// <summary>Previous point in graph space</summary>        
        protected Vec2F PreviousGraphPoint;

        /// <summary>Current point in graph space</summary>        
        protected Vec2F CurrentGraphPoint;

        /// <summary>Pan value on mouse down</summary>
        protected PointD ClickPan_d;

        /// <summary>Zoom value on mouse down</summary>
        protected PointD ClickZoom_d;

        /// <summary>Selection rectangle</summary>
        protected RectangleF SelectionRect;

        /// <summary>
        /// Gets whether dragging over threshold. True if user dragging.</summary>
        protected bool DraggingOverThreshold
        {
            get { return m_dragOverThreshold; }
        }
       
        private bool m_startDrag;
        private float m_dragThreshold = 3.0f;
        private bool m_dragOverThreshold;        
        private PointD m_clientSize;
        private PointD m_trans;
        private PointD m_scale = new PointD(1, 1);
        private readonly Matrix m_xform = new Matrix();
        private double m_minOffsetX = float.MinValue;
        private double m_maxOffsetX = float.MaxValue;

        private double m_minOffsetY = float.MinValue;
        private double m_maxOffsetY = float.MaxValue;
        
        private double m_minZoom = 1e-4;        
        private double m_maxZoom = 1e5;
                        
        // grid related members.        
        private double m_numOfMinorTicks = 5;                
        private double m_majorTickSpacing = 50.0f;        
        public  const float  GridTextMargin = 3.0f;
        private double m_majorTickY = 60;
        private double m_majorTickX = 60;               
        private OriginLockMode m_lockorg = OriginLockMode.Free;
    }

    /// <summary>
    /// Origin lock modes</summary>
    public enum OriginLockMode
    {
        /// <summary>No lock</summary>
        Free,

        /// <summary>Lock origin to center</summary>
        Center,

        /// <summary>Lock to positive x axis</summary>                
        Left,

        /// <summary>Lock to left top</summary>
        LeftTop,

        /// <summary>Lock to left middle</summary>
        LeftMiddle,

        /// <summary>Lock to left bottom</summary>
        LeftBottom,
    }

    
    /// <summary>
    /// Double 2D point. Used by canvas 2D.</summary>
    public struct PointD
    {
        /// <summary>
        /// Constructor</summary>        
        /// <param name="x">X-coordinate</param>
        /// <param name="y">Y-coordinate</param>
        public PointD(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Auto converts vec2F to PointD</summary> 
        /// <param name="v">Vec2F to convert to PointD</param>
        /// <returns>Converted PointD</returns>
        public static implicit operator PointD(Vec2F v)
        {
            PointD p = new PointD();
            p.X = v.X;
            p.Y = v.Y;
            return p;
        }

        /// <summary>
        /// Auto converts PointF to PointD</summary>     
        /// <param name="v">PointF to convert to PointD</param>
        /// <returns>Converted PointD</returns>
        public static implicit operator PointD(PointF v)
        {
            PointD p = new PointD();
            p.X = v.X;
            p.Y = v.Y;
            return p;
        }

        /// <summary>
        /// Converts PointD to Vec2F. Data loss may occur.</summary>   
        /// <param name="p">PointD to convert to Vec2F</param>
        /// <returns>Converted Vec2F</returns>
        public static explicit operator Vec2F(PointD p)
        {
            Vec2F t = new Vec2F();
            t.X = (float)p.X;
            t.Y = (float)p.Y;
            return t;
        }

        /// <summary>
        /// Convert PointD to PointF. Data loss may occur.</summary>     
        /// <param name="p">PointD to convert to PointF</param>
        /// <returns>Converted PointF</returns>
        public static explicit operator PointF(PointD p)
        {
            PointF t = new PointF();
            t.X = (float)p.X;
            t.Y = (float)p.Y;
            return t;           
        }

        /// <summary>X-coordinate</summary>
        public double X;

        /// <summary>Y-coordinate</summary>
        public double Y;       
    }
}
