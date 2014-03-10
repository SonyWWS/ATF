//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// Event args for manipulator events</summary>
    public class ManipulatorEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="manipulator">Manipulator for event</param>
        public ManipulatorEventArgs(IManipulator manipulator) 
        {
            Manipulator = manipulator;
        }

        /// <summary>
        /// Manipulator for event</summary>
        public readonly IManipulator Manipulator;
    }
}
