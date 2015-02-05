//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;


namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// An instance of this class in a GraphHitRecord indicates that the diagram expander button
    /// has been clicked.</summary>
    public class DiagramExpander
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="expanderBounds">Expander's bounding rectangle</param>
        public DiagramExpander(RectangleF expanderBounds)
        {
            m_expanderBounds = expanderBounds;
        }

        /// <summary>
        /// Gets the expander's bounding rectangle</summary>
        public RectangleF Bounds
        {
            get { return m_expanderBounds; }
        }

        private readonly RectangleF m_expanderBounds;
    }
}
