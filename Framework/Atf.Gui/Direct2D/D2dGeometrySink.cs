//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;
using SharpDX.Direct2D1;

namespace Sce.Atf.Direct2D
{
    public enum D2dFigureBegin
    {
        Filled = 0,
        Hollow = 1
    }

    public enum D2dFigureEnd
    {
        Open = 0,
        Closed = 1
    }

    public sealed class D2dGeometrySink : D2dResource
    {
        private GeometrySink m_sink;

        internal D2dGeometrySink(GeometrySink sink)
        {
            m_sink = sink;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && m_sink != null)
            {
                m_sink.Dispose();
                m_sink = null;
            }
            base.Dispose(disposing);
        }

        public void BeginFigure(PointF startPoint, D2dFigureBegin figureBegin)
        {
            m_sink.BeginFigure(startPoint.ToSharpDX(), (FigureBegin)figureBegin);
        }

        public void EndFigure(D2dFigureEnd figureEnd)
        {
            m_sink.EndFigure((FigureEnd)figureEnd);
        }

        public void AddLine(PointF point)
        {
            m_sink.AddLine(point.ToSharpDX());
        }

        public void AddBeziers(params D2dBezierSegment[] beziers)
        {
            foreach (var b in beziers)
                m_sink.AddBezier(b.ToSharpDX());
        }

        public void AddLines(params PointF[] points)
        {
            foreach (var b in points)
                m_sink.AddLine(b.ToSharpDX());
        }

        public void AddArc(D2dArcSegment arc)
        {
            m_sink.AddArc(arc.ToSharpDX());
        }

        public void Close()
        {
            m_sink.Close();
        }
    }
}
