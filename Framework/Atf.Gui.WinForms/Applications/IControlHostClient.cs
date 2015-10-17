//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for control host clients</summary>
    /// <remarks>This interface should be implemented by any class that registers controls
    /// with an IControlHostService. Only IControlHostService should call these methods.</remarks>
    public interface IControlHostClient
    {
        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the Control gets focus, or a parent "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        void Activate(Control control);

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another Control or "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        void Deactivate(Control control);

        /// <summary>
        /// Requests permission to close the client's Control</summary>
        /// <param name="control">Client Control to be closed</param>
        /// <returns><c>True</c> if the Control can close, or false to cancel</returns>
        /// <remarks>
        /// 1. This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.
        /// 2. If true is returned, the IControlHostService calls its own
        /// UnregisterControl. The IControlHostClient has to call RegisterControl again
        /// if it wants to re-register this Control.
        /// 3. This method is NOT called when the user toggles the visibility using the Windows
        /// menu commands. To know if your Control is actually visible or not requires a bit
        /// of a hack, as the VisibleChanged event is only raised when this Control is made
        /// visible but not when it is hidden. This is a .NET bug. http://tracker.ship.scea.com/jira/browse/WWSATF-1335 </remarks>
        bool Close(Control control);
    }
}
