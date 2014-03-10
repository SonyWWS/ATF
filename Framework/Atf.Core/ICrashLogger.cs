//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf
{
    /// <summary>
    /// Interface for a service that allows asynchronous sending of exceptions to a remote
    /// server, such as TNT's Debug and Test Environment's Recap server</summary>
    public interface ICrashLogger
    {
        /// <summary>
        /// Logs the given exception to the remote server</summary>
        /// <param name="e">Exception to log</param>
        void LogException(Exception e);
    }
}
