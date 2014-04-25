//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

//#define FullISyntaxEditorControl

using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

#if (FullISyntaxEditorControl)
using System.Collections.Generic;
using System.IO;
#endif

//
// SyntaxEditorControl License Usage
// Patrick O'Leary (March 15th, 2011)
//
// The ActiproSoftware .dll's contain tons of classes and items we don't use
// so we would never go over the limit specified in the license. However,
// to get a feel for how much of the ActiproSoftare SyntaxEditor we are using
// I wrote a function to do some counting based on reflection. (Our
// ISyntaxEditorControl interface that's exposed to the clients directly
// references items from the SyntaxEditor class so that's why it was used).
//
// I have a function in the SyntaxEditorControl class called PerformAndShowAuditStuff()
// Simply uncomment the line calling it from the SyntaxEditorControl constructor
// and then run a sample like the CodeEditor and open a document. You'll get a
// message box with a percentage. This is an approximate value as it doesn't include
// the find/replace wrapper classes we've created and expose to clients. The current
// percentage from running this function as of March 15th, 2011 is 29.17647%.
//

namespace Sce.Atf.Controls.SyntaxEditorControl
{
    /// <summary>
    /// Method called when KeyPress occurs</summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">KeyPressEventArgs arguments containing event data</param>
    public delegate void OnKeyPressEventHandler(object sender, KeyPressEventArgs e);

    /// <summary>
    /// Interface for syntax aware editor controls</summary>
    public interface ISyntaxEditorControl
    {
        /// <summary>
        /// Gets underlying control</summary>
        Control Control
        {
            get;
        }
        /// <summary>
        /// Gets and sets the control's text</summary>
        string Text
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets whether the document has a vertical splitter</summary>
        bool VerticalSplitter
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets whether the document has a horizontal splitter</summary>
        bool HorizontalSplitter
        {
            get;
            set;
        }

        /// <summary>
        ///  Event raised when a key is pressed</summary>
        event OnKeyPressEventHandler OnKeyPressEvent;

        /// <summary>
        /// Event that is raised after the control's text has changed</summary>
        event EventHandler<EditorTextChangedEventArgs> EditorTextChanged;
              
        /// <summary>
        /// Event that is raised after the ReadOnly property changes</summary>
        event EventHandler ReadOnlyChanged;

        /// <summary>
        /// Event that is raised when the context menu should be displayed by right clicking
        /// on the SyntaxEditor control</summary>
        event EventHandler<ShowContextMenuEventArg> ShowContextMenu;

        /// <summary>
        /// Event that is raised after an alpha-numerical key is pressed</summary>
        event KeyPressEventHandler KeyPress;
       
        /// <summary>
        /// Gets and sets whether to enable a default context menu</summary>
        /// <remarks>Set this property to false when implementing a custom context menu.
        /// Default value is true.</remarks>
        bool DefaultContextMenuEnabled
        {
            get;
            set;
        }
       
        /// <summary>
        /// Gets and sets whether to enable word wrap</summary>
        bool EnableWordWrap
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets whether the content is read-only</summary>
        bool ReadOnly
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets whether the control's text is dirty</summary>
        bool Dirty
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the current selection's text</summary>
        string Selection
        {
            get;
        }

        /// <summary>
        /// Gets whether the selection is not null or an empty string</summary>
        bool HasSelection
        {
            get;
        }

        /// <summary>
        /// Event raised when the selection changes</summary>
        event EventHandler SelectionChanged;

        /// <summary>
        /// Gets whether the editor can paste</summary>
        bool CanPaste
        {
            get;
        }

        /// <summary>
        /// Gets whether the editor can undo</summary>
        bool CanUndo
        { 
            get; 
        }

        /// <summary>
        /// Gets whether the editor can redo</summary>
        bool CanRedo
        {
            get;
        }

        /// <summary>
        /// Gets the number of document lines</summary>
        int DocumentLineCount
        {
            get;
        }

        /// <summary>
        /// Gets and sets current line number</summary>
        int CurrentLineNumber
        {
            get;
            set;            
        }

