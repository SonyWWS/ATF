//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// An item on the status bar that displays an image</summary>
    public interface IStatusImage
    {
        /// <summary>
        /// Gets and sets the status image</summary>
        Image Image
        {
            get;
            set;
        }
    }
}


