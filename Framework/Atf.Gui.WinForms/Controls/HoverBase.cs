//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Base class for forms while the mouse hovers over an item</summary>
    public class HoverBase : System.Windows.Forms.Form
    {
        /// <summary>
        /// Constructor</summary>
        public HoverBase()
        {
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;

            BackColor = System.Drawing.SystemColors.Info;
            ForeColor = System.Drawing.SystemColors.InfoText;

            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        }

        /// <summary>
        /// Makes the popup visible, but without taking the focus</summary>
        public void ShowWithoutFocus()
        {
            User32.ShowWindow(Handle, User32.SW_SHOWNOACTIVATE);
        }

        /// <summary>
        /// Performs custom actions when the <see cref="E:System.Windows.Forms.Control.Click"></see> event occurs</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data</param>
        protected override void OnClick(EventArgs e)
        {
            base.Dispose();
            base.Close();
        }

        /// <summary>
        /// Performs custom actions when the <see cref="E:System.Windows.Forms.Control.MouseLeave"></see> event occurs</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.Dispose();
            base.Close();
        }
    }
}
