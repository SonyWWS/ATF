//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Control to display a string when the mouse hovers</summary>
    public class HoverLabel : HoverBase
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="label">Hover text</param>
        public HoverLabel(string label)
        {
            Label = label;
        }

        /// <summary>
        /// Gets or sets hover text</summary>
        public string Label
        {
            get { return m_label; }
            set
            {
                m_label = value;
                SetBounds();
            }
        }

        /// <summary>
        /// Raises the Paint event</summary>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.DrawRectangle(SystemPens.ControlDarkDark, new Rectangle(0, 0, Width - 1, Height - 1));
            e.Graphics.DrawString(m_label, Font, Brushes.Black, TextMargin, TextMargin);
        }

        private void SetBounds()
        {
            using (Graphics g = CreateGraphics())
            {
                SizeF labelSize = g.MeasureString(m_label, Font);
                Size = new Size((int)Math.Ceiling(labelSize.Width) + 2 * TextMargin, (int)Math.Ceiling(labelSize.Height) + 2 * TextMargin);
            }
        }

        private string m_label;
        private const int TextMargin = 2;
    }
}
