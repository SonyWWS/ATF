//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;

namespace Sce.Atf
{
    /// <summary>
    /// Class that provides atomic file copy, move, and delete operations, i.e., they
    /// either complete successfully, or the file system is rolled back to its
    /// initial state</summary>
    [Export(typeof(FileMoveService))]
    [Export(typeof(IFileMoveService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class FileMoveService : IFileMoveService
    {
        #region IFileMoveService Members

        /// <summary>
        /// Performs a sequence of file moves atomically; that is, if any operations can't be
        /// completed, the file system is rolled back to its initial state</summary>
        /// <param name="moves">Sequence of file moves</param>
        public void AtomicMove(IEnumerable<FileMoveInfo> moves)
        {
            string tempPath = Path.GetTempPath();
            tempPath = Path.Combine(tempPath, "{88056160-E42B-4133-93FF-58A02F9DFDBE}");

            m_fileOperations = new List<FileOperation>();

            try
            {
                int tempFolderNumber = 1;
                foreach (FileMoveInfo move in moves)
                {
                    string srcPath = move.SourcePath;
                    string dstPath = move.DestinationPath;
                    if (string.Equals(srcPath, dstPath, StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    // for deletes, create a destination in the temp path
                    if (move.Type == FileMoveType.Delete)
                    {
                        // give each move its own temp directory to prevent interference
                        dstPath = Path.Combine(tempPath, tempFolderNumber++.ToString());
                        Directory.CreateDirectory(dstPath);
                    }

                    if ((File.GetAttributes(srcPath) & FileAttributes.Directory) != 0)
                    {
                        MoveDirectory(srcPath, dstPath, move);
                    }
                    else
                    {
                        // get a file name from temp directory
                        if (move.Type == FileMoveType.Delete)
                            dstPath = Path.Combine(dstPath, Path.GetFileName(srcPath));

                        MoveFile(srcPath, dstPath, move);
                    }
                }
            }
            catch (IOException)
            {
                // revert any completed file moves, in reverse order
                for (int i = m_fileOperations.Count - 1; i >= 0; i--)
                    m_fileOperations[i].Rollback();

                throw;
            }
            finally
            {
                DirectoryInfo tempDirectory = new DirectoryInfo(tempPath);
                if (tempDirectory.Exists)
                    ClearDirectory(tempDirectory);

                m_fileOperations = null;
            }
        }

        private void MoveDirectory(string srcPath, string dstPath, FileMoveInfo move)
        {
            if (!Directory.Exists(srcPath))
                throw new IOException("Can't find source directory".Localize());

            if (!Directory.Exists(dstPath))
            {
                Directory.CreateDirectory(dstPath);
                m_fileOperations.Add(new DirectoryMoveOperation(srcPath, dstPath));
            }

            foreach (string filePath in Directory.GetFiles(srcPath))
            {
                string fileName = filePath.Substring(srcPath.Length + 1);
                MoveFile(filePath, Path.Combine(dstPath, fileName), move);
            }

            foreach (string directoryPath in Directory.GetDirectories(srcPath))
            {
                string childName = directoryPath.Substring(srcPath.Length + 1);
                MoveDirectory(directoryPath, Path.Combine(dstPath, childName), move);
            }

            if (move.Type == FileMoveType.Delete ||
                move.Type == FileMoveType.Move)
            {
                Directory.Delete(srcPath);
            }
        }

        private void MoveFile(string srcPath, string dstPath, FileMoveInfo move)
        {
            if (!File.Exists(srcPath))
                throw new IOException("Can't find source file".Localize());

            if (File.Exists(dstPath))
            {
                if (move.Type == FileMoveType.Delete)
                {
                    // deleting, get rid of any file in temp directory
                    File.SetAttributes(dstPath, FileAttributes.Normal);
                    File.Delete(dstPath);
                }
                else if (!move.AllowOverwrites)
                {
                    throw new IOException("Can't overwrite existing file".Localize());
                }
            }

            switch (move.Type)
            {
                case FileMoveType.Move:
                    File.Move(srcPath, dstPath);
                    break;
                case FileMoveType.Copy:
                    File.Copy(srcPath, dstPath, move.AllowOverwrites);
                    break;
                case FileMoveType.Delete:
                    File.Move(srcPath, dstPath);
                    break;
                default:
                    throw new InvalidOperationException("illegal FileMoveType value");
            }

            m_fileOperations.Add(new FileMoveOperation(srcPath, dstPath));
        }

        #endregion

        private static void ClearDirectory(DirectoryInfo info)
        {
            try
            {
                foreach (FileInfo fileInfo in info.GetFiles())
                {
                    fileInfo.IsReadOnly = false;
                    fileInfo.Delete();
                }
                foreach (DirectoryInfo childInfo in info.GetDirectories())
                {
                    ClearDirectory(childInfo);
                }

                info.Delete();
            }
            catch (IOException)
            {
            }
            catch (AccessViolationException)
            {
            }
            // ignore io exceptions, leaving junk in temporary folder
        }

        private abstract class FileOperation
        {
            public abstract void Rollback();
        }

        private class DirectoryMoveOperation : FileOperation
        {
            public DirectoryMoveOperation(string sourcePath, string destinationPath)
            {
                m_sourcePath = sourcePath;
                m_destinationPath = destinationPath;
            }

            public override void Rollback()
            {
                // move directory back if it was moved
                if (!Directory.Exists(m_sourcePath))
                    Directory.Move(m_destinationPath, m_sourcePath);

                Directory.Delete(m_destinationPath);
            }

            private readonly string m_sourcePath;
            private readonly string m_destinationPath;
        }

        private class FileMoveOperation : FileOperation
        {
            public FileMoveOperation(string sourcePath, string destinationPath)
            {
                m_sourcePath = sourcePath;
                m_destinationPath = destinationPath;
            }

            public override void Rollback()
            {
                // move file back if it was deleted
                if (!File.Exists(m_sourcePath))
                    File.Move(m_destinationPath, m_sourcePath);

                File.Delete(m_destinationPath);
            }

            private readonly string m_sourcePath;
            private readonly string m_destinationPath;
        }

        private List<FileOperation> m_fileOperations;
    }
}
