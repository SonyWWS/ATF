using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace Sce.Atf.Wpf.Docking.Automation
{
    public class DockedWindowAutomationPeer : FrameworkElementAutomationPeer
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public DockedWindowAutomationPeer(FrameworkElement owner) : base(owner)
        {
        }

        #region Override Members
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string GetClassNameCore()
        {
            return nameof(DockedWindow);
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
            var owner = Owner as DockedWindow;
            if (owner == null)
            {
                return automationPeers;
            }

            // ContentPanel
            var contentControl = owner.FindVisualChild<ContentControl>();
            if (contentControl == null)
            {
                return automationPeers;
            }

            // Content
            var content = contentControl?.Content as UIElement;
            if (content == null)
            {
                return automationPeers;
            }

            // Peers
            var peer = CreatePeerForElement(content);
            if (peer == null)
            {
                return automationPeers;
            }

            automationPeers.Add(peer);

            return automationPeers;
        }
        #endregion
    }

}
