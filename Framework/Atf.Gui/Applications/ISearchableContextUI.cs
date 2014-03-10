//Sony Computer Entertainment Confidential

using System;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
	/// <summary>
    /// Interface to client-defined UI for entering search data and triggering searches</summary>
	public interface ISearchableContextUI
	{
		/// <summary>
		/// Event to be fired by client when UI has graphically changed</summary>
		event EventHandler UIChanged;

		/// <summary>
		/// Gets actual client-defined GUI control (TODO: make this not WinForms-specific)</summary>
		Control Control { get; }
	}
}