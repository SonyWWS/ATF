//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Route for directed graph edges, so that multiple edges do not overlap in
    /// rendering. Route 0 is a straight line between nodes, route 1 is a slight arc,
    /// route 2 is a more curved arc, and so on.</summary>
    public class NumberedRoute : IEdgeRoute
    {
        /// <summary>
        /// Gets or sets the integer index identifying the route taken by the edge</summary>
        public int Index
        {
            get { return m_index; }
            set { m_index = value; }
        }

        #region IEdgeRoute Members

        /// <summary>
        /// Gets whether this route can accept multiple edges</summary>
        public virtual bool AllowFanIn
        {
            get { return true; }
        }

        /// <summary>
        /// Gets whether this route can accept multiple edges</summary>
        public virtual bool AllowFanOut
        {
            get { return true; }
        }

        #endregion

        private int m_index;
    }
}
