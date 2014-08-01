//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Interop;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Service to allow Control+Tabbing between content</summary>
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SwitchToService : IInitializable
    {
        #region IInitializable Members

        public void Initialize()
        {
            if (m_mainWindow != null)
            {
                m_mainWindow.Loaded += MainWindowLoaded;
            }
        }

        #endregion

        private void MainWindowLoaded(object sender, EventArgs e)
        {
            if (m_mainWindow != null)
                m_mainWindow.MainWindow.PreviewKeyDown += MainWindowOnPreviewKeyDown;
        }

        private void MainWindowOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // TODO: Disable this if menus are open?
            if (e.Handled)
                return;

            if ((Keyboard.Modifiers == ModifierKeys.Control) && (e.Key == Key.Tab))
            {
                if (!SwitchToDialog.IsInUse)
                {
                    new SwitchToDialog(m_controlHostService).ShowParentedDialog();
                }
                else
                {
                    SwitchToDialog.FocusCurrentInstance();
                }

                e.Handled = true;
            }
        }

        [Import(AllowDefault = true)]
        private MainWindowAdapter m_mainWindow = null;
        [Import]
        private IControlHostService m_controlHostService = null;
    }
}
