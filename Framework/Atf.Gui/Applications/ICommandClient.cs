//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for clients of menu and toolbar services</summary>
    public interface ICommandClient
    {
        /// <summary>
        /// Checks whether the client can do the command, if it handles it</summary>
        /// <param name="commandTag">Command to be done</param>
        /// <returns><c>True</c> if client can do the command</returns>
        bool CanDoCommand(object commandTag);

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandTag">Command to be done</param>
        void DoCommand(object commandTag);

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="commandState">Command info to update</param>
        void UpdateCommand(object commandTag, CommandState commandState);
    }
}
