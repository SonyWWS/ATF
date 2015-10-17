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
            FileType = fileType;
            Extensions = extensions;
            m_newIconName = newIconName;
            m_openIconName = openIconName;
            MultiDocument = multiDocument;
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="fileType">File type name</param>
        /// <param name="extension">File extension, without the '.'</param>
        /// <param name="newIconKey">Name of 'New' icon, or null</param>
        /// <param name="openIconKey">Name of 'Open' icon, or null</param>
        public DocumentClientInfo(
            string fileType,
            string extension,
            object newIconKey,
            object openIconKey)
            : this(fileType, new[] { extension }, newIconKey, openIconKey, true)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="fileType">File type name</param>
        /// <param name="extension">File extension, without the '.'</param>
        /// <param name="newIconKey">Name of 'New' icon, or null</param>
        /// <param name="openIconKey">Name of 'Open' icon, or null</param>
        /// <param name="multiDocument">Whether client opens multiple documents simultaneously</param>
        public DocumentClientInfo(
            string fileType,
            string extension,
            object newIconKey,
            object openIconKey,
            bool multiDocument)
            : this(fileType, new[] { extension }, newIconKey, openIconKey, multiDocument)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="fileType">File type name</param>
        /// <param name="extensions">File extensions, without the '.'</param>
        /// <param name="newIconKey">Name of 'New' icon, or null</param>
        /// <param name="openIconKey">Name of 'Open' icon, or null</param>
        /// <param name="multiDocument">Whether client opens multiple documents simultaneously</param>
        public DocumentClientInfo(
            string fileType,
            string[] extensions,
            object newIconKey,
            object openIconKey,
            bool multiDocument)
        {
            FileType = fileType;
            Extensions = extensions;
            NewIconKey = newIconKey;
            OpenIconKey = openIconKey;
            MultiDocument = multiDocument;
        }

        /// <summary>
        /// Gets and sets the document client's user-readable file type name</summary>
        /// <remarks>The type name should be unique within an application and can contain
        /// any characters except ','</remarks>
        public string FileType { get; set; }

        /// <summary>
        /// Gets and sets the file extensions that the editor can handle</summary>
        public string[] Extensions { get; set; }

        /// <summary>
        /// Gets and sets the default new file extension that the editor can handle. If not null,
        /// then it should be a string that is in the Extensions property.</summary>
        /// <remarks>If the editor supports multiple extensions, setting the default extension 
        /// will stop ATF from prompting the user to choose the extension, when creating a new file</remarks>
        public string DefaultExtension { get; set; }

        /// <summary>
        /// Gets and sets the editor's 'New' icon name, or null</summary>
        public string NewIconName
        {
            get { return (!String.IsNullOrEmpty(m_newIconName)) ? m_newIconName : (NewIconKey as string); }
            set { m_newIconName = value; }
        }

        /// <summary>
        /// Gets and sets the editor's 'New' icon resource key, or null</summary>
        public object NewIconKey { get; set; }

        /// <summary>
        /// Gets and sets the editor's 'Open' icon name, or null</summary>
        public string OpenIconName
        {
            get { return (!String.IsNullOrEmpty(m_openIconName)) ? m_openIconName : OpenIconKey as string; }
            set { m_openIconName = value; }
        }

        /// <summary>
        /// Gets and sets the editor's 'Open' icon name, or null</summary>
        public object OpenIconKey { get; set; }

        /// <summary>
        /// Gets and sets the editor's default new document name</summary>
        public string NewDocumentName
        {
            get { return m_newDocumentName; }
            set { m_newDocumentName = value; }
        }

        /// <summary>
        /// Gets and sets a value indicating if the editor can open multiple documents simultaneously</summary>
        public bool MultiDocument { get; set; }

        /// <summary>
        /// Gets and sets a string to be used as the initial directory for the first time this application runs
        /// on the user's computer</summary>
        public string InitialDirectory { get; set; }

        /// <summary>
        /// Gets and sets a value indicating whether the StandardFileCommands service should automatically create commands for this client's document</summary>
        public bool AllowStandardFileCommands
        {
            get { return m_allowStandardFileCommands; }
            set { m_allowStandardFileCommands = value; }
        }

        /// <summary>
        /// Convenience method to check if file path is compatible with the extensions
        /// supported by the document client</summary>
        /// <param name="filePath">File path</param>
        /// <returns><c>True</c> if file path is compatible with the extensions supported by
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
        /// <returns><c>True</c> if file URI is compatible with the extensions supported by
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

        private string m_newIconName;
        private string m_openIconName;
        private string m_newDocumentName = "Untitled".Localize();
        private bool m_allowStandardFileCommands = true;
    }
}
