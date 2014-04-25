//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using Sce.Atf.Direct2D;

namespace Sce.Atf.DirectWrite
{
    /// <summary>
    /// Text editing, selecting using DirectWrite </summary>
    /// <remarks>The implementation detail is based on Microsoft PadWrite C++ sample:
    /// http://msdn.microsoft.com/en-us/library/windows/desktop/dd941792%28v=vs.85%29.aspx
    /// </remarks>
    public class TextEditor
    {
        /// <summary>
        /// Selection mode enumeration</summary>
        public  enum SelectionMode
        {
            /// <summary>Select cluster of characters to the left</summary>
            Left,
            /// <summary>Select cluster of characters to the right</summary>
            Right,
            /// <summary>Select characters a line up</summary>
            Up,
            /// <summary>Select characters a line down</summary>
            Down,
            /// <summary>Select single character to left (backspace uses it)</summary>
            LeftChar,
            /// <summary>Select single character to right</summary>
            RightChar,          // single character right
            /// <summary>Select single word to left</summary>
            LeftWord,
            /// <summary>Select single word to right</summary>
            RightWord,
            /// <summary>Select single word</summary>
            SingleWord,
            /// <summary>Select characters from front of line</summary>
            Home,
            /// <summary>Select characters from end of line</summary>
            End,
            /// <summary>Select characters from very first position</summary>
            First,
            /// <summary>Select characters from very last position</summary>
            Last,
            /// <summary>Set caret position to absolute explicit position (for mouse click)</summary>
            AbsoluteLeading,
            /// <summary>Set caret position to explicit position, trailing edge</summary>
            AbsoluteTrailing,
            /// <summary>Select all text</summary>
            All
        }

        /// <summary>
        /// Gets or sets formatted text</summary>
        public D2dTextLayout TextLayout { get; set; }

        /// <summary>
        /// Gets or sets text format object used for text layout</summary>
        public D2dTextFormat TextFormat { get; set; }
          
        /// <summary>
        /// Gets or sets starting position of text selected for editing</summary>
        public int SelectionStart { get; set; }  
        
        /// <summary>
        /// Gets or sets number of characters selected for editing</summary>
        public int SelectionLength { get; set; }

        /// <summary>
        /// Gets position of input caret</summary>
        public int CaretPosition
        {
            get { return m_caretPosition; }

        }

        /// <summary>
        /// Gets anchor position of input caret</summary>
        public int CaretAnchorPosition
        {
            get { return m_caretAnchor; }

        }

        /// <summary>
        /// Gets absolute position of input caret</summary>
        public int CaretAbsolutePosition
        {
            get { return m_caretPosition + m_caretPositionOffset; }

        }

        /// <summary>
        /// Gets or sets top visible line number</summary>
        public int TopLine { get; set; }

        /// <summary>
        /// Gets or sets whether the vertical scroll bar should be visible</summary>
        public bool VerticalScrollBarVisibe { get; set; }

        /// <summary>
        /// Updates the text position corresponding to the position x,y in graph space</summary>
        /// <param name="x">Mouse position x in graph space</param>
        /// <param name="y">Mouse position y in graph space</param>
        /// <param name="extendSelection">Whether to extend current selection to additional selection</param>
        /// <remarks>If hitting the trailing side of a cluster, return the
        /// leading edge of the following text position.</remarks>
        public void SetSelectionFromPoint(float x, float y, bool extendSelection)
        {
            var caretMetrics = TextLayout.HitTestPoint(x, y);
            // Update current selection according to click or mouse drag.
            SetSelection(
                caretMetrics.IsTrailingHit ? SelectionMode.AbsoluteTrailing : SelectionMode.AbsoluteLeading,
                caretMetrics.TextPosition, extendSelection,
                true //updateCaretFormat
                );
            UpdateSelectionRange();
        }

       
        private void AlignCaretToNearestCluster(bool isTrailingHit, bool skipZeroWidth)
        {
            // Uses hit-testing to align the current caret position to a whole cluster,
            // rather than residing in the middle of a base character + diacritic,
            // surrogate pair, or character + UVS.

            // Align the caret to the nearest whole cluster.
            HitTestMetrics hitTestMetrics = TextLayout.HitTestTextPosition(m_caretPosition, false);
        
            // The caret position itself is always the leading edge.
            // An additional offset indicates a trailing edge when non-zero.
            // This offset comes from the number of code-units in the
            // selected cluster or surrogate pair.
            m_caretPosition = hitTestMetrics.TextPosition;
            m_caretPositionOffset = (isTrailingHit) ? hitTestMetrics.Length : 0;

            // For invisible, zero-width characters (like line breaks
            // and formatting characters), force leading edge of the
            // next position.
            if (skipZeroWidth && hitTestMetrics.Width == 0)
            {
                m_caretPosition += m_caretPositionOffset;
                m_caretPositionOffset = 0;
            }
        }

