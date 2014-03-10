//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Class that holds information about recently open documents</summary>
    public class RecentDocumentInfo : IComparable, IPinnable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="uri">Document URI</param>
        /// <param name="type">Document type</param>
        public RecentDocumentInfo(Uri uri, string type)
        {
            m_uri = uri;
            m_type = type;
            Pinned = false;
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="uri">Document URI</param>
        /// <param name="type">Document type</param>
        /// <param name="pinned">Whether the document is pinned to the MRU list</param>
        public RecentDocumentInfo(Uri uri, string type, bool pinned)
        {
            m_uri = uri;
            m_type = type;
            Pinned = pinned;
        }

        /// <summary>
        /// Gets the document URI</summary>
        public Uri Uri
        {
            get { return m_uri; }
        }

        /// <summary>
        /// Gets the document type, which is the user-readable string that represents this type
        /// of document.</summary>
        public string Type
        {
            get { return m_type; }
        }

        /// <summary>
        /// Gets and sets whether the user has pinned this document to the MRU list.</summary>
        public bool Pinned { get; set; }

        /// <summary>
        /// Gets or sets the recent file index, which determines order of display in the UI</summary>
        public int Index
        {
            get { return m_index; }
            set { m_index = value; }
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal</summary>
        /// <param name="obj">Another object to compare to</param>
        /// <returns>True iff obj and this instance are the same type and represent
        /// the same value</returns>
        public override bool Equals(object obj)
        {
            RecentDocumentInfo other = obj as RecentDocumentInfo;
            return
                other != null &&
                Uri == other.Uri &&
                m_type == other.m_type;
        }

        /// <summary>
        /// Returns the hash code for this instance</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance</returns>
        public override int GetHashCode()
        {
            return
                Uri.GetHashCode() ^
                m_type.GetHashCode();
        }

        #region IComparable Members

        /// <summary>
        /// Compare function for recent documents</summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>-1, 0 or 1 depending on compare</returns>
        int IComparable.CompareTo(object obj)
        {
            RecentDocumentInfo other = obj as RecentDocumentInfo;
            if (other == null)
                return 0;

            return
                m_index.CompareTo(other.m_index);
        }

        #endregion

        private readonly Uri m_uri;
        private readonly string m_type;
        private int m_index = -1;
    }
}
