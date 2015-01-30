//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// An instance of this class in a GraphHitRecord indicates that the show-pins toggle button
    /// has been clicked.</summary>
    public class ShowPinsToggle
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="bounds">Expander's bounding rectangle</param>
        public ShowPinsToggle(RectangleF bounds)
        {
            m_bounds = bounds;
        }

        /// <summary>
        /// Gets the expander's bounding rectangle</summary>
        public RectangleF Bounds
        {
            get { return m_bounds; }
        }

        private readonly RectangleF m_bounds;
    }
}
