//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Control for editing a boolean value</summary>
    public class BoolInputControl : System.Windows.Forms.UserControl
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="value">Initial control value</param>
        /// <param name="trueText">Text associated with true value</param>
        /// <param name="falseText">Text associated with false value</param>
        public BoolInputControl(bool value, string trueText, string falseText)
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            m_trueText = trueText;
            m_falseText = falseText;
            m_textBox.Text = falseText;
            Value = value;

            m_trackBar.ValueChanged += control_ValueChanged;
        }

        /// <summary>
        /// Cleans up any resources being used</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; 
        /// false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Properties

        /// <summary>
        /// Gets or sets value of boolean control</summary>
        public bool Value
        {
            get { return m_value; }
            set
            {
                if (value != m_value)
                {
                    m_value = value;
                    if (value)
                    {
                        m_textBox.Text = m_trueText;
                        m_trackBar.Value = 1;
                    }
                    else
                    {
                        m_textBox.Text = m_falseText;
                        m_trackBar.Value = 0;
                    }
                    OnValueChanged();
                }
            }
        }

        /// <summary>
        /// Gets the track bar button offset</summary>
        public Point TrackBarButtonOffset
        {
            get
            {
                int y = m_trackBar.Location.Y + m_trackBar.Height / 2;
                int x = (int)(m_trackBar.Location.X + 10 +
                    (m_trackBar.Width - 20) * m_trackBar.Value / (float)(m_trackBar.Maximum - m_trackBar.Minimum));
                return new Point(x, y);
            }
        }

        /// <summary>
        /// Event that is raised when the control value changes</summary>
        public event EventHandler ValueChanged;

        #endregion

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint"></see> event.</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"></see> that contains the event data</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, Border3DStyle.Flat);
        }

        /// <summary>
        /// Method called when value changed</summary>
        protected virtual void OnValueChanged()
        {
            if (ValueChanged != null)
                ValueChanged(this, EventArgs.Empty);
        }

        #region Event Handlers

        private void control_ValueChanged(object sender, EventArgs e)
        {
            Value = (m_trackBar.Value == 1);
        }

        #endregion

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify the contents of this method
        /// with the code editor</summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BoolInputControl));
            this.m_trackBar = new System.Windows.Forms.TrackBar();
            this.m_textBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.m_trackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // _trackBar
            // 
            resources.ApplyResources(this.m_trackBar, "_trackBar");
            this.m_trackBar.BackColor = System.Drawing.SystemColors.Window;
            this.m_trackBar.Maximum = 1;
            this.m_trackBar.Name = "_trackBar";
            this.m_trackBar.TickFrequency = 0;
            // 
            // _textBox
            // 
            this.m_textBox.AcceptsReturn = true;
            this.m_textBox.AcceptsTab = true;
            this.m_textBox.BackColor = System.Drawing.SystemColors.Window;
            this.m_textBox.HideSelection = false;
            resources.ApplyResources(this.m_textBox, "_textBox");
            this.m_textBox.Name = "_textBox";
            this.m_textBox.ReadOnly = true;
            // 
            // BoolInputControl
            // 
            this.Controls.Add(this.m_textBox);
            this.Controls.Add(this.m_trackBar);
            this.Name = "BoolInputControl";
            resources.ApplyResources(this, "$this");
            ((System.ComponentModel.ISupportInitialize)(this.m_trackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private bool m_value;
        private readonly string m_trueText;
        private readonly string m_falseText;

        private System.Windows.Forms.TrackBar m_trackBar;
        private System.Windows.Forms.TextBox m_textBox;

        // Required designer variable.
        private readonly System.ComponentModel.Container components = null;
    }
}
