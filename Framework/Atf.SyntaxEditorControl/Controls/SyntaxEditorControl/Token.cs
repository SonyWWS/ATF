//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Controls.SyntaxEditorControl
{
    /// <summary>
    /// Text token representing a word in a document</summary>
    public struct Token
    {
        private readonly int m_id;
        private readonly int m_startOffset;
        private readonly int m_endOffset;
        private readonly string m_tokenType;
        private readonly string m_lexeme;


        /// <summary>
        /// Constructor</summary>
        /// <param name="startOffset">Starting offset of this token, relative to the whole document</param>
        /// <param name="endOffset">Ending offset of this token, relative to the whole document</param>
        /// <param name="id">Token ID</param>
        /// <param name="tokenType">Token type</param>
        /// <param name="lexeme">Token lexeme, i.e., set of forms taken by the single word represented by this token</param>
        public Token(int startOffset, int endOffset,int id, string tokenType, string lexeme)
        {
            if (StringUtil.IsNullOrEmptyOrWhitespace(tokenType))
                throw new Exception("tokenType cannot be null or empty or whitespace");
            if (lexeme == null)
                throw new ArgumentNullException();
            m_id = id;
            m_tokenType = tokenType;
            m_lexeme = lexeme;
            m_startOffset = startOffset;
            m_endOffset = endOffset;
        }

        /// <summary>
        /// Gets token type</summary>
        public string TokenType
        {
            get { return m_tokenType; }
        }

        /// <summary>
        /// Gets token lexeme, i.e., set of forms taken by the single word represented by this token</summary>
        public string Lexeme
        {
            get { return m_lexeme; }
        }

        /// <summary>
        /// Gets token ID</summary>
        public int Id
        {
            get { return m_id; }
        }

        /// <summary>
        /// Gets starting offset for this token, relative to the whole document</summary>
        public int StartOffset
        {
            get { return m_startOffset; }
        }

        /// <summary>
        /// Gets ending offset for this token, relative to the whole document</summary>
        public int EndOffset
        {
            get { return m_endOffset; }
        }
    }
}
