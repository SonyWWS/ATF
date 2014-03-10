//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;

namespace FileExplorerSample
{
    // Examples of file data extensions that can be added to the composition container
    //  at startup to customize the FileViewer. These extensions are all discovered via
    //  MEF by the FileViewer component

    /// <summary>
    /// Name file data extension that can be added to the composition container
    /// at startup to customize the FileViewer. This extensions is discovered via
    /// MEF by the FileViewer component.</summary>
    [Export(typeof(IFileDataExtension))]
    public class NameDataExtension : IFileDataExtension
    {
        /// <summary>
        /// Gets the name of the column</summary>
        public string ColumnName
        {
            get { return "Name"; }
        }

        /// <summary>
        /// Gets the value for the column and given file system item</summary>
        /// <param name="fileSystemInfo">Info describing the file or directory</param>
        /// <returns>Value for the column and given file system item</returns>
        public string GetValue(FileSystemInfo fileSystemInfo)
        {
            return fileSystemInfo.Name;
        }
    }

    /// <summary>
    /// Creation Time file data extension that can be added to the composition container
    /// at startup to customize the FileViewer. This extensions is discovered via
    /// MEF by the FileViewer component.</summary>
    [Export(typeof(IFileDataExtension))]
    public class CreationTimeDataExtension : IFileDataExtension
    {
        /// <summary>
        /// Gets the name of the column</summary>
        public string ColumnName
        {
            get { return "Creation Time"; }
        }

        /// <summary>
        /// Gets the value for the column and given file system item</summary>
        /// <param name="fileSystemInfo">Info describing the file or directory</param>
        /// <returns>Value for the column and given file system item</returns>
        public string GetValue(FileSystemInfo fileSystemInfo)
        {
            return fileSystemInfo.CreationTime.ToString(CultureInfo.CurrentCulture);
        }
    }

    /// <summary>
    /// Size file data extension that can be added to the composition container
    /// at startup to customize the FileViewer. This extensions is discovered via
    /// MEF by the FileViewer component.</summary>
    [Export(typeof(IFileDataExtension))]
    public class SizeDataExtension : IFileDataExtension
    {
        /// <summary>
        /// Gets the name of the column</summary>
        public string ColumnName
        {
            get { return "Size"; }
        }

        /// <summary>
        /// Gets the value for the column and given file system item</summary>
        /// <param name="fileSystemInfo">Info describing the file or directory</param>
        /// <returns>value for the column and given file system item</returns>
        public string GetValue(FileSystemInfo fileSystemInfo)
        {
            FileInfo fileInfo = fileSystemInfo as FileInfo;
            if (fileInfo != null)
                return fileInfo.Length.ToString(CultureInfo.CurrentCulture);

            return string.Empty;
        }
    }
}
