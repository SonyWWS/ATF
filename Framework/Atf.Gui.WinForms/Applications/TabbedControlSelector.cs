//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf.Controls;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Component that enables the user to switch control focus using the Ctrl+Tab keyboard command,
    /// similar to Visual Studio, Windows, or any tabbed Internet browser application</summary>
    [Export(typeof(TabbedControlSelector))]
    [Export(typeof(IInitializable))]
    public class TabbedControlSelector : IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="mainForm">Main form</param>
        /// <param name="controlHostService">Control host service</param>
        [ImportingConstructor]
        public TabbedControlSelector(
            Form mainForm,
            IControlHostService controlHostService)
        {
            m_mainForm = mainForm;
            m_controlHostService = controlHostService;
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            m_mainForm.KeyDown += OnKeyDown;
            m_mainForm.KeyPreview = true;
        }

        #endregion

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Tab && e.Control)
            {
                TabbedControlSelectorDialog dialog = new TabbedControlSelectorDialog(m_controlHostService, !e.Shift);
                dialog.ShowDialog(m_mainForm);
            }
        }

        private readonly Form m_mainForm;
        private readonly IControlHostService m_controlHostService;
    }
}
