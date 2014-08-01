//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Applications
{
    public enum FileDialogResult
    {
        Yes,
        No,
        OK,
        Cancel
    }

    /// <summary>
    /// Interface for a service that provides the standard file dialogs. ATF
    /// provides a concrete class implementing these, FileDialogService. Use this
    /// or define your own implementation. This service is required by
    /// StandardFileCommands to implement an application's standard File menu
    /// commands: File/New, File/Open, File/Save, File/Save As, and File/Close.</summary>
    public interface IFileDialogService
    {
        /// <summary>
        /// Gets or sets a string used as the initial directory for the first time this application runs
        /// on the user's computer. The default value is the user's "My Documents" folder. This property
        /// can only be set to a directory that exists.</summary>
        string InitialDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a string used as the initial directory for the open/save dialog box,
        /// regardless of whatever directory the user may have previously navigated to. The default
        /// value is null. Set to null to cancel this behavior.</summary>
        string ForcedInitialDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets file name for file "Open" operation</summary>
        /// <param name="pathName">Resulting file name</param>
        /// <param name="filter">File extension filter, e.g., "Setting file (*.xml;*.txt)|*.xml;*.txt|Any (*.*)|*.*"</param>
        /// <returns>True iff operation is not cancelled</returns>
        FileDialogResult OpenFileName(ref string pathName, string filter);

        /// <summary>
        /// Gets multiple file names for file "Open" operation</summary>
        /// <param name="pathNames">Resulting file names</param>
        /// <param name="filter">File extension filter, e.g., "Setting file (*.xml;*.txt)|*.xml;*.txt|Any (*.*)|*.*"</param>
        /// <returns>True iff operation is not cancelled</returns>
        FileDialogResult OpenFileNames(ref string[] pathNames, string filter);

        /// <summary>
        /// Gets file name for file "Save" operation</summary>
        /// <param name="pathName">Suggested file name and resulting file name</param>
        /// <param name="filter">File extension filter, e.g., "Setting file (*.xml;*.txt)|*.xml;*.txt|Any (*.*)|*.*"</param>
        /// <returns>True iff operation is not cancelled</returns>
        FileDialogResult SaveFileName(ref string pathName, string filter);

        /// <summary>
        /// Confirms that file should be closed</summary>
        /// <param name="message">Confirmation message</param>
        /// <returns>True iff file should be closed</returns>
        FileDialogResult ConfirmFileClose(string message);

        /// <summary>
        /// Returns a value indicating if the file path exists</summary>
        /// <param name="pathName">File path</param>
        /// <returns>True if the file path exists</returns>
        bool PathExists(string pathName);
    }

    /// <summary>
    /// Class with useful static methods for IFileDialogService</summary>
    public static class FileDialogServices
    {
        /// <summary>
        /// Gets the file name for file "Open" operation, starting at the specified directory</summary>
        /// <param name="service">File dialog service</param>
        /// <param name="pathName">Resulting file name</param>
        /// <param name="filter">File extension filter, e.g., "Setting file (*.xml;*.txt)|*.xml;*.txt|Any (*.*)|*.*"</param>
        /// <param name="directory">Directory that the user first sees, or null for default behavior</param>
        /// <returns>True iff operation is not cancelled</returns>
        public static FileDialogResult OpenFileName(this IFileDialogService service, ref string pathName, string filter, string directory)
        {
            string originalDir = service.ForcedInitialDirectory;
            try
            {
                service.ForcedInitialDirectory = directory;
                return service.OpenFileName(ref pathName, filter);
            }
            finally
            {
                service.ForcedInitialDirectory = originalDir;
            }
        }

        /// <summary>
        /// Gets multiple file names for file "Open" operation</summary>
        /// <param name="service">File dialog service</param>
        /// <param name="pathNames">Resulting file names</param>
        /// <param name="filter">File extension filter, e.g., "Setting file (*.xml;*.txt)|*.xml;*.txt|Any (*.*)|*.*"</param>
        /// <param name="directory">Directory that the user first sees, or null for default behavior</param>
        /// <returns>True iff operation is not cancelled</returns>
        public static FileDialogResult OpenFileNames(this IFileDialogService service, ref string[] pathNames, string filter, string directory)
        {
            string originalDir = service.ForcedInitialDirectory;
            try
            {
                service.ForcedInitialDirectory = directory;
                return service.OpenFileNames(ref pathNames, filter);
            }
            finally
            {
                service.ForcedInitialDirectory = originalDir;
            }
        }

        /// <summary>
        /// Gets file name for file "Save" operation</summary>
        /// <param name="service">File dialog service</param>
        /// <param name="pathName">Suggested file name and resulting file name</param>
        /// <param name="filter">File extension filter, e.g., "Setting file (*.xml;*.txt)|*.xml;*.txt|Any (*.*)|*.*"</param>
        /// <param name="directory">Directory that the user first sees, or null for default behavior</param>
        /// <returns>True iff operation is not cancelled</returns>
        public static FileDialogResult SaveFileName(this IFileDialogService service, ref string pathName, string filter, string directory)
        {
            string originalDir = service.ForcedInitialDirectory;
            try
            {
                service.ForcedInitialDirectory = directory;
                return service.SaveFileName(ref pathName, filter);
            }
            finally
            {
                service.ForcedInitialDirectory = originalDir;
            }
        }
    }
}
