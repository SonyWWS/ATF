//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// An item on the status bar that displays text</summary>
    public interface IStatusText
    {
        /// <summary>
        /// Gets and sets the status text</summary>
        string Text
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the status foreground color</summary>
        Color ForeColor
        {
            get;
            set;
        }
    }
}


