//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// ToolStrip label for search query</summary>
    public class QueryLabel : QueryNode
    {
        /// <summary>
        /// Constructor</summary>
        private QueryLabel() { }   // no default constructor

        /// <summary>
        /// Constructor with label text</summary>
        /// <param name="text">Desired label text</param>
        public QueryLabel(string text)
        {
            m_text = text;
        }

        /// <summary>
        /// Gets the ToolStripLabel instance for this QueryLabel, creating it if necessary</summary>
        private ToolStripLabel ToolStripLabel
        {
            get
            {
                if (m_toolStripLabel == null)
                    m_toolStripLabel = new ToolStripLabel(m_text);
                return m_toolStripLabel;
            }
        }

        /// <summary>
        /// Obtains a list of ToolStrip items for all children recursively</summary>
        /// <param name="items">List of ToolStripItems</param>
        public override void GetToolStripItems(List<ToolStripItem> items)
        {
            items.Add(ToolStripLabel);
            base.GetToolStripItems(items);
        }

        /// <summary>
        /// Gets label's ToolStripItem</summary>
        /// <returns>ToolStripLabel for this QueryLabel</returns>
        public override ToolStripItem GetToolStripItem()
        {
            return ToolStripLabel;
        }

        /// <summary>
        /// ToolStripLabel for label</summary>
        ToolStripLabel m_toolStripLabel;
        /// <summary>
        /// Text for ToolStripLabel</summary>
        readonly string m_text;
    }
}
