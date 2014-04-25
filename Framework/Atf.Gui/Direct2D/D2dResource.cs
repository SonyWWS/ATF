//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Direct2D
{
    /// <summary>
    /// Base class for all Direct2D resources</summary>
    public abstract class D2dResource : IDisposable
    {        
        /// <summary>        
        /// Gets whether the object has been disposed of</summary>
        public bool IsDisposed
        {
            get { return m_disposed; }
        }
        private bool m_disposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or 
        /// resetting unmanaged resources</summary>
        public void Dispose()
        {
            if (m_disposed) return;
            Dispose(true);
            GC.SuppressFinalize(this);            
        }

        /// <summary>
        /// Sets dispose flag</summary>
        /// <param name="disposing">Value to set dispose flag to</param>
        protected virtual void Dispose(bool disposing)
        {            
            m_disposed = true;
        }

        /// <summary>
        /// Destructor</summary>
        ~D2dResource()
        {
            Dispose(false);
        }        
    }
}
