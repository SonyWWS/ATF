//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Controls.SyntaxEditorControl
{
    /// <summary>
    /// Event arguments for EditorTextChanged event</summary>
    public class EditorTextChangedEventArgs : EventArgs
    {
        private readonly string m_text;
        private readonly int m_startOffset;
        private readonly int m_endOffset;
        private readonly int m_startLineNumber;
        private readonly int m_endLineNumber;

        /// <summary>
        /// Constructor</summary>
        /// <param name="text">The text that has been changed</param>
        /// <param name="startOffset">The starting offset of the changed text</param>
        /// <param name="endOffset">The end offset of the changed text</param>
        /// <param name="startLineNumber">Document starting line index at which the modification occurred</param>
        /// <param name="endLineNumber">Document ending line index at which the modification occurred</param>        
        internal EditorTextChangedEventArgs(string text, int startOffset, int endOffset, int startLineNumber, int endLineNumber)
        {
            m_text = text;
            m_startOffset = startOffset;
            m_endOffset = endOffset;
            m_startLineNumber = startLineNumber;
            m_endLineNumber = endLineNumber;
        }

        /// <summary>
        /// Gets the text that has been changed</summary>
        public string Changes
        {
            get { return m_text; }
        }

        /// <summary>
        /// Gets the starting offset of the changed text</summary>
        public int StartOffset
        {
            get { return m_startOffset; }
        }

        /// <summary>
        /// Gets the end offset of the changed text</summary>
        public int EndOffset
        {
            get { return m_endOffset; }
        }

        /// <summary>
        /// Gets the document starting line index at which the modification occurred</summary>
        public int StartLineNumber
        {
            get { return m_startLineNumber; }
        }

        /// <summary>
        /// Gets the document ending line index at which the modification occurred</summary>
        public int EndLineNumber
        {
            get { return m_endLineNumber; }
        }

        /// <summary>
        /// Formats description of text change</summary>
        /// <returns>Description of text change</returns>
        public override string ToString()
        {
            return string.Format("Change:{0}\r\nstartOffset:{1} EndOffset:{2} StartLine:{3} EndLine:{4}",
                m_text,m_startOffset, m_endOffset, m_startLineNumber, m_endLineNumber);
            
        }
    }

}
