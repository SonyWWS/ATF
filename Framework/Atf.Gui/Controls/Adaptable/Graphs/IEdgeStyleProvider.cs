//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Drawing;
using Sce.Atf.Direct2D;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Graph edge style enumeration</summary>
    public enum EdgeStyle
    {
        /// <summary>
        /// Edge goes through each expanded group pin position even if the group pin box is hidden, in Bezier</summary>
        Default,
        /// <summary>
        /// Edge goes through each expanded group pin position even if the group pin box is hidden, in line</summary>
        Polyline,
        /// <summary>
        /// Edge bypasses each expanded group pin positions whenever the group pin box is hidden</summary>
        DirectCurve,
        //DirectLine,    // Edge is represented by a straight line directly links two internal pins without routing via group pins(TODO)
        //Orthogonal,    // Edge shape is computed automatically to a sequence of vertical or horizontal segments (TODO)
    }

    /// <summary>
    /// Provides information about an edge's shape and appearance</summary>
    public interface IEdgeStyleProvider
    {
        /// <summary>
        /// Gets or sets current edge style</summary>
        EdgeStyle EdgeStyle { get; set; }


        /// <summary>
        /// Retrieves edge data to be plotted for current edge style</summary>
        /// <param name="render">Renderer to retrieve data for</param>
        /// <param name="worldOffset">Current world offset of renderer when drawing this edge</param>
        /// <param name="g">Graphics object</param>
        /// <returns>Edge data to plot for</returns>
        IEnumerable<EdgeStyleData> GetData(DiagramRenderer render, Point worldOffset, D2dGraphics g);
    }
}
