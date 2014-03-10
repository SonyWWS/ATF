//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using System.Drawing;


namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Diagram pin, to specify a hit on an item's pin part
    /// </summary>
    public class DiagramPin
    {
        /// <summary>
        /// Contructor</summary>
        /// <param name="pinBounds">Expander's bounding rectangle</param>
        public DiagramPin(RectangleF pinBounds)
        {
            m_pinBounds = pinBounds;
        }

       
        /// <summary>
        /// Gets the expander's bounding rectangle</summary>
        public RectangleF Bounds
        {
            get { return m_pinBounds; }
        }
        private readonly RectangleF m_pinBounds;
    }
}
