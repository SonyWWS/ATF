//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;
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

        /// <summary>
        /// Get or set FontWeight</summary>
        public FontWeight FontWeight
        {
            get
            {
                var boldFontSet = (FontStyle & System.Drawing.FontStyle.Bold) == System.Drawing.FontStyle.Bold;
                return (boldFontSet) ? FontWeights.Bold : FontWeights.Normal;
            }
            set
            {
                if (value == FontWeights.Bold)
                    FontStyle |= System.Drawing.FontStyle.Bold;
                else
                    FontStyle &= ~System.Drawing.FontStyle.Bold;
            }
        }

        /// <summary>
        /// Get or set FontStyle for italic style</summary>
        public FontStyle FontItalicStyle
        {
            get
            {
                var italicsBitSet = (FontStyle & System.Drawing.FontStyle.Italic) == System.Drawing.FontStyle.Italic;
                return (italicsBitSet) ? FontStyles.Italic : FontStyles.Normal;
            }
            set
            {
                if (value == FontStyles.Italic)
                    FontStyle |= System.Drawing.FontStyle.Italic;
                else
                    FontStyle &= ~System.Drawing.FontStyle.Italic;
            }
        }

        /// <summary>
        /// Get or set resource key for associated item overlay image</summary>
        public object OverlayImageKey { get; set; }

        /// <summary>
        /// Gets or sets the enabled flag</summary>
        public bool IsEnabled
        {
            get { return m_isEnabled; }
            set { m_isEnabled = value; }
        }
        private bool m_isEnabled = true;

        /// <summary>
        /// Gets or sets the visibility flag</summary>
        public bool IsVisible
        {
            get { return m_isVisible; }
            set { m_isVisible = value; }
        }
        private bool m_isVisible = true;
    }
}
