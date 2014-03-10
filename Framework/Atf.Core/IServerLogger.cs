//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf
{
    /// <summary>
    /// Interface for a service that allows sending asynchronous logging messages to a remote
    /// server, such as TNT's Debug &amp; Test Environment's Recap server</summary>
    public interface IServerLogger
    {
        /// <summary>
        /// Sends a log message asynchronously to the logging server</summary>
        /// <param name="data">Message to log</param>
        void SendLogData(string data);
    }
}