        /// <summary>
        /// Event raised when the CurrentLineNumber property changes</summary>
        event EventHandler CurrentLineNumberChanged;

        /// <summary>
        /// Gets and sets the caret offset</summary>
        int CurrentOffset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets substring</summary>
        /// <param name="startOffset">Start offset of the substring</param>
        /// <param name="count">Length of the substring in characters</param>
        /// <returns>Specified substring</returns>
        string GetSubString( int startOffset, int count );

        /// <summary>
        /// Gets caret position in client coordinates</summary>
        Point CaretPosition
        {
            get;
        }
       
        /// <summary>
        /// Selects line, given a line number.
        /// This method throws ArgumentOutOfRangeException
        /// if the line number is out of range.</summary>
        /// <param name="lineNumber">The line number to be selected</param>
        void SelectLine(int lineNumber);

        /// <summary>
        /// Selects line, given line number</summary>
        /// <remarks>Selection is [startOffset, endOffset] inclusive</remarks>
        /// <param name="lineNumber">The line number to be selected</param>
        /// <param name="startOffset">Beginning offset of the selection; starting index is zero</param>
        /// <param name="endOffset">Ending offset of selection</param>
        void SelectLine(int lineNumber, int startOffset, int endOffset);

        /// <summary>
        /// Gets the text for the given line number</summary>
        /// <param name="lineNumber">The line number</param>
        /// <returns>Text of the given line</returns>
        /// <remarks>If the line is empty, an empty string is returned;
        /// this method does not return null.
        /// If the line number is out of range, the ArgumentOutOfRangeException
        /// exception is thrown.</remarks>
        string GetLineText(int lineNumber);
                    
        /// <summary>
        /// Cuts the selection and copies it to the clipboard</summary>
        void Cut();

        /// <summary>
        /// Copies the selection to the clipboard</summary>
        void Copy();

        /// <summary>
        /// Pastes the clipboard at the current selection</summary>
        void Paste();

        /// <summary>
        /// Deletes the selection</summary>
        void Delete();

        /// <summary>
        /// Undoes the last change</summary>
        void Undo();

        /// <summary>
        /// Redoes the last undone</summary>
        void Redo();

        /// <summary>
        /// Selects all text in the document</summary>
        void SelectAll();

        /// <summary>
        /// Shows the find/replace dialog</summary>
        void ShowFindReplaceForm();

        /// <summary>
        /// Shows the go to line dialog</summary>
        void ShowGoToLineForm();

        /// <summary>
        /// Sets control to one of the built-in languages</summary>
        /// <param name="language">Language to be edited in control</param>
        void SetLanguage(Languages language);

        /// <summary>
        /// Displays a print preview dialog for the document</summary>
        void PrintPreview();

        /// <summary>
        /// Prints the current document to the printer</summary>
        /// <param name="showDialog">Whether to show the standard Print dialog 
        /// before printing</param>
        void Print(bool showDialog);

        /// <summary>
        /// Releases all resources used by this component</summary>
        void Dispose();

        /// <summary>
        /// Gets and sets multiline property</summary>
        bool Multiline
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets whether the line number margin is visible</summary>
        bool LineNumberMarginVisible
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets whether the indicator margin is visible</summary>
        bool IndicatorMarginVisible
        {
            get;
            set;
        }

        /// <summary>
        /// Clears the IntelliPrompt member collection</summary>
        void ClearIntelliPromptMemberList();

        /// <summary>
        /// Adds an item to the IntelliPrompt member collection</summary>
        /// <param name="item">Item to add</param>
        void AddIntelliPromptMemberListItem(string item);

        /// <summary>
        /// Adds an item to the IntelliPrompt member collection</summary> 
        /// <param name="item">Item to add</param>
        /// <param name="description">Item's description</param>
        /// <param name="autoCompletePreText">Text before auto completion</param>
        /// <param name="autoCompletePostText">Text after auto completion</param>
        void AddIntelliPromptMemberListItem(string item, string description, string autoCompletePreText, string autoCompletePostText);

