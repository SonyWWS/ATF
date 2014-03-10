//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// ToolStrip button for search query</summary>
    public class QueryButton : QueryNode
    {
        /// <summary>
        /// Event that is raised when the QueryButton is clicked</summary>
        public event EventHandler Clicked;

        /// <summary>
        /// Constructor</summary>
        private QueryButton() {}
        /// <summary>
        /// Constructor with button text</summary>
        /// <param name="text">Desired button text</param>
        public QueryButton(string text) { m_text = text; }

        /// <summary>
        /// Gets the ToolStripButton instance for this QueryButton, creating it if necessary</summary>
        private ToolStripButton ToolStripButton
        {
            get
            {
                if (m_toolStripButton == null)
                {
                    m_toolStripButton = new ToolStripButton(m_text);
                    m_toolStripButton.Click += ToolStripButton_Clicked;
                }
                return m_toolStripButton;
            }
        }

        /// <summary>
        /// Raises the ToolStripButton Clicked event</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">EventArgs that contains the event data</param>
        private void ToolStripButton_Clicked(object sender, EventArgs e)
        {
            Clicked.Raise(sender, e);
        }

        /// <summary>
        /// Obtains a list of ToolStrip items for all children recursively</summary>
        /// <param name="items">List of ToolStripItems</param>
        public override void GetToolStripItems(List<ToolStripItem> items)
        {
            items.Add(ToolStripButton);
            base.GetToolStripItems(items);
        }

        /// <summary>
        /// Gets button's ToolStripItem</summary>
        /// <returns>ToolStripButton for this QueryButton</returns>
        public override ToolStripItem GetToolStripItem()
        {
            return ToolStripButton;
        }

        private ToolStripButton m_toolStripButton;
        private readonly string m_text;
    }
}
