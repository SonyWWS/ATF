//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.IO;

namespace Sce.Atf
{
    /// <summary>
    /// Interface for a service that watches for changes to files</summary>
    public interface IFileWatcherService
    {
        /// <summary>
        /// Registers a file to be watched</summary>
        /// <param name="filePath">Path of file to watch</param>
        void Register(string filePath);

        /// <summary>
        /// Unregisters a file, so it is no longer watched</summary>
        /// <param name="filePath">Path of file to no longer watch</param>
        void Unregister(string filePath);

        /// <summary>
        /// Event that is raised when a file is changed</summary>
        event FileSystemEventHandler FileChanged;
    }
}
