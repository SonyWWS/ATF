using System.Windows;
using System.Windows.Automation.Peers;

namespace Sce.Atf.Wpf.Docking.Automation
{
    /// <summary>
    /// Exposes <see cref="DockPanel"/> types to UI Automation.
    /// </summary>
    public class DockPanelAutomationPeer : FrameworkElementAutomationPeer
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public DockPanelAutomationPeer(FrameworkElement owner) : base(owner)
        {
        }

        #region Override Members
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string GetClassNameCore()
        {
            return nameof(DockPanel);
        }
        #endregion
    }
}
