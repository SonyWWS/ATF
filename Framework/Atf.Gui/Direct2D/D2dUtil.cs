//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;

using SharpDX;

using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.VectorMath;
using SharpDX.Direct2D1;
using Matrix = System.Drawing.Drawing2D.Matrix;
using RectangleF = System.Drawing.RectangleF;

namespace Sce.Atf.Direct2D
{
    public static class D2dUtil
    {

        private static PointF[] s_expanderPoints = new PointF[3];
        /// <summary>
        /// Draws a tree-control style expander, which looks like a square with
        /// a dash (expanded) or a cross (unexpanded)</summary>
        /// <param name="g">The Direct2D graphics object</param>
        /// <param name="x">X coordinate of expander top left corner</param>
        /// <param name="y">Y coordinate of expander top left corner</param>
        /// <param name="size">Size of expander, in pixels</param>
        /// <param name="brush">Brush</param>
        /// <param name="expanded">Whether or not expander should appear "expanded"</param>
        public static void DrawExpander(this D2dGraphics g, float x, float y, float size, D2dBrush brush, bool expanded)
        {
            s_expanderPoints[0] = new PointF(x, y + size);
            if (expanded)
            {                
                s_expanderPoints[1] = new PointF(x + size, y + size);
                s_expanderPoints[2] = new PointF(x + size, y);
                g.FillPolygon(s_expanderPoints, brush);
            }
            else
            {                
                s_expanderPoints[1] = new PointF(x + size, y + size / 2);
                s_expanderPoints[2] = new PointF(x, y);
                g.DrawPolygon(s_expanderPoints, brush, 1.0f);
            }

            //g.DrawRectangle(new RectangleF(x, y, size, size), brush);           
            //float lineLength = size - 4;
            //float center = size / 2;
            //g.DrawLine(x + 2, y + center, x + 2 + lineLength, y + center, brush);
            //if (!expanded)
            //    g.DrawLine(x + center, y + 2, x + center, y + 2 + lineLength, brush);
        }

        /// <summary>
        /// Draws grid lines corresponding to a vertical scale</summary>
        /// <param name="g">The Direct2D graphics object</param>
        /// <param name="transform">Graph (world) to window's client (screen) transform</param>
        /// <param name="graphRect">Graph rectangle</param>
        /// <param name="majorSpacing">Scale's major spacing</param>
        /// <param name="lineBrush">Grid line brush</param>
        public static void DrawVerticalScaleGrid(
            this D2dGraphics g,
            Matrix transform,
            RectangleF graphRect,
            int majorSpacing,
            D2dBrush lineBrush)
        {
            double xScale = transform.Elements[0];
            RectangleF clientRect = Transform(transform, graphRect);

            double min = Math.Min(graphRect.Left, graphRect.Right);
            double max = Math.Max(graphRect.Left, graphRect.Right);
            double tickAnchor = CalculateTickAnchor(min, max);
            double step = CalculateStep(min, max, Math.Abs(clientRect.Right - clientRect.Left), majorSpacing, 0.0);
            if (step > 0)
            {
                double offset = tickAnchor - min;
                offset = offset - MathUtil.Remainder(offset, step) + step;
                for (double x = tickAnchor - offset; x <= max; x += step)
                {
                    double cx = (x - graphRect.Left) * xScale + clientRect.Left;
                    g.DrawLine((float)cx, clientRect.Top, (float)cx, clientRect.Bottom, lineBrush);
                }
            }
        }

        /// <summary>
        /// Transforms rectangle</summary>
        /// <param name="matrix">Matrix representing transform</param>
        /// <param name="r">Rectangle</param>
        /// <returns>Transformed rectangle</returns>
        public static RectangleF Transform(Matrix matrix, RectangleF r)
        {
            s_tempPtsF[0] = new PointF(r.Left, r.Top);
            s_tempPtsF[1] = new PointF(r.Right, r.Bottom);
            matrix.TransformPoints(s_tempPtsF);
            return MakeRectangle(s_tempPtsF[0], s_tempPtsF[1]);
        }

