//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Abstract base class for diagram renderers, which render and hit-test diagrams</summary>
    public abstract class DiagramRenderer : IDisposable
    {
        /// <summary>
        /// Gets or sets (protected) a value indicating if the renderer is printing</summary>
        public bool IsPrinting
        {
            get { return m_isPrinting; }
            protected set { m_isPrinting = value; }
        }
        private bool m_isPrinting;

        /// <summary>
        /// Event that is raised after any user property of the style changes</summary>
        public event EventHandler Redraw;

        /// <summary>
        /// Raises the Redraw event</summary>
        protected virtual void OnRedraw()
        {
            Redraw.Raise(this, EventArgs.Empty);
        }

        /// <summary>
        /// Function to compute rendering style for an item</summary>
        public Func<object, DiagramDrawingStyle> GetStyle;
  

        #region IDisposable Members

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
        ~DiagramRenderer()
        {
            Dispose(false);
        }
        private bool m_disposed;                  
        #endregion
    }
}
