//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Adapts a DOM node to implement IResource</summary>
    public class DomResource : DomNodeAdapter, IResource
    {
        #region IResource Members

        /// <summary>
        /// Gets a string identifying the type of the resource to the end-user</summary>
        public virtual string Type
        {
            get { return "Unknown".Localize(); }
        }

        /// <summary>
        /// Gets or sets the resource URI</summary>
        public virtual Uri Uri
        {
            get { return m_uri; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (value != m_uri)
                {
                    Uri oldUri = m_uri;
                    m_uri = value;
                    OnUriChanged(new UriChangedEventArgs(oldUri));
                }
            }
        }

        /// <summary>
        /// Event that is raised after the resource's URI changes</summary>
        public event EventHandler<UriChangedEventArgs> UriChanged;

        /// <summary>
        /// Raises the UriChanged event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnUriChanged(UriChangedEventArgs e)
        {
            UriChanged.Raise(this, e);
        }

        #endregion

        private Uri m_uri;
    }
}