        /// <summary>
        /// Sets text selection. This may possibly only move the caret, not selecting characters.</summary>
        /// <param name="moveMode">Text selection mode</param>
        /// <param name="advance">Number of characters to advance or start selection</param>
        /// <param name="extendSelection">Whether to extend current selection to additional selection</param>
        /// <param name="updateCaretFormat">Whether to update caret format based on selection</param>
        /// <returns>True iff caret changed position as result of selection</returns>
        public bool SetSelection(SelectionMode moveMode, int advance, bool extendSelection, bool updateCaretFormat)
        {

            // Moves the caret relatively or absolutely, optionally extending the
            // selection range (for example, when shift is held).

            int line = int.MaxValue; // current line number, needed by a few modes
            int absolutePosition = m_caretPosition + m_caretPositionOffset;
            int oldAbsolutePosition = absolutePosition;
            int oldCaretAnchor = m_caretAnchor;

            switch (moveMode)
            {
                case SelectionMode.Left:
                    m_caretPosition += m_caretPositionOffset;
                    if (m_caretPosition > 0)
                    {
                        --m_caretPosition;
                        AlignCaretToNearestCluster(false, true);

                        // special check for CR/LF pair
                        absolutePosition = m_caretPosition + m_caretPositionOffset;
                        if (absolutePosition >= 1
                        && absolutePosition < TextLayout.Text.Length
                        && TextLayout.Text[absolutePosition - 1] == '\r'
                        && TextLayout.Text[absolutePosition] == '\n')
                        {
                            m_caretPosition = absolutePosition - 1;
                            AlignCaretToNearestCluster(false, true);
                        }
                    }
                    break;

                case SelectionMode.Right:
                    m_caretPosition = absolutePosition;
                    AlignCaretToNearestCluster(true, true);

                    // special check for CR/LF pair
                    absolutePosition = m_caretPosition + m_caretPositionOffset;
                    if (absolutePosition >= 1
                    && absolutePosition < TextLayout.Text.Length
                    && TextLayout.Text[absolutePosition - 1] == '\r'
                    && TextLayout.Text[absolutePosition] == '\n')
                    {
                        m_caretPosition = absolutePosition + 1;
                        AlignCaretToNearestCluster(false, true);
                    }
                    break;

                case SelectionMode.LeftChar:
                    m_caretPosition = absolutePosition;
                    m_caretPosition -= Math.Min(advance, absolutePosition);
                    m_caretPositionOffset = 0;
                    break;

                case SelectionMode.RightChar:
                    m_caretPosition = absolutePosition + advance;
                    m_caretPositionOffset = 0;
                    {
                        // Use hit-testing to limit text position.         
                        HitTestMetrics hitTestMetrics = TextLayout.HitTestTextPosition(
                            m_caretPosition,
                            false);
                        m_caretPosition = Math.Min(m_caretPosition, hitTestMetrics.TextPosition + hitTestMetrics.Length);
                    }
                    break;

                case SelectionMode.Up:
                case SelectionMode.Down:
                    {
                        // Retrieve the line metrics to figure out what line we are on.
                        var lineMetrics = TextLayout.GetLineMetrics();
                        int linePosition;
                        GetLineFromPosition(lineMetrics, m_caretPosition, out  line, out linePosition);

                        // Move up a line or down
                        if (moveMode == SelectionMode.Up)
                        {
                            if (line <= 0)
                                break; // already top line
                            line--;
                            linePosition -= lineMetrics[line].Length;
                        }
                        else
                        {
                            linePosition += lineMetrics[line].Length;
                            line++;
                            if (line >= lineMetrics.Length)
                                break; // already bottom line
                        }

                        // To move up or down, we need three hit-testing calls to determine:
                        // 1. The x of where we currently are.
                        // 2. The y of the new line.
                        // 3. New text position from the determined x and y.
                        // This is because the characters are variable size.


                        float caretX, caretY;
                        // Get x of current text position
                        var hitTestMetrics = TextLayout.HitTestTextPosition(
                           m_caretPosition,
                           m_caretPositionOffset > 0 // trailing if nonzero, else leading edge           
                           );
                        caretX = hitTestMetrics.Point.X;

                        // Get y of new position
                        hitTestMetrics = TextLayout.HitTestTextPosition(
                           linePosition,
                           false// leading edge            
                           );
                        caretY = hitTestMetrics.Point.Y;

                        // Now get text position of new x,y
                        hitTestMetrics = TextLayout.HitTestPoint(caretX, caretY);


                        m_caretPosition = hitTestMetrics.TextPosition;
                        m_caretPositionOffset = hitTestMetrics.IsTrailingHit ? (hitTestMetrics.Length > 0) ? 1 : 0 : 0;
                    }
                    break;

                case SelectionMode.LeftWord:
                case SelectionMode.RightWord:
                    {
                        // To navigate by whole words, we look for the canWrapLineAfter
                        // flag in the cluster metrics.

                        // Now we actually read them.
                        var clusterMetrics = TextLayout.GetClusterMetrics();
                        if (clusterMetrics.Length == 0)
                            break;

                        m_caretPosition = absolutePosition;

                        int clusterPosition = 0;
                        int oldCaretPosition = m_caretPosition;

                        if (moveMode == SelectionMode.LeftWord)
                        {
                            // Read through the clusters, keeping track of the farthest valid
                            // stopping point just before the old position.
                            m_caretPosition = 0;
                            m_caretPositionOffset = 0; // leading edge
                            for (int cluster = 0; cluster < clusterMetrics.Length; ++cluster)
                            {
                                clusterPosition += clusterMetrics[cluster].Length;
                                if (clusterMetrics[cluster].CanWrapLineAfter)
                                {
                                    if (clusterPosition >= oldCaretPosition)
                                        break;

                                    // Update in case we pass this point next loop.
                                    m_caretPosition = clusterPosition;
                                }
                            }
                        }
                        else // SetSelectionModeRightWord
                        {
                            // Read through the clusters, looking for the first stopping point
                            // after the old position.
                            for (int cluster = 0; cluster < clusterMetrics.Length; ++cluster)
                            {
                                int clusterLength = clusterMetrics[cluster].Length;
                                m_caretPosition = clusterPosition;
                                m_caretPositionOffset = clusterLength; // trailing edge
                                if (clusterPosition >= oldCaretPosition && clusterMetrics[cluster].CanWrapLineAfter)
                                    break; // first stopping point after old position.

                                clusterPosition += clusterLength;
                            }
                        }
                    }
                    break;
                case SelectionMode.SingleWord:
                    {
                        var clusterMetrics = TextLayout.GetClusterMetrics();
                        if (clusterMetrics.Length == 0)
                            break;

                        // Left of word
                        m_caretPosition = absolutePosition;

                        int clusterPosition = 0;
                        int oldCaretPosition = m_caretPosition;

                        // Read through the clusters, keeping track of the farthest valid
                        // stopping point just before the old position.
                        m_caretPosition = 0;
                        m_caretPositionOffset = 0; // leading edge
                        for (int cluster = 0; cluster < clusterMetrics.Length; ++cluster)
                        {
                            clusterPosition += clusterMetrics[cluster].Length;
                            if (clusterMetrics[cluster].CanWrapLineAfter)
                            {
                                if (clusterPosition >= oldCaretPosition)
                                    break;

                                // Update in case we pass this point next loop.
                                m_caretPosition = clusterPosition;
                            }
                        }

                        int leftOfWord = m_caretPosition;


                        // Right of word
                        // Read through the clusters, looking for the first stopping point
                        // after the old position.
                        for (int cluster = 0; cluster < clusterMetrics[cluster].Length; ++cluster)
                        {
                            int clusterLength = clusterMetrics[cluster].Length;
                            m_caretPosition = clusterPosition;
                            m_caretPositionOffset = clusterLength; // trailing edge
                            if (clusterPosition >= oldCaretPosition && clusterMetrics[cluster].CanWrapLineAfter)
                                break; // first stopping point after old position.

                            clusterPosition += clusterLength;
                        }              

                        int rightOfWord = m_caretPosition -1;
                        m_caretPositionOffset = 0;
                        m_caretAnchor = leftOfWord;

                        //while (rightOfWord > leftOfWord)
                        //{
                        //    char c = TextLayout.Text[rightOfWord];
                        //    if (!(char.IsWhiteSpace(c) || char.IsPunctuation(c)))
                        //        break;
                        //    --rightOfWord;
                        //}
                        m_caretPosition = rightOfWord;
                       

                    }
                    break;

                case SelectionMode.Home:
                case SelectionMode.End:
                    {
                        // Retrieve the line metrics to know first and last position
                        // on the current line.
                        var lineMetrics = TextLayout.GetLineMetrics();
                        int linePosition;
                        GetLineFromPosition(lineMetrics, m_caretPosition, out  line, out linePosition);
                        m_caretPosition = linePosition;

                        m_caretPositionOffset = 0;
                        if (moveMode == SelectionMode.End)
                        {
                            // Place the caret at the last character on the line,
                            // excluding line breaks. In the case of wrapped lines,
                            // newlineLength will be 0.
                            int lineLength = lineMetrics[line].Length - lineMetrics[line].NewlineLength;
                            m_caretPositionOffset = Math.Min(lineLength, 1);
                            m_caretPosition += lineLength - m_caretPositionOffset;
                            AlignCaretToNearestCluster(true, false);
                        }
                    }
                    break;

                case SelectionMode.First:
                    m_caretPosition = 0;
                    m_caretPositionOffset = 0;
                    break;

                case SelectionMode.All:
                    m_caretAnchor = 0;
                    extendSelection = true;
                    goto fallthrough;
 
                case SelectionMode.Last:
            fallthrough:
                    m_caretPosition = int.MaxValue;
                    m_caretPositionOffset = 0;
                    AlignCaretToNearestCluster(true, false);
                    break;

                case SelectionMode.AbsoluteLeading:
                    m_caretPosition = advance;
                    m_caretPositionOffset = 0;
                    break;

                case SelectionMode.AbsoluteTrailing:
                    m_caretPosition = advance;
                    AlignCaretToNearestCluster(true, false);
                    break;
            }

            absolutePosition = m_caretPosition + m_caretPositionOffset;

            if (!extendSelection)
                m_caretAnchor = absolutePosition;

            bool caretMoved = (absolutePosition != oldAbsolutePosition)
                           || (m_caretAnchor != oldCaretAnchor);

            if (caretMoved)
            {
                //// update the caret formatting attributes
                //if (updateCaretFormat)
                //    UpdateCaretFormatting();

                //PostRedraw();

                //RectF rect;
                GetCaretRect();
                //UpdateSystemCaret(rect);
                //UpdateSelectionRange();
            }

            //Trace.TraceInformation("caretMoved {0} caretPosition  {1} caretPositionOffset {2} caretAnchor  {3} ",
            //                       caretMoved, m_caretPosition, m_caretPositionOffset, m_caretAnchor);

            return caretMoved;



        }

      
       
