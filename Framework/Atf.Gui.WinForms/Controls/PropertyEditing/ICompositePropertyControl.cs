//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Interface for editor controls that operate on composite properties. Such controls
    /// allow the user to navigate into a selected part of the property.</summary>
    public interface ICompositePropertyControl
    {
        /// <summary>
        /// Event that is raised when part of the composite is opened by the user</summary>
        event EventHandler<CompositeOpenedEventArgs> CompositeOpened;
    }
}