        /// <summary>
        /// Transforms rectangle</summary>
        /// <param name="matrix">Matrix representing transform</param>
        /// <param name="r">Rectangle</param>
        /// <returns>Transformed rectangle</returns>
        public static RectangleF Transform(Matrix3x2F matrix, RectangleF r)
        {
            s_tempPtsF[0] = new PointF(r.Left, r.Top);
            s_tempPtsF[1] = new PointF(r.Right, r.Bottom);
            s_tempPtsF[0] = Matrix3x2F.TransformPoint(matrix, s_tempPtsF[0]);
            s_tempPtsF[1] = Matrix3x2F.TransformPoint(matrix, s_tempPtsF[1]);
            return MakeRectangle(s_tempPtsF[0], s_tempPtsF[1]);
        }

        /// <summary>
        /// Transforms point by inverse transform</summary>
        /// <param name="matrix">Matrix representing transform</param>
        /// <param name="p">Point</param>
        /// <returns>Inverse transformed point</returns>
        public static Point InverseTransform(Matrix3x2F matrix, Point p)
        {
            Matrix3x2F inverse = matrix;
            inverse.Invert();
            s_tempPtsF[0] = p;
            s_tempPtsF[0] = Matrix3x2F.TransformPoint(inverse, s_tempPtsF[0]);
            return new Point((int)s_tempPtsF[0].X, (int)s_tempPtsF[0].Y);
        }

        /// <summary>
        /// Transforms vector</summary>
        /// <param name="matrix">Matrix representing transform</param>
        /// <param name="v">Vector</param>
        /// <returns>Transformed vector</returns>
        public static PointF TransformVector(Matrix3x2F matrix, PointF v)
        {
            s_tempPtsF[0] = v;
            Matrix3x2F.TransformVector(matrix, s_tempPtsF[0]);
            return s_tempPtsF[0];
        }

      

        /// <summary>
        /// Draws a horizontal chart scale</summary>
        /// <param name="g">The Direct2D graphics object</param>
        /// <param name="transform">Graph (world) to window's client (screen) transform</param>
        /// <param name="graphRect">Graph rectangle</param>
        /// <param name="top">Whether or not the scale should be aligned along the top of the rectangle</param>
        /// <param name="majorSpacing">Spacing, in pixels, between major tick marks</param>
        /// <param name="minimumGraphStep">Minimum spacing, in graph (world) space, between ticks.
        /// For example, 1.0 would limit ticks to being drawn on whole integers.</param>
        /// <param name="lineBrush">Scale line pen</param>
        /// <param name="textFormat">Text format</param>
        /// <param name="textBrush">Text brush</param>
        public static void DrawHorizontalScale(
            this D2dGraphics g,
            Matrix transform,
            RectangleF graphRect,
            bool top,
            int majorSpacing,
            float minimumGraphStep,
            D2dBrush lineBrush,
            D2dTextFormat textFormat,
            D2dBrush textBrush)
        {
            double xScale = transform.Elements[0];
            RectangleF clientRect = Transform(transform, graphRect);

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

            double tickAnchor = CalculateTickAnchor(min, max);
            double majorGraphStep = CalculateStep(
                min, max, Math.Abs(clientRect.Right - clientRect.Left), majorSpacing, minimumGraphStep);
            int numMinorTicks = CalculateNumMinorTicks(majorGraphStep, minimumGraphStep, 5);
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
                        g.DrawLine((float)cmx, (float)minorTickStart, (float)cmx, (float)tickEnd, lineBrush);
                    }
                    cmx += cMinorStep;
                }

