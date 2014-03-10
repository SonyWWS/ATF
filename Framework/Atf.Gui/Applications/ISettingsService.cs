//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for service that provides settings persistence and user editing</summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Registers persistent application settings</summary>
        /// <param name="uid">Unique identifer for settings</param>
        /// <param name="properties">Property descriptors to get/set values</param>
        /// <remarks>If a property's value is an XML document, the header will be removed when saving
        /// and won't be restored when loading.</remarks>
        void RegisterSettings(string uid, params PropertyDescriptor[] properties);

        /// <summary>
        /// Registers settings that can be presented to the user for editing. These are not
        /// persisted unless RegisterSettings is also called.</summary>
        /// <param name="pathName">Path to settings in tree control</param>
        /// <param name="properties">Property descriptors to get/set values</param>
        /// <remarks>If a property's value is an XML document, the header will be removed when saving
        /// and won't be restored when loading.</remarks>
        void RegisterUserSettings(string pathName, params PropertyDescriptor[] properties);

        /// <summary>
        /// Presents the settings dialog to the user, with the settings tree control opened
        /// to the given path</summary>
        /// <param name="pathName">Path of settings to display initially, or null to display
        /// the first leaf node</param>
        void PresentUserSettings(string pathName);

        /// <summary>
        /// Event that is raised before settings are saved</summary>
        event EventHandler Saving;

        /// <summary>
        /// Event that is raised before the settings are loaded (or reloaded)</summary>
        event EventHandler Loading;

        /// <summary>
        /// Event that is raised when the settings have been loaded or reloaded.</summary>
        event EventHandler Reloaded;
    }

    /// <summary>
    /// Static/extension methods for ISettingsService implementations</summary>
    public static class SettingsServices
    {
        /// <summary>
        /// Registers persistent application settings</summary>
        /// <param name="settingsService">Settings service</param>
        /// <param name="owner">Setting owner</param>
        /// <param name="properties">Property descriptors to get/set value</param>
        public static void RegisterSettings(this ISettingsService settingsService, object owner, params PropertyDescriptor[] properties)
        {
            settingsService.RegisterSettings(owner.GetType().FullName, properties);
        }

        /// <summary>
        /// Registers settings that can be presented to the user for editing. These are not
        /// persisted unless RegisterSettings is also called.</summary>
        /// <param name="settingsService">Settings service</param>
        /// <param name="pathName">Path to settings in tree control</param>
        /// <param name="properties">Property descriptors to get/set value</param>
        public static void RegisterUserSettings(this ISettingsService settingsService, string pathName, params PropertyDescriptor[] properties)
        {
            settingsService.RegisterUserSettings(pathName, properties);
        }
    }
}
