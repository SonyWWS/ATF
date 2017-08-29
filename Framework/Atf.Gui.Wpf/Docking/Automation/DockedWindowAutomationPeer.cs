using System.Windows;
using System.Windows.Automation.Peers;

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
        #endregion
    }

}
