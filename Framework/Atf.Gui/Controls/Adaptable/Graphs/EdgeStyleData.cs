//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Contains options for specifying the shape or appearance of an edge.</summary>
    public class EdgeStyleData
    {
        public enum EdgeShape
        {
            Default,       // use  graph render default edge shape
            Line,          // edge is a straight line between the start point and the end point.
            Bezier,        // edge shape is represented as one Bezier curve
            BezierSpline,  // edge shape is displayed as Bezier spline
            Polyline,      // edge is represented by straight line segments,
            None           // do not draw edge
        }

        public EdgeStyleData()
        {
            ShapeType = EdgeShape.Default;
            Thickness = 1.5f;
        }

        /// <summary>
        /// Gets/sets the edge shape type.</summary>
        public EdgeShape ShapeType { get; set; }

        /// <summary>
        /// Gets/sets the edge thickness.</summary>
        public float Thickness { get; set; }

  
        /// <summary>
        /// Gets/sets the edge data that represents the edge shape.</summary>
        public object EdgeData { get; set; }
    }
}
