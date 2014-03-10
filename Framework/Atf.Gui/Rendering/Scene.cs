//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;
using System.Drawing;

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// SceneNode that holds the root of the scene graph</summary>
    [Export(typeof(Scene))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Scene : SceneNode
    {
        /// <summary>
        /// Constructor</summary>
        public Scene()
            : base(null)
        {
        }

        /// <summary>
        /// Gets or sets the color used to clear the framebuffer. The alpha channel is ignored.</summary>
        public Color BackgroundColor
        {
            get { return m_backgroundColor; }
            set { m_backgroundColor = value; }
        }

        private Color m_backgroundColor = Color.LightGray;
    }
}

