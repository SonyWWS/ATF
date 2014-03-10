//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing.Printing;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for printable documents</summary>
    public interface IPrintableDocument
    {
        /// <summary>
        /// Gets a PrintDocument to work with the standard Windows print dialogs</summary>
        /// <returns>PrintDocument to work with the standard Windows print dialogs</returns>
        PrintDocument GetPrintDocument();
    }
}
