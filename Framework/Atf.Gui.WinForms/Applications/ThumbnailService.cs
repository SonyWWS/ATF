//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace Sce.Atf.Applications
{
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
            Application.Idle += Application_Idle;
        }

        #region IInitializable Members

        /// <summary>
        /// Initializes the service</summary>
        public virtual void Initialize()
        {
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
        /// Resolves the Resource into a path to a thumbnail image file</summary>
        /// <param name="resourceUri">URI of the resource to resolve</param>
        public void ResolveThumbnail(Uri resourceUri)
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
            m_workThread = new Thread(ResolverThread)
                               {
                                   Name = "thumbnail service",
                                   IsBackground = true,
                                   CurrentUICulture = Thread.CurrentThread.CurrentUICulture,
                                   Priority = ThreadPriority.Lowest
                               };
            m_workThread.Start();
        }

        private bool IsThreadAlive()
        {
            return
                m_workThread != null &&
                m_workThread.IsAlive;
        }

        private void Application_Idle(object sender, EventArgs e)
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
            Uri resourceUri = null;
            do
            {
                lock (m_resourcesToResolve)
                {
                    if (m_resourcesToResolve.Count > 0)
                    {
                        resourceUri = m_resourcesToResolve.Dequeue();
                    }
                    else
                    {
                        resourceUri = null;
                    }
                }

                if (resourceUri != null)
                {
                    lock (m_resolvers)
                    {
                        foreach (IThumbnailResolver resolver in m_resolvers)
                        {
                            try
                            {
                                Image thumbnailImage = resolver.Resolve(resourceUri);
                                if (thumbnailImage != null)
                                {
                                    // Add it to the resolved queue
                                    lock (m_resolvedResources)
                                    {
                                        m_resolvedResources.Enqueue(new ResolvedThumbnail(resourceUri, thumbnailImage));
                                    }
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                Outputs.WriteLine(OutputMessageType.Warning, ex.Message);
                            }
                        }
                    }
                }
            }
            while (resourceUri != null);
        }

        /// <summary>
        /// Class to hold an resource/path pair</summary>
        private class ResolvedThumbnail
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="resourceUri">Resource URI</param>
            /// <param name="image">Image to associate with URI</param>
            public ResolvedThumbnail(Uri resourceUri, Image image)
            {
                ResourceUri = resourceUri;
                Image = image;
            }

            public readonly Uri ResourceUri;
            public readonly Image Image;
        }

        /// <summary>
        /// MEF import of available thumbnail resolvers</summary>
        [ImportMany]
        private IEnumerable<IThumbnailResolver> m_resolvers;

        private Thread m_workThread;
        private readonly Queue<Uri> m_resourcesToResolve = new Queue<Uri>();
        private readonly Queue<ResolvedThumbnail> m_resolvedResources = new Queue<ResolvedThumbnail>();

        private static XmlResolver s_defaultXmlResolver = new FindFileResolver();
    }
}
