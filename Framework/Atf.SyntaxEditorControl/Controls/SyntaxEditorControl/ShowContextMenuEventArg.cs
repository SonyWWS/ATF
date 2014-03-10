//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;

namespace Sce.Atf.Controls.SyntaxEditorControl
{
    /// <summary>
    /// Event arguments for ShowContextMenu event</summary>
    public class ShowContextMenuEventArg : EventArgs
    {
        
        private readonly int m_lineNumber = -1;
        private readonly Point m_mouseLocation;
        private readonly Point m_menuLocation;

        /// <summary>
        /// Constructor</summary>
        /// <param name="mouseLoc">Mouse location point</param>
        /// <param name="menuLoc">Point where the menu should be displayed</param>
        /// <param name="lineNumber">Line number in document</param>
        public ShowContextMenuEventArg(Point mouseLoc, Point menuLoc, int lineNumber)
        {
            m_lineNumber = lineNumber;
            m_menuLocation = menuLoc;
            m_mouseLocation = mouseLoc;
        }

        /// <summary>
        /// Gets mouse location</summary>
        public Point MouseLocation
        {
            get { return m_mouseLocation; }
        }

        /// <summary>
        /// Gets the location where the menu should be displayed</summary>
        public Point MenuLocation
        {
            get { return m_menuLocation; }
        }

        /// <summary>
        /// Gets the line number, when applicable.
        /// The value -1 means that this property is not applicable 
        /// to the current region.</summary>
        public int LineNumber
        {
            get { return m_lineNumber; }
        }
    }
}
