//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Interface for control host services in WPF</summary>
    /// <remarks>The control host service is responsible for exposing client Controls in the
    /// application's main form.  It allows the client to specify a preferred location for the
    /// form, and calls the client back when the Control gets or loses focus, or is closed by
    /// the user. Implementers should also consider implementing IControlRegistry.</remarks>
    public interface IControlHostService
    {
        /// <summary>
        /// Registers a control with the control host service</summary>
        /// <param name="definition">Control definition</param>
        /// <param name="control">Control</param>
        /// <param name="client">Client that owns the control and receives notifications
        /// about its status, or null if no notifications are needed</param>
        /// <returns>IControlInfo for registered control</returns>
        IControlInfo RegisterControl(ControlDef definition, object control, IControlHostClient client);

        /// <summary>
        /// Unregisters content in control</summary>
        /// <param name="control">Control</param>
        void UnregisterContent(object control);

        /// <summary>
        /// Makes a registered control visible</summary>
        /// <param name="control">Control to be made visible</param>
        void Show(object control);

        /// <summary>
        /// Gets the sequence of all registered controls and associated hosting information</summary>
        IEnumerable<IControlInfo> Contents { get; }

        /// <summary>
        /// Gets or sets the dock panel state</summary>
        string DockPanelState { get; set; }
    }

    /// <summary>
    /// Useful static/extension methods for IControlHostServices</summary>
    public static class ControlHostServices
    {
        /// <summary>
        /// Registers a control with the control host service</summary>
        /// <param name="controlHostService">Control host service</param>
        /// <param name="control">Control</param>
        /// <param name="name">Control name</param>
        /// <param name="description">Control description</param>
        /// <param name="group">Initial location of control on main form</param>
        /// <param name="id">Unique ID for control</param>
        /// <param name="client">Client that owns control, or null</param>
        /// <returns>IControlInfo for registered control</returns>
        public static IControlInfo RegisterControl(
            this IControlHostService controlHostService,
            object control,
            string name,
            string description,
            Sce.Atf.Applications.StandardControlGroup group,
            string id,
            IControlHostClient client)
        {
            var def = new ControlDef() { Name = name, Description = description, Group = group, Id = id };
            return controlHostService.RegisterControl(def, control, client);
        }

        /// <summary>
        /// Registers a control with the control host service</summary>
        /// <param name="controlHostService">Control host service</param>
        /// <param name="control">Control</param>
        /// <param name="name">Control name</param>
        /// <param name="description">Control description</param>
        /// <param name="group">Initial location of control on main form</param>
        /// <param name="imageSourceKey">Control icon resource</param>
        /// <param name="id">Unique ID for control</param>
        /// <param name="client">Client that owns control, or null</param>
        /// <returns>IControlInfo for registered control</returns>
        public static IControlInfo RegisterControl(
            this IControlHostService controlHostService,
            object control,
            string name,
            string description,
            Sce.Atf.Applications.StandardControlGroup group,
            object imageSourceKey,
            string id,
            IControlHostClient client)
        {
            var def = new ControlDef() { Name = name, Description = description, Group = group, Id = id, ImageSourceKey = imageSourceKey };
            return controlHostService.RegisterControl(def, control, client);
        }

        /// <summary>
        /// Unregisters content by control info</summary>
        /// <param name="controlHostService">Control host service</param>
        /// <param name="info">Control info to unregister</param>
        public static void UnregisterContent(
            this IControlHostService controlHostService,
            IControlInfo info)
        {
            controlHostService.UnregisterContent(info.Content);
        }

        /// <summary>
        /// Shows a control by control info</summary>
        /// <param name="controlHostService">Control host service</param>
        /// <param name="info">Control info for control</param>
        public static void Show(
            this IControlHostService controlHostService,
            IControlInfo info)
        {
            controlHostService.Show(info.Content);
        }

    }
}
