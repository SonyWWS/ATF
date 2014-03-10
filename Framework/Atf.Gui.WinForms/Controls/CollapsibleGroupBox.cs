//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Collapsible GroupBox control</summary>
    public class CollapsibleGroupBox : GroupBox
    {
        /// <summary>
        /// Constructor</summary>
        public CollapsibleGroupBox()
        {
            m_btn = new Button { Location = new Point(0, 0), Size = new Size(ButtonWidth, ButtonHeight) };
            m_btn.Paint += BtnPaint;
            m_btn.Click += BtnClick;

            m_btnFont = new Font(m_btn.Font.FontFamily, 8.0f);
            
            m_btnFormat = new StringFormat { Alignment = StringAlignment.Center };

            Controls.Add(m_btn);
        }

        /// <summary>
        /// Gets or sets the height</summary>
        public new int Height
        {
            get { return IsCollapsed ? CollapseHeight : base.Height; }
            set
            {
                if (IsCollapsed)
                    LastHeight = value;
                else
                    base.Height = value;
            }
        }

        /// <summary>
        /// Gets or sets the text</summary>
        public override string Text
        {
            get { return base.Text; }
            set
            {
                // In DesignMode don't add the extra padding. It messes up things
                // when the app is run live (ie. it will have extra padding).
                base.Text = DesignMode ? value : value.Insert(0, PaddingString);
            }
        }

        /// <summary>
        /// Gets the height of the GroupBox before it was collapsed</summary>
        public int LastHeight { get; private set; }

        /// <summary>
        /// Gets whether the GroupBox is collapsed</summary>
        public bool IsCollapsed { get; private set; }

        /// <summary>
        /// Collapses the GroupBox</summary>
        public void Collapse()
        {
            if (IsCollapsed)
                return;

            Collapsing.Raise(this, EventArgs.Empty);

            foreach (Control child in Controls)
            {
                if (ReferenceEquals(child, m_btn))
                    continue;

                child.Hide();
            }

            m_prevMinSize = new Size(MinimumSize.Width, MinimumSize.Height);
            MinimumSize = new Size(MinimumSize.Width, CollapseHeight);

            LastHeight = base.Height;
            base.Height = CollapseHeight;

            IsCollapsed = true;

            Invalidate();
            m_btn.Invalidate();

            Collapsed.Raise(this, EventArgs.Empty);
        }

        /// <summary>
        /// Expands the GroupBox</summary>
        public void Expand()
        {
            if (!IsCollapsed)
                return;

            Expanding.Raise(this, EventArgs.Empty);

            foreach (Control child in Controls)
            {
                if (ReferenceEquals(child, m_btn))
                    continue;

                child.Show();
            }

            MinimumSize = new Size(m_prevMinSize.Width, m_prevMinSize.Height);

            base.Height = LastHeight;
            LastHeight = CollapseHeight;

            IsCollapsed = false;

            Invalidate();
            m_btn.Invalidate();

            Expanded.Raise(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event fired when the GroupBox is collapsing</summary>
        public event EventHandler Collapsing;

        /// <summary>
        /// Event fired when the GroupBox is collapsed</summary>
        public event EventHandler Collapsed;

        /// <summary>
        /// Event fired when the GroupBox is expanding</summary>
        public event EventHandler Expanding;

        /// <summary>
        /// Event fired when the GroupBox is expanded</summary>
        public event EventHandler Expanded;

        /// <summary>
        /// Disposes of resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_btn != null)
                {
                    m_btn.Dispose();
                    m_btn = null;
                }

                if (m_btnFont != null)
                {
                    m_btnFont.Dispose();
                    m_btnFont = null;
                }

                if (m_btnFormat != null)
                {
                    m_btnFormat.Dispose();
                    m_btnFormat = null;
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Performs custom actions on paint event</summary>
        /// <param name="e">Paint event args</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            var rectangle = ClientRectangle;

            GroupBoxRenderer.DrawGroupBox(
                e.Graphics,
                rectangle,
                Text,
                Font,
                ForeColor,
                TextFormatFlags.Default,
                Enabled ? GroupBoxState.Normal : GroupBoxState.Disabled);
        }

        private void BtnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawString(
                IsCollapsed ? "+" : "-",
                m_btnFont,
                SystemBrushes.ControlText,
                new RectangleF(0.0f, 0.0f, m_btn.Width, m_btn.Height),
                m_btnFormat);
        }

        private void BtnClick(object sender, EventArgs e)
        {
            if (IsCollapsed)
                Expand();
            else
                Collapse();
        }

        private Button m_btn;
        private Font m_btnFont;
        private Size m_prevMinSize;
        private StringFormat m_btnFormat;

        private const int ButtonWidth = 16;
        private const int ButtonHeight = 16;
        private const int CollapseHeight = 16;
        private const string PaddingString = "   ";
    }
}