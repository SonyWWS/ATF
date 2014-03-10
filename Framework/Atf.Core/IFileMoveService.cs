//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf
{
    /// <summary>
    /// Interface for file move services</summary>
    public interface IFileMoveService
    {
        /// <summary>
        /// Performs a sequence of file moves atomically; that is, if any operations can't be
        /// completed, then the file system is rolled back to its initial state</summary>
        /// <param name="moves">Sequence of file moves</param>
        void AtomicMove(IEnumerable<FileMoveInfo> moves);
    }
}
