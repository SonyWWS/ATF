//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;
using System.Drawing.Printing;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Interface for control adapters that perform printing, i.e., drawing
    /// print documents to arbitrary graphics devices</summary>
    public interface IPrintingAdapter
    {
        /// <summary>
        /// Prints the document to the graphics device</summary>
        /// <param name="printDocument">Print document, containing information about
        /// what to print.</param>
        /// <param name="g">Graphics device to use for drawing</param>
        void Print(PrintDocument printDocument, Graphics g);
    }
}
