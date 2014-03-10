//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.IO;

namespace FileExplorerSample
{
    /// <summary>
    /// Interface for file data extensions, which determine the columns of file
    /// data that are displayed in the FileViewer's ListView</summary>
    public interface IFileDataExtension
    {
        /// <summary>
        /// Gets the name of the column</summary>
        string ColumnName
        {
            get;
        }

        /// <summary>
        /// Gets the value for the column and given file system item</summary>
        /// <param name="fileSystemInfo">Info describing the file or directory</param>
        /// <returns>Value for the column and given file system item</returns>
        string GetValue(FileSystemInfo fileSystemInfo);
    }
}