        /// <summary>
        /// Shows the IntelliPrompt member list</summary>
        void ShowIntelliPromptMemberList();
        
        /// <summary>
        /// Shows the IntelliPrompt member list</summary>
        /// <param name="offset">Offset of list</param>
        /// <param name="length">List length</param>
        void ShowIntelliPromptMemberList(int offset, int length);

        // please notify ATF team before defining  FullISyntaxEditorControl
#if (FullISyntaxEditorControl)

        /// <summary>
        /// Gets list of IBreakpoints</summary>        
        IBreakpoint[] GetBreakpoints();

        /// <summary>
        /// Sets breakpoint for specified lines</summary>
        /// <param name="list">List of lines</param>
        void SetBreakpoints(IList<int> list);

        /// <summary>
        /// Gets IBreakpoint for a given line number</summary>
        /// <param name="lineNumber">Line number</param>
        /// <returns>IBreakpoint or null if there is none for given line</returns>
        IBreakpoint GetBreakpoint(int lineNumber);

        /// <summary>
        /// Clears all breakpoints for the current document</summary>
        void ClearAllBreakpoints();
        
        /// <summary>
        /// Toggles break point for the current line</summary>
        void ToggleBreakpoint();

        /// <summary>
        /// Sets or unsets breakpoint for a given line</summary>
        /// <param name="lineNumber">The line to set or unset breakpoint</param>
        /// <param name="set">True to set break point, false to unset it</param>
        /// <remarks>
        /// Setting an already set breakpoint will not have any side effect.
        /// Unsetting an already unset breakpoint will not have any side effect.
        /// This method throws ArgumentOutOfRangeException if line number is out of range.</remarks>
        void Breakpoint(int lineNumber, bool set);

        /// <summary>
        /// Sets/unsets current-statement indicator for the specified line</summary>
        /// <param name="lineNumber">Line to set/unset statement indicator</param>
        /// <param name="set">True to set statement indicator, false to unset it</param>
        /// <remarks>
        /// Setting an already set current-statement indicator will not have any side effect.
        /// Unsetting an already unsetted current-statement indicator will not have any side effect.
        /// This method throws ArgumentOutOfRangeException if line number is out of range.</remarks>
        void CurrentStatement(int lineNumber, bool set);

         /// <summary>
        /// Gets or sets list of current-statement-indicators' line number</summary>
        IList<int> CurrentStatements
        {
            get;
            set;
        }

         /// <summary>
        /// Event that is raised after a new breakpoint added</summary>
        event EventHandlerNC<IBreakpoint> BreakpointAdded;

        /// <summary>
        /// Event that is raised after an existing breakpoint is removed</summary>
        event EventHandlerNC<IBreakpoint> BreakpointRemoved;

        /// <summary>
        /// Event that is raised before the break point is changed</summary>
        event EventHandler<BreakpointEventArgs> BreakpointChanging;
        /// <summary>
        /// Event that is raised after the mouse hovers over a token</summary>
        event EventHandler<MouseHoverOverTokenEventArgs> MouseHoveringOverToken;

        /// <summary>
        /// Gets the current language</summary>
        string CurrentLanguageId
        {
            get;
        }

        /// <summary>
        /// Returns the SyntaxEditor region for the given point</summary>
        /// <param name="location">Point</param>
        /// <returns>SyntaxEditor region</returns>
        /// <remarks>
        /// A mouse location can be passed, then the region under 
        /// the mouse pointer is returned.</remarks>
        SyntaxEditorRegions GetRegion(Point location);

        /// <summary>
        /// Gets all the tokens for the current line</summary>
        /// <param name="lineNumber">Line number</param>
        /// <returns>An array of tokens</returns>
        Token[] GetTokens(int lineNumber);

        /// <summary>
        /// Sets control to a custom language</summary>
        /// <param name="stream">XML stream containing the syntax rules for the language</param>
        void SetLanguage(Stream stream);
#endif

        /// <summary>
        /// Returns line number from offset</summary>
        /// <param name="offset">Offset in the document</param>
        /// <returns>Line number of offset</returns>
        int GetLineFromOffset(int offset);

