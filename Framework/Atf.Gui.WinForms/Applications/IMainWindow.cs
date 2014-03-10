//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface that abstracts the idea of a main window for the application. This allows
    /// components to work with WinForms, WPF, and other UI toolkits.</summary>
    public interface IMainWindow
    {
        /// <summary>
        /// Gets or sets the main window text</summary>
        string Text
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a Win32 handle for displaying WinForms dialogs with an owner</summary>
        IWin32Window DialogOwner
        {
            get;
        }

        /// <summary>
        /// Closes the application's main window</summary>
        void Close();

        /// <summary>
        /// Event that is raised before the application is loaded</summary>
        event EventHandler Loading;

        /// <summary>
        /// Event that is raised after the application is loaded</summary>
        event EventHandler Loaded;

        /// <summary>
        /// Event that is raised before the main window closes. Subscribers can cancel
        /// the closing action.</summary>
        event CancelEventHandler Closing;

        /// <summary>
        /// Event that is raised after the main window is closed</summary>
        event EventHandler Closed;
    }
}


