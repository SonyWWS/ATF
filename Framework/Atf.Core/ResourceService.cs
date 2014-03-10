//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Sce.Atf.Adaptation;

namespace Sce.Atf
{
    /// <summary>
    /// Default IResourceService implementation. Contains currently loaded resources and can load
    /// and unload resources. Imports all available IResourceResolver MEF components.</summary>
    [Export(typeof(ResourceService))]
    [Export(typeof(IInitializable))]
    [Export(typeof(IResourceService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ResourceService : IInitializable, IResourceService
    {
        #region IInitializable Members

        /// <summary>
        /// Initializes the service</summary>
        public void Initialize()
        {
            // Empty implementation, but required by ATF's MEF architecture.
        }

        #endregion
        
        #region IResourceService

        /// <summary>
        /// Attempts to load the resource specified by the given URI using all of the current
        /// IResourceResolvers until one succeeds or they all fail. If the resource has already
        /// been loaded, it is returned.</summary>
        /// <param name="uri">URI specifying resource</param>
        /// <returns>The resolved resource or null if all registered resolvers have failed</returns>
        public IResource Load(Uri uri)
        {
            IResource resource;
            if (!m_resourceMap.TryGetValue(uri, out resource))
            {
                foreach (IResourceResolver resolver in m_resolvers)
                {
                    resource = resolver.Resolve(uri);
                    if (resource != null)
                    {
                        m_resourceMap[uri] = resource;
                        OnResourceLoaded(resource);
                        break;
                    }
                }
            }

            return resource;
        }

        /// <summary>
        /// Gets the resource if already loaded, but doesn't attempt to load it.
        /// Returns null if the resource has not been loaded.</summary>
        /// <param name="uri">URI specifying resource</param>
        /// <returns>Resource if loaded, null otherwise</returns>
        public IResource GetResource(Uri uri)
        {
            IResource resource = null;
            m_resourceMap.TryGetValue(uri, out resource); // bool return value ignored
            return resource;
        }

        /// <summary>
        /// Unloads the IResource associated with the specified URI</summary>
        /// <param name="uri">URI specifying resource</param>
        /// <returns>True iff the resource was found and unloaded</returns>
        public bool Unload(Uri uri)
        {
            IResource resource;
            if (m_resourceMap.TryGetValue(uri, out resource))
            {
                Unload(resource);
                m_resourceMap.Remove(uri);
                return true;
            }
            return false;
        }

        private void Unload(IResource resource)
        {
            IDisposable disposable = resource.As<IDisposable>();
            if (disposable != null)
                disposable.Dispose();
        }

        /// <summary>
        /// Gets the enumeration of all loaded resources</summary>
        public IEnumerable<IResource> Resources
        {
            get { return m_resourceMap.Values; }
        }

        /// <summary>
        /// Raised when a resouce has been loaded</summary>
        public event EventHandler<ItemChangedEventArgs<IResource>> ResourceLoaded;

        /// <summary>
        /// Raised when a resource has been unloaded</summary>
        public event EventHandler<ItemChangedEventArgs<IResource>> ResourceUnloaded;

        /// <summary>
        /// Called when a resource has been loaded. Raises the ResourceLoaded event.</summary>
        /// <param name="resource">Resource</param>
        protected virtual void OnResourceLoaded(IResource resource)
        {
            if (ResourceLoaded != null)
                ResourceLoaded(this, new ItemChangedEventArgs<IResource>(resource));
        }

        /// <summary>
        /// Called when a resource has been unloaded. Raises the ResourceUnloaded event.</summary>
        /// <param name="resource">Resource</param>
        protected virtual void OnResourceUnloaded(IResource resource)
        {
            if (ResourceUnloaded != null)
                ResourceUnloaded(this, new ItemChangedEventArgs<IResource>(resource));
        }

        #endregion

        /// <summary>
        /// Main data structure for managing URI->IResource mappings</summary>
        private readonly Dictionary<Uri, IResource> m_resourceMap = new Dictionary<Uri, IResource>();

        /// <summary>
        /// MEF import of all available resource resolvers</summary>
        [ImportMany(typeof(IResourceResolver))]
        private IEnumerable<IResourceResolver> m_resolvers;
    }
}
