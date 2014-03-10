//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Interface for data objects that provide context sensitive help</summary>
    internal interface IHelpContext
    {
        /// <summary>
        /// Returns an array of help lookup keys</summary>
        /// <returns>Array of help lookup keys</returns>
        string[] GetHelpKeys();
    }
}
