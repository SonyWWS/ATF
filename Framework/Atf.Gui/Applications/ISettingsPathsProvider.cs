//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface to access where settings data should be persisted</summary>
    public interface ISettingsPathsProvider
    {
        /// <summary>
        /// Gets the file path where settings data should be persisted</summary>
        /// <remarks>For example, in the SettingsService implementation, this might be
        /// [Environment.SpecialFolder.ApplicationData]\[Application.ProductName]\[Version]\AppSettings.xml</remarks>
        string SettingsPath { get; }

        /// <summary>
        /// Gets the file path where default settings data can be found. Default data is used
        /// when no persisted settings data can be found.</summary>
        /// <remarks>For example, in the SettingsService implementationm, this might be
        /// [Application.StartupPath]\DefaultSettings.xml</remarks>
        string DefaultSettingsPath { get; }
    }
}