//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Interface for ITarget management</summary>
    public interface ITargetService
    {
        /// <summary>
        /// Gets the list of Protocols</summary>
        IEnumerable<IProtocol> Protocols { get; }

        /// <summary>
        /// Gets the list of Targets</summary>
        IEnumerable<ITarget> Targets { get; }

        /// <summary>
        /// Gets the list of selected targets</summary>
        IEnumerable<ITarget> SelectedTargets { get; }

        /// <summary>
        /// Gets and sets the currently selected target</summary>
        ITarget SelectedTarget { get; set; }

        /// <summary>
        /// Shows the dialog for editing Target information</summary>
        /// <returns>Result returned by the dialog</returns>
        bool? ShowTargetDialog();
    }
}
