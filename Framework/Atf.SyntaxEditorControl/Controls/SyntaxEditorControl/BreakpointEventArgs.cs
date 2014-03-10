//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Controls.SyntaxEditorControl
{
    /// <summary>
    /// Break point event argument for syntax editor control</summary>
    public class BreakpointEventArgs : EventArgs
    {
        private readonly bool m_isSet;
        private bool m_cancel;
        private readonly int m_lineNumber = -1;
        private readonly string m_lineText;
        
        /// <summary>
        /// Constructor with specified values</summary>
        /// <param name="isSet">True if the breakpoint set, false otherwise</param>
        /// <param name="lineNumber">Line number of the breakpoint</param>
        /// <param name="lineText">Line text of the breakpoint</param>
        public BreakpointEventArgs(bool isSet, int lineNumber, string lineText)
        {
            m_isSet = isSet;
            m_lineNumber = lineNumber;
            m_lineText = lineText;
        }

        /// <summary>
        /// Gets whether the breakpoint is set</summary>
        public bool IsSet 
        {
            get { return m_isSet; } 
        } 

        /// <summary>
        /// Gets the line number for the breakpoint</summary>
        public int LineNumber 
        {
            get { return m_lineNumber; }
        } 

        /// <summary>
        /// Gets the line text for the breakpoint</summary>
        public string LineText 
        {
            get { return m_lineText; }
        }

        /// <summary>
        /// Gets or sets whether to cancel breakpoint operation</summary>
        public bool Cancel
        {
            get { return m_cancel; }
            set { m_cancel = value; }
        } 
    }
}