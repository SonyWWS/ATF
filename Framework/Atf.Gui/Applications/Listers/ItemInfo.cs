//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Holds information on the appearance and behavior of an item in a list or
    /// tree control</summary>
    public abstract class ItemInfo
    {
        /// <summary>
        /// Constructor for items with no associated images to be drawn</summary>
        public ItemInfo()
        {
            CheckBoxEnabled = true;
        }

        /// <summary>
        /// Gets or sets item's label</summary>
        /// <remarks>Default is empty string if item has no label</remarks>
        public string Label
        {
            get { return m_label; }
            set { m_label = value; }
        }

        /// <summary>
        /// Gets or sets the font style for the item</summary>
        /// <remarks>Default is FontStyle.Regular</remarks>
        public FontStyle FontStyle
        {
            get { return m_fontStyle; }
            set { m_fontStyle = value; }
        }

        /// <summary>
        /// Gets or sets item's description</summary>
        /// <remarks>Default is empty string if item has no description</remarks>
        public string Description
        {
            get { return m_description; }
            set { m_description = value; }
        }

        /// <summary>
        /// Gets or sets whether item should have a check box control</summary>
        /// <remarks>Default is false</remarks>
        public bool HasCheck
        {
            get { return m_hasCheck; }
            set { m_hasCheck = value; }
        }

        /// <summary>
        /// Gets or sets whether check box is enabled</summary>
        /// <remarks>Default is true.
        /// This property makes sense only if HasCheck is true</remarks>
        public bool CheckBoxEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether item is checked</summary>
        /// <remarks>Default is false</remarks>
        public abstract bool Checked
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether item is a leaf (has no sub-items)</summary>
        /// <remarks>Used by tree controls to inhibit drawing the node expander; default
        /// is false</remarks>
        public bool IsLeaf
        {
            get { return m_isLeaf; }
            set { m_isLeaf = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the label is editable</summary>
        /// <remarks>Used by tree controls to inhibit editing the node label; default
        /// is true</remarks>
        public bool AllowLabelEdit
        {
            get { return m_allowLabelEdit; }
            set { m_allowLabelEdit = value; }
        }

        /// <summary>
        /// Gets or sets whether the item is selectable</summary>
        /// <remarks>Used by tree controls to inhibit selecting the node; default
        /// is true</remarks>
        public bool AllowSelect
        {
            get { return m_allowSelect; }
            set { m_allowSelect = value; }
        }

        /// <summary>
        /// Gets or sets whether this item is expanded in the view. Set by
        /// Control adapters - client code shouldn't set this value.</summary>
        public bool IsExpandedInView
        {
            get { return m_isExpandedInView; }
            set { m_isExpandedInView = value; }
        }

        /// <summary>
        /// Gets or sets index of item's image in image list</summary>
        /// <remarks>Default is -1 if item has no image.
        /// DAN: This is not required by WPF so could be moved to WinFormsItemInfo</remarks>
        public int ImageIndex
        {
            get { return m_imageIndex; }
            set { m_imageIndex = value; }
        }

        /// <summary>
        /// Gets or sets index of item's "State" image in image list</summary>
        /// <remarks>Default is -1 if item has no "State" image.
        /// DAN: This is not required by WPF so could be moved to WinFormsItemInfo</remarks>
        public int StateImageIndex
        {
            get { return m_stateImageIndex; }
            set { m_stateImageIndex = value; }
        }

        /// <summary>
        /// Gets or sets item's properties for lists and tree lists</summary>
        /// <remarks>Default is an empty array if item has no properties</remarks>
        public object[] Properties
        {
            get { return m_properties; }
            set { m_properties = value; }
        }

        /// <summary>
        /// Gets or sets item's mouse hover over text</summary>
        public string HoverText
        {
            get { return m_hoverText; }
            set { m_hoverText = value; }
        }

        private string m_label = string.Empty;
        private FontStyle m_fontStyle = FontStyle.Regular;
        private string m_description = string.Empty;
        private int m_imageIndex = -1;
        private int m_stateImageIndex = -1;
        private object[] m_properties = EmptyArray<object>.Instance;
        private bool m_hasCheck;
        private bool m_isLeaf;
        private bool m_allowLabelEdit = true;
        private bool m_allowSelect = true;
        private bool m_isExpandedInView;
        private string m_hoverText = string.Empty;
    }
}
