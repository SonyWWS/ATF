//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for control host services</summary>
    /// <remarks>The control host service is responsible for exposing client Controls in the
    /// application's main form.  It allows the client to specify a preferred location for the
    /// form, and calls the client back when the Control gets or loses focus, or is closed by
    /// the user. Implementers should also consider implementing IControlRegistry.</remarks>
    public interface IControlHostService
    {
        /// <summary>
        /// Registers the control and adds it to a visible form</summary>
        /// <param name="control">Control</param>
        /// <param name="controlInfo">Control display information</param>
        /// <param name="client">Client that owns the control and receives notifications
        /// about its status, or null if no notifications are needed</param>
        /// <remarks>If IControlHostClient.Close() has been called, the IControlHostService
        /// also calls UnregisterControl. Call RegisterControl again to re-register the Control.</remarks>
        void RegisterControl(Control control, ControlInfo controlInfo, IControlHostClient client);

        /// <summary>
        /// Unregisters the control and removes it from its containing form</summary>
        /// <param name="control">Control to be unregistered</param>
        /// <remarks>This method is called by IControlHostService after IControlHostClient.Close() is called.</remarks>
        void UnregisterControl(Control control);

        /// <summary>
        /// Makes a registered control visible</summary>
        /// <param name="control">Control to be made visible</param>
        void Show(Control control);

        /// <summary>
        /// Gets the open controls, in order of least-recently-active to the active control</summary>
        /// <remarks>IControlRegistry has additional functionality related to the active control.</remarks>
        IEnumerable<ControlInfo> Controls
        {
            get;
        }
    }

    /// <summary>
    /// Useful extension methods for IControlHostServices</summary>
    public static class ControlHostServices
    {
        /// <summary>
        /// Registers a control with the control host service</summary>
        /// <param name="controlHostService">Control host service</param>
        /// <param name="control">Control</param>
        /// <param name="name">Control name</param>
        /// <param name="description">Control description</param>
        /// <param name="group">Initial location of control on main form</param>
        /// <param name="client">Client that owns control, or null</param>
        /// <returns>ControlInfo for registered control</returns>
        public static ControlInfo RegisterControl(
            this IControlHostService controlHostService,
            Control control,
            string name,
            string description,
            StandardControlGroup group,
            IControlHostClient client)
        {
            var info = new ControlInfo(name, description, group);
            controlHostService.RegisterControl(control, info, client);
            return info;
        }

        /// <summary>
        /// Registers a control with the control host service</summary>
        /// <param name="controlHostService">Control host service</param>
        /// <param name="control">Control</param>
        /// <param name="name">Control name</param>
        /// <param name="description">Control description</param>
        /// <param name="group">Initial location of control on main form</param>
        /// <param name="image">Control icon</param>
        /// <param name="client">Client that owns control, or null</param>
        /// <param name="helpUrl">URL to open when the user presses F1 and this Control has focus.
        /// If null or the empty string, then this feature is disabled. It is better to use this
        /// parameter than to use the WebHelp object directly, so that the container in the docking
        /// framework will have a WebHelp object attached.</param>
        /// <returns>ControlInfo for registered control</returns>
        public static ControlInfo RegisterControl(
            this IControlHostService controlHostService,
            Control control,
            string name,
            string description,
            StandardControlGroup group,
            Image image,
            IControlHostClient client,
            string helpUrl = null)
        {
            var info = new ControlInfo(name, description, group, image, helpUrl);
            controlHostService.RegisterControl(control, info, client);
            return info;
        }
    }
}
