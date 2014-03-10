//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Edge route for statecharts</summary>
    public class BoundaryRoute : IEdgeRoute
    {
        /// <summary>
        /// Constructor, sets position to 0</summary>
        public BoundaryRoute()
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="position">Route position on perimeter of state</param>
        public BoundaryRoute(float position)
        {
            m_position = position;
        }

        /// <summary>
        /// Gets or sets route position on perimeter of state; range is [0..4[, starting and
        /// ending at the top-left</summary>
        public float Position
        {
            get { return m_position; }
            set { m_position = value; }
        }

        #region IEdgeRoute Members

        /// <summary>
        /// Gets whether this route can accept multiple edges</summary>
        bool IEdgeRoute.AllowFanIn
        {
            get { return true; }
        }

        /// <summary>
        /// Gets whether this route can accept multiple edges</summary>
        bool IEdgeRoute.AllowFanOut
        {
            get { return true; }
        }

        #endregion

        private float m_position;
    }
}
