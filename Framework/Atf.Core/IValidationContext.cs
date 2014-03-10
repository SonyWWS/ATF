//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf
{
    /// <summary>
    /// Interface for contexts, which provide events so listeners can perform validation.
    /// Validation events allow listeners to update themselves and check constraints
    /// more efficiently.</summary>
    public interface IValidationContext
    {
        /// <summary>
        /// Event that is raised before validation begins</summary>
        event EventHandler Beginning;

        /// <summary>
        /// Event that is raised after validation is cancelled</summary>
        event EventHandler Cancelled;

        /// <summary>
        /// Event that is raised before validation ends</summary>
        event EventHandler Ending;

        /// <summary>
        /// Event that is raised after validation ends</summary>
        event EventHandler Ended;
    }
}
