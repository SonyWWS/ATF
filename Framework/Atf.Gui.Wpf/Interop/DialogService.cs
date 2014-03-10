//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Interop
{
    /// <summary>
    /// Service to abstract display of parented WinForms dialog</summary>
    [Export(typeof(IDialogService))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DialogService : IDialogService, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="mainWindow">Main window</param>
        [ImportingConstructor]
        public DialogService(Window mainWindow)
        {
            m_mainWindow = mainWindow;
        }

        private Window m_mainWindow;

        #region IDialogService Members

        /// <summary>
        /// Shows WinForms dialog parented to main application window</summary>
        /// <param name="form">Form to show</param>
        /// <returns>Dialog result</returns>
        public DialogResult ShowParentedDialog(Form form)
        {
            var helper = new WindowInteropHelper(m_mainWindow);
            return form.ShowDialog(new WindowWrapper(helper.Handle));
        }

        #endregion

        #region IInitializable Members

        public void Initialize()
        {
        }

        #endregion

        private class WindowWrapper : System.Windows.Forms.IWin32Window
        {
            public WindowWrapper(IntPtr handle)
            {
                Handle = handle;
            }

            public IntPtr Handle { get; private set; }
        }
    }
}
