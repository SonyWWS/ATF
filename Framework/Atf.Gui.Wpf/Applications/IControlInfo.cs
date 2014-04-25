//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Interface for Control information</summary>
    /// <remarks>An IControlInfo instance is returned by ControlHostService.RegisterControl()</remarks>
    public interface IControlInfo
    {
        /// <summary>
        /// Gets or sets the control's name, which may be displayed as the title of
        /// a hosting control or form</summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the control's description, which may be displayed as a tooltip
        /// when the user hovers over a hosting control or form</summary>
        string Description { get; set; }

        /// <summary>
        /// Gets or sets the control's image, which may be displayed on a hosting control
        /// or form</summary>
        object ImageSourceKey { get; set; }

        /// <summary>
        /// Gets the unique ID for the control</summary>
        string Id { get; }

        /// <summary>
        /// Gets the control's desired initial control group</summary>
        Sce.Atf.Applications.StandardControlGroup Group { get; }

        /// <summary>
        /// Gets the client that registered this control</summary>
        IControlHostClient Client { get; }

        /// <summary>
        /// Gets the control that was registered with this info</summary>
        object Content { get; }
    }
}
