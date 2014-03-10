//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;

namespace Sce.Atf
{
    /// <summary>
    /// A class to set a wait cursor and automatically restore the old cursor</summary>
    /// <example>
    /// using (new WaitCursor())
    /// {
    ///    // do some stuff
    /// }
    /// // old cursor is restored here
    /// </example>
    public class WaitCursor : IDisposable
    {
        /// <summary>
        /// Constructor</summary>
        public WaitCursor()
        {
            m_oldCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
        }

        /// <summary>
        /// Releases unmanaged resources</summary>
        public void Dispose()
        {
            Cursor.Current = m_oldCursor;
        }
        
        private readonly Cursor m_oldCursor;
    }
}


