//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// Class for a cursor that is Freezable, i.e., has a modifiable state and a read-only (frozen) state</summary>
    public class FreezableCursor : Freezable
    {
        /// <summary>
        /// Get or set freezable cursor</summary>
        public System.Windows.Input.Cursor Cursor { get; set; }

        /// <summary>
        /// Create new FreezableCursor</summary>
        /// <returns>New FreezableCursor</returns>
        protected override Freezable CreateInstanceCore()
        {
            return new FreezableCursor();
        }
    }
}
