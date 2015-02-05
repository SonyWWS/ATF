namespace Sce.Atf.Controls.SyntaxEditorControl
{
    /// <summary>
    /// Factory for text editor and related objects</summary>
    public static class TextEditorFactory
    {
        /// <summary>
        /// Creates and initializes a new SyntaxEditorControl</summary>
        /// <returns>A new SyntaxEditorControl</returns>
        public static ISyntaxEditorControl CreateSyntaxHighlightingEditor()
        {
            return new SyntaxEditorControl();
        }

        /// <summary>
        /// Creates and initializes a new SyntaxEditorFindReplaceOptions object</summary>
        /// <returns>A new SyntaxEditorFindReplaceOptions object</returns>
        public static ISyntaxEditorFindReplaceOptions CreateSyntaxEditorFindReplaceOptions()
        {
            return new SyntaxEditorFindReplaceOptions();
        }
    }
}
