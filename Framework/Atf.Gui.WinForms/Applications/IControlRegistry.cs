//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for component that tracks active controls</summary>
    /// <remarks>Implementers should also consider implementing IControlHostService.</remarks>
    public interface IControlRegistry
    {
        /// <summary>
        /// Gets or sets the active control</summary>
        ControlInfo ActiveControl
        {
            get;
            set;
        }

        /// <summary>
        /// Event that is raised before the active control changes</summary>
        event EventHandler ActiveControlChanging;

        /// <summary>
        /// Event that is raised after the active control changes</summary>
        event EventHandler ActiveControlChanged;

        /// <summary>
        /// Gets the open controls' ControlInfos, in order of least-recently-active to the active control</summary>
        IEnumerable<ControlInfo> Controls
        {
            get;
        }

        /// <summary>
        /// Event that is raised after a control is added; it becomes the active control</summary>
        event EventHandler<ItemInsertedEventArgs<ControlInfo>> ControlAdded;

        /// <summary>
        /// Event that is raised after a control is removed</summary>
        event EventHandler<ItemRemovedEventArgs<ControlInfo>> ControlRemoved;
    }
}
