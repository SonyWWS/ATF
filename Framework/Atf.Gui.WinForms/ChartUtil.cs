//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Sce.Atf.Direct2D;

namespace Sce.Atf
{
    /// <summary>
    /// Chart helper class, to draw grids, scales and coordinate axes</summary>
    public class ChartUtil
    {
        /// <summary>
        /// Draws horizontal grid lines using GDI+</summary>
        /// <param name="transform">Transform from graph (world) to Window's client (screen) transform</param>
        /// <param name="graphRect">Graph rectangle</param>
        /// <param name="step">Grid step in canvas coordinates</param>
        /// <param name="color">Grid line color</param>
        /// <param name="g">Graphics GDI+ drawing surface</param>
        public static void DrawHorizontalGrid(
            Matrix transform,
            RectangleF graphRect,
            double step,
            Color color,
            System.Drawing.Graphics g)
        {
            double yScale = transform.Elements[3];
            RectangleF clientRect = GdiUtil.Transform(transform, graphRect);

            double screenStep = Math.Abs(yScale * step);
            Pen pen = CreateFadedPen(screenStep, color);

            double start = graphRect.Top - MathUtil.Remainder(graphRect.Top, step) + step;
            for (double y = start; y < graphRect.Bottom; y += step)
            {
                double cy = (y - graphRect.Top) * yScale + clientRect.Top;
                g.DrawLine(pen, clientRect.Left, (float)cy, clientRect.Right, (float)cy);
            }

            pen.Dispose();
        }

        /// <summary>
        /// Draws horizontal grid using Direct2D</summary>
        /// <param name="transform">Transform from graph (world) to Window's client (screen) transform</param>
        /// <param name="graphRect">Graph rectangle</param>
        /// <param name="step">Grid step in canvas coordinates</param>
        /// <param name="color">Grid line color</param>
        /// <param name="g">Graphics Direct2D drawing surface</param>
        public static void DrawHorizontalGrid(
          Matrix transform,
          RectangleF graphRect,
          double step,
          Color color,
          D2dGraphics g)
        {
            double yScale = transform.Elements[3];
            RectangleF clientRect = GdiUtil.Transform(transform, graphRect);

            double screenStep = Math.Abs(yScale * step);

            int a  = ComputeOpacity(screenStep);
            color = Color.FromArgb(a, color);                            
            double start = graphRect.Top - MathUtil.Remainder(graphRect.Top, step) + step;
            for (double y = start; y < graphRect.Bottom; y += step)
            {
                double cy = (y - graphRect.Top) * yScale + clientRect.Top;
                g.DrawLine(clientRect.Left, (float)cy, clientRect.Right, (float)cy, color);
            }            
        }

        /// <summary>
        /// Draws vertical grid lines using GDI+</summary>
        /// <param name="transform">Transform from graph (world) to Window's client (screen) transform</param>
        /// <param name="graphRect">Graph rectangle</param>
        /// <param name="step">Grid step in canvas coordinates</param>
        /// <param name="color">Grid line color</param>
        /// <param name="g">Graphics GDI+ drawing surface</param>
        public static void DrawVerticalGrid(
            Matrix transform,
            RectangleF graphRect,
            double step,
            Color color,
            System.Drawing.Graphics g)
        {
            double xScale = transform.Elements[0];
            RectangleF clientRect = GdiUtil.Transform(transform, graphRect);

            double screenStep = Math.Abs(xScale * step);
            Pen pen = CreateFadedPen(screenStep, color);

            double start = graphRect.Left - MathUtil.Remainder(graphRect.Left, step) + step;
            for (double x = start; x < graphRect.Right; x += step)
            {
                double cx = (x - graphRect.Left) * xScale + clientRect.Left;
                g.DrawLine(pen, (float)cx, clientRect.Top, (float)cx, clientRect.Bottom);
            }

            pen.Dispose();
        }

