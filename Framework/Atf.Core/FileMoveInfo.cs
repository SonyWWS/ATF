//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;

namespace Sce.Atf
{
    /// <summary>
    /// Class to hold information about file system Delete, Move, and Copy operations.
    /// Contains file move type, source path, and a destination path, which is optional
    /// for file delete operations. Paths may be to files or directories, in which case
    /// all files in that directory and its sub-directories are affected.</summary>
    public class FileMoveInfo
    {
        /// <summary>
        /// Constructor using file move info</summary>
        /// <param name="type">File move type (Delete, Move, or Copy)</param>
        /// <param name="sourcePath">Source path</param>
        /// <param name="destinationPath">Destination path, or null if deleting</param>
        public FileMoveInfo(FileMoveType type, string sourcePath, string destinationPath)
            : this(type, sourcePath, destinationPath, false)
        {
        }

        /// <summary>
        /// Constructor using file move info and overwrite indicator</summary>
        /// <param name="type">File move type (Delete, Move, or Copy)</param>
        /// <param name="sourcePath">Source path</param>
        /// <param name="destinationPath">Destination path, or null if deleting</param>
        /// <param name="allowOverwrites">If true, files at destination path may be overwritten</param>
        public FileMoveInfo(FileMoveType type, string sourcePath, string destinationPath, bool allowOverwrites)
        {
            if (sourcePath == null)
                throw new ArgumentNullException("sourcePath");
            if (destinationPath == null && type != FileMoveType.Delete)
                throw new ArgumentNullException("destinationPath");

            // make sure that source and destination path are either both directories or both files
            if (sourcePath != null &&
                destinationPath != null)
            {
                if (string.IsNullOrEmpty(Path.GetFileName(sourcePath)) !=
                    string.IsNullOrEmpty(Path.GetFileName(destinationPath)))
                {
                    throw new InvalidOperationException("source and destination paths must both be directories or files");
                }
            }

            Type = type;
            SourcePath = sourcePath;
            DestinationPath = destinationPath;

            m_allowOverwrites = allowOverwrites;
        }

        /// <summary>
        /// File move type</summary>
        public readonly FileMoveType Type;

        /// <summary>
        /// Source path</summary>
        public readonly string SourcePath;

        /// <summary>
        /// Destination path, or null if deleting</summary>
        public readonly string DestinationPath;

        /// <summary>
        /// Gets or sets if files at destination path may be overwritten</summary>
        public bool AllowOverwrites
        {
            get { return m_allowOverwrites; }
            set { m_allowOverwrites = value; }
        }

        /// <summary>
        /// Whether files at destination path may be overwritten</summary>
        public bool m_allowOverwrites;
    }
}
