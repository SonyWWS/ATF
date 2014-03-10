//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf
{
    /// <summary>
    /// Interface for documents</summary>
    public interface IDocument : IResource
    {
        /// <summary>
        /// Gets whether the document is read-only</summary>
        bool IsReadOnly
        {
            get;
        }

        /// <summary>
        /// Gets or sets whether the document is dirty (does it differ from its file)</summary>
        bool Dirty
        {
            get;
            set;
        }

        /// <summary>
        /// Event that is raised when the Dirty property changes</summary>
        event EventHandler DirtyChanged;
    }
}
