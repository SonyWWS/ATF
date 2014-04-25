//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// ToolStrip drop down button for search query</summary>
    public class QueryOption : QueryNode
    {
        /// <summary>
        /// Query option changed event</summary>
        public event EventHandler OptionChanged;

        /// <summary>
        /// Constructor</summary>
        public QueryOption()
        {
        }

        /// <summary>
        /// Registers item for drop down button menu</summary>
        /// <param name="optionItem">QueryOptionItem to be registered</param>
        public void RegisterOptionItem(QueryOptionItem optionItem)
        {
            optionItem.ItemSelected += queryOptionItem_Selected;
        }

        /// <summary>
        /// Gets the ToolStripDropDownButton instance for this QueryOption, creating it if necessary</summary>
        private ToolStripDropDownButton ToolStripDropDownButton
        {
            get
            {
                if (m_toolStripDropDownButton == null)
                {
                    m_toolStripDropDownButton = new ToolStripDropDownButton();
                    m_toolStripDropDownButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                }

                return m_toolStripDropDownButton;
            }
        }

        private void queryOptionItem_Selected(object sender, EventArgs e)
        {
            QueryOptionItem selectedItem = sender as QueryOptionItem;
            if (selectedItem != null)
                m_selectedItem = selectedItem;

            OptionChanged.Raise(this, EventArgs.Empty);
        }

        /// <summary>
        /// Obtains a list of ToolStrip items for all children recursively</summary>
        /// <param name="items">List of ToolStripItems</param>
        public override void GetToolStripItems(List<ToolStripItem> items)
        {
            ToolStripDropDownButton.DropDownItems.Clear();
            foreach (QueryOptionItem childOption in Children)
                ToolStripDropDownButton.DropDownItems.Add(childOption.MenuItem);

            items.Add(ToolStripDropDownButton);

            if (m_selectedItem == null && Children.Count > 0)
                m_selectedItem = Children[0] as QueryOptionItem;

            if (m_selectedItem != null)
            {
                ToolStripDropDownButton.Text = m_selectedItem.Text;
                m_selectedItem.GetToolStripItems(items);
            }
        }

        /// <summary>
        /// Gets drop down button's ToolStripItem</summary>
        /// <returns>ToolStripDropDownButton for this QueryOption</returns>
        public override ToolStripItem GetToolStripItem()
        {
            return ToolStripDropDownButton;
        }

        /// <summary>
        /// Builds predicate for selected QueryOptionItem in ToolStripDropDownButton</summary>
        /// <param name="predicate">Search predicates</param>
        public override void BuildPredicate(IQueryPredicate predicate)
        {
            if (m_selectedItem != null)
            {
                m_selectedItem.BuildPredicate(predicate);
            }
        }

        /// <summary>
        /// Gets selected QueryOptionItem in ToolStripDropDownButton</summary>
        protected QueryOptionItem SelectedItem
        {
            get { return m_selectedItem; }
        }

        private ToolStripDropDownButton m_toolStripDropDownButton;
        private QueryOptionItem m_selectedItem;
    }
}
