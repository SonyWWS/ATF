//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// A version of the <see cref="System.Windows.Forms.TrackBar"/> that doesn't show the selection rectangle</summary>
    public class NoFocusTrackBar : TrackBar
    {
        /// <summary>
        /// Overridden OnGotFocus that removes the selection rectangle</summary>
        /// <param name="e">Event arguments</param>
        /// <remarks>See http://stackoverflow.com/questions/1484270/hiding-dashed-outline-around-trackbar-control-when-selected. </remarks>
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            User32.SendMessage(Handle, User32.WM_UPDATEUISTATE, MakeParam(1, 0x1), 0);
        }

        private static int MakeParam(int loWord, int hiWord)
        {
            return (hiWord << 16) | (loWord & 0xffff);
        }
    }
}