                for (double x = tickAnchor - offset; x < max; x += majorGraphStep)
                {
                    double cx = (x - min) * xScale + clientRect.Left;
                    g.DrawLine((float)cx, (float)majorTickStart, (float)cx, (float)tickEnd, lineBrush);
                    string xString = String.Format("{0:G8}", Math.Round(x, 6));
                    SizeF textSize = g.MeasureText(xString, textFormat);
                    var textRect = new RectangleF(new PointF((float)cx + 1, (float)textStart), textSize);
                    g.DrawText(xString, textFormat, textRect, textBrush);

                    // draw minor ticks
                    cmx = cx + cMinorStep;
                    for (int i = 0; i < numMinorTicks - 1 && cmx < clientRect.Right; i++)
                    {
                        g.DrawLine((float)cmx, (float)minorTickStart, (float)cmx, (float)tickEnd, lineBrush);
                        cmx += cMinorStep;
                    }
                }
            }
        }

        /// <summary>
        /// Calculate tick anchor, i.e. the lowest value on an axis where a tick mark is placed</summary>
        /// <param name="min">Minumum</param>
        /// <param name="max">Maximum</param>
        /// <returns>Tick anchor</returns>
        public static double CalculateTickAnchor(double min, double max)
        {
            double tickAnchor = min * max <= 0 ? 0 : Math.Pow(10.0, Math.Floor(Math.Log10(Snap10(Math.Abs(max)))));

            return tickAnchor;
        }

        /// <summary>
        /// Calculates the step size in graph (world) space between major ticks</summary>
        /// <param name="graphMin">Minumum value on tick mark axis</param>
        /// <param name="graphMax">Maximum value on tick mark axis</param>
        /// <param name="screenLength">Graph rectangle width</param>
        /// <param name="majorScreenSpacing">Spacing, in pixels, between major tick marks</param>
        /// <param name="minimumGraphSpacing">Minimum spacing, in graph (world) space, between ticks.</param>
        /// <returns>Step size</returns>
        public static double CalculateStep(double graphMin, double graphMax, double screenLength,
            int majorScreenSpacing, double minimumGraphSpacing)
        {
            double graphRange = graphMax - graphMin;
            double screenSteps = screenLength / majorScreenSpacing;
            double requestedSteps = graphRange / screenSteps;
            return Snap10(requestedSteps);
        }

        /// <summary>
        /// Calculates the number of minor ticks per major tick. This could be described as the number
        /// of minor ticks in between major ticks, plus 1. The lowest result is 1.</summary>
        /// <param name="majorGraphStep">Step size in graph (world) space between major ticks</param>
        /// <param name="minimumGraphStep">Minimum spacing, in graph (world) space, between ticks.
        /// For example, 1.0 would limit ticks to being drawn on whole integers.</param>
        /// <param name="maxMinorTicks">Maximum value for minor ticks</param>
        /// <returns>Number of minor ticks per major tick</returns>
        public static int CalculateNumMinorTicks(double majorGraphStep, double minimumGraphStep, int maxMinorTicks)
        {
            if (minimumGraphStep <= 0)
                return maxMinorTicks;

            int num = (int)(majorGraphStep / minimumGraphStep); //round down
            if (num > maxMinorTicks)
                num = maxMinorTicks;

            if (num <= 0)
                num = 1;

            return num;
        }

        /// <summary>
        /// Snaps value to an "aesthetically pleasing" value</summary>
        /// <param name="proposed">Proposed value</param>
        /// <returns>Snapped value</returns>
        public static double Snap10(double proposed)
        {
            double lesserPowerOf10 = Math.Pow(10.0, Math.Floor(Math.Log10(proposed)));
            int msd = (int)(proposed / lesserPowerOf10 + .5);

            // snap msd to 2, 5 or 10
            if (msd > 5)
                msd = 10;
            else if (msd > 2)
                msd = 5;
            else if (msd > 1)
                msd = 2;

            return msd * lesserPowerOf10;
        }

        /// <summary>
        /// Size of standard pin</summary>
        public const int ThumbtackSize = 8;

        /// <summary>
        /// Draws a pin button, which looks like a square with
        /// a dash (expanded) or a cross (unexpanded).</summary>
        /// <param name="x">X coordinate of pin top left corner</param>
        /// <param name="y">Y coordinate of pin top left corner</param>
        /// <param name="pinned">Whether or not pin should appear "pinned"</param>
        /// <param name="toLeft">Whether pin points left or right</param>
        /// <param name="pen">Pen, should be 1 pixel wide</param>
        /// <param name="g">Graphics object</param>
        public static void DrawPin(int x, int y, bool pinned, bool toLeft, D2dBrush pen, D2dGraphics g)
        {
            if (toLeft)
                DrawLeftPin(x, y, ThumbtackSize, pen, pinned, g);
            else
                DrawRightPin(x, y, ThumbtackSize, pen, pinned, g);
        }

        /// <summary>
        /// Draws a tree-control style expander, which looks like a square with
        /// a dash (expanded) or a cross (unexpanded).</summary>
        /// <param name="x">X coordinate of expander top left corner</param>
        /// <param name="y">Y coordinate of expander top left corner</param>
        /// <param name="pen">Pen, should be 1 pixel wide</param>
        /// <param name="size">Size of expander, in pixels</param>
        /// <param name="pinned">Whether or not expander should appear "expanded"</param>
        /// <param name="g">Graphics object</param>
        public static void DrawLeftPin(int x, int y, int size, D2dBrush pen, bool pinned, D2dGraphics g)
        {
            //g.DrawRectangle(pen, x, y, size, size);

            int rectWidth = size / 4;
            int rectHeight = 2 * size / 3;
            int center = size / 2;
            if (pinned)
            {
                g.DrawLine(x + center, y + rectHeight, x + center, y + size,pen); //lower center-vertical line
                g.DrawLine(x, y + rectHeight, x + size, y + rectHeight, pen); // middle-horizontal line
                g.DrawRectangle(new RectangleF(x+rectWidth, y, 2 * rectWidth, rectHeight), pen);
                g.DrawLine(x + 3 * rectWidth - 1, y, x + 3 * rectWidth - 1, y + rectHeight, pen); // a vertial line next to the right side of the rect

            }
            else
            {
                g.DrawLine(x, y + center, x + size - rectHeight, y + center, pen); //left center-horizontal line
                g.DrawLine(x + size - rectHeight, y, x + size - rectHeight, y + size, pen); // middle-vertical line
                g.DrawRectangle(new RectangleF(x + size - rectHeight, y + (size - rectWidth) / 2 - 1, rectHeight, 2 * rectWidth), pen);
                g.DrawLine(x + size - rectHeight, y + (size - rectWidth) / 2 + 2 * rectWidth - 2, x + size, y + (size - rectWidth) / 2 + 2 * rectWidth - 2, pen); // a horizontal line next to the bottom side of the rect

            }

        }

        /// <summary>
        /// Draws a tree-control style expander, which looks like a square with
        /// a dash (expanded) or a cross (unexpanded).</summary>
        /// <param name="x">X coordinate of expander top right corner</param>
        /// <param name="y">Y coordinate of expander top right corner</param>
        /// <param name="pen">Pen, should be 1 pixel wide</param>
        /// <param name="size">Size of pin, in pixels</param>
        /// <param name="pinned">Whether or not expander should appear "expanded"</param>
        /// <param name="g">Graphics object</param>
        public static void DrawRightPin(int x, int y, int size, D2dBrush pen, bool pinned, D2dGraphics g)
        {
            //g.DrawRectangle(pen, x - size, y, size, size);

            int rectWidth = size / 4;
            int rectHeight = 2 * size / 3;
            int center = size / 2;
            if (pinned)
            {
                g.DrawLine(x + center - size, y + rectHeight, x + center - size, y + size, pen); //lower center-vertical line
                g.DrawLine(x - size, y + rectHeight, x, y + rectHeight, pen); // middle-horizontal line
                g.DrawRectangle(new RectangleF(x + rectWidth - size, y, 2 * rectWidth, rectHeight), pen);
                //g.DrawLine(pen, x + 3 * rectWidth - 1, y, x + 3 * rectWidth - 1, y + rectHeight); // a vertial line next to the right side of the rect
                g.DrawLine(x + 3 * rectWidth - 1 - size, y,
                               x + 3 * rectWidth - 1 - size, y + rectHeight, pen); // a vertial line next to the right side of the rect

            }
            else
            {
                g.DrawLine(x, y + center, x - size + rectHeight, y + center, pen); //left center-horizontal line
                g.DrawLine(x - size + rectHeight, y, x - size + rectHeight, y + size, pen); // middle-vertical line
                g.DrawRectangle(new RectangleF(x - size, y + (size - rectWidth) / 2 - 1, rectHeight, 2 * rectWidth), pen);
                g.DrawLine(x - size + rectHeight, y + (size - rectWidth) / 2 + 2 * rectWidth - 2, x - size, y + (size - rectWidth) / 2 + 2 * rectWidth - 2, pen); // a horizontal line next to the bottom side of the rect

            }

        }

        /// <summary>
        /// Draws an icon that indicates a linked (referenced) item </summary>
        /// <param name="g">The Direct2D graphics object</param>
        /// <param name="x">X coordinate of icon top left corner</param>
        /// <param name="y">Y coordinate of icon top left corner</param>
        /// <param name="size">Size of expander, in pixels</param>
        /// <param name="brush">Brush</param>
        public static void DrawLink(this D2dGraphics g, float x, float y, float size, D2dBrush brush)
        {
            var path = new EdgeStyleData[5];
            var pathData = new PointF[16];
            for (int i = 0; i < 16; ++i)
            {
                pathData[i] = new PointF(s_unitCurvedArrowData[i].X * size + x, s_unitCurvedArrowData[i].Y * size + y);
            }

            var edgeData = new EdgeStyleData
           {
               ShapeType = EdgeStyleData.EdgeShape.Line,
               EdgeData = new PointF[] { pathData[0], pathData[1], pathData[2], pathData[3] }
           };
            path[0] = edgeData;

            edgeData = new EdgeStyleData
            {
                ShapeType = EdgeStyleData.EdgeShape.Bezier,
                EdgeData = new BezierCurve2F(pathData[3], pathData[4], pathData[5], pathData[6])
            };
            path[1] = edgeData;

            edgeData = new EdgeStyleData
            {
                ShapeType = EdgeStyleData.EdgeShape.Bezier,
                EdgeData = new BezierCurve2F(pathData[6], pathData[7], pathData[8], pathData[9])
            };
            path[2] = edgeData;

            edgeData = new EdgeStyleData
            {
                ShapeType = EdgeStyleData.EdgeShape.Bezier,
                EdgeData = new BezierCurve2F(pathData[9], pathData[10], pathData[11], pathData[12])
            };
            path[3] = edgeData;

            edgeData = new EdgeStyleData
            {
                ShapeType = EdgeStyleData.EdgeShape.Bezier,
                EdgeData = new BezierCurve2F(pathData[12], pathData[13], pathData[14], pathData[15])
            };
            path[4] = edgeData;

            g.FillPath(path, brush);
        }

        internal static Color4 ToColor4(this System.Drawing.Color color)
        {
            uint argb = (uint)color.ToArgb();
            uint abgr =
                (argb & 0xff00ff00) |       //alpha and green stay in place
                ((argb >> 16) & 0xff) |     //red goes down 16 bits
                ((argb & 0xff) << 16);      //blue goes up 16 bits
            return new Color4(abgr);
        }

        internal static System.Drawing.Color ToSystemColor(this Color4 color4)
        {
            // The D2d naming convention is the opposite of what we might expect --
            //  blue is the low byte and alpha is the high byte.
            return System.Drawing.Color.FromArgb(color4.ToBgra());
        }

        /// <summary>
        /// Creates new instance of Rectangle specified by 2 points</summary>
        /// <param name="p1">Start point </param>
        /// <param name="p2">End point</param>
        /// <returns>A new instance of Rectangle that is specified by 2 points</returns>
        private static RectangleF MakeRectangle(PointF p1, PointF p2)
        {
            float x = p1.X;
            float y = p1.Y;
            float width = p2.X - p1.X;
            float height = p2.Y - p1.Y;
            if (width < 0)
            {
                x += width;
                width = -width;
            }
            if (height < 0)
            {
                y += height;
                height = -height;
            }
            return new RectangleF(x, y, width, height);
        }

        private static readonly PointF[] s_tempPtsF = new PointF[2];

        private static readonly PointF[] s_unitCurvedArrowData = new []
            {
                new PointF(0.07094253f, 0.0f),          // 0
                new PointF(1.0f , 0.0f),                // 1
                new PointF(1.0f , 0.648277938f),        // 2
                new PointF(0.658584f, 0.436954677f),    // 3 
                new PointF(0.658584f, 0.436954677f),    // 4
                new PointF(0.5315179f, 0.541161954f),   // 5
                new PointF(0.404450327f, 0.648277938f), // 6
                new PointF(0.2773828f, 0.7553939f),     // 7
                new PointF(0.3329844f, 0.8624769f),     // 8
                new PointF(0.6029824f, 0.930122137f),   // 9
                new PointF(0.6029824f, 0.930122137f),   // 10
                new PointF(0.0f , 1.0f),                // 11
                new PointF(0.0312547237f, 0.5355469f),  // 12
                new PointF(0.039974235f, 0.406073779f), // 13
                new PointF(0.145124525f, 0.356557816f), // 14
                new PointF(0.309162378f, 0.186057463f), // 15
            };
    }
}
