//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using System.Drawing;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Class for visibility icon position</summary>
    public class DiagramVisibilityCheck
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="checkBounds">Expander's bounding rectangle</param>
        public DiagramVisibilityCheck(RectangleF checkBounds)
        {
            m_checkBounds = checkBounds;
        }

       
        /// <summary>
        /// Gets the expander's bounding rectangle</summary>
        public RectangleF Bounds
        {
            get { return m_checkBounds; }
        }
        private readonly RectangleF m_checkBounds;
    }
}
