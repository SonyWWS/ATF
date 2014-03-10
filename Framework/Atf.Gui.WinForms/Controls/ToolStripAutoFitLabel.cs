//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// A ToolStripLabel that automatically changes its preferred width, so as to display as much text
    /// as possible without going into the overflow menu. The available space is divided evenly among
    /// all ToolStripAutoFitLabel objects that are children of the owning ToolStrip.</summary>
    public class ToolStripAutoFitLabel : ToolStripLabel
    {
        /// <summary>
        /// Gets preferred ToolStripAutoFitLabel size</summary>
        /// <param name="constrainingSize">Suggested size</param>
        /// <returns>Preferred size</returns>
        public override Size GetPreferredSize(Size constrainingSize)
        {
            int width = GdiUtil.GetPreferredWidth<ToolStripAutoFitLabel>(Owner);
            width = Math.Max(MinimumWidth, width);
            width = Math.Min(MaximumWidth, width);
            Size baseSize = base.GetPreferredSize(constrainingSize);
            return new Size(width, baseSize.Height);
        }

        /// <summary>
        /// Gets or sets the minimum width</summary>
        public int MinimumWidth
        {
            get { return m_minimumWidth; }
            set { m_minimumWidth = value; }
        }

        /// <summary>
        /// Gets or sets the maximum width</summary>
        public int MaximumWidth
        {
            get { return m_maximumWidth; }
            set { m_maximumWidth = value; }
        }

        private int m_minimumWidth = 50;
        private int m_maximumWidth = 200;
    }
}
