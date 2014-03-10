//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.SyntaxEditorControl
{
    /// <summary>
    /// Specifies the target of a hit-test operation</summary>
    public enum SyntaxEditorRegions
    {
         
        /// <summary>
        /// An unknown part of the SyntaxEditor control</summary>
        Unknown,
        
        /// <summary>
        /// A horizontal splitter between two SyntaxEditor.EditorView objects</summary>
        HorizontalSplitter,
         

        /// <summary>
        /// A vertical splitter between two SyntaxEditor.EditorView objects</summary>
        VerticalSplitter,
        
        /// <summary>
        /// A splitter between four SyntaxEditor.EditorView objects</summary>
        FourWaySplitter,
       
        /// <summary>
        /// A horizontal split button within a SyntaxEditor.EditorView</summary>
        HorizontalSplitButton,
        
        /// <summary>
        /// A vertical split button within a SyntaxEditor.EditorView</summary>
        VerticalSplitButton,

        /// <summary>
        /// A horizontal scrollbar within a SyntaxEditor.EditorView</summary>
        HorizontalScrollBar,
        
        /// <summary>
        /// A vertical scrollbar within a SyntaxEditor.EditorView</summary>
        VerticalScrollBar,
        

        /// <summary>
        /// The block that appears between the horizontal and vertical scrollbars within
        /// a SyntaxEditor.EditorView</summary>
        ScrollBarBlock,
           

        /// <summary>
        /// An EditorViewButtonLink in a SyntaxEditor.EditorView</summary>
        EditorViewButtonLink,
        
        /// <summary>
        /// The indicator margin of a SyntaxEditor.EditorView</summary>
        IndicatorMargin,
        
        /// <summary>
        /// The line number margin of a SyntaxEditor.EditorView</summary>
        LineNumberMargin,
            

        /// <summary>
        /// The user margin of a SyntaxEditor.EditorView</summary>
        UserMargin,
        
        /// <summary>
        /// The selection margin of a SyntaxEditor.EditorView</summary>
        SelectionMargin,
       
        /// <summary>
        /// The text area of a SyntaxEditor.EditorView</summary>
        TextArea,
        
        /// <summary>
        /// The word wrap margin of a SyntaxEditor.EditorView</summary>
        WordWrapMargin,
       
        /// <summary>
        /// An outlining node indicator box within the selection margin of a SyntaxEditor.EditorView</summary>
        SelectionMarginOutliningNodeIndicatorBox,
    }
}
