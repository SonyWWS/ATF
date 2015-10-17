﻿//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

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

        /// <summary>
        /// Constructor, creating PathGeometry object</summary>
        public D2dGeometry()
        {
            m_geometry = new PathGeometry(D2dFactory.NativeFactory);
        }

        /// <summary>
        /// Disposes of resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
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
        /// Retrieves the number of figures in the path geometry.</summary>
        public int FigureCount
        {
            get { return m_geometry.FigureCount; }
        }

        /// <summary>
        /// Retrieves the number of segments in the path geometry.</summary>
        public int SegmentCount
        {
            get { return m_geometry.SegmentCount; }
        }

        /// <summary>
        /// Retrieves the bounds of the geometry.</summary>
        public RectangleF Bounds
        {
            get
            {
                var rect = m_geometry.GetBounds();
                return new RectangleF(rect.Left, rect.Top, rect.Width, rect.Height);
            }
        }

        /// <summary>
        /// Opens the mesh for population</summary>
        /// <returns>D2dGeometrySink object</returns>
        public D2dGeometrySink Open()
        {
            return new D2dGeometrySink(m_geometry.Open());
        }

        /// <summary>
        /// Indicates whether the area filled by the geometry would contain the specified point given the specified flattening tolerance</summary>
        /// <param name="point">Specified point</param>
        /// <returns><c>True</c> if area filled by the geometry contains point</returns>
        public bool FillContainsPoint(PointF point)
        {
            return m_geometry.FillContainsPoint(point.ToSharpDX());
        }

        /// <summary>	
        /// Determines whether the geometry's stroke contains the specified point given the specified stroke thickness and style.</summary>
        /// <param name="point">Specified point</param>
        /// <param name="strokeWidth">Stroke width</param>
        /// <param name="strokeStyle">D2dStrokeStyle</param>
        /// <returns><c>True</c> if geometry's stroke contains specified point</returns>
        public bool StrokeContainsPoint(PointF point, float strokeWidth = 1.0f, D2dStrokeStyle strokeStyle = null)
        {
            return m_geometry.StrokeContainsPoint(point.ToSharpDX(), strokeWidth,
                strokeStyle != null ? strokeStyle.NativeStrokeStyle : null);
        }
    }
}
