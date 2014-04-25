//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Contains options for specifying the shape or appearance of an edge</summary>
    public class EdgeStyleData
    {
        /// <summary>
        /// Enumeration for edge shapes</summary>
        public enum EdgeShape
        {
            /// <summary>Use graph render default edge shape</summary>
            Default,
            /// <summary>Edge is a straight line between the start point and the end point</summary>
            Line,
            /// <summary>Edge shape is represented as one Bezier curve</summary>
            Bezier,
            /// <summary>Edge shape is displayed as Bezier spline</summary>
            BezierSpline,
            /// <summary>Edge is represented by straight line segments</summary>
            Polyline,
            /// <summary>Do not draw edge</summary>
            None
        }

        /// <summary>
        /// Constructor</summary>
        public EdgeStyleData()
        {
            ShapeType = EdgeShape.Default;
            Thickness = 1.5f;
        }

        /// <summary>
        /// Gets or sets the edge shape type</summary>
        public EdgeShape ShapeType { get; set; }

        /// <summary>
        /// Gets or sets the edge thickness</summary>
        public float Thickness { get; set; }

  
        /// <summary>
        /// Gets or sets the edge data that represents the edge shape</summary>
        public object EdgeData { get; set; }
    }
}
