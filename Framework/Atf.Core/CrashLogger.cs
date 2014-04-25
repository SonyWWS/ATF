//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;

namespace Sce.Atf
{
    /// <summary>
    /// A service to log unhandled exceptions (crashes) to a remote server, by listening
    /// to the AppDomain.CurrentDomain.UnhandledException event</summary>
    /// <remarks>Default server's web UI: https://sd-cdump-dev002.share.scea.com/recap/.
    /// If used with Sce.Atf.Applications.UnhandledExceptionService, put CrashLogger before 
    /// UnhandledExceptionService in the TypeCatalog, because otherwise UnhandledExceptionService 
    /// can prevent CrashLogger from receiving events, but CrashLogger does not prevent 
    /// UnhandledExceptionService from receiving events.</remarks>
    [Export(typeof(IInitializable))]
    [Export(typeof(ICrashLogger))]
    [Export(typeof(CrashLogger))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CrashLogger : ServerLogger, ICrashLogger, IInitializable
    {
        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by creating CrashReporter</summary>
        public void Initialize()
        {
            m_crashHandler = new Scea.CrashReporter(GetConnection());
            m_crashHandler.Enable();
        }

        #endregion

        #region ICrashLogger Members

        /// <summary>
        /// Logs the given exception to the remote server</summary>
        /// <param name="e">Exception</param>
        public virtual void LogException(Exception e)
        {
            m_crashHandler.CrashHandler(e);
        }

        #endregion

        private Scea.CrashReporter m_crashHandler;
    }
}
