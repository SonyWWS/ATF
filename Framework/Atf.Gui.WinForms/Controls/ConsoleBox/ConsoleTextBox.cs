//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Sce.Atf.Controls.ConsoleBox
{
    /// <summary>
    /// Text box for a console</summary>
    class ConsoleTextBox : TextBox, IConsoleTextBox
    {
        /// <summary>
        /// Constructor</summary>
        public ConsoleTextBox()
        {
            Font = new Font("Lucida Console", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            MaxLength = 0; // allow max character
            Multiline = true;
            ScrollBars = ScrollBars.Vertical;
            ShortcutsEnabled = true;
            WordWrap = true;
            BorderStyle = BorderStyle.None;
            m_suggestionListBox = new SuggestionListBox
            {
                InsertText = InsertTextAtCaret,
                ItemHeight = 11,
                RemoveText = RemoveTextBeforeCaret,
                Suggest = Suggest
            };
            m_suggestionListBox.Hide();

            Controls.Add(m_suggestionListBox);

            WritePrompt();
            m_commandHandler = DefaultCommandHandler;
        }

        /// <summary>
        /// Gets or sets a value indicating the command prompt.</summary>
        /// <returns>The command prompt. The default is (">>> ").</returns>
        [Browsable(true), Category("Appearance"), DefaultValue(">>> ")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string Prompt
        {
            get { return m_prompt; }
            set
            {
                string prompt = value;
                if (String.IsNullOrEmpty(prompt))
                    prompt = " ";
                m_prompt = prompt;

                m_multilinePrompt = new String('.', m_prompt.Length - 1);
                m_multilinePrompt += " ";
            }
        }

        /// <summary>
        /// Get or sets a value indicating the number of spaces per indentation level.</summary>
        /// <returns>The number of spaces per indentation level. The default is 4.</returns>
        [Browsable(true), Category("Appearance"), DefaultValue(4)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int SpacesPerIndent
        {
            get { return m_spacesPerIndent; }
            set { m_spacesPerIndent = value; }
        }

        /// <summary>
        /// Sets the command handler. This delegate is called when the command is entered.</summary>
        public CmdHandler CommandHandler
        {
            set { m_commandHandler = value ?? DefaultCommandHandler; }
        }

        /// <summary>
        /// Sets the suggestion handler.  This delegate gets called when the user types Ctrl+Space.</summary>
        public SuggestionHandler SuggestionHandler
        {
            set { m_suggestionHandler = value ?? DefaultSuggestionHandler; }
        }

        /// <summary>
        /// Clears console window</summary>
        public new void Clear()
        {
            Text = String.Empty;
            WritePrompt();
        }

        /// <summary>
        /// Copies the current selection in the text box to the Clipboard.</summary>
        public new void Copy()
        {
            string text = GetSelectedText();
            if (!String.IsNullOrEmpty(text))
                Clipboard.SetText(text);
        }

        /// <summary>
        /// Moves the current selection to the Clipboard. Removing only the selected portion of the unsubmitted statement.</summary>
        public new void Cut()
        {
            Copy();
            RemoveSelection();
        }

        /// <summary>
        /// Gets the underlying control</summary>
        public Control Control { get { return this; } }

        /// <summary>
        /// Enter command programatically.
        /// The result is equivalent to manually typing the command and pressing the Enter key.
        /// The command doesn't need to terminate with a newline.</summary>
        /// <param name="cmd">Command</param>
        public void EnterCommand(string cmd)
        {
            if (String.IsNullOrEmpty(cmd))
                return;

            // TODO: Do not overwrite current prompt
            WriteLine(cmd);
            ExecuteCommand(cmd);
        }

        /// <summary>
        /// Writes to the console window</summary>
        /// <param name="text">Text to write</param>
        public void Write(string text)
        {
            if (String.IsNullOrEmpty(text))
                return;

            MoveCaretToEnd();
            AppendText(text);
        }

        /// <summary>
        /// Writes to the console window, appending a newline character</summary>
        /// <param name="text">Text to write</param>
        public void WriteLine(string text)
        {
            Write(text);
            AppendText(Environment.NewLine);
        }

        /// <summary>
        /// Raises the KeyDown event. Performs custom actions on KeyDown events.</summary>
        /// <param name="e">Key event args that contains the event data</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Control)
            {
                base.OnKeyDown(e);  //Necessary for Ctrl-C to work properly
                if (e.KeyCode == Keys.Enter) // prevent creating new lines without evaluating command
                    e.SuppressKeyPress = true;
                return;
            }

            e.Handled = e.KeyCode == Keys.Up ||
                        e.KeyCode == Keys.Down ||
                        e.KeyCode == Keys.Left ||
                        e.KeyCode == Keys.Right || 
                        e.KeyCode == Keys.Home ||
                        e.KeyCode == Keys.End;

            switch (e.KeyCode)
            {
                case Keys.Left:
                    PrevChar(MoveLeft);
                    break;

                case Keys.Right:
                    NextChar(MoveRight);
                    break;

                case Keys.Up: // show previous command.
                    string prevCmd = m_commandHistory.PreviousCommand;
                    if (prevCmd.Length > 0)
                        ReplaceTextAtPrompt(prevCmd);
                    break;

                case Keys.Down: // show next command.
                    string nextCmd = m_commandHistory.NextCommand;
                    ReplaceTextAtPrompt(nextCmd);
                    break;

                case Keys.End:
                    Next(MoveRight, GetCurrentLine().Length - CaretOffsetFromCurrentLine);
                    break;

                case Keys.Home:
                    Prev(MoveLeft, CaretOffsetFromCurrentLine);
                    break;

                default:
                    MoveCaretToWritablePosition();
                    break;
            }

            base.OnKeyDown(e);
        }

        /// <summary>
        /// Raises the KeyPress event. Performs custom actions on KeyPress events.</summary>
        /// <param name="e">Key press event args that contains the event data</param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case (char)Keys.Back:
                    e.Handled = true;

                    if (SelectionLength > 0)
                        RemoveSelection();
                    else
                        PrevChar(RemoveTextBeforeCaret);
                    break;

                case (char)Keys.Return:
                    e.Handled = true;

                    string command = GetTextAtPrompt();
                    AppendText(Environment.NewLine);
                    SubmitCommand(command);
                    break;
            }

            base.OnKeyPress(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ((msg.Msg == User32.WM_KEYDOWN) || (msg.Msg == User32.WM_SYSKEYDOWN))
            {
                switch (keyData)
                {
                    case Keys.Control | Keys.Back:
                    {
                        m_suggestionListBox.Hide();

                        PrevWord(RemoveTextBeforeCaret);
                        return true;
                    }

                    case Keys.Delete:
                    {
                        m_suggestionListBox.Hide();

                        if (SelectionLength > 0)
                            RemoveSelection();
                        else
                            NextChar(RemoveTextAfterCaret);
                        return true;
                    }

                    case Keys.Control | Keys.Delete:
                    {
                        m_suggestionListBox.Hide();

                        NextWord(RemoveTextAfterCaret);
                        return true;
                    }

                    case Keys.Control | Keys.End:
                    case Keys.Control | Keys.Shift | Keys.End:
                    {
                        m_suggestionListBox.Hide();

                        MoveRight(TextLength - CaretIndex);
                        return true;
                    }

                    case Keys.Control | Keys.Enter:
                    {
                        m_suggestionListBox.Hide();

                        string lastLine = GetCurrentLine();

                        // Split line
                        InsertTextAtCaret(Environment.NewLine);
                        Indent(lastLine);
                        break;
                    }

                    case Keys.Control | Keys.Home:
                    case Keys.Control | Keys.Shift | Keys.Home:
                    {
                        m_suggestionListBox.Hide();

                        MoveLeft(CaretOffsetFromPrompt);
                        return true;
                    }

                    case Keys.Control | Keys.Left:
                    case Keys.Control | Keys.Shift | Keys.Left:
                    {
                        m_suggestionListBox.Hide();

                        PrevWord(MoveLeft);
                        return true;
                    }

                    case Keys.Control | Keys.Right:
                    case Keys.Control | Keys.Shift | Keys.Right:
                    {
                        m_suggestionListBox.Hide();

                        NextWord(MoveRight);
                        return true;
                    }

                    case Keys.Control | Keys.Space:
                    {
                        Suggest();
                        return true;
                    }
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Function called before editing operations to perform special processing for these events</summary>
        /// <param name="m">A Windows Message object.</param>
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case User32.WM_CUT:
                    Cut();
                    return;

                case User32.WM_COPY:
                    Copy();
                    return;

                case User32.WM_PASTE:
                    string text = Clipboard.GetText();
                    InsertTextAtCaret(text);
                    return;

                case User32.WM_SETTEXT:
                    if (!IsCaretAtWritablePosition)
                        MoveCaretToEnd();
                    break;

                case User32.WM_CLEAR:
                    return;
            }

            base.WndProc(ref m);
        }

        private void ExecuteCommand(string cmd)
        {
            m_commandHistory.AddCommand(cmd);

            if (cmd.Length > 0)
            {
                using (var reader = new StringReader(cmd))
                {
                    string command = reader.ReadLine();
                    while (command != null)
                    {
                        if (IsCompoundStatement(command))
                        {
                            var sb = new StringBuilder();
                            do
                            {
                                sb.AppendLine(command);
                                command = reader.ReadLine();
                            } while (!String.IsNullOrEmpty(command));

                            m_commandHandler(sb.ToString());
                        }
                        else
                        {
                            m_commandHandler(command);
                        }

                        command = reader.ReadLine();
                    }
                }
            }

            if (TextLength > m_prompt.Length)
                WritePrompt();

            m_multiline = false;
        }

        private void SubmitCommand(string cmd)
        {
            string command = cmd.TrimEnd(' ');
            if (command.EndsWith(Environment.NewLine))
            {
                if (m_multiline)
                {
                    ExecuteCommand(command);
                }
                else
                {
                    if (TextLength > m_prompt.Length)
                        WritePrompt();
                }
            }
            else
            {
                if (IsCompoundStatement(command) || m_multiline)
                {
                    m_multiline = true;

                    WriteMultilinePrompt();

                    string lastLine = command;
                    int index = command.LastIndexOf(Environment.NewLine);
                    if (index != -1)
                    {
                        lastLine = command.Substring(index);
                        lastLine = lastLine.TrimStart('\r', '\n');
                    }

                    Indent(lastLine);
                }
                else
                {
                    ExecuteCommand(command);
                }
            }
        }

        private bool Suggest()
        {
            m_suggestionListBox.Items.Clear();

            if (IsCaretAtWritablePosition)
            {
                string text = GetCurrentLine();
                int offset = CaretOffsetFromCurrentLine;

                int charIndex;
                string partial;
                IEnumerable<string> suggestions;

                int triggerIndex = (offset > 0) ? text.LastIndexOfAny(s_triggers, offset - 1) : -1;
                if (triggerIndex > -1)
                {
                    // Find partially completed string to right of trigger
                    charIndex = triggerIndex;
                    partial = text.Substring(triggerIndex + 1, offset - triggerIndex - 1);

                    // Find string to left of trigger
                    var startIndex = text.LastIndexOfAny(s_delimiters, triggerIndex) + 1;
                    var value = text.Substring(startIndex, triggerIndex - startIndex);

                    if (String.IsNullOrWhiteSpace(value))
                    {
                        m_suggestionListBox.Hide();
                        return false;
                    }

                    var trigger = text[triggerIndex].ToString();
                    var quoteType = String.Empty;
                    if (trigger == "[")
                        quoteType = partial.StartsWith("'''")
                            ? "'''"
                            : partial.StartsWith("\"\"\"")
                                ? "\"\"\""
                                : partial.StartsWith("'")
                                    ? "'"
                                    : "\"";

                    suggestions = m_suggestionHandler(value, trigger)
                        .Select(suggestion => String.Format("{0}{1}{0}", quoteType, StringUtil.EscapeQuotes(suggestion)));
                }
                else
                {
                    // Find partially completed string to right of ' '
                    charIndex = (offset > 0) ? text.LastIndexOf(' ', offset - 1) : -1;
                    partial = text.Substring(charIndex + 1, offset - charIndex - 1);

                    suggestions = m_suggestionHandler(String.Empty, String.Empty);
                }

                // Only add unique suggestions
                var uniqueSuggestions = suggestions
                    .Where(suggestion => suggestion.StartsWith(partial, StringComparison.InvariantCultureIgnoreCase))
                    .Distinct();

                m_suggestionListBox.SetSuggestions(partial, uniqueSuggestions);

                // Show suggestions (if any)
                var count = m_suggestionListBox.Items.Count;
                if (count > 0)
                {
                    int line = GetLineFromCharIndex(CaretIndex);
                    int index = GetFirstCharIndexFromLine(line) + m_prompt.Length + charIndex;
                    Point pt = GetPositionFromCharIndex(index);

                    int itemCount = Math.Min(count, 8);
                    int itemHeight = m_suggestionListBox.ItemHeight;

                    int bottom = Height - itemHeight;
                    if (pt.Y < bottom - 3)
                    {
                        int height = Math.Max(pt.Y - 8, bottom - pt.Y);
                        height = Math.Min(height, itemHeight * itemCount + 4); // Limit to 8 entries

                        int y;
                        if (pt.Y < height)
                            y = pt.Y + FontHeight;
                        else
                            y = pt.Y - height;

                        // Set width based on longest item
                        int maxWidth = 0;
                        foreach (var item in m_suggestionListBox.Items)
                        {
                            int width = TextRenderer.MeasureText(item.ToString(), m_suggestionListBox.Font).Width;
                            if (width > maxWidth)
                                maxWidth = width;
                        }

                        m_suggestionListBox.Bounds = new Rectangle(pt.X + 3, y, maxWidth + 25, height);
                        m_suggestionListBox.SelectedIndex = 0;

                        m_suggestionListBox.Show();
                        m_suggestionListBox.Focus();
                    }

                    return true;
                }
            }

            m_suggestionListBox.Hide();
            return false;
        }

        private void WriteMultilinePrompt()
        {
            if (TextLength > 0 && Text[TextLength - 1] != '\n')
                AppendText(Environment.NewLine);

            AppendText(m_multilinePrompt);
            MoveCaretToEnd();
        }

        private void WritePrompt()
        {
            if (TextLength > 0 && Text[TextLength - 1] != '\n')
                AppendText(Environment.NewLine);

            AppendText(m_prompt);
            MoveCaretToEnd();

            m_promptIndex = TextLength;
        }

        private void MoveCaretToEnd()
        {
            SelectionStart = TextLength;
            ScrollToCaret();
        }

        private void MoveCaretToPrompt()
        {
            // Move to nearest prompt
            MoveLeft((CaretOffsetFromPrompt < 0)
                ? CaretOffsetFromPrompt
                : CaretOffsetFromCurrentLine);
        }

        private void MoveCaretToWritablePosition()
        {
            // Move to nearest prompt if in unwritable position
            if (CaretOffsetFromPrompt < 0)
            {
                MoveLeft(CaretOffsetFromPrompt);
            }
            else if (CaretOffsetFromCurrentLine < 0)
            {
                MoveLeft(CaretOffsetFromCurrentLine);
            }
        }

        private string GetSelectedText()
        {
            string text = Text.Substring(SelectionStart, SelectionLength);

            // Multiline selection
            if (text.Contains(Environment.NewLine))
            {
                text = text.Replace(Environment.NewLine + m_prompt, Environment.NewLine);
                text = text.Replace(Environment.NewLine + m_multilinePrompt, Environment.NewLine);
            }

            return text;
        }

        private string GetTextAtPrompt()
        {
            string text = Text.Substring(m_promptIndex, TextLength - m_promptIndex);

            // Multiline prompts
            if (text.Contains(Environment.NewLine))
                text = text.Replace(Environment.NewLine + m_multilinePrompt, Environment.NewLine);

            return text;
        }

        private void Indent(string lastLine)
        {
            MoveCaretToPrompt();

            var indent = new String(' ', SpacesPerIndent);
            
            int level = lastLine.Replace("\t", indent).TakeWhile(Char.IsWhiteSpace).Count();
            if (IsCompoundStatement(lastLine))
                level += SpacesPerIndent;

            var indentation = new String(' ', level);
            InsertTextAtCaret(indentation);
        }

        private bool IsCaretAtWritablePosition
        {
            get { return CaretOffsetFromPrompt >= 0 && CaretOffsetFromCurrentLine >= 0; }
        }

        private bool IsCaretAtEnd
        {
            get { return CaretOffsetFromCurrentLine == GetCurrentLine().Length; }
        }

        private int CaretIndex
        {
            get
            {
                Point caret;
                User32.GetCaretPos(out caret);

                int caretIndex = GetCharIndexFromPosition(caret);

                // Correct caretIndex when at end of line
                if (caretIndex != SelectionStart)
                    return SelectionStart + SelectionLength;

                return caretIndex;
            }
        }

        private int CaretOffsetFromCurrentLine
        {
            get
            {
                int index = CaretIndex;
                int line = GetLineFromCharIndex(index);

                return index - GetFirstCharIndexFromLine(line) - m_prompt.Length;
            }
        }

        private int CaretOffsetFromPrompt
        {
            get { return CaretIndex - m_promptIndex; }
        }

        private void AdjustSelection(int count)
        {
            if (count == 0) return;

            if (SelectionLength == 0 && count < 0 || CaretIndex == SelectionStart)
            {
                int length = SelectionLength - count;
                if (length < 0)
                    SelectionStart += SelectionLength;
                else
                    SelectionStart += count;

                SelectionLength = Math.Abs(length);

                Point start = (length < 0) 
                    ? GetPositionFromCharIndex(SelectionStart + SelectionLength) // TODO: Cannot set caret to end
                    : GetPositionFromCharIndex(SelectionStart);

                User32.SetCaretPos(start.X, start.Y);
            }
            else
            {
                int length = SelectionLength + count;
                if (length < 0)
                    SelectionStart += length;

                SelectionLength = Math.Abs(length);

                if (length < 0)
                {
                    Point start = GetPositionFromCharIndex(SelectionStart);
                    User32.SetCaretPos(start.X, start.Y);
                }
            }
        }

        /// <remarks><see cref="TextBoxBase.Lines"/> will not work when Control when WordWrap is true.</remarks>
        private string GetCurrentLine()
        {
            int n = GetLineFromCharIndex(CaretIndex);
            int index = GetFirstCharIndexFromLine(n) + m_prompt.Length;

            int end = Text.IndexOf(Environment.NewLine, index);
            int length = (end < 0)
                ? TextLength - index
                : end - index;

            return Text.Substring(index, length);
        }

        private static bool IsCompoundStatement(string text)
        {
            string statement = text.Trim();

            // TODO: Support languages other than Python.
            if (statement.StartsWith("@")) // Decorated
                return true;

            if (statement.StartsWith("if ") ||
                statement.StartsWith("while ") ||
                statement.StartsWith("for ") ||
                statement.StartsWith("try ") ||
                statement.StartsWith("with ") ||
                statement.StartsWith("def ") ||
                statement.StartsWith("class "))
            {
                if (statement.EndsWith(":"))
                    return true;
            }

            return false;
        }

        private void MoveCaret(int count)
        {
            if ((ModifierKeys & Keys.Shift) == Keys.None)
            {
                int start = SelectionStart;
                if (CaretIndex > start)
                    start += SelectionLength;
                start += count;

                SelectionLength = 0;
                SelectionStart = start;
            }
            else
            {
                AdjustSelection(count);
            }
        }

        private void MoveLeft(int count)
        {
            MoveCaret(-count);
        }

        private void MoveRight(int count)
        {
            MoveCaret(count);
        }

        private void Next(Action<int> action, int count)
        {
            if (CaretOffsetFromPrompt < 0)
            {
                // Move to prompt
                action(-CaretOffsetFromPrompt);
            }
            else if (IsCaretAtEnd && SelectionStart != TextLength)
            {
                // Next line
                action(m_multilinePrompt.Length + 2);
            }
            else if (CaretOffsetFromCurrentLine < 0)
            {
                // Move to prompt
                action(-CaretOffsetFromCurrentLine);
            }
            else
            {
                action(count);
            }
        }

        private void NextChar(Action<int> action)
        {
            Next(action, 1);
        }

        private void NextWord(Action<int> action)
        {
            int count = 0;

            if (IsCaretAtWritablePosition && !IsCaretAtEnd)
            {
                // Move to beginning of next word
                string right = GetCurrentLine().Substring(CaretOffsetFromCurrentLine);
                MatchCollection matches = s_nextWord.Matches(right);
                if (matches.Count > 0)
                {
                    Match match = matches[0];
                    count = match.Length;
                }
            }

            Next(action, count);
        }

        private void Prev(Action<int> action, int count)
        {
            if (CaretOffsetFromPrompt <= 0)
            {
                // Move to prompt
                action(CaretOffsetFromPrompt);
            }
            else if (CaretOffsetFromCurrentLine > 0)
            {
                action(count);
            }
            else
            {
                // Previous line
                action(m_multilinePrompt.Length + CaretOffsetFromCurrentLine + 2);
            }
        }

        private void PrevChar(Action<int> action)
        {
            Prev(action, 1);
        }

        private void PrevWord(Action<int> action)
        {
            int count = 0;

            if (CaretOffsetFromPrompt > 0 && CaretOffsetFromCurrentLine > 0)
            {
                // Move to end of previous word
                string left = GetCurrentLine().Substring(0, CaretOffsetFromCurrentLine);
                MatchCollection matches = s_nextWord.Matches(left);
                if (matches.Count > 0)
                {
                    Match match = matches[matches.Count - 1];
                    count = match.Length;
                }
            }

            Prev(action, count);
        }

        private static void DefaultCommandHandler(string cmd)
        {

        }

        private static IEnumerable<string> DefaultSuggestionHandler(string obj, string trigger)
        {
            yield break;
        }

        private void InsertTextAtCaret(string text)
        {
            RemoveSelection();

            // Multiline prompts
            if (text.Contains(Environment.NewLine))
            {
                text = text.Replace(Environment.NewLine, Environment.NewLine + m_multilinePrompt);
                m_multiline = true;
            }

            int startIndex = Math.Max(CaretIndex, m_promptIndex);

            Text = Text.Insert(startIndex, text);
            SelectionStart = startIndex + text.Length;

            ScrollToCaret();
        }

        private void RemoveSelection()
        {
            if (SelectionLength > 0)
                RemoveText(SelectionStart, SelectionLength);
        }

        private void RemoveText(int startIndex, int count)
        {
            int start = MathUtil.Clamp(startIndex, m_promptIndex, TextLength);
            int end = MathUtil.Clamp(startIndex + count, m_promptIndex, TextLength);

            int length = end - start;
            if (length > 0)
                Text = Text.Remove(start, length);
            else
                SelectionLength = 0;

            SelectionStart = start;

            // Multiline prompts
            var text = GetTextAtPrompt();
            m_multiline = text.Contains(Environment.NewLine);

            ScrollToCaret();
        }

        private void RemoveTextAfterCaret(int count)
        {
            RemoveText(CaretIndex, count);
        }

        private void RemoveTextBeforeCaret(int count)
        {
            RemoveText(CaretIndex - count, count);
        }

        private void ReplaceTextAtPrompt(string text)
        {
            // Multiline prompts
            m_multiline = text.Contains(Environment.NewLine);
            if (m_multiline)
            {
                text = text.Replace(Environment.NewLine, Environment.NewLine + m_multilinePrompt);
            }

            Select(m_promptIndex, TextLength - m_promptIndex);
            SelectedText = text;
        }

        private CmdHandler m_commandHandler;
        private SuggestionHandler m_suggestionHandler;
        private readonly CommandList m_commandHistory = new CommandList();
        private bool m_multiline;
        private string m_multilinePrompt = "... ";
        private string m_prompt = ">>> ";
        private int m_promptIndex;
        private int m_spacesPerIndent = 4; // PEP-8: 4 spaces
        private readonly SuggestionListBox m_suggestionListBox;

        private static readonly char[] s_delimiters = { ' ', '\n', '\r', '\t' };
        private static readonly Regex s_nextWord = new Regex(@"\w+\s*|\W+", RegexOptions.Compiled);
        private static readonly char[] s_triggers = { '.', '[' };

        /// <summary>
        /// List of all the entered commands</summary>
        private class CommandList
        {
            /// <summary>
            /// Adds command to list</summary>
            /// <param name="cmd">Command to add</param>
            public void AddCommand(string cmd)
            {
                if (string.IsNullOrEmpty(cmd))
                    return;

                if (LastCommand != cmd)
                {
                    m_commands.Add(cmd);
                }
                m_currentPosition = m_commands.Count;
            }

            /// <summary>
            /// Gets previous command in list</summary>
            public string PreviousCommand
            {
                get
                {
                    string result = String.Empty;
                    if (m_currentPosition > 0)
                    {
                        result = m_commands[--m_currentPosition];
                    }
                    return result;
                }
            }

            /// <summary>
            /// Gets next command in list</summary>
            public string NextCommand
            {
                get
                {
                    string result = String.Empty;
                    if (m_currentPosition < (m_commands.Count - 1))
                    {
                        result = m_commands[++m_currentPosition];
                    }
                    else
                    {
                        m_currentPosition = m_commands.Count;
                    }
                    return result;
                }
            }

            /// <summary>
            /// Gets last command added to list</summary>
            private string LastCommand
            {
                get
                {
                    int count = m_commands.Count;
                    return (count > 0) ? m_commands[count - 1] : String.Empty;
                }
            }

            private int m_currentPosition = -1;
            private readonly List<string> m_commands = new List<string>();
        }

        private class SuggestionListBox : ListBox
        {
            public Action<string> InsertText { get; set; }
            public Action<int> RemoveText { get; set; }
            public Func<bool> Suggest { get; set; }

            public void SetSuggestions(string partial, IEnumerable<string> suggestions)
            {
                m_partial = partial;

                Items.Clear();
                Items.AddRange(suggestions.ToArray());
            }

            protected override void OnClick(EventArgs e)
            {
                base.OnClick(e);

                Complete();
            }

            protected override bool IsInputKey(Keys keyData)
            {
                // Allow 'Tab' to be used to auto-complete
                return (keyData == Keys.Tab) || base.IsInputKey(keyData);
            }

            protected override void OnKeyDown(KeyEventArgs e)
            {
                base.OnKeyDown(e);

                switch (e.KeyData)
                {
                    case Keys.Back:
                        RemoveText(1);
                        Suggest(); // TODO: Hide if there was nothing to remove
                        break;

                    case Keys.Down:
                        // Wrap selection around to first
                        if (SelectedIndex == Items.Count - 1)
                        {
                            SelectedIndex = 0;
                            e.Handled = true;
                        }
                        break;

                    case Keys.Up:
                        // Wrap selection around to last
                        if (SelectedIndex == 0)
                        {
                            SelectedIndex = Items.Count - 1;
                            e.Handled = true;
                        }
                        break;

                    case Keys.Enter:
                    case Keys.Tab:
                        Complete();
                        break;

                    case Keys.Escape:
                    case Keys.Left:
                    case Keys.Right:
                        Hide();
                        break;
                }
            }

            protected override void OnKeyPress(KeyPressEventArgs e)
            {
                base.OnKeyPress(e);

                // Check if character is printable
                char key = e.KeyChar;
                if (key >= ' ' && key <= '~')
                {
                    // Complete on triggers or continue suggesting
                    var character = key.ToString();
                    if (s_triggers.Contains(key))
                    {
                        Complete(character);
                    }
                    else
                    {
                        InsertText(character);
                        Suggest();
                    }
                }
            }

            protected override void OnLostFocus(EventArgs e)
            {
                base.OnLostFocus(e);

                Hide();
            }

            /// <param name="extra">Optional text to insert after completing.</param>
            private void Complete(string extra = null)
            {
                if (SelectedItem != null)
                {
                    RemoveText(m_partial.Length);

                    var text = SelectedItem.ToString();
                    InsertText(text);
                }

                // Continue suggesting when adding characters
                if (String.IsNullOrEmpty(extra))
                    Hide();
                else
                {
                    InsertText(extra);
                    Suggest();
                }
            }

            private string m_partial = String.Empty;
        }
    }
}
