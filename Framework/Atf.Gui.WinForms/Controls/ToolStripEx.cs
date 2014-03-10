using System;
using System.Windows.Forms;

namespace Sce.Atf.Applications.Controls
{
    /// <summary>
    /// This class enables "click through" to System.Windows.Forms.ToolStrip.
    /// See http://blogs.msdn.com/b/rickbrew/archive/2006/01/09/511003.aspx .</summary>
    public class ToolStripEx : ToolStrip
    {

        /// <summary>
        /// Gets or sets whether the ToolStripEx honors item clicks when its containing form does
        /// not have input focus</summary>
        /// <remarks>
        /// Default value is true, which enables "click through"</remarks>
        public bool ClickThrough
        {
            get { return m_clickThrough; }
            set { m_clickThrough = value;}
        }

        /// <summary>
        /// WndProc</summary>
        /// <param name="m">Message</param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (ClickThrough &&
                m.Msg == NativeConstants.WM_MOUSEACTIVATE &&
                m.Result == (IntPtr)NativeConstants.MA_ACTIVATEANDEAT)
            {
                m.Result = (IntPtr)NativeConstants.MA_ACTIVATE;
            }
        }

        private bool m_clickThrough = true;

    }

    internal sealed class NativeConstants
    {
        private NativeConstants()
        {
        }

        internal const uint WM_MOUSEACTIVATE = 0x21;
        internal const uint MA_ACTIVATE = 1;
        internal const uint MA_ACTIVATEANDEAT = 2;
        internal const uint MA_NOACTIVATE = 3;
        internal const uint MA_NOACTIVATEANDEAT = 4;
    }
}
