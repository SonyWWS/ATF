//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Specifies the bounds of the control to use when defining a control's size and position</summary>
    [Flags]
    public enum BoundsSpecified
    {
        /// <summary>
        /// No bounds are specified</summary>
        None = 0,
        /// <summary>
        /// The left edge of the control is defined</summary>
        X = 1,
        /// <summary>
        /// The top edge of the control is defined</summary>
        Y = 2,
        /// <summary>
        /// Both x and y coordinates of the control are defined</summary>
        Location = 3,
        /// <summary>
        /// The width of the control is defined</summary>
        Width = 4,
        /// <summary>
        /// The height of the control is defined</summary>
        Height = 8,
        /// <summary>
        /// Both System.Windows.Forms.Control.Width and System.Windows.Forms.Control.Height property values
        /// of the control are defined</summary>
        Size = 12,
        /// <summary>
        /// Both System.Windows.Forms.Control.Location and System.Windows.Forms.Control.Size property values 
        /// are defined</summary>
        All = 15,
    }
}