        /// <summary>
        /// Draws vertical grid lines using Direct2D</summary>
        /// <param name="transform">Transform from graph (world) to Window's client (screen) transform</param>
        /// <param name="graphRect">Graph rectangle</param>
        /// <param name="step">Grid step in canvas coordinates</param>
        /// <param name="color">Grid line color</param>
        /// <param name="g">Graphics Direct2D drawing surface</param>
        public static void DrawVerticalGrid(
            Matrix transform,
            RectangleF graphRect,
            double step,
            Color color,
            D2dGraphics g)
        {
            double xScale = transform.Elements[0];
            RectangleF clientRect = GdiUtil.Transform(transform, graphRect);

            double screenStep = Math.Abs(xScale * step);
            int a  = ComputeOpacity(screenStep);
            color = Color.FromArgb(a, color);

            double start = graphRect.Left - MathUtil.Remainder(graphRect.Left, step) + step;
            for (double x = start; x < graphRect.Right; x += step)
            {
                double cx = (x - graphRect.Left) * xScale + clientRect.Left;
                g.DrawLine((float)cx, clientRect.Top, (float)cx, clientRect.Bottom, color);
            }            
        }

        /// <summary>
        /// Labels junctions on a grid background using GDI+</summary>
        /// <param name="transform">Transform from graph (world) to Window's client (screen) transform</param>
        /// <param name="graphRect">Graph rectangle</param>
        /// <param name="step">Grid step in canvas coordinates</param>
        /// <param name="font">Font</param>
        /// <param name="textBrush">Text brush</param>
        /// <param name="g">Graphics GDI+ drawing surface</param>
        public static void LabelGrid(
            Matrix transform,
            RectangleF graphRect,
            double step,
            Font font,
            Brush textBrush,
            System.Drawing.Graphics g)
        {
            double xScale = transform.Elements[0];
            double yScale = transform.Elements[3];
            RectangleF clientRect = GdiUtil.Transform(transform, graphRect);

            double screenStep = Math.Min(Math.Abs(xScale * step), Math.Abs(yScale * step));
            const int MIN_LABEL_SPACING = 96;
            while (screenStep < MIN_LABEL_SPACING)
            {
                screenStep *= 2;
                step *= 2;
            }

            double xStart = graphRect.Left - MathUtil.Remainder(graphRect.Left, step);
            double yStart = graphRect.Top - MathUtil.Remainder(graphRect.Top, step);

            for (double y = yStart; y <= graphRect.Bottom; y += step)
            {
                double cy = (y - graphRect.Top) * yScale + clientRect.Top;
                string yString = String.Format("{0:G4}", y);

                for (double x = xStart; x <= graphRect.Right; x += step)
                {
                    double cx = (x - graphRect.Left) * xScale + clientRect.Left;
                    string xyString = String.Format("({0:G4}, " + yString + ")", x);

                    g.DrawString(xyString, font, textBrush, (float)cx, (float)cy);
                }
            }
        }

        /// <summary>
        /// Draws grid lines corresponding to a horizontal scale using GDI+</summary>
        /// <param name="transform">Transform from graph (world) to Window's client (screen) transform</param>
        /// <param name="graphRect">Graph rectangle</param>
        /// <param name="majorSpacing">Spacing, in pixels, between major tick marks</param>
        /// <param name="linePen">Grid line pen</param>
        /// <param name="g">Graphics GDI+ drawing surface</param>
        public static void DrawHorizontalScaleGrid(
            Matrix transform,
            RectangleF graphRect,
            int majorSpacing,
            Pen linePen,
            System.Drawing.Graphics g)
        {
            double yScale = transform.Elements[3];
            RectangleF clientRect = GdiUtil.Transform(transform, graphRect);

            double min = Math.Min(graphRect.Top, graphRect.Bottom);
            double max = Math.Max(graphRect.Top, graphRect.Bottom);
            double tickAnchor = D2dUtil.CalculateTickAnchor(min, max);
            double step = D2dUtil.CalculateStep(min, max, Math.Abs(clientRect.Bottom - clientRect.Top), majorSpacing, 0.0);
            if (step > 0)
            {
                double offset = tickAnchor - min;
                offset = offset - MathUtil.Remainder(offset, step) + step;
                for (double y = tickAnchor - offset; y <= max; y += step)
                {
                    double cy;
                    if (yScale > 0)
                        cy = (y - min) * yScale + clientRect.Top;
                    else
                        cy = (y - max) * yScale + clientRect.Top;

                    g.DrawLine(linePen, clientRect.Left, (float)cy, clientRect.Right, (float)cy);
                }
            }
        }

