//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using Sce.Atf.Direct2D;

namespace Sce.Atf.Controls.Timelines.Direct2D
{
    /// <summary>
    /// Default timeline renderer. This class is designed to be instantiated
    /// once per TimelineControl.</summary>
    public class D2dDefaultTimelineRenderer : D2dTimelineRenderer, IDisposable
    {
        /// <summary>
        /// Constructor</summary>
        public D2dDefaultTimelineRenderer()
            : this(SystemFonts.StatusFont)
        {
        }

        /// <summary>
        /// Constructor with font</summary>
        /// <param name="font">Font to use for rendering timeline text</param>
        public D2dDefaultTimelineRenderer(Font font)
            : base(font)
        {
        }

        /// <summary>
        /// Initializes class with graphics object</summary>
        /// <param name="graphics">Graphics object for drawing</param>
        public override void Init(D2dGraphics graphics)
        {
            base.Init(graphics);

            SelectedBrush = graphics.CreateSolidBrush(Color.Tomato);//should have width of 3
            CollapsedBrush = graphics.CreateSolidBrush(Color.LightGray);
            InvalidBrush = graphics.CreateSolidBrush(Color.DimGray);
            GroupBrush = graphics.CreateLinearGradientBrush(
                new D2dGradientStop(Color.LightGoldenrodYellow, 0),
                new D2dGradientStop(Color.Khaki, 1));
            GhostGroupBrush = graphics.CreateSolidBrush(Color.FromArgb(128, Color.Gray));
            TrackBrush = graphics.CreateSolidBrush(Color.LightGray);
            GhostTrackBrush = graphics.CreateSolidBrush(Color.FromArgb(128, Color.Gray));
            TextBrush = graphics.CreateSolidBrush(SystemColors.WindowText);
        }

