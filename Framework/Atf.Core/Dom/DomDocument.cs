//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Adapts a DOM node to implement IDocument</summary>
    public class DomDocument : DomResource, IDocument
    {
        #region IDocument Members

        /// <summary>
        /// Gets whether the document is read-only</summary>
        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Gets or sets whether the document is dirty (does it differ from its file)</summary>
        public virtual bool Dirty
        {
            get
            {
                return m_dirty;
            }
            set
            {
                if (value != m_dirty)
                {
                    m_dirty = value;

                    OnDirtyChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Event that is raised when the Dirty property changes</summary>
        public event EventHandler DirtyChanged;

        #endregion

        /// <summary>
        /// Raises the DirtyChanged event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnDirtyChanged(EventArgs e)
        {
            DirtyChanged.Raise(this, e);
        }

        /// <summary>
        /// Raises the IObservableContext.Reloaded event</summary>
        /// <param name="args">Event args</param>
        protected virtual void OnReloaded(EventArgs args)
        {
        }

        private bool m_dirty;
    }
}
