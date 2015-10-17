//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Base class for controllers</summary>
    public abstract class Controller
    {
        /// <summary>
        /// Value pi as float</summary>
        public const float PI = (float)Math.PI;

        /// <summary>
        /// Value two pi as float</summary>
        public const float TwoPI = 2.0f * PI;

        /// <summary>
        /// Handles key-down events</summary>
        /// <param name="sender">Control that raised original event</param>
        /// <param name="e">Event args</param>
        /// <returns><c>True</c> if controller handled the event</returns>
        public virtual bool KeyDown(object sender,KeyEventArgs e)
        {
            return false;
        }

        /// <summary>
        /// Handles key-up events</summary>
        /// <param name="sender">Control that raised original event</param>
        /// <param name="e">Event args</param>
        /// <returns><c>True</c> if controller handled the event</returns>
        public virtual bool KeyUp(object sender, KeyEventArgs e)
        {
            return false;
        }

        /// <summary>
        /// Handles mouse wheel events</summary>
        /// <param name="sender">Control that raised original event</param>
        /// <param name="e">Event args</param>
        /// <returns><c>True</c> if controller handled the event</returns>
        public virtual bool MouseWheel(object sender, MouseEventArgs e)
        {
            return false;
        }

        /// <summary>
        /// Handles mouse-down events</summary>
        /// <param name="sender">Control that raised original event</param>
        /// <param name="e">Event args</param>
        /// <returns><c>True</c> if controller handled the event</returns>
        public virtual bool MouseDown(object sender, MouseEventArgs e)
        {
            return false;
        }

        /// <summary>
        /// Handles mouse-move events</summary>
        /// <param name="sender">Control that raised original event</param>
        /// <param name="e">Event args</param>
        /// <returns><c>True</c> if controller handled the event</returns>
        public virtual bool MouseMove(object sender, MouseEventArgs e)
        {
            return false;
        }

        /// <summary>
        /// Handles mouse-up events</summary>
        /// <param name="sender">Control that raised original event</param>
        /// <param name="e">Event args</param>
        /// <returns><c>True</c> if controller handled the event</returns>
        public virtual bool MouseUp(object sender, MouseEventArgs e)
        {
            return false;
        }
    }
}
