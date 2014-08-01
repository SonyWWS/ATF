//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Xml;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Base class for providing information about a thumbnail resource</summary>
    public class ThumbnailParameters
    {
        /// <summary>
        /// URI for the thumbnail image</summary>
        public Uri Source;
    }

    /// <summary>
    /// Service that manages the transformation of Resources into thumbnail images and file paths</summary>
    [Export(typeof(ThumbnailService))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ThumbnailService : IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        public ThumbnailService()
        {
        }

        #region IInitializable Members

        /// <summary>
        /// Initializes the service</summary>
        public void Initialize()
        {
            // Note: ThreadIdle isn't exactly what we're after: if there is no user interaction
            // then the callback never gets called so thumbnails aren't generated.
            //ComponentDispatcher.ThreadIdle += DispatcherThreadIdle;

            // This seems to perform more efficiently.
            m_timer = new DispatcherTimer
                    (
                    TimeSpan.FromSeconds(1),
                    DispatcherPriority.ApplicationIdle,
                    DispatcherThreadIdle,
                    Application.Current.Dispatcher
                    );
        }

        #endregion

        /// <summary>
        /// Gets or sets the default XML resolver, which is used when no resolver is provided</summary>
        public static XmlResolver DefaultXmlResolver
        {
            get { return s_defaultXmlResolver; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                s_defaultXmlResolver = value;
            }
        }

        /// <summary>
        /// Gets the resource path for the given resource, using the DefaultXmlResolver</summary>
        /// <param name="resourceUri">Resource URI</param>
        /// <returns>Local path to resource file</returns>
        public static string GetResourcePath(Uri resourceUri)
        {
            string path = null;

            string localPath = resourceUri.LocalPath;
            Uri uri = DefaultXmlResolver.ResolveUri(null, localPath);
            if (uri != null)
                path = uri.LocalPath;

            return path;
        }

        /// <summary>
        /// Event that is raised when a thumbnail is ready</summary>
        public event EventHandler<ThumbnailReadyEventArgs> ThumbnailReady;

        /// <summary>
        /// Resolves the Resource into a path to a thumbnail image file using a background thread</summary>
        /// <param name="resourceUri">URI of the resource to resolve</param>
        public void ResolveThumbnail(ThumbnailParameters resourceUri)
        {
            // Push the resource onto the resolve queue
            lock (m_resourcesToResolve)
            {
                m_resourcesToResolve.Enqueue(resourceUri);
            }

            if (!IsThreadAlive())
            {
                StartThread();
            }
        }

        /// <summary>
        /// Resolves the Resource into a path to a thumbnail image file</summary>
        /// <param name="resourceUri">URI of the resource to resolve</param>
        /// <returns>Path to thumbnail image file</returns>
        public object ResolveThumbnailBlocking(ThumbnailParameters resourceUri)        {
            object thumbnailImage = null;
            
            foreach (IThumbnailResolver resolver in m_resolvers)
            {
                thumbnailImage = resolver.Resolve(resourceUri);
                if (thumbnailImage != null)
                {
                    break;
                }
            }

            return thumbnailImage;
        }

        /// <summary>
        /// Raises the ThumbnailReady event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnThumbnailReady(ThumbnailReadyEventArgs e)
        {
            EventHandler<ThumbnailReadyEventArgs> handler = ThumbnailReady;
            if (handler != null)
                handler(this, e);
        }

        private void StartThread()
        {
            m_workThread = new Thread(ResolverThread);
            m_workThread.Name = "thumbnail service";
            m_workThread.IsBackground = true; //so that the thread can be killed if app dies.
            m_workThread.SetApartmentState(ApartmentState.STA);
            m_workThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
            m_workThread.Start();
        }

        private bool IsThreadAlive()
        {
            return
                m_workThread != null &&
                m_workThread.IsAlive;
        }

        private void DispatcherThreadIdle(object sender, EventArgs e)
        {
            if (!IsThreadAlive())
            {
                while (m_resolvedResources.Count > 0)
                {
                    ResolvedThumbnail resolved = m_resolvedResources.Dequeue();
                    OnThumbnailReady(new ThumbnailReadyEventArgs(resolved.ResourceUri, resolved.Image));
                }

                // Check if there are more resources to resolve and the thread is inactive
                if (m_resourcesToResolve.Count > 0)
                {
                    StartThread();
                }
            }
            else
            {
                lock (m_resolvedResources)
                {
                    while (m_resolvedResources.Count > 0)
                    {
                        ResolvedThumbnail resolved = m_resolvedResources.Dequeue();
                        OnThumbnailReady(new ThumbnailReadyEventArgs(resolved.ResourceUri, resolved.Image));
                    }
                }
            }
        }

        private void ResolverThread()
        {
            ThumbnailParameters thumbnailParameters = null;
            do
            {
                lock (m_resourcesToResolve)
                {
                    if (m_resourcesToResolve.Count > 0)
                    {
                        thumbnailParameters = m_resourcesToResolve.Dequeue();
                    }
                    else
                    {
                        thumbnailParameters = null;
                    }
                }

                if (thumbnailParameters != null)
                {
                    lock (m_resolvers)
                    {
                        object thumbnailImage = null;
                        foreach (var resolver in m_resolvers)
                        {
                            try
                            {
                                thumbnailImage = resolver.Resolve(thumbnailParameters);
                                if (thumbnailImage != null)
                                {
                                    // Add it to the resolved queue
                                    lock (m_resolvedResources)
                                    {
                                        m_resolvedResources.Enqueue(new ResolvedThumbnail(thumbnailParameters.Source, thumbnailImage));
                                    }
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                Outputs.WriteLine(OutputMessageType.Warning, ex.Message);
                            }
                        }

                        // Signal that no resolver managed to generate the image
                        if (thumbnailImage == null)
                        {
                            lock (m_resolvedResources)
                            {
                                m_resolvedResources.Enqueue(new ResolvedThumbnail(thumbnailParameters.Source, thumbnailImage));
                            }
                        }
                    }
                }
            }
            while (thumbnailParameters != null);
        }

        /// <summary>
        /// Class to hold an resource/path pair</summary>
        private class ResolvedThumbnail
        {
            public ResolvedThumbnail(Uri resourceUri, object image)
            {
                ResourceUri = resourceUri;
                Image = image;
            }

            public readonly Uri ResourceUri;
            public readonly object Image;
        }

        /// <summary>
        /// MEF import of available thumbnail resolvers</summary>
        [ImportMany]
        private IEnumerable<IThumbnailResolver> m_resolvers = null;

        private Thread m_workThread;
        private DispatcherTimer m_timer;
        private readonly Queue<ResolvedThumbnail> m_resolvedResources = new Queue<ResolvedThumbnail>();
        private readonly Queue<ThumbnailParameters> m_resourcesToResolve = new Queue<ThumbnailParameters>();

        private static XmlResolver s_defaultXmlResolver = new FindFileResolver();
    }
}