        /// <summary>
        /// Gets or sets the selected range of text in the document</summary>
        ISyntaxEditorTextRange SelectRange
        {
            get;
            set;
        }

        /// <summary>Gets a word text range starting at an offset in the document</summary>
        /// <param name="offset">Offset in the document</param>
        /// <returns>Word range based on offset</returns>
        ISyntaxEditorTextRange GetWordTextRange(int offset);

        /// <summary>
        /// Clears any bookmarks that have been placed due to a MarkAll</summary>
        void ClearSpanIndicatorMarks();

        /// <summary>
        /// Finds text instance based on options starting at offset</summary>
        /// <param name="options">Find/replace options</param>
        /// <param name="startOffset">Offset in the document at which to start find operation</param>
        /// <returns>Find/replace result set</returns>
        ISyntaxEditorFindReplaceResultSet Find(ISyntaxEditorFindReplaceOptions options, int startOffset);
        /// <summary>
        /// Finds all instances of text based on options in entire document</summary>
        /// <param name="options">Find/replace options</param>
        /// <returns>Find/replace result set</returns>
        ISyntaxEditorFindReplaceResultSet FindAll(ISyntaxEditorFindReplaceOptions options);
        /// <summary>
        /// Finds all instances of text based on options in text range</summary>
        /// <param name="options">Find/replace options</param>
        /// <param name="textRange">Text range to search</param>
        /// <returns>Find/replace result set</returns>
        ISyntaxEditorFindReplaceResultSet FindAll(ISyntaxEditorFindReplaceOptions options, ISyntaxEditorTextRange textRange);
        /// <summary>
        /// Marks all instances of text based on options in entire document</summary>
        /// <param name="options">Find/replace options</param>
        /// <returns>Find/replace result set</returns>
        ISyntaxEditorFindReplaceResultSet MarkAll(ISyntaxEditorFindReplaceOptions options);
        /// <summary>
        /// Marks all instances of text based on options in text range</summary>
        /// <param name="options">Find/replace options</param>
        /// <param name="textRange">Text range to search</param>
        /// <returns>Find/replace result set</returns>
        ISyntaxEditorFindReplaceResultSet MarkAll(ISyntaxEditorFindReplaceOptions options, ISyntaxEditorTextRange textRange);
        /// <summary>
        /// Replaces text based on options in previously found text indicated by a ISyntaxEditorFindReplaceResult</summary>
        /// <param name="options">Find/replace options</param>
        /// <param name="result">Previously found text in which to do the replace</param>
        /// <returns>Find/replace result set</returns>
        ISyntaxEditorFindReplaceResultSet Replace(ISyntaxEditorFindReplaceOptions options, ISyntaxEditorFindReplaceResult result);
        /// <summary>
        /// Replaces all text based on options in entire document</summary>
        /// <param name="options">Find/replace options</param>
        /// <returns>Find/replace result set</returns>
        ISyntaxEditorFindReplaceResultSet ReplaceAll(ISyntaxEditorFindReplaceOptions options);
        /// <summary>
        /// Replaces all text based on options in text range</summary>
        /// <param name="options">Find/replace options</param>
        /// <param name="textRange">Text range in which to replace text</param>
        /// <returns>Find/replace result set</returns>
        ISyntaxEditorFindReplaceResultSet ReplaceAll(ISyntaxEditorFindReplaceOptions options, ISyntaxEditorTextRange textRange);
    }
    
    /// <summary>
    /// Enum for type of search</summary>
    public enum SyntaxEditorFindReplaceSearchType
    {
        /// <summary>
        /// Search for text</summary>
        Normal,
        /// <summary>
        /// Search for regular expression</summary>
        RegularExpression,
        /// <summary>
        /// Search for text with wildcard(s)</summary>
        Wildcard,
    }

