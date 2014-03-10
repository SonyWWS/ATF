//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Represents a combination of a standard button on the left and a drop-down button on the right
    /// that is not limited to Toolstrip</summary>
    /// <remarks>ToolStripSplitButton is managed only by Toolstrip</remarks>
    public class SplitButton : Button
    {
        /// <summary>
        /// Constructor</summary>
         public SplitButton()
        {
            AutoSize = true;
        }

        /// <summary>
        /// Sets whether to show split</summary>
        [DefaultValue(true)]
        public bool ShowSplit
        {
            set
            {
                if (value != m_showSplit)
                {
                    m_showSplit = value;
                    Invalidate();
                    if (Parent != null)
                    {
                        Parent.PerformLayout();
                    }
                }
            }
        }

        private PushButtonState MState
        {
            get { return m_state; }
            set
            {
                if (!m_state.Equals(value))
                {
                    m_state = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets preferred split button size</summary>
        /// <param name="proposedSize">Suggested size</param>
        /// <returns>Preferred size</returns>
        public override Size GetPreferredSize(Size proposedSize)
        {
            Size preferredSize = base.GetPreferredSize(proposedSize);
            if (m_showSplit && !string.IsNullOrEmpty(Text) &&
                TextRenderer.MeasureText(Text, Font).Width + PushButtonWidth > preferredSize.Width)
            {
                return preferredSize + new Size(PushButtonWidth + BorderSize*2, 0);
            }
            return preferredSize;
        }

        /// <summary>
        /// Tests if key is a regular input key or a special key that requires preprocessing</summary>
        /// <param name="keyData">Key to test</param>
        /// <returns>True iff key is input key</returns>
        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData.Equals(Keys.Down) && m_showSplit)
            {
                return true;
            }
            else
            {
                return base.IsInputKey(keyData);
            }
        }

        /// <summary>
        /// Raises the GotFocus event</summary>
        /// <param name="e">A System.EventArgs that contains the event data</param>
        protected override void OnGotFocus(EventArgs e)
        {
            if (!m_showSplit)
            {
                base.OnGotFocus(e);
                return;
            }

            if (!MState.Equals(PushButtonState.Pressed) && !MState.Equals(PushButtonState.Disabled))
            {
                MState = PushButtonState.Default;
            }
        }

        /// <summary>
        /// Raises the KeyDown event</summary>
        /// <param name="kevent">KeyEventArgs that contains the event data</param>
        protected override void OnKeyDown(KeyEventArgs kevent)
        {
            if (m_showSplit)
            {
                if (kevent.KeyCode.Equals(Keys.Down))
                {
                    ShowContextMenuStrip();
                }
                else if (kevent.KeyCode.Equals(Keys.Space) && kevent.Modifiers == Keys.None)
                {
                    MState = PushButtonState.Pressed;
                }
            }

            base.OnKeyDown(kevent);
        }

        /// <summary>
        /// Raises the KeyUp event</summary>
        /// <param name="kevent">KeyEventArgs that contains the event data</param>
        protected override void OnKeyUp(KeyEventArgs kevent)
        {
            if (kevent.KeyCode.Equals(Keys.Space))
            {
                if (MouseButtons == MouseButtons.None)
                {
                    MState = PushButtonState.Normal;
                }
            }
            base.OnKeyUp(kevent);
        }

        /// <summary>
        /// Raises the LostFocus event</summary>
        /// <param name="e">EventArgs that contains the event data</param>
        protected override void OnLostFocus(EventArgs e)
        {
            if (!m_showSplit)
            {
                base.OnLostFocus(e);
                return;
            }
            if (!MState.Equals(PushButtonState.Pressed) && !MState.Equals(PushButtonState.Disabled))
            {
                MState = PushButtonState.Normal;
            }
        }

        /// <summary>
        /// Raises the MouseDown event</summary>
        /// <param name="e">MouseEventArgs that contains the event data</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!m_showSplit)
            {
                base.OnMouseDown(e);
                return;
            }

            if (m_dropDownRectangle.Contains(e.Location))
            {
                ShowContextMenuStrip();
            }
            else
            {
                MState = PushButtonState.Pressed;
            }
        }

        /// <summary>
        /// Raises the MouseEnter event</summary>
        /// <param name="e">EventArgs that contains the event data</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            if (!m_showSplit)
            {
                base.OnMouseEnter(e);
                return;
            }

            if (!MState.Equals(PushButtonState.Pressed) && !MState.Equals(PushButtonState.Disabled))
            {
                MState = PushButtonState.Hot;
            }
        }

        /// <summary>
        /// Raises the MouseLeave event</summary>
        /// <param name="e">EventArgs that contains the event data</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            if (!m_showSplit)
            {
                base.OnMouseLeave(e);
                return;
            }

            if (!MState.Equals(PushButtonState.Pressed) && !MState.Equals(PushButtonState.Disabled))
            {
                if (Focused)
                {
                    MState = PushButtonState.Default;
                }
                else
                {
                    MState = PushButtonState.Normal;
                }
            }
        }

        /// <summary>
        /// Raises the MouseUp event</summary>
        /// <param name="mevent">MouseEventArgs that contains the event data</param>
        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            if (!m_showSplit)
            {
                base.OnMouseUp(mevent);
                return;
            }

            if (ContextMenuStrip == null || !ContextMenuStrip.Visible)
            {
                SetButtonDrawState();
                if (Bounds.Contains(Parent.PointToClient(Cursor.Position)) &&
                    !m_dropDownRectangle.Contains(mevent.Location))
                {
                    OnClick(new EventArgs());
                }
            }
        }

        /// <summary>
        /// Raises the Paint event</summary>
        /// <param name="pevent">PaintEventArgs that contains the event data</param>
        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);

            if (!m_showSplit)
            {
                return;
            }

            Graphics g = pevent.Graphics;
            Rectangle bounds = ClientRectangle;

            // draw the button background as according to the current state.
            if (MState != PushButtonState.Pressed && IsDefault && !Application.RenderWithVisualStyles)
            {
                Rectangle backgroundBounds = bounds;
                backgroundBounds.Inflate(-1, -1);
                ButtonRenderer.DrawButton(g, backgroundBounds, MState);

                // button renderer doesnt draw the black frame when themes are off =(
                g.DrawRectangle(SystemPens.WindowFrame, 0, 0, bounds.Width - 1, bounds.Height - 1);
            }
            else
            {
                ButtonRenderer.DrawButton(g, bounds, MState);
            }
            // calculate the current dropdown rectangle.
            m_dropDownRectangle = new Rectangle(bounds.Right - PushButtonWidth - 1, BorderSize, PushButtonWidth,
                                              bounds.Height - BorderSize*2);

            int internalBorder = BorderSize;
            var focusRect =
                new Rectangle(internalBorder,
                              internalBorder,
                              bounds.Width - m_dropDownRectangle.Width - internalBorder,
                              bounds.Height - (internalBorder*2));

            bool drawSplitLine = (MState == PushButtonState.Hot || MState == PushButtonState.Pressed ||
                                  !Application.RenderWithVisualStyles);

            if (RightToLeft == RightToLeft.Yes)
            {
                m_dropDownRectangle.X = bounds.Left + 1;
                focusRect.X = m_dropDownRectangle.Right;
                if (drawSplitLine)
                {
                    // draw two lines at the edge of the dropdown button
                    g.DrawLine(SystemPens.ButtonShadow, bounds.Left + PushButtonWidth, BorderSize,
                               bounds.Left + PushButtonWidth, bounds.Bottom - BorderSize);
                    g.DrawLine(SystemPens.ButtonFace, bounds.Left + PushButtonWidth + 1, BorderSize,
                               bounds.Left + PushButtonWidth + 1, bounds.Bottom - BorderSize);
                }
            }
            else
            {
                if (drawSplitLine)
                {
                    // draw two lines at the edge of the dropdown button
                    g.DrawLine(SystemPens.ButtonShadow, bounds.Right - PushButtonWidth, BorderSize,
                               bounds.Right - PushButtonWidth, bounds.Bottom - BorderSize);
                    g.DrawLine(SystemPens.ButtonFace, bounds.Right - PushButtonWidth - 1, BorderSize,
                               bounds.Right - PushButtonWidth - 1, bounds.Bottom - BorderSize);
                }
            }

            // Draw an arrow in the correct location 
            PaintArrow(g, m_dropDownRectangle);

            // Figure out how to draw the text
            TextFormatFlags formatFlags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;

            // If we dont' use mnemonic, set formatFlag to NoPrefix as this will show ampersand.
            if (!UseMnemonic)
            {
                formatFlags = formatFlags | TextFormatFlags.NoPrefix;
            }
            else if (!ShowKeyboardCues)
            {
                formatFlags = formatFlags | TextFormatFlags.HidePrefix;
            }

            if (!string.IsNullOrEmpty(Text))
            {
                TextRenderer.DrawText(g, Text, Font, focusRect, SystemColors.ControlText, formatFlags);
            }

            // draw the focus rectangle.

            if (MState != PushButtonState.Pressed && Focused)
            {
                ControlPaint.DrawFocusRectangle(g, focusRect);
            }
        }

        private void PaintArrow(Graphics g, Rectangle dropDownRect)
        {
            var middle = new Point(Convert.ToInt32(dropDownRect.Left + dropDownRect.Width/2),
                                   Convert.ToInt32(dropDownRect.Top + dropDownRect.Height/2));

            //if the width is odd - favor pushing it over one pixel right.
            middle.X += (dropDownRect.Width%2);

            var arrow = new[]
                            {
                                new Point(middle.X - 2, middle.Y - 1), new Point(middle.X + 3, middle.Y - 1),
                                new Point(middle.X, middle.Y + 2)
                            };

            g.FillPolygon(SystemBrushes.ControlText, arrow);
        }

        private void ShowContextMenuStrip()
        {
            if (m_skipNextOpen)
            {
                // we were called because we're closing the context menu strip
                // when clicking the dropdown button.
                m_skipNextOpen = false;
                return;
            }
            MState = PushButtonState.Pressed;

            if (ContextMenuStrip != null)
            {
                ContextMenuStrip.Closing += ContextMenuStrip_Closing;
                ContextMenuStrip.Show(this, new Point(0, Height), ToolStripDropDownDirection.BelowRight);
            }
        }

        private void ContextMenuStrip_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            var cms = sender as ContextMenuStrip;
            if (cms != null)
            {
                cms.Closing -= ContextMenuStrip_Closing;
            }

            SetButtonDrawState();

            if (e.CloseReason == ToolStripDropDownCloseReason.AppClicked)
            {
                m_skipNextOpen = (m_dropDownRectangle.Contains(PointToClient(Cursor.Position)));
            }
        }


        private void SetButtonDrawState()
        {
            if (Bounds.Contains(Parent.PointToClient(Cursor.Position)))
            {
                MState = PushButtonState.Hot;
            }
            else if (Focused)
            {
                MState = PushButtonState.Default;
            }
            else
            {
                MState = PushButtonState.Normal;
            }
        }

        private const int PushButtonWidth = 14;
        private static readonly int BorderSize = SystemInformation.Border3DSize.Width * 2;

        private PushButtonState m_state;
        private bool m_skipNextOpen;
        private Rectangle m_dropDownRectangle;
        private bool m_showSplit = true;


    }
}

