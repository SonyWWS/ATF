//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Sce.Atf.Controls
{

    /// <summary>
    /// Control for editing a bounded int value</summary>
    public class FloatInputControl : UserControl
    {
        /// <summary>
        /// Constructor</summary>
        public FloatInputControl()
            : this(0, 0, 100)
        {
        }

        /// <summary>
        /// Constructor with initial and bounding values</summary>
        /// <param name="value">Initial value</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        public FloatInputControl(float value, float min, float max)
        {
            if (min >= max)
                throw new ArgumentException("min must be less than max");
            DoubleBuffered = true;
            m_min = min;
            m_max = max;
            m_value = MathUtil.Clamp(value, m_min, m_max);
            m_lastChange = m_value;
            m_lastEdit = m_value;
            
            m_textBox = new NumericTextBox();
            m_textBox.BorderStyle = BorderStyle.None;
            m_textBox.Name = "m_textBox";

            m_spinner = new CompactSpinner();
            m_spinner.BackColor = m_textBox.BackColor;
            
            SuspendLayout();            
            UpdateTextBox();            
            Controls.Add(m_textBox);            
            Controls.Add(m_spinner);            
            ResumeLayout(false);
            PerformLayout();

            m_textBox.ValueEdited += (sender, e) =>
            {
                float val = (float)m_textBox.Value;
                SetValue(val, false);
                EndEdit(true);
            };

            m_spinner.Changed += (sender, e) =>
            {
                // might be better to expose delta as property
                float delta = (m_max - m_min) / 100.0f;
                float newValue = Value + (float)e.Value * delta;
                SetValue(newValue, false);
            };

            m_textBox.SizeChanged += (sender, e) => Height = m_textBox.Height + 3;
            SizeChanged += (sender, e) =>
            {
                m_spinner.Bounds = new Rectangle(0, 0, Height, Height);
                m_textBox.Bounds = new Rectangle(m_spinner.Width, 0, Width - m_spinner.Width, m_textBox.Height);
            };
        }

        /// <summary>
        /// Gets and sets the current value of the control</summary>
        public float Value
        {
            get { return m_value; }
            set
            {
                SetValue(value, false);
            }
        }

        /// <summary>
        /// Gets the last changed value</summary>
        public float LastChange
        {
            get { return m_lastChange; }
        }

        /// <summary>
        /// Gets the last edited value</summary>
        public float LastEdit
        {
            get { return m_lastEdit; }
        }

        /// <summary>
        /// Gets or sets the minimum value</summary>
        public float Min
        {
            get { return m_min; }
            set
            {
                if (value >= m_max)
                    throw new ArgumentException("Min");
                m_min = value;
                if (m_value < m_min)
                    Value = m_min;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the maximum value</summary>
        public float Max
        {
            get { return m_max; }
            set
            {
                if(value <= m_min)
                    throw new ArgumentException("Max");
                m_max = value;
                if (m_value > m_max)
                    Value = m_max;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets and sets whether to draw a border around the control</summary>        
        public bool DrawBorder
        {
            get { return m_drawBorder; }
            set { m_drawBorder = value; }
        }

        /// <summary>
        /// Event that is raised after value is changed</summary>
        public event EventHandler ValueChanged;

        /// <summary>
        /// Event that is raised after the value is edited</summary>
        public event EventHandler ValueEdited;

        /// <summary>
        /// Translates from normalized trackbar position to value</summary>
        /// <param name="position">The normalized trackbar position, in [0..1]</param>
        /// <returns>The value (should be between Min and Max)</returns>
        /// <remarks>Override this method to change the mapping between position and
        /// value, for example, for a logarithmic slider.</remarks>
        protected virtual float GetValue(float position)
        {            
            return Constrain(m_min + position * (m_max - m_min));
        }

        /// <summary>
        /// Translates from value to normalized position</summary>
        /// <param name="value">The value, in [Min..Max]</param>
        /// <returns>The normalized trackbar position (should be in [0..1])</returns>
        /// <remarks>Override this method to change the mapping between position and
        /// value, for example, for a logarithmic slider. This should be the inverse
        /// of GetValue().</remarks>
        protected virtual float GetPosition(float value)
        {
            value = Constrain(value);
            return (value - m_min) / (m_max - m_min);
        }

       
        /// <summary>
        /// Raises the <see cref="E:Sce.Atf.Controls.IntInputControl.ValueChanged"/> event</summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data</param>
        protected virtual void OnValueChanged(EventArgs e)
        {
            ValueChanged.Raise(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:Sce.Atf.Controls.IntInputControl.ValueEdited"/> event</summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data</param>
        protected virtual void OnValueEdited(EventArgs e)
        {
            ValueEdited.Raise(this, e);
        }


        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.BackColorChanged"></see> event</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data</param>
        protected override void OnBackColorChanged(EventArgs e)
        {
            m_textBox.BackColor = BackColor;
            m_spinner.BackColor = BackColor;

            base.OnBackColorChanged(e);
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            float r = (float)(m_value - m_min) / (float)(m_max - m_min);
            int w = (int)(r * m_textBox.Width);
            Rectangle rec
                = new Rectangle(m_textBox.Location.X, m_textBox.Height, w, 3);
            e.Graphics.FillRectangle(Brushes.LightBlue, rec);
            if (m_drawBorder)
                ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, Border3DStyle.Flat);
        }

        private void EndEdit(bool forceNewValue)
        {
            if (forceNewValue ||
                m_value != m_lastEdit)
            {
                OnValueEdited(EventArgs.Empty);
                m_lastEdit = m_value;
            }
        }

        private void SetValue(float value, bool forceNewValue)
        {
            value = Sce.Atf.MathUtil.Clamp(value, m_min, m_max);
            if (forceNewValue ||
                value != m_value)
            {
                m_value = value;
                OnValueChanged(EventArgs.Empty);
                m_lastChange = value;
            }

            // Update the user interface to make sure the displayed text is in sync.
            //  If these two methods are in the above 'if', then typing in the same
            //  out-of-range value twice in a row persists, indicating that m_textBox.Text
            //  is out of sync with m_value.
            UpdateTextBox();
            this.Invalidate();
        }



        private void UpdateTextBox()
        {
            m_textBox.Value = m_value;         
        }

        private float Constrain(float value)
        {
            return MathUtil.Clamp(value,m_min,m_max);            
        }


        private float m_value;
        private float m_lastChange;
        private float m_lastEdit;
        private float m_min;
        private float m_max;

        private bool m_drawBorder = true;

        private CompactSpinner m_spinner;
        private NumericTextBox m_textBox;

    }
}
