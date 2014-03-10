//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Drawing;
using Sce.Atf.Direct2D;

namespace Sce.Atf.Controls.Adaptable.Graphs
{

    public enum EdgeStyle
    {
        Default,       // The edge goes through each expanded group pin position even the group pin box is hidden, in Bezier 
        Polyline,      // The edge goes through each expanded group pin position even the group pin box is hidden, in line 
        DirectCurve,   // The edge bypasses each expanded group pin positions whenever the group pin box is hidden
        //DirectLine,    // Edge is represented by a straight line directly links two internal pins without routing via group pins(TODO)
        //Orthogonal,    // Edge shape is computed automatically to a sequence of vertical or horizontal segments (TODO)
    }

    /// <summary>
    /// Provides information about an edge's shape and appearance
    /// </summary>
    public interface IEdgeStyleProvider
    {
        /// <summary>
        /// Gets or sets the current edge style.</summary>
        EdgeStyle EdgeStyle { get; set; }


        /// <summary>
        /// Retrieves the edge data to be plotted for the current edge style.</summary>
        /// <param name="render">The render to retrieve the data for.</param>
        /// <param name="worldOffset">Current world offset of the render when drawing this edge.</param>
        /// <param name="g">Graphics object</param>
        /// <returns>The edge data to plot for.</returns>
        IEnumerable<EdgeStyleData> GetData(DiagramRenderer render, Point worldOffset, D2dGraphics g);
    }
}