        /// <summary>
        /// Gets and sets the track height, relative to a unit of time</summary>
        [DefaultValue(1)]
        public virtual float TrackHeight
        {
            get { return GlobalTrackHeight; }
            set
            {
                if (GlobalTrackHeight != value)
                {
                    GlobalTrackHeight = value;
                    OnInvalidated(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets and sets the size of keys, in pixels</summary>
        [DefaultValue(12)]
        public virtual int KeySize
        {
            get { return GlobalKeySize; }
            set
            {
                if (GlobalKeySize != value)
                {
                    GlobalKeySize = value;
                    OnInvalidated(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets and sets the minimum visible interval length, in pixels. If the actual length of an interval is so
        /// short that, with the current zoom settings, the visible width would be less than
        /// MinimumDrawnIntervalLength pixels wide, then a 'tail' is drawn to help with visualization
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
            if (!m_disposed)
            {
                if (disposing)
                {
                    SelectedBrush.Dispose();
                    CollapsedBrush.Dispose();
                    InvalidBrush.Dispose();
                    GroupBrush.Dispose();
                    GhostGroupBrush.Dispose();
                    TrackBrush.Dispose();
                    GhostTrackBrush.Dispose();
                    TextBrush.Dispose();
                }

                // dispose any unmanaged resources.

                m_disposed = true;
            }
            // always call base regardles.
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
                    GroupBrush.StartPoint = new PointF(0, bounds.Top);
                    GroupBrush.EndPoint = new PointF(0, bounds.Bottom);
                    c.Graphics.FillRectangle(bounds, GroupBrush);
                    break;
                case DrawMode.Ghost:
                    c.Graphics.FillRectangle(bounds, GhostGroupBrush);
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
                    c.Graphics.DrawRectangle(bounds, TrackBrush);
                    break;
                case DrawMode.Collapsed:
                    break;
                case DrawMode.Ghost:
                    c.Graphics.FillRectangle(bounds, GhostTrackBrush);
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
                    c.Graphics.FillRectangle(
                        realPart,
                        new PointF(0, realPart.Top),new PointF(0, realPart.Bottom),
                        color, endColor);

                    if (hasTail)
                    {
                        endColor = ColorUtil.FromAhsb(64, h, s * 0.3f, b);
                        RectangleF tailPart = new RectangleF(
                            realPart.Right,
                            bounds.Y,
                            bounds.Width - realPart.Width,
                            bounds.Height);
                        c.Graphics.FillRectangle(tailPart, endColor);
                    }

                    if (color.R + color.G + color.B < 3 * 160)
                        TextBrush.Color = SystemColors.HighlightText;
                    else
                        TextBrush.Color = SystemColors.WindowText;

                    c.Graphics.DrawText(interval.Name, c.TextFormat, bounds.Location, TextBrush);

                    if ((drawMode & DrawMode.Selected) != 0)
                    {
                        c.Graphics.DrawRectangle(
                            new RectangleF(bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2),
                            SelectedBrush, 3.0f);
                    }
                    break;
                case DrawMode.Collapsed:
                    c.Graphics.FillRectangle(bounds, CollapsedBrush);
                    break;
                case DrawMode.Ghost:
                    c.Graphics.FillRectangle(bounds, Color.FromArgb(128, color));
                    bool showRight = (drawMode & DrawMode.ResizeRight) != 0;
                    float x = showRight ? bounds.Right : bounds.Left;
                    c.Graphics.DrawText(
                        GetXPositionString(x, c),
                        c.TextFormat,
                        new PointF(x, bounds.Bottom - c.FontHeight),
                        TextBrush);
                    break;
                case DrawMode.Invalid:
                    c.Graphics.FillRectangle(bounds, InvalidBrush);
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
            bounds.Width = bounds.Height = KeySize; // key is always square, fixed size

            switch (drawMode & DrawMode.States)
            {
                case DrawMode.Normal:
                    c.Graphics.FillEllipse(bounds, color);

                    if ((drawMode & DrawMode.Selected) != 0)
                    {
                        D2dAntialiasMode originalAntiAliasMode = c.Graphics.AntialiasMode;
                        c.Graphics.AntialiasMode = D2dAntialiasMode.PerPrimitive;
                        c.Graphics.DrawEllipse(
                            new D2dEllipse(
                                new PointF(bounds.X + bounds.Width * 0.5f, bounds.Y + bounds.Height * 0.5f),
                                bounds.Width * 0.5f, bounds.Height * 0.5f),
                            SelectedBrush, 3.0f);
                        c.Graphics.AntialiasMode = originalAntiAliasMode;
                    }
                    break;
                case DrawMode.Collapsed:
                    c.Graphics.FillEllipse(bounds, CollapsedBrush);
                    break;
                case DrawMode.Ghost:
                    c.Graphics.FillEllipse(bounds, Color.FromArgb(128, color));
                    c.Graphics.DrawText(
                        GetXPositionString(bounds.Left + KeySize * 0.5f, c),
                        c.TextFormat,
                        new PointF(bounds.Right + 16, bounds.Y),
                        TextBrush);
                    break;
                case DrawMode.Invalid:
                    c.Graphics.FillEllipse(bounds, InvalidBrush);
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
                    c.Graphics.DrawLine(middle, bounds.Top, middle, bounds.Bottom, color, 1.0f, null);

                    bool selected = (drawMode & DrawMode.Selected) != 0;
                    Color handleColor = selected ? Color.Tomato : color;
                    RectangleF handleRect =
                        new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Width);
                    c.Graphics.FillRectangle(handleRect, handleColor);
                    break;
                case DrawMode.Collapsed:
                    c.Graphics.FillRectangle(
                        new RectangleF(middle, bounds.Y, 1, bounds.Height), CollapsedBrush);
                    break;
                case DrawMode.Ghost:
                    c.Graphics.DrawLine(middle, bounds.Top, middle, bounds.Bottom, Color.FromArgb(128, color), 1.0f, null);
                    c.Graphics.DrawText(
                        GetXPositionString(middle, c),
                        c.TextFormat,
                        new PointF(bounds.Right + 16, bounds.Y),
                        TextBrush);
                    break;
                case DrawMode.Invalid:
                    c.Graphics.DrawRectangle(bounds, Color.DimGray, 1.0f, null);
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
        /// <returns>String for displaying a time (world x-coordinate) to the user</returns>
        protected virtual string GetXPositionString(float x, Context c)
        {
            float frame = x * c.InverseTransform.Elements[0] + c.InverseTransform.Elements[4];
            frame = (float)Math.Round(frame);
            return frame.ToString(CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// The brush used for drawing highlights around intervals and keys</summary>
        protected D2dBrush SelectedBrush;

        /// <summary>
        /// The brush used for drawing "shadows" for intervals, keys, and
        /// markers that are in collapsed groups</summary>
        protected D2dBrush CollapsedBrush;

        /// <summary>
        /// The brush used for drawing invalid intervals and keys</summary>
        protected D2dBrush InvalidBrush;

        /// <summary>
        /// Gets and sets the brush used for drawing groups</summary>
        protected D2dLinearGradientBrush GroupBrush;

        /// <summary>
        /// The brush used for drawing ghosted groups (i.e., groups that
        /// are being actively moved by the user)</summary>
        protected D2dBrush GhostGroupBrush;

        /// <summary>
        /// The brush used for drawing tracks</summary>
        protected D2dBrush TrackBrush;

        /// <summary>
        /// The brush used for drawing ghosted tracks (i.e., tracks that
        /// are being actively moved by the user)</summary>
        protected D2dBrush GhostTrackBrush;

        /// <summary>
        /// The brush used for drawing text on intervals, keys, and markers</summary>
        protected D2dSolidColorBrush TextBrush;

        private bool m_disposed;
        private int m_minimumDrawnIntervalLength = 28;

        static D2dDefaultTimelineRenderer()
        {
            s_leftBottomFormat = new StringFormat();
            s_leftBottomFormat.Alignment = StringAlignment.Near;
            s_leftBottomFormat.LineAlignment = StringAlignment.Far;
        }

        private static readonly StringFormat s_leftBottomFormat;
    }
}
