//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Holds information on the appearance and behavior of an item in a list or
    /// tree control</summary>
    public class WpfItemInfo : ItemInfo
    {
        /// <summary>
        /// Constructor for items with no associated images to be drawn</summary>
        public WpfItemInfo()
        {
        }

        /// <summary>
        /// Gets or sets whether item is checked</summary>
        /// <remarks>Default is false</remarks>
        public override bool Checked
        {
            get { return (m_checkState == true); }
            set { m_checkState = value; }
        }

        /// <summary>
        /// Gets or sets the CheckState: Checked, Unchecked or Indeterminate</summary>
        /// <remarks>Default is CheckState.Unchecked</remarks>
        public bool? CheckState
        {
            get { return m_checkState; }
            set { m_checkState = value; }
        }
        private bool? m_checkState;

        /// <summary>
        /// Gets or sets the resource key for associated item image</summary>
        public object ImageKey { get; set; }
        
        /// <summary>
        /// Gets or sets the resource key for associated item state image</summary>
        public object StateImageKey { get; set; }
    }
}
