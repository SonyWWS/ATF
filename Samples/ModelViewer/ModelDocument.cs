//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf;
using Sce.Atf.Dom;

namespace ModelViewerSample
{
    /// <summary>
    /// Implements a read only IDocument that holds the root node  of loaded 3D model</summary>    
    public class ModelDocument : IDocument
    {
        /// <summary>
        /// Construct from root node and URI</summary>        
        public ModelDocument(DomNode node, Uri ur)
        {
            RootNode = node;
            m_uri = ur;
        }

        /// <summary>
        /// Gets root node of this document</summary>
        public DomNode RootNode
        {
            get;
            private set;
        }

        #region IDocument Members

        /// <summary>
        /// Gets whether the document is read only</summary>
        public bool IsReadOnly
        {
            get { return true; }
        }


        /// <summary>
        /// Gets or sets whether the document is dirty (does it differ from its file)</summary>
        public bool Dirty
        {
            get { return false; }
            set { throw new InvalidOperationException(); }
        }
        /// <summary>
        /// Event that is raised when the Dirty property changes</summary>
        public event EventHandler DirtyChanged = delegate { };

        #endregion

        #region IResource Members

        /// <summary>
        /// Gets a string identifying the type of the resource to the end-user</summary>
        public string Type
        {
            get { return "3D Model"; }
        }

        /// <summary>
        /// Gets or sets the resource URI</summary>
        public Uri Uri
        {
            get { return m_uri; }
            set { throw new InvalidOperationException(); }
        }


        /// <summary>
        /// Event that is raised after the resource's URI changes</summary>
        public event EventHandler<UriChangedEventArgs> UriChanged = delegate { };

        #endregion

        private Uri m_uri;        
    }
}
