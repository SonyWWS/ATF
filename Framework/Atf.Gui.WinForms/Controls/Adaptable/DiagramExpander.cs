//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;


namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Diagram expander, which specifies a hit on an item's expander part</summary>
    public class DiagramExpander
    {
        /// <summary>
        /// Contructor</summary>
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
