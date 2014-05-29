//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Input
{
    /// <summary>
    /// Key press event arguments</summary>
    public class KeyPressEventArgs : EventArgs
    {
        /// <summary>
        /// Character for key pressed</summary>
        public char KeyChar;
        /// <summary>
        /// Whether event handled or not</summary>
        public bool Handled;
    }
}
