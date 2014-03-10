//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// An interface for a graph node (IGraphNode) that can provide additional options</summary>
    public interface IGraphNodeOptionsProvider
    {
        /// <summary>
        /// Gets the options object</summary>
        GraphNodeOptions Options
        {
            get;
        }
    }

    /// <summary>
    /// Contains options for controlling the display and other aspects of a graph node. See IGraphNodeOptionsProvider.</summary>
    public class GraphNodeOptions
    {
        /// <summary>
        /// Gets or sets the element's unique color. Default is Color.LightSteelBlue.</summary>
        public Color FillColor
        {
            get { return m_fillColor; }
            set { m_fillColor = value; }
        }

        /// <summary>
        /// Gets or sets whether or not the element can be repositioned by the user. Default is false.</summary>
        public bool Pinned
        {
            get;
            set;
        }

        private Color m_fillColor = Color.LightSteelBlue;
    }
}
