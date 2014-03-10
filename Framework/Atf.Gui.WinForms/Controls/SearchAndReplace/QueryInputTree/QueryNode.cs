//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// QueryTree nodes that are building blocks for specifying search parameters in the search ToolStrip</summary>
    public class QueryNode : Tree<QueryNode>
    {
        /// <summary>
        /// Constructor</summary>
        public QueryNode() { }

        /// <summary>
        /// Gets root node of tree of QueryNodes</summary>
        public QueryNode Root
        {
            get
            {
                // get root node
                QueryNode nodeAbove = this;
                while (nodeAbove.Parent != null)
                    nodeAbove = nodeAbove.Parent as QueryNode;
                return nodeAbove;
            }
        }

        /// <summary>
        /// Gets first child of QueryNode</summary>
        protected QueryNode FirstChild
        {
            get { return (Children.Count > 0) ? Children[0].Value : null; }
        }

        /// <summary>
        /// Obtains a list of ToolStrip items for all children recursively</summary>
        /// <param name="items">List of ToolStripItems</param>
        public virtual void GetToolStripItems(List<ToolStripItem> items)
        {
            foreach (QueryNode child in Children)
                child.GetToolStripItems(items);
        }

        /// <summary>
        /// Gets node's ToolStripItem recursively</summary>
        /// <returns>ToolStripItem</returns>
        public virtual ToolStripItem GetToolStripItem()
        {
            ToolStripItem returnItem = null;

            returnItem = FirstChild.GetToolStripItem();

            return returnItem;
        }

        /// <summary>
        /// Builds predicate from all children recursively</summary>
        /// <param name="predicate">Search predicates</param>
        public virtual void BuildPredicate(IQueryPredicate predicate)
        {
            foreach (QueryNode child in Children)
                child.BuildPredicate(predicate);
        }
    }
}
