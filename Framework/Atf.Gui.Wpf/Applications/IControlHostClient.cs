//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Interface for control host clients</summary>
    /// <remarks>This interface should be implemented by any class that registers controls
    /// with an IControlHostService. Only IControlHostService should call these methods.</remarks>
    public interface IControlHostClient
    {
        /// <summary>
        /// Notifies the client that its control has been activated. Activation occurs when
        /// the control gets focus, or a parent "host" control gets focus.</summary>
        /// <param name="control">Client control that was activated</param>
        /// <remarks>This method is only called by IControlHostService if the control was previously
        /// registered for this IControlHostClient.</remarks>
        void Activate(object control);

        /// <summary>
        /// Notifies the client that its control has been deactivated. Deactivation occurs when
        /// another control or "host" control gets focus.</summary>
        /// <param name="control">Client control that was deactivated</param>
        /// <remarks>This method is only called by IControlHostService if the control was previously
        /// registered for this IControlHostClient.</remarks>
        void Deactivate(object control);

        /// <summary>
        /// Requests permission to close the client's control</summary>
        /// <param name="control">Client control to be closed</param>
        /// <param name="mainWindowClosing">True if the application main window is closing</param>
        /// <returns>True if the control can close, or false to cancel.</returns>
        /// <remarks>
        /// 1. This method is only called by IControlHostService if the control was previously
        /// registered for this IControlHostClient.
        /// 2. If true is returned, the IControlHostService will call its own
        /// UnregisterContent. The IControlHostClient has to call RegisterControl again
        /// if it wants to re-register this control.</remarks>
        bool Close(object control, bool mainWindowClosing);
    }
}
