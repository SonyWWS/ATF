//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// ToolStrip menu item in ToolStrip drop down button for search query</summary>
    public class QueryOptionItem : QueryNode
    {
        /// <summary>
        /// Event that is raised when a ToolStrip menu item is selected</summary>
        public event EventHandler ItemSelected;

        private QueryOptionItem() { }        // no default constructor
        /// <summary>
        /// Constructor</summary>
        /// <param name="text">Text on ToolStrip menu item</param>
        /// <param name="tag">ID tag for ToolStrip menu item</param>
        public QueryOptionItem(string text, UInt64 tag)
        {
            m_text = text;
            m_tag = tag;
        }

        /// <summary>
        /// Gets text on ToolStrip menu item</summary>
        public string Text
        {
            get { return m_text; }
        }

        /// <summary>
        /// Gets ID tag for ToolStrip menu item</summary>
        public UInt64 Tag
        {
            get { return m_tag; }
        }

        /// <summary>
        /// Gets the ToolStripMenuItem instance for this QueryOptionItem, creating it if necessary</summary>
        public ToolStripMenuItem MenuItem
        {
            get
            {
                ToolStripMenuItem menuItem = new ToolStripMenuItem(m_text, null, queryOptionItem_Clicked);
                menuItem.CheckOnClick = false;
                menuItem.Checked = false;
                menuItem.Tag = this;
                return menuItem;
            }
        }

        private void queryOptionItem_Clicked(object sender, EventArgs e)
        {
            ItemSelected.Raise(this, EventArgs.Empty);
        }

        private readonly string m_text;
        private readonly UInt64 m_tag;
    }
}
