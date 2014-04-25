//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;
using SharpDX.Direct2D1;

namespace Sce.Atf.Direct2D
{
    /// <summary>
    /// Enumeration indicating whether path is filled or hollow</summary>
    public enum D2dFigureBegin
    {
        /// <summary>Path should be filled</summary>
        Filled = 0,
        /// <summary>Path should be hollow</summary>
        Hollow = 1
    }

    /// <summary>
    /// Enumeration indicating whether path is open or closed</summary>
    public enum D2dFigureEnd
    {
        /// <summary>Path is open</summary>
        Open = 0,
        /// <summary>Path is closed</summary>
        Closed = 1
    }

    /// <summary>
    /// Class with operations on a GeometrySink, which is a path that can contain lines, arcs, 
    /// cubic Bezier curves, and quadratic Bezier curves</summary>
    /// <remarks>For more information, see http://sharpdx.org/documentation/api. </remarks>
    public sealed class D2dGeometrySink : D2dResource
    {
        private GeometrySink m_sink;

        internal D2dGeometrySink(GeometrySink sink)
        {
            m_sink = sink;
        }

        /// <summary>
        /// Disposes of resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && m_sink != null)
            {
                m_sink.Dispose();
                m_sink = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Starts a path at a point, optionally filling it</summary>
        /// <param name="startPoint">Path starting point</param>
        /// <param name="figureBegin">Whether path is open or closed</param>
        public void BeginFigure(PointF startPoint, D2dFigureBegin figureBegin)
        {
            m_sink.BeginFigure(startPoint.ToSharpDX(), (FigureBegin)figureBegin);
        }

        /// <summary>
        /// Ends the current path, optionally closing it</summary>
        /// <param name="figureEnd">Whether path is open or closed</param>
        public void EndFigure(D2dFigureEnd figureEnd)
        {
            m_sink.EndFigure((FigureEnd)figureEnd);
        }

        /// <summary>
        /// Adds point to the current path from the current point to the given point</summary>
        /// <param name="point">Point to extend path to</param>
        public void AddLine(PointF point)
        {
            m_sink.AddLine(point.ToSharpDX());
        }

        /// <summary>
        /// Creates and adds set of Cubic Bezier curves to the current path</summary>
        /// <param name="beziers">Array of Cubic Bezier curves to add</param>
        public void AddBeziers(params D2dBezierSegment[] beziers)
        {
            foreach (var b in beziers)
                m_sink.AddBezier(b.ToSharpDX());
        }

        /// <summary>
        /// Adds set of lines to the current path from a given list of points</summary>
        /// <param name="points">Array of points to add lines for</param>
        public void AddLines(params PointF[] points)
        {
            foreach (var b in points)
                m_sink.AddLine(b.ToSharpDX());
        }

        /// <summary>
        /// Adds arc to the current path</summary>
        /// <param name="arc">Arc to add</param>
        public void AddArc(D2dArcSegment arc)
        {
            m_sink.AddArc(arc.ToSharpDX());
        }

        /// <summary>
        /// Closes current path, indicating whether it is in an error state and resets error state</summary>
        public void Close()
        {
            m_sink.Close();
        }
    }
}
