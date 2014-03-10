//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;

using SharpDX.Direct2D1;

namespace Sce.Atf.Direct2D
{
    /// <summary>
    /// Brush that paints an area with a solid color</summary>
    public class D2dSolidColorBrush : D2dBrush
    {
        /// <summary>
        /// Gets and sets the color of the solid color brush</summary>
        public Color Color
        {
            get { return m_color; }
            set
            {
                m_color = value;
                var brush = (SolidColorBrush)NativeBrush;
                brush.Color = value.ToColor4();
            }
        }

        /// <summary>
        /// Create or recreate native resource</summary>
        internal override void Create()
        {
            if (NativeBrush != null)
                NativeBrush.Dispose();
            NativeBrush = new SolidColorBrush(Owner.D2dRenderTarget, m_color.ToColor4());
        }

        internal D2dSolidColorBrush(D2dGraphics owner, Color color)
            : base(owner)
        {
            m_color = color;
            Create();//to-do: it's dangerous to call a virtual method in a constructor; derived class may not be properly initialized!
        }

        private Color m_color;
    }
}
