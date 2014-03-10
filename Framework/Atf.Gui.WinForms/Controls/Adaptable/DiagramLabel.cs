//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Diagram label, which specifies a hit on an item's label part</summary>
    public class DiagramLabel
    {
        /// <summary>
        /// Contructor</summary>
        /// <param name="labelBounds">Label's bounding rectangle</param>
        /// <param name="labelFormat">Label's text format</param>
        public DiagramLabel(Rectangle labelBounds, TextFormatFlags labelFormat)
        {
            m_labelBounds = labelBounds;
            m_labelFormat = labelFormat;
        }

        /// <summary>
        /// Gets the label text format</summary>
        public TextFormatFlags Format
        {
            get { return m_labelFormat; }
        }
        private readonly TextFormatFlags m_labelFormat;

        /// <summary>
        /// Gets the label's bounding rectangle</summary>
        public Rectangle Bounds
        {
            get { return m_labelBounds; }
        }
        private readonly Rectangle m_labelBounds;
    }
}