        /// <summary>
        /// Draws grid lines corresponding to a vertical scale using GDI+</summary>
        /// <param name="transform">Transform from graph (world) to Window's client (screen) transform</param>
        /// <param name="graphRect">Graph rectangle</param>
        /// <param name="majorSpacing">Spacing, in pixels, between major tick marks</param>
        /// <param name="linePen">Grid line pen</param>
        /// <param name="g">Graphics GDI+ drawing surface</param>
        public static void DrawVerticalScaleGrid(
            Matrix transform,
            RectangleF graphRect,
            int majorSpacing,
            Pen linePen,
            System.Drawing.Graphics g)
        {
            double xScale = transform.Elements[0];
            RectangleF clientRect = GdiUtil.Transform(transform, graphRect);

            double min = Math.Min(graphRect.Left, graphRect.Right);
            double max = Math.Max(graphRect.Left, graphRect.Right);
            double tickAnchor = D2dUtil.CalculateTickAnchor(min, max);
            double step = D2dUtil.CalculateStep(min, max, Math.Abs(clientRect.Right - clientRect.Left), majorSpacing, 0.0);
            if (step > 0)
            {
                double offset = tickAnchor - min;
                offset = offset - MathUtil.Remainder(offset, step) + step;
                for (double x = tickAnchor - offset; x <= max; x += step)
                {
                    double cx = (x - graphRect.Left) * xScale + clientRect.Left;
                    g.DrawLine(linePen, (float)cx, clientRect.Top, (float)cx, clientRect.Bottom);
                }
            }
        }

