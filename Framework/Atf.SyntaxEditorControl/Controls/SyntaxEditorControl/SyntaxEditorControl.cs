//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using ActiproSoftware.SyntaxEditor;
using ActiproSoftware.SyntaxEditor.Addons.Dynamic;

namespace Sce.Atf.Controls.SyntaxEditorControl
{
    /// <summary>
    /// This class encapsulates the Actipro SyntaxEditor control, implementing ISyntaxEditorControl</summary>
    /// <remarks> This class must remain internal to abide to SyntaxEditor licensing terms.
    /// Please do not remove internal keyword.</remarks>
    internal class SyntaxEditorControl : SyntaxEditor, ISyntaxEditorControl
    {
        private readonly FindReplaceForm m_findReplaceForm;
        private readonly FindReplaceOptions m_findReplaceOptions;
        private static ImageList s_imgList;

        /// <summary>
        /// Constructor</summary>
        public SyntaxEditorControl()
        {
            // Uncomment the next line and use something like
            // CodeEditor to open a document and see these results.
            //PerformAndShowAuditStuff();

            var doc = new Document { LineModificationMarkingEnabled = true };
            doc.Outlining.Mode = OutliningMode.Automatic;

            Location = new Point(0, 0);
            Name = "editor";
            base.AllowDrop = false;
            Document = doc;
            DefaultContextMenuEnabled = true;
            IntelliPrompt.DropShadowEnabled = true;
            IntelliPrompt.SmartTag.ClearOnDocumentModification = true;
            IntelliPrompt.SmartTag.MultipleSmartTagsEnabled = false;
            LineNumberMarginVisible = true;
            IndentType = IndentType.Smart;
            BracketHighlightingVisible = true;
            SplitType = SyntaxEditorSplitType.FourWay;

            DocumentIndicatorRemoved += EditorDocumentIndicatorRemoved;
            SmartIndent += EditorSmartIndent;
            DocumentSyntaxLanguageLoaded += EditorDocumentSyntaxLanguageLoaded;
            DocumentIndicatorAdded += EditorDocumentIndicatorAdded;
            KeyTyping += EditorKeyTyping;
            ViewMouseHover += EditorViewMouseHover;
            DocumentTextChanged += EditorDocumentTextChanged;
            ContextMenuRequested += EditorContextMenuRequested;
            PasteDragDrop += EditorPasteDragDrop;
            TriggerActivated += EditorTriggerActivated;
            ViewMouseDown += EditorViewMouseDown;
            base.SelectionChanged += SyntaxEditorControlSelectionChanged;

            m_findReplaceOptions = new FindReplaceOptions();
            m_findReplaceForm = new FindReplaceForm(this, m_findReplaceOptions);

            if (s_imgList == null)
                s_imgList = ReflectionImageList;

            // Added by PJO
            KeyUp += SyntaxEditorControlKeyUp;
            MouseUp += SyntaxEditorControlMouseUp;

            Renderer = new VisualStudio2005SyntaxEditorRenderer();
        }

        /// <summary>
        /// Processes a command key</summary>
        /// <param name="msg">A <see cref="T:System.Windows.Forms.Message"></see>, passed by reference, that represents the Win32 message to process</param>
        /// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys"></see> values that represents the key to process</param>
        /// <returns><c>True</c> if the keystroke was processed and consumed by the control; otherwise, false to allow further processing</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (KeysUtil.IsTextBoxInput(this, keyData))
                return false;
            return base.ProcessCmdKey(ref msg, keyData);
        }

        #region Event Handlers

        private void EditorContextMenuRequested(object sender, ContextMenuRequestEventArgs e)
        {
            SyntaxEditorHitTestResult ht = HitTest(e.MouseLocation);
            if (ht == null)
                return;
            EventHandler<ShowContextMenuEventArg> handler = ShowContextMenu;
            if (handler != null)
            {
                int lineIndex = (ht.DocumentLine != null) ? ht.DocumentLine.Index + 1 : -1;
                handler(this, new ShowContextMenuEventArg(e.MouseLocation, e.MenuLocation, lineIndex));
            }
        }

        private void EditorDocumentIndicatorAdded(object sender, IndicatorEventArgs e)
        {
            var breakpointIndicator = e.Indicator as BreakpointIndicator;
            if (breakpointIndicator != null && BreakpointAdded != null)
                BreakpointAdded(this, breakpointIndicator);
        }

        private void EditorDocumentIndicatorRemoved(object sender, IndicatorEventArgs e)
        {
            var breakpointIndicator = e.Indicator as BreakpointIndicator;
            if (breakpointIndicator != null && BreakpointRemoved != null)
                BreakpointRemoved(this, breakpointIndicator);
        }

        private void EditorDocumentSyntaxLanguageLoaded(object sender, SyntaxLanguageEventArgs e)
        {
            var editor = sender as SyntaxEditor;
            if (!(e.Language is ActiproSoftware.SyntaxEditor.Addons.CSharp.CSharpSyntaxLanguage))
            {
                // Clear the language data
                if (editor != null)
                    editor.Document.LanguageData = null;
            }
        }

        private void EditorDocumentTextChanged(object sender, DocumentModificationEventArgs e)
        {
            if (EditorTextChanged == null)
                return;

            string str = null;
            int startOffset = e.Modification.StartOffset;
            int endoffset = -1;
            int startLine = e.Modification.StartLineIndex + 1;
            int endLine = -1;
            if (!String.IsNullOrEmpty(e.Modification.InsertedText))
            {
                endoffset = e.Modification.InsertionEndOffset;
                str = e.Modification.InsertedText;
                endLine = e.Modification.InsertionEndLineIndex + 1;

            }
            else if (!String.IsNullOrEmpty(e.Modification.DeletedText))
            {
                endoffset = e.Modification.DeletionEndOffset;
                str = e.Modification.DeletedText;
                endLine = e.Modification.DeletionEndLineIndex + 1;
            }

            EditorTextChanged(this, new EditorTextChangedEventArgs(str, startOffset, endoffset, startLine, endLine));
        }

        private void EditorKeyTyping(object sender, KeyTypingEventArgs e)
        {
            var editor = sender as SyntaxEditor;
            if (editor == null)
                return;

            if (e.KeyChar != '(')
                return;

            // Get the offset
            int offset = editor.SelectedView.Selection.EndOffset;

            // Get the text stream
            TextStream stream = editor.Document.GetTextStream(offset);

            // Get the language
            SyntaxLanguage language = stream.Token.Language;

            // If in C#...
            if ((!(language is DynamicSyntaxLanguage)) || (language.Key != "C#"))
                return;

            if ((offset < 10) || (editor.Document.GetSubstring(offset - 10, 10) != "Invalidate"))
                return;

            // Show parameter info
            editor.IntelliPrompt.ParameterInfo.Info.Clear();
            editor.IntelliPrompt.ParameterInfo.Info.Add(@"void <b>Control.Invalidate</b>()<br/>" +
                                                        @"Invalidates the entire surface of the control and causes the control to be redrawn.");
            editor.IntelliPrompt.ParameterInfo.Info.Add(@"void Control.Invalidate(<b>System.Drawing.Rectangle rc</b>, bool invalidateChildren)<br/>" +
                                                        @"<b>rc:</b> A System.Drawing.Rectangle object that represents the region to invalidate.");
            editor.IntelliPrompt.ParameterInfo.Show(offset - 10);
        }

