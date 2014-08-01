//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Globalization;
using System.Windows.Forms;
using System.Drawing;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// TextBox control extended to edit numeric types</summary>
    public class NumericTextBox : TextBox
    {
        /// <summary>
        /// Constructor</summary>
        public NumericTextBox()
            : this(typeof(Single))
        {
        }

        /// <summary>
        /// Constructor with numeric type</summary>
        /// <param name="numericType">Type of number to edit</param>
        /// <remarks>All numeric types, except System.Decimal, are currently supported.</remarks>
        public NumericTextBox(Type numericType)
        {
            if (numericType != typeof(Int64) &&
                numericType != typeof(UInt64) &&
                numericType != typeof(Int32) &&
                numericType != typeof(UInt32) &&
                numericType != typeof(Int16) &&
                numericType != typeof(UInt16) &&
                numericType != typeof(SByte) &&
                numericType != typeof(Byte) &&
                numericType != typeof(Single) &&
                numericType != typeof(Double))
            {
                ThrowInvalidTypeException();
            }

            m_numericType = numericType;
        }

        /// <summary>
        /// Gets or sets the scale factor, which is used to scale the value for presentation to the
        /// user. The inverse factor is used to scale the user entered value back. Only applies to
        /// floating point types. The integer types are not scaled.</summary>
        public double ScaleFactor
        {
            get { return m_scaleFactor; }
            set
            {
                if (value == 0)
                    throw new ArgumentException("value must be non-zero");

                m_scaleFactor = value;
                Value = Value; // update value
            }
        }

        /// <summary>
        /// Gets or sets the current numeric value</summary>
        public object Value
        {
            get
            {
                if (m_numericType == typeof(Int64))
                    return m_int64Value;
                if (m_numericType == typeof(UInt64))
                    return m_uint64Value;
                if (m_numericType == typeof(Int32))
                    return m_int32Value;
                if (m_numericType == typeof(UInt32))
                    return m_uint32Value;
                if (m_numericType == typeof(Int16))
                    return m_int16Value;
                if (m_numericType == typeof(UInt16))
                    return m_uint16Value;
                if (m_numericType == typeof(SByte))
                    return m_sByteValue;
                if (m_numericType == typeof(Byte))
                    return m_byteValue;
                if (m_numericType == typeof(Single))
                    return m_singleValue;
                if (m_numericType == typeof(Double))
                    return m_doubleValue;

                // can't get here
                return null;
            }
            set
            {
                if (m_numericType != value.GetType())
                    ThrowInvalidTypeException();

                if (m_numericType == typeof(Single))
                    value = (Single)value * (Single)m_scaleFactor;
                else if (m_numericType == typeof(Double))
                    value = (Double)value * m_scaleFactor;

                m_lastEdit = value;

                string formattedText = ((IFormattable)value).ToString(null, CultureInfo.CurrentCulture);
                Text = formattedText;                
                // set values to be consistent in case there's no edit
                TryValidateText(formattedText);
            }
        }

        /// <summary>
        /// Gets the value of the last edit</summary>
        public object LastEdit
        {
            get { return m_lastEdit; }
        }

        /// <summary>
        /// Event that is raised after the value is edited</summary>
        public event EventHandler ValueEdited;

        /// <summary>
        /// Processes a dialog key</summary>
        /// <param name="keyData">One of the System.Windows.Forms.Keys values that represents the key to process</param>
        /// <returns>True iff the key was processed by the control</returns>
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                Flush();
               // Let parent handle Enter key.
               // Some editor might want to focus 
               // on next child On Enter.
               // return true;
            }
            else if (keyData == Keys.Escape)
            {
                Cancel();
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }

        /// <summary>
        /// Tests if key is an input key</summary>
        /// <param name="keyData">Key data</param>
        /// <returns>True iff key is an input key</returns>
        protected override bool IsInputKey(Keys keyData)
        {
            // ignore arrow keys and tab so they're available for containers, like our PropertyGridView
            if (keyData == Keys.Up ||
                keyData == Keys.Down || 
                (keyData & Keys.Tab) == Keys.Tab)
            {
                return false;
            }

            return base.IsInputKey(keyData);
        }

        /// <summary>
        /// Raises the <see cref="E:Sce.Atf.Controls.NumericTextBox.ValueEdited"/> event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnValueEdited(EventArgs e)
        {
            ValueEdited.Raise(this, e);
        }

        /// <summary>
        /// Raises the LostFocus event</summary>
        /// <param name="e">Event args</param>
        protected override void OnLostFocus(EventArgs e)
        {
            Flush();
            base.OnLostFocus(e);
        }

        private bool m_selectAll;
        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            m_selectAll = MouseButtons == MouseButtons.Left;

            if (MouseButtons == MouseButtons.None)
            {// select all, when tabbing into this control.
                SelectAll();
            }
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            m_selectAll = false;
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            if (m_selectAll && SelectionLength == 0)
            {
                m_selectAll = false;
                SelectAll();
                Focus();
            }

        }
       
        /// <summary>
        /// Performs custom actions on MouseDown events.
        /// Overrides the default behavior on a double-click so that all the text is selected, not
        /// just the consecutive sequence of digits. The default behavior would not select the leading
        /// '-' on a negative number, for example.
        /// Raises the System.Windows.Forms.Control.MouseDown event.</summary>
        /// <param name="e">A System.Windows.Forms.MouseEventArgs that contains the event data</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left &&
                e.Clicks == 2)
            {
                SelectAll();
                return;
            }
            base.OnMouseDown(e);
        }

        private void Flush()
        {
            string text = base.Text;
            if (TryValidateText(text))
            {
                bool equal;
                object value = Value;

                if (m_numericType == typeof(Single))
                {
                    equal =
                        m_lastEdit != null &&
                        MathUtil.AreApproxEqual((Single)m_lastEdit, (Single)value, 0.000001);
                }
                else if (m_numericType == typeof(Double))
                {
                    equal =
                        m_lastEdit != null &&
                        MathUtil.AreApproxEqual((Double)m_lastEdit, (Double)value, 0.000001);
                }
                else
                {
                    equal =
                        m_lastEdit != null &&
                        m_lastEdit.Equals(value);
                }

                if (!equal)
                {
                    OnValueEdited(EventArgs.Empty);
                    m_lastEdit = value;
                }
            }
            else
            {
                Cancel();
            }

            // Move the caret back to the beginning so that the most significant digits are shown.
            SelectionStart = 0;
            SelectionLength = 0;
        }

        private void Cancel()
        {
            if (m_lastEdit != null)
                Text = m_lastEdit.ToString();
            else
                Text = Value.ToString();
        }

        // use the type's parse function to validate the text; return true iff valid
        private bool TryValidateText(string text)
        {
            bool accept = false;

            if (m_numericType == typeof(Int64))
            {
                accept = Int64.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out m_int64Value);
            }
            else if (m_numericType == typeof(UInt64))
            {
                accept = UInt64.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out m_uint64Value);
            }
            else if (m_numericType == typeof(Int32))
            {
                accept = Int32.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out m_int32Value);
            }
            else if (m_numericType == typeof(UInt32))
            {
                accept = UInt32.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out m_uint32Value);
            }
            else if (m_numericType == typeof(Int16))
            {
                accept = Int16.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out m_int16Value);
            }
            else if (m_numericType == typeof(UInt16))
            {
                accept = UInt16.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out m_uint16Value);
            }
            else if (m_numericType == typeof(SByte))
            {
                accept = SByte.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out m_sByteValue);
            }
            else if (m_numericType == typeof(Byte))
            {
                accept = Byte.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out m_byteValue);
            }
            else if (m_numericType == typeof(Single))
            {
                accept = Single.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out m_singleValue);
                m_singleValue /= (float)m_scaleFactor;
            }
            else if (m_numericType == typeof(Double))
            {
                accept = Double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out m_doubleValue);
                m_doubleValue /= m_scaleFactor;
            }

            return accept;
        }

        private static void ThrowInvalidTypeException()
        {
            throw new ArgumentException("Unsupported numeric type");
        }

        private readonly Type m_numericType;
        private double m_scaleFactor = 1.0;

        private Int64 m_int64Value;
        private UInt64 m_uint64Value;
        private Int32 m_int32Value;
        private UInt32 m_uint32Value;
        private Int16 m_int16Value;
        private UInt16 m_uint16Value;
        private SByte m_sByteValue;
        private Byte m_byteValue;
        private Single m_singleValue;
        private Double m_doubleValue;

        private object m_lastEdit;
    }


    #region temp location until the code solidify

    /// <summary>
    /// Event argument for indicating the direction of spin.</summary>
    public class SpinDirectionEventArgs : EventArgs
    {
        /// <summary>
        /// Construct new instance with the given argument</summary>
        /// <param name="direction"> move direction</param>
        /// <param name="value">move value (direction and magnitude</param>
        public SpinDirectionEventArgs(int direction, int value)
        {
            Direction = direction;
            Value = value;
        }

        /// <summary>
        /// Direction of move, 
        /// 1 = right
        ///-1 = left</summary>
        public readonly int Direction;

        /// <summary>
        /// Value of the move.
        /// greater than zero = right
        /// less than zero = left.
        /// </summary>
        public readonly int Value;


    }

    /// <summary>
    /// This control is a replacement for TrackBar.
    /// It is more compact and does not hold data like trackbar.</summary>
    public class CompactSpinner : Control
    {
        /// <summary>
        /// Spin direction changed event</summary>
        public event EventHandler<SpinDirectionEventArgs> Changed = delegate { };
        /// <summary>
        /// Constructor</summary>
        public CompactSpinner()
        {
            this.DoubleBuffered = true;
            TabStop = false;
        }

        /// <summary>
        /// Handler for mouse enter event</summary>
        /// <param name="e">Event args</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            this.Cursor = Cursors.SizeWE;
        }

        /// <summary>
        /// Handler for mouse down event</summary>
        /// <param name="e">Mouse event args</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            this.Capture = true;
            base.OnMouseDown(e);
            m_currentMouseX = e.X;

        }

        private int m_currentMouseX;
        /// <summary>
        /// Handler for mouse move event</summary>
        /// <param name="e">Mouse event args</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                int val = e.X - m_currentMouseX;
                int direction = Math.Sign(val);
                m_currentMouseX = e.X;
                if (direction != 0)
                {
                    Changed(this, new SpinDirectionEventArgs(direction,val));
                }
            }

        }

        /// <summary>
        /// Paint event handler</summary>
        /// <param name="e">Paint event args</param>
        protected override void OnPaint(PaintEventArgs e)
        {            
            int midy = this.Height / 2;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (Pen pen = new Pen(Color.LightBlue))
            {
                pen.Width = 4;
                pen.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
                pen.StartCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
                e.Graphics.DrawLine(pen, 0, midy, Width, midy);                
            }          
        }        
    }

    #endregion
}
