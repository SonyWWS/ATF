//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// View model class for the TcpIpTargetEditDialog</summary>
    public class TcpIpTargetEditDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="target">TCP/IP target being displayed</param>
        public TcpIpTargetEditDialogViewModel(TcpIpTarget target)
        {
            Title = "Edit TCP/IP Target".Localize();
            Target = target;
        }

        /// <summary>
        /// Gets the TcpIpTarget to display</summary>
        public TcpIpTarget Target { get; private set; }
    }
}