        /// <summary>
        /// Obtains the current caret position (in untransformed space)</summary>
        public RectangleF GetCaretRect()
        {
             if (TextLayout == null)
                return new RectangleF();
          
            var caretMetrics = TextLayout.HitTestTextPosition(m_caretPosition, m_caretPositionOffset > 0);
            float caretX = caretMetrics.Point.X;
            float caretY = caretMetrics.Point.Y;
            UpdateSelectionRange();
            // If a selection exists, draw the caret using the
            // line size rather than the font size.
            if (SelectionLength > 0)
            {
                var lineMetrics = TextLayout.HitTestTextRange(m_caretPosition, 0, 0, 0);
                caretY = lineMetrics[0].Top;
            }

            const int caretThickness = 1;
            return new RectangleF(caretX - caretThickness/2.0f, caretY, caretThickness, caretMetrics.Height);                     
        }

        /// <summary>
        /// Returns a valid range of the current selection,
        /// regardless of whether the caret or anchor is first</summary>
        public void UpdateSelectionRange()
        {
            int caretBegin = m_caretAnchor;
            int caretEnd = m_caretPosition + m_caretPositionOffset;
            if (caretBegin > caretEnd)
            {
                int temp = caretEnd;
                caretEnd = caretBegin;
                caretBegin = temp;
            }

            // Limit to actual text length.    
            caretBegin = Math.Min(caretBegin,   TextLayout.Text.Length);
            caretEnd = Math.Min(caretEnd, TextLayout.Text.Length);
            SelectionStart = caretBegin;
            SelectionLength = caretEnd - caretBegin;
        }

