//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows.Controls;
using System.Windows;

namespace Sce.Atf.Wpf.Docking
{
    /// <summary>
    /// This is class for Icon used when user wants to dock the window, and the UI shows little icon
    /// indicators, for where to dock the window.</summary>
    internal class DockIcon : Control
    {
        /// <summary>
        /// Gets and sets offset origin of the icon from top left corner of the parent control</summary>
        public Point Offset
        {
            get { return m_offset; }
            set
            {
                m_offset = value;
                Canvas.SetLeft(this, m_offset.X);
                Canvas.SetTop(this, m_offset.Y);
            }
        }

        /// <summary>
        /// Dependency property for highlight state of the icon</summary>
        public static DependencyProperty HighlightProperty = DependencyProperty.Register("Highlight", typeof(bool), typeof(DockIcon));

        /// <summary>
        /// Gets and sets the highlight state of the icon</summary>
        public bool Highlight
        {
            get { return ((bool)(base.GetValue(DockIcon.HighlightProperty))); }
            set { base.SetValue(DockIcon.HighlightProperty, value); }
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="style">Style to use for Icon</param>
        /// <param name="size">Size of Icon</param>
        public DockIcon(Style style, Size size)
        {
            Highlight = false;
            Style = style;
            Width = size.Width;
            Height = size.Height;
            m_size = size;
        }

        /// <summary>
        /// Hit test the input point whether it is inside of the bounds of this Icon.</summary>
        /// <param name="p">Point to test</param>
        /// <returns><c>True</c> if the point is within the icon bounds, otherwise false</returns>
        public bool HitTest(Point p)
        {
            Rect rect = new Rect(m_offset, m_size);
            return rect.Contains(p);
        }

        private Size m_size;
        private Point m_offset;
    }
}
