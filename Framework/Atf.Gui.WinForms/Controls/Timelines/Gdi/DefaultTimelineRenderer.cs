//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace Sce.Atf.Controls.Timelines
{
    /// <summary>
    /// OBSOLETE. Please use the much faster D2dDefaultTimelineRenderer in the Sce.Atf.Controls.Timelines.Direct2D namespace.
    /// Default timeline renderer</summary>
    /// <remarks>In the next release of ATF, we are planning on marking this with the Obsolete attribute</remarks>
    public class DefaultTimelineRenderer : TimelineRenderer, IDisposable
    {
        /// <summary>
        /// Constructor</summary>
        public DefaultTimelineRenderer()
            : this(SystemFonts.StatusFont)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="font">Font to use for rendering timeline text</param>
        public DefaultTimelineRenderer(Font font)
            : base(font)
        {
            m_selectedPen = new Pen(Color.Tomato, 3);
            Color lightGray = Color.LightGray;
            m_collapsedBrush = new SolidBrush(lightGray);
            m_invalidBrush = new HatchBrush(HatchStyle.ForwardDiagonal, Color.DimGray, Color.FromArgb(0, 0, 0, 0));
        }

        /// <summary>
        /// Gets and sets the track height, relative to a unit of time</summary>
        [DefaultValue(1)]
        public float TrackHeight
        {
            get { return m_trackHeight; }
            set
            {
                if (m_trackHeight != value)
                {
                    m_trackHeight = value;
                    OnInvalidated(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets and sets the size of keys, in pixels</summary>
        [DefaultValue(12)]
        public int KeySize
        {
            get { return m_keySize; }
            set
            {
                if (m_keySize != value)
                {
                    m_keySize = value;
                    OnInvalidated(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The minimum visible interval length, in pixels. If the actual length of an interval is so
        /// short that, with the current zoom settings, the visible width would be less than
        /// MinimumDrawnIntervalLength pixels wide, then a 'tail' will be drawn to help with visualization
        /// and selection.</summary>
        [DefaultValue(28)]
        public int MinimumDrawnIntervalLength
        {
            get { return m_minimumDrawnIntervalLength; }
            set { m_minimumDrawnIntervalLength = value; }
        }

        /// <summary>
        /// Disposes unmanaged resources</summary>
        /// <param name="disposing">Whether or not Dispose invoked this method</param>
        protected override void Dispose(bool disposing)
        {
            m_selectedPen.Dispose();
            m_collapsedBrush.Dispose();
            m_invalidBrush.Dispose();

            base.Dispose(disposing);
        }

        /// <summary>
        /// Draws a group</summary>
        /// <param name="group">Group</param>
        /// <param name="bounds">Bounding rectangle, computed during layout phase</param>
        /// <param name="drawMode">Drawing mode</param>
        /// <param name="c">Drawing context</param>
        protected override void Draw(IGroup group, RectangleF bounds, DrawMode drawMode, Context c)
        {
            switch (drawMode & DrawMode.States)
            {
                case DrawMode.Normal:
                case DrawMode.Collapsed:
                    using (Brush brush = new LinearGradientBrush(
                        bounds, Color.LightGoldenrodYellow, Color.Khaki, LinearGradientMode.Vertical))
                    {
                        c.Graphics.FillRectangle(brush, bounds);
                    }
                    break;
                case DrawMode.Ghost:
                    using (Brush brush = new SolidBrush(Color.FromArgb(128, Color.Gray)))
                    {
                        c.Graphics.FillRectangle(brush, bounds);
                    }
                    break;
            }
        }

        /// <summary>
        /// Draws a track</summary>
        /// <param name="track">Track</param>
        /// <param name="bounds">Bounding rectangle, in screen space</param>
        /// <param name="drawMode">Drawing mode</param>
        /// <param name="c">Drawing context</param>
        protected override void Draw(ITrack track, RectangleF bounds, DrawMode drawMode, Context c)
        {
            switch (drawMode & DrawMode.States)
            {
                case DrawMode.Normal:
                    c.Graphics.DrawRectangle(Pens.LightGray, bounds.X, bounds.Y, bounds.Width, bounds.Height);
                    break;
                case DrawMode.Collapsed:
                    break;
                case DrawMode.Ghost:
                    using (Brush brush = new SolidBrush(Color.FromArgb(128, Color.Gray)))
                    {
                        c.Graphics.FillRectangle(brush, bounds);
                    }
                    break;
            }
        }

        /// <summary>
        /// Draws an interval</summary>
        /// <param name="interval">Interval</param>
        /// <param name="bounds">Bounding rectangle, in screen space</param>
        /// <param name="drawMode">Drawing mode</param>
        /// <param name="c">Drawing context</param>
        protected override void Draw(IInterval interval, RectangleF bounds, DrawMode drawMode, Context c)
        {
            Color color = interval.Color;
            switch (drawMode & DrawMode.States)
            {
                case DrawMode.Normal:
                    RectangleF realPart = new RectangleF(
                        bounds.X,
                        bounds.Y,
                        GdiUtil.TransformVector(c.Transform, interval.Length),
                        bounds.Height);
                    bool hasTail = realPart.Width < MinimumDrawnIntervalLength;
                    
                    float h = color.GetHue();
                    float s = color.GetSaturation();
                    float b = color.GetBrightness();
                    Color endColor = ColorUtil.FromAhsb(color.A, h, s * 0.3f, b);

                    using (LinearGradientBrush intervalBrush =
                        new LinearGradientBrush(realPart, color, endColor, LinearGradientMode.Vertical))
                    {
                        c.Graphics.FillRectangle(intervalBrush, realPart);
                        if (hasTail)
                        {
                            Color[] colors = intervalBrush.LinearColors;
                            colors[0] = Color.FromArgb(64, colors[0]);
                            colors[1] = Color.FromArgb(64, colors[1]);
                            intervalBrush.LinearColors = colors;
                            RectangleF tailPart = new RectangleF(
                                realPart.Right,
                                bounds.Y,
                                bounds.Width - realPart.Width,
                                bounds.Height);
                            c.Graphics.FillRectangle(intervalBrush, tailPart);
                        }
                    }

                    Brush textBrush = SystemBrushes.WindowText;
                    if ((int)color.R + (int)color.G + (int)color.B < 3 * 160)
                        textBrush = SystemBrushes.HighlightText;
                    c.Graphics.DrawString(interval.Name, c.Font, textBrush, bounds.Location);

                    if ((drawMode & DrawMode.Selected) != 0)
                    {
                        c.Graphics.DrawRectangle(m_selectedPen, bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2);
                    }
                    break;
                case DrawMode.Collapsed:
                    c.Graphics.FillRectangle(m_collapsedBrush, bounds);
                    break;
                case DrawMode.Ghost:
                    using (Brush brush = new SolidBrush(Color.FromArgb(128, color)))
                    {
                        c.Graphics.FillRectangle(brush, bounds);
                        bool showRight = (drawMode & DrawMode.ResizeRight) != 0;
                        float x = showRight ? bounds.Right : bounds.Left;
                        c.Graphics.DrawString(GetXPositionString(x, c), c.Font, SystemBrushes.WindowText, x, bounds.Bottom - c.FontHeight);
                    }
                    break;
                case DrawMode.Invalid:
                    c.Graphics.FillRectangle(m_invalidBrush, bounds);
                    break;
            }
        }

        /// <summary>
        /// Draws a key</summary>
        /// <param name="key">Key</param>
        /// <param name="bounds">Bounding rectangle, computed during layout phase</param>
        /// <param name="drawMode">Drawing mode</param>
        /// <param name="c">Drawing context</param>
        protected override void Draw(IKey key, RectangleF bounds, DrawMode drawMode, Context c)
        {
            Color color = key.Color;
            bounds.Width = bounds.Height = m_keySize; // key is always square, fixed size

            switch (drawMode & DrawMode.States)
            {
                case DrawMode.Normal:
                    using (SolidBrush brush = new SolidBrush(color))
                    {
                        c.Graphics.FillEllipse(brush, bounds);
                    }

                    if ((drawMode & DrawMode.Selected) != 0)
                    {
                        c.Graphics.DrawEllipse(m_selectedPen, bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2);
                    }
                    break;
                case DrawMode.Collapsed:
                    c.Graphics.FillEllipse(m_collapsedBrush, bounds);
                    break;
                case DrawMode.Ghost:
                    using (Brush brush = new SolidBrush(Color.FromArgb(128, color)))
                    {
                        c.Graphics.FillEllipse(brush, bounds);
                        c.Graphics.DrawString(GetXPositionString(bounds.Left + m_keySize / 2, c), c.Font, SystemBrushes.WindowText, bounds.Right + 16, bounds.Y);
                    }
                    break;
                case DrawMode.Invalid:
                    c.Graphics.FillEllipse(m_invalidBrush, bounds);
                    break;
            }
        }

        /// <summary>
        /// Draws a marker</summary>
        /// <param name="marker">Marker</param>
        /// <param name="bounds">Bounding rectangle, computed during layout phase</param>
        /// <param name="drawMode">Drawing mode</param>
        /// <param name="c">Drawing context</param>
        protected override void Draw(IMarker marker, RectangleF bounds, DrawMode drawMode, Context c)
        {
            float middle = bounds.X + bounds.Width / 2;
            Color color = marker.Color;

            switch (drawMode & DrawMode.States)
            {
                case DrawMode.Normal:
                    using (Pen pen = new Pen(color))
                    {
                        c.Graphics.DrawLine(pen, middle, bounds.Top, middle, bounds.Bottom);
                    }

                    bool selected = (drawMode & DrawMode.Selected) != 0;
                    Color handleColor = selected ? Color.Tomato : color;
                    using (Brush brush = new SolidBrush(handleColor))
                    {
                        RectangleF handleRect =
                            new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Width);
                        c.Graphics.FillRectangle(brush, handleRect);
                    }
                    break;
                case DrawMode.Collapsed:
                    c.Graphics.FillRectangle(m_collapsedBrush, middle, bounds.Y, 1, bounds.Height);
                    break;
                case DrawMode.Ghost:
                    using (Pen pen = new Pen(Color.FromArgb(128, color)))
                    {
                        c.Graphics.DrawLine(pen, middle, bounds.Top, middle, bounds.Bottom);
                        c.Graphics.DrawString(GetXPositionString(middle, c), c.Font, SystemBrushes.WindowText, bounds.Right + 16, bounds.Y);
                    }
                    break;
                case DrawMode.Invalid:
                    c.Graphics.DrawRectangle(Pens.DimGray, bounds.X, bounds.Y, bounds.Width, bounds.Height);
                    break;
            }
        }

        /// <summary>
        /// Finds hits on a marker, given a picking rectangle</summary>
        /// <param name="marker">Marker</param>
        /// <param name="bounds">Bounding rectangle, computed during layout phase</param>
        /// <param name="pickRect">Picking rectangle</param>
        /// <param name="c">Drawing context</param>
        /// <returns>Type of hit</returns>
        protected override HitType Pick(IMarker marker, RectangleF bounds, RectangleF pickRect, Context c)
        {
            RectangleF handleRect =
                new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Width);
            return handleRect.IntersectsWith(pickRect) ? HitType.Marker : HitType.None;
        }

        /// <summary>
        /// Gets the bounding rectangle for an interval, in timeline coordinates</summary>
        /// <param name="interval">Interval</param>
        /// <param name="trackTop">Top of track holding interval</param>
        /// <param name="c">Drawing context</param>
        /// <returns>Bounding rectangle for the interval, in timeline coordinates</returns>
        protected override RectangleF GetBounds(IInterval interval, float trackTop, Context c)
        {
            // Calculate the width, in timeline coordinates. If the group is expanded, then
            //  make sure that interval meets the minimum visible width requirement.
            float visibleWidth = interval.Length;
            if (interval.Track != null &&
                interval.Track.Group != null &&
                interval.Track.Group.Expanded)
            {
                float minimumTimelineUnits = c.PixelSize.Width * MinimumDrawnIntervalLength;
                visibleWidth = Math.Max(visibleWidth, minimumTimelineUnits);
            }
            
            return new RectangleF(
                interval.Start,
                trackTop,
                visibleWidth,
                TrackHeight);
        }

        /// <summary>
        /// Gets the bounding rectangle for a key, in timeline coordinates</summary>
        /// <param name="key">Key</param>
        /// <param name="trackTop">Top of track holding key</param>
        /// <param name="c">Drawing context</param>
        /// <returns>Bounding rectangle for the key, in timeline coordinates</returns>
        protected override RectangleF GetBounds(IKey key, float trackTop, Context c)
        {
            float keyWidth = c.PixelSize.Width * KeySize;
            float keyHeight = c.PixelSize.Height * KeySize;

            return new RectangleF(
                key.Start - keyWidth / 2,
                trackTop,
                keyWidth,
                keyHeight);
        }

        /// <summary>
        /// Gets the bounding rectangle for a marker, in timeline coordinates</summary>
        /// <param name="marker">Marker</param>
        /// <param name="c">Drawing context</param>
        /// <returns>Bounding rectangle for the marker, in timeline coordinates</returns>
        protected override RectangleF GetBounds(IMarker marker, Context c)
        {
            const float DefaultMarkerHandleSize = 10;
            float handleSize = c.PixelSize.Width * DefaultMarkerHandleSize;

            return new RectangleF(
                marker.Start - handleSize / 2,
                c.Bounds.Top,
                handleSize,
                c.Bounds.Height);
        }

        /// <summary>
        /// Gets the string used to display a timeline event's time or x-position in world coordinates</summary>
        /// <param name="x">The position in canvas coordinates. The Context's InverseTransform can convert it to timeline coordinates.</param>
        /// <param name="c">Drawing context</param>
        /// <returns>A string for displaying a time (world x-coordinate) to the user</returns>
        protected virtual string GetXPositionString(float x, Context c)
        {
            float frame = x * c.InverseTransform.Elements[0] + c.InverseTransform.Elements[4];
            frame = (float)Math.Round(frame);
            return frame.ToString(CultureInfo.CurrentCulture);
        }

        private readonly Pen m_selectedPen;
        private readonly Brush m_collapsedBrush;
        private readonly Brush m_invalidBrush;
        private float m_trackHeight = 1;
        private int m_keySize = 12;
        private int m_minimumDrawnIntervalLength = 28;

        static DefaultTimelineRenderer()
        {
            s_leftBottomFormat = new StringFormat();
            s_leftBottomFormat.Alignment = StringAlignment.Near;
            s_leftBottomFormat.LineAlignment = StringAlignment.Far;
        }

        private static readonly StringFormat s_leftBottomFormat;
    }
}
