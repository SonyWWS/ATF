//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Wpf.Controls.Adaptable;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Interface for contexts where items can be viewed</summary>
    public interface IViewingContext : Sce.Atf.Applications.IViewingContext
    {
        /// <summary>
        /// Gets or sets the viewing context's adaptable item</summary>
        IAdaptable Adaptable { get; set; }

        /// <summary>
        /// Gets or sets the viewing context's canvas adapter</summary>
        IViewingAdapter ViewingAdapter { get; set; }
        
        /// <summary>
        /// Gets or sets an enumeration of the viewing context's picking adapters</summary>
        IEnumerable<IPickingAdapter> PickingAdapters { get; set; }

        /// <summary>
        /// Gets or sets an enumeration of the viewing context's layout constraints</summary>
        IEnumerable<ILayoutConstraint> LayoutConstraints { get; set; }
    }
}
