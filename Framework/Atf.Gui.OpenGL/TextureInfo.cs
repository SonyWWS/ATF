//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Class that holds data about a texture</summary>
    public class TextureInfo
    {
        /// <summary>
        /// Constructor</summary>
        public TextureInfo()
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="format">OpenGL texture format</param>
        /// <param name="width">Texture width, in pixels</param>
        /// <param name="height">Texture height, in pixels</param>
        /// <param name="components">Number of components</param>
        public TextureInfo(
            int format,
            int width,
            int height,
            int components)
        {
            m_format = format;
            m_width = width;
            m_height = height;
            m_components = components;
        }

        /// <summary>
        /// Gets and sets OpenGL texture format</summary>
        public int Format
        {
            get { return m_format; }
            set { m_format = value; }
        }

        /// <summary>
        /// Gets and sets texture width, in pixels</summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        /// Gets and sets texture height, in pixels</summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        /// <summary>
        /// Gets and sets number of components</summary>
        public int Components
        {
            get { return m_components; }
            set { m_components = value; }
        }

        /// <summary>
        /// Gets and sets whether the image should be flipped</summary>
        public bool EnableFlipImage
        {
            get { return m_enableflipimage; }
            set { m_enableflipimage = value; }
        }

        private int m_format;
        private int m_width;
        private int m_height;
        private int m_components;
        private bool m_enableflipimage;
    }
}
