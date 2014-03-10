//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Controls.SyntaxEditorControl
{
    /// <summary>
    /// Event arguments for MouseHoverOverToken event</summary>
    public class MouseHoverOverTokenEventArgs : EventArgs
    {
        private readonly Token m_token;
        private string m_tooltipText;
        private readonly Languages m_language;
        private readonly int m_lineNumver;

        /// <summary>
        /// Constructor with the specified arguments</summary>
        /// <param name="language">Language</param>
        /// <param name="token">Token type</param>        
        /// <param name="lineNumber">The line number of the token</param>
        public MouseHoverOverTokenEventArgs(Languages language,Token token, int lineNumber)
        {
            m_language = language;
            m_token = token;            
            m_lineNumver = lineNumber;            
        }

        /// <summary>
        /// Gets the name of language</summary>
        public Languages Language
        {
            get { return m_language; }
        }
        /// <summary>
        /// Gets the token type</summary>
        public Token Token
        {
            get { return m_token; }
        }

        /// <summary>
        /// Gets line number of the token</summary>
        public int LineNumber
        {
            get { return m_lineNumver; }
        }

        /// <summary>
        /// Gets or sets tooltip text</summary>
        public string TooltipText
        {
            get { return m_tooltipText; }
            set { m_tooltipText = value; }
        }
    
    
    }
}