        /// <summary>
        /// Inserts text at current caret position</summary>
        /// <param name="originalText">Original text in editor</param>
        /// <param name="textToInsert">Text to insert</param>
        /// <returns>Text after insertion occurs</returns>
        public string InsertTextAt(string originalText, string textToInsert)
        {
            int absolutePosition = m_caretPosition + m_caretPositionOffset;
            int insertPosition = Math.Min(absolutePosition, originalText.Length);
            string newValue = originalText.Insert(insertPosition, textToInsert);
            RecreateLayout(newValue);
            return newValue;
        }

        /// <summary>
        /// Removes text with given length and position</summary>
        /// <param name="originalText">Original text in editor</param>
        /// <param name="startPosition">Position at which to remove text</param>
        /// <param name="length">Number of characters to remove</param>
        /// <returns>Text after deletion occurs</returns>
        public string RemoveTextAt(string originalText, int startPosition, int length)
        {
            startPosition = Math.Min(startPosition, originalText.Length);
            length = Math.Min(originalText.Length - startPosition, length);
            string newValue = originalText.Remove(startPosition, length);
            RecreateLayout(newValue);
            return newValue;
        }

        /// <summary>
        /// Resets the text in the editor</summary>
        /// <param name="newText">New text for editor</param>
        public void ResetText(string newText)
        {
            RecreateLayout(newText);
            // Limit to actual text length.    
            m_caretPosition = Math.Min(m_caretPosition, TextLayout.Text.Length);
            m_caretPositionOffset = 0;
            m_caretAnchor = m_caretPosition;
            UpdateSelectionRange();
        }

