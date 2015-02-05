//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf
{
    /// <summary>
    /// Contains currently loaded resources</summary>
    public interface IResourceService
    {
        /// <summary>
        /// Attempts to load the resource specified by the given URI using all of the current
        /// IResourceResolvers until one succeeds or they all fail. If the resource has already
        /// been loaded, it is returned.</summary>
        /// <param name="uri">Resource URI</param>
        /// <returns>The resolved resource or null if all registered resolvers have failed</returns>
        IResource Load(Uri uri);

        /// <summary>
        /// Gets the resource if already loaded, but doesn't attempt to load it.
        /// Returns null if the resource has not been loaded.</summary>
        /// <param name="uri">Resource URI</param>
        /// <returns>Resource if loaded, null otherwise</returns>
        IResource GetResource(Uri uri);

        /// <summary>
        /// Unloads the IResource associated with the specified URI</summary>
        /// <param name="uri">Resource URI</param>
        /// <returns>True iff the resource was found and unloaded</returns>
        bool Unload(Uri uri);


        /// <summary>
        /// Gets the enumeration of all loaded resources</summary>
        IEnumerable<IResource> Resources
        {
            get;
        }

        /// <summary>
        /// Event that is raised when a resource has been loaded</summary>
        event EventHandler<ItemChangedEventArgs<IResource>> ResourceLoaded;

        /// <summary>
        /// Event that is raised when a resource has been unloaded</summary>
        event EventHandler<ItemChangedEventArgs<IResource>> ResourceUnloaded;
    }

    /// <summary>
    /// Extension methods for IResourceService</summary>
    public static class ResourceServices
    {
        /// <summary>
        /// Returns the number of loaded resources</summary>
        /// <param name="resourceService">Currently loaded resources</param>
        /// <returns>Number of loaded resources</returns>
        public static int GetResourceCount(this IResourceService resourceService)
        {
            return System.Linq.Enumerable.Count(resourceService.Resources);
        }

        /// <summary>
        /// Returns an enumeration of loaded URIs</summary>
        /// <param name="resourceService">Currently loaded resources</param>
        /// <returns>Enumeration of loaded URIs</returns>
        public static IEnumerable<Uri> GetUris(this IResourceService resourceService)
        {
            foreach (IResource resource in resourceService.Resources)
                yield return resource.Uri;
        }
    }
}
