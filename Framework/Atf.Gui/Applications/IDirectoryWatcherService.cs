//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.IO;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for a service that watches for changes to files within a directory</summary>
    public interface IDirectoryWatcherService
    {
        /// <summary>
        /// Registers a directory to be watched</summary>
        /// <param name="directory">Path of directory to watch</param>
        /// <param name="extensions">List of extensions to watch</param>
        /// <param name="includeSubdirectories">True to watch sub directories, too</param>
        void Register(string directory, IEnumerable<string> extensions, bool includeSubdirectories);

        /// <summary>
        /// Unregister a directory, so it is no longer watched</summary>
        /// <param name="directory">Path of directory to no longer watch</param>
        void Unregister(string directory);

        /// <summary>
        /// Event that is raised when a file is changed</summary>
        event FileSystemEventHandler FileChanged;
    }
    
    /// <summary>
    /// Service that watches for changes to files within a directory</summary>
    public static class DirectoryWatcherServices
    {
        /// <summary>
        /// Register a directory to be watched</summary>
        /// <param name="service">IDirectoryWatcherService to use to watch</param>
        /// <param name="directory">Path of directory to watch</param>
        public static void Register(this IDirectoryWatcherService service, string directory)
        {
            service.Register(directory, new string[] { "*.*" }, false);
        }

        /// <summary>
        /// Registers a directory to be watched</summary>
        /// <param name="service">IDirectoryWatcherService to use to watch</param>
        /// <param name="directory">Path of directory to watch</param>
        /// <param name="includeSubdirectories">True to watch sub directories, too</param>
        public static void Register(this IDirectoryWatcherService service, string directory, bool includeSubdirectories)
        {
            service.Register(directory, new string[] { "*.*" }, includeSubdirectories);
        }
    }
}