        private void RecreateLayout(string text)
        {
            var textLayout = D2dFactory.CreateTextLayout(text, TextFormat, TextLayout.LayoutWidth, TextLayout.LayoutHeight);
            if (TextFormat.Underlined)
                textLayout.SetUnderline(true, 0, text.Length);
            if (TextFormat.Strikeout)
                textLayout.SetStrikethrough(true, 0, text.Length);
            TextLayout.Dispose();
            TextLayout = textLayout;
        }

        /// <summary>
        /// Given the line metrics, determines the current line and starting text
        /// position of that line by summing up the lengths. When the starting
        /// line position is beyond the given text position, we have our line.</summary>
        /// <param name="lineMetrics">Line metrics</param>
        /// <param name="textPosition">Text position</param>
        /// <param name="line">Line text</param>
        /// <param name="linePosition">Line position</param>
        private void  GetLineFromPosition(LineMetrics[] lineMetrics, int textPosition, out int line, out int linePosition)
        {
            line = 0;
            linePosition = 0;
            int nextLinePosition = 0;

            for (; line < lineMetrics.Length; ++line)
            {
                linePosition = nextLinePosition;
                nextLinePosition = linePosition + lineMetrics[line].Length;
                if (nextLinePosition > textPosition)
                {
                    // The next line is beyond the desired text position,
                    // so it must be in the current line.
                    break;
                }
            }

            // If there is no text at all, the above logic will leave 'line' with a value of 1
            //  which can cause an index-out-of-bounds exception in the caller.
            if (line >= lineMetrics.Length)
                line = lineMetrics.Length - 1;
        }


        // caretAnchor equals caretPosition when there is no selection.
        // Otherwise, the anchor holds the point where shift was held
        // or left drag started.
        //
        // The offset is used as a sort of trailing edge offset from
        // the caret position. For example, placing the caret on the
        // trailing side of a surrogate pair at the beginning of the
        // text would place the position at zero and offset of two.
        // So to get the absolute leading position, sum the two.
        private int m_caretAnchor;
        private int m_caretPosition;
        private int m_caretPositionOffset;    // > 0 used for trailing edge
    }
}
