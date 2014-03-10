//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Class to hold document editor information</summary>
    public class DocumentClientInfo
    {
        /// <summary>
        /// Constructor</summary>
        public DocumentClientInfo()
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="fileType">File type name</param>
        /// <param name="extension">File extension, without the '.'</param>
        /// <param name="newIconName">Name of 'New' icon, or null</param>
        /// <param name="openIconName">Name of 'Open' icon, or null</param>
        public DocumentClientInfo(
            string fileType,
            string extension,
            string newIconName,
            string openIconName)
            : this(fileType, new[] { extension }, newIconName, openIconName, true)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="fileType">File type name</param>
        /// <param name="extension">File extension, without the '.'</param>
        /// <param name="newIconName">Name of 'New' icon, or null</param>
        /// <param name="openIconName">Name of 'Open' icon, or null</param>
        /// <param name="multiDocument">Whether client opens multiple documents simultaneously</param>
        public DocumentClientInfo(
            string fileType,
            string extension,
            string newIconName,
            string openIconName,
            bool multiDocument)
            : this(fileType, new[] { extension }, newIconName, openIconName, multiDocument)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="fileType">File type name</param>
        /// <param name="extensions">File extensions, without the '.'</param>
        /// <param name="newIconName">Name of 'New' icon, or null</param>
        /// <param name="openIconName">Name of 'Open' icon, or null</param>
        /// <param name="multiDocument">Whether client opens multiple documents simultaneously</param>
        public DocumentClientInfo(
            string fileType,
            string[] extensions,
            string newIconName,
            string openIconName,
            bool multiDocument)
        {
            m_fileType = fileType;
            m_extensions = extensions;
            m_newIconName = newIconName;
            m_openIconName = openIconName;
            m_multiDocument = multiDocument;
        }

        /// <summary>
        /// Gets and sets the document client's user-readable file type name</summary>
        /// <remarks>The type name should be unique within an application and can contain
        /// any characters except ','</remarks>
        public string FileType
        {
            get { return m_fileType; }
            set { m_fileType = value; }
        }

        /// <summary>
        /// Gets and sets the file extensions that the editor can handle</summary>
        public string[] Extensions
        {
            get { return m_extensions; }
            set { m_extensions = value; }
        }

        /// <summary>
        /// Gets and sets the editor's 'New' icon name, or null</summary>
        public string NewIconName
        {
            get { return m_newIconName; }
            set { m_newIconName = value; }
        }

        /// <summary>
        /// Gets and sets the editor's 'Open' icon name, or null</summary>
        public string OpenIconName
        {
            get { return m_openIconName; }
            set { m_openIconName = value; }
        }

        /// <summary>
        /// Gets and sets the editor's default new document name</summary>
        public string NewDocumentName
        {
            get { return m_newDocumentName; }
            set { m_newDocumentName = value; }
        }

        /// <summary>
        /// Gets and sets a value indicating if the editor can open multiple documents simultaneously</summary>
        public bool MultiDocument
        {
            get { return m_multiDocument; }
            set { m_multiDocument = value; }
        }

        /// <summary>
        /// Gets and sets a string to be used as the initial directory for the first time this application runs
        /// on the user's computer</summary>
        public string InitialDirectory
        {
            get { return m_initialDirectory; }
            set { m_initialDirectory = value; }
        }

        /// <summary>
        /// Convenience method to check if file path is compatible with the extensions
        /// supported by the document client</summary>
        /// <param name="filePath">File path</param>
        /// <returns>True iff file path is compatible with the extensions supported by
        /// the document client</returns>
        public bool IsCompatiblePath(string filePath)
        {
            string uriExtension = Path.GetExtension(filePath);            
            foreach (string extension in Extensions)
                if (string.Compare(extension, uriExtension, true) == 0)
                    return true;

            return false;
        }

        /// <summary>
        /// Convenience method to check if file URI is compatible with the extensions
        /// supported by the document client</summary>
        /// <param name="uri">File URI</param>
        /// <returns>True iff file URI is compatible with the extensions supported by
        /// the document client</returns>
        public bool IsCompatibleUri(Uri uri)
        {
            string uriString = Uri.UnescapeDataString(uri.ToString());
            return IsCompatiblePath(uriString);
        }

        /// <summary>
        /// Gets the Windows filter string for this document type</summary>
        /// <returns>Windows filter string for this document type</returns>
        public string GetFilterString()
        {
            return FileFilterBuilder.GetFilterString(FileType, Extensions);
        }

        private string m_fileType;
        private string m_newIconName;
        private string m_openIconName;
        private string m_newDocumentName= "Untitled".Localize();
        private bool m_multiDocument;
        private string[] m_extensions;
        private string m_initialDirectory;
    }
}
