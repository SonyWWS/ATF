//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Windows.Forms;

using ActiproSoftware.SyntaxEditor;

namespace Sce.Atf.Controls.SyntaxEditorControl
{
    /// <summary>
    /// Breakpoint indicator</summary>
    internal class BreakpointIndicator : BreakpointSpanIndicator, IBreakpoint
    {
        /// <summary>
        /// Constructor</summary>
        internal BreakpointIndicator(){}

        /// <summary>
        /// Constructor</summary>
        internal BreakpointIndicator(string name, Color foreColor, Color backColor)
            : base(name, foreColor, backColor)
        {
           
        }

        #region IBreakpoint Members       
        /// <summary>
        /// Gets starting offset of the text range for this instance</summary>
        public int StartOffset
        {
            get { return TextRange.StartOffset; }
        }

        /// <summary>
        /// Gets end offset of the text range for this instance</summary>
        public int EndOffset
        {
            get { return TextRange.EndOffset; }
        }

        /// <summary>
        /// Gets the line number on which this instance is set.
        /// If this instance has been removed, this method returns
        /// the last known line number.</summary>
        public int LineNumber
        {
            get
            {
                if (Layer != null)
                {                   
                    foreach (DocumentLine line in Layer.Document.Lines)
                    {
                        if (line.TextRange == TextRange)
                        {
                            m_lineNumber = line.Index + 1;
                            break;
                        }
                    }
                }
                return m_lineNumber;
            }
        }
        /// <summary>
        /// Gets or sets whether to draw a marker on this breakpoint.
        /// This marker is used to identify a conditional breakpoint from a regular breakpoint.</summary>
        public bool Marker
        {
            get { return m_marker; }
            set { m_marker = value; }
        }
        #endregion
        /// <summary>
        /// Draws the glyph associated with the breakpoint indicator</summary>
        public override void DrawGlyph(PaintEventArgs e, Rectangle bounds)
        {
            base.DrawGlyph(e, bounds);
            if (m_marker)
            {
                float lineWidth = 0.2f * bounds.Width;
                int marginW = (int)Math.Round(0.3f * bounds.Width);
                int marginH = (int)Math.Round(0.3f * bounds.Height);
                bounds.Inflate(-marginW, -marginH);
                Pen p = new Pen(ForeColor, lineWidth);
                e.Graphics.DrawLine(p, bounds.Left, (bounds.Top + bounds.Bottom) / 2, bounds.Right, (bounds.Top + bounds.Bottom) / 2);
                e.Graphics.DrawLine(p, (bounds.Left + bounds.Right) / 2, bounds.Top, (bounds.Left + bounds.Right) / 2, bounds.Bottom);
                p.Dispose();
            }
        }

        /// <summary>
        /// Gets whether this instance has been removed</summary>
        public bool Removed
        {
            get { return Layer == null; }
        }
        private bool m_marker;
        private int m_lineNumber = -1;
    }
}
