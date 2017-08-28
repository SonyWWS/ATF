using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override List<AutomationPeer> GetChildrenCore()
        {
            // Variables
            var automationPeers = new List<AutomationPeer>();

            // Owner
            var owner = Owner as DockPanel;
            if (owner == null)
            {
                return automationPeers;
            }

            // Children
            var children = owner.FindVisualChildren<DockedWindow>();
            if (children == null)
            {
                return automationPeers;
            }

            var dockedWindows = children as IList<DockedWindow> ?? children.ToList();

            automationPeers.AddRange(dockedWindows.Where(child => child != null).Select(CreatePeerForElement).Where(peer => peer != null));

            return automationPeers;
        }
        #endregion
    }
}