    /// <summary>
    /// Interface for find replace options</summary>
    public interface ISyntaxEditorFindReplaceOptions
    {
        /// <summary>
        /// Gets and sets whether to change the selection</summary>
        bool ChangeSelection
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the text to find</summary>
        string FindText
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets whether to match case</summary>
        bool MatchCase
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets whether to match whole words</summary>
        bool MatchWholeWord
        {
            get;
            set;
        }

        /// <summary>
        /// Gets whether text should be modifed</summary>
        bool Modified
        {
            get;
        }

        /// <summary>
        /// Gets and sets whether to replace text</summary>
        string ReplaceText
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets whether to search in hidden text</summary>
        bool SearchHiddenText
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets whether to search in selected text only</summary>
        bool SearchInSelection
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets whether to search backwards</summary>
        bool SearchUp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the search type</summary>
        SyntaxEditorFindReplaceSearchType SearchType
        {
            get;
            set;
        }

        /// <summary>
        /// Disposes of resources</summary>
        void Dispose();
    }

    /// <summary>
    /// Interface for find replace result</summary>
    public interface ISyntaxEditorFindReplaceResult
    {
        /// <summary>
        /// Gets text found</summary>
        string Text
        {
            get;
        }

        /// <summary>
        /// Gets starting offset of found text in document</summary>
        int StartOffset
        {
            get;
        }

        /// <summary>
        /// Gets ending offset of found text in document</summary>
        int EndOffset
        {
            get;
        }
    }

    /// <summary>
    /// Interface for set of find replace results</summary>
    public interface ISyntaxEditorFindReplaceResultSet
    {
        /// <summary>
        /// Gets whether search went past end of document</summary>
        bool PastDocumentEnd
        {
            get;
        }

        /// <summary>
        /// Gets whether search went past end of starting offset</summary>
        bool PastSearchStartOffset
        {
            get;
        }

        /// <summary>
        /// Gets whether text replacement occurred</summary>
        bool ReplaceOccurred
        {
            get;
        }

        /// <summary>
        /// Gets result from array of find replace results</summary>
        /// <param name="index">Index of a find replace result</param>
        /// <returns>Find replace result at index</returns>
        ISyntaxEditorFindReplaceResult this[int index]
        {
            get;
        }

        /// <summary>
        /// Gets count of find replace results</summary>
        int Count
        {
            get;
        }

        /// <summary>
        /// Gets enumerator for collection of find replace results</summary>
        /// <returns>enumerator for collection of find replace results</returns>
        IEnumerator GetEnumerator();
    }

    /// <summary>
    /// Interface for text range in document</summary>
    public interface ISyntaxEditorTextRange
    {
        /// <summary>
        /// Gets starting offset</summary>
        int StartOffset
        {
            get;
        }

        /// <summary>
        /// Gets ending offset</summary>
        int EndOffset
        {
            get;
        }
    }

    /// <summary>
    /// Document text range class implementing ISyntaxEditorTextRange</summary>
    public class SyntaxEditorTextRange : ISyntaxEditorTextRange
    {
        /// <summary>
        /// Constructor with starting and ending offset</summary>
        /// <param name="startOffset">Starting offset</param>
        /// <param name="endOffset">Ending offset</param>
        public SyntaxEditorTextRange(int startOffset, int endOffset)
        {
            m_startOffset = startOffset;
            m_endOffset = endOffset;
        }

        /// <summary>
        /// Gets starting offset</summary>
        public int StartOffset
        {
            get { return m_startOffset; }
        }

        /// <summary>
        /// Gets ending offset</summary>
        public int EndOffset
        {
            get { return m_endOffset; }
        }

        private readonly int m_startOffset;
        private readonly int m_endOffset;
    }

    /// <summary>
    /// Represents the method that will handle an event. The generic type argument
    /// specifies the type of the event data generated by the event. 
    /// There is no constraint on the TEventArgs.</summary>
    /// <typeparam name="TEventArgs">Type of the event data generated by the event</typeparam>
    /// <param name="sender">Event sender</param>
    /// <param name="e">Event arguments containing event data</param>
    [Serializable]
    public delegate void EventHandlerNC<TEventArgs>(object sender, TEventArgs e);
}
