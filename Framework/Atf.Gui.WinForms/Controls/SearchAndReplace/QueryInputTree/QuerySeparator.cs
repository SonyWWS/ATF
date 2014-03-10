//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// ToolStrip separator for search query</summary>
    public class QuerySeparator : QueryNode
    {
        /// <summary>
        /// Constructor</summary>
        public QuerySeparator() { }

        /// <summary>
        /// Gets the ToolStripLabel instance for this QuerySeparator, creating it if necessary</summary>
        private ToolStripSeparator ToolStripSeparator
        {
            get
            {
                if (m_toolStripSeparator == null)
                    m_toolStripSeparator = new ToolStripSeparator();
                return m_toolStripSeparator;
            }
        }

        /// <summary>
        /// Obtains a list of ToolStrip items for all children recursively</summary>
        /// <param name="items">List of ToolStripItems</param>
        public override void GetToolStripItems(List<ToolStripItem> items)
        {
            items.Add(ToolStripSeparator);
            base.GetToolStripItems(items);
        }

        /// <summary>
        /// Gets ToolStrip separator's ToolStripItem</summary>
        /// <returns>ToolStripSeparator for this QuerySeparator</returns>
        public override ToolStripItem GetToolStripItem()
        {
            return ToolStripSeparator;
        }

        /// <summary>
        /// ToolStripSeparator for this instance</summary>
        ToolStripSeparator m_toolStripSeparator;
    }
}