        /// <summary>
        /// Draws a horizontal chart scale using GDI+</summary>
        /// <param name="transform">Graph (world) to Window's client (screen) transform</param>
        /// <param name="graphRect">Graph rectangle</param>
        /// <param name="top">Whether the scale should be aligned along the top of the rectangle</param>
        /// <param name="majorSpacing">Spacing, in pixels, between major tick marks</param>
        /// <param name="minimumGraphStep">Minimum spacing, in graph (world) space, between ticks.
        /// For example, 1.0 would limit ticks to being drawn on whole integers.</param>
        /// <param name="linePen">Scale line pen</param>
        /// <param name="font">Scale font</param>
        /// <param name="textBrush">Text brush</param>
        /// <param name="g">Graphics GDI+ drawing surface</param>
        public static void DrawHorizontalScale(
            Matrix transform,
            RectangleF graphRect,
            bool top,
            int majorSpacing,
            float minimumGraphStep,
            Pen linePen,
            Font font,
            Brush textBrush,
            System.Drawing.Graphics g)
        {
            double xScale = transform.Elements[0];
            RectangleF clientRect = GdiUtil.Transform(transform, graphRect);

            double tickEnd, majorTickStart, minorTickStart, textStart;

            if (top)
            {
                tickEnd = clientRect.Top + 1;
                majorTickStart = tickEnd + 12;
                minorTickStart = tickEnd + 6;
                textStart = tickEnd + 8;
            }
            else
            {
                tickEnd = clientRect.Bottom - 1;
                majorTickStart = tickEnd - 12;
                minorTickStart = tickEnd - 6;
                textStart = tickEnd - 19;
            }

            double min = Math.Min(graphRect.Left, graphRect.Right);
            double max = Math.Max(graphRect.Left, graphRect.Right);

            double tickAnchor = D2dUtil.CalculateTickAnchor(min, max);
            double majorGraphStep = D2dUtil.CalculateStep(
                min, max, Math.Abs(clientRect.Right - clientRect.Left), majorSpacing, minimumGraphStep);
            int numMinorTicks = D2dUtil.CalculateNumMinorTicks(majorGraphStep, minimumGraphStep, 5);
            double cMinorStep = (majorGraphStep / numMinorTicks) * xScale;
            if (majorGraphStep > 0)
            {
                double offset = tickAnchor - min;
                offset = offset - MathUtil.Remainder(offset, majorGraphStep);

                // draw leading minor ticks
                double cmx;
                cmx = ((tickAnchor - (offset + majorGraphStep)) - min) * xScale + clientRect.Left + cMinorStep;
                for (int i = 0; i < numMinorTicks - 1 && cmx < clientRect.Right; i++)
                {
                    // cull minor ticks outside of the view
                    if (cmx > clientRect.Left)
                    {
                        g.DrawLine(linePen, (float)cmx, (float)minorTickStart, (float)cmx, (float)tickEnd);
                    }
                    cmx += cMinorStep;
                }
                
                for (double x = tickAnchor - offset; x < max; x += majorGraphStep)
                {
                    double cx = (x - min) * xScale + clientRect.Left;
                    g.DrawLine(linePen, (float)cx, (float)majorTickStart, (float)cx, (float)tickEnd);
                    string xString = String.Format("{0:G8}", Math.Round(x, 6));
                    g.DrawString(xString, font, textBrush, (float)cx+1, (float)textStart);

                    // draw minor ticks
                    cmx = cx + cMinorStep;
                    for (int i = 0; i < numMinorTicks - 1 && cmx < clientRect.Right; i++)
                    {
                        g.DrawLine(linePen, (float)cmx, (float)minorTickStart, (float)cmx, (float)tickEnd);
                        cmx += cMinorStep;
                    }
                }
            }
        }

        /// <summary>
        /// Draws a vertical chart scale using GDI+</summary>
        /// <param name="transform">Transform from graph (world) to Window's client (screen) transform</param>
        /// <param name="graphRect">Graph rectangle</param>
        /// <param name="left">Scale left</param>
        /// <param name="majorSpacing">Spacing, in pixels, between major tick marks</param>
        /// <param name="minimumGraphStep">Minimum spacing, in graph (world) space, between ticks.
        /// For example, 1.0 would limit ticks to being drawn on whole integers.</param>
        /// <param name="linePen">Scale line pen</param>
        /// <param name="font">Scale font</param>
        /// <param name="textBrush">Text brush</param>
        /// <param name="g">Graphics GDI+ drawing surface</param>
        public static void DrawVerticalScale(
            Matrix transform,
            RectangleF graphRect,
            bool left,
            int majorSpacing,
            float minimumGraphStep,
            Pen linePen,
            Font font,
            Brush textBrush,
            System.Drawing.Graphics g)
        {
            double yScale = transform.Elements[3];
            RectangleF clientRect = GdiUtil.Transform(transform, graphRect);

            double tickEnd, minorTickStart, textStart;

            Matrix temp = g.Transform.Clone();
            Matrix vertical = g.Transform;
            vertical.Translate(clientRect.Right, clientRect.Bottom);
            vertical.Rotate(90);
            vertical.Translate(-clientRect.Left, -clientRect.Top);
            g.Transform = vertical;
            if (left)
            {
                tickEnd = clientRect.Right - clientRect.X;
                minorTickStart = tickEnd - 6;
                textStart = tickEnd - 19;
            }
            else
            {
                tickEnd = clientRect.Left + 1;
                minorTickStart = tickEnd + 6;
                textStart = tickEnd + 8;
            }

            double min = Math.Min(graphRect.Top, graphRect.Bottom);
            double max = Math.Max(graphRect.Top, graphRect.Bottom);

            double tickAnchor = D2dUtil.CalculateTickAnchor(min, max);
            double majorGraphStep = D2dUtil.CalculateStep(
                min, max, Math.Abs(clientRect.Bottom - clientRect.Top), majorSpacing, minimumGraphStep);
            int numMinorTicks = D2dUtil.CalculateNumMinorTicks(majorGraphStep, minimumGraphStep, 5);
            double cMinorStep = (majorGraphStep / numMinorTicks) * yScale;
            if (majorGraphStep > 0)
            {
                double offset = tickAnchor - min;
                offset = offset - MathUtil.Remainder(offset, majorGraphStep) + majorGraphStep;
                for (double x = tickAnchor - offset; x <= max; x += majorGraphStep)
                {
                    double cx = (x - min) * yScale + clientRect.Left;
                    //g.DrawLine(linePen, (float)cx, (float)majorTickStart, (float)cx, (float)tickEnd);
                    // draw minor ticks
                    double cmx = cx;
                    for (int i = 0; i < numMinorTicks; i++)
                    {
                        cmx += cMinorStep;
                        g.DrawLine(linePen, (float)cmx, (float)minorTickStart, (float)cmx, (float)tickEnd);
                    }
                    string xString = String.Format("{0:G8}", Math.Round(x, 6));
                    g.DrawString(xString, font, textBrush, (float)cx + 2, (float)textStart);
                }
            }
            g.Transform = temp;
        }