        private void EditorPasteDragDrop(object sender, PasteDragDropEventArgs e)
        {
            var editor = sender as SyntaxEditor;
            if (editor == null)
                return;

            // Allow file name drops from Windows Explorer
            if (e.DataObject.GetDataPresent(DataFormats.FileDrop))
            {
                object files = e.DataObject.GetData(DataFormats.FileDrop);
                if ((files is string[]) && (((string[])files).Length > 0))
                {
                    string filename = ((string[])files)[0];

                    // If performing a drop of a .snippet file, see if it contains any code snippets and if so, 
                    //    activate the first one that is found
                    if ((e.Source == PasteDragDropSource.DragDrop) && (Path.GetExtension(filename).ToLower() == ".snippet"))
                    {
                        CodeSnippet[] codeSnippets = CodeSnippet.LoadFromXml(filename);
                        if (codeSnippets.Length > 0)
                        {
                            e.Text = String.Empty;
                            editor.IntelliPrompt.CodeSnippets.Activate(codeSnippets[0]);
                            return;
                        }
                    }
                }
            }
        }

        private void EditorSmartIndent(object sender, SmartIndentEventArgs e)
        {
            var editor = sender as SyntaxEditor;
            if (editor == null)
                return;

            if (!(editor.Document.Language is DynamicSyntaxLanguage))
                return;

            // Increment indent if pressing ENTER after a curly brace
            switch (editor.Document.Language.Key)
            {
                case "C#":
                case "Java":
                case "JScript":
                case "PHP":
                    {
                        TextStream stream = editor.Document.GetTextStream(editor.SelectedView.Selection.FirstOffset);
                        bool exitLoop = false;
                        while (stream.Offset > 0)
                        {
                            stream.GoToPreviousToken();
                            switch (stream.Token.Key)
                            {
                                case "WhitespaceToken":
                                    // Ignore whitespace
                                    break;
                                case "OpenCurlyBraceToken":
                                    e.IndentAmount++;
                                    exitLoop = true;
                                    break;
                                default:
                                    exitLoop = true;
                                    break;
                            }
                            if (exitLoop)
                                break;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Occurs after a trigger is activated</summary>
        private void EditorTriggerActivated(object sender, TriggerEventArgs e)
        {
            var editor = sender as SyntaxEditor;
            if (editor == null)
                return;

            switch (editor.Document.Language.Key)
            {
                case "C#":
                    {
                        switch (e.Trigger.Key)
                        {
                            case "MemberListTrigger":
                                {
                                    // Construct full name of item to see if reflection can be used... iterate backwards through the token stream
                                    TokenStream stream = editor.Document.GetTokenStream(editor.Document.Tokens.IndexOf(
                                        editor.SelectedView.Selection.EndOffset - 1));
                                    string fullName = String.Empty;
                                    int periods = 0;
                                    while (stream.Position > 0)
                                    {
                                        IToken token = stream.ReadReverse();
                                        switch (token.Key)
                                        {
                                            case "IdentifierToken":
                                            case "NativeTypeToken":
                                                fullName = editor.Document.GetTokenText(token) + fullName;
                                                break;
                                            case "PunctuationToken":
                                                if ((token.Length == 1) && (editor.Document.GetTokenText(token) == "."))
                                                {
                                                    fullName = editor.Document.GetTokenText(token) + fullName;
                                                    periods++;
                                                }
                                                else
                                                    stream.Position = 0;
                                                break;
                                            default:
                                                stream.Position = 0;
                                                break;
                                        }
                                    }

                                    // Convert common types
                                    if ((fullName.Length > 0) && (periods == 0))
                                    {
                                        switch (fullName)
                                        {
                                            case "bool":
                                                fullName = "System.Boolean";
                                                break;
                                            case "byte":
                                                fullName = "System.Byte";
                                                break;
                                            case "char":
                                                fullName = "System.Char";
                                                break;
                                            case "decimal":
                                                fullName = "System.Decimal";
                                                break;
                                            case "double":
                                                fullName = "System.Double";
                                                break;
                                            case "short":
                                                fullName = "System.Int16";
                                                break;
                                            case "int":
                                                fullName = "System.Int32";
                                                break;
                                            case "long":
                                                fullName = "System.Int64";
                                                break;
                                            case "object":
                                                fullName = "System.Object";
                                                break;
                                            case "sbyte":
                                                fullName = "System.SByte";
                                                break;
                                            case "float":
                                                fullName = "System.Single";
                                                break;
                                            case "string":
                                                fullName = "System.String";
                                                break;
                                            case "ushort":
                                                fullName = "System.UInt16";
                                                break;
                                            case "uint":
                                                fullName = "System.UInt32";
                                                break;
                                            case "ulong":
                                                fullName = "System.UInt64";
                                                break;
                                            case "void":
                                                fullName = "System.Void";
                                                break;
                                        }
                                    }

                                    // If a full name is found...
                                    if (fullName.Length > 0)
                                    {
                                        // Get the member list
                                        IntelliPromptMemberList memberList = editor.IntelliPrompt.MemberList;

                                        // Set IntelliPrompt ImageList
                                        memberList.ImageList = s_imgList;//documentOutlineTreeView.ImageList;

                                        // Add items to the list
                                        memberList.Clear();

                                        // Find a type that matches the full name
                                        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                                        foreach (Assembly assemblyData in assemblies)
                                        {
                                            Type type = assemblyData.GetType(fullName, false, false);
                                            if (type != null)
                                            {
                                                memberList.AddReflectionForTypeMembers(type, IntelliPromptTypeMemberFlags.AllMemberTypes |
                                                    IntelliPromptTypeMemberFlags.AllAccessTypes | IntelliPromptTypeMemberFlags.Static);
                                                break;
                                            }
                                        }

                                        // If no specific type was found... 
                                        if (memberList.Count == 0)
                                        {
                                            // Add namespace to examine
                                            var namespaceNames = new System.Collections.Specialized.StringCollection { fullName };

                                            // Create the array of flags for each Assembly... this generic example will assume we only
                                            //   want namespaces and types that are public
                                            var flags = new IntelliPromptNamespaceAndTypeFlags[assemblies.Length];
                                            for (int index = 0; index < flags.Length; index++)
                                                flags[index] = IntelliPromptNamespaceAndTypeFlags.NamespacesAndTypes | IntelliPromptNamespaceAndTypeFlags.Public;

                                            // Use the reflection helper method
                                            memberList.AddReflectionForAssemblyNamespacesAndTypes(assemblies, flags, namespaceNames, null, false);

                                            // Loop through the items that were created and add some descriptions
                                            foreach (IntelliPromptMemberListItem item in memberList)
                                            {
                                                if (item.ImageIndex == (int)ActiproSoftware.Products.SyntaxEditor.IconResource.Namespace)
                                                    item.Description = String.Format("namespace <b>{0}</b>", item.Tag.ToString());
                                                else if (item.Tag is Type)
                                                {
                                                    var type = (Type)item.Tag;
                                                    if (type.IsEnum)
                                                        item.Description = String.Format("enum <b>{0}</b>", type.FullName);
                                                    else if (type.IsInterface)
                                                        item.Description = String.Format("interface <b>{0}</b>", type.FullName);
                                                    else if (type.IsValueType)
                                                        item.Description = String.Format("struct <b>{0}</b>", type.FullName);
                                                    else if (type.IsSubclassOf(typeof(Delegate)))
                                                        item.Description = String.Format("delegate <b>{0}</b>", type.FullName);
                                                    else
                                                        item.Description = String.Format("class <b>{0}</b>", type.FullName);
                                                }
                                            }
                                        }

                                        // Show the list
                                        if (memberList.Count > 0)
                                            memberList.Show();
                                    }
                                    break;
                                }
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// Occurs when a mouse button is pressed over an <c>EditorView</c></summary>
        private void EditorViewMouseDown(object sender, EditorViewMouseEventArgs e)
        {
            var editor = sender as SyntaxEditor;

            switch (e.HitTestResult.Target)
            {
                case SyntaxEditorHitTestTarget.IndicatorMargin:
                    {
                        if (e.HitTestResult.DocumentLine == null || e.Button == MouseButtons.Right || e.HitTestResult.DocumentLine.Text.Trim().Length == 0)
                            return;

                        ProcessBreakpointChange(e.HitTestResult.DocumentLine, BreakpointOperation.Toggle);
                    }
                    break;

                default:
                    {
                        if (e.HitTestResult.Token != null)
                        {
                            // See if the token is a URL token
                            switch (e.HitTestResult.Token.Key)
                            {
                                case "CommentURLToken":
                                case "MultiLineCommentURLToken":
                                case "XMLCommentURLToken":
                                    {
                                        // If the CTRL key is pressed, navigate to the URL
                                        if ((e.Button == MouseButtons.Left) && (ModifierKeys == Keys.Control))
                                        {
                                            e.Cancel = true;
                                            if (editor != null)
                                                System.Diagnostics.Process.Start(editor.Document.GetTokenText(e.HitTestResult.Token));
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Occurs when the mouse is hovered over an <c>EditorView</c></summary>
        private void EditorViewMouseHover(object sender, EditorViewMouseEventArgs e)
        {
            var editor = sender as SyntaxEditor;

            if ((e.HitTestResult.Token != null) &&
                (MouseHoveringOverToken != null) &&
                (e.HitTestResult.Token.Language.Key.ToLower() != "plain text"))
            {
                Languages lang = Languages.Text;
                switch (e.HitTestResult.Token.Language.Key.ToLower())
                {
                    case "c#":
                        lang = Languages.Csharp;
                        break;
                    case "lua":
                        lang = Languages.Lua;
                        break;
                    case "python":
                        lang = Languages.Python;
                        break;
                    case "xml":
                        lang = Languages.Xml;
                        break;
                }

                if (editor != null)
                {
                    IToken itok = e.HitTestResult.Token;

                    var t = new Token(itok.StartOffset, itok.EndOffset, itok.ID, itok.Key, editor.Document.GetTokenText(itok));
                    var arg = new MouseHoverOverTokenEventArgs(lang, t, e.HitTestResult.DocumentLine.Index + 1);

                    MouseHoveringOverToken(this, arg);
                    if (!string.IsNullOrEmpty(arg.TooltipText))
                        e.ToolTipText = arg.TooltipText;
                }
            }
        }

        #endregion

        #region ISyntaxEditorControl Members

        /// <summary>
        /// Gets underlying control</summary>
        public Control Control
        {
            get { return this; }
        }

        /// <summary>
        /// Gets and sets the control's text</summary>
        public new string Text
        {
            get { return Document.Text; }
            set { Document.Text = value; }
        }

        /// <summary>
        /// Gets and sets whether the document has a vertical splitter</summary>
        public bool VerticalSplitter
        {
            get { return HasVerticalSplit; }
            set
            {
                // Already set to the current value
                if (value == HasVerticalSplit)
                    return;

                VerticalSplitPosition = HasVerticalSplit ? 0 : Width / 2;
            }
        }

        /// <summary>
        /// Gets and sets whether the document has a horizontal splitter</summary>
        public bool HorizontalSplitter
        {
            get { return HasHorizontalSplit; }
            set
            {
                // Already set to the current value
                if (value == HasHorizontalSplit)
                    return;

                HorizontalSplitPosition = HasHorizontalSplit ? 0 : Height / 2;
            }
        }

        /// <summary>
        /// Event that is raised when control text changes</summary>
        public event EventHandler<EditorTextChangedEventArgs> EditorTextChanged;

        /// <summary>
        /// Event that is raised when mouse hovering over a token</summary>
        public event EventHandler<MouseHoverOverTokenEventArgs> MouseHoveringOverToken;

#pragma warning disable 0067

        /// <summary>
        ///Event that is raised before the break point changed</summary>
        public event EventHandler<BreakpointEventArgs> BreakpointChanging;

        /// <summary>
        /// Event that is raised when the context menu should be displayed by right clicking
        /// on the SyntaxEditor control</summary>
        public event EventHandler<ShowContextMenuEventArg> ShowContextMenu;

        /// <summary>
        /// Event that is raised when a new breakpoint is added</summary>
        public event EventHandlerNC<IBreakpoint> BreakpointAdded;

        /// <summary>
        /// Event that is raised when an existing breakpoint is removed</summary>
        public event EventHandlerNC<IBreakpoint> BreakpointRemoved;

        /// <summary>
        /// Event that is raised when the read only status is changed</summary>        
        public event EventHandler ReadOnlyChanged;

        /// <summary>
        /// Gets and sets whether the content is read-only</summary>
        public bool ReadOnly
        {
            get { return Document.ReadOnly; }
            set
            {
                if (Document.ReadOnly != value)
                {
                    Document.ReadOnly = value;
                    EventHandler handler = ReadOnlyChanged;
                    if (handler != null)
                        handler(this, EventArgs.Empty);
                }
            }

        }

        /// <summary>
        /// Gets and sets whether the control's text is dirty</summary>
        public bool Dirty
        {
            get { return Document.Modified; }
            set { Document.Modified = value; }
        }

        /// <summary>
        /// Gets and sets whether to enable word wrap</summary>
        public bool EnableWordWrap
        {
            get { return WordWrap != WordWrapType.None; }
            set { WordWrap = (value) ? WordWrapType.Word : WordWrapType.None; }
        }

        /// <summary>
        /// Gets the current language</summary>
        public string CurrentLanguageId
        {
            get { return Document.Language.Key; }
        }

        /// <summary>
        /// Gets the current selection</summary>
        public string Selection
        {
            get { return SelectedView != null ? SelectedView.SelectedText : string.Empty; }
        }

        /// <summary>
        /// Gets whether the selection is not null or an empty string</summary>
        public bool HasSelection
        {
            get { return (SelectedView != null) && !string.IsNullOrEmpty(SelectedView.SelectedText); }
        }

        /// <summary>
        /// Gets whether the editor can paste</summary>
        public bool CanPaste
        {
            get { return (SelectedView != null) && SelectedView.CanPaste; }
        }

        /// <summary>
        /// Gets whether the editor can undo</summary>
        public bool CanUndo
        {
            get { return Document.UndoRedo.CanUndo; }
        }

        /// <summary>
        /// Gets whether the editor can redo</summary>
        public bool CanRedo
        {
            get { return Document.UndoRedo.CanRedo; }
        }

        /// <summary>
        /// Cuts the selection and copies it to the clipboard</summary>
        public void Cut()
        {
            if (SelectedView != null)
                SelectedView.CutToClipboard();
        }

        /// <summary>
        /// Copies the selection to the clipboard</summary>
        public void Copy()
        {
            if (SelectedView != null)
                SelectedView.CopyToClipboard();
        }

        /// <summary>
        /// Pastes the clipboard at the current selection</summary>
        public void Paste()
        {
            if (SelectedView != null)
                SelectedView.PasteFromClipboard();
        }

        /// <summary>
        /// Deletes the selection</summary>
        public void Delete()
        {
            if (SelectedView != null)
                SelectedView.Delete();
        }

        /// <summary>
        /// Undoes the last change</summary>
        public void Undo()
        {
            if (Document.UndoRedo.CanUndo)
                Document.UndoRedo.Undo();
        }

        /// <summary>
        /// Redoes the last undone</summary>
        public void Redo()
        {
            if (Document.UndoRedo.CanRedo)
                Document.UndoRedo.Redo();
        }

        /// <summary>
        /// Selects all text in the document</summary>
        public void SelectAll()
        {
            if (SelectedView != null)
                SelectedView.Selection.SelectAll();
        }

        /// <summary>
        /// Shows the find/replace dialog</summary>
        public void ShowFindReplaceForm()
        {
            if (m_findReplaceForm.Visible)
                m_findReplaceForm.Activate();
            else
                m_findReplaceForm.Show();
        }

        /// <summary>
        /// Shows the go to line dialog</summary>
        public void ShowGoToLineForm()
        {
            ShowGoToLineForm(Form.ActiveForm);
        }

        /// <summary>
        /// Gets the number of document lines</summary>
        public int DocumentLineCount
        {
            get { return Document.Lines.Count; }
        }

        /// <summary>
        /// Event raised when the CurrentLineNumber property changes</summary>
        public event EventHandler CurrentLineNumberChanged;

        /// <summary>
        /// Gets and sets current line number</summary>
        public int CurrentLineNumber
        {
            get { return SelectedView.CurrentDocumentLine.Index + 1; }
            set
            {
                // check if the value is within the ranges.
                if (value < 1 || value > Document.Lines.Count)
                    throw new ArgumentOutOfRangeException();

                SelectedView.GoToLine(value - 1);
                OnCurrentLineNumberChanged();
            }
        }

        private void SyntaxEditorControlKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up: break;
                case Keys.Down: break;
                case Keys.PageDown: break;
                case Keys.PageUp: break;
                case Keys.Home: break;
                case Keys.End: break;
                default: return;
            }

            OnCurrentLineNumberChanged();
        }

        private void SyntaxEditorControlMouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left: break;
                case MouseButtons.Right: break;
                default: return;
            }

            OnCurrentLineNumberChanged();
        }

        private void OnCurrentLineNumberChanged()
        {
            int iCurrentLineNumber = CurrentLineNumber;
            if (iCurrentLineNumber != m_iPrevCurrentLineNumber)
            {
                // Fire event
                EventHandler handler = CurrentLineNumberChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }

            m_iPrevCurrentLineNumber = iCurrentLineNumber;
        }

        private int m_iPrevCurrentLineNumber;

        /// <summary>
        /// Gets or sets the caret offset</summary>
        public int CurrentOffset
        {
            get { return Caret.Offset; }
            set { Caret.Offset = value; }
        }

        /// <summary>
        /// Gets substring</summary>
        /// <param name="startOffset">Start offset of the substring</param>
        /// <param name="count">Length of the substring in characters</param>
        /// <returns>Specified substring</returns>
        public string GetSubString(int startOffset, int count)
        {
            return Document.GetSubstring(startOffset, count);
        }

        /// <summary>
        /// Gets caret position in client coordinates</summary>
        public Point CaretPosition
        {
            get
            {
                Rectangle bound = SelectedView.GetCharacterBounds(Caret.EditPosition);
                return bound.Location;
            }
        }

        /// <summary>
        /// Returns the SyntaxEditor region for the given point</summary>
        /// <param name="location">Point</param>
        /// <returns>SyntaxEditor region</returns>
        /// <remarks>
        /// A mouse location can be passed, and then the region under 
        /// the mouse pointer is returned.</remarks>
        public SyntaxEditorRegions GetRegion(Point location)
        {
            SyntaxEditorHitTestResult ht = HitTest(location);
            return SyntaxEditorHitTestTargetToSyntaxEditorRegions(ht.Target);
        }

        /// <summary>
        /// Selects line, given a line number.
        /// This method throws ArgumentOutOfRangeException
        /// if the line number is out of range.</summary>
        /// <param name="lineNumber">The line number to be selected</param>
        public void SelectLine(int lineNumber)
        {
            // check if the value is within the ranges.
            if (lineNumber < 1 || lineNumber > Document.Lines.Count)
                throw new ArgumentOutOfRangeException();

            DocumentLine line = Document.Lines[lineNumber - 1];
            if (line.Length > 0)
            {
                int vx = SelectedView.FirstVisibleX;
                int vl = SelectedView.FirstVisibleDisplayLineIndex;
                SelectedView.Selection.SelectRange(line.TextRange);
                SelectedView.FirstVisibleX = vx;
                SelectedView.FirstVisibleDisplayLineIndex = vl;
            }
        }

        /// <summary>
        /// Selects line, given line number</summary>
        /// <remarks>Selection is [startOffset, endOffset] inclusive</remarks>
        /// <param name="lineNumber">The line number to be selected</param>
        /// <param name="startOffset">Beginning offset of the selection; starting index is zero</param>
        /// <param name="endOffset">Ending offset of selection</param>
        public void SelectLine(int lineNumber, int startOffset, int endOffset)
        {
            // check if the value is within the ranges.
            if (lineNumber < 1 || lineNumber > Document.Lines.Count)
                throw new ArgumentOutOfRangeException();
            DocumentLine line = Document.Lines[lineNumber - 1];
            if (line.Length == 0)
                return;
            if (startOffset < 0 || startOffset >= line.Length
                || endOffset < 0 || endOffset >= line.Length)
                throw new ArgumentOutOfRangeException();
            int vx = SelectedView.FirstVisibleX;
            int vl = SelectedView.FirstVisibleDisplayLineIndex;
            SelectedView.Selection.SelectRange(line.StartOffset + startOffset, endOffset - startOffset + 1);
            SelectedView.FirstVisibleX = vx;
            SelectedView.FirstVisibleDisplayLineIndex = vl;
        }

        /// <summary>
        /// Gets the text for the given line number</summary>
        /// <param name="lineNumber">The line number</param>
        /// <returns>Text of the given line</returns>
        /// <remarks>If the line is empty, an empty string is returned;
        /// this method does not return null.
        /// If the line number is out of range, the ArgumentOutOfRangeException
        /// exception is thrown.</remarks>
        public string GetLineText(int lineNumber)
        {
            // check if the value is within the ranges.
            if (lineNumber < 1 || lineNumber > Document.Lines.Count)
                throw new ArgumentOutOfRangeException();

            DocumentLine line = Document.Lines[lineNumber - 1];
            return line.Text ?? string.Empty;
        }

        /// <summary>
        /// Returns line number from offset</summary>
        /// <param name="offset">Offset in the document</param>
        /// <returns>Line number of offset</returns>
        public int GetLineFromOffset(int offset)
        {
            return Document.Lines.IndexOf(offset) + 1;
        }

        /// <summary>
        /// Gets all the tokens for the current line</summary>
        /// <param name="lineNumber">Line number</param>
        /// <returns>An array of tokens</returns>
        public Token[] GetTokens(int lineNumber)
        {
            // check if the value is within the ranges.
            if (lineNumber < 1 || lineNumber > Document.Lines.Count)
                throw new ArgumentOutOfRangeException();

            var tokens = new List<Token>();
            DocumentLine line = Document.Lines[lineNumber - 1];
            TextStream strm = Document.GetTextStream(line.StartOffset);

            while (true)
            {
                IToken t = strm.ReadToken();
                tokens.Add(new Token(t.StartOffset, t.EndOffset, t.ID, t.Key, Document.GetTokenText(t)));
                if (t.StartOffset >= line.EndOffset)
                    break;
            }
            return tokens.ToArray();
        }

        /// <summary>
        /// Sets/unsets current-statement indicator for the specified line</summary>
        /// <param name="lineNumber">Line to set/unset statement indicator</param>
        /// <param name="set">True to set statement indicator, false to unset it</param>
        /// <remarks>
        /// Setting an already set current-statement indicator will not have any side effect.
        /// Unsetting an already unsetted current-statement indicator will not have any side effect.
        /// This method throws ArgumentOutOfRangeException if line number is out of range.</remarks>
        public void CurrentStatement(int lineNumber, bool set)
        {
            // check if the value is within the ranges.
            if (lineNumber < 1 || lineNumber > Document.Lines.Count)
                throw new ArgumentOutOfRangeException();

            DocumentLine line = Document.Lines[lineNumber - 1];
            if (StringUtil.IsNullOrEmptyOrWhitespace(line.Text))
                return;

            // try to get indicators layer
            SpanIndicatorLayer layer = Document.SpanIndicatorLayers[SpanIndicatorLayer.CurrentStatementKey];
            if (set)
            {
                if (layer == null)
                {
                    layer = new SpanIndicatorLayer(SpanIndicatorLayer.CurrentStatementKey, SpanIndicatorLayer.CurrentStatementDisplayPriority);
                    Document.SpanIndicatorLayers.Add(layer);
                }
                if (!layer.OverlapsWith(line.TextRange))
                    layer.Add(new CurrentStatementSpanIndicator(), line.TextRange);
            }
            else
            {
                if (layer != null && layer.OverlapsWith(line.TextRange))
                {
                    SpanIndicator[] spanIndicators = layer.GetIndicatorsForTextRange(line.TextRange);
                    foreach (SpanIndicator s in spanIndicators)
                        if (s is CurrentStatementSpanIndicator)
                            layer.Remove(s);
                }
            }
        }

        /// <summary>
        /// Sets control to one of the built-in languages</summary>
        /// <param name="lang">Language to be edited in control</param>
        public void SetLanguage(Languages lang)
        {
            // load language definition from resource.
            Assembly thisAssem = Assembly.GetAssembly(GetType());
            // load language definition.
            Stream strm = null;

            if (lang == Languages.Csharp)
                Document.Language = new ActiproSoftware.SyntaxEditor.Addons.CSharp.CSharpSyntaxLanguage();
            else if (lang == Languages.Text)
                Document.ResetLanguage();
            else
            {
                const string langPath = "Sce.Atf.Controls.SyntaxEditorControl.LanguageDefinitions.";
                if (lang == Languages.Python)
                    strm = thisAssem.GetManifestResourceStream(langPath + "PythonDefinition.xml");
                else if (lang == Languages.Lua)
                {
                    // Use custom class that adds "extras" (like code folding)
                    Document.Language = new LuaDynamicSyntaxLanguage(thisAssem, langPath + "LuaDefinition.xml");
                    return;
                }
                else if (lang == Languages.Squirrel)
                    strm = thisAssem.GetManifestResourceStream(langPath + "SquirrelDefinition.xml");
                else if (lang == Languages.Xml)
                    strm = thisAssem.GetManifestResourceStream(langPath + "XMLDefinition.xml");
                else if (lang == Languages.Cg)
                    strm = thisAssem.GetManifestResourceStream(langPath + "CgDefinition.xml");
                else if (lang == Languages.Imat)
                    strm = thisAssem.GetManifestResourceStream(langPath + "ImatDefinition.xml");

                if (strm == null)
                    throw new Exception(lang + " language definition not found");

                Document.LoadLanguageFromXml(strm, 0);
            }
        }

        /// <summary>
        /// Sets control to a custom language</summary>
        /// <param name="stream">XML stream containing the syntax rules for the language</param>
        public void SetLanguage(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            Document.LoadLanguageFromXml(stream, 0);
        }

        /// <summary>
        /// Gets or sets list of current-statement-indicators' line number</summary>
        public IList<int> CurrentStatements
        {
            get
            {
                var retVal = new List<int>();
                foreach (DocumentLine line in Document.Lines)
                {
                    foreach (SpanIndicator si in line.SpanIndicators)
                    {
                        if (si is CurrentStatementSpanIndicator)
                        {
                            retVal.Add(line.Index + 1);
                            break;
                        }
                    }
                }
                return retVal;
            }
            set
            {
                int lineCount = Document.Lines.Count;
                IList<int> list = value;
                // Add a current statement indicators layter 
                SpanIndicatorLayer layer = Document.SpanIndicatorLayers[SpanIndicatorLayer.CurrentStatementKey];
                if (layer == null)
                {
                    layer = new SpanIndicatorLayer(SpanIndicatorLayer.CurrentStatementKey, SpanIndicatorLayer.CurrentStatementDisplayPriority);
                    Document.SpanIndicatorLayers.Add(layer);
                }
                foreach (int ln in list)
                {
                    if (ln <= lineCount)
                    {
                        DocumentLine line = Document.Lines[ln - 1];
                        if (!StringUtil.IsNullOrEmptyOrWhitespace(line.Text))
                        {
                            if (!layer.OverlapsWith(line.TextRange))
                            {
                                layer.Add(new CurrentStatementSpanIndicator(), line.TextRange);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Toggles break point for the current line</summary>
        public void ToggleBreakpoint()
        {
            DocumentLine line = SelectedView.CurrentDocumentLine;
            ProcessBreakpointChange(line, BreakpointOperation.Toggle);
        }

        /// <summary>
        /// Gets list of IBreakpoints</summary>        
        public IBreakpoint[] GetBreakpoints()
        {
            var retVal = new List<IBreakpoint>();
            foreach (DocumentLine line in Document.Lines)
            {
                foreach (SpanIndicator si in line.SpanIndicators)
                {
                    var ib = si as IBreakpoint;
                    if (ib != null)
                        retVal.Add(ib);
                }
            }
            return retVal.ToArray();
        }

        /// <summary>
        /// Sets breakpoint for specified lines</summary>
        /// <param name="list">List of lines</param>
        public void SetBreakpoints(IList<int> list)
        {
            if (list == null || list.Count == 0)
                return;

            int lineCount = Document.Lines.Count;
            foreach (int ln in list)
            {
                // skip over invalid line number.
                if (ln < 1 || ln > lineCount)
                    continue;
                DocumentLine line = Document.Lines[ln - 1];
                ProcessBreakpointChange(line, BreakpointOperation.Set);
            }
        }

        /// <summary>
        /// Sets or unsets breakpoint for a given line</summary>
        /// <param name="lineNumber">The line to set or unset breakpoint</param>
        /// <param name="set">True to set break point, false to unset it</param>
        /// <remarks>
        /// Setting an already set breakpoint will not have any side effect.
        /// Unsetting an already unset breakpoint will not have any side effect.
        /// This method throws ArgumentOutOfRangeException if line number is out of range.</remarks>
        public void Breakpoint(int lineNumber, bool set)
        {
            // check if the value is within the ranges.
            if (lineNumber < 1 || lineNumber > Document.Lines.Count)
                throw new ArgumentOutOfRangeException();
            DocumentLine line = Document.Lines[lineNumber - 1];
            BreakpointOperation ops = (set) ? BreakpointOperation.Set : BreakpointOperation.Unset;
            ProcessBreakpointChange(line, ops);
        }

        /// <summary>
        /// Gets IBreakpoint for a given line number</summary>
        /// <param name="lineNumber">Line number</param>
        /// <returns>IBreakpoint or null if there is none for given line</returns>
        public IBreakpoint GetBreakpoint(int lineNumber)
        {
            // check if the value is within the ranges.
            if (lineNumber < 1 || lineNumber > Document.Lines.Count)
                throw new ArgumentOutOfRangeException();
            IBreakpoint retVal = null;
            DocumentLine line = Document.Lines[lineNumber - 1];
            foreach (SpanIndicator si in line.SpanIndicators)
            {
                retVal = si as IBreakpoint;
                if (retVal != null)
                    break;
            }
            return retVal;
        }

        /// <summary>
        /// Clears all breakpoints for the current document</summary>
        public void ClearAllBreakpoints()
        {
            // try to get breakpoint layer
            SpanIndicatorLayer layer = Document.SpanIndicatorLayers[SpanIndicatorLayer.BreakpointKey];
            if (layer == null)
                return;
            for (int i = layer.Count - 1; i >= 0; i--)
            {
                SpanIndicator s = layer[i];
                if (s is BreakpointIndicator)
                {
                    layer.Remove(s);
                }
            }
        }

        /// <summary>
        /// Gets and sets multiline property</summary>
        public bool Multiline
        {
            get { return Document.Multiline; }
            set { Document.Multiline = value; }
        }

        /// <summary>
        /// Gets or sets the selected range of text in the document</summary>
        public ISyntaxEditorTextRange SelectRange
        {
            get { return new SyntaxEditorTextRange(SelectedView.Selection.StartOffset, SelectedView.Selection.EndOffset); }
            set { SelectedView.Selection.SelectRange(new TextRange(value.StartOffset, value.EndOffset)); }
        }

        /// <summary>Gets a word text range starting at an offset in the document</summary>
        /// <param name="offset">Offset in the document</param>
        /// <returns>Word range based on offset</returns>
        public ISyntaxEditorTextRange GetWordTextRange(int offset)
        {
            var range = Document.GetWordTextRange(offset);
            return new SyntaxEditorTextRange(range.StartOffset, range.EndOffset);
        }

        /// <summary>
        /// Clears the IntelliPrompt member collection</summary>
        public void ClearIntelliPromptMemberList()
        {
            IntelliPrompt.MemberList.Clear();
        }

        /// <summary>
        /// Adds an item to the IntelliPrompt member collection</summary>
        /// <param name="item">Item to add</param>
        public void AddIntelliPromptMemberListItem(string item)
        {
            IntelliPrompt.MemberList.Add(new IntelliPromptMemberListItem(item, 0));
        }

        /// <summary>
        /// Adds an item to the IntelliPrompt member collection</summary> 
        /// <param name="item">Item to add</param>
        /// <param name="description">Item's description</param>
        /// <param name="autoCompletePreText">Text before auto completion</param>
        /// <param name="autoCompletePostText">Text after auto completion</param>
        public void AddIntelliPromptMemberListItem(string item, string description, string autoCompletePreText, string autoCompletePostText)
        {
            IntelliPrompt.MemberList.Add(
                new IntelliPromptMemberListItem(
                    item,
                    0,
                    string.IsNullOrEmpty(description) ? string.Empty : description,
                    string.IsNullOrEmpty(autoCompletePreText) ? string.Empty : autoCompletePreText,
                    string.IsNullOrEmpty(autoCompletePostText) ? string.Empty : autoCompletePostText));
        }

        /// <summary>
        /// Shows the IntelliPrompt member list</summary>
        public void ShowIntelliPromptMemberList()
        {
            IntelliPrompt.MemberList.Show();
            IntelliPrompt.MemberList.SelectedItem = IntelliPrompt.MemberList[0];
        }

        /// <summary>
        /// Shows the IntelliPrompt member list</summary>
        /// <param name="offset">Offset of list</param>
        /// <param name="length">List length</param>
        public void ShowIntelliPromptMemberList(int offset, int length)
        {
            if (IntelliPrompt.MemberList.Count > 0)
                IntelliPrompt.MemberList.SelectedItem = IntelliPrompt.MemberList[0];

            IntelliPrompt.MemberList.Show(offset, length);
        }

        /// <summary>
        ///  Event raised when a key is pressed</summary>
        public event OnKeyPressEventHandler OnKeyPressEvent;

        /// <summary>
        /// Raises the KeyPress event and performs custom processing</summary>
        /// <param name="e">KeyPressEventArgs containing event data</param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (OnKeyPressEvent != null)
                OnKeyPressEvent(this, e);
        }

        private void ProcessBreakpointChange(DocumentLine line, BreakpointOperation ops)
        {
            // please notify ATF team before defining  FullISyntaxEditorControl
#if (FullISyntaxEditorControl)
            if (line == null || StringUtil.IsNullOrEmptyOrWhitespace(line.Text))
                return;

            // get or create breakpoint layer.
            SpanIndicatorLayer layer = Document.SpanIndicatorLayers[SpanIndicatorLayer.BreakpointKey];
            if (layer == null)
            {
                layer = new SpanIndicatorLayer(SpanIndicatorLayer.BreakpointKey, SpanIndicatorLayer.BreakpointDisplayPriority);
                Document.SpanIndicatorLayers.Add(layer);
            }
            
            // find breakpoint indicator for the current line.
            BreakpointIndicator breakpointIndicator = null;
            foreach (SpanIndicator si in line.SpanIndicators)
            {
                breakpointIndicator = si as BreakpointIndicator;
                if (breakpointIndicator != null)
                    break;
            }

            bool breakpointExist = breakpointIndicator != null;
            bool addbreakpoint = ((ops == BreakpointOperation.Set || ops == BreakpointOperation.Toggle) && !breakpointExist);

            // do nothing 
            if (addbreakpoint == breakpointExist)
                return;
                       
            var e = new BreakpointEventArgs(addbreakpoint, line.Index + 1, line.Text);            
            BreakpointChanging.Raise(this, e);

            // cancel the operation
            if (e.Cancel)
                return;

            if (addbreakpoint)
            {
                breakpointIndicator = new BreakpointIndicator();
                layer.Add(breakpointIndicator, line.TextRange);
            }
            else // remove breakpoint
            {
                layer.Remove(breakpointIndicator);                
            }  
#endif
        }

        private SyntaxEditorRegions SyntaxEditorHitTestTargetToSyntaxEditorRegions(SyntaxEditorHitTestTarget target)
        {
            return (SyntaxEditorRegions)Enum.Parse(typeof(SyntaxEditorRegions), target.ToString());
        }

        private enum BreakpointOperation
        {
            Toggle,
            Set,
            Unset,
        }

        #region Find and Replace Stuff

        /// <summary>
        /// Clears any bookmarks that have been placed due to a MarkAll</summary>
        public void ClearSpanIndicatorMarks()
        {
            Document.FindReplace.ClearSpanIndicatorMarks();
        }

        #region FindReplaceResult and FindReplaceResultSet Wrapper Private Classes

        private class SyntaxEditorFindReplaceResult : FindReplaceResult, ISyntaxEditorFindReplaceResult
        {
            public SyntaxEditorFindReplaceResult(FindReplaceResult result)
                : base(result.TextRange, result.Groups)
            {
            }
        }

        private class SyntaxEditorFindReplaceResultSet : ISyntaxEditorFindReplaceResultSet
        {
            public SyntaxEditorFindReplaceResultSet(FindReplaceResultSet results)
            {
                m_lstResults = new List<SyntaxEditorFindReplaceResult>();
                foreach (FindReplaceResult result in results)
                    m_lstResults.Add(new SyntaxEditorFindReplaceResult(result));
                m_bPastDocumentEnd = results.PastDocumentEnd;
                m_bPastSearchStartOffset = results.PastSearchStartOffset;
                m_bReplaceOccurred = results.ReplaceOccurred;
            }

            public bool PastDocumentEnd
            {
                get { return m_bPastDocumentEnd; }
            }

            public bool PastSearchStartOffset
            {
                get { return m_bPastSearchStartOffset; }
            }

            public bool ReplaceOccurred
            {
                get { return m_bReplaceOccurred; }
            }

            public int Count
            {
                get { return m_lstResults.Count; }
            }

            public ISyntaxEditorFindReplaceResult this[int index]
            {
                get { return m_lstResults[index]; }
            }

            public IEnumerator GetEnumerator()
            {
                return m_lstResults.GetEnumerator();
            }

            private readonly bool m_bPastDocumentEnd;
            private readonly bool m_bPastSearchStartOffset;
            private readonly bool m_bReplaceOccurred;
            private readonly List<SyntaxEditorFindReplaceResult> m_lstResults;
        }

        #endregion

        /// <summary>
        /// Finds text instance based on options starting at offset</summary>
        /// <param name="options">Find/replace options</param>
        /// <param name="startOffset">Offset in the document at which to start find operation</param>
        /// <returns>Find/replace result set</returns>
        public ISyntaxEditorFindReplaceResultSet Find(ISyntaxEditorFindReplaceOptions options, int startOffset)
        {
            FindReplaceResultSet results = Document.FindReplace.Find((FindReplaceOptions)options, startOffset);
            return new SyntaxEditorFindReplaceResultSet(results);
        }

        /// <summary>
        /// Finds all instances of text based on options in entire document</summary>
        /// <param name="options">Find/replace options</param>
        /// <returns>Find/replace result set</returns>
        public ISyntaxEditorFindReplaceResultSet FindAll(ISyntaxEditorFindReplaceOptions options)
        {
            FindReplaceResultSet results = Document.FindReplace.FindAll((FindReplaceOptions)options);
            return new SyntaxEditorFindReplaceResultSet(results);
        }

        /// <summary>
        /// Finds all instances of text based on options in text range</summary>
        /// <param name="options">Find/replace options</param>
        /// <param name="textRange">Text range to search</param>
        /// <returns>Find/replace result set</returns>
        public ISyntaxEditorFindReplaceResultSet FindAll(ISyntaxEditorFindReplaceOptions options, ISyntaxEditorTextRange textRange)
        {
            FindReplaceResultSet results = Document.FindReplace.FindAll((FindReplaceOptions)options, new TextRange(textRange.StartOffset, textRange.EndOffset));
            return new SyntaxEditorFindReplaceResultSet(results);
        }

        /// <summary>
        /// Marks all instances of text based on options in entire document</summary>
        /// <param name="options">Find/replace options</param>
        /// <returns>Find/replace result set</returns>
        public ISyntaxEditorFindReplaceResultSet MarkAll(ISyntaxEditorFindReplaceOptions options)
        {
            FindReplaceResultSet results = Document.FindReplace.MarkAll((FindReplaceOptions)options);
            return new SyntaxEditorFindReplaceResultSet(results);
        }

        /// <summary>
        /// Marks all instances of text based on options in text range</summary>
        /// <param name="options">Find/replace options</param>
        /// <param name="textRange">Text range to search</param>
        /// <returns>Find/replace result set</returns>
        public ISyntaxEditorFindReplaceResultSet MarkAll(ISyntaxEditorFindReplaceOptions options, ISyntaxEditorTextRange textRange)
        {
            FindReplaceResultSet results = Document.FindReplace.MarkAll((FindReplaceOptions)options, new TextRange(textRange.StartOffset, textRange.EndOffset));
            return new SyntaxEditorFindReplaceResultSet(results);
        }

        /// <summary>
        /// Replaces text based on options in previously found text indicated by a ISyntaxEditorFindReplaceResult</summary>
        /// <param name="options">Find/replace options</param>
        /// <param name="result">Previously found text in which to do the replace</param>
        /// <returns>Find/replace result set</returns>
        public ISyntaxEditorFindReplaceResultSet Replace(ISyntaxEditorFindReplaceOptions options, ISyntaxEditorFindReplaceResult result)
        {
            FindReplaceResultSet results = Document.FindReplace.Replace((FindReplaceOptions)options, (FindReplaceResult)result);
            return new SyntaxEditorFindReplaceResultSet(results);
        }

        /// <summary>
        /// Replaces all text based on options in entire document</summary>
        /// <param name="options">Find/replace options</param>
        /// <returns>Find/replace result set</returns>
        public ISyntaxEditorFindReplaceResultSet ReplaceAll(ISyntaxEditorFindReplaceOptions options)
        {
            FindReplaceResultSet results = Document.FindReplace.ReplaceAll((FindReplaceOptions)options);
            return new SyntaxEditorFindReplaceResultSet(results);
        }

        /// <summary>
        /// Replaces all text based on options in text range</summary>
        /// <param name="options">Find/replace options</param>
        /// <param name="textRange">Text range in which to replace text</param>
        /// <returns>Find/replace result set</returns>
        public ISyntaxEditorFindReplaceResultSet ReplaceAll(ISyntaxEditorFindReplaceOptions options, ISyntaxEditorTextRange textRange)
        {
            FindReplaceResultSet results = Document.FindReplace.ReplaceAll((FindReplaceOptions)options, new TextRange(textRange.StartOffset, textRange.EndOffset));
            return new SyntaxEditorFindReplaceResultSet(results);
        }

        /// <summary>
        /// Sets the color of the background in the text editing area</summary>
        public Color TextAreaBackgroundFill
        {
            get { return GetColor(SERenderer.TextAreaBackgroundFill); }
            set { SERenderer.TextAreaBackgroundFill = CreateColorFill(value); }
        }

        /// <summary>
        /// Sets the color of the background in the text editing area, when control is disabled</summary>
        public Color TextAreaDisabledBackgroundFill
        {
            get { return GetColor(SERenderer.TextAreaDisabledBackgroundFill); }
            set { SERenderer.TextAreaDisabledBackgroundFill = CreateColorFill(value); }
        }

        /// <summary>
        /// Sets the color of the background for selected text in the text editing area</summary>
        public Color SelectionFocusedBackColor
        {
            get { return SERenderer.SelectionFocusedBackColor; }
            set { SERenderer.SelectionFocusedBackColor = value; SERenderer.SelectionFocusedBorderColor = value; }
        }

        /// <summary>
        /// Sets the color of the foreground for selected text in the text editing area</summary>
        public Color SelectionFocusedForeColor
        {
            get { return SERenderer.SelectionFocusedForeColor; }
            set { SERenderer.SelectionFocusedForeColor = value; }
        }

        /// <summary>
        /// Sets the color of the background for selected text in the text editing area, when control is unselected</summary>
        public Color SelectionUnfocusedBackColor
        {
            get { return SERenderer.SelectionUnfocusedBackColor; }
            set { SERenderer.SelectionUnfocusedBackColor = value; SERenderer.SelectionUnfocusedBorderColor = value; }
        }

        /// <summary>
        /// Sets the color of the foreground for selected text in the text editing area, when control is unselected</summary>
        public Color LineNumberForeColor
        {
            get { return SERenderer.LineNumberMarginForeColor; }
            set { SERenderer.LineNumberMarginForeColor = value; }
        }

        /// <summary>
        /// Sets the color of the background for the "line number" margin area</summary>
        public Color LineNumberMarginBackColor 
        { 
            get { return GetColor(SERenderer.LineNumberMarginBackgroundFill); }
            set 
            { 
                SERenderer.LineNumberMarginBackgroundFill = CreateColorFill(value); 
                SERenderer.LineNumberMarginBorderColor = value;
            } 
        }

        /// <summary>
        /// Sets the color of the text of the "line number" margin area</summary>
        public Color SelectionUnfocusedForeColor
        {
            get { return SERenderer.SelectionUnfocusedForeColor; }
            set { SERenderer.SelectionUnfocusedForeColor = value; }
        }

        /// <summary>
        /// Sets the color of the background for the "line select" margin area</summary>
        public Color LineSelectMarginBackColor
        {
            get { return GetColor(SERenderer.SelectionMarginBackgroundFill); }
            set
            {
                SERenderer.ScrollBarBlockBackgroundFill = CreateColorFill(value);
                SERenderer.SelectionMarginBackgroundFill = CreateColorFill(value);
                SERenderer.SelectionMarginBorderColor = value;
            }
        }

        /// <summary>
        /// Sets the color of the background for the outline indicator (in the "line selection" margin area)</summary>
        public Color OutlineIndicatorBackColor
        {
            get { return SERenderer.OutliningIndicatorBackColor; }
            set { SERenderer.OutliningIndicatorBackColor = value; }
        }

        /// <summary>
        /// Sets the color of the foreground for the outline indicator (in the "line selection" margin area)</summary>
        public Color OutlineIndicatorForeColor
        {
            get { return SERenderer.OutliningIndicatorForeColor; }
            set { SERenderer.OutliningIndicatorForeColor = value; }
        }

        /// <summary>
        /// Sets the foreground color for a specified language-specific keyword in the text editing area</summary>
        public IEnumerable<TextHighlightStyle> TextHighlightStyles
        {
            get 
            {
                var styles = new List<TextHighlightStyle>();
                for (int i = 0; i < Document.Language.HighlightingStyles.Count; i++)
                {
                    var item = Document.Language.HighlightingStyles[i];
                    styles.Add(new TextHighlightStyle(item.Key, item.ForeColor));
                }
                return styles; 
            }
            set
            {
                foreach (var item in value)
                {
                    var style = Document.Language.HighlightingStyles[item.Key];
                    if (style != null)
                        style.ForeColor = item.ForeColor;
                }
            }
        }

        #endregion

        private VisualStudio2005SyntaxEditorRenderer SERenderer
        {
            get { return (VisualStudio2005SyntaxEditorRenderer)Renderer; }
        }

        private static ActiproSoftware.Drawing.SolidColorBackgroundFill CreateColorFill(Color color)
        {
            return new ActiproSoftware.Drawing.SolidColorBackgroundFill(color);
        }

        private static Color GetColor(ActiproSoftware.Drawing.BackgroundFill bgFill)
        {
            var solidColorBgFill = bgFill as ActiproSoftware.Drawing.SolidColorBackgroundFill;
            return solidColorBgFill == null ? Color.Black : solidColorBgFill.Color;
        }

        private void SyntaxEditorControlSelectionChanged(object sender, SelectionEventArgs e)
        {
            SelectionChanged.Raise(this, EventArgs.Empty);
        }

        public new event EventHandler SelectionChanged;

        #endregion

        #region Silly Audit Function

        private static void PerformAndShowAuditStuff()
        {
            int totalSyntaxEditorPublicItems = 0;
            int totalInterfacePublicItems = 0;
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

            {
                Type type = typeof(SyntaxEditor);
                PropertyInfo[] properties = type.GetProperties(flags);
                MethodInfo[] methods = type.GetMethods(flags);
                EventInfo[] events = type.GetEvents(flags);
                totalSyntaxEditorPublicItems = properties.Length + methods.Length + events.Length;
            }

            {
                Type type = typeof(ISyntaxEditorControl);
                PropertyInfo[] properties = type.GetProperties(flags);
                MethodInfo[] methods = type.GetMethods(flags);
                EventInfo[] events = type.GetEvents(flags);
                totalInterfacePublicItems = properties.Length + methods.Length + events.Length;
            }

            MessageBox.Show(
                string.Format(
                    "ATF's ISyntaxEditorControl is using approximately {0}% " +
                    "of the ActiproSoftware SyntaxEditor class!",
                    (totalInterfacePublicItems / (float)totalSyntaxEditorPublicItems) * 100),
                @"SyntaxEditorControl Audit");
        }

        #endregion
    }

    internal class SyntaxEditorFindReplaceOptions : FindReplaceOptions, ISyntaxEditorFindReplaceOptions
    {
        public new SyntaxEditorFindReplaceSearchType SearchType
        {
            get
            {
                if (base.SearchType == FindReplaceSearchType.RegularExpression)
                    return SyntaxEditorFindReplaceSearchType.RegularExpression;
                if (base.SearchType == FindReplaceSearchType.Wildcard)
                    return SyntaxEditorFindReplaceSearchType.Wildcard;
                return SyntaxEditorFindReplaceSearchType.Normal;
            }

            set
            {
                switch (value)
                {
                    default:
                    case SyntaxEditorFindReplaceSearchType.Normal: base.SearchType = FindReplaceSearchType.Normal; break;
                    case SyntaxEditorFindReplaceSearchType.RegularExpression: base.SearchType = FindReplaceSearchType.RegularExpression; break;
                    case SyntaxEditorFindReplaceSearchType.Wildcard: base.SearchType = FindReplaceSearchType.Wildcard; break;
                }
            }
        }
    }

    internal class TextHighlightStyle
    {
        public TextHighlightStyle(string key, Color foreColor)
        {
            Key = key;
            ForeColor = foreColor;
        }

        public string Key { get; private set; }
        public Color ForeColor { get; private set; }
    }
}