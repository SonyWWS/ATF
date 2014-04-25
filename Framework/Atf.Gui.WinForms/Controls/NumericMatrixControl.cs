//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Control for editing a numeric matrix</summary>
    public class NumericMatrixControl : Control
    {
        /// <summary>
        /// Constructor</summary>
        /// <remarks>Default is a 4 X 4 matrix of type Single</remarks>
        public NumericMatrixControl()
            : this(typeof(Single), 4, 4)
        {
        }

        /// <summary>
        /// Constructor specifying matrix element type and dimensions</summary>
        /// <param name="numericType">Numeric type of matrix elements</param>
        /// <param name="rows">Number of matrix rows</param>
        /// <param name="columns">Number of matrix columns</param>
        /// <remarks>All numeric types, except Decimal, are supported.</remarks>
        public NumericMatrixControl(Type numericType, int rows, int columns)
        {
            Define(numericType, rows, columns);
        }

        /// <summary>
        /// Sets the matrix element type and the dimensions of the matrix</summary>
        /// <param name="numericType">Numeric type of matrix elements</param>
        /// <param name="rows">Number of matrix rows</param>
        /// <param name="columns">Number of matrix columns</param>
        /// <remarks>All numeric types, except Decimal, are supported</remarks>
        public void Define(Type numericType, int rows, int columns)
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
                throw new ArgumentException("Unsupported numeric type");
            }

            if (rows < 1 || columns < 1)
                throw new ArgumentException("Must have at least 1 row and column in the matrix");

            m_numericType = numericType;
            m_rows = rows;
            m_columns = columns;

            for (int i = 0; i < Controls.Count; )
            {
                Control control = Controls[0];
                control.Parent = null;
                control.Dispose();
            }

            SuspendLayout();
            NumericTextBox textBox = null;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    textBox = new NumericTextBox(m_numericType);
                    textBox.BorderStyle = BorderStyle.None;
                    textBox.Name = "M" + i + j;
                    textBox.TabStop = true;
                    textBox.TabIndex = i;
                    textBox.ScaleFactor = m_scaleFactor;
                    textBox.ValueEdited += textBox_ValueEdited;
                    Controls.Add(textBox);
                }
            }
            // TODO needs dpi adjustment
            Size = new Size(80 * columns, textBox.Height * rows - 1);
            ResumeLayout();
        }

        /// <summary>
        /// Gets or sets the scale factor, which is used to scale the value for presentation to the
        /// user. The inverse factor is used to scale the user entered value back.</summary>
        public double ScaleFactor
        {
            get { return m_scaleFactor; }
            set
            {
                if (value == 0)
                    throw new ArgumentException("value must be non-zero");

                m_scaleFactor = value;
                foreach (NumericTextBox textBox in Controls)
                    textBox.ScaleFactor = m_scaleFactor;
            }
        }

        /// <summary>
        /// Gets or sets matrix value as array</summary>
        public virtual object Value
        {
            get
            {
                if (m_numericType == typeof(Int64))
                    return GetValue<Int64>();
                if (m_numericType == typeof(UInt64))
                    return GetValue<UInt64>();
                if (m_numericType == typeof(Int32))
                    return GetValue<Int32>();
                if (m_numericType == typeof(UInt32))
                    return GetValue<UInt32>();
                if (m_numericType == typeof(Int16))
                    return GetValue<Int16>();
                if (m_numericType == typeof(UInt16))
                    return GetValue<UInt16>();
                if (m_numericType == typeof(SByte))
                    return GetValue<SByte>();
                if (m_numericType == typeof(Byte))
                    return GetValue<Byte>();
                if (m_numericType == typeof(Single))
                    return GetValue<Single>();
                if (m_numericType == typeof(Double))
                    return GetValue<Double>();

                // can't get here
                return null;
            }
            set
            {
                Array array = value as Array;
                if (array == null)
                    throw new ArgumentException("value must be array");
                if (array.Length != Controls.Count)
                    throw new ArgumentException("array has the wrong dimension");

                m_lastChange = (Array)array.Clone();
                if (!m_editing)
                    m_lastEdit = m_lastChange;

                for (int i = 0; i < array.Length; i++)
                {
                    NumericTextBox textBox = Controls[i] as NumericTextBox;
                    object obj = array.GetValue(i);
                    if (obj.GetType() != m_numericType)
                        throw new ArgumentException("Matrix element type not supported");
                    textBox.Value = obj;
                }
            }
        }

        /// <summary>
        /// Gets or sets (protected) the last changed value</summary>
        public Array LastChange
        {
            get { return m_lastChange; }
            protected set { m_lastChange = value; }
        }

        /// <summary>
        /// Gets or sets (protected) the last edited value</summary>
        public Array LastEdit
        {
            get { return m_lastEdit; }
            protected set { m_lastEdit = value; }
        }

        /// <summary>
        /// Gets whether or not the user is currently editing matrix values</summary>
        public bool IsEditing
        {
            get { return m_editing; }
        }

        /// <summary>
        /// Event that is raised after value is changed</summary>
        public event EventHandler ValueChanged;

        /// <summary>
        /// Event that is raised after the value is edited</summary>
        public event EventHandler ValueEdited;

        /// <summary>
        /// Gets the numeric type of the matrix elements</summary>
        protected Type NumericType
        {
            get { return m_numericType; }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Resize"></see> event</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data</param>
        protected override void OnResize(EventArgs e)
        {
            int desiredHeight = (Controls[0].Height + 1) * m_rows;
            if (Height != desiredHeight)
                Height = desiredHeight;

            SuspendLayout();

            int width = Width - Margin.Left - Margin.Right;
            m_coordinateWidth = width / m_columns;

            int y = Margin.Top;
            for (int i = 0; i < m_rows; i++)
            {
                NumericTextBox textBox = null;
                int x = Margin.Left;
                for (int j = 0; j < m_columns; j++)
                {
                    textBox = Controls[i * m_columns + j] as NumericTextBox;
                    textBox.Bounds = new Rectangle(x + 1, y, m_coordinateWidth - 2, textBox.Height);
                    x += m_coordinateWidth + 1;
                }

                y += textBox.Height + 1;
            }

            ResumeLayout(true);

            base.OnResize(e);
        }

        

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint"></see> event</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"></see> that contains the event data</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int y = Margin.Top;
            NumericTextBox textBox = Controls[0] as NumericTextBox; ;
            for (int i = 0; i < m_rows - 1; i++)
            {
                y += textBox.Height;
                e.Graphics.DrawLine(SystemPens.Control, 0, y, Width, y);
                ++y;
            }

            int x = Margin.Left;
            
            for (int j = 0; j < m_columns; ++j)
            {
                x += (m_coordinateWidth);
                e.Graphics.DrawLine(SystemPens.Control, x, 0, x, Height);
                x += 1;

            }
        }


        /// <summary>
        /// Raises the ValueChanged event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnValueChanged(EventArgs e)
        {
            EventHandler handler = ValueChanged;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Raises the ValueEdited event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnValueEdited(EventArgs e)
        {
            EventHandler handler = ValueEdited;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Gets the component that changed</summary>
        /// <remarks>Only valid during OnValueChanged or OnValueEdited methods</remarks>
        protected int Component
        {
            get { return m_component; }
        }

        private void textBox_ValueEdited(object sender, EventArgs e)
        {
            NumericTextBox textbox = (NumericTextBox)sender;
            m_component = Controls.IndexOf(textbox);
            Array value = Value as Array;
            m_editing = true;

            OnValueChanged(EventArgs.Empty);

            m_lastChange = value;
            if (!ContainsFocus && m_editing)
            {
                OnValueEdited(EventArgs.Empty);
                m_lastEdit = value;
                m_editing = false;
            }
        }

        private T[] GetValue<T>()
        {
            T[] result = new T[Controls.Count];
            for (int i = 0; i < result.Length; i++)
                result[i] = (T)((NumericTextBox)Controls[i]).Value;
            return result;
        }

        private bool AreEqual(Array array1, Array array2)
        {
            if (m_numericType == typeof(Single))
                return MathUtil.AreApproxEqual((Single[])array1, (Single[])array2, 0.000001);

            if (m_numericType == typeof(Double))
                return MathUtil.AreApproxEqual((Double[])array1, (Double[])array2, 0.000001);

            for (int i = 0; i < array1.Length; i++)
                if (!array1.GetValue(i).Equals(array2.GetValue(i)))
                    return false;

            return true;
        }

        private Type m_numericType;
        private int m_rows;
        private int m_columns;
        private double m_scaleFactor = 1.0;
        private Array m_lastChange;
        private Array m_lastEdit;
        private int m_component;
        private int m_coordinateWidth;
        private bool m_editing;
    }
}
