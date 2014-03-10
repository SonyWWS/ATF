//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Interface for a layout constraint, which modifies bounding rectangles</summary>
    public interface ILayoutConstraint
    {
        /// <summary>
        /// Gets the displayable name of the constraint</summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// Gets or sets whether the constraint is enabled</summary>
        bool Enabled
        {
            get;
            set;
        }

        /// <summary>
        /// Applies constraint to bounding rectangle</summary>
        /// <param name="bounds">Unconstrained bounding rectangle</param>
        /// <param name="specified">Flags indicating which parts of bounding rectangle are meaningful</param>
        /// <returns>Constrained bounding rectangle</returns>
        Rect Constrain(Rect bounds, BoundsSpecified specified);
    }
}
