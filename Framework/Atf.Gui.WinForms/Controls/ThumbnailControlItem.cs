//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Item in a ThumbnailControl</summary>
    public class ThumbnailControlItem
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="image">Thumbnail image</param>
        public ThumbnailControlItem(Image image)
        {
            m_image = image;
        }

        /// <summary>
        /// Gets and sets the image</summary>
        public Image Image
        {
            get { return m_image; }
            set
            { 
                m_image = value;
                InvalidateControl();
            }
        }

        /// <summary>
        /// Gets and sets the indicator</summary>
        public string Indicator
        {
            get { return m_indicator; }
            set
            { 
                m_indicator = value;
                InvalidateControl();
            }
        }

        /// <summary>
        /// Gets and sets the tag</summary>
        public object Tag
        {
            get { return m_tag; }
            set { m_tag = value; }
        }

        /// <summary>
        /// Gets and sets the name</summary>
        public string Name
        {
            get { return m_name; }
            set
            { 
                m_name = value;
                InvalidateControl();
            }
        }

        /// <summary>
        /// Gets and sets the description</summary>
        public string Description
        {
            get { return m_description; }
            set { m_description = value; }
        }

        // maintained by ThumbnailItemList
        internal ThumbnailControl Control;

        private void InvalidateControl()
        {
            if (Control != null)
                Control.Invalidate();
        }

        private Image m_image;
        private object m_tag;
        private string m_indicator;
        private string m_name;
        private string m_description;
    }
}
