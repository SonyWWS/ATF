//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Holds information on the appearance and behavior of an item in a list or
    /// tree control</summary>
    public class WinFormsItemInfo : ItemInfo
    {
        /// <summary>
        /// Constructor for items with no associated images to be drawn</summary>
        public WinFormsItemInfo()
            : this(null, null)
        {
        }

        /// <summary>
        /// Constructor for items that may have an associated image to be drawn, as chosen
        /// by ItemInfo.ImageIndex, but no "state" image (as for source control state)</summary>
        /// <param name="imageList">ImageList to which ItemInfo.ImageIndex refers</param>
        public WinFormsItemInfo(ImageList imageList)
            : this(imageList, null)
        {
        }

        /// <summary>
        /// Constructor for items that may have both a normal associated image (chosen
        /// by ItemInfo.ImageIndex) and a "state" image (chosen by ItemInfo.StateImageIndex)</summary>
        /// <param name="imageList">ImageList to which ImageIndex refers</param>
        /// <param name="stateImageList">ImageList that the ItemInfo.StateImageIndex refers, used to indicate
        /// source control state, for example</param>
        public WinFormsItemInfo(ImageList imageList, ImageList stateImageList)
        {
            if (imageList == null)
                imageList = s_emptyImageList;
            if (stateImageList == null)
                stateImageList = imageList;

            m_imageList = imageList;
            m_stateImageList = stateImageList;
        }

        /// <summary>
        /// Gets or sets whether item is checked</summary>
        /// <remarks>Default is false.</remarks>
        public override bool Checked
        {
            get { return (m_checkState == CheckState.Checked); }
            set { m_checkState = value? CheckState.Checked : CheckState.Unchecked; }
        }

        /// <summary>
        /// Gets or sets the CheckState: Checked, Unchecked or Indeterminate</summary>
        /// <remarks>Default is CheckState.Unchecked.</remarks>
        public CheckState CheckState
        {
            get { return m_checkState; }
            set { m_checkState = value; }
        }

        /// <summary>
        /// Gets ImageList to which ItemInfo.ImageIndex refers</summary>
        public ImageList ImageList
        {
            get { return m_imageList; }
        }

        /// <summary>
        /// Gets ImageList to which ItemInfo.StateImageIndex refers</summary>
        public ImageList StateImageList
        {
            get { return m_stateImageList; }
        }

        private readonly ImageList m_imageList;
        private readonly ImageList m_stateImageList;
        private CheckState m_checkState = CheckState.Unchecked;
        private static readonly ImageList s_emptyImageList = new ImageList();
    }
}
