//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Drawing;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.VectorMath;
using SharpDX;
using SharpDX.Direct2D1;
using RectangleF = System.Drawing.RectangleF;

namespace Sce.Atf.Direct2D
{
    /// <summary>
    /// Wrapper for the SharpDX PathGeometry class.</summary>
    public class D2dGeometry : D2dResource
    {
        private PathGeometry m_geometry;

        public D2dGeometry()
        {
            m_geometry = new PathGeometry(D2dFactory.NativeFactory);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && m_geometry != null)
            {
                m_geometry.Dispose();
                m_geometry = null;
            }
            base.Dispose(disposing);
        }

        internal PathGeometry NativeGeometry
        {
            get { return m_geometry; }
        }

        /// <summary>
        /// Retrieves the number of figures in the path geometry.
        /// </summary>
        public int FigureCount
        {
            get { return m_geometry.FigureCount; }
        }

        /// <summary>
        /// Retrieves the number of segments in the path geometry.
        /// </summary>
        public int SegmentCount
        {
            get { return m_geometry.SegmentCount; }
        }

        /// <summary>
        /// Opens the mesh for population.
        /// </summary>
        public D2dGeometrySink Open()
        {
            return new D2dGeometrySink(m_geometry.Open());
        }

        /// <summary>
        /// Indicates whether the area filled by the geometry would contain the specified point given the specified flattening tolerance. 	
        /// </summary>
        public bool FillContainsPoint(PointF point)
        {
            return m_geometry.FillContainsPoint(point.ToSharpDX());
        }

        /// <summary>	
        /// Determines whether the geometry's stroke contains the specified point given the specified stroke thickness and style. 	
        /// </summary>	
        public bool StrokeContainsPoint(PointF point, float strokeWidth = 1.0f, D2dStrokeStyle strokeStyle = null)
        {
            return m_geometry.StrokeContainsPoint(point.ToSharpDX(), strokeWidth,
                strokeStyle != null ? strokeStyle.NativeStrokeStyle : null);
        }
    }
}
