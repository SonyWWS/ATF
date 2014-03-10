//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface to client-defined UI for entering data for, and triggering, searches</summary>
    public interface ISearchableContextUI
    {
        /// <summary>
        /// Event raised by client when UI has graphically changed</summary>
        event EventHandler UIChanged;

        /// <summary>
        /// Gets actual client-defined GUI Control (TODO: make this not WinForms-specific)</summary>
        Control Control { get; }
    }
}