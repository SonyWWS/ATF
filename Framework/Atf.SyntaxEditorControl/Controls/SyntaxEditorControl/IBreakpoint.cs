//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;

namespace Sce.Atf.Controls.SyntaxEditorControl
{
    public interface IBreakpoint
    {
        /// <summary>
        /// Gets or sets whether to draw a marker on this breakpoint.
        /// This marker is used to identify a conditional breakpoint from a regular breakpoint.</summary>
        bool Marker
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether breakpoint is enabled</summary>
        bool Enabled
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets foreground color of breakpoint indicator</summary>
        Color ForeColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets background color of breakpoint indicator</summary>
        Color BackColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets starting offset of the text range for this instance</summary>
        int StartOffset
        {
            get;
        }

        /// <summary>
        /// Gets end offset of the text range for this instance</summary>
        int EndOffset
        {
            get;
        }

        /// <summary>
        /// Gets the line number on which this instance is set.
        /// If this instance has been removed, this method returns
        /// the last known line number.</summary>
        int LineNumber
        {
            get;
        }
    }
}