        /// <summary>
        /// Draws a label for x, y coordinates using GDI+</summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="position">Position of top left corner of label</param>
        /// <param name="backgroundBrush">Brush for background of label</param>
        /// <param name="linePen">Pen for outline of label</param>
        /// <param name="font">Font for label text</param>
        /// <param name="textBrush">Brush for text</param>
        /// <param name="g">Graphics GDI+ drawing surface</param>
        public static void DrawXYLabel(
            float x,
            float y,
            Point position,
            Brush backgroundBrush,
            Pen linePen,
            Font font,
            Brush textBrush,
            System.Drawing.Graphics g)
        {
            string xyString = x.ToString() + ", " + y.ToString();
            SizeF size = g.MeasureString(xyString, font);
            Rectangle box = new Rectangle(position.X + 8, position.Y + 8, (int)size.Width + 2, (int)size.Height + 2);
            g.FillRectangle(backgroundBrush, box);
            g.DrawRectangle(linePen, box);
            g.DrawString(xyString, font, textBrush, box.X + 1, box.Y + 1);
        }

        /// <summary>
        /// Calculates the nearest grid point. Points and distances are in canvas coordinates.</summary>
        /// <param name="original">Point to be snapped</param>
        /// <param name="horizontalStep">Horizontal distance between vertical lines on the grid</param>
        /// <param name="verticalStep">Vertical distance between horizontal lines on the grid</param>
        /// <returns>Nearest grid point</returns>
        public static Point SnapToGrid(
            Point original,
            float horizontalStep,
            float verticalStep)
        {
            int nearestX = (int)Sce.Atf.MathUtil.Snap((double)original.X, (double)horizontalStep);
            int nearestY = (int)Sce.Atf.MathUtil.Snap((double)original.Y, (double)verticalStep);
            return new Point(nearestX, nearestY);
        }

        private static Pen CreateFadedPen(double screenStep, Color color)
        {
            const int FADE_START = 64;
            const int FADE_END = 4;
            screenStep = Math.Min(screenStep, FADE_START);
            screenStep = Math.Max(screenStep, FADE_END);
            int alpha = (int)(255 * (screenStep - FADE_END) / (FADE_START - FADE_END));
            return new Pen(Color.FromArgb(alpha, color));
        }

        private static int ComputeOpacity(double screenStep)
        {
            const int FADE_START = 64;
            const int FADE_END = 4;
            screenStep = Math.Min(screenStep, FADE_START);
            screenStep = Math.Max(screenStep, FADE_END);
            return (int )( 255 * (screenStep - FADE_END) / (FADE_START - FADE_END));
        }
    }
}
