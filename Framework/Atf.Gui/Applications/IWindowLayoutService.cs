//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Common interface for WPF and WinForms WindowLayoutService class</summary>
    /// <remarks>Allows customized window layouts for ATF applications</remarks>
    public interface IWindowLayoutService
    {
        /// <summary>
        /// Gets or sets the current layout. If setting a new layout name, a new layout is created</summary>
        string CurrentLayout { get; set; }

        /// <summary>
        /// Gets all of the layout names</summary>
        IEnumerable<string> Layouts { get; }

        /// <summary>
        /// Rename a layout</summary>
        /// <param name="oldLayoutName">Old layout name</param>
        /// <param name="newLayoutName">New layout name</param>
        /// <returns>True if layout renamed, or false if layout not removed or doesn't exist or new name is invalid</returns>
        bool RenameLayout(string oldLayoutName, string newLayoutName);

        /// <summary>
        /// Remove (i.e. delete) a layout</summary>
        /// <param name="layoutName">Layout name</param>
        /// <returns>True if layout removed, or false if layout not removed or doesn't exist</returns>
        bool RemoveLayout(string layoutName);

        /// <summary>
        /// Event that is raised before either CurrentLayout or Layouts changes</summary>
        event EventHandler<EventArgs> LayoutsChanging;

        /// <summary>
        /// Event that is raised after either CurrentLayout or Layouts changes</summary>
        event EventHandler<EventArgs> LayoutsChanged;
    }

    /// <summary>
    /// Interface for clients to implement so that they can supply custom data
    /// when the WindowLayoutService saves and loads layouts</summary>
    public interface IWindowLayoutClient
    {
        /// <summary>
        /// Gets or sets the layout data</summary>
        /// <remarks>
        /// Even though the property get and set value is an object, the ATF WindowLayoutService
        /// is expecting an XML string, something with formatting similar to:
        /// &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; standalone=&quot;yes&quot;?&gt;
        /// &lt;YourCustomElementsHere/&gt;
        /// Follow the standard XmlDocument() use in other persisted settings that return XML
        /// trimmed strings back to SettingsService.
        /// </remarks>
        object LayoutData { get; set; }
    }

    /// <summary>
    /// IWindowLayoutService extension methods</summary>
    public static class WindowLayoutServiceExtension
    {
        /// <summary>
        /// Check if the layout name is the current layout</summary>
        /// <param name="windowLayoutService">Window layout service</param>
        /// <param name="layoutName">Layout name to check</param>
        /// <returns>True iff layout name to check is current</returns>
        public static bool IsCurrent(this IWindowLayoutService windowLayoutService, string layoutName)
        {
            if (windowLayoutService == null)
                return false;

            if (string.IsNullOrEmpty(layoutName))
                return false;

            return string.Compare(windowLayoutService.CurrentLayout, layoutName) == 0;
        }

        /// <summary>
        /// Programatically add a new layout or update an existing layout. The current layout
        /// becomes this given layout.</summary>
        /// <param name="windowLayoutService">Window layout service</param>
        /// <param name="dockStateProvider">Dock state provider</param>
        /// <param name="layoutName">Layout name</param>
        /// <param name="dockState">Dock state</param>
        public static void SetOrAddLayout(this IWindowLayoutService windowLayoutService, IDockStateProvider dockStateProvider, string layoutName, object dockState)
        {
            // Update current window arrangement and everything
            dockStateProvider.DockState = dockState;

            // Save or update window arrangement as the layout name
            windowLayoutService.CurrentLayout = layoutName;
        }

        /// <summary>
        /// Strictly adds a layout</summary>
        /// <param name="windowLayoutService">Window layout service</param>
        /// <param name="newLayoutName">New layout name</param>
        /// <param name="dockState">Dock state</param>
        /// <remarks>
        /// Used to add default layouts.
        /// Requires the implementor of IWindowLayoutService to implement:
        /// void AddLayout(string newLayoutName, object dockState)</remarks>
        public static void AddLayout(this IWindowLayoutService windowLayoutService, string newLayoutName, object dockState)
        {
            MethodInfo method = windowLayoutService.GetType().GetMethod("AddLayout");
            var args = new object[] { newLayoutName, dockState };
            method.Invoke(windowLayoutService, args);
        }
    }
